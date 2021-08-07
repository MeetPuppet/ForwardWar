Shader "Hidden/KriptoFX/Water/SSPR_Reflection"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{

		Cull Off ZWrite Off ZTest Always

		Pass
		{


			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#pragma multi_compile _ KW_USE_COLOR_AFTER_EVERYTHING
			#pragma multi_compile _ KW_USE_HDR_COLOR

			#include "UnityCG.cginc"
			Texture2D<uint> HashData;
			sampler2D _CameraDepthTexture;
			sampler2D _GrabTexture;
			float4x4 _ViewToWorld;
			float4x4 _ProjToView;
			float4x4 _WorldToView;
			float4x4 _CameraMatrix;
			float _Height;
			int UseDebugHash;
			float2 KW_SSPR_ScreenSize;
			float _UpsideDownFix;
			float _UpsideDownImageEffectsFix;
			float4 Test4;
			float _ReflectedImageEffectsHDR_Mul;
			float KW_WindSpeed;

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

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.ray = UnityObjectToViewPos(v.vertex) * float3(-1, -1, 1);
				o.uv = v.uv;
				o.screenPos = ComputeScreenPos(o.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}

			sampler2D _MainTex;
			sampler2D _CameraColorTexture;
			sampler2D _CameraColorTextureFinal;
			float4 _CameraColorTexture_TexelSize;
			float4 _CameraColorTextureFinal_TexelSize;
			float Threshold;
			sampler2D KW_WaterMaskDepth;

			float KW_FFTDomainSize;
			sampler2D KW_NormTex;
			sampler2D KW_WaterMaskScatterNormals_Blured;
			float KW_FFT_Size_Normalized;
			float KW_DepthHolesFillDistance;

			float4 frag(v2f i) : SV_Target
			{
				float3 texSize = float3(_CameraColorTexture_TexelSize.xy, 0);
				uint hash = HashData[i.uv * (KW_SSPR_ScreenSize.xy) ].x;

				uint left = HashData[(i.uv + texSize.xz * KW_DepthHolesFillDistance * 0.15) * (KW_SSPR_ScreenSize.xy)].x;
				uint right = HashData[(i.uv  - texSize.xz * KW_DepthHolesFillDistance * 0.15) * (KW_SSPR_ScreenSize.xy)].x;
				uint up = HashData[(i.uv + texSize.zy) * (KW_SSPR_ScreenSize.xy)].x;
				uint down = HashData[(i.uv - texSize.zy * KW_DepthHolesFillDistance) * (KW_SSPR_ScreenSize.xy)].x;

				hash = min(hash, left);
				hash = min(hash, right);
				hash = min(hash, up);
				hash = min(hash, down);

				if (hash != 0)
				{
					uint x = hash & 0xFFFF; uint y = hash >> 16;
					float2 uv = float2(x, y) / KW_SSPR_ScreenSize.xy;
					if (_UpsideDownFix > 0) uv.y = 1 - uv.y;

					float3 reflColor = tex2D(_CameraColorTexture, uv);

					float3 col = 0;

					if ((x <= 0 || y >= KW_SSPR_ScreenSize.y)) return 0;

					float fringeY = _UpsideDownFix > 0 ? uv.y : (1 - uv.y);
					float fringeX = (1 - abs(uv.x * 2 - 1));
					fringeX = saturate(lerp(fringeX * 10 - 1, 1, saturate(fringeY * 2.3)));
					fringeY = fringeY * 7;

					return float4(reflColor, saturate(fringeX * fringeY));

				}

				return 0;
			}
			ENDCG
		}
	}
}
