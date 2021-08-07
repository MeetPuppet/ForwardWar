Shader "Hidden/KriptoFX/Water/SSPR_Projection"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
		SubShader
	{

		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{


			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0

			#include "UnityCG.cginc"
			RWTexture2D<uint> HashData : register(u1);
			sampler2D _CameraDepthTexture;
			sampler2D _MainTex;
			sampler2D _GrabTexture;
			float4x4 KW_ViewToWorld;
			float4x4 KW_ProjToView;
			float4x4 KW_CameraMatrix;
			float _Height;
			float Threshold;
			float Intensity;
			float CameraDirection;
			float LimitZ;
			float2 KW_SSPR_ScreenSize;
			sampler2D KW_WaterMaskDepth;
			float3 KW_WaterPosition;
			float4 Test4;
			sampler2D KW_NormTex;
			float KW_FFTDomainSize;
			float KW_SSR_ClipOffset;


			struct appdata
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 ray : TEXCOORD1;
				float4 screenPos : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.ray = UnityObjectToViewPos(v.vertex) * float3(-1, -1, 1);
				o.uv = v.uv;
				o.screenPos = ComputeScreenPos(o.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}


			float3 ScreenToWorld(float2 UV, float depth)
			{
				float2 uvClip = UV * 2.0 - 1.0;
				float4 clipPos = float4(uvClip, depth, 1.0);
				float4 viewPos = mul(KW_ProjToView, clipPos);
				viewPos /= viewPos.w;
				float3 worldPos = mul(KW_ViewToWorld, viewPos).xyz;
				return worldPos;
			}

			float2 WorldToScreenPos(float3 pos) {
				float4 projected = mul(KW_CameraMatrix, float4(pos, 1.0f));
				float2 uv = (projected.xy / projected.w) * 0.5f + 0.5f;
				uv = 1 - uv;
				return uv;
			}

			float frag (v2f i) : SV_Target
			{
				float depth = tex2D(_CameraDepthTexture, i.uv);
				float3 worldPos = ScreenToWorld(i.uv, depth);

				if (worldPos.y < KW_WaterPosition.y - KW_SSR_ClipOffset) return 0;

				float3 ReflPosWS = float3(worldPos.x, KW_WaterPosition.y * 2 - worldPos.y, worldPos.z);
				float2 ReflPosUV = WorldToScreenPos(ReflPosWS);

				float reflectedDepth = tex2D(_CameraDepthTexture, ReflPosUV);

				uint2 SrcPosPixel = i.uv * (KW_SSPR_ScreenSize.xy);
				uint2 ReflPosPixel = ReflPosUV * (KW_SSPR_ScreenSize.xy);

				uint projectionHash = SrcPosPixel.y << 16 | SrcPosPixel.x;
				InterlockedMin(HashData[ReflPosPixel], projectionHash);

				return 0;
			}
			ENDCG
		}
	}
}
