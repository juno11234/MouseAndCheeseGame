#ifndef TOON_DEPTH_ONLY_PASS_INCLUDED
#define TOON_DEPTH_ONLY_PASS_INCLUDED

// _CameraDepthTexture 생성 전용이므로 클립공간 위치만 필요한 최소 구조체를 사용한다
struct DepthOnlyVaryings
{
    float4 positionHCS : SV_POSITION;
};

DepthOnlyVaryings ToonDepthOnlyVertex(Attributes input)
{
    DepthOnlyVaryings output = (DepthOnlyVaryings)0;

    VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
    output.positionHCS = positionInputs.positionCS;

    return output;
}

half4 ToonDepthOnlyFragment(DepthOnlyVaryings input) : SV_TARGET
{
    return 0;
}

#endif
