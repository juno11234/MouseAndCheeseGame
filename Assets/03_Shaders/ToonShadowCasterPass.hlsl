#ifndef TOON_SHADOW_CASTER_PASS_INCLUDED
#define TOON_SHADOW_CASTER_PASS_INCLUDED

// 그림자 맵 용도
struct ShadowVaryings //클립 공간 좌표 하나만 필요
{
    float4 positionHCS : SV_POSITION;
};

ShadowVaryings ToonShadowVertex(Attributes input)
{
    ShadowVaryings output = (ShadowVaryings)0;

    VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

    // 정점의 월드 위치를 라이트 방향/노멀 기준으로 살짝 밀어냄 
    // 그림자맵 접선 방향 왜곡(피터패닝)을 줄이기 위해 URP 표준 바이어스 함수를 사용한다
    float3 biasedPositionWS = ApplyShadowBias(positionInputs.positionWS, normalInputs.normalWS,
                                              GetMainLight().direction);
    output.positionHCS = TransformWorldToHClip(biasedPositionWS); //클립 공간 변환

    return output;
}

half4 ToonShadowFragment(ShadowVaryings input) : SV_TARGET
{
    return 0;
}

#endif
