using System;
using UnityEngine;

/// <summary>
/// 플레이어의 배고픔 게이지를 관리하고, 배고픔 수치를 이동 속도로 매핑하여 Player에게 제공하는 클래스.
/// 시간 경과에 따른 지속 감소와 `IHungerObj`를 구현한 트리거 콜라이더(장애물/음식)와의 접촉을
/// 하나의 `ChangeHunger` 경로로 통합 처리하며, 배고픔이 변경될 때마다 통합 이벤트로 외부(연출, UI, 고양이 AI 등)에 알린다.
/// 이 스크립트는 트리거 이벤트를 받을 수 있는 Collider(CharacterController 포함)가 부착된
/// Player GameObject에 함께 부착한다.
/// </summary>
public class HungerController : MonoBehaviour
{
    [SerializeField] private PlayerStatData _playerStatData;

    private float _currentHunger;

    /// <summary>
    /// 배고픔 수치가 변경될 때마다 발생하는 통합 이벤트 (현재 배고픔 비율 0~1 전달).
    /// 시간 경과 감소, 장애물 충돌, 음식 섭취 등 배고픔이 바뀌는 모든 경로가 이 이벤트 하나로 알림을 보낸다
    /// </summary>
    public event Action<float> OnHungerChanged;

    /// <summary>
    /// 현재 배고픔 비율 (0~1). UI/연출/고양이 AI 등 다른 Feature가 배고픔 상태를 읽는 공개 인터페이스
    /// </summary>
    public float HungerRatio => Mathf.Clamp01(_currentHunger / _playerStatData.MaxHunger);

    /// <summary>
    /// 현재 배고픔 비율을 데이터 테이블의 매핑 곡선으로 변환한 이동 속도.
    /// 값을 캐싱하지 않고 매 호출마다 최신 배고픔 수치로 즉시 계산한다
    /// (CameraController.ForwardDirection과 동일하게, Update 실행 순서에 따른 한 프레임 지연을 피하기 위함)
    /// </summary>
    public float CurrentMoveSpeed => _playerStatData.EvaluateMoveSpeed(HungerRatio);

    /// <summary>
    /// 배고픔 수치를 최대치로 초기화한다
    /// </summary>
    private void Awake()
    {
        _currentHunger = _playerStatData.MaxHunger;
    }
    
    private void Update()
    {
        ChangeHunger(-_playerStatData.HungerDecayPerSecond * Time.deltaTime);
    }
    
    public void ChangeHunger(float amount)
    {
        _currentHunger = Mathf.Clamp(_currentHunger + amount, 0f, _playerStatData.MaxHunger);
        OnHungerChanged?.Invoke(HungerRatio);
    }
}
