Shader "Unlit/GreenKey_Transparent"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GreenMin ("Green Threshold (g-max(r,b))", Range(0,1)) = 0.12
        _Softness ("Edge Softness", Range(0,0.2)) = 0.03
        _MinSat ("Min Saturation (approx)", Range(0,1)) = 0.08
        _MinVal ("Min Brightness", Range(0,1)) = 0.05

        [Toggle] _Invert ("Invert (ON: remove green / keep non-green)", Float) = 1
        _AlphaMul ("Alpha Multiply", Range(0,2)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Cull Off
        ZWrite Off

        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _GreenMin, _Softness, _MinSat, _MinVal;
            float _Invert, _AlphaMul;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);

                float r = c.r, g = c.g, b = c.b;
                float greenScore = g - max(r, b);

                float mx = max(r, max(g,b));
                float mn = min(r, min(g,b));
                float sat = (mx > 1e-5) ? (mx - mn) / mx : 0;
                float val = mx;

                float t = smoothstep(_GreenMin - _Softness, _GreenMin + _Softness, greenScore);

                float valid = step(_MinSat, sat) * step(_MinVal, val);
                t *= valid;

                float keep = (_Invert > 0.5) ? (1.0 - t) : t;

                float a = c.a * keep * _AlphaMul;

                c.rgb *= a;
                c.a = a;
                return c;
            }
            ENDCG
        }
    }
}
