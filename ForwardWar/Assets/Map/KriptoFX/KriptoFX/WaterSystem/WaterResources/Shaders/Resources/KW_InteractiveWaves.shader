Shader "Hidden/KriptoFX/Water/KW_InteractiveWaves"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "" {}
	}

		CGINCLUDE

#include "UnityCG.cginc"

	struct vertexInput
	{
		float4 vertex : POSITION;
		float3 texcoord : TEXCOORD0;
		float color : COLOR0;
	};

	struct v2fDraw
	{
		float4 vertex  : POSITION;
		float3 uv : TEXCOORD0;
		float force : COLOR0;
	};

	struct v2f
	{
		float4 vertex  : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2fMultiTap
	{
		float4 vertex  : POSITION;
		float2 uv[5] : TEXCOORD0;
	};

	sampler2D KW_DrawedPointTex;
	sampler2D _PrevTex;
	sampler2D _MainTex;
	float4 _MainTex_TexelSize;
	float2 _WorldOffset;
	float2 _WorldOffsetPrev;
	float4x4  _WaterMVPDepth;

	float _DropSize;
	float _Damping;
	float4 _TEST;
	float4 _MousePos;
	half KW_RipplesScaleLerped;
	float2 _MouseWorldPos;
	float _MyTest;

	float3 KW_AreaOffset;
	float3 KW_LastAreaOffset;
	float KW_DrawPointForce;
	half KW_InteractiveWavesAreaSize;
	half KW_InteractiveWavesPixelSpeed;
	sampler2D KW_InteractiveWavesNormalTexPrev;

	v2fDraw vertDraw(vertexInput v)
	{
		v2fDraw o;

		o.vertex = float4(v.vertex.xy, 1, 1);
#if UNITY_UV_STARTS_AT_TOP
		o.vertex.y = 1 - o.vertex.y;
#endif
		o.vertex.xy = o.vertex.xy * 2 - 1;
		
		o.uv = v.texcoord.xyz * 2 - 1;
		o.force = v.color;

		return o;
	}

	v2f vert(appdata_img v)
	{
		v2f o;

		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;

		return o;
	}

	v2fMultiTap vertMultiTap(appdata_img v)
	{
		v2fMultiTap o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		float3 size = float3(_MainTex_TexelSize.x, -_MainTex_TexelSize.x, 0) * KW_InteractiveWavesPixelSpeed.x;

		o.uv[0] = v.texcoord.xy + KW_LastAreaOffset.xz;
		o.uv[1] = v.texcoord.xy + size.xz + KW_AreaOffset.xz; //0.004, 0      right
		o.uv[2] = v.texcoord.xy + size.yz + KW_AreaOffset.xz; //-0.004, 0     left
		o.uv[3] = v.texcoord.xy + size.zx + KW_AreaOffset.xz; //0, 0.004      up
		o.uv[4] = v.texcoord.xy + size.zy + KW_AreaOffset.xz; //0, -0.004     down

		return o;
	}

	v2fMultiTap vertNormal(appdata_img v)
	{
		v2fMultiTap o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		float3 size = float3(_MainTex_TexelSize.x, -_MainTex_TexelSize.y, 0);

		o.uv[0] = v.texcoord.xy;
		o.uv[1] = v.texcoord.xy + size.xz; 
		o.uv[2] = v.texcoord.xy + size.yz;
		o.uv[3] = v.texcoord.xy + size.zx; 
		o.uv[4] = v.texcoord.xy + size.zy; 

		return o;
	}

	half4 fragDraw(v2fDraw i) : COLOR
	{
		return step(saturate(length(i.uv)), 0.99) * i.force;
	}

	half4 fragCopy(v2f i) : COLOR
	{
		float orig = (tex2D(_MainTex, i.uv).r - 0.5) * 2.0;
		float drawedPoints = tex2D(KW_DrawedPointTex, i.uv).r;
		float newVal = orig + drawedPoints;
		return (newVal / 2.0) + 0.5;
	}

	float fragPropogate(v2fMultiTap i) : COLOR
	{
		if (i.uv[1].x < 0.02 || i.uv[1].x > 0.98 || i.uv[1].y < 0.021 || i.uv[1].y > 0.98) return 0.5;

		float prevSample = (tex2D(_PrevTex, i.uv[0]).r - 0.5) * 2.0;
		float sample = (tex2D(_MainTex, i.uv[1]).r - 0.5) * 2.0;
		sample		+= (tex2D(_MainTex, i.uv[2]).r - 0.5) * 2.0;
		sample		+= (tex2D(_MainTex, i.uv[3]).r - 0.5) * 2.0;
		sample		+= (tex2D(_MainTex, i.uv[4]).r - 0.5) * 2.0;
		sample		/= 2.0;

		float newValue = sample - prevSample;
		float dampedValue = newValue * 0.985 ;

		return dampedValue / 2.0 + 0.5;
	}

	float2 fragNormal(v2fMultiTap i) : COLOR
	{
		float last = (tex2D(KW_InteractiveWavesNormalTexPrev, i.uv[0]).r) * KW_InteractiveWavesAreaSize;

		float left  = (tex2D(_MainTex, i.uv[2]).r) * KW_InteractiveWavesAreaSize;
		float right = (tex2D(_MainTex, i.uv[1]).r) * KW_InteractiveWavesAreaSize;
		float down  = (tex2D(_MainTex, i.uv[4]).r) * KW_InteractiveWavesAreaSize;
		float top   = (tex2D(_MainTex, i.uv[3]).r) * KW_InteractiveWavesAreaSize;
	
		float3 va = float3(float2(1.0, 0.0), right - left);
		float3 vb = float3(float2(0.0, 1.0), top - down);

		return normalize(cross(va,vb).rbg).xz * 0.5 + 0.5;
	}

	ENDCG

Subshader {
	
	Pass {
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }
		Blend SrcAlpha One

		CGPROGRAM
		#pragma vertex vertDraw
		#pragma fragment fragDraw
		ENDCG
	}

	Pass{
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }
		Blend Off

		CGPROGRAM
		#pragma vertex vert
		#pragma fragment fragCopy
		ENDCG
	}

	Pass {
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }
		Blend Off

		CGPROGRAM
		#pragma vertex vertMultiTap
		#pragma fragment fragPropogate
		ENDCG
	}

	Pass{
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }
		Blend Off

		CGPROGRAM
		#pragma vertex vertNormal
		#pragma fragment fragNormal
		ENDCG
	}
}

} 