Shader "Unlit/Robot"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        [HDR] _Emission ("Emission", Color) = (0, 0, 0, 0)
        _MainTex ("Texture", 2D) = "white" {}
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull mode", Float) = 2
    }
    SubShader
    {
        CGINCLUDE
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 3.0
        #pragma multi_compile ___ UNITY_HDR_ON

        #include "UnityCG.cginc"
        ENDCG

        Pass
        {
            Name "Robot Front Pass"
            Tags { "RenderType"="Opaque" "LightMode"="Deferred" }
            ZTest Equal
            Cull [_Cull]
            LOD 100

            CGPROGRAM

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float3 world : TEXCOORD1;
                float3 normal : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            struct fs
			{
				half4 albedo : SV_Target0;
				half4 specular : SV_Target1;
				half4 normal : SV_Target2;
				half4 emission : SV_Target3;
			};

            fixed4 _Color;
            float4 _Emission;
            float _Test;

            sampler2D _MainTex;
            float4 _MainTex_ST;

            // @Temp
            float _Cull;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.world = mul(unity_ObjectToWorld, v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fs frag (v2f i)
            {
                fs o;
                o.albedo = _Color;

                float3 n = normalize(i.normal);
                // @Temp
                if (_Cull == 1) n *= -1;

                o.normal = half4(n * 0.5 + 0.5, 1.0);
                o.emission = half4(0.05, 0.05, 0.05, 0) + _Emission;
                return o;
            }
            ENDCG
        }
    }
}
