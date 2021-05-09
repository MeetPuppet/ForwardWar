Shader "Custom/blood"
{
    Properties {
	_Color("Color", Color) = (0,0,0,1)
	_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
	_MainTex2("Fog (RGB) Trans (A)", 2D) = "white" {}
}

SubShader {
	CGPROGRAM
    #pragma surface surf Lambert alpha:blend

	sampler2D _MainTex;
	sampler2D _MainTex2;
	fixed4     _Color;

    struct Input {
        float2 uv_MainTex;
    };

    void surf (Input IN, inout SurfaceOutput o) {
		fixed4 colorZero = tex2D(_MainTex, IN.uv_MainTex);
		fixed4 colorMid = tex2D(_MainTex2, IN.uv_MainTex);

		o.Albedo = _Color;
		float alpha = 1.0f - colorZero.g - colorMid.g / 4;
		o.Albedo = alpha;
    }
    ENDCG
}
}
