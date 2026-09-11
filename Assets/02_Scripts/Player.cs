using UnityEngine;

/// <summary>
/// InputManager로부터 전진 입력을 받아, CameraController가 바라보는 방향으로
/// CharacterController를 이용해 전진 이동을 처리하고, 시각적 모델이 이동 방향을 바라보게 하는 클래스
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private Transform _visualTransform;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 720f;
    [SerializeField] private float _jumpForce = 8f;
    [SerializeField] private int _maxJumpCount = 2;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask _groundLayer;
    private float _gravity = -9.81f;
    private Vector3 _velocity;
    private CharacterController _characterController;
    private bool _autoMove = false;
    private int _jumpCount;
    private bool _sphereHit;

    /// <summary>
    /// 같은 GameObject에 부착된 CharacterController 컴포넌트를 캐싱한다
    /// </summary>
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    /// <summary>
    /// InputManager의 OnAutoInput 이벤트를 구독해 Q 입력마다 자동 전진 상태를 토글한다
    /// </summary>
    private void OnEnable()
    {
        _inputManager.OnAutoInput += ToggleAutoMove;
        _inputManager.OnJumpInput += HandleJumpInput;
    }

    /// <summary>
    /// OnAutoInput, OnJumpInput 이벤트 구독을 해제한다
    /// </summary>
    private void OnDisable()
    {
        _inputManager.OnAutoInput -= ToggleAutoMove;
        _inputManager.OnJumpInput -= HandleJumpInput;
    }

    /// <summary>
    /// 자동 전진 상태(_autoMove)를 반전시킨다. 켜져 있으면 다시 Q를 눌렀을 때 꺼진다
    /// </summary>
    private void ToggleAutoMove(bool isPressed)
    {
        _autoMove = !_autoMove;
    }

    /// <summary>
    /// 스페이스바 입력 시 남은 점프 횟수가 있으면 위쪽 속도를 부여해 점프시킨다 (최대 _maxJumpCount회, 2단 점프)
    /// </summary>
    private void HandleJumpInput(bool isPressed)
    {
        if (_jumpCount >= _maxJumpCount)
        {
            return;
        }

        _velocity.y = _jumpForce;
        _jumpCount++;
    }

    /// <summary>
    /// 발밑으로 SphereCast를 쏘아 바닥에 닿아 있는지 확인한다
    /// </summary>
    public bool IsGrounded()
    {
        _sphereHit = Physics.SphereCast
        (transform.position, groundCheckRadius, Vector3.down,
            out _, groundCheckDistance, _groundLayer);

        return _sphereHit;
    }

    /// <summary>
    /// 자동 전진 중이면 항상 전진, 아니면 MoveInput의 Y값(W, 음수는 무시)으로 이동 방향을 계산해 이동과 회전을 처리한다
    /// </summary>
    private void Update()
    {
        float forwardInput = _autoMove ? 1f : Mathf.Max(0f, _inputManager.MoveInput.y);
        Vector3 moveDirection = _cameraController.ForwardDirection * forwardInput;

        ApplyGravity();
        HandleMovement(moveDirection);
        HandleRotation(moveDirection);
    }

    /// <summary>
    /// 주어진 방향으로 CharacterController를 이용해 이동시킨다
    /// </summary>
    private void HandleMovement(Vector3 moveDirection)
    {
        _characterController.Move(moveDirection * (_moveSpeed * Time.deltaTime));
    }

    /// <summary>
    /// 이동 중일 때 시각적 모델(_visualTransform)만 이동 방향을 바라보도록 회전시킨다.
    /// Player 루트가 아니라 자식(Capsule)을 회전시켜, 형제 오브젝트인 CameraPivot의 회전에 영향을 주지 않는다
    /// </summary>
    private void HandleRotation(Vector3 moveDirection)
    {
        if (moveDirection == Vector3.zero)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        _visualTransform.rotation = Quaternion.RotateTowards(_visualTransform.rotation, targetRotation,
            _rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 매 프레임 중력을 적용한다. 바닥에 닿아 있고 낙하 중이면 속도와 점프 횟수를 초기화한다
    /// </summary>
    private void ApplyGravity()
    {
        if (IsGrounded() && _velocity.y < 0f)
        {
            _velocity.y = -2f;
            _jumpCount = 0;
        }

        _velocity.y += _gravity * Time.deltaTime;
        if (_velocity.y < _gravity)
        {
            _velocity.y = _gravity;
        }

        _characterController.Move(_velocity * Time.deltaTime);
    }
}