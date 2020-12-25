using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Fur : MonoBehaviour
{
    public float speed;

    public Camera renderCamera;

    public Light mainLight;

    public Material material;

    public ComputeShader computeShader;

    public Mesh mesh;

    public int subdivisions;

    public float fixedDeltaTime;

    public int vertexCount;

    public float furLength;

    public float springConstant;

    public float innerFriction, airFriction;

    public float stiffness;

    public Vector3 gravity;

    public float irregularWind;
    public float windStrength;
    public Vector3 windDirection;

    public int noiseWidth, noiseHeight, noiseDepth;
    public Vector3 noiseScale;
    public Vector3 noise3DScale;
    public Vector3 noiseScrollSpeed;

    private Vector3 noiseOffset;

    private int instanceCount
    {
        get
        {
            return mesh.vertexCount;
        }
    }

    private int kernelID, bufferID, dtID, segmentLengthID, springConstantID, stiffnessID,
        fixedDeltaTimeID, noiseTextureID, innerFrictionID, airFrictionID, gravityID,
        windDirectionID, windStrengthID, windForwardID, windRightID, windUpID, noise3DScaleID,
        noiseOffsetID, iterationsID,
        irregularWindID, positionID, positionDeltaID, rotationDeltaID, vertexCountID;


    struct FurVertexData
    {
        public Vector3 direction;
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 acceleration;
        public Vector3 rootForce;
        public Vector3 color;
    }
    private const int VERTEX_DATA_SIZE = 72;

    private ComputeBuffer buffer;

    private RenderTexture shadowMap;

    private Texture3D noiseTexture;

    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private Vector3 posDelta, rotDelta;

    void Start()
    {
        StartCoroutine(WaitInit());
    }

    IEnumerator WaitInit()
    {
        yield return new WaitForSecondsRealtime(0.75f);
        Initialize();
    }

    void Initialize()
    {

        kernelID = computeShader.FindKernel("SimulateGPUFur");

        bufferID = Shader.PropertyToID("buffer");
        fixedDeltaTimeID = Shader.PropertyToID("fixedDeltaTime");
        dtID = Shader.PropertyToID("dt");
        springConstantID = Shader.PropertyToID("springConstant");
        stiffnessID = Shader.PropertyToID("stiffness");
        segmentLengthID = Shader.PropertyToID("segmentLength");
        noiseTextureID = Shader.PropertyToID("noiseTexture");
        innerFrictionID = Shader.PropertyToID("innerFriction");
        airFrictionID = Shader.PropertyToID("airFriction");
        gravityID = Shader.PropertyToID("gravity");
        positionID = Shader.PropertyToID("position");
        positionDeltaID = Shader.PropertyToID("positionDelta");
        rotationDeltaID = Shader.PropertyToID("rotationDelta");
        vertexCountID = Shader.PropertyToID("vertexCount");
        windStrengthID = Shader.PropertyToID("windStrength");
        windForwardID = Shader.PropertyToID("windForward");
        windRightID = Shader.PropertyToID("windRight");
        windUpID = Shader.PropertyToID("windUp");
        windDirectionID = Shader.PropertyToID("windDirection");
        irregularWindID = Shader.PropertyToID("irregularWind");
        noise3DScaleID = Shader.PropertyToID("noise3DScale");
        noiseOffsetID = Shader.PropertyToID("noiseOffset");
        iterationsID = Shader.PropertyToID("iterations");

        mesh = MeshLabCore.MeshLab.SubdivideMesh(mesh, subdivisions);

        Vector3[] meshVertices = mesh.vertices, meshNormals = mesh.normals;
        Vector3 step, offset, root, normal;
        float segmentLength = furLength / (vertexCount - 1);
        FurVertexData[] furVertices = new FurVertexData[instanceCount * vertexCount];
        FurVertexData furVertex;
        for (int i = 0; i < instanceCount; i += 1)
        {
            root = transform.TransformPoint(meshVertices[i]);
            offset = Vector3.zero;
            normal = transform.TransformDirection(meshNormals[i].normalized);
            step = normal * segmentLength;
            for (int j = 0; j < vertexCount; j += 1)
            {
                furVertex = new FurVertexData();
                furVertex.direction = normal;
                furVertex.position = root + offset;
                offset += step;
                furVertex.velocity = Vector3.zero;
                furVertex.acceleration = Vector3.zero;
                furVertex.rootForce = Vector3.zero;
                furVertices[i * vertexCount + j] = furVertex;
            }
        }

        buffer = new ComputeBuffer(instanceCount * vertexCount, VERTEX_DATA_SIZE);
        buffer.SetData(furVertices);
        

        computeShader.SetBuffer(kernelID, bufferID, buffer);

        material.SetBuffer("vertexBuffer", buffer);
        material.SetInt("_VertexCount", vertexCount);

        computeShader.SetFloats("noiseDimensions", noiseWidth, noiseHeight, noiseDepth);

        computeShader.SetFloat(fixedDeltaTimeID, fixedDeltaTime);

        computeShader.SetFloat(springConstantID, springConstant);
        computeShader.SetFloat(stiffnessID, stiffness * stiffness);
        computeShader.SetFloat(segmentLengthID, segmentLength);

        noiseTexture = ThreeDNoise.Get3DNoise(noiseWidth, noiseHeight, noiseDepth, noiseScale);
        computeShader.SetTexture(kernelID, noiseTextureID, noiseTexture);
        computeShader.SetFloat(innerFrictionID, innerFriction);
        computeShader.SetFloat(airFrictionID, airFriction);
        computeShader.SetFloats(gravityID, gravity.x, gravity.y, gravity.z);
        computeShader.SetFloats(positionID, transform.position.x, transform.position.y, transform.position.z);
        computeShader.SetFloats(positionDeltaID, 0, 0, 0);
        computeShader.SetFloats(rotationDeltaID, 0, 0, 0);

        computeShader.SetInt(vertexCountID, vertexCount);

        shadowMap = new RenderTexture(Screen.width, Screen.height, 0);
        material.SetTexture("_ShadowMapTexture", shadowMap);

        material.EnableKeyword("SHADOWS_SCREEN");
        material.EnableKeyword("LIGHTPROBE_SH");

        uint[] args = new uint[4] { (uint)vertexCount, (uint)instanceCount, 0, 0 };

        ComputeBuffer argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        CommandBuffer cameraBuffer = new CommandBuffer();
        cameraBuffer.DrawProceduralIndirect(Matrix4x4.identity, material, 0, MeshTopology.LineStrip, argsBuffer);
        //cameraBuffer.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.LineStrip, vertexCount, instanceCount);
        renderCamera.AddCommandBuffer(CameraEvent.AfterDepthTexture, cameraBuffer);
        renderCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cameraBuffer);

        CommandBuffer lightBuffer = new CommandBuffer();
        lightBuffer.DrawProceduralIndirect(Matrix4x4.identity, material, 1, MeshTopology.LineStrip, argsBuffer);
        //lightBuffer.DrawProcedural(Matrix4x4.identity, material, 1, MeshTopology.LineStrip, vertexCount, instanceCount);
        mainLight.AddCommandBuffer(LightEvent.BeforeShadowMapPass, lightBuffer);

        CommandBuffer shadowMapBuffer = new CommandBuffer();
        shadowMapBuffer.SetShadowSamplingMode(BuiltinRenderTextureType.CurrentActive, ShadowSamplingMode.CompareDepths);
        shadowMapBuffer.Blit(BuiltinRenderTextureType.CurrentActive, new RenderTargetIdentifier(shadowMap));
        mainLight.AddCommandBuffer(LightEvent.AfterScreenspaceMask, shadowMapBuffer);

        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    private void OnDestroy()
    {
        if (buffer != null)
        {
            buffer.Release();
        }
    }

    void Update()
    {
        if (shadowMap != null)
        {
            transform.position += Vector3.right * speed;

            computeShader.SetFloat(dtID, Time.deltaTime);

            computeShader.SetFloat(fixedDeltaTimeID, fixedDeltaTime);

            computeShader.SetFloat(innerFrictionID, innerFriction);
            computeShader.SetFloat(airFrictionID, airFriction);
            computeShader.SetFloat(stiffnessID, stiffness * stiffness);
            computeShader.SetFloat(springConstantID, springConstant);

            computeShader.SetFloats(gravityID, gravity.x, gravity.y, gravity.z);

            computeShader.SetFloats(positionID, transform.position.x, transform.position.y, transform.position.z);

            posDelta = (transform.position - lastPosition);
            computeShader.SetFloats(positionDeltaID, posDelta.x, posDelta.y, posDelta.z);
            lastPosition = transform.position;

            float dx = Mathf.DeltaAngle(lastRotation.eulerAngles.x, transform.eulerAngles.x) * Mathf.Deg2Rad;
            float dy = Mathf.DeltaAngle(lastRotation.eulerAngles.y, transform.eulerAngles.y) * Mathf.Deg2Rad;
            float dz = Mathf.DeltaAngle(lastRotation.eulerAngles.z, transform.eulerAngles.z) * Mathf.Deg2Rad;
            computeShader.SetFloats(rotationDeltaID, dx, dy, dz);
            lastRotation = transform.rotation;

            computeShader.SetInt(iterationsID, (int)(Time.deltaTime / fixedDeltaTime) + 1);

            /* Wind */
            computeShader.SetFloat(windStrengthID, windStrength);
            computeShader.SetFloat(irregularWindID, irregularWind);
            Vector3 v = windDirection.normalized;
            computeShader.SetFloats(windForwardID, v.x, v.y, v.z);
            v = new Vector3(-v.y + v.z, v.x - v.z, +v.y - v.x).normalized;
            computeShader.SetFloats(windUpID, v.x, v.y, v.z);
            v = Vector3.Cross(windDirection, v).normalized;
            computeShader.SetFloats(windRightID, v.x, v.y, v.z);

            noiseOffset += Time.deltaTime * noiseScrollSpeed;
            computeShader.SetFloats(noiseOffsetID, noiseOffset.x, noiseOffset.y, noiseOffset.z);

            computeShader.SetFloats(noise3DScaleID, noise3DScale.x, noise3DScale.y, noise3DScale.z);

            computeShader.Dispatch(kernelID, instanceCount, 1, 1);
        }
    }
}
