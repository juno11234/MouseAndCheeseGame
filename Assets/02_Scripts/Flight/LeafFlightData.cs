using UnityEngine;

/// <summary>
/// 풀잎 비행의 밸런싱 수치를 담는 데이터 테이블 스크립터블 오브젝트.
/// "플레이어,고양이 스탯 관련 데이터 테이블" Feature에서 정식으로 구현될 예정이며,
/// 이 클래스는 그 전까지 Player가 참조할 최소한의 필드 계약(임시 스텁)이다.
/// 아래 기본값은 모두 개발 테스트용 임시 플레이스홀더이며, 실제 수치는 확정되지 않았다
/// (구현 단계 결정 필요 — 0절 참고).
/// </summary>
[CreateAssetMenu(fileName = "LeafFlightData", menuName = "MouseAndCheese/Leaf Flight Data")]
public class LeafFlightData : ScriptableObject
{
    [Header("비행")]
    [SerializeField] private float _flightDuration = 5f;
    [SerializeField] private float _ascendSpeed = 3f;

    [Header("쿨타임")]
    [SerializeField] private float _cooldownDuration = 15f;

    /// <summary>
    /// 비행 제한 시간(초). 이 시간 안에 착지하지 못하면 비행이 강제 종료된다
    /// </summary>
    public float FlightDuration => _flightDuration;

    /// <summary>
    /// 비행 중 자동으로 상승하는 속도(Player.ApplyGravity가 중력 대신 사용). 전진 속도는 별도로 두지 않는다 —
    /// 비행 중에도 Player의 기존 이동 로직(HungerController.CurrentMoveSpeed 기반)을 그대로 쓰기 때문이다
    /// </summary>
    public float AscendSpeed => _ascendSpeed;

    /// <summary>
    /// 풀잎을 한 번 사용한 뒤 그 풀잎이 다시 사용 가능해지기까지의 쿨타임(초).
    /// 쿨타임 상태 자체는 Player가 아니라 각 풀잎(LeafFlightTrigger)이 개별로 관리하며,
    /// 이 값은 그 계산에 쓰일 공유 밸런싱 수치만 제공한다
    /// </summary>
    public float CooldownDuration => _cooldownDuration;
}
