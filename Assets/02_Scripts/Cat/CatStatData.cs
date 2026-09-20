using UnityEngine;

/// <summary>
/// 고양이 AI 밸런싱 수치를 담는 임시 스텁 ScriptableObject. 정식 수치는 데이터 테이블 Feature에서 확정한다
/// </summary>
[CreateAssetMenu(fileName = "CatStatData", menuName = "MouseAndCheese/Cat Stat Data")]
public class CatStatData : ScriptableObject
{
    [Header("기본 이동")]
    [SerializeField] private float _moveSpeed = 4f;
    [SerializeField] private float _waypointArrivalTolerance = 0.1f;

    [Header("직접 추격")]
    [SerializeField] private float _directChaseDistance = 2f;

    [Header("러버밴딩")]
    [SerializeField] private float _maxGap = 15f;
    [SerializeField] private float _rubberBandSpeed = 8f;

    [Header("풀잎 비행 갭 보정")]
    [SerializeField] private float _gapCorrectionSpeed = 10f;
    [SerializeField] private float _gapCorrectionTolerance = 0.1f;

    /// <summary>
    /// 평상시 및 직접 추격 중 이동 속도
    /// </summary>
    public float MoveSpeed => _moveSpeed;

    /// <summary>
    /// 경로점에 도착했다고 간주할 거리 허용 오차
    /// </summary>
    public float WaypointArrivalTolerance => _waypointArrivalTolerance;

    /// <summary>
    /// 플레이어와의 직선거리가 이 값 이하로 좁혀지면 플레이어를 직접 추격한다
    /// </summary>
    public float DirectChaseDistance => _directChaseDistance;

    /// <summary>
    /// 갭이 이 값을 넘으면 러버밴딩이 발동한다
    /// </summary>
    public float MaxGap => _maxGap;

    /// <summary>
    /// 러버밴딩 중 이동 속도
    /// </summary>
    public float RubberBandSpeed => _rubberBandSpeed;

    /// <summary>
    /// 착지 후 갭 보정 중 이동 속도
    /// </summary>
    public float GapCorrectionSpeed => _gapCorrectionSpeed;

    /// <summary>
    /// 갭 보정 완료로 간주할 허용 오차(갭이 비행 전 갭 + 이 값 이하가 되면 완료)
    /// </summary>
    public float GapCorrectionTolerance => _gapCorrectionTolerance;
}
