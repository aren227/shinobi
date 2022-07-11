// see README here:
// github.com/ColinLeung-NiloCat/UnityURPUnlitScreenSpaceDecalShader

Shader "NiloCat Extension/Screen Space Decal/Unlit"
{
    Properties
    {
        [Header(Basic)]
        [MainTexture]_MainTex("Texture", 2D) = "white" {}
        [MainColor][HDR]_Color("_Color (default = 1,1,1,1)", Color) = (1,1,1,1)

        [Header(Blending)]
        // https://docs.unity3d.com/ScriptReference/Rendering.BlendMode.html
        [Enum(UnityEngine.Rendering.BlendMode)]_DecalSrcBlend("_DecalSrcBlend (default = SrcAlpha)", Int) = 5 // 5 = SrcAlpha
        [Enum(UnityEngine.Rendering.BlendMode)]_DecalDstBlend("_DecalDstBlend (default = OneMinusSrcAlpha)", Int) = 10 // 10 = OneMinusSrcAlpha

        [Header(Alpha remap(extra alpha control))]
        _AlphaRemap("_AlphaRemap (default = 1,0,0,0) _____alpha will first mul x, then add y    (zw unused)", vector) = (1,0,0,0)

        [Header(Prevent Side Stretching(Compare projection direction with scene normal and Discard if needed))]
        [Toggle(_ProjectionAngleDiscardEnable)] _ProjectionAngleDiscardEnable("_ProjectionAngleDiscardEnable (default = off)", float) = 0
        _ProjectionAngleDiscardThreshold("_ProjectionAngleDiscardThreshold (default = 0)", range(-1,1)) = 0

        [Header(Mul alpha to rgb)]
        [Toggle]_MulAlphaToRGB("_MulAlphaToRGB (default = off)", Float) = 0

        [Header(Ignore texture wrap mode setting)]
        [Toggle(_FracUVEnable)] _FracUVEnable("_FracUVEnable (default = off)", Float) = 0
    }

    SubShader
    {
        // To avoid render order problems, Queue must >= 2501, which enters the transparent queue,
        // in transparent queue Unity will always draw from back to front
        // https://github.com/ColinLeung-NiloCat/UnityURPUnlitScreenSpaceDecalShader/issues/6#issuecomment-615940985

        // https://docs.unity3d.com/Manual/SL-SubShaderTags.html
        // Queues up to 2500 (“Geometry+500”) are consided “opaque” and optimize the drawing order of the objects for best performance.
        // Higher rendering queues are considered for “transparent objects” and sort objects by distance,
        // starting rendering from the furthest ones and ending with the closest ones.
        // Skyboxes are drawn in between all opaque and all transparent objects.
        // "Queue" = "Transparent-499" means "Queue" = "2501", which is almost equals "draw right before any transparent objects"

        // "DisableBatching" means disable "dynamic batching", not "srp batching"
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent-499" }

        ZWrite off
        Blend[_DecalSrcBlend][_DecalDstBlend]

        CGPROGRAM

        #pragma surface surf Standard vertex:vert alpha:auto

        // @Todo: Should be 3.0
        #pragma target 4.0

        sampler2D _MainTex;
        sampler2D _CameraDepthTexture;

        float4 _MainTex_ST;
        float _ProjectionAngleDiscardThreshold;
        half4 _Color;
        half2 _AlphaRemap;
        half _MulAlphaToRGB;

        struct Input
        {
            float4 screenPos;
            float4 viewRayOS;
            float4 cameraPosOSAndFogFactor;
        };

        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            o.screenPos = ComputeScreenPos(v.vertex);

            float3 viewRay = mul(UNITY_MATRIX_MV, float4(v.vertex.x, v.vertex.y, v.vertex.z, 1)).xyz;
            o.viewRayOS.w = viewRay.z;

            viewRay *= -1;

            float4x4 ViewToObjectMatrix = mul(unity_WorldToObject, unity_MatrixInvV);

            o.viewRayOS.xyz = mul((float3x3)ViewToObjectMatrix, viewRay);
            o.cameraPosOSAndFogFactor.xyz = mul(ViewToObjectMatrix, float4(0,0,0,1)).xyz;
        }

        void surf(Input i, inout SurfaceOutputStandard o) {
            i.viewRayOS.xyz /= i.viewRayOS.w;

            float2 screenSpaceUV = i.screenPos.xy / i.screenPos.w;
            float sceneRawDepth = tex2D(_CameraDepthTexture, screenSpaceUV).r;

            float3 decalSpaceScenePos;

            // if perspective camera, LinearEyeDepth will handle everything for user
            // remember we can't use LinearEyeDepth for orthographic camera!
            float sceneDepthVS = LinearEyeDepth(sceneRawDepth);

            // scene depth in any space = rayStartPos + rayDir * rayLength
            // here all data in ObjectSpace(OS) or DecalSpace
            // be careful, viewRayOS is not a unit vector, so don't normalize it, it is a direction vector which view space z's length is 1
            decalSpaceScenePos = i.cameraPosOSAndFogFactor.xyz + i.viewRayOS.xyz * sceneDepthVS;


            // convert unity cube's [-0.5,0.5] vertex pos range to [0,1] uv. Only works if you use a unity cube in mesh filter!
            float2 decalSpaceUV = decalSpaceScenePos.xy + 0.5;

            // discard logic
            //===================================================
            // discard "out of cube volume" pixels
            float shouldClip = 0;
#if _ProjectionAngleDiscardEnable
            // also discard "scene normal not facing decal projector direction" pixels
            float3 decalSpaceHardNormal = normalize(cross(ddx(decalSpaceScenePos), ddy(decalSpaceScenePos)));//reconstruct scene hard normal using scene pos ddx&ddy

            // compare scene hard normal with decal projector's dir, decalSpaceHardNormal.z equals dot(decalForwardDir,sceneHardNormalDir)
            shouldClip = decalSpaceHardNormal.z > _ProjectionAngleDiscardThreshold ? 0 : 1;
#endif
            // call discard
            // if ZWrite is Off, clip() is fast enough on mobile, because it won't write the DepthBuffer, so no GPU pipeline stall(confirmed by ARM staff).
            clip(0.5 - abs(decalSpaceScenePos) - shouldClip);
            //===================================================

            // sample the decal texture
            float2 uv = decalSpaceUV.xy * _MainTex_ST.xy + _MainTex_ST.zw;//Texture tiling & offset
#if _FracUVEnable
            uv = frac(uv);// add frac to ignore texture wrap setting
#endif
            half4 col = tex2D(_MainTex, uv);
            col *= _Color;// tint color
            col.a = saturate(col.a * _AlphaRemap.x + _AlphaRemap.y);// alpha remap MAD
            col.rgb *= lerp(1, col.a, _MulAlphaToRGB);// extra multiply alpha to RGB

            o.Albedo = col;
            o.Alpha = col.a;
        }

        // copied from URP12.1.2's ShaderVariablesFunctions.hlsl
        float LinearDepthToEyeDepth(float rawDepth)
        {
            #if UNITY_REVERSED_Z
                return _ProjectionParams.z - (_ProjectionParams.z - _ProjectionParams.y) * rawDepth;
            #else
                return _ProjectionParams.y + (_ProjectionParams.z - _ProjectionParams.y) * rawDepth;
            #endif
        }

        ENDCG
    }
}