using UnityEngine;

/// <summary>
/// InputManager로부터 좌우 회전 입력을 받아 카메라 피벗(자신)을 Y축 회전시키고,
/// SphereCast로 벽 등 지오메트리와의 충돌을 검사해 카메라가 벽을 뚫고 들어가는
/// 클리핑 현상을 방지하는 클래스. 이 스크립트는 카메라 피벗(CameraPivot) 오브젝트에 부착한다.
/// </summary>
public class CameraController : MonoBehaviour
{
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private float _rotationSpeed = 180f;
    [SerializeField] private Transform player;
    [SerializeField] private float _pivotHeight = 1.5f;
    [Header("벽 충돌 회피 설정")]
    [SerializeField] private LayerMask _collisionLayerMask;
    [SerializeField] private float _collisionRadius = 0.3f;
    [SerializeField] private float _collisionBuffer = 0.2f;
    [SerializeField] private float _minDistance = 0.5f;
    [SerializeField] private float _cameraSmoothSpeed = 15f;
    [SerializeField] private float _snapSpeed = 20f;

    private float _defaultDistance;
    private Vector3 _cameraVelocity;

    /// <summary>
    /// 카메라 피벗이 현재 바라보는 수평(Y=0으로 투영) 정면 방향. Player의 전진 이동 방향 계산에 사용된다.
    /// 매 호출 시 최신 회전값으로 즉시 계산되는 프로퍼티이며, 별도로 캐싱하지 않는다
    /// (Update 실행 순서에 따른 한 프레임 지연 문제를 피하기 위함)
    /// </summary>
    public Vector3 ForwardDirection
    {
        get
        {
            Vector3 flatForward = transform.forward;
            flatForward.y = 0f;
            return flatForward.normalized;
        }
    }

    /// <summary>
    /// 피벗과 카메라 사이의 기본(충돌 없을 때) 거리를 최초 배치된 로컬 위치 기준으로 측정한다
    /// </summary>
    private void Start()
    {
        _defaultDistance = Vector3.Distance(transform.position, _cameraTransform.position);
    }

    /// <summary>
    /// 매 프레임 좌우 회전 입력을 처리한다
    /// </summary>
    private void Update()
    {
        HandleRotation();
    }

    /// <summary>
    /// 모든 Update가 끝난 뒤(Player 이동 반영 후) 벽 충돌 검사를 수행한다
    /// </summary>
    private void LateUpdate()
    {
        FollowPlayer();
        HandleCollision();
    }

    /// <summary>
    /// 피벗을 플레이어 위치(+ 높이 오프셋)에 고정시켜, 좌우 회전이 플레이어를 축으로 이루어지게 한다
    /// </summary>
    private void FollowPlayer()
    {
        transform.position = player.position + Vector3.up * _pivotHeight;
    }

    /// <summary>
    /// MoveInput의 X값(A/D)으로 카메라 피벗을 Y축 기준 좌우 회전시킨다
    /// </summary>
    private void HandleRotation()
    {
        float turnInput = _inputManager.MoveInput.x;
        transform.Rotate(Vector3.up, turnInput * _rotationSpeed * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// 피벗에서 카메라 방향(-forward)으로 SphereCast를 쏘아 벽 등 지오메트리에 닿으면
    /// 카메라와의 목표 거리를 줄여, 카메라가 지오메트리를 뚫고 들어가는 현상을 방지한다
    /// </summary>
    private void HandleCollision()
    {
        Vector3 direction = -transform.forward;
        float targetDistance = _defaultDistance;
        bool needsInstantSnap = false;

        bool isHit = Physics.SphereCast(transform.position, _collisionRadius, direction,
            out RaycastHit hit, _defaultDistance, _collisionLayerMask);

        if (isHit)
        {
            targetDistance = Mathf.Max(_minDistance, hit.distance - _collisionBuffer);
            needsInstantSnap = true;
        }
        else
        {
            Vector3 backCheckPoint = transform.position + direction * _collisionRadius;
            if (Physics.CheckSphere(backCheckPoint, _collisionRadius, _collisionLayerMask))
            {
                // 피벗 바로 뒤(카메라 방향)에 지오메트리가 겹쳐 SphereCast가 감지하지 못하는 경우만 보정
                targetDistance = _minDistance;
                needsInstantSnap = true;
            }
        }

        Vector3 targetPosition = transform.position + direction * targetDistance;
        float currentDistanceSqr = (transform.position - _cameraTransform.position).sqrMagnitude;
        float targetDistanceSqr = targetDistance * targetDistance;
        
        if (needsInstantSnap && currentDistanceSqr > targetDistanceSqr)
        {
            _cameraTransform.position = Vector3.MoveTowards(_cameraTransform.position, targetPosition,
                _snapSpeed * Time.deltaTime);
            _cameraVelocity = Vector3.zero;
        }
        else
        {
            _cameraTransform.position = Vector3.SmoothDamp(_cameraTransform.position, targetPosition,
                ref _cameraVelocity, 1f / _cameraSmoothSpeed);
        }
    }
}
