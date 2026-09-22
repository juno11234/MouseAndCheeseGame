using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Input System을 통해 WASD 키보드 입력을 수집하고 MoveInput으로 외부에 제공하는 클래스
/// </summary>
public class InputManager : MonoBehaviour
{
    private PlayerInput _input;
    private PlayerInput.PlayerActionsActions _playerActions;

    /// <summary>
    /// Q 입력 시 발생하는 디버깅용 이벤트. 에디터에서만 발생하고 빌드에서는 발생하지 않는다
    /// </summary>
    public event Action<bool> OnAutoInput;

    /// <summary>
    /// 점프(스페이스바) 입력 시 발생하는 이벤트
    /// </summary>
    public event Action<bool> OnJumpInput;

    /// <summary>
    /// WASD 입력으로부터 얻은 원본 이동 입력 벡터 (X: 좌우 A/D, Y: 전후 W/S)
    /// </summary>
    public Vector2 MoveInput { get; private set; }

    /// <summary>
    /// Input Action Asset과 PlayerActions 액션 맵을 초기화한다
    /// </summary>
    private void Awake()
    {
        _input = new PlayerInput();
        _playerActions = _input.PlayerActions;
    }

    #region 인풋 함수

    /// <summary>
    /// Move 액션이 수행될 때 MoveInput 값을 갱신한다
    /// </summary>
    private void MovePerformed(InputAction.CallbackContext context)
        => MoveInput = context.ReadValue<Vector2>();

    /// <summary>
    /// Move 액션이 취소(방향키 전부 해제 등)될 때 MoveInput을 0으로 초기화한다
    /// </summary>
    private void MoveCanceled(InputAction.CallbackContext context)
        => MoveInput = Vector2.zero;

#if UNITY_EDITOR
    /// <summary>
    /// AutoTest(Q) 액션이 수행될 때 OnAutoInput 이벤트를 발생시킨다
    /// </summary>
    private void QPerformed(InputAction.CallbackContext context)
        => OnAutoInput?.Invoke(true);
#endif

    /// <summary>
    /// Jump 액션이 수행될 때 OnJumpInput 이벤트를 발생시킨다
    /// </summary>
    private void JumpPerformed(InputAction.CallbackContext context)
        => OnJumpInput?.Invoke(true);

    #endregion

    /// <summary>
    /// Input Action을 활성화하고 Move 이벤트 콜백을 등록한다
    /// </summary>
    private void OnEnable()
    {
        _input.Enable();

        _playerActions.Move.performed += MovePerformed;
        _playerActions.Move.canceled += MoveCanceled;

#if UNITY_EDITOR
        _playerActions.AutoTest.performed += QPerformed;
#endif
        _playerActions.Jump.performed += JumpPerformed;
    }

    /// <summary>
    /// Input Action을 비활성화하고 Move 이벤트 콜백을 해제한다
    /// </summary>
    private void OnDisable()
    {
        _input.Disable();

        _playerActions.Move.performed -= MovePerformed;
        _playerActions.Move.canceled -= MoveCanceled;

#if UNITY_EDITOR
        _playerActions.AutoTest.performed -= QPerformed;
#endif
        _playerActions.Jump.performed -= JumpPerformed;
    }
}