using UnityEngine;

/// <summary>
/// HungerController.CurrentMoveSpeed를 폴링해 임계 속도 이상이면 카메라 FOV를 넓히고
/// 화면 라디얼 블러 강도를 높이는 연출을 담당하는 클래스. CameraPivot 오브젝트에 부착한다
/// </summary>
public class SpeedEffectController : MonoBehaviour
{
    private static readonly int BlurIntensityId = Shader.PropertyToID("_BlurIntensity");

    [SerializeField] private HungerController _hungerController;
    [SerializeField] private PlayerStatData _playerStatData;
    [SerializeField] private SpeedEffectData _speedEffectData;
    [SerializeField] private Camera _camera;
    [SerializeField] private Material _radialBlurMaterial;

    private float _fovVelocity;
    private float _blurVelocity;
    private float _currentBlurIntensity;

    /// <summary>
    /// 카메라 FOV를 평상시 값으로 초기화한다
    /// </summary>
    private void Start()
    {
        _camera.fieldOfView = _speedEffectData.BaseFov;
    }

    /// <summary>
    /// 매 프레임 발동 조건을 확인하고, 목표 FOV와 블러 강도로 서서히 근접시킨다.
    /// CameraController.LateUpdate()(피벗 추종, 벽 충돌 회피)와 동일한 타이밍에 맞춰
    /// 그 프레임의 최종 이동 상태를 반영하도록 LateUpdate에서 처리한다
    /// </summary>
    private void LateUpdate()
    {
        bool isBoosted = IsSpeedBoosted();

        float targetFov = isBoosted ? _speedEffectData.BoostedFov : _speedEffectData.BaseFov;
        _camera.fieldOfView = Mathf.SmoothDamp(_camera.fieldOfView, targetFov, ref _fovVelocity, _speedEffectData.FovSmoothTime);

        float targetBlur = isBoosted ? _speedEffectData.MaxBlurIntensity : 0f;
        _currentBlurIntensity = Mathf.SmoothDamp(_currentBlurIntensity, targetBlur, ref _blurVelocity, _speedEffectData.BlurSmoothTime);
        _radialBlurMaterial.SetFloat(BlurIntensityId, _currentBlurIntensity);
    }

    /// <summary>
    /// 현재 이동 속도가 임계 배고픔 비율(HungerRatioThreshold)에 해당하는 속도 이상인지 판정한다.
    /// 비교 자체는 요구사항에 따라 HungerRatio가 아니라 CurrentMoveSpeed로 수행하며,
    /// PlayerStatData.EvaluateMoveSpeed로 임계 비율을 속도 단위로 환산해 비교한다(0-3절 근거)
    /// </summary>
    private bool IsSpeedBoosted()
    {
        float thresholdSpeed = _playerStatData.EvaluateMoveSpeed(_speedEffectData.HungerRatioThreshold);
        return _hungerController.CurrentMoveSpeed >= thresholdSpeed;
    }
}
