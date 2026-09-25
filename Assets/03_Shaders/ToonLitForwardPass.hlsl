#ifndef TOON_LIT_FORWARD_PASS_INCLUDED
#define TOON_LIT_FORWARD_PASS_INCLUDED
// 해당 hlsl이 메인 나머지는 보조
Varyings ToonVertex(Attributes input) // 버텍스 셰이더
{
    Varyings output = (Varyings)0;

    //오브젝트 좌표를 월드, 뷰 ,클립으로 한번에 변환
    VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

    output.positionWS = positionInputs.positionWS;
    output.positionHCS = positionInputs.positionCS;
    output.normalWS = normalInputs.normalWS;
    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);

    // 메인 라이트 그림자 샘플링을 위한 그림자맵 좌표.
    // 정점 단계에서 계산하는게 비용이 저렴
    output.shadowCoord = TransformWorldToShadowCoord(positionInputs.positionWS);
    // LIGHTMAP_ON(동적) 여부에 따라 매크로가 알아서 처리 
    OUTPUT_LIGHTMAP_UV(input.staticLightmap, unity_LightmapST, output.staticLightmap);
    
    return output;
}

half4 ToonFragment(Varyings input) : SV_TARGET //픽셀 셰이더
{
    half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor; // 원본 알베도 구하기

    float3 normalWS = normalize(input.normalWS);

    // URP 라이팅 데이터(메인 라이트 방향/색상/그림자 감쇠)는 GetMainLight()로 가져온다.
    // shadowCoord를 함께 넘기면 shadowAttenuation에 그림자맵 결과가 반영된다
    // Light mainLight = GetMainLight(input.shadowCoord); 기존 코드

    // 정적 오브젝트면 구워둔 shadowmask에서, 동적 오브젝트면 unity_ProbesOcclusion 에서 값 가져옴
    half4 shadowMask = SAMPLE_SHADOWMASK(input.staticLightmap);
    Light mainLight = GetMainLight(input.shadowCoord, input.positionWS, shadowMask);// 값넘기면 알아서 구분


    half NdotL = dot(normalWS, mainLight.direction); // 램버트 반사 기본 항 (내적으로 빛과 얼마나 마주 보는지)
    half litMask = NdotL * mainLight.shadowAttenuation; // 그림자에 가려지지 않은 정도를 곱해 해당 픽셀이 얼마나 밝은지

    // 일반 셰이더는 litMask를 그대로 밝기에 곱해서 부드러운 그라데이션
    // 툰 셰이더는 smoothstep으로 임계값(ShadowThreshold) 기준 0, 1 두단계로 나눔
    half lightBand = smoothstep(_ShadowThreshold - _ShadowSmoothness, _ShadowThreshold + _ShadowSmoothness, litMask);

    // 두단계로 나눈 lightBand를 그림자색과 라이트색 보간
    half3 shading = lerp(_ShadowColor.rgb, mainLight.color, lightBand);
    half3 albedo = baseColor.rgb * shading; // 셀 셰이딩 적용

    // 림 라이트: 시야 방향과 노멀이 수직에 가까울수록(가장자리) 밝아진다. 그림자 영역에는 가산하지 않는다
    float3 viewDirWS = normalize(GetCameraPositionWS() - input.positionWS); // 카메라를 향하는 방향
    half rim = 1.0 - saturate(dot(viewDirWS, normalWS)); // 카메라를 정면을 볼수록 1 아니면 0 이걸 뒤집음
    // 0,1 두단계로 나눔 lightBand를 곱해 이미 그림자져있는 부분 반영
    half rimBand = smoothstep(_RimThreshold - _RimSmoothness, _RimThreshold + _RimSmoothness, rim) * lightBand;

    half3 finalColor = albedo + _RimColor.rgb * rimBand; // 최종색을 구하고

    return half4(finalColor, baseColor.a); // 반환
}

#endif
