Shader "Custom/SpeedRadialBlur"
{
    Properties
    {
        _BlurIntensity("Blur Intensity", Range(0, 1)) = 0
        _InnerRadius("Inner Radius (Clear)", Range(0, 1)) = 0.35
        _OuterRadius("Outer Radius (Full Blur)", Range(0, 1)) = 0.75
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "SpeedRadialBlur"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment SpeedRadialBlurFragment

            // ToonOutlineFullScreen.shader와 동일한 include 순서(Core.hlsl 먼저, XR 텍스처 매크로 정의 후 Blit.hlsl)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // Vert()/Varyings/_BlitTexture/sampler_LinearClamp는 URP 코어의 Blit.hlsl(및 그 안의 GlobalSamplers.hlsl)이 제공
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // 컴파일 타임 상수 — 런타임 프로퍼티로 노출하지 않아 셰이더 복잡도를 낮춘다(0-5절 근거)
            #define SAMPLE_COUNT 8

            CBUFFER_START(UnityPerMaterial)
                half _BlurIntensity;
                half _InnerRadius;
                half _OuterRadius;
            CBUFFER_END

            half4 SpeedRadialBlurFragment(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float2 center = float2(0.5, 0.5);

                // 중심으로부터의 거리로 마스크를 만들어, InnerRadius 안쪽은 완전히 선명하게 유지하고
                // OuterRadius 바깥에서만 블러가 최대 강도가 되도록 한다(외곽부만 블러 처리)
                float distanceFromCenter = length(uv - center);
                float radiusMask = smoothstep(_InnerRadius, _OuterRadius, distanceFromCenter);

                // 화면 중심→현재 픽셀 방향으로, 강도(_BlurIntensity)와 위 마스크만큼 샘플 지점을 중심 쪽으로 당겨
                // 방사형(radial) 블러를 만든다. 마스크가 0인 중심부는 원본 uv와 같아져 블러가 사라진다
                float2 direction = (uv - center) * _BlurIntensity * radiusMask;

                half4 accumulatedColor = half4(0, 0, 0, 0);

                // SAMPLE_COUNT가 컴파일 타임 상수라 언롤 가능한 고정 루프
                for (int i = 0; i < SAMPLE_COUNT; i++)
                {
                    float t = (float)i / (SAMPLE_COUNT - 1);
                    float2 sampleUV = uv - direction * t;
                    accumulatedColor += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, sampleUV, 0);
                }

                return accumulatedColor / SAMPLE_COUNT;
            }
            ENDHLSL
        }
    }
}
