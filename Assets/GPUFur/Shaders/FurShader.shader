Shader "Fur/Default"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
	}
		SubShader
	{
		/*Tags { "RenderType" = "Opaque" }
		LOD 100*/

		Pass
	{
		Tags{ "BW" = "TrueProbes" "LightMode" = "ForwardBase" }
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#include "UnityCG.cginc"
#include "Lighting.cginc"
#pragma multi_compile_fwdbase
#include "AutoLight.cginc"
		float4 _Color;

	uint _VertexCount;

	struct FurVertex
	{
		float3 direction;
		float3 position;
		float3 velocity;
		float3 acceleration;
		float3 rootForce;
		float3 color;
	};

	StructuredBuffer<FurVertex> vertexBuffer;


	struct v2f
	{
		SHADOW_COORDS(1)
			float4 pos : SV_POSITION;
		half3 ambient : COLOR1;
		half3 diff : COLOR0;
		float3 color : COLOR2;
	};


	v2f vert(uint vertex_id : SV_VertexID, uint instance_id : SV_InstanceID)
	{
		v2f o;
		FurVertex vertex = vertexBuffer[instance_id * _VertexCount + vertex_id];
		float3 wpos = vertex.position;
		o.pos = UnityWorldToClipPos(wpos);
		o.diff = _LightColor0.rgb * _Color.rgb;
		o.ambient = ShadeSH9(float4(0, 1, 0, 1));
		o.color = vertex.color;
		TRANSFER_SHADOW(o);
		return o;
	}

	float4 frag(v2f i) : SV_Target
	{
		fixed shadow = SHADOW_ATTENUATION(i);
	//return float4(i.color, 1);
	return fixed4(i.diff * (shadow + i.ambient), 1);
	}
		ENDCG
	}

		Pass
	{
		Tags{ "LightMode" = "ShadowCaster" }
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
#pragma multi_compile_shadowcaster


		uint _VertexCount;

	struct FurVertex
	{
		float3 direction;
			float3 position;
		float3 velocity;
		float3 acceleration;
		float3 rootForce;
		float3 color;
	};

	StructuredBuffer<FurVertex> vertexBuffer;


	struct v2f
	{
		float4 vertex : SV_POSITION;
	};


	v2f vert(uint vertex_id : SV_VertexID, uint instance_id : SV_InstanceID)
	{
		v2f o;
		FurVertex vertex = vertexBuffer[instance_id * _VertexCount + vertex_id];
		float3 wpos = vertex.position;
		o.vertex = UnityWorldToClipPos(wpos);
		return o;
	}

	float4 frag(v2f i) : SV_Target
	{
		return 0;
	}
		ENDCG
	}
	}
}
