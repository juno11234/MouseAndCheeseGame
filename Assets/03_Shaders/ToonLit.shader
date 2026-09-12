Shader "Custom/ToonLit"
{
    Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)

        [Header(Cel Shading)]
        _ShadowColor("Shadow Color", Color) = (0.6, 0.6, 0.7, 1)
        _ShadowThreshold("Shadow Threshold", Range(0, 1)) = 0.5
        _ShadowSmoothness("Shadow Band Smoothness", Range(0.001, 0.3)) = 0.05

        [Header(Rim Light)]
        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimThreshold("Rim Threshold", Range(0, 1)) = 0.7
        _RimSmoothness("Rim Smoothness", Range(0.001, 0.3)) = 0.1

        // 외곽선 프로퍼티는 여기 없음 — 화면공간 포스트프로세스(ToonOutlineFullScreen.shader)로 분리됨(6절/7-4절 근거)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        // 캐릭터 본체를 그리는 메인 라이팅 Pass
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back

            HLSLPROGRAM
            #pragma vertex ToonVertex
            #pragma fragment ToonFragment

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "ToonLitInput.hlsl"
            #include "ToonLitForwardPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex ToonShadowVertex
            #pragma fragment ToonShadowFragment

            #include "ToonLitInput.hlsl"
            #include "ToonShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ColorMask 0
            ZWrite On

            HLSLPROGRAM
            #pragma vertex ToonDepthOnlyVertex
            #pragma fragment ToonDepthOnlyFragment

            #include "ToonLitInput.hlsl"
            #include "ToonDepthOnlyPass.hlsl"
            ENDHLSL
        }

        // PC_Renderer의 SSAO Renderer Feature가 참조하는 노멀 버퍼 생성용 Pass
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On

            HLSLPROGRAM
            #pragma vertex ToonDepthNormalsVertex
            #pragma fragment ToonDepthNormalsFragment

            #include "ToonLitInput.hlsl"
            #include "ToonDepthNormalsPass.hlsl"
            ENDHLSL
        }
    }
}
