#ifndef TOON_SHADOW_CASTER_PASS_INCLUDED
#define TOON_SHADOW_CASTER_PASS_INCLUDED

struct ShadowVaryings
{
    float4 positionHCS : SV_POSITION;
};

ShadowVaryings ToonShadowVertex(Attributes input)
{
    ShadowVaryings output = (ShadowVaryings)0;

    VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

    // 그림자맵 접선 방향 왜곡(피터패닝)을 줄이기 위해 URP 표준 바이어스 함수를 사용한다
    float3 biasedPositionWS = ApplyShadowBias(positionInputs.positionWS, normalInputs.normalWS, GetMainLight().direction);
    output.positionHCS = TransformWorldToHClip(biasedPositionWS);

    return output;
}

half4 ToonShadowFragment(ShadowVaryings input) : SV_TARGET
{
    return 0;
}

#endif
