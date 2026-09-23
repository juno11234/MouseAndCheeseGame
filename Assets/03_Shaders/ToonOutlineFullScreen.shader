Shader "Custom/ToonOutlineFullScreen"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
        _DepthThreshold("Depth Threshold", Range(0.0001, 0.1)) = 0.01
        _NormalThreshold("Normal Threshold", Range(0, 1)) = 0.4
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "ToonOutlineFullScreen"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment ToonOutlineFullScreenFragment

            // Core.hlsl을 먼저 include해 XR 관련 텍스처 매크로(TEXTURE2D_X 등)를 정의한 뒤 Blit.hlsl을 include해야 한다
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // Vert()/Varyings/_BlitTexture는 URP 코어의 Blit.hlsl이 제공
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                half _DepthThreshold;
                half _NormalThreshold;
            CBUFFER_END

            half4 ToonOutlineFullScreenFragment(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                // _X는 XR 대응 _LOD 는 밉맵 레벨 지정(텍스터 해상도)
                half4 sceneColor = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, uv, 0); 

                // 인접 픽셀 오프셋 (화면 해상도 기준 1텍셀)
                float2 texel = _BlitTexture_TexelSize.xy;

                // Roberts Cross: 대각선 방향 2쌍의 샘플을 비교해 깊이/노멀이 급격히 변하는 지점(=윤곽선)을 찾는다
                float depthTL = SampleSceneDepth(uv + texel * float2(-1, -1));
                float depthBR = SampleSceneDepth(uv + texel * float2( 1,  1));
                float depthTR = SampleSceneDepth(uv + texel * float2( 1, -1));
                float depthBL = SampleSceneDepth(uv + texel * float2(-1,  1));
                float depthEdge = abs(depthTL - depthBR) + abs(depthTR - depthBL);

                float3 normalTL = SampleSceneNormals(uv + texel * float2(-1, -1));
                float3 normalBR = SampleSceneNormals(uv + texel * float2( 1,  1));
                float3 normalTR = SampleSceneNormals(uv + texel * float2( 1, -1));
                float3 normalBL = SampleSceneNormals(uv + texel * float2(-1,  1));
                float normalEdge = length(normalTL - normalBR) + length(normalTR - normalBL);

                // 둘 중 하나라도 임계값을 넘으면 외곽선으로 판단 (깊이만으로는 같은 평면 위의 그림 경계를 못 잡고,
                // 노멀만으로는 평행한 면이 겹쳐진 경우를 못 잡기 때문에 두 기준을 함께 사용한다)
                half edge = saturate(step(_DepthThreshold, depthEdge) + step(_NormalThreshold, normalEdge));

                return lerp(sceneColor, _OutlineColor, edge);
            }
            ENDHLSL
        }
    }
}
