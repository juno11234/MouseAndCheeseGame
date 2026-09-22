#ifndef TOON_LIT_INPUT_INCLUDED // 중복 방지용
#define TOON_LIT_INPUT_INCLUDED

// 유니티 공식함수 라이브러리 2개
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

// 텍스처와 샘플러
TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

// SRP Batcher 호환을 위해 머티리얼별 프로퍼티는 반드시 이 CBUFFER 안에 선언한다
// 순서도 프로퍼티와 맞춰서
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

// ForwardLit/ShadowCaster/DepthOnly/DepthNormals Pass가 공유
// 버텍스에 인풋하는 값
struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS   : NORMAL;
    float2 uv         : TEXCOORD0;
};

// ForwardLit Pass의 보간 출력
// 버텍스 아웃풋, 프래그먼트 인풋
struct Varyings
{
    float4 positionHCS : SV_POSITION;
    float2 uv          : TEXCOORD0;
    float3 normalWS    : TEXCOORD1;
    float3 positionWS  : TEXCOORD2;
    float4 shadowCoord  : TEXCOORD3;
};

#endif
