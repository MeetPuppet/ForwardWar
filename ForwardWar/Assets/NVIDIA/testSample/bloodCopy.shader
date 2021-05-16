// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/bloodCopy"
{
    Properties {
    _Color ("Main Color", Color) = (1,0,0,1)
    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}

SubShader {
    Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
    LOD 200
    Blend SrcAlpha OneMinusSrcAlpha
    Cull Off

    CGPROGRAM
	#pragma surface surf Lambert alpha:blend

    sampler2D _MainTex;
    fixed4     _Color;

    struct Input {
        float2 uv_MainTex;
        float2 location;
    };

    float powerForPos(float4 pos, float2 nearVertex);

    void vert(inout appdata_full vertexData, out Input outData) {
        float4 pos = UnityObjectToClipPos(vertexData.vertex);
        float4 posWorld = mul(unity_ObjectToWorld, vertexData.vertex);
        outData.uv_MainTex = vertexData.texcoord;
        outData.location = posWorld.xz;
    }

    void surf (Input IN, inout SurfaceOutput o) {
        fixed4 baseColor = tex2D(_MainTex, IN.uv_MainTex);

		baseColor.rgb = _Color;
		o.Albedo = baseColor.rgb;
        o.Alpha = baseColor.a;
    }


    ENDCG
}

Fallback "Transparent/VertexLit"
}
