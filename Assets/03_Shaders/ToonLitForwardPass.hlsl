#ifndef TOON_LIT_FORWARD_PASS_INCLUDED
#define TOON_LIT_FORWARD_PASS_INCLUDED

Varyings ToonVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

    output.positionWS = positionInputs.positionWS;
    output.positionHCS = positionInputs.positionCS;
    output.normalWS = normalInputs.normalWS;
    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);

    // 메인 라이트 그림자 샘플링을 위한 그림자맵 좌표. GetMainLight(shadowCoord) 호출에 사용한다
    output.shadowCoord = TransformWorldToShadowCoord(positionInputs.positionWS);

    return output;
}

half4 ToonFragment(Varyings input) : SV_TARGET
{
    half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;

    float3 normalWS = normalize(input.normalWS);

    // URP 라이팅 데이터(메인 라이트 방향/색상/그림자 감쇠)는 GetMainLight()로 가져온다.
    // shadowCoord를 함께 넘기면 shadowAttenuation에 그림자맵 결과가 반영된다
    Light mainLight = GetMainLight(input.shadowCoord);

    half NdotL = dot(normalWS, mainLight.direction);
    half litMask = NdotL * mainLight.shadowAttenuation;

    // smoothstep으로 경계를 완만하게 처리한 2-step 밴딩 (완전 계단식이 필요하면 step()으로 교체)
    half lightBand = smoothstep(_ShadowThreshold - _ShadowSmoothness, _ShadowThreshold + _ShadowSmoothness, litMask);

    half3 shading = lerp(_ShadowColor.rgb, mainLight.color, lightBand);
    half3 albedo = baseColor.rgb * shading;

    // 림 라이트: 시야 방향과 노멀이 수직에 가까울수록(가장자리) 밝아진다. 그림자 영역에는 가산하지 않는다
    float3 viewDirWS = normalize(GetCameraPositionWS() - input.positionWS);
    half rim = 1.0 - saturate(dot(viewDirWS, normalWS));
    half rimBand = smoothstep(_RimThreshold - _RimSmoothness, _RimThreshold + _RimSmoothness, rim) * lightBand;

    half3 finalColor = albedo + _RimColor.rgb * rimBand;

    return half4(finalColor, baseColor.a);
}

#endif
