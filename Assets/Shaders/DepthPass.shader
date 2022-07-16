Shader "Unlit/DepthPass"
{
    SubShader
    {
        CGINCLUDE
        #pragma vertex vert
        #pragma fragment frag

        #include "UnityCG.cginc"

        sampler2D _FrontDepth;
        sampler2D _BackDepth;

        struct appdata
        {
            float4 vertex : POSITION;
        };

        struct v2f
        {
            float4 vertex : SV_POSITION;
            float4 screenpos : TEXCOORD0;
        };

        v2f vert (appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.screenpos = ComputeScreenPos(o.vertex);
            return o;
        }

        fixed4 frag (v2f i) : SV_Target
        {
            float d = i.screenpos.z / i.screenpos.w;
#if !defined(UNITY_REVERSED_Z)
            d = d * 0.5 + 0.5;
#endif

            float2 uv = i.screenpos.xy / i.screenpos.w;
            float f = tex2D(_FrontDepth, uv).r;
            float b = tex2D(_BackDepth, uv).r;
#if !defined(UNITY_REVERSED_Z)
            if (b < 1 && f <= d && d <= b) discard;
#else
            if (b <= d && d <= f) discard;
#endif
            return 0;
        }
        ENDCG

        // 3
        Pass
        {
            Cull Back
            ZTest Less
            ColorMask 0

            CGPROGRAM
            ENDCG
        }

        // 4
        Pass
        {
            Cull Front
            ZTest Less
            ColorMask 0

            Stencil {
                Ref 1
                Comp Always
                Pass Replace
            }

            CGPROGRAM
            ENDCG
        }
    }
}
