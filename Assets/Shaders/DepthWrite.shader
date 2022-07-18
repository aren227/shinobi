Shader "Unlit/DepthWrite"
{
    SubShader
    {
        CGINCLUDE
        #pragma vertex vert
        #pragma fragment frag

        #include "UnityCG.cginc"

        struct appdata
        {
            float4 vertex : POSITION;
        };

        struct v2f
        {
            float4 vertex : SV_POSITION;
        };

        v2f vert (appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            return o;
        }

        fixed4 frag (v2f i) : SV_Target
        {
            return 0;
        }
        ENDCG

        // 1
        Pass
        {
            Cull Back
            ZTest Less
            ColorMask 0

            CGPROGRAM
            ENDCG
        }

        // 2
        Pass
        {
            Cull Front
            ZTest Greater
            ColorMask 0

            CGPROGRAM
            ENDCG
        }
    }
}
