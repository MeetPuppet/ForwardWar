Shader "KriptoFX/Water/KW_WetDecal"
{
	Subshader
	{
		Tags {
				"LightMode" = "Deferred" "Queue" = "AlphaTest"
			}
	Pass
	{

		ZWrite Off
		Cull Front
		ZTest Greater


		CGPROGRAM

		#pragma vertex vert
		#pragma fragment frag
		#pragma exclude_renderers nomrt

#define DEFERRED_PASS

		#pragma multi_compile _ USE_DISPERSION
		#pragma multi_compile _ USE_LOD1 USE_LOD2 USE_LOD3

		#include "UnityCG.cginc"
		#include "KW_WaterVariables.cginc"


		sampler2D KW_CausticLod0;
		sampler2D KW_CausticLod1;
		sampler2D KW_CausticLod2;
		sampler2D KW_CausticLod3;

		float KW_DecalScale;

		float4 KW_CausticLod0_TexelSize;
		float4 KW_CausticLod1_TexelSize;
		float KW_CausticDispersionStrength;
		float KW_CaustisStrength;

		float4 KW_CausticLodSettings;
		float3 KW_CausticLodOffset;


		struct appdata_t {
			float4 vertex : POSITION;
			float2 texcoord : TEXCOORD0;
		};


		struct v2f {
			float4 vertex : SV_POSITION;
			float2 texcoord : TEXCOORD0;
			float4 screenUV : TEXCOORD1;
			float4 ray : TEXCOORD2;
			float3 rayCameraOffset : TEXCOORD3;
		};

		struct FragmentOutput {
#if defined(DEFERRED_PASS)
			float4 gBuffer0 : SV_Target0;
			float4 gBuffer1 : SV_Target1;
			float4 gBuffer2 : SV_Target2;
			float4 gBuffer3 : SV_Target3;
#else
			float4 color : SV_Target;
#endif
		};

		v2f vert(appdata_t v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.texcoord = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xz / KW_FFTDomainSize;

			float3 viewRay = UnityObjectToViewPos(v.vertex).xyz;
			o.ray.w = viewRay.z;
			viewRay *= -1;
			o.ray.xyz = mul((float3x3)mul(unity_WorldToObject, UNITY_MATRIX_I_V), viewRay);
			o.rayCameraOffset = mul(mul(unity_WorldToObject, UNITY_MATRIX_I_V), float4(0, 0, 0, 1)).xyz;

			o.screenUV = ComputeScreenPos(o.vertex);

			return o;
		}

		half3 GetCausticLod(float2 decalUV, float lodDist, sampler2D tex, half3 lastLodCausticColor)
		{
			float2 uv = lodDist * decalUV + 0.5 - KW_CausticLodOffset.xz;
			float caustic = tex2D(tex, uv);
			uv = 1 - min(1, abs(uv * 2 - 1));
			float lerpLod = uv.x * uv.y;
			lerpLod = min(1, lerpLod * 3);
			return lerp(lastLodCausticColor, caustic, lerpLod);
		}

		half3 GetCausticLodWithDispersion(float2 decalUV, float lodDist, sampler2D tex, half3 lastLodCausticColor, float texelSize, float dispersionStr)
		{
			float2 uv = lodDist * decalUV + 0.5 - KW_CausticLodOffset.xz;
			float3 caustic;
			caustic.r = tex2D(tex, uv);
			caustic.g = tex2D(tex, uv + texelSize * dispersionStr * 2);
			caustic.b = tex2D(tex, uv + texelSize * dispersionStr * 4);

			uv = 1 - min(1, abs(uv * 2 - 1));
			float lerpLod = uv.x * uv.y;
			lerpLod = min(1, lerpLod * 3);
			return lerp(lastLodCausticColor, caustic, lerpLod);
		}

		inline float3 ScreenToWorld(float2 UV, float depth)
		{
			float2 uvClip = UV * 2.0 - 1.0;
			float4 clipPos = float4(uvClip, depth, 1.0);
			float4 viewPos = mul(KW_ProjToView, clipPos);
			viewPos /= viewPos.w;
			float3 worldPos = mul(KW_ViewToWorld, viewPos).xyz;
			return worldPos;
		}


		FragmentOutput  frag(v2f i)
		{
			FragmentOutput output;

			float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, (i.screenUV.xy / i.screenUV.w));
			float waterDepth = tex2Dproj(KW_WaterDepth, i.screenUV);
			bool isUnderwater = tex2Dproj(KW_WaterMaskScatterNormals_Blured, i.screenUV).x > 0.7;
			bool causticMask = isUnderwater ? waterDepth < depth : waterDepth > depth;

			//if (causticMask < 0.0001) discard;

			i.ray /= i.ray.w;
			float zEye = LinearEyeDepth(depth);
			float3 localPos = i.rayCameraOffset + i.ray.xyz * zEye;

			float waterHeightOffset = ScreenToWorld((i.screenUV.xy / i.screenUV.w), waterDepth).y;
			float3 worldPos = mul(unity_ObjectToWorld, float4(localPos, 1));


			float dist = length(worldPos - _WorldSpaceCameraPos);
			float2 depthUV = (worldPos.xz - KW_DepthPos.xz) / KW_DepthOrthographicSize + 0.5;
			float terrainDepthRaw = tex2Dlod(KW_OrthoDepth, float4(depthUV, 0, 0)).r * KW_DepthNearFarDistance.z - KW_DepthNearFarDistance.y + KW_DepthPos.y - KW_WaterPosition.y;



			float depthTransparent = max(1, KW_Transparent * 2);
			float terrainDepth = clamp(-terrainDepthRaw, 0, depthTransparent) / (depthTransparent);
			half deepFade = tex2D(_MainTex, terrainDepth).r;
			//return float4(-terrainDepth + waterHeightOffset < Test4.x, 0, 0, 1);

			float3 caustic = 0.1;

#if defined(USE_LOD3)
			caustic = GetCausticLod(localPos.xz, KW_DecalScale / KW_CausticLodSettings.w, KW_CausticLod3, caustic);
#endif
#if defined(USE_LOD2) || defined(USE_LOD3)
			caustic = GetCausticLod(localPos.xz, KW_DecalScale / KW_CausticLodSettings.z, KW_CausticLod2, caustic);
#endif

#if USE_DISPERSION
#if defined(USE_LOD1) || defined(USE_LOD2) || defined(USE_LOD3)
				caustic = GetCausticLodWithDispersion(localPos.xz, KW_DecalScale / KW_CausticLodSettings.y, KW_CausticLod1, caustic, KW_CausticLod0_TexelSize.x, KW_CausticDispersionStrength);
#endif
			caustic = GetCausticLodWithDispersion(localPos.xz, KW_DecalScale / KW_CausticLodSettings.x, KW_CausticLod0, caustic, KW_CausticLod0_TexelSize.x, KW_CausticDispersionStrength);

#else
#if defined(USE_LOD1) || defined(USE_LOD2) || defined(USE_LOD3)
				caustic = GetCausticLod(localPos.xz, KW_DecalScale / KW_CausticLodSettings.y, KW_CausticLod1, caustic);
#endif
			caustic = GetCausticLod(localPos.xz, KW_DecalScale / KW_CausticLodSettings.x, KW_CausticLod0, caustic);
#endif

			//caustic = lerp(0.1, caustic, saturate(0.25 + KW_Transparent / 5));
			//caustic += pow(caustic.rrr, 5) * 10 * (1-terrainDepth);
			//caustic *= 10;
			caustic = lerp(1, caustic * 10, saturate(KW_CaustisStrength));
			caustic += caustic * caustic * caustic * saturate(KW_CaustisStrength - 1);
			float distFade = 1 - saturate(dist / KW_DecalScale * 2);
			caustic = lerp(1, caustic, distFade);

			//float fade = saturate((waterHeightOffset - worldPos.y) * 2);

			//if(!isUnderwater) caustic = lerp(1, caustic, fade);
			//return lerp(caustic, 1, terrainDepth);
			//if (terrainDepthRaw > Test4.x) discard;

			if (localPos.y > 0.9) discard;

#if defined(DEFERRED_PASS)
			output.gBuffer1.rgb =0.11;
			output.gBuffer1.a = 0.75;
			output.gBuffer3.rgb = 0;
#else
			output.color = 0;
#endif
			return output;

		}

			ENDCG
	}
	}
}
