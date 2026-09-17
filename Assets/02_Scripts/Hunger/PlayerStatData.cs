using UnityEngine;

/// <summary>
/// 플레이어의 배고픔·이동속도 밸런싱 수치를 담는 데이터 테이블 스크립터블 오브젝트.
/// "플레이어,고양이 스탯 관련 데이터 테이블" Feature에서 정식으로 구현될 예정이며,
/// 이 클래스는 그 전까지 HungerController가 참조할 최소한의 필드 계약(임시 스텁)이다.
/// 실제 밸런싱 값이 담긴 정식 에셋 제작은 이번 Feature 범위 밖이다.
/// </summary>
[CreateAssetMenu(fileName = "PlayerStatData", menuName = "MouseAndCheese/Player Stat Data")]
public class PlayerStatData : ScriptableObject
{
    [Header("배고픔")]
    [SerializeField] private float _maxHunger = 100f;
    [SerializeField] private float _hungerDecayPerSecond = 2f;

    [Header("배고픔 -> 이동속도 매핑")]
    [SerializeField] private float _minMoveSpeed = 2f;
    [SerializeField] private float _maxMoveSpeed = 15f;
    [SerializeField] private AnimationCurve _hungerToSpeedCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    /// <summary>
    /// 배고픔 게이지의 최대치
    /// </summary>
    public float MaxHunger => _maxHunger;

    /// <summary>
    /// 초당 배고픔 감소량
    /// </summary>
    public float HungerDecayPerSecond => _hungerDecayPerSecond;

    /// <summary>
    /// 배고픔 비율(0~1)을 이동 속도로 변환한다. _hungerToSpeedCurve로 곡선 형태(가속/감속 구간)를 조정하고,
    /// 곡선 평가 결과(0~1)를 _minMoveSpeed~_maxMoveSpeed 구간에 선형 보간해 최종 이동 속도를 계산한다
    /// </summary>
    public float EvaluateMoveSpeed(float hungerRatio)
    {
        float curveValue = _hungerToSpeedCurve.Evaluate(Mathf.Clamp01(hungerRatio));
        return Mathf.Lerp(_minMoveSpeed, _maxMoveSpeed, Mathf.Clamp01(curveValue));
    }
}
