# WASD 플레이어 이동 기능 구현 계획

- 작성 날짜: 2026-08-26 (26_0826, 피드백 반영 재작성)
- 작성자: planner (Claude Code)
- 요청 범위: WASD 키보드 입력으로 `MoveInput`을 수집하고, `CharacterController` 기반으로 플레이어를 이동시키는 기능까지만 구현한다.
  - 참고 양식(`InputManager.cs`)에 포함된 Roll/Attack/Shift/LockOn/R/Q/E 등 다른 입력 기능은 계획에서 제외한다.
  - Camera/Look 입력도 이번 계획 범위에서 제외한다 (이동 기능과 무관).

## 이번 개정 사항 (사용자 피드백 반영)

기존 계획은 `Transform.Translate`로 X/Z 평면을 자유 이동(strafe)하는 방식이었다. 아래 피드백을 반영해 이동 로직을 전면 수정했다.

1. 이동 방식을 `Transform.Translate` 직접 이동에서 `CharacterController.Move()` 기반으로 변경한다.
2. A/D 입력(`MoveInput.x`)은 좌우 strafe 이동이 아니라 **좌우 회전(rotation)** 으로 동작한다.
3. W 입력(`MoveInput.y` 양수)은 **전진 이동**이다.
4. S 입력(`MoveInput.y` 음수)에 대한 **후진 이동 로직은 삭제**한다. 음수 값은 무시하고 전진 이동을 발생시키지 않는다.

---

## 1. 현재 코드베이스 조사 결과 (재확인)

### 1-1. 프로젝트 구조 (관련 부분만)
```
Assets/
├── 01_Scenes/SampleScene.unity        # "Player" GameObject 존재 (Transform만 보유, 스크립트/CharacterController 미부착)
├── 02_Scripts/
│   ├── Player.cs                      # 빈 스텁 (Start/Update만 존재, 아직 미수정 상태)
│   └── Input/
│       ├── PlayerInput.cs             # Input System Code Generator 자동 생성 파일 (수정 금지)
│       └── PlayerInput.inputactions   # Input Action Asset 원본
└── 10_ignore/
    └── 01_Plan/                       # 계획 파일 저장 위치
```

### 1-2. `CharacterController` 관련 기존 설정 확인
- `Assets` 전체에서 `CharacterController` 문자열을 검색한 결과, 계획 파일 자체를 제외하면 프로젝트 내에 기존 사용/설정 이력이 전혀 없음을 확인했다.
- `SampleScene.unity`에서 `Player` GameObject(`m_Name: Player`)를 확인한 결과 `Transform` 컴포넌트만 존재하며 `CharacterController`는 부착되어 있지 않다. 자식 `Capsule` 오브젝트에는 `CapsuleCollider`가 시각 표현용으로 부착되어 있다.
- 결론: `CharacterController` 컴포넌트는 이번 계획에서 신규로 부착해야 하며, 기존 설정을 참고할 근거가 없으므로 Unity 기본값을 기준으로 하되 자식 `Capsule`의 크기에 맞춰 Editor에서 수동 조정하도록 안내한다 (6-4절 주의 사항 참고).

### 1-3. `PlayerInput.inputactions` / `PlayerInput.cs` 분석 (변경 없음)
- 액션 맵 이름: `PlayerActions` → 생성된 코드 기준 `PlayerInput.PlayerActionsActions` 구조체, `_input.PlayerActions` 프로퍼티 사용.
- 정의된 액션은 `Move`(Vector2, Value 타입)와 `Look`(Vector2, Value 타입) 두 개뿐이며, Roll/Attack/Shift/LockOn/R/Q/E 액션은 존재하지 않는다.
- `Move` 액션은 2D Vector 컴포지트 바인딩(`up=w`, `down=s`, `left=a`, `right=d`)으로 이미 WASD가 바인딩되어 있다. → **Input Action Asset은 수정할 필요 없음.**
- `PlayerInput.cs`는 `auto-generated` 헤더가 있는 코드 생성 파일이므로 직접 수정하지 않는다 (CLAUDE.md 파일 수정 규칙).

### 1-4. `Player.cs` 분석 (변경 없음)
- `Assets/02_Scripts/Player.cs`는 `Start()`/`Update()`만 있는 빈 스텁 상태. 네임스페이스 없음, Unity 기본 템플릿 스타일. 이번 계획에서 CharacterController 기반 이동 로직으로 대체한다.

### 1-5. `InputManager.cs` 존재 여부 재확인
- `Assets/02_Scripts/Input/InputManager.cs`는 아직 생성되지 않은 상태(신규 파일 계획만 존재)임을 재확인했다. 이번 피드백은 이동 로직(소비 측)에 대한 것이므로 `InputManager.cs`의 입력 수집 로직(`MoveInput` 원본 Vector2 제공) 자체는 수정하지 않는다.

### 1-6. 기존 아키텍처 패턴 (변경 없음)
- DI 프레임워크(Zenject 등) 없음, Singleton/ServiceLocator 패턴 없음 → 프로젝트 초기 단계로 판단됨.
- 참고 양식(`InputManager.cs`)의 네이밍 규칙:
  - private 필드: `_camelCase` (언더스코어 prefix)
  - public 프로퍼티: `PascalCase { get; private set; }`
  - 이벤트/콜백 메서드: `{액션명}Performed` / `{액션명}Canceled` 형태의 private 메서드
  - `OnEnable`/`OnDisable`에서 대칭적으로 구독/해제
  - `#region 인풋 함수` 블록으로 콜백 메서드 묶음
- 네임스페이스 미사용 (기존 `Player.cs`, `PlayerInput.cs`와 동일하게 유지)

---

## 2. 신규 파일 목록

| 파일명 | 위치 | 역할 |
|---|---|---|
| `InputManager.cs` | `Assets/02_Scripts/Input/InputManager.cs` | Input System의 `PlayerActions.Move` 액션을 구독해 WASD 원본 입력을 `MoveInput`(Vector2) 프로퍼티로 외부에 제공하는 MonoBehaviour. 회전/이동 해석 없이 원본 값만 제공한다 (변경 없음). |

## 3. 수정 파일 목록

| 파일명 | 위치 | 변경 내용 |
|---|---|---|
| `Player.cs` | `Assets/02_Scripts/Player.cs` | `CharacterController`를 요구하는 컴포넌트로 변경. `InputManager.MoveInput`의 X값으로 `transform.Rotate()` 좌우 회전, Y값(0 이상)으로 `CharacterController.Move()` 전진 이동을 각각 처리. 후진(S, Y값 음수) 로직은 포함하지 않는다. |
| `SampleScene.unity` (Editor 작업) | `Assets/01_Scenes/SampleScene.unity` | 코드 수정이 아닌 **Unity Editor 수동 작업**: `Player` GameObject에 `CharacterController`, `InputManager.cs`, `Player.cs` 컴포넌트 부착 및 인스펙터 필드 연결 (4-3절 참고) |

> 주의: `Player.cs`는 CLAUDE.md 파일 수정 규칙에 따라 실제 수정 작업(builder 단계) 전 사용자 확인이 필요하다. 본 계획서는 수정 "계획"만 다루며, 실제 수정은 사용자 승인 후 진행한다.

---

## 4. 단계별 구현 내용

### 4-1. `InputManager.cs` 신규 작성 (기존 계획과 동일, 변경 없음)

`Assets/02_Scripts/Input/InputManager.cs`

```csharp
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

    #endregion

    /// <summary>
    /// Input Action을 활성화하고 Move 이벤트 콜백을 등록한다
    /// </summary>
    private void OnEnable()
    {
        _input.Enable();

        _playerActions.Move.performed += MovePerformed;
        _playerActions.Move.canceled += MoveCanceled;
    }

    /// <summary>
    /// Input Action을 비활성화하고 Move 이벤트 콜백을 해제한다
    /// </summary>
    private void OnDisable()
    {
        _input.Disable();

        _playerActions.Move.performed -= MovePerformed;
        _playerActions.Move.canceled -= MoveCanceled;
    }
}
```

> `InputManager`는 WASD 입력을 해석하지 않고 원본 `Vector2`만 제공한다. "A/D는 회전, W만 전진, S 무시"라는 정책은 소비자인 `Player.cs`에서 처리한다 (근거는 6-8절 참고).

### 4-2. `Player.cs` 수정 (CharacterController 기반으로 전면 수정)

`Assets/02_Scripts/Player.cs` (기존 빈 스텁을 아래 내용으로 대체)

```csharp
using UnityEngine;

/// <summary>
/// InputManager로부터 WASD 이동 입력을 받아 CharacterController로 좌우 회전과 전진 이동을 처리하는 클래스
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _turnSpeed = 180f;

    private CharacterController _characterController;

    /// <summary>
    /// 같은 GameObject에 부착된 CharacterController 컴포넌트를 캐싱한다
    /// </summary>
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    /// <summary>
    /// 매 프레임 회전 처리 후 이동 처리를 수행한다 (회전이 먼저 반영되어야 이동 방향에 적용됨)
    /// </summary>
    private void Update()
    {
        HandleRotation();
        HandleMovement();
    }

    /// <summary>
    /// MoveInput의 X값(A/D)으로 Y축 기준 좌우 회전을 처리한다
    /// </summary>
    private void HandleRotation()
    {
        if (_inputManager == null)
        {
            return;
        }

        float turnInput = _inputManager.MoveInput.x;
        transform.Rotate(Vector3.up, turnInput * _turnSpeed * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// MoveInput의 Y값(W)이 양수일 때만 CharacterController로 전방 이동을 처리한다. 음수(S, 후진 입력)는 무시한다
    /// </summary>
    private void HandleMovement()
    {
        if (_inputManager == null || _characterController == null)
        {
            return;
        }

        float forwardInput = Mathf.Max(0f, _inputManager.MoveInput.y);
        Vector3 moveDirection = transform.forward * forwardInput;
        _characterController.Move(moveDirection * (_moveSpeed * Time.deltaTime));
    }
}
```

### 4-3. Unity Editor 수동 작업 (코드 외 작업)

1. `SampleScene.unity`의 `Player` GameObject 선택
2. `Add Component` → `CharacterController` 컴포넌트 부착
   - `Center`/`Radius`/`Height` 값을 자식 `Capsule`의 시각 표현 크기에 맞춰 조정한다.
   - 자식 `Capsule`에 이미 `CapsuleCollider`가 부착되어 있으므로, `CharacterController`의 내장 캡슐 콜라이더와 충돌 판정이 중복/간섭될 수 있는지 확인이 필요하다 (6-4절 주의 사항 참고).
3. `Add Component` → `InputManager` 컴포넌트 부착
4. `Add Component` → `Player` 컴포넌트 부착 (`[RequireComponent(typeof(CharacterController))]`가 있으므로 `CharacterController`가 먼저 부착되어 있지 않으면 Unity가 자동으로 추가한다. 단, 자동 추가 시 2번 항목의 수동 크기 조정이 별도로 필요하다.)
5. `Player` 컴포넌트 인스펙터에서 `_inputManager` 필드에 같은 GameObject의 `InputManager` 컴포넌트를 드래그하여 연결
6. `_moveSpeed`(기본값 5), `_turnSpeed`(기본값 180, 초당 회전 각도) 값 확인 및 기획에 맞게 조정

> 이 작업은 `.unity` 씬 파일을 YAML 수준에서 직접 수정하는 대신, Unity Editor UI를 통해 수행하는 것을 권장한다. 씬 파일을 텍스트로 직접 편집하면 GUID/fileID 불일치로 씬이 손상될 위험이 있기 때문이다.

---

## 5. 데이터 흐름 설명

```
[플레이어의 W/A/D 키 입력] (S는 입력은 감지되나 이동에 반영되지 않음)
        ↓
Unity Input System (PlayerInput.inputactions)
  - "PlayerActions" 액션 맵의 "Move" 액션 (2D Vector 컴포지트: w/a/s/d)
        ↓ performed / canceled 콜백
InputManager.MovePerformed() / MoveCanceled()
  - InputManager.MoveInput (Vector2) 갱신 (원본 값 그대로, 해석 없음)
        ↓ 매 프레임 읽기
Player.Update()
  ├─ HandleRotation()
  │    - MoveInput.x → transform.Rotate(Vector3.up, x * turnSpeed * dt) → Player의 로컬 Y축 회전(향하는 방향) 갱신
  │
  └─ HandleMovement()
       - MoveInput.y → Mathf.Max(0f, y) 로 음수(후진) 값 차단
       - transform.forward(회전이 반영된 최신 정면 방향) * forwardInput 로 이동 벡터 계산
       - CharacterController.Move(moveDirection * moveSpeed * dt) 호출
        ↓
CharacterController가 충돌 검사를 수행하며 Player GameObject의 Transform 위치를 갱신 (씬 상의 실제 이동)
```

핵심 변경점: 이전에는 `MoveInput.x`/`MoveInput.y`를 각각 월드 X/Z 이동량으로 직접 변환(strafe)했으나, 이제는 `MoveInput.x`는 회전에만, `MoveInput.y`(양수만)는 회전이 반영된 `transform.forward` 방향의 전진 이동량에만 사용된다.

---

## 6. 주의 사항 및 설계 결정 근거

1. **액션 맵 이름 불일치 수정**: 참고 양식은 `_input.Player`, `PlayerInput.PlayerActions` 타입을 사용하지만, 실제 프로젝트의 `PlayerInput.inputactions`는 액션 맵 이름이 `PlayerActions`이고 생성된 타입은 `PlayerInput.PlayerActionsActions`, 접근 프로퍼티는 `_input.PlayerActions`이다. 계획은 실제 생성된 코드 기준으로 작성했다.
2. **`MoveCanceled` 콜백 추가 (참고 양식과의 의도적 차이)**: `Move`는 `Value` 타입 액션이며, WASD를 모두 떼어 값이 기본값(0,0)으로 돌아가는 시점은 `performed`가 아니라 `canceled` 페이즈로 발생한다. `performed`만 구독하면 키를 떼어도 `MoveInput`이 마지막 값에 고정되어 회전/이동이 멈추지 않는 버그가 발생한다. 따라서 `Move.canceled`도 함께 구독해 `MoveInput`을 `Vector2.zero`로 초기화하도록 설계했다.
3. **`PlayerInput.cs`는 수정하지 않음**: 자동 생성 파일(`auto-generated` 헤더 존재)이며, `PlayerInput.inputactions`에 이미 WASD 바인딩이 구성되어 있어 Input Action Asset도 수정할 필요가 없다.
4. **이동 방식을 `Transform.Translate` → `CharacterController.Move()`로 변경 (사용자 피드백 반영)**: 이전 계획은 물리 컴포넌트 없이 `Transform.Translate`로 단순 이동했으나, 사용자 피드백에 따라 `CharacterController` 기반으로 변경했다. `[RequireComponent(typeof(CharacterController))]`를 `Player.cs`에 명시해 컴포넌트 누락을 방지하고, `Awake()`에서 `GetComponent<CharacterController>()`로 캐싱한다. 씬의 `Player` GameObject에는 현재 `CharacterController`가 없으므로 4-3절의 Editor 수동 작업으로 부착이 필요하다. 자식 `Capsule`에 이미 존재하는 `CapsuleCollider`와 `CharacterController` 내장 콜라이더가 함께 충돌 판정에 관여할 수 있으므로, 실제 부착 시 두 콜라이더의 크기/역할 중복 여부를 확인할 필요가 있다(중복 충돌이 문제가 되면 자식 `CapsuleCollider`를 비활성화하거나 시각 전용 메시로만 남기는 것을 고려할 수 있으나, 이는 이번 계획 범위를 벗어나므로 별도 확인 후 진행한다).
5. **회전과 이동을 분리해 처리 (Transform.Rotate + CharacterController.Move)**: `CharacterController.Move()`는 위치 이동만 처리하며 회전 기능은 제공하지 않는다. 따라서 좌우 회전은 `transform.Rotate(Vector3.up, ...)`으로 오브젝트 자체를 Y축 기준 회전시키고, 전진 이동은 회전이 반영된 최신 `transform.forward` 방향을 `CharacterController.Move()`에 전달하는 방식으로 분리했다. `Update()`에서 `HandleRotation()`을 `HandleMovement()`보다 먼저 호출해, 같은 프레임에 회전한 방향으로 즉시 전진하도록 순서를 고정했다.
6. **A/D는 strafe가 아닌 회전으로 해석 (사용자 피드백 반영)**: 기존 계획은 `MoveInput.x`를 월드 X축 이동량으로 사용했으나, 피드백에 따라 좌우 회전(`turnInput * turnSpeed * Time.deltaTime`)으로 용도를 변경했다.
7. **S(후진) 로직 삭제, 음수 Y값 무시 (사용자 피드백 반영)**: `HandleMovement()`에서 `Mathf.Max(0f, _inputManager.MoveInput.y)`로 음수 값을 0으로 클램프해, S 입력이 감지되더라도 전진/후진 어떤 이동도 발생시키지 않는다. 후진 이동을 위한 별도 분기나 로직은 만들지 않는다.
8. **입력 수집(InputManager)과 입력 해석(Player) 책임 분리**: "A/D는 회전, W만 이동, S 무시"라는 정책은 이동을 수행하는 `Player.cs`에만 반영하고, `InputManager.MoveInput`은 원본 `Vector2` 값을 그대로 제공하도록 유지했다. 향후 다른 소비자(예: UI 표시, 다른 이동 스킴)가 원본 입력을 그대로 필요로 할 가능성을 고려해 입력 수집 계층에는 정책을 넣지 않는 관심사 분리 원칙을 따랐다.
9. **`InputManager` 참조 방식**: `Player`가 `InputManager`를 `[SerializeField]`로 인스펙터에서 직접 연결받는 방식을 사용했다. 프로젝트에 DI 컨테이너나 Singleton 패턴이 없으므로, 기존 코드베이스의 단순한 MonoBehaviour 참조 스타일을 그대로 따른 것이다.
10. **중력 미적용은 이번 범위 밖**: `CharacterController.Move()`에 수평 이동 벡터만 전달하며 별도의 중력/낙하 처리는 추가하지 않았다. 요청 범위가 WASD 회전/전진 이동에 한정되므로, 중력이 필요해지면(예: 경사면, 낙하) 별도 계획으로 `Time.deltaTime` 기반 수직 속도 누적 로직을 추가해야 한다.
11. **Camera/Look 입력 미포함**: 요청 범위가 WASD 이동으로 한정되어 있어 `Look` 액션 및 카메라 관련 로직은 이번 계획에 포함하지 않는다.
12. **씬 파일 변경은 Editor 수동 작업**: `SampleScene.unity`는 코드 파일이 아니므로 이번 계획에서 직접 텍스트 편집을 시도하지 않고, Unity Editor UI를 통한 컴포넌트 부착 절차로 안내한다.
13. **코딩 컨벤션 준수**: 접근 제어자 명시, `var` 미사용, `!` 대신 `== false`/`== null` 형태 조건문 사용, 클래스/메서드/public 프로퍼티에 한국어 `<summary>` 주석 작성 등 CLAUDE.md 규칙을 반영했다. `InputManager`의 `MovePerformed`/`MoveCanceled`는 식-바디(`=>`) 문법을 사용한 일반 인스턴스 메서드이며 `+=`로 메서드 그룹을 구독하는 방식이라, 인라인 람다식을 델리게이트에 직접 전달하는 경우가 아니므로 CLAUDE.md의 "람다식 사용 시 근거 주석 필요" 규칙 대상에는 해당하지 않는다(기존 계획과 동일하게 유지, 람다식은 사용하지 않음).
