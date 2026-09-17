# Blender → Unity FBX Export 버그 두 가지 — 문제 해결 과정 정리

**프로젝트**: MouseAndCheeseGame (Unity 6 / Blender 5.2)
**날짜**: 2026-09-16
**증상**: Blender에서 만든 소품(Apple, Cheese, Trap 등)을 FBX로 export해서 Unity로 가져오면 (1) 오브젝트 Scale이 100으로 들어오거나, (2) Rotation이 0인데도 Unity에서 뒤집혀 보이는 두 가지 문제가 반복됐다.

---

## 1. 버그 A: Scale이 100으로 들어옴

### 증상

Blender에서 오브젝트 Scale은 (1,1,1)인데, Unity로 FBX를 임포트하면 개별 오브젝트의 Transform Scale이 100(경우에 따라 500대의 lossyScale)으로 찍혔다. 이미 프로젝트에 들어와 있던 Oven/Sink/Table 등 가구 오브젝트도 전부 같은 증상이었다.

### 시도했지만 아닌 것으로 판명된 가설

| 가설 | 확인 방법 | 결과 |
|---|---|---|
| Unity FBX Importer의 Scale Factor 문제 | `get_import_settings`로 `globalScale`, `useFileScale` 확인 | `globalScale=1`, `useFileScale=true` — Unity 쪽은 기본값 그대로, 원인 아님 |
| Blender Scene의 Unit Scale 설정 문제 | `scene.unit_settings` 확인 | `system=METRIC`, `scale_length=1.0`, `length_unit=METERS` — 정상 |
| 오브젝트 자체에 Scale이 안 Apply됨 | 오브젝트의 `obj.scale` 직접 확인 | `(1.0, 1.0, 1.0)` — 이미 클린한 상태인데도 100으로 들어옴 |
| FBX Export 오퍼레이터의 마지막 사용 설정(sticky 값)이 100으로 남아있음 | `operator_properties_last("export_scene.fbx")` 확인 | `global_scale=1.0` — 정상, 원인 아님 |

네 가지 가설을 다 확인했는데도 재현이 안 돼서, 실제로 Blender→Unity 파이프라인을 처음부터 다시 재현(export → 완전히 새로 import)해보며 좁혀나갔다.

### 진짜 원인

FBX Export 다이얼로그의 **Apply Scalings** 옵션이 **"Local"**로 설정되어 있었다 (Blender가 마지막 사용값을 세션 내에서 기억하는 UI라, 대화상자를 열 때마다 이전 값이 남아있었음).

- **"FBX Units Scale"**(권장/기본값): 블렌더-미터 ↔ 유니티-미터 변환 배율을 **파일 헤더의 UnitScaleFactor 하나**로만 기록한다. Unity는 이걸 `Use File Scale`로 한 번만 깔끔하게 변환해서, 개별 오브젝트 Transform은 Scale 1로 들어온다.
- **"Local"**: 이 배율을 파일 헤더가 아니라 **오브젝트 하나하나의 Local Scale 속성에 직접 구워넣는다.** Unity는 파일에 적힌 값을 있는 그대로 정직하게 읽기 때문에, 모든 오브젝트가 개별적으로 Scale 100을 달고 들어온 것.

### 해결

Export 다이얼로그의 **Apply Scalings**을 **"FBX Units Scale"**로 변경. 이후 모든 export에서 Scale 100 문제가 사라졌다.

---

## 2. 버그 B: Rotation이 0인데도 Unity에서 뒤집힘

### 증상

Blender 오브젝트의 Rotation은 (0,0,0)이 확실한데, Unity에 임포트하면 오브젝트가 옆으로 눕거나 이상한 방향으로 뒤집혀 보였다.

### 1차 조사 — 잘못된 방향으로 흘렀던 부분

1. 오브젝트를 export → 새로 import 해서 확인해보니 Unity에서 **Rotation Euler (270.02, 0, 0)** 로 나왔다. (Blender는 Z-up, Unity는 Y-up이라 축 변환이 필요한데, `Bake Space Transform`이 꺼져있으면 이 보정 회전이 정점 데이터가 아니라 FBX 노드의 회전값으로 남는다.)
2. `Bake Space Transform`을 켜서 재-export → Rotation이 (0,0,0)으로 깔끔해짐을 확인하고, 이걸 정답으로 판단해 Export 프리셋 두 개(정적 소품용/애니메이션 캐릭터용) 모두에 적용했다.
3. **사용자 피드백**: "자식 구조 있는 오브젝트는 이상해지던데" — 확인해보니 `Bake Space Transform`은 Blender 자체에서도 경고 아이콘으로 표시하는 실험적 옵션이고, **부모-자식 계층 구조가 있는 오브젝트를 export할 때 계층을 깨트리는** 게 맞았다. 프리셋을 다시 `Bake Space Transform = Off`(기본값)로 되돌렸다.
4. 그런데 다시 "그래서 뒤집히는 건 어떻게 해결하는데"라는 질문에, **실제로 Unity 씬에 인스턴스를 놓고 스크린샷으로 눈으로 확인**해보니 — Rotation이 (270,0,0)으로 찍혀도 **화면에는 정확히 똑바로 서 있었다.** 이미 잘 작동하던 Table.fbx의 자식 메쉬들(Cube.018 등)도 전부 같은 270도를 갖고 있었지만 게임에서 아무 문제 없이 보이고 있었던 것도 재확인. 즉 이 숫자는 화면에 아무 영향 없는 정상적인 표기였다.

### 2차 조사 — 진짜 문제를 찾음

사용자가 **실제로 뒤집혀 보이는 스크린샷**(Apple 프리팹, Rotation은 (0,0,0)인데 사과가 옆으로 누워있음)을 보여주면서 재조사가 시작됐다.

- `Apple.prefab`의 구조를 까보니, FBX를 통째로 인스턴스화한 게 아니라 **FBX 안의 메쉬 서브에셋만 뽑아서 새 오브젝트에 직접 얹고 Rotation을 (0,0,0)으로 리셋한 구조**였다.
- `Bake Space Transform`이 꺼진 상태로 export되면, 축 보정 회전은 메쉬 데이터가 아니라 **오브젝트(노드)의 Rotation 값에만** 남는다. 그런데 이 오브젝트의 Rotation을 강제로 0으로 리셋해버리면, 보정 자체가 통째로 날아가서 실제로 뒤집혀 보이는 것.
- 반면 이미 정상이었던 `Leaf.prefab`은 왜 괜찮았는지 비교: Leaf는 Blender에서 `Circle.017`, `Cylinder.001` **오브젝트 2개**가 같이 export됐고, Unity는 여러 최상위 노드를 담기 위해 **자동으로 빈 루트("Leaf")를 만들어줬다.** 그 밑의 진짜 메쉬 2개는 각자 270도를 그대로 유지한 채 들어왔고, 아무도 그 회전을 건드리지 않았기 때문에 정상으로 보였던 것.
- 처음엔 이 차이를 "Blender Object Parenting 여부"로 잘못 짚었다가, 사용자가 "Leaf도 부모가 있는 게 아니라 Collection에 들어가 있는 건데"라고 정정해줘서 재확인 — `Circle.017`/`Cylinder.001` 둘 다 `parent: None`, Collection("Leaf")에만 소속되어 있었다. **Collection은 단순 정리용 그룹이라 FBX 계층에 영향을 주지 않는다.**

### 진짜 원인 (최종)

**"몇 개의 오브젝트를 한 FBX로 같이 export했는가"** 가 진짜 변수였다.

- **오브젝트 1개만 export** (Apple, Cheese, Trap처럼 각자 Collection에 혼자 들어있는 소품) → Unity가 감쌀 필요 없이 **그 오브젝트 자신이 루트가 되며, 축 보정 회전(270도)이 자기 자신에게 직접 붙는다.** 이후 프리팹을 만들면서(메쉬만 추출해서 새 오브젝트에 얹거나, Rotation을 실수로 리셋하는 식으로) 이 보정을 잃어버리기 쉬운 구조.
- **오브젝트 여러 개를 같이 export** (Leaf처럼) → Unity가 자동으로 만든 빈 루트(회전 0)가 보정을 흡수하고, 실제 메쉬들은 각자 270도를 유지한 채로 안전하게 들어온다.

### 해결

**단일 오브젝트로 export하는 소품**(Apple, Cheese, Trap)은 그 export에 한해 **Bake Space Transform을 켜서** 축 보정을 정점 데이터 자체에 구워넣었다. 이러면 계층 구조가 아예 없으므로(자식이 없음) 부작용 없이, 결과 오브젝트가 Rotation (0,0,0) 그 자체로 올바른 모양이 된다.

- 검증은 스크린샷이 아니라 **정점 좌표 비교**로 했다: Blender에서 "가장 높은(Z 최댓값) 정점"(사과 꼭지)의 로컬 좌표를 구하고, Unity 메쉬에서 "Y 최댓값 정점"을 찾아 같은 지점인지 대조 — 정확히 일치함을 확인.

### 최종 규칙 정리

| export 대상 | Bake Space Transform | 이유 |
|---|---|---|
| 계층 구조가 있는 것 (가구 조립품, 리깅된 캐릭터, Leaf처럼 여러 오브젝트를 한번에 export) | **끄기(기본값)** | 켜면 부모-자식 상대 위치가 깨짐. 자식 메쉬에 270도가 남는 건 정상이며 화면엔 영향 없음 |
| 단일 오브젝트 하나만 export하는 소품 (Apple, Cheese, Trap) | **켜기** | 자식이 없어 부작용이 없고, 프리팹을 만들 때 루트 Rotation을 리셋해도 안전해짐 |

---

## 3. 만들어둔 도구 — Export 프리셋

반복 작업을 줄이려고 Blender FBX Export 오퍼레이터 프리셋 2개를 만들어뒀다 (`%APPDATA%\Blender Foundation\Blender\5.2\scripts\presets\operator\export_scene.fbx\`):

- **정적_소품(Static_Prop)**: Apply Scalings = FBX Units Scale, Bake Space Transform = Off (기본, 계층 구조 있는 경우 대비)
- **애니메이션_캐릭터(Animated_Character)**: 위 설정 + Armature 포함, Bake Animation, All Actions 굽기, Add Leaf Bones 끔

> 단일 오브젝트 소품(Apple류)은 프리셋 적용 후 Bake Space Transform만 수동으로 한 번 더 켜야 한다 — 프리셋을 이런 케이스까지 자동 분기하진 못했다는 점은 향후 개선 여지.

---

## 4. 배운 점

1. **증상이 같아 보여도 원인 레이어가 다를 수 있다.** Scale 문제와 Rotation 문제는 둘 다 "Blender→Unity FBX export 이상 현상"이라는 같은 카테고리처럼 보였지만, 하나는 Export 다이얼로그의 Apply Scalings 옵션, 다른 하나는 Bake Space Transform + 프리팹 제작 방식의 조합이라는 전혀 다른 원인이었다.
2. **"이상해 보이는 숫자"가 항상 버그는 아니다.** Rotation (270,0,0)은 처음엔 명백한 버그처럼 보였지만, 실제로는 Blender-Unity 축 변환의 정상적인 부산물이었다. 눈에 보이는 결과(스크린샷/실제 렌더링)로 검증하지 않고 Inspector 숫자만 보고 "고치려던" 시도가 오히려 계층 구조를 깨는 부작용을 만들 뻔했다.
3. **경고 아이콘이 붙은 옵션은 이유가 있다.** `Bake Space Transform`은 Blender 스스로 위험하다고 표시해둔 기능이었고, 실제로 계층 구조를 깨는 문제를 일으켰다. "회전 숫자가 이상하다"는 증상만 보고 경고 옵션을 바로 켜기보다, 먼저 그 옵션이 왜 위험하다고 표시돼 있는지 확인했어야 했다.
4. **디버깅 중간에 나온 사용자의 정정이 결정적이었다.** "Leaf도 부모가 아니라 Collection인데?"라는 한 마디가 "Object Parenting vs Collection"이라는 잘못된 가설을 "오브젝트 개수(1개 vs 여러 개 export)"라는 진짜 원인으로 바로잡는 계기가 됐다. 스스로 세운 가설이라도 사용자의 실측 정보와 어긋나면 바로 재검증하는 게 맞다.
5. **검증 수단이 막히면 다른 방법으로 우회한다.** Scene View 스크린샷 캡처가 계속 실패(Prefab Stage 격리 뷰와 일반 Scene 뷰가 서로 다른 카메라라 엉뚱한 화면이 찍힘)했을 때, 스크린샷 대신 **정점 좌표를 직접 비교하는 수학적 검증**으로 전환해서 문제를 확실히 해결할 수 있었다.
