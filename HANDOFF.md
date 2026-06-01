# HANDOFF — 다음 세션 인수인계

> 마지막 갱신: 2026-06-01 (3차 세션)
> 직전 큰 작업: **튜토리얼 시스템 풀 플로우 + 모달 다이얼로그 + DoT 시스템 + 노드/적 시스템 다수 보강**
> 2026-06-01 변경: ① 모달 박스 1100×560 적용했다가 **사용자 판단으로 원본(900×400)으로 롤백** — 최종 원본 유지
> ② **튜토리얼 적 스폰 RoomType별 분기** — Boss→까마귀보스 / Elite→약탈자 / 그 외→고블린 (전 노드 고블린 고정 버그 해소, EnemySpawner.cs)

---

## 🚀 다음 세션 진입 시 첫 행동 (필독)

### 1) Unity Editor 살아있는지 확인
```
mcp__UnityMCP__manage_editor action=telemetry_status
```
- `no_unity_session` 응답 시: 사용자에게 Unity Editor 재실행 요청
- 정상 시: 다음 단계 진행

### 2) 튜토리얼 모달 박스 사이즈 — 원본 유지 (롤백됨)
- 2026-06-01 에 1100×560 으로 키워봤으나, **사용자가 기존 위치/크기가 더 낫다고 판단해 원본으로 롤백**.
- **현재 최종 상태 (원본):** GuidePanel **900×400** (pos 0,0 중앙) / MessageText fontSize **30** · **Center** 정렬 · anchor(0.05~0.95, 0.25~0.95)
- 롤백 방식: `~/Documents/backup/2026-06-01_tutorial_modal_size/GamePlayScene.unity.bak` 파일 복원 → refresh.
  1100×560 버전은 같은 폴더의 `GamePlayScene.unity.modal1100x560.bak` 에 보관 (되살리려면 이 파일 복원).
- ⚠️ MCP 팁: `save` 가 path 인자를 무시하고 활성 씬을 저장하므로, 특정 씬 저장 전 `set_active_scene` 으로 대상 활성화 필요.

### 3) Play 검증 권장 (선택)
- 튜토리얼 한 사이클 (자동 진입 → 5노드 → 보스 즉사 → 메뉴)
- W1 전투 시각 흐름 (dash → 모션 → impact → back dash)
- W6 Idle 5종 자동 재생
- DoT 초록 tint + 도발 + 전장의 방패

---

## 🟢 2026-05-29 세션 완료 작업 (시간순 정리)

### A. 원거리 적 스킬 dash 제거 (5종)
- `enemy_skills.json` 5개 스킬에 `isRanged: true` 추가
- 대상: 고블린 독침, 약탈자 도끼던지기, 보스 까마귀 부름/수확/순간이동
- `MotionCategoryResolver.Resolve(..., bool isRanged)` → 최우선 분기로 Ranged 반환
- `OnSkillCast` 시그니처: `Action<string, int>` → **`Action<string, int, bool>`** (EnemyData.Hp / FellowData.Hp 둘 다)
- 동료 호출부 `false` 전달, 적 호출부 `skill.isRanged` 전달

### B. 까마귀 자폭 카운트다운 UI
- `EnemyData.OnLifeTurnsChanged` 이벤트 추가
- `ProcessSummonExpiration` 에서 `currentLifeTurns--` 후 Invoke
- `BattleCardView` 가 까마귀(`summonLifeTurns > 0`) 일 때 hpScoreText 복제로 카운트다운 텍스트 동적 생성 (HP 라인 아래 주황 "자폭까지 N턴")

### C. 노드 분포 동적 가중치 (엘리트 후반, 일반 초반)
- `MapGenerator.GetCombatEliteWeights(layer)` — layer 1~7 표 lookup
  - layer 1: Combat 70 / Elite 0 / Shop 20 / Event 10
  - layer 7: Combat 30 / Elite 40 / Shop 20 / Event 10
- 인스펙터 `combatWeight/eliteWeight` 폐기 (shopWeight/eventWeight 만 유지)

### D. 노드별 적 수 랜덤화 (후반 가중)
- `FloorTierResolver.GetEnemyPool(floor)` + `RollCount(floor)` 분리
- 일반 전투 풀: **고블린만** (약탈자 제거)
- 마릿수: 1~2층=2 / 3~4층=2~3 / 5~6층=3~4 / 7~8층=3~4 (4가중 70%)
- `EnemySpawner` 가 Combat/Elite 모두 풀+RollCount 방식

### E. 행동/피격 순서 분리 (allies 진형 영구)
- 기존 버그: 미행동자가 다음 턴 `allies` 맨 앞으로 이동 → 적 FrontFirst 타겟이 매 턴 바뀜
- 해결: `BattleManager.Phases.cs:122` 의 `allies.Remove/InsertRange` 코드 제거
- `BattleManager.Combat.cs:55` `ExecuteAction(true)` 진입 시 별도 `actionOrder` 임시 리스트 빌드
  - priority (이전 미행동자) → 나머지 allies 순서
- `allies` 자체는 진형 순서 영구 유지 — FrontFirst/BackLast 일관성

### F. D1 — 어택커.role=Tanker 데이터 검증
- fellow.json / 기획서 §08 모두 일치: 어택커 = Tank role (탱커형 공격 + 전체 힐)
- 데이터 오류 아님. 기획 의도 유지. 변경 없음.

### G. W4 — 적 카드 visualScale (적별 개별)
- `EnemyDef`/`EnemyData` 에 `float visualScale` 필드
- `enemies.json`: 고블린 0.85 / 약탈자 1.0 / 보스 1.25 / 까마귀 0.6
- `DefaultSetting.cs` 적 spawn 후 `newObj.transform.localScale = ObjectPrefab.transform.localScale * vs`
- `EnemyObject.prefab` 자체 localScale 1.0 정상화 (MCP)

### H. W6 — Idle 자동 재생 (5종 키프레임)
- 5종 동료 Idle.anim 의 키프레임을 sprite 시트에서 자동 수집해 균등 배치 + loop
- Attacker/Defender/Offender/Priest: 4프레임 ~0.77s 사이클
- Caster: 처음 13프레임 → **0~4 영어 텍스트 sprite 발견 → 5~12 만 사용 (8프레임)**
  - 추가로 _8 (width 150 비정상) 제외 → 최종 7프레임
- Priest: 사용자 요청으로 1프레임(Priest_Idle_0)만 loop (정적)

### I. WS — skill_war_shield (Damage + Shield 복합) ✅ 완성 (2026-06-01 확인)
- 기획 §10: Tank 6 / AllEnemies / Damage 20 + Shield 30 (전체 실드)
- skills.json 정의 ✅ (`MixedDamageShield`, power 20 / shieldPower 30) / 디펜더 skillIds 할당 ✅
- 코드 `BattleManager.Combat.cs:461` → `ApplySkillDamage` + `ApplyMixedShield`(AllAllies 실드) ✅, `FellowData.Hp.AddShield` 구현 ✅
- 남은 것: **Play 검증만**
- 사용자 메모: "스킬 전부 늘어났다 — 1직업당 4스킬, 2개 랜덤 지정" 요청 → fellow.json 의 skillIds 가 직업별 4개로 확장 + PartyManager 가 2개 랜덤 선택 (BattleManager.cs:283 `skillPool` 변수)

### J. WC — skill_war_cry (Damage + Taunt 도발) ✅ 완성 (2026-06-01 확인)
- skills.json 정의 ✅ (`MixedDamageTaunt`, power 20 / tauntTurns 1) / 어택커 skillIds 할당 ✅
- 도발 폐루프 전부 연결: 부착 `Combat.cs:504` / 적 타겟 우선 `EnemySkillExecutor.cs:42` / 매 턴 -1·해제 `Phases.cs:152`
- 남은 것: **Play 검증만**

### K. 동료 4스킬 풀 + 2개 랜덤 선택
- `BattleManager.cs:280` 동료 스킬 배정 시 fellow.skillIds 4개 풀에서 2개 비복원 추출
- 변수명 충돌 fix: `pool` → `skillPool` (외부 스코프 line 337의 `pool` 과 충돌)

### L. 튜토리얼 시스템 (대규모)

#### L-1. 기본 골격
- `TutorialManager.cs` 신규 — 싱글톤, PlayerPrefs (`tutorial_completed` 0/1), IsTutorial 플래그
- `TutorialGuidePanel.cs` 신규 → 이후 **모달 다이얼로그 컨트롤러**로 재작성 (L-5)
- `MoveScene.cs` — `InGameSceneLoaded()`: 완료 플래그 분기. `StartTutorialAgain()` 신규 핸들러
- 메인 메뉴 `TutorialAgainButton` (MCP) — [처음이신가요?] 버튼. 완료 플래그 true 일 때만 노출

#### L-2. 파티/적 분기
- `PartyManager.InitDefaultParty` — IsTutorial 면 강제 클리어 + `GenerateTutorialParty()` (캐스터/프리스트/디펜더 3인 + null slot3)
- `EnemySpawner.SpawnIntoBattleManager` — IsTutorial 진입 시 **RoomType별 단일 적 고정** (2026-06-01 분기 완료): Boss→까마귀 보스 / Elite→약탈자 / 그 외→고블린. (이전엔 전 노드 고블린 고정 — 엘리트·보스도 고블린 뜨던 문제 해소)

#### L-3. Canvas 통합 + CanvasGroup 전환
- 기존 root level `TutorialCanvas` (별도 Canvas + CanvasScaler + GraphicRaycaster) → 기존 Canvas 자식으로 reparent
- 자식 Canvas 컴포넌트 제거 + CanvasGroup 부착 (alpha/interactable/blocksRaycasts 일괄)
- TutorialGuidePanel.cs 슬롯 `rootCanvas (GameObject)` → `canvasGroup (CanvasGroup)`

#### L-4. 재진입 버그 fix (3명 파티)
- 원인: PartyManager 가 `DontDestroyOnLoad` → InitDefaultParty 가 Start 에서 단 1회만 호출. 재진입 시 튜토리얼 파티 잔재
- 해결: `PartyManager.ForceReinitParty()` 공개 메소드 추가 → MoveScene 의 `InGameSceneLoaded()` 와 `StartTutorialAgain()` 둘 다 호출

#### L-5. TUT-EXIT / TUT-FAIL
- `BattleManager.Phases.HandleBattleEnd` 진입에 IsTutorial 분기
  - **승리 (일반 노드)**: 다음 노드로 진행 (일반 흐름과 동일)
  - **승리 (보스 노드 — 일어나지 않음, 즉사 시나리오로 보스는 못 죽임)**
  - **패배 (일반 노드)**: ForceReinitParty + 같은 씬 리로드 (재시작)
  - **패배 (보스 노드)**: `EndTutorial(true)` + 메뉴 복귀 = 튜토리얼 완료

#### L-6. 5노드 시퀀스
- `MapGenerator.GenerateTutorialMap()` — IsTutorial 면 5층 일렬 맵
- 시퀀스: `Combat → Shop → Event → Elite → Boss`
- 각 층 1노드 (분기 없음)

#### L-7. 보스 즉사 시나리오 (1턴 후 전멸)
- `BattleManager.Combat.ApplyDamageToAlly` 진입 시 IsTutorial + RoomType.Boss → damage 를 target.maxHp 로 갈음
- 사용자가 카드 1번 쓰고 보스 1턴 행동으로 어떤 보스 스킬이든 즉사

#### L-8. 모달 다이얼로그 전환 (진행형) ★ 가장 큰 변경
**기존 (제거됨)**: 하단 고정 패널 + 자동 단계 진행 (CurrentStep 0~3)
**현재**: 중앙 모달 + 9개 진행형 다이얼로그 (1회 표시 보장)

- `TutorialManager.DialogueId` enum (9개) + `_shownDialogues HashSet`
- `TutorialManager.TryShowDialogue(id)` — 1회만 표시
- `TutorialGuidePanel.Show(message)` / `Hide()` + 정적 `Instance`
- 호출 위치:
  - **0 NodeMapIntro**: `NodeSystem.Start()` 첫 노드맵
  - **1 CombatIntro**: `DispatchByRoomType` Combat case
  - **2 EnemyTurnIntro**: `BattleManager.Phases.HandleActionPhase` 적군 행동 진입
  - **3 ResultIntro**: `HandleResultProcessing` 진입
  - **4 CombatVictory**: `HandleBattleEnd` 튜토리얼 승리 분기
  - **5 ShopIntro**: `DispatchByRoomType` Shop case
  - **6 ChurchIntro**: Event case
  - **7 EliteIntro**: Elite case
  - **8 BossIntro**: Boss case
- 메시지 강화 (Shop/Church/Elite/Boss 각 버튼 안내 포함)
- 라벨: [다음] → [확인], [스킵] → [메인 메뉴로]
- UI 재배치 (MCP): 풀스크린 → 중앙 900×400 모달 (BackgroundDim 풀스크린 자식 추가) — **추후 1100×560 으로 키우는 작업 미완** (위 §다음 세션 진입 시 첫 행동 §2 참조)

### M. C3 — DoT 시스템 + 초록 tint 시각화
- `EnemySkillData.dotPower/dotTurns` 필드. 독침: 10/2
- `FellowData.Hp` 에 `dotTurnsLeft/dotPerTurn` + `OnDotChanged` 이벤트
- `EnemyAction.cs` DoT 부착 (덮어쓰기) + 사망 대상 제외
- `BattleManager.Phases.HandleResultProcessing` 에 매 턴 끝 누적 적용 + 카운터 -1
- `BattleCardSprites` 에 `SetPersistentTint(Color)` / `ClearPersistentTint()` public 메소드
- `BattleCardView` 가 `OnDotChanged` 구독해서 카드 sprite 초록 tint (DotTintColor = 0.55, 1, 0.55)

---

## ⚠️ 미완 / 검증 미수행

| # | 항목 | 처리 방법 |
|---|---|---|
| — UI | 튜토리얼 모달 박스 크기 | 1100×560 시도 → **원본(900×400) 유지로 롤백** (추가 작업 없음) |
| W1 | 전투 시각 흐름 (dash → 모션 → impact → back dash) | Play 모드 1회 전투 |
| W2/W3 | Portrait (LeftPanel) | 좌패널 표시 |
| W5 | Attack2 모션 (캐스터/어택커) | 2번 스킬 발동 |
| W6 | Idle 5종 자동 재생 | 카드 spawn 직후 |
| **신규** | **튜토리얼 한 사이클** | 자동진입 / 5노드 / 보스 즉사 / 메뉴 / [처음이신가요?] |
| **신규** | DoT 초록 tint + 도발 + 전장의 방패 | 각 스킬 발동 후 |
| ✅ WS | ~~skill_war_shield JSON/코드 effectType 처리~~ | **완성 — Play 검증만 남음** (2026-06-01 확인) |
| ✅ WC | ~~skill_war_cry 도발 어택커 할당~~ | **완성 — Play 검증만 남음** (2026-06-01 확인) |

---

## 🚧 남은 작업 — 우선순위 표

### 🟥 작음 (즉시 가능, 30분 내)
| # | 작업 | 위치 / 메모 |
|---|---|---|
| ~~TUT-HL~~ | ✅ 튜토리얼 하이라이트 오버레이 (기획 §4-2) | **2026-06-01 완료** — 외곽선 박스. `TutorialGuidePanel.highlightBox`+`handTarget=LeftPanel`. CombatIntro·ResultIntro→손패 강조. enemyTarget 미배선(확장 가능) |
| ~~WS-ASSIGN~~ | ✅ 디펜더 skill_war_shield 할당 + MixedDamageShield 처리 | **완료** (코드·데이터 모두 구현, Play 검증만) |
| ~~WC-ASSIGN~~ | ✅ 어택커 skill_war_cry 할당 + 도발 폐루프 | **완료** (부착·타겟·소모 전부 연결, Play 검증만) |

### 🟧 중간 (1~2시간)
| # | 작업 | 위치 / 메모 |
|---|---|---|
| 적 신규 컨트롤러 | 적도 동료 패턴으로 .controller + Idle.anim/Attack.anim | 작업자 자산 대기 |
| 까마귀 자폭 모션 | Resources/Animators/Crow/ 자산 후 dash + 폭발 | 자산 대기 |

### 🟦 큼 (반나절~)
| # | 작업 | 위치 / 메모 |
|---|---|---|
| C1 | 엘리트 적 1~2종 신규 (현재 약탈자 1종) | 적 정의 + 스킬 + sprite |
| ~~11~~ | ✅ 마석 메타 시스템 (재설계) | **2026-06-02 재설계** — 보상형 3종 폐기 → **전투 관여형 시그니처 패시브 5종 + 시그니처 스킬 해금 5종**. 마석 영구 누적. 진입: 노드맵 초록 시작노드 + 새 런 첫 노드 전 자동. 기획 §16 |
| **로그라이크 메타 루프** | ✅ 2026-06-02 — 보스클리어/전멸 공통: 예비대·파티·영혼석 초기화(마석 유지) → 패시브 해금 화면 → GamePlayScene 재로드. `BattleManager.Phases.StartNextRunLoop` (Play 검증 대기) |
| **동료 패시브 15종 (풀+랜덤)** | ✅ 2026-06-02 — 동료당 3개(총 15). 해금된 풀에서 **런 시작 시 무작위 1개** 배정(`FellowData.activePassiveId`, InitBattle). 전투 훅 15종 BattleManager.Combat. 카드(CardSlotView)에 활성 패시브 표시. 상점 스크롤(20항목). (Play 검증 대기) |
| **시그니처 스킬 마석 해금** | ✅ 2026-06-02 — 아이스스톰/하늘가르기/전장의방패/워크라이/기원. 미해금 시 스킬풀 제외 (BattleManager.cs) |
| ~~13~~ | ✅ 역할별 중증 디버프 | **2026-06-01 완료** — 첫 패닉 시 부착(전투종료까지): 딜러 받는피해+30%/탱커 실드-50%/서포터 광역힐→단일. FellowData.hasSevereDebuff |

### 🟪 기획자 추가 작업 대기 중
| # | 항목 | 메모 |
|---|---|---|
| ~~B1~~ | ✅ 스트레스 51~99 압박 디버프 | **2026-06-02 완료** — 스킬 −10%(`pressureSkillPenaltyPercent`) + 피격 스트레스 +10%(`pressureStressGainPercent`). 기획 §04 확정 |
| B2 | 패닉 후유증 (영구 stressResist 감소) |
| B4 | 시작 영혼석 증액 |
| 보스 패턴 추가 |
| 신규 동료 (샤먼 외) |

---

## 📂 핵심 위치 메모

| 종류 | 경로 |
|---|---|
| 프로젝트 루트 | `/Users/kosungmo/Desktop/Project/` |
| 기획 문서 | `/Users/kosungmo/Desktop/Project/기획/` |
| 데이터 JSON | `Assets/Resources/Data/` — `enemies.json` / `enemy_skills.json` / `skills.json` / `fellow.json` |
| 동료 애니메이션 | `Assets/Resources/Animators/Fellows/<Role>/` — Attacker / Caster / Defender / Offender / Priest |
| 적 sprite | `Assets/Resources/Characters/test_enemy_goblin.png` (적 컨트롤러 미생성) |
| 백업 | `~/Documents/backup/YYYY-MM-DD_<작업명>/` |
| Unity 씬 | `Assets/Scenes/GamePlayScene.unity`, `GameStartScene.unity` |
| 전투 카드 prefab | `Assets/Prefab/MyObject.prefab`, `Assets/Prefab/EnemyObject.prefab` |
| **튜토리얼 코드** | `Assets/Scripts/Tutorial/TutorialManager.cs`, `TutorialGuidePanel.cs` |
| **튜토리얼 UI** | GamePlayScene 의 `Canvas/TutorialCanvas` (CanvasGroup + 중앙 모달) |
| **튜토리얼 메뉴 버튼** | GameStartScene 의 `Canvas/Panel/TutorialAgainButton` |
| 핵심 코드 | `Assets/Scripts/BattleManager*.cs`, `Church/*`, `Log/*`, `Mercenary/*`, `Node/NodeSystem.cs`, `PartyManager.cs`, `Currency/*`, `Fellow/*`, `Enemy/*`, `UI/BattleCardSprites.cs`, `UI/BattleCardView.cs` |
| 메모리 | `/Users/kosungmo/.claude/projects/-Users-kosungmo-Desktop-Project/memory/MEMORY.md` |

---

## 💾 2026-05-29 세션 백업 폴더 (시간순, 2차 세션)

```
~/Documents/backup/2026-05-29_ranged_and_crow_countdown/  (원거리 dash + 까마귀 카운트다운)
~/Documents/backup/2026-05-29_node_distribution_and_count/ (노드 분포 + 적 수)
~/Documents/backup/2026-05-29_action_order_split/        (행동/피격 분리)
~/Documents/backup/2026-05-29_enemy_visual_scale/        (적 visualScale)
~/Documents/backup/2026-05-29_idle_animation/            (Fellows Idle.anim 전체)
~/Documents/backup/2026-05-29_tutorial/                  (튜토리얼 1차)
~/Documents/backup/2026-05-29_tutorial_exit_fail/        (TUT-EXIT/FAIL)
~/Documents/backup/2026-05-29_dot_system/                (DoT + 초록 tint)
~/Documents/backup/2026-05-29_tutorial_full_flow/        (5노드 시퀀스 + 보스 즉사 + 모달 전환)
~/Documents/backup/2026-05-29_handoff_update_session2/   (2차 세션 HANDOFF 갱신 직전)
~/Documents/backup/2026-06-01_tutorial_modal_size/       (3차 세션 — 모달 롤백 + EnemySpawner/NodeSystem deprecated fix)
~/Documents/backup/2026-06-01_severe_debuff/             (#13 역할별 중증 디버프 — FellowData/BattleManager*)
~/Documents/backup/2026-06-01_tut_highlight/             (TUT-HL — TutorialGuidePanel/TutorialManager)
~/Documents/backup/2026-06-01_meta_passive/              (#11 마석 메타 — Phases/Combat/SettingPopup)
```

---

## 🛠️ 새 세션 시작 시 행동 가이드

1. **MEMORY.md 전수 확인** (~/.claude/.../memory/)
2. **이 HANDOFF.md 정독**
3. **MCP 연결 확인** — `mcp__UnityMCP__manage_editor action=telemetry_status`
4. **컴파일 + 콘솔 점검** — `read_console` 으로 에러/워닝 0건 확인
5. ~~미완 작업 자동 적용~~ — ✅ 모달 박스 사이즈 조정은 2026-06-01 완료 (남은 미완 없음)
6. **Play 모드 검증 권장** — 튜토리얼 한 사이클 + W1~W6 + 모달 박스 육안 확인
7. **사용자 첫 메시지 대기**

---

## ⚠️ 진행 중 / 미해결 이슈

### 튜토리얼 시스템 (2026-05-29 풀 구현)
- **모달 박스 사이즈** — 1100×560 시도 후 사용자 판단으로 **원본(900×400, fontSize 30, Center) 롤백**. 최종 원본 유지
- **사용자 의도 변경 흔적**: 처음엔 자동 진행 (CurrentStep 0~3), 후반에 진행형 모달로 재설계. 기존 OnStepAdvanced/TryAdvanceTo 코드는 `TutorialManager.cs` 에 호환용으로 남아있지만 사용처는 없음 (정리 가능)
- **하이라이트 오버레이** 백로그 (모달 메시지로만 안내 중)

### DoT 시스템 (2026-05-29 신규)
- 적 → 아군만 — 동료 → 적 DoT 는 백로그
- 스택 X (덮어쓰기) — 추후 정책 결정 시 변경 가능
- 사망 대상 제외 (기획 §02-2 명세)

### 모션 시스템
- Idle.anim: 5종 모두 sprite 시트 슬라이싱 적용 완료. Caster 만 7프레임 (영어 텍스트 + 비정상 wide 프레임 제외)
- Priest Idle = 1프레임 정적 (사용자 결정)
- Attack2 자산: Defender/Offender/Priest 미생성. Attack 폴백 정상 동작
- 적 컨트롤러 미생성

### 정책 의문점
- ~~마석↔영혼석 변환~~ — ✅ 2026-06-01 해소: 변환 폐기, 마석은 영구 메타 재화로 전환 (기획 §16)
- `skill_war_shield` / `skill_war_cry` — ✅ 2026-06-01 코드·데이터 완성 확인 (Play 검증만 남음)

---

## 🔗 기획서 ↔ 코드 동기화 상태

| 기획서 항목 | 코드 상태 |
|---|---|
| 보스 maxHp 700 / 약탈자 250 / 약탈자 스킬 2종 | ✅ |
| 보스 수확 스킬 (HP≤50% 1회) | ✅ |
| ~~마석↔영혼석 10:1 (런 종료 자동 변환)~~ | ⛔ 폐기 → 마석 영구 메타 재화 (기획 §16, 2026-06-01) |
| **마석 메타 성장** (영구 누적 + 패시브 3종 해금 + 마석상점) | ✅ 2026-06-01 (Play 검증 대기) |
| **역할별 중증 디버프** (패닉 시 부착) | ✅ 2026-06-01 (Play 검증 대기) |
| **튜토리얼 하이라이트** (외곽선 박스) | ✅ 2026-06-01 (Play 검증 대기) |
| 교회 부활 100% | ✅ |
| 동료 스킬 추가 8종 → 4종 풀 × 5직업 = 20종 | ✅ (war_shield/war_cry 코드 처리도 완성) |
| 동료 5종 애니메이션 (Attacker/Caster/Defender/Offender/Priest) | ✅ Idle 자동 재생 적용 |
| 모션·데미지 동기화 (impactDelay 1.25s) | ✅ |
| **원거리 스킬 dash 제거** (isRanged 플래그) | ✅ |
| **동료 스킬별 원/근거리 포지션** | ✅ 2026-06-02 — skills.json `isRanged` per-skill. 원거리: 매직미사일/파이어볼/아이스스톰/심판. 나머지 Damage 근거리. `UseSkill`이 skill.isRanged 전달, Resolver 캐스터 자동규칙 제거(데이터 기반) |
| **해상도 대응 UI** | ✅ 2026-06-02 — 두 Canvas ScaleWithScreenSize 1920×1080 **match 0.5 통일**(GamePlayScene 0→0.5). 최상위 패널 앵커 감사: LeftPanel 좌측edge·우측영역 stretch·팝업 풀스크린 모두 양호 |
| **압박 디버프 / 투혼 이중적용 / 패시브명 자동축소** | ✅ 2026-06-02 — 압박 스킬-10%·피격+10% / 투혼 분담분 재적용 제거 / 카드 Job 폰트 autoSize(20~11) |
| **노드 분포 동적 가중치** (엘리트 후반) | ✅ |
| **노드별 적 수 랜덤화** | ✅ |
| **행동/피격 순서 분리** (allies 진형 영구) | ✅ |
| **적 visualScale 개별** | ✅ |
| **DoT 시스템** (고블린 독침 10×2턴) | ✅ + 초록 tint |
| **튜토리얼 시스템** (모달 + 5노드 + 보스 즉사) | ✅ (UI 사이즈 원본 900×400 유지) |
| 까마귀 자폭 카운트다운 UI | ✅ |
| 시작 영혼석 / 보스 추가 패턴 / 신규 동료 | ⏳ 기획자 |
| 적 애니메이션 | ⏳ 작업자 |

---

## 📌 마지막 변경 (분 단위 시간 순, 2026-05-29 2차 세션)

1. 원거리 적 스킬 5종 dash 제거 (`isRanged` 플래그)
2. 까마귀 자폭 카운트다운 UI (`OnLifeTurnsChanged`)
3. 노드 분포 동적 가중치 (`GetCombatEliteWeights`)
4. 노드별 적 수 랜덤화 (`FloorTierResolver.RollCount`)
5. 행동/피격 순서 분리 (`_carryoverOrderList` priority 큐)
6. 적 visualScale 개별 (4종 enemies.json 값 다름)
7. Idle 5종 자동 재생 (sprite 시트 자동 키프레임)
8. Caster Idle 슬라이싱 (영어 텍스트 sprite 5개 제외 → 7프레임)
9. Priest Idle 1프레임 고정 (사용자 결정)
10. 동료 4스킬 풀 + 2개 랜덤 (`skillPool` 변수)
11. 튜토리얼 시스템 골격 (TutorialManager + GuidePanel + PartyManager/EnemySpawner 분기 + MoveScene 진입 + 메뉴 버튼)
12. Canvas 통합 + CanvasGroup 전환 (root → Canvas 자식)
13. 파티 3명 재진입 버그 fix (`ForceReinitParty`)
14. TUT-EXIT (보스 패배 = 완료) / TUT-FAIL (일반 노드 패배 = 재시작)
15. 5노드 시퀀스 (Combat→Shop→Event→Elite→Boss)
16. 보스 즉사 시나리오 (ApplyDamageToAlly 분기)
17. **모달 다이얼로그 전환** (9개 진행형, hashSet 1회 표시)
18. 다이얼로그 메시지 강화 (Shop/Church/Elite/Boss 각 버튼 안내)
19. DoT 시스템 + 초록 tint (`OnDotChanged` + `SetPersistentTint`)
20. (미완) 모달 박스 1100×560 사이즈 조정 — 다음 세션
21. HANDOFF.md 갱신 (현재 문서)
