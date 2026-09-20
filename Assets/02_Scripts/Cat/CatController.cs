using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 고양이의 추격 상태. Chasing(경로 추격), DirectChasing(플레이어 직접 추격), Frozen(비행 중 정지), GapCorrecting(착지 후 갭 보정), Caught(포획)
/// </summary>
public enum CatState
{
    Chasing,
    DirectChasing,
    Frozen,
    GapCorrecting,
    Caught
}

/// <summary>
/// 기록된 경로를 NavMeshAgent로 따라가며 플레이어를 추격하고, 직선거리 갭·러버밴딩·풀잎 비행 갭 중립화·충돌 포획을 처리한다. Cat GameObject에 부착한다
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class CatController : MonoBehaviour
{
    private static readonly int RunHash = Animator.StringToHash("Run");

    public delegate void GapChangedHandler(float gap);

    public delegate void PlayerCaughtHandler();

    [SerializeField] private Player _player;
    [SerializeField] private PlayerPathRecorder _pathRecorder;
    [SerializeField] private CatStatData _catStatData;

    private NavMeshAgent _agent;
    private Animator _animator;
    private CatState _state = CatState.Chasing;
    private Vector3? _currentTarget;
    private float _gapSnapshotAtFlightStart;

    /// <summary>
    /// 갭이 갱신될 때마다 발생하는 이벤트
    /// </summary>
    public event GapChangedHandler OnGapChanged;

    /// <summary>
    /// 고양이가 플레이어와 충돌해 포획하면 한 번 발생하는 이벤트(게임오버 트리거)
    /// </summary>
    public event PlayerCaughtHandler OnPlayerCaught;

    /// <summary>
    /// 고양이와 플레이어 사이의 직선거리
    /// </summary>
    public float Gap => Vector3.Distance(transform.position, _player.transform.position);

    /// <summary>
    /// 고양이와 플레이어 사이의 직선거리가 distance 이하인지 제곱 거리로 비교한다
    /// </summary>
    private bool IsWithinGap(float distance)
    {
        Vector3 offset = transform.position - _player.transform.position;
        return offset.sqrMagnitude <= distance * distance;
    }

    /// <summary>
    /// NavMeshAgent와 Animator를 가져와 설정한다
    /// </summary>
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
        _agent.autoBraking = false;
    }

    /// <summary>
    /// Player 이동 상태 변경 이벤트를 구독한다
    /// </summary>
    private void OnEnable()
    {
        _player.OnMoveStateChanged += HandlePlayerMoveStateChanged;
    }

    /// <summary>
    /// 이벤트 구독을 해제한다
    /// </summary>
    private void OnDisable()
    {
        _player.OnMoveStateChanged -= HandlePlayerMoveStateChanged;
    }

    /// <summary>
    /// 상태별 추격 동작을 수행하고 갭 갱신 이벤트를 발생시킨다
    /// </summary>
    private void Update()
    {
        Debug.Log(_state);
        switch (_state)
        {
            case CatState.Frozen:
            case CatState.Caught:
                return;
            case CatState.Chasing:
                FollowRecordedPath(ResolveChasingSpeed());
                if (IsWithinGap(_catStatData.DirectChaseDistance))
                {
                    SetState(CatState.DirectChasing);
                }

                break;
            case CatState.DirectChasing:
                ChasePlayerDirectly();
                if (IsWithinGap(_catStatData.DirectChaseDistance) == false)
                {
                    SetState(CatState.Chasing);
                }

                break;
            case CatState.GapCorrecting:
                FollowRecordedPath(_catStatData.GapCorrectionSpeed);
                // 갭은 스냅샷 위에서 아래로만 줄어들므로 단방향 비교로 프레임 간 감소량이 커도 허용 구간을 건너뛰지 않는다
                if (IsWithinGap(_gapSnapshotAtFlightStart + _catStatData.GapCorrectionTolerance))
                {
                    SetState(CatState.Chasing);
                }

                break;
        }

        OnGapChanged?.Invoke(Gap);
    }

    /// <summary>
    /// 플레이어와 충돌하면 Caught로 전환하고 OnPlayerCaught를 발생시킨다. Frozen·Caught 상태에서는 무시한다
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (_state == CatState.Frozen || _state == CatState.Caught)
        {
            return;
        }

        if (other.TryGetComponent(out Player _))
        {
            SetState(CatState.Caught);
            _animator.SetBool(RunHash, false);
            OnPlayerCaught?.Invoke();
        }
    }

    /// <summary>
    /// 갭이 MaxGap 이하면 기본 속도, 넘으면 러버밴딩 속도를 반환한다
    /// </summary>
    private float ResolveChasingSpeed()
    {
        return IsWithinGap(_catStatData.MaxGap) ? _catStatData.MoveSpeed : _catStatData.RubberBandSpeed;
    }

    /// <summary>
    /// 큐의 다음 경로점을 목적지로 지정하고, 도착하면 다음 경로점을 소비할 수 있게 목표를 비운다
    /// </summary>
    private void FollowRecordedPath(float speed)
    {
        _agent.speed = speed;

        if (_currentTarget.HasValue == false && _pathRecorder.TryDequeueNextPoint(out Vector3 next))
        {
            _currentTarget = next;
            _agent.SetDestination(next);
            Debug.Log($"[Cat] 경로점 목표 지정: {next}");
        }

        if (_currentTarget.HasValue == false)
        {
            return;
        }

        if (_agent.pathPending == false && _agent.remainingDistance <= _catStatData.WaypointArrivalTolerance)
        {
            _currentTarget = null;
        }
    }

    /// <summary>
    /// 플레이어의 현재 위치를 목적지로 직접 추격한다
    /// </summary>
    private void ChasePlayerDirectly()
    {
        _agent.speed = _catStatData.MoveSpeed;
        _agent.SetDestination(_player.transform.position);
        Debug.Log($"[Cat] 플레이어 직접 추격 목표 지정: {_player.transform.position}");
    }

    /// <summary>
    /// 상태를 바꾸고 NavMeshAgent 정지 여부와 Animator Run(Frozen에서만 false)을 맞춘다. 직접 추격 상태를 떠날 때는 큐와 현재 목표를 비운다
    /// </summary>
    private void SetState(CatState newState)
    {
        if (_state == CatState.DirectChasing && newState != CatState.DirectChasing)
        {
            _pathRecorder.ClearQueue();
            _currentTarget = null;
        }

        _state = newState;

        bool shouldStop = newState == CatState.Frozen || newState == CatState.Caught;
        _agent.isStopped = shouldStop;
        if (shouldStop)
        {
            _agent.velocity = Vector3.zero;
        }

        _animator.SetBool(RunHash, newState != CatState.Frozen);
    }

    /// <summary>
    /// Fly가 되면 갭을 기록하고 정지, Run으로 돌아오면 갭 보정을 시작한다. Caught 이후에는 무시한다
    /// </summary>
    private void HandlePlayerMoveStateChanged(PlayerMoveState newState)
    {
        if (_state == CatState.Caught)
        {
            return;
        }

        if (newState == PlayerMoveState.Fly)
        {
            _gapSnapshotAtFlightStart = Gap;
            SetState(CatState.Frozen);
        }
        else
        {
            SetState(CatState.GapCorrecting);
        }
    }
}