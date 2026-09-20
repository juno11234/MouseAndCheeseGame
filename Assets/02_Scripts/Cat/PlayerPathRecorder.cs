using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player의 위치를 프레임 단위로 기록해 고양이가 따라갈 경로점 큐를 제공하는 클래스. Player와 같은 GameObject에 부착한다
/// </summary>
public class PlayerPathRecorder : MonoBehaviour
{
    [SerializeField] private Transform _catTransform;
    [SerializeField] private int _nearFrameInterval = 1;
    [SerializeField] private int _farFrameInterval = 10;
    [SerializeField] private float _farDistance = 20f;

    private Player _player;
    private readonly Queue<Vector3> _pathQueue = new Queue<Vector3>();
    private int _framesSinceLastRecord;
    private PlayerMoveState _cachedMoveState = PlayerMoveState.Run;

    /// <summary>
    /// 큐에 남은 경로점 개수(디버깅용)
    /// </summary>
    public int QueuedPointCount => _pathQueue.Count;

    /// <summary>
    /// 참조를 초기화한다
    /// </summary>
    private void Awake()
    {
        _player = GetComponent<Player>();
    }

    /// <summary>
    /// Player 이동 상태 변경 이벤트를 구독한다
    /// </summary>
    private void OnEnable()
    {
        _player.OnMoveStateChanged += HandleMoveStateChanged;
    }

    /// <summary>
    /// 이벤트 구독을 해제한다
    /// </summary>
    private void OnDisable()
    {
        _player.OnMoveStateChanged -= HandleMoveStateChanged;
    }

    /// <summary>
    /// Run 상태에서 고양이와의 직선거리로 정한 프레임 간격마다 현재 위치를 큐에 추가한다
    /// </summary>
    private void Update()
    {
        if (_cachedMoveState != PlayerMoveState.Run)
        {
            return;
        }

        _framesSinceLastRecord++;
        if (_framesSinceLastRecord >= ResolveFrameInterval())
        {
            _pathQueue.Enqueue(transform.position);
            _framesSinceLastRecord = 0;
        }
    }

    /// <summary>
    /// 고양이와의 직선거리가 멀수록 긴 프레임 간격, 가까울수록 짧은 프레임 간격을 반환한다
    /// </summary>
    private int ResolveFrameInterval()
    {
        float sqrDistanceToCat = (transform.position - _catTransform.position).sqrMagnitude;
        if (sqrDistanceToCat >= _farDistance * _farDistance)
        {
            return _farFrameInterval;
        }

        float ratio = Mathf.Sqrt(sqrDistanceToCat) / _farDistance;
        return Mathf.RoundToInt(Mathf.Lerp(_nearFrameInterval, _farFrameInterval, ratio));
    }

    /// <summary>
    /// 가장 오래된 경로점을 꺼낸다. 큐가 비어 있으면 false를 반환한다
    /// </summary>
    public bool TryDequeueNextPoint(out Vector3 point)
    {
        if (_pathQueue.Count == 0)
        {
            point = default;
            return false;
        }

        point = _pathQueue.Dequeue();
        return true;
    }

    /// <summary>
    /// 큐에 쌓인 경로점을 전부 버린다
    /// </summary>
    public void ClearQueue()
    {
        _pathQueue.Clear();
    }

    /// <summary>
    /// 이동 상태를 캐싱한다. Fly 중에는 경로점을 기록하지 않는다
    /// </summary>
    private void HandleMoveStateChanged(PlayerMoveState newState)
    {
        _cachedMoveState = newState;
    }
}
