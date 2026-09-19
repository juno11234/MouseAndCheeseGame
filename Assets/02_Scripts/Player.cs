using UnityEngine;

/// <summary>
/// Player의 이동 상태. Run은 기존 지상 이동(중력 적용), Fly는 풀잎 비행 중(중력 대신 자동 상승) 상태를 뜻한다
/// </summary>
public enum PlayerMoveState
{
    Run,
    Fly
}

/// <summary>
/// InputManager로부터 전진 입력을 받아, CameraController가 바라보는 방향으로
/// CharacterController를 이용해 전진 이동을 처리하고, 시각적 모델이 이동 방향을 바라보게 하는 클래스
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    private static readonly int Fly = Animator.StringToHash("Fly");

    public delegate void MoveStateChangedHandler(PlayerMoveState newState);


    [SerializeField] private LeafFlightData _leafFlightData;
    [SerializeField] private GameObject _leaf;

    [SerializeField] private float _jumpForce = 8f;
    [SerializeField] private int _maxJumpCount = 2;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask _groundLayer;

    private CharacterController _characterController;
    private InputManager _inputManager;
    private HungerController _hungerController;
    private Animator _animator;
    private CameraController _cameraController;

    private Transform _visualTransform;
    private float _rotationSpeed = 720f;
    private float _gravity = -9.81f;
    private Vector3 _velocity;
    private bool _autoMove = false;
    private int _jumpCount;
    private bool _sphereHit;
    private PlayerMoveState _moveState = PlayerMoveState.Run;
    private float _flightTimeRemaining;

    /// <summary>
    /// 이동 상태(Run/Fly)가 바뀔 때 발생하는 이벤트. 아직 구현되지 않은 고양이 AI("플레이어 추격")가
    /// Fly로 바뀌면 갭을 기록하고 제자리에서 대기, Run으로 돌아오면 속도 보정을 시작하는 데 사용할 훅이다.
    /// </summary>
    public event MoveStateChangedHandler OnMoveStateChanged;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _inputManager = GetComponent<InputManager>();
        _hungerController = GetComponent<HungerController>();
        _animator = GetComponentInChildren<Animator>();
        _cameraController = GetComponentInChildren<CameraController>();
        _visualTransform = _animator.gameObject.transform;
    }

    private void OnEnable()
    {
        _inputManager.OnAutoInput += ToggleAutoMove;
        _inputManager.OnJumpInput += HandleJumpInput;
    }

    private void OnDisable()
    {
        _inputManager.OnAutoInput -= ToggleAutoMove;
        _inputManager.OnJumpInput -= HandleJumpInput;
    }

    /// <summary>
    /// 자동 전진 중이면 항상 전진, 아니면 MoveInput의 Y값(W, 음수는 무시)으로 이동 방향을 계산해 이동과 회전을 처리한다.
    /// 이동 상태에 따라 ApplyGravity(Run) 또는 ApplyFlying(Fly)을 분기 호출
    /// </summary>
    private void Update()
    {
        float forwardInput = _autoMove ? 1f : Mathf.Max(0f, _inputManager.MoveInput.y);
        Vector3 moveDirection = _cameraController.ForwardDirection * forwardInput;

        switch (_moveState)
        {
            case PlayerMoveState.Run:
                ApplyGravity();
                HandleMovement(moveDirection, _hungerController.CurrentMoveSpeed);
                break;
            case PlayerMoveState.Fly:
                ApplyFlying();
                HandleMovement(moveDirection, 8f);
                break;
        }

        HandleRotation(moveDirection);
    }

    private void ToggleAutoMove(bool isPressed)
    {
        _autoMove = !_autoMove;
    }

    /// <summary>
    /// Run 상태이고, 스페이스바 입력 시 남은 점프 횟수가 있으면 위쪽 속도를 부여해 점프시킨다
    /// (최대 _maxJumpCount회, 2단 점프). Fly 상태의 입력은 무시해 _velocity/_jumpCount가 오염되지 않게 한다
    /// </summary>
    private void HandleJumpInput(bool isPressed)
    {
        if (_moveState == PlayerMoveState.Fly)
        {
            return;
        }

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
    private bool IsGrounded()
    {
        _sphereHit = Physics.SphereCast
        (transform.position, groundCheckRadius, Vector3.down,
            out _, groundCheckDistance, _groundLayer);

        return _sphereHit;
    }


    /// <summary>
    /// 주어진 방향으로 CharacterController를 이용해 이동시킨다. 이동 속도는 HungerController가 배고픔 비율로부터
    /// 계산한 값을 그대로 사용한다 (배고픔이 낮을수록 느려지고, 0이어도 최저 속도로 계속 이동한다)
    /// </summary>
    private void HandleMovement(Vector3 moveDirection, float moveSpeed)
    {
        _characterController.Move(moveDirection * (moveSpeed * Time.deltaTime));
    }

    /// <summary>
    /// 이동 중일 때 시각적 모델(_visualTransform)만 이동 방향을 바라보도록 회전시킨다.
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

    /// <summary>
    /// 비행 중 매 프레임 호출되는 상승 처리. 중력 대신 LeafFlightData.AscendSpeed로 곧장 위로 이동시키고,
    /// 제한 시간을 감소시켜 다 되면 착지 여부와 무관하게 Run 상태로 되돌린다
    /// </summary>
    private void ApplyFlying()
    {
        _velocity.y = _leafFlightData.AscendSpeed;
        _characterController.Move(_velocity * Time.deltaTime);

        _flightTimeRemaining -= Time.deltaTime;
        if (_flightTimeRemaining <= 0f)
        {
            SetMoveState(PlayerMoveState.Run);
        }
    }

    /// <summary>
    /// 풀잎(LeafFlightTrigger)과 접촉했을 때, Run 상태이고 그 풀잎이 사용 가능한 상태(IsAvailable)라면
    /// 비행을 시작한다. HungerController의 OnTriggerEnter와는 서로 독립적으로 동작한다(형제 컴포넌트 패턴과
    /// 동일하게, 이번엔 Player 자신이 직접 받는 차이만 있음)
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (_moveState == PlayerMoveState.Run && other.TryGetComponent(out LeafFlightTrigger leaf) && leaf.IsAvailable)
        {
            StartFlight(leaf);
        }
    }

    /// <summary>
    /// 비행 제한 시간을 초기화하고 Fly 상태로 전환한다. 사용한 풀잎을 즉시 쿨타임 상태로 전환시킨다
    /// </summary>
    private void StartFlight(LeafFlightTrigger leaf)
    {
        _flightTimeRemaining = _leafFlightData.FlightDuration;
        leaf.StartCooldown();
        SetMoveState(PlayerMoveState.Fly);
    }

    /// <summary>
    /// 이동 상태를 바꾸고 OnMoveStateChanged 이벤트를 발생시킨다. 상태를 바꾸는 모든 경로가 이 메서드 하나를 거친다
    /// </summary>
    private void SetMoveState(PlayerMoveState newState)
    {
        _moveState = newState;
        if (_moveState == PlayerMoveState.Fly)
        {
            _animator.SetBool(Fly, true);
            _leaf.SetActive(true);
        }
        else
        {
            _animator.SetBool(Fly, false);
            _leaf.SetActive(false);
        }

        OnMoveStateChanged?.Invoke(newState);
    }
}