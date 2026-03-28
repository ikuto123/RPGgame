Shader "Unlit/WaterShader_WavyTransparent"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint (A = Alpha)", Color) = (1,1,1,0.5)

        _ScrollSpeed ("Scroll Speed (XY)", Vector) = (0.05, 0.05, 0, 0)

        _WaveAmp   ("Wave Amplitude", Float) = 0.08
        _WaveFreq  ("Wave Frequency", Float) = 2.0
        _WaveSpeed ("Wave Speed", Float) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            float4 _ScrollSpeed;
            float _WaveAmp;
            float _WaveFreq;
            float _WaveSpeed;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                float t = _Time.y * _WaveSpeed;
                float wave =
                    sin((worldPos.x + t) * _WaveFreq) +
                    cos((worldPos.z + t * 0.8) * _WaveFreq * 1.2);

                worldPos.y += wave * (_WaveAmp * 0.5);

                o.vertex = mul(UNITY_MATRIX_VP, float4(worldPos, 1));

                float2 uv = TRANSFORM_TEX(v.uv, _MainTex);
                uv += _ScrollSpeed.xy * _Time.y;
                o.uv = uv;

                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
