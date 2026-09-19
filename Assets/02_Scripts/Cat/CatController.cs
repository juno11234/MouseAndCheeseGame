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
/// 기록된 경로를 NavMeshAgent로 따라가며 플레이어를 추격하고, 갭·러버밴딩·풀잎 비행 갭 중립화·게임오버 트리거를 처리한다. Cat GameObject에 부착한다
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class CatController : MonoBehaviour
{
    public delegate void GapChangedHandler(float gap);

    public delegate void GapDepletedHandler();

    [SerializeField] private Player _player;
    [SerializeField] private PlayerPathRecorder _pathRecorder;
    [SerializeField] private CatStatData _catStatData;

    private NavMeshAgent _agent;
    private CatState _state = CatState.Chasing;
    private Vector3? _currentTarget;
    private Vector3 _lastPosition;
    private float _catTraveledDistance;
    private float _gapSnapshotAtFlightStart;
    private bool _hasGapDepleted;

    /// <summary>
    /// 갭이 갱신될 때마다 발생하는 이벤트
    /// </summary>
    public event GapChangedHandler OnGapChanged;

    /// <summary>
    /// 갭이 0 이하가 되면 한 번 발생하는 이벤트(게임오버 트리거)
    /// </summary>
    public event GapDepletedHandler OnGapDepleted;

    /// <summary>
    /// 플레이어와 고양이 사이의 경로상 거리(플레이어 누적 이동 거리 - 고양이 누적 이동 거리)
    /// </summary>
    public float Gap => _pathRecorder.TotalPathDistance - _catTraveledDistance;

    /// <summary>
    /// 플레이어와의 직선거리
    /// </summary>
    private float DistanceToPlayer => Vector3.Distance(transform.position, _player.transform.position);

    /// <summary>
    /// NavMeshAgent를 설정하고 초기 갭을 반영한다
    /// </summary>
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.autoBraking = false;
        _lastPosition = transform.position;
        _catTraveledDistance = -_catStatData.InitialGap;
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
    /// 누적 이동 거리를 갱신하고 상태별 추격 동작을 수행하며, 갭이 0 이하가 되면 Caught로 전환하고 OnGapDepleted를 발생시킨다
    /// </summary>
    private void Update()
    {
        TrackTraveledDistance();
        Debug.Log(_state);
        switch (_state)
        {
            case CatState.Frozen:
            case CatState.Caught:
                return;
            case CatState.Chasing:
                FollowRecordedPath(ResolveChasingSpeed());
                if (DistanceToPlayer <= _catStatData.DirectChaseDistance)
                {
                    SetState(CatState.DirectChasing);
                }

                break;
            case CatState.DirectChasing:
                ChasePlayerDirectly();
                if (DistanceToPlayer > _catStatData.DirectChaseDistance)
                {
                    SetState(CatState.Chasing);
                }

                break;
            case CatState.GapCorrecting:
                FollowRecordedPath(_catStatData.GapCorrectionSpeed);
                if (Mathf.Abs(Gap - _gapSnapshotAtFlightStart) <= _catStatData.GapCorrectionTolerance)
                {
                    SetState(CatState.Chasing);
                }

                break;
        }

        OnGapChanged?.Invoke(Gap);

        if (Gap <= 0f && _hasGapDepleted == false)
        {
            _hasGapDepleted = true;
            SetState(CatState.Caught);
            OnGapDepleted?.Invoke();
        }
    }

    /// <summary>
    /// 이번 프레임에 실제로 이동한 거리를 고양이 누적 이동 거리에 더한다
    /// </summary>
    private void TrackTraveledDistance()
    {
        _catTraveledDistance += Vector3.Distance(transform.position, _lastPosition);
        _lastPosition = transform.position;
    }

    /// <summary>
    /// 갭이 MaxGap을 넘으면 러버밴딩 속도, 아니면 기본 속도를 반환한다
    /// </summary>
    private float ResolveChasingSpeed()
    {
        return Gap > _catStatData.MaxGap ? _catStatData.RubberBandSpeed : _catStatData.MoveSpeed;
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
    /// 상태를 바꾸고 NavMeshAgent 정지 여부를 맞춘다. 직접 추격 상태를 떠날 때는 큐와 현재 목표를 비워, 이후에는 상태가 바뀐 뒤 기록된 경로만 따르게 한다
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