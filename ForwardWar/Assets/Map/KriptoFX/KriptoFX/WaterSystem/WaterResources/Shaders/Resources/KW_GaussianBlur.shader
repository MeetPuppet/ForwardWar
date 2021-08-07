Shader "Hidden/KriptoFX/Water/GaussianBlur"
{
	Properties
	{
		_MainTex("", 2D) = "white" {}
	}

		CGINCLUDE

#include "UnityCG.cginc"

		struct vertexInput
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct vertexOutput
	{
		float4 vertex : SV_POSITION;
		float2 texcoord : TEXCOORD0;
	};

	struct output_5tap
	{
		float4 vertex : SV_POSITION;
		float2 texcoord : TEXCOORD0;
		float4 blurTexcoord : TEXCOORD1;
	};


	sampler2D _CameraDepthTextureBeforeWaterZWrite;
	sampler2D KW_WaterMaskDepth;
	sampler2D _MainTex;
	float4 _MainTex_ST;
	float2 _MainTex_TexelSize;
	float _DepthBlurRadius;
	float _SampleScale;
	sampler2D _BaseTex;
	float iterations;
	float2 blurWeight;

	vertexOutput vert(vertexInput v)
	{
		vertexOutput o;

		o.vertex = UnityObjectToClipPos(v.vertex);
		o.texcoord = v.uv;

		return o;
	}

	half4 DownsampleFilter(float2 uv)
	{
		//return tex2D(_MainTex, uv);
		float2 texelSize = _MainTex_TexelSize;
		half4 A = tex2D(_MainTex,  UnityStereoTransformScreenSpaceTex(uv + texelSize * float2(-1.0, -1.0)));
		half4 B = tex2D(_MainTex,  UnityStereoTransformScreenSpaceTex(uv + texelSize * float2(0.0, -1.0)));
		half4 C = tex2D(_MainTex,  UnityStereoTransformScreenSpaceTex(uv + texelSize * float2(1.0, -1.0)));
		half4 D = tex2D(_MainTex,  UnityStereoTransformScreenSpaceTex(uv + texelSize * float2(-0.5, -0.5)));
		half4 E = tex2D(_MainTex,  UnityStereoTransformScreenSpaceTex(uv + texelSize * float2(0.5, -0.5)));
		half4 F = tex2D(_MainTex,  UnityStereoTransformScreenSpaceTex(uv + texelSize * float2(-1.0, 0.0)));
		half4 G = tex2D(_MainTex,  UnityStereoTransformScreenSpaceTex(uv));
		half4 H = tex2D(_MainTex,  UnityStereoTransformScreenSpaceTex(uv + texelSize * float2(1.0, 0.0)));
		half4 I = tex2D(_MainTex,  UnityStereoTransformScreenSpaceTex(uv + texelSize * float2(-0.5, 0.5)));
		half4 J = tex2D(_MainTex,  UnityStereoTransformScreenSpaceTex(uv + texelSize * float2(0.5, 0.5)));
		half4 K = tex2D(_MainTex,  UnityStereoTransformScreenSpaceTex(uv + texelSize * float2(-1.0, 1.0)));
		half4 L = tex2D(_MainTex,  UnityStereoTransformScreenSpaceTex(uv + texelSize * float2(0.0, 1.0)));
		half4 M = tex2D(_MainTex,  UnityStereoTransformScreenSpaceTex(uv + texelSize * float2(1.0, 1.0)));

		half2 div = (1.0 / 4.0) * half2(0.5, 0.125);

		half4 o = (D + E + I + J) * div.x;
		o += (A + B + G + F) * div.y;
		o += (B + C + H + G) * div.y;
		o += (F + G + L + K) * div.y;
		o += (G + H + M + L) * div.y;

		return o;

		/*float4 d = _MainTex_TexelSize.xyxy * float4(-1, -1, +1, +1);

		half4 s;
		s = tex2D(_MainTex, uv + d.xy);
		s += tex2D(_MainTex, uv + d.zy);
		s += tex2D(_MainTex, uv + d.xw);
		s += tex2D(_MainTex, uv + d.zw);

		return s * (1.0 / 4);*/
	}

	half4 UpsampleFilter(float2 uv)
	{
		// 4-tap bilinear upsampler
		float4 d = _MainTex_TexelSize.xyxy * float4(-1, -1, +1, +1) * (_SampleScale * 0.5);

		half4 s;
		s = tex2D(_MainTex, uv + d.xy);
		s += tex2D(_MainTex, uv + d.zy);
		s += tex2D(_MainTex, uv + d.xw);
		s += tex2D(_MainTex, uv + d.zw);

		return s * (1.0 / 4);
	}

	half4 frag_downsample(vertexInput i) : SV_Target
	{
		return DownsampleFilter(i.uv);

	}

	half4 frag_upsample(vertexInput i) : SV_Target
	{
		return UpsampleFilter(i.uv);
	}


	float frag(vertexOutput i) : SV_Target
	{
		float sceneZDefault = LinearEyeDepth(tex2D(_CameraDepthTextureBeforeWaterZWrite, i.texcoord));
		float waterDepth = LinearEyeDepth(tex2D(KW_WaterMaskDepth, i.texcoord).g); \
		if (sceneZDefault < waterDepth) return 0;
		else
			return clamp(sceneZDefault - waterDepth, 0, 10);
	}

	output_5tap vert5Horizontal(vertexInput IN)
	{
		output_5tap OUT;

		OUT.vertex = UnityObjectToClipPos(IN.vertex);

		float2 offset = float2(_MainTex_TexelSize.x * _DepthBlurRadius, 0.0);

#if UNITY_VERSION >= 540
		float2 uv = UnityStereoScreenSpaceUVAdjust(IN.uv, _MainTex_ST);
#else
		float2 uv = IN.uv;
#endif

		OUT.texcoord = uv;
		OUT.blurTexcoord.xy = uv + offset;
		OUT.blurTexcoord.zw = uv - offset;

		return OUT;
	}

	output_5tap vert5Vertical(vertexInput IN)
	{
		output_5tap OUT;

		OUT.vertex = UnityObjectToClipPos(IN.vertex);

		float2 offset = float2(0.0, _MainTex_TexelSize.y * _DepthBlurRadius);

#if UNITY_VERSION >= 540
		float2 uv = UnityStereoScreenSpaceUVAdjust(IN.uv, _MainTex_ST);
#else
		float2 uv = IN.uv;
#endif

		OUT.texcoord = uv;
		OUT.blurTexcoord.xy = uv + offset;
		OUT.blurTexcoord.zw = uv - offset;

		return OUT;
	}

	fixed4 frag5Blur(output_5tap IN) : SV_Target
	{
	#if GAMMA_CORRECTION
		fixed3 sum = GammaToLinearSpace(tex2D(_MainTex, IN.texcoord).xyz) * 0.29411764;
		sum += GammaToLinearSpace(tex2D(_MainTex, IN.blurTexcoord.xy).xyz) * 0.35294117;
		sum += GammaToLinearSpace(tex2D(_MainTex, IN.blurTexcoord.zw).xyz) * 0.35294117;
		return fixed4(LinearToGammaSpace(sum), 1.0);
	#else
		fixed3 sum = tex2D(_MainTex, IN.texcoord).xyz * 0.29411764;
		sum += tex2D(_MainTex, IN.blurTexcoord.xy).xyz * 0.35294117;
		sum += tex2D(_MainTex, IN.blurTexcoord.zw).xyz * 0.35294117;
		return fixed4(sum, 1.0);
	#endif
	}

		ENDCG

		SubShader
	{
		ZTest Always Cull Off ZWrite Off

			Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}

			Pass
		{
			CGPROGRAM
			#pragma multi_compile _ GAMMA_CORRECTION
			#pragma vertex vert5Horizontal
			#pragma fragment frag5Blur
			ENDCG
		}

			Pass
		{
			CGPROGRAM
			#pragma multi_compile _ GAMMA_CORRECTION
			#pragma vertex vert5Vertical
			#pragma fragment frag5Blur
			ENDCG
		}

			//3
			Pass
		{
			ZTest Always Cull Off ZWrite Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_downsample
			ENDCG
		}
			//4
			Pass
		{
			ZTest Always Cull Off ZWrite Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_upsample
			ENDCG
		}

	}
}
