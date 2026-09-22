#ifndef TOON_DEPTH_ONLY_PASS_INCLUDED
#define TOON_DEPTH_ONLY_PASS_INCLUDED
// 순수 클립 공간 깊이버퍼 기록용, 카메라 기준 가까운 순서부터 그리는 등 용도에 사용
struct DepthOnlyVaryings// 클립공간 위치만 필요한 최소 구조체를 사용한다
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
