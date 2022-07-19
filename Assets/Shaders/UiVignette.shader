Shader "Unlit/UiVignette"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Health ("Health", Float) = 1 // [0, 0.5]
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

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

            fixed4 _Color;

            float _Health;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float a = tex2D(_MainTex, i.uv * _ScreenParams.xy / 512).a;
                a = smoothstep(0.1, 1, a);

                float r = distance(i.uv, float2(0.5, 0.5));
                r = smoothstep(_Health, _Health+0.1, r);

                a = a * r;

                return fixed4(_Color.rgb, a);
            }
            ENDCG
        }
    }
}
