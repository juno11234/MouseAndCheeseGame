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
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry"
        }

        // 캐릭터 본체를 그리는 메인 라이팅 Pass
        Pass
        {
            Name "ForwardLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            Cull Back // 삼각형 앞면만 그려라 (기본값)

            HLSLPROGRAM
            #pragma vertex ToonVertex
            #pragma fragment ToonFragment

            // multi_compile A B 두형식을 모두 컴파일 _는 안켜진 상태, GPU가 알아서 상황에 맞게 골라씀
            // 쓰지 않는 그림자 연산비용 아낌, URP 에서 그림자 관련설정에 맞춰 사용
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS // 메인 라이트가 그림자를 드리우는지
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE // 카메라 거리에 따라 그림자 해상도 사용여부
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_SCREEN // 그림자를 화면공간에서 한번 더 처리
            #pragma multi_compile_fragment _ _SHADOWS_SOFT // 그림자 경계를 부드럽게할지, fragment를 붙여 픽셀 단계에서만
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK

            #include "ToonLitInput.hlsl"
            #include "ToonLitForwardPass.hlsl"
            ENDHLSL
        }

        // 다른 오브젝트에 그림자를 드리우기 위한 패스
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
            Cull Back
            ZWrite On // 지금 그리는 픽셀의 깊이값을 기록할지
            ZTest LEqual // 픽셀을 그릴지 말지 깊이 버퍼와 비교후 판단

            HLSLPROGRAM
            #pragma vertex ToonShadowVertex
            #pragma fragment ToonShadowFragment

            #include "ToonLitInput.hlsl"
            #include "ToonShadowCasterPass.hlsl"
            ENDHLSL
        }

        // 포스트 프로세싱 or 불투명 텍스처 샘플링에 사용
        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }
            ColorMask 0
            ZWrite On

            HLSLPROGRAM
            #pragma vertex ToonDepthOnlyVertex
            #pragma fragment ToonDepthOnlyFragment

            #include "ToonLitInput.hlsl"
            #include "ToonDepthOnlyPass.hlsl"
            ENDHLSL
        }

        // PC_Renderer의 SSAO Renderer Feature가 참조하는 노멀 방향 버퍼 생성용 Pass
        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }
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