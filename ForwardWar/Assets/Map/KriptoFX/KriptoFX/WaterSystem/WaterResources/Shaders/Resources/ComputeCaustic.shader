Shader "Hidden/KriptoFX/Water/ComputeCaustic"
{
	CGINCLUDE

#include "UnityCG.cginc"
#include "KW_WaterVariables.cginc"
#include "KW_WaterHelpers.cginc"
#include "WaterVertFrag.cginc"

	float KW_MeshScale;
	float KW_CausticDepthScale;

	struct appdata_caustic
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f_caustic
	{
		float4 vertex : SV_POSITION;
		float3 oldPos : TEXCOORD0;
		float3 newPos : TEXCOORD1;


	};


	v2f_caustic vert_caustic(appdata_caustic v)
	{
		v2f_caustic o;

		//float4 screenPos = ComputeScreenPos(UnityObjectToClipPos(v.vertex));

		//float depth = tex2Dlod(_CameraDepthTexture, screenPos);
		//float waterDepth = tex2Dlod(KW_WaterDepth, screenPos);
		//bool isUnderwater = tex2Dlod(KW_WaterMaskScatterNormals_Blured, screenPos).x > 0.7;
		//bool causticMask = isUnderwater ? waterDepth < depth : waterDepth > depth;
		//if (causticMask < 0.5) {
		//	o.vertex = 0.0 / 0.0;
		//	return o;
		//}

		//float3 viewRay = UnityObjectToViewPos(v.vertex).xyz;
		//float4 ray;
		//float3 rayCameraOffset;

		//ray.w = viewRay.z;
		//viewRay *= -1;
		//ray.xyz = mul((float3x3)mul(unity_WorldToObject, UNITY_MATRIX_I_V), viewRay);
		//rayCameraOffset = mul(mul(unity_WorldToObject, UNITY_MATRIX_I_V), float4(0, 0, 0, 1)).xyz;

		//ray /= ray.w;
		//float zEye = LinearEyeDepth(depth);
		//float3 opos = rayCameraOffset + ray.xyz * zEye;
		//float2 uvNear = opos.xz - 0.5;

		float3 offset = 0;
		float3 shorelineOffset = 0;
		float2 uv;
		ShorelineData shorelineData;
		float3 worldPos = mul(unity_ObjectToWorld, v.vertex);

		offset += ComputeWaterOffset(worldPos) * KW_CausticDepthScale;
		shorelineOffset += ComputeBeachWaveOffset(worldPos, shorelineData, -0.35) * 0.07;

		float2 terrainDepthUV = (worldPos.xz - KW_DepthPos.xz) / KW_DepthOrthographicSize + 0.5;
		float terrainDepth = tex2Dlod(KW_OrthoDepth, float4(terrainDepthUV, 0, 0)).r * KW_DepthNearFarDistance.z - KW_DepthNearFarDistance.y + KW_DepthPos.y - KW_WaterPosition.y - offset.y;
		terrainDepth = clamp(-terrainDepth * 5, 0.0, 10);
		float windStr = clamp(KW_WindSpeed, 0.0, 6.0) / 6.0;

		//v.vertex = float4(opos, 1);
		o.oldPos = v.vertex;

		float3 worldOffset = 0;
		//worldOffset.xz += (offset.xz / 1) * lerp(terrainDepth * terrainDepth, terrainDepth * 0.25, 0);
		//worldOffset.xz += (shorelineOffset.xz / 1) * terrainDepth;
		//v.vertex = mul(unity_WorldToObject, float4(worldPos.xyz + worldOffset, 1));
		v.vertex.xz += (offset.xz / KW_MeshScale) * lerp(terrainDepth, terrainDepth * 0.1, windStr);
		v.vertex.xz += (shorelineOffset.xz / KW_MeshScale) * terrainDepth;
		o.newPos = v.vertex.xyz;
		o.vertex = UnityObjectToClipPos(v.vertex);

		return o;
	}

	half4 frag_caustic(v2f_caustic i) : SV_Target
	{
		float oldArea = length(ddx(i.oldPos.xyz)) * length(ddy(i.oldPos.xyz));
		float newArea = length(ddx(i.newPos.xyz)) * length(ddy(i.newPos.xyz));

		float color = oldArea / newArea * 0.1;
		return  float4(color.xxx, 1);
	}

		ENDCG

	SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Pass
		{
			Blend One One

			ZWrite Off
			ZTest Always
			Cull Off

			CGPROGRAM
			#pragma vertex vert_caustic
			#pragma fragment frag_caustic

			#pragma multi_compile _ KW_INTERACTIVE_WAVES
			#pragma shader_feature  KW_FLOW_MAP_EDIT_MODE
			#pragma multi_compile _ KW_FLOW_MAP
			#pragma multi_compile _ KW_FOAM
			#pragma multi_compile _ USE_MULTIPLE_SIMULATIONS
			#pragma multi_compile _ USE_FILTERING

			ENDCG
		}

	}
}
