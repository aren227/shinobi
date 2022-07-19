// Pasted from GlowUi.
Shader "Unlit/StrippedUi"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _Size ("Size", Vector) = (1, 1, 1, 1)
        _Border ("Border", Float) = 4
        _Full ("Full", Float) = 0.5
        _Stripe ("Stripe", Float) = 0.7
        _Scale ("Scale", Float) = 10
    }
    SubShader
    {
        Tags
        {
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

        Cull Off
		Lighting Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex    : SV_POSITION;
				half4 color      : COLOR;
				float2 texcoord  : TEXCOORD0;
            };

            half4 _Color;
            sampler2D _MainTex;
			sampler2D _AlphaTex;

            float2 _Size;
            float _Border;
            float _Full;
            float _Stripe;
            float _Scale;

            v2f vert (appdata v)
            {
                v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				o.color = v.color * _Color;
				#ifdef PIXELSNAP_ON
				o.vertex = UnityPixelSnap (o.vertex);
				#endif
                return o;
            }

            fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);
#if ETC1_EXTERNAL_ALPHA
				// get the color from an external texture (usecase: Alpha support for ETC1 on android)
				color.a = tex2D (_AlphaTex, uv).r;
#endif //ETC1_EXTERNAL_ALPHA
				return color;
			}

            half4 frag (v2f i) : SV_Target
            {
                half4 c = SampleSpriteTexture(i.texcoord);
                c *= i.color;

                c.a = 0;

                float2 scaled = i.texcoord * _Size.xy;

                if (i.texcoord.x < _Border / _Size.x) c.a = 1;
                if (i.texcoord.x > 1 - _Border / _Size.x) c.a = 1;
                if (i.texcoord.y < _Border / _Size.y) c.a = 1;
                if (i.texcoord.y > 1 - _Border / _Size.y) c.a = 1;

                if (i.texcoord.x <= _Full) c.a = 1;
                if (i.texcoord.x <= _Stripe && frac((scaled.x + scaled.y) / _Scale) > 0.5) c.a = 1;

                return c;
            }
            ENDCG
        }
    }
}
