Shader "Unlit/DepthWrite2"
{
    SubShader
    {
        CGINCLUDE

        #include "UnityCG.cginc"

        struct appdata
        {
            float4 vertex : POSITION;
        };

        struct v2f
        {
            float4 vertex : SV_POSITION;
            float4 cube : TEXCOORD0;
        };

        float4x4 _ObjectToCube;

        v2f vert (appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.cube = mul(_ObjectToCube, v.vertex);
            return o;
        }

        fixed4 frag (v2f i) : SV_Target
        {
            if (max(abs(i.cube.x), max(abs(i.cube.y), abs(i.cube.z))) < 0.5) discard;
            return 0;
        }

        fixed4 frag2 (v2f i) : SV_Target
        {
            return 0;
        }

        ENDCG

        Pass
        {
            Cull Back
            ZTest Less
            ColorMask 0

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }

        Pass
        {
            Cull Front
            ZTest Less
            ColorMask 0

            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }

        Pass
        {
            Cull Front
            ZTest Less
            ColorMask 0

            Stencil
            {
                Ref 1
                Comp Equal
                Pass Keep
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag2
            ENDCG
        }
    }
}
