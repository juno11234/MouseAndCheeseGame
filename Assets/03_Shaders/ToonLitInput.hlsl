#ifndef TOON_LIT_INPUT_INCLUDED
#define TOON_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

// SRP Batcher 호환을 위해 머티리얼별 프로퍼티는 반드시 이 CBUFFER 안에 선언한다
CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half4 _ShadowColor;
    half _ShadowThreshold;
    half _ShadowSmoothness;
    half4 _RimColor;
    half _RimThreshold;
    half _RimSmoothness;
CBUFFER_END

// ForwardLit/ShadowCaster/DepthOnly/DepthNormals Pass가 공유하는 정점 입력
struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS   : NORMAL;
    float2 uv         : TEXCOORD0;
};

// ForwardLit Pass의 보간 출력
struct Varyings
{
    float4 positionHCS : SV_POSITION;
    float2 uv          : TEXCOORD0;
    float3 normalWS    : TEXCOORD1;
    float3 positionWS  : TEXCOORD2;
    float4 shadowCoord  : TEXCOORD3;
};

#endif
