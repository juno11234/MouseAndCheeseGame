using UnityEngine;

/// <summary>
/// 속도 연출(카메라 FOV·관성, 화면 라디얼 블러)의 밸런싱 수치를 담는 임시 스텁 ScriptableObject.
/// 정식 수치는 "데이터 테이블" Feature에서 확정한다(0-1절 참고)
/// </summary>
[CreateAssetMenu(fileName = "SpeedEffectData", menuName = "MouseAndCheese/Speed Effect Data")]
public class SpeedEffectData : ScriptableObject
{
    [Header("발동 조건")]
    [SerializeField] private float _hungerRatioThreshold = 0.75f;

    [Header("카메라 FOV")]
    [SerializeField] private float _baseFov = 60f;
    [SerializeField] private float _boostedFov = 75f;
    [SerializeField] private float _fovSmoothTime = 0.5f;

    [Header("화면 라디얼 블러")]
    [SerializeField] private float _maxBlurIntensity = 0.3f;
    [SerializeField] private float _blurSmoothTime = 0.5f;

    /// <summary>
    /// 연출이 발동하는 배고픔 비율 임계값(0~1). CurrentMoveSpeed와 비교하기 위해
    /// PlayerStatData.EvaluateMoveSpeed로 속도 단위로 환산해서 쓴다(0-3절, 4-2절 근거)
    /// </summary>
    public float HungerRatioThreshold => _hungerRatioThreshold;

    /// <summary>
    /// 평상시(비발동) 카메라 시야각
    /// </summary>
    public float BaseFov => _baseFov;

    /// <summary>
    /// 연출 발동 시 목표 카메라 시야각
    /// </summary>
    public float BoostedFov => _boostedFov;

    /// <summary>
    /// FOV가 목표값에 도달하는 대략적인 시간(SmoothDamp의 smoothTime, 관성감 조절용)
    /// </summary>
    public float FovSmoothTime => _fovSmoothTime;

    /// <summary>
    /// 연출 발동 시 라디얼 블러의 최대 강도(0~1)
    /// </summary>
    public float MaxBlurIntensity => _maxBlurIntensity;

    /// <summary>
    /// 블러 강도가 목표값에 도달하는 대략적인 시간(SmoothDamp의 smoothTime)
    /// </summary>
    public float BlurSmoothTime => _blurSmoothTime;
}
