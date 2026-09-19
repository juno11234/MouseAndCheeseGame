# 고양이 AI (Path Replay 추격) 기능 매뉴얼

- 작성 날짜: 2026-09-19 (26_0919)
- 작성자: builder (Claude Code)
- 참고 계획 파일: `Assets/Ignore/Task/Plan/26_0919_고양이AI_plan.md` (수정 이력 01~03 반영본, 4절 코드 스니펫 그대로 구현)

## 1. 기능 요약

플레이어가 지나간 경로를 기록했다가 고양이가 그 경로를 그대로 따라가며 추격하는 시스템이다. 플레이어가 통과한 곳만 고양이도 통과하므로 플레이어를 향한 최단 경로 추격(불합리한 캐치)이 생기지 않는다.

- **경로 기록**: `PlayerPathRecorder`가 매 프레임 플레이어 위치를 검사하고, 고양이와의 직선거리에 따라 정해진 프레임 간격마다 현재 위치를 `Queue<Vector3>`에 넣는다. 고양이와 가까울수록 자주(기본 1프레임), 멀수록 드물게(기본 10프레임, 20m 이상) 기록한다. 그 사이는 선형 보간이다.
- **경로 추격(`Chasing`)**: `CatController`가 큐에서 경로점을 하나씩 꺼내 `NavMeshAgent.SetDestination`으로 이동한다. 도착(`remainingDistance <= WaypointArrivalTolerance`)하면 다음 점을 꺼낸다.
- **직접 추격(`DirectChasing`)**: 플레이어와의 직선거리가 `DirectChaseDistance` 이하로 가까워지면 큐를 소비하지 않고 플레이어 현재 위치를 목적지로 직접 추격한다. 직선거리가 다시 멀어져 `Chasing`으로 돌아올 때 큐와 현재 목표를 비워, 이후에는 상태가 바뀐 뒤 기록된 경로만 따라간다.
- **갭(Gap)**: `플레이어 누적 이동 거리 - 고양이 누적 이동 거리`로 정의한다. 각자 실제로 움직인 거리를 매 프레임 누적한 값의 차이이며(초기 갭은 `InitialGap`만큼 고양이 누적값을 음수로 시작해 반영), 이동 방식(경로 추격/직접 추격/NavMeshLink 통과)과 무관하게 동일하게 계산된다.
- **러버밴딩**: `Chasing` 중 갭이 `MaxGap`을 넘으면 고양이 속도가 `RubberBandSpeed`(고정 배율 이진 방식)로 올라간다.
- **풀잎 비행 예외 처리**: `Player.OnMoveStateChanged` 이벤트만 구독해 처리한다.
  - `Fly` 진입: 그 시점의 갭을 저장하고 고양이 `Frozen`(에이전트 정지, 속도 0). 플레이어 경로점 기록도 중단되지만 플레이어 누적 이동 거리는 계속 커지므로 갭이 벌어진다.
  - `Run` 복귀: `GapCorrecting`(속도 `GapCorrectionSpeed`)으로 큐의 경로점을 따라가다가 `|Gap - 비행 시작 시 갭| <= GapCorrectionTolerance`가 되면 `Chasing` 재개.
- **갭 0 처리**: 갭이 0 이하가 되면 `Caught` 상태로 전환하고 `OnGapDepleted` 이벤트를 한 번 발생시킨다. 게임오버 UI 등 실제 처리는 이 이벤트를 구독할 별도 Feature 책임이다.
- **외부 노출**: `CatController.Gap`(프로퍼티), `OnGapChanged`(매 프레임 갱신 이벤트), `OnGapDepleted`. 갭과 배고픔을 직접 연동하는 코드는 없다(배고픔 -> `CurrentMoveSpeed` -> 플레이어 프레임당 이동거리 -> 갭이라는 기존 간접 인과만 존재).

## 2. 변경된 파일 목록

| 구분 | 파일 경로 | 내용 |
|---|---|---|
| 신규 | `Assets/02_Scripts/Cat/PlayerPathRecorder.cs` | Player와 같은 GameObject에 부착. 프레임 기반 경로점 큐(`TryDequeueNextPoint`/`ClearQueue`), 갭 계산용 누적 이동 거리(`TotalPathDistance`), `OnMoveStateChanged` 구독으로 Fly 중 기록 중단. 프레임 간격 매핑은 계획서 0-1절의 임시 선형 보간(`ResolveFrameInterval`). |
| 신규 | `Assets/02_Scripts/Cat/CatStatData.cs` | 고양이 이동 속도/도착 허용 오차/직접 추격 거리/러버밴딩(`MaxGap`, `RubberBandSpeed`)/갭 보정(속도, 허용 오차)/초기 갭을 담는 `ScriptableObject` 임시 스텁(`PlayerStatData`, `LeafFlightData` 선례와 동일). 모두 개발 테스트용 임시값. |
| 신규 | `Assets/02_Scripts/Cat/CatController.cs` | Cat GameObject에 부착(`NavMeshAgent` 필수). `CatState` enum(`Chasing`/`DirectChasing`/`Frozen`/`GapCorrecting`/`Caught`), 경로 추격/직접 추격/러버밴딩/비행 갭 중립화/갭 0 이벤트 전담. |

기존 코드 파일은 수정하지 않았다. 각 신규 파일에는 이 매뉴얼 작성 시점에 Unity가 `.meta` 파일을 자동 생성한다.

## 3. Unity Editor에서 사용자가 직접 해야 할 수동 작업

코드만으로는 동작하지 않는다. 아래를 Unity Editor에서 직접 수행해야 한다. 컴파일 오류가 없는지 먼저 Console에서 확인한다.

1. **`CatStatData` 에셋 생성**
   - Project 창에서 우클릭 -> `Create > MouseAndCheese > Cat Stat Data`로 에셋 1개를 만든다(임시 테스트용, 정식 밸런싱은 별도 Feature 담당).

2. **NavMesh 준비 (사용자가 직접 수행)**
   - 현재 씬에는 `NavMeshSurface`/`NavMeshAgent`/`NavMeshLink`가 하나도 없고 베이크된 NavMesh 데이터도 없다. `com.unity.ai.navigation`(2.0.13) 패키지는 이미 설치돼 있다.
   - 바닥/가구 등 고양이가 다닐 영역에 `NavMeshSurface`를 추가하고 베이크한다. 베이크 범위와 Agent 반경/높이/점프 가능 높이 등은 계획서에서 정하지 않았으므로 에디터에서 시각적으로 확인하며 결정한다.

3. **`NavMeshLink` 배치 (점프 지점, 레벨 디자인)**
   - 풀잎으로 올라간 가구 위에서 플레이어가 점프해 넘어갈 수 있는 구간마다, 고양이가 따라 넘을 수 있도록 `NavMeshLink`를 배치한다. 위치와 개수는 미확정(계획서 0-3절 (a)).
   - 점프 연출은 없다. `NavMeshAgent` 기본 자동 링크 통과(`autoTraverseOffMeshLink`)를 그대로 사용한다(0-3절 (b)).

4. **`Player` GameObject에 `PlayerPathRecorder` 부착**
   - `SampleScene.unity`의 `Player` GameObject에 `PlayerPathRecorder` 컴포넌트를 추가한다(`Player`와 같은 GameObject여야 `GetComponent<Player>()`가 성공한다).
   - `_catTransform`에 `Cat` 오브젝트의 Transform을 드래그해 연결한다.
   - `_nearFrameInterval`(1), `_farFrameInterval`(10), `_farDistance`(20)는 기본값을 임시로 사용한다.

5. **`Cat` GameObject에 컴포넌트 부착**
   - `SampleScene.unity`의 `Cat`(루트, `Cat.prefab` 인스턴스)에 `NavMeshAgent`와 `CatController`를 추가한다(`CatController`는 `[RequireComponent]`로 `NavMeshAgent`를 자동 추가할 수도 있다).
   - `CatController` 인스펙터에서 `_player`(Player GameObject의 `Player` 컴포넌트), `_pathRecorder`(Player GameObject의 `PlayerPathRecorder`), `_catStatData`(1번 에셋)를 연결한다.
   - `Cat`의 시작 위치(현재 배치값 `(-19.16, 10.89653, 15.04)`)가 베이크된 NavMesh 위에 있어야 한다. 벗어나 있으면 `NavMeshAgent`가 활성화되지 못하거나 시작 시점에 표면으로 스냅되면서 그 이동량이 고양이 누적 거리에 섞여 초기 갭이 어긋난다. 베이크 후 반드시 확인한다.

6. **프리팹 적용 여부(선택)**
   - `Player.prefab`/`Cat.prefab`에도 컴포넌트를 미리 부착할지는 씬 재사용 여부에 따라 결정한다. 이번 안내는 씬 인스턴스 기준이다.

필수 참조(`_catTransform`, `_player`, `_pathRecorder`, `_catStatData`)를 연결하지 않으면 `NullReferenceException`이 발생한다(의도된 fail-fast, 디버깅용 null 체크를 넣지 않는 규칙).

## 4. 동작 확인 방법

1. 위 3번까지 완료한 뒤 Play 모드에 진입한다. Cat이 NavMesh 위에서 정상적으로 시작하고 Console에 NavMeshAgent 관련 오류가 없는지 확인한다.
2. **경로 추격**: 플레이어를 전진시키면 고양이가 플레이어가 지나간 경로를 따라 뒤따라오는지 확인한다. 플레이어가 코너를 돌았을 때 고양이가 지름길로 가로지르지 않고 같은 코너를 도는지 본다.
3. **직접 추격**: 플레이어가 멈추거나 느려져 고양이가 `DirectChaseDistance`(기본 2m) 안으로 들어오면 플레이어를 직접 향하는지 확인한다. 이후 플레이어가 다시 빨라져 멀어지면 경로 추격으로 돌아오며, 예전 경로로 되돌아가지 않고 그 이후 기록된 경로만 따르는지 확인한다.
4. **러버밴딩**: 갭이 `MaxGap`(기본 15) 이상 벌어지면 고양이가 `RubberBandSpeed`(기본 8)로 빨라지는지 확인한다(`CatController.Gap`을 인스펙터 디버그 모드나 임시 로그로 확인).
5. **풀잎 비행**: 풀잎으로 비행을 시작하면 고양이가 즉시 멈추고, 비행이 끝나(Run 복귀) 후 빠르게 달려 갭이 비행 시작 시점 값으로 돌아오면 정상 속도로 복귀하는지 확인한다.
6. **갭 0**: 갭이 0 이하가 되면 고양이가 정지(`Caught`)하고 `OnGapDepleted`가 한 번만 발생하는지 확인한다. 현재는 구독자가 없어 화면상 변화는 고양이 정지뿐이다(임시 로그 구독으로 확인 가능).
7. `PlayerPathRecorder.QueuedPointCount`(디버깅용 프로퍼티)로 큐가 쌓이고 소비되는 추이를 확인할 수 있다.

## 5. 확정되지 않아 이번 구현에서 임의로 결정하지 않은 사항 (계획서 0절 그대로 유지)

- **프레임 간격 매핑**: `_nearFrameInterval`(1)/`_farFrameInterval`(10)/`_farDistance`(20m)와 선형 보간은 임시 자리 표시자다. 기준 거리를 직선거리 대신 갭으로 바꾸려면 `PlayerPathRecorder.ResolveFrameInterval` 한 곳만 수정하면 된다. 경로점 버퍼 크기/폐기 정책은 없다(무제한 `Queue`).
- **러버밴딩 수치와 곡선**: `MaxGap`(15)/`RubberBandSpeed`(8)는 임시값이며 갭 초과 시 고정 속도로 바꾸는 이진 방식이다. 점진 가속 곡선은 정해지지 않았다.
- **`NavMeshLink` 배치와 점프 연출, 도달 불가 지점 처리**: 미확정이다. 플레이어가 링크로 연결되지 않은 지점에 착지하면 별도 처리 없이 `NavMeshAgent` 기본 동작(갈 수 있는 곳까지만 이동)에 맡긴다.
- **세부 밸런싱 수치 전반**: `_moveSpeed`(4), `_waypointArrivalTolerance`(0.1), `_directChaseDistance`(2), `_gapCorrectionSpeed`(10), `_gapCorrectionTolerance`(0.1), `_initialGap`(5)는 모두 개발 테스트용 임시값이다. 경로점 간격이 플레이어 속도/프레임레이트에 따라 달라지므로 도달 허용 오차는 조정이 필요할 수 있다.

## 6. 구현 중 확인한 유의점 (코드는 계획서 그대로, 수정하지 않음)

- **`Player.cs` 전제 확인 결과**: `PlayerMoveState { Run, Fly }`와 `Player.OnMoveStateChanged` 이벤트(`MoveStateChangedHandler(PlayerMoveState newState)`)는 계획서 전제와 일치한다. `MoveState` 폴링 프로퍼티와 `FlightTimeRatio`는 실제 코드에 없다(계획서 1-1절이 이미 반영, 26_0918 풀잎비행 매뉴얼의 해당 서술은 실제 코드와 다르다).
- **비행 종료 조건**: 실제 `Player.Update()`의 Fly 분기는 `ApplyFlying()`뿐이며, 착지(`IsGrounded()`) 감지로 Run에 복귀하는 코드가 없다. 비행은 제한 시간 종료(`_flightTimeRemaining <= 0`)로만 끝난다. 계획서 5절 데이터 흐름의 "착지 -> IsGrounded() 감지 -> SetMoveState(Run)" 서술과 다르지만, 고양이 쪽은 `Run` 복귀 이벤트만 받으므로 동작에는 영향이 없다.
- **`Frozen` 중 갭 이벤트/갭 0 판정 없음**: 계획서의 `Update()`는 `Frozen`/`Caught`에서 즉시 `return`하므로, 비행 중에는 `OnGapChanged`가 발생하지 않고 갭 0 판정도 수행하지 않는다(비행 중 갭은 커지기만 하므로 실질적 문제는 없다).
- **경로점이 공중 위치일 수 있음**: 기록은 `Run` 상태의 모든 프레임에서 이뤄지므로 점프(2단 점프) 중 위치도 경로점이 된다. NavMesh 위가 아닌 점이 목적지가 되면 `SetDestination`이 근처 표면으로 스냅하거나 실패할 수 있다. 계획서에 처리 방침이 없어 넣지 않았다.
- **정지 상태 경로점 누적**: 플레이어가 멈춰도 같은 위치가 프레임 간격마다 큐에 쌓인다(계획서 6-11절, 걸러내는 처리는 요청에 없음).
- **다중 고양이 미지원**: `TryDequeueNextPoint`는 파괴적 소비 방식이라 고양이가 한 마리라는 전제다(계획서 6-7절).

## 7. 이번 범위에 포함하지 않은 것

- 갭 0 시점의 게임오버 UI/화면 전환/재시작/스코어 확정: 별도 "게임오버/리스타트" Feature가 `CatController.OnGapDepleted`를 구독해 처리한다.
- 갭/배고픔 UI: `CatController.Gap`/`OnGapChanged`만 노출했다.
- 가구 등반: 구현하지 않는다(점프 지점은 `NavMeshLink`로 대체).
- 고양이 스탯 정식 밸런싱: "데이터 테이블" Feature 책임이며 `CatStatData`는 임시 스텁이다.
- 고양이 스폰 위치 변경, `Cat.prefab` 수정, 애니메이션 연동(이동/점프 애니메이션 등): 이번 범위 밖이다.
