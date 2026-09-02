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

    private CharacterController _characterController;

    /// <summary>
    /// 같은 GameObject에 부착된 CharacterController 컴포넌트를 캐싱한다
    /// </summary>
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    /// <summary>
    /// MoveInput의 Y값(W, 음수는 무시)으로 이동 방향을 계산해 이동과 회전을 처리한다
    /// </summary>
    private void Update()
    {
        float forwardInput = Mathf.Max(0f, _inputManager.MoveInput.y);
        Vector3 moveDirection = _cameraController.ForwardDirection * forwardInput;

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
}