# WASD 플레이어 이동 기능 매뉴얼

- 작성 날짜: 2026-08-26 (26_0826)
- 작성자: builder (Claude Code)
- 참고 계획 파일: `Assets/10_ignore/01_Plan/26_0826_WASD이동_plan.md`

## 1. 기능 요약

WASD 키보드 입력을 `InputManager`가 수집하고, `Player`가 그 값을 해석해 `CharacterController`로 이동을 처리한다.

- **A / D**: 좌우 회전 (`transform.Rotate`, Y축 기준). strafe 이동이 아니다.
- **W**: 회전이 반영된 `transform.forward` 방향으로 전진 (`CharacterController.Move`).
- **S**: 입력은 감지되지만 이동에는 아무 영향도 주지 않는다 (후진 로직 없음, 음수 값은 `Mathf.Max(0f, ...)`로 차단).
- Look(마우스) 입력, Roll/Attack/Shift/LockOn 등 다른 액션은 이번 구현 범위에 포함하지 않았다.

## 2. 변경된 파일 목록

| 구분 | 파일 경로 | 내용 |
|---|---|---|
| 신규 | `Assets/02_Scripts/Input/InputManager.cs` | `PlayerInput` (Input System 자동 생성 클래스)의 `PlayerActions.Move` 액션을 구독해 원본 `Vector2` 입력을 `MoveInput` 프로퍼티로 제공. `performed`/`canceled` 콜백을 `OnEnable`/`OnDisable`에서 대칭 구독/해제. |
| 수정 | `Assets/02_Scripts/Player.cs` | 빈 스텁을 `[RequireComponent(typeof(CharacterController))]`가 붙은 클래스로 전면 교체. `_inputManager`(SerializeField), `_moveSpeed`(기본 5), `_turnSpeed`(기본 180) 필드 추가. `Update()`에서 `HandleRotation()` → `HandleMovement()` 순서로 호출. `HandleRotation()`은 `MoveInput.x`로 Y축 회전, `HandleMovement()`는 `MoveInput.y`를 `Mathf.Max(0f, ...)`로 클램프한 뒤 `transform.forward` 방향으로 `CharacterController.Move()` 호출. |

수정하지 않은 파일 (계획서에 따라 그대로 유지):
- `Assets/02_Scripts/Input/PlayerInput.cs` (Input System 자동 생성 파일, 수정 금지 대상)
- `Assets/02_Scripts/Input/PlayerInput.inputactions` (이미 WASD가 `Move` 액션 2D Vector 컴포지트로 바인딩되어 있어 수정 불필요)

## 3. Unity Editor에서 사용자가 직접 해야 할 수동 작업

씬 파일(`Assets/01_Scenes/SampleScene.unity`)은 코드로 직접 편집하지 않았다. 아래 절차를 Unity Editor에서 직접 수행해야 한다.

### 3-1. 확인된 현재 씬 상태
씬 파일을 확인한 결과, `Player` GameObject에는 이미 **`CharacterController` 컴포넌트가 부착되어 있음**을 확인했다 (계획서 작성 시점과 달리 이후 추가된 것으로 보인다). 현재 값은 다음과 같다.
- `Height`: 2
- `Radius`: 0.5
- `Center`: (0, 1, 0)
- `Slope Limit`: 45, `Step Offset`: 0.3, `Skin Width`: 0.08 (Unity 기본값)

`Player`에는 아직 `InputManager`, `Player` 스크립트 컴포넌트는 부착되어 있지 않다. 아래 3-2 절차를 진행하면 된다.

### 3-2. 절차
1. `SampleScene.unity`를 열고 `Player` GameObject를 선택한다.
2. `CharacterController`의 `Center`/`Radius`/`Height` 값이 자식 `Capsule`의 시각 표현 크기와 맞는지 확인한다. 자식 `Capsule`에는 이미 `CapsuleCollider`가 부착되어 있으므로, `CharacterController` 내장 캡슐과 `CapsuleCollider`가 충돌 판정에 이중으로 관여하지 않는지 확인이 필요하다 (문제가 되면 자식 `CapsuleCollider`를 비활성화하거나 시각 전용으로 남기는 것을 고려할 수 있으나, 이는 이번 작업 범위 밖이다).
3. `Add Component` → `InputManager` 컴포넌트를 부착한다.
4. `Add Component` → `Player` 컴포넌트를 부착한다.
5. `Player` 컴포넌트 인스펙터에서 `_inputManager` 필드에 같은 GameObject에 부착된 `InputManager` 컴포넌트를 드래그하여 연결한다. (연결하지 않으면 `HandleRotation()`/`HandleMovement()`가 null 체크로 아무 동작도 하지 않는다.)
6. `_moveSpeed`(기본값 5), `_turnSpeed`(기본값 180, 초당 회전 각도)를 기획 의도에 맞게 조정한다.
7. 변경 사항을 저장한다 (Ctrl+S로 씬 저장).

## 4. 테스트 방법

1. Unity Editor에서 `SampleScene`을 Play 모드로 실행한다.
2. `W` 키를 누르면 `Player`가 현재 바라보는 방향(`transform.forward`)으로 전진하는지 확인한다.
3. `A` / `D` 키를 누르면 좌우로 제자리 회전(Y축 기준)하는지 확인한다. strafe(옆으로 미끄러지듯 이동)가 발생하지 않아야 한다.
4. `W`와 `A`(또는 `D`)를 동시에 누르면, 회전하면서 그 방향으로 전진하는지 확인한다 (곡선 이동).
5. `S` 키만 누르면 아무 이동도 발생하지 않아야 한다 (후진 없음).
6. 모든 키를 떼면 즉시 회전/이동이 멈추는지 확인한다 (`Move.canceled` 콜백으로 `MoveInput`이 `Vector2.zero`로 초기화되는지 검증).
7. `Player` 인스펙터에서 `_inputManager` 필드가 비어 있는 상태로 Play 하면 예외 없이 아무 반응도 하지 않는지 확인한다 (null 체크 동작 확인, 선택 사항).

## 5. 구현 범위 외 사항 (참고)

- 중력/낙하 처리는 포함하지 않았다. `CharacterController.Move()`에는 수평 이동 벡터만 전달한다.
- Look(마우스) 입력, Roll/Attack/Shift/LockOn 액션은 구현하지 않았다.
- 씬 파일(.unity) 텍스트 직접 편집은 수행하지 않았다 (GUID/fileID 손상 위험 때문에 Editor UI 작업으로 안내).
