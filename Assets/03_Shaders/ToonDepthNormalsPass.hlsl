#ifndef TOON_DEPTH_NORMALS_PASS_INCLUDED
#define TOON_DEPTH_NORMALS_PASS_INCLUDED

// PC_Renderer의 SSAO 및 ToonOutlineFullScreen 포스트프로세스가 공통으로 사용하는
// 노멀 버퍼 생성용 구조체
struct DepthNormalsVaryings
{
    float4 positionHCS : SV_POSITION;
    float3 normalWS    : TEXCOORD0;
};

DepthNormalsVaryings ToonDepthNormalsVertex(Attributes input)
{
    DepthNormalsVaryings output = (DepthNormalsVaryings)0;

    VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

    output.positionHCS = positionInputs.positionCS;
    output.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS); // 노멀을 정규화

    return output;
}

half4 ToonDepthNormalsFragment(DepthNormalsVaryings input) : SV_TARGET
{
    // PC_Renderer.asset은 Accurate G-Buffer Normals(Octahedral 인코딩)를 사용하지 않으므로
    // (m_AccurateGbufferNormals: 0) 월드공간 노멀을 그대로 기록한다
    float3 normalWS = NormalizeNormalPerPixel(input.normalWS); // 보간 왜곡 때문에 한번더 정규화
    return half4(normalWS, 0.0);
}

#endif
