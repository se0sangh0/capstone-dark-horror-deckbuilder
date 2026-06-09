# HANDOFF — 다음 세션 인수인계

> 마지막 갱신: **2026-06-05** (6차 세션 — 기획자 피드백 15항목 **전부 완료**)
> 6차 세션 요약: 버그 4건(#4 로그스크롤·#10 노드클릭가드·#12 사망재빌드·#13 패배엔딩) + 밸런스 2건(#11 역할별 스킬우선순위·#14 해금 직업당1) + UI 7건(#1 한글·#3 카드 스트레스바·#5 dim·#6 초상화·#2·7 스킬툴팁·#8 한기 팔레트·#9 모집비용 30 통일). 상세는 아래 §6차 세션 완료.
> 직전 큰 작업(2026-06-02~06-05): UI 전반 다크 서사톤 통일, 좌측 패널 접기 기능, 파티편집 양방향 스왑, 용병소/교회 통일 등 — 상세는 아래 §직전 세션 완료(2026-06-02~05)
> (이전) 2026-06-01: 튜토리얼 풀 플로우 + 모달 다이얼로그 + DoT + 노드/적 보강. 모달 박스 원본(900×400) 유지 롤백.

---

## 🎯 NEXT — 2차 피드백 12항목 (2026-06-05) ★현재 진행

> 사용자 2차 버그/개선 리스트. **#7만 이번에 수정 완료**, 나머지는 백로그.

| # | 항목 | 상태 / 메모 |
|---|---|---|
| 1 | 파티 편집 오타 | ⬜ 오타 위치 찾아 수정 (어느 텍스트인지 확인 필요) |
| 2 | 좌패널 스킬 팝업이 "지점"이 아니라 "영역"으로 떠야 함 | ✅ **수정 완료** — CardSlotView가 트리거를 스킬 항목 컨테이너(Skill1/Skill2)에 부착, SkillTooltipTrigger.Ensure가 투명 raycast Image를 보장 → 칸 전체에서 호버. |
| 3 | 노드 화면 포커싱이 현재 노드 단계(라인)로 자동 스크롤돼야 함 | ✅ **수정 완료** — NodeSystem.UpdateNodeStates 끝에 FocusCurrentRow() 추가. NodeDisplay ScrollRect를 현재 행이 viewport 중앙에 오도록 스크롤(끝단 클램프). 스크롤뷰 아니면 무시. |
| 4 | 파티편집 후 전투 진입 시 초기 정렬 안 됨 → 턴 종료 눌러야 재정렬 | ✅ **수정 완료** — DefaultSetting.SpawnObject가 스폰 직후 아군 battleSlotIndex를 allies 순서로 확정한 뒤 RelayoutCards(instant). GetActiveFellows 스탬프 타이밍/빈슬롯으로 초기 relayout이 스킵되던 문제 해소. |
| 5 | 이벤트 노드 색상 제거(랜덤이라) + **시퀀스 고정**: 시작>전투>용병소>전투>화톳불>보스 | ⬜ MapGenerator 일반 맵 재구성(고정 시퀀스) + 노드 색상 제거 |
| 6 | 전투 중 아군 사망했는데 손패 안 줄어듦 | 🟡 **부분 수정** — 사망 1회 처리(deathHandled)로 RemoveCardsOfFellow 중복 호출 정리, ProcessPendingDiscard에 인게임 로그 추가. 체인은 정상이나 "랜덤 파괴 아예 안 됨"은 런타임 repro 필요(드로우 리필/marked 비어있음 가능성). 사양: 사망 동료 카드가 손패에 없으면 다음 턴 랜덤 1장 파괴. |
| 7 | 미행동 보상 로그/애니메이션 없음 + 스택 인원보다 적게 들어감 | ✅ **수정 완료** (아래 §2차-#7) |
| 8 | 게임 내부 텍스트 특수문자 금지 | ✅ **수정 완료** — enemy_skills.json 설명 8개 재작성(개발메모 (기획 §…)·TryGetForcedSkill·weight 0 제거 + §/≤/·/→/— 치환), MetaPassive 해금설명 "— 스킬 풀에 추가"→"(스킬 풀에 추가)", 튜토리얼 대사 "[X] — Y"→"[X]: Y"·"직업·성급"→"직업, 성급". 데이터/씬/프리팹 표시텍스트 장식 특수문자 0 확인. (정상 부호 +·-·/·%·()·[]·: 및 CollapseTab ◀▶ 화살표는 유지) |
| 9 | 아군 사망 시 스트레스 반복 상승 → 사망당 1회만 | ✅ **수정 완료** — FellowData.deathHandled 플래그 추가, ProcessDeathAndStress가 새 사망만 처리(매 턴 재적용 방지), InitBattle 리셋. |
| 10 | 프리스트 스킬 시 옷 색상 바뀜 | ⬜ BattleCardSprites 색 복원/틴트 점검 — 다만 Priest_Attack 스프라이트 아트가 다른 색일 가능성(아트 이슈). repro 필요. |
| 11 | 패시브 해금이 마석상점 아니라 **영혼석 기반**이어야 함 | ✅ **수정 완료** — MetaPassiveManager.TryUnlock이 SoulstoneManager.Amount/Use 사용(영혼석). ※스킬 해금도 동일하게 영혼석. 마석은 현재 미사용 — 의도면 OK, 아니면 알려주세요. |
| 12 | 좌패널 파티창 텍스트 밑 잘림 + 특수문자 제거 | ✅ **수정 완료** — 원인: 스킬 이름 폰트 24 > 스킬 칸 30px(밑 잘림). CardSlotView가 스킬이름/이름/성향 텍스트에 오토사이즈(ApplyAutoFit) 적용해 칸에 맞춰 축소. 특수문자는 #8에서 정리됨(카드 표시 텍스트 깨끗). |

### §2차-#7 미행동 보상 (2026-06-05 수정 완료)
- **원인 1(로그 없음)**: 미행동 보상이 `Debug.Log`(에디터 콘솔)만 하고 `GameLog.Event`(인게임 로그)가 없었음 → 로그 팝업·연출에 안 뜸.
- **원인 2(스택 인원보다 적음)**: `skills.Count==0`(보유 스킬 0)인 동료는 carryover 없이 조용히 `continue` → 미행동 3명인데 스택 +2처럼 1명 누락. (스택 상한 cap은 없음 — `RoleCostBase.Add` clamp만.)
- **수정**(`BattleManager.Combat.cs` ExecuteAction): 스킬 부족/스킬 없음 **모두 미행동 보상(+1)으로 통일** + 미행동 시 `GameLog.Event("…행동하지 않아 다음 턴 스택 +1")` 추가. carryover는 여전히 역할(StackType)별 누적(기획 "해당 스택 +1").
- ※후속: "보유 스킬 없음" 경고가 실제로 찍히면 스킬 배정(PickSkillsFromPool) 쪽 별도 점검 필요. 미행동 "애니메이션"은 재배치 카드 이동으로 일부 표현되나 전용 연출은 백로그.
- 백업: `~/Documents/backup/2026-06-05_noaction_log_fix/`.

---

## ✅ 6차 세션 완료 — 기획자 피드백 15항목 (2026-06-05)

> 백업: `~/Documents/backup/2026-06-05_planner_feedback_15/` (Scripts 전체 + 두 씬 + 프리팹 + Data).
> 컴파일 0에러, Play 런타임 0에러 검증. 좌측패널 한글·팔레트 스크린샷 육안 확인.

| # | 결과 |
|---|---|
| 4 | ✅ 로그 스크롤 — `Canvas/LogPopup/.../Content` VerticalLayoutGroup `childControlHeight=false`가 원인. `true`+`childForceExpandHeight=false`로 수정(텍스트만큼 Content 성장). 씬 인스턴스 오버라이드. |
| 10 | ✅ 노드 클릭 가드 — `NodeSystem.OnNodeClicked` 진입에 `IsAnyBlockingPanelOpen()`(PanelBase alpha>0.5) 가드. 열린 팝업 위 클릭 무시. |
| 12 | ✅ 사망 즉시 파티 재빌드 — `LeftPanelView`가 멤버별 `OnDied` 구독→`Refresh()`, 그리고 Refresh가 `!isDead` 필터로 즉시 압축. |
| 13 | ✅ 패배 엔딩 텍스트 — `BattleManager.Phases.ShowEndingPanel(string)`로 변경. 승리="보스 처치\n\n엔딩" / 패배="전원 전멸…\n\n패배"(`EndingPanel/EndingText`). |
| 11 | ✅ 역할별 스킬 우선순위 — `BattleManager.Combat.SelectSkillByRole()`. 서포터=아군 HP<60% 시 Heal / 탱커=Shield / 딜러=Damage 우선, 없으면 코스트 최고 폴백. effectType.Contains 판정. |
| 14 | ✅ 해금 직업당 1개 — `MetaPassiveManager`에서 fireball/moonlight_slash/battle_stance/indomitable/starlight 5종 게이트 제거(상수·All·_skillUnlockKey). 기본3/해금1. 랜덤 다양성 복원. |
| 1 | ✅ 좌측 메뉴 한글 — `LeftPanel.prefab`: 재화/파티/덱/스트레스(헤더), 설정/로그(버튼). 파티편집은 기존 한글. |
| 3 | ✅ 카드 스트레스 바 — `CardSlotView.stressSlider` 추가(OnStressChanged 구독, 안정/압박/패닉 색). 4개 카드 `Right_Area`의 HP `Bar_Line` 복제→`Stress_Line`(HP 아래)·점수 제거·maxValue 100·각 CardSlotView 연결. ※좌패널 별도 Stress 섹션(stressEntries)은 잔존(중복) — 추후 제거 검토. |
| 5 | ✅ 파티편집 dim — `PartyEditPanel.prefab`에 이미 `BackgroundDim`(풀스크린 활성) 존재. α 0.51→0.6. (옛 `Background`(비활성)는 잔재.) |
| 6 | ✅ 카드 초상화 — `FellowCardView`가 roleBadgeImage에 `fellow.portrait??fellowSprite` 표시(white+preserveAspect), 없으면 역할색 폴백. |
| 2·7 | ✅ 스킬 호버 툴팁(공용) — 신규 `UI/SkillTooltip.cs`(`SkillTooltipController`+`SkillTooltipTrigger`). 씬 `Canvas/SkillTooltip`(Canvas overrideSorting 100, TooltipBox+Body). `CardSlotView`(스킬1·2 라벨, #2)·`FellowCardView`(skillsLabel 전체, #7)가 런타임 `Ensure().SetSkills()`로 부착. 명/코스트/효과/설명 표시. |
| 8 | ✅ 한기 강화 팔레트(사용자 선택) — 두 씬 배경 0.05·0.067·0.075→0.04·0.06·0.085 / GameStartScene 버튼 0.16·0.19·0.22→0.15·0.19·0.26 / `LeftPanel.prefab` 슬레이트 0.0588·0.0706·0.0941→0.043·0.062·0.105(17곳). ※메르세나리/교회 등 패널은 미적용 — 추가 통일은 백로그. |
| 9 | ✅ 모집비용 통일 30 — 기획 §14 "1성 모집비용 30" 확정값. `fellow.json` 전원 30(디펜더·어택커 40→30, 프리스트 35→30). ※README/09 표는 옛 30/40/35 잔존 — 문서 동기화 백로그. |
| 15 | ✅ 확인 완료(이전 세션) — 튜토리얼 `tutorial_completed`==0 시 자동. |

**남은 백로그(이번 세션 발견)**: ③ README/09_캐릭터시트 모집비용 표를 30으로 문서 동기화.

### 6차 세션 후속(2026-06-05, 동일 세션 추가 처리)
- ✅ **좌패널 별도 Stress 섹션 제거** — `LeftPanel.prefab`의 `Stress_Accordion` 비활성(m_IsActive 0). `LeftPanelView.cs`에서 stressEntries 필드·StressEntry 클래스·RefreshStressRows/GetStressColor/GetStressLabel·OnStressChanged 구독 전부 제거(데드코드). 스트레스는 카드 바(#3)로 일원화. 검증: Play 시 좌패널에 재화/파티/덱만 표시(스트레스 섹션 없음).
- ✅ **팔레트 메르세나리/교회 확장** — 두 패널 배경은 프리팹이 아닌 **씬 인스턴스 오버라이드**로 다크 슬레이트(0.05·0.06·0.07, α0.96)였음(grep이 α0.96을 놓쳐 초기엔 흰색으로 오판). 4개 `Background`(MercenaryOffice/Recruit/Growth + Church)를 한기 슬레이트 0.04·0.06·0.085(α0.96)로 통일(GamePlayScene 저장). ※패널 내 버튼은 슬레이트 색이 검출 안 됨(스프라이트 기반 추정) — 미적용.
- ✅ **고블린 애니메이션 연결** — 자산(`Resources/Animators/Enemies/Goblin/`: Goblin.controller + Idle/Attack/Attack2.anim, 각 0.52s, 키프레임 채워짐)은 있었으나 `enemies.json` 고블린 `animatorPath`가 **빈 문자열**이라 미연결이었음. `"Animators/Enemies/Goblin/Goblin"`로 설정. `DefaultSetting.cs`가 컨트롤러 로드→EnemyObject(Animator 보유) 주입. **런타임 검증**: Combat 진입 시 스폰 고블린 2마리 모두 `runtimeAnimatorController=Goblin` + Idle 재생(playing=True), 로드 실패 경고 0. ※raider/boss/crow는 컨트롤러 없어 animatorPath 빈 채 유지.
- 백업: `~/Documents/backup/2026-06-05_stress_palette_goblin/`.

### 6차 세션 후속 2 (2026-06-05, 사용자 추가 요청)
- ✅ **고블린 좌우 반전** — 고블린 애니메이션 아트가 기본 적 페이싱(좌향, `SetFacing` localScale.x −)과 반대로 그려져 있어 보정. 데이터 필드 `flipSprite`(EnemyDef/EnemyData/EnemyDatabase) 신설, `enemies.json` 고블린 `"flipSprite": true`. `DefaultSetting`이 `SetFacing(faceLeft: isEnemy ^ flipSprite)`. 검증: 스폰 고블린 렌더러 localScale.x=+0.85(일반 적은 −). raider/boss/crow는 flipSprite 미설정.
- ✅ **스킬 해금 2디폴트+2잠금 복원(#14 되돌림)** — 직전 #14에서 제거했던 fireball/moonlight_slash/battle_stance/indomitable/starlight 5종 해금을 `MetaPassiveManager`(상수·All·_skillUnlockKey)에 재추가. 검증: All 내 스킬해금 10개, 캐스터 풀 잠금=[fireball, ice_storm] → 직업당 기본2/해금2(마석 해금). ⚠️ 이전 #14 표 기록은 무효(사용자가 2잠금 유지를 원함).
- ✅ **공격 로직 복원(미행동→진형 변경)** — 2026-05-29의 actionOrder 분리(allies 불변) 방식을 사용자 요청으로 원복. `BattleManager.Combat.ExecuteAction`: allies 진형 순서대로 행동, 미행동자는 턴 종료 시 `allies` 맨 앞으로 이동(상대순서 유지) + `battleSlotIndex` 재할당 + `DefaultSetting.AllyLayout.RelayoutNow()`로 즉시 시각 재배치. `_carryoverOrderList`(Phases·BattleManager.cs) 제거. `DefaultSetting`에 `static AllyLayout` + `RelayoutNow()` 신설. 검증: [Support,Tank,Tank,Dealer]에서 Dealer 미행동→[Dealer,Support,Tank,Tank], slot 재할당, RelayoutNow 정상.
  - ※부수효과: 미행동자가 allies 맨 앞 → 적 FrontFirst 타겟도 그 앞열을 향함(원래 동작, 의도됨).
  - 🔧 정렬 순서 사양 보정(2026-06-05): 코어루프 명세 §동료 행동 — 복수 미행동 시 **감지(배치) 순서대로 각자 맨 앞 삽입** → 나중 미행동자가 더 앞. 예 `1-2-3-4`에서 3,4 미행동 → **`4-3-1-2`**(역순 삽입 버그를 spec대로 수정). 단일 3 미행동 → `3-1-2-4`. 검증 완료.

### 6차 세션 후속 3 (2026-06-05) — 프리스트 스케일 / 모션 검토 / 파티편집 UI
- ✅ **프리스트 스케일** — 6/5 추가 Idle 시트 `Priest_Idle_2~5.png`가 기본 PPU 100이라 renderH 2.66~2.74로 큼(원본 `Priest_Idle` PPU 129.5=2.0). 새 4시트 PPU를 2→133/3→135/4→137/5→137로 보정 → 전 프레임 renderH=2.0(다른 동료와 동일, 프레임별 편차 해소). 백업 `~/Documents/backup/2026-06-05_priest_scale/`.
- ✅ **스킬 모션 검토** — `MotionCategoryResolver`(effectType+isRanged): 검 근접→Melee(대시), 마법/신성 원거리→Ranged(제자리), 힐/실드/혼합→Stationary. 20개 스킬 전수 검토 결과 **전부 적합** — 데이터 변경 없음. (한계: 힐/실드도 Attack 애니 재생 — 전용 캐스트 에셋 필요, AoE 멜리(역류)는 단일 타겟 대시 — 백로그.)
- ✅ **파티편집 UI 개선** — ⚠️핵심: 패널이 LeftPanel·노드맵 뒤에 렌더돼 모달이 화면을 못 덮고 좌패널/노드가 비쳐 겹쳤음. 수정: `Canvas/PartyEditPanel`에 **Canvas(overrideSorting, order 80) + GraphicRaycaster** 추가 → LeftPanel(0)·노드 위로. `BackgroundDim`을 불투명 쿨 슬레이트(0.04·0.055·0.08, α0.98)로 → 배경 완전 차단. 잔재 `Background`(비활성) 삭제. `ReserveScrollView` 밝은 회색→다크 슬레이트(0.075·0.09·0.12). 풀해상도 캡처로 클린 확인(타이틀/인원/4카드/예비대/나가기/안내 정렬, 비침 0). ※저해상도 MCP 오버레이 캡처는 비침 아티팩트 있음(실제 렌더는 정상). 백업 `~/Documents/backup/2026-06-05_partyedit_ui/`.
- 백업: `~/Documents/backup/2026-06-05_flip_unlock_actionorder/`.

---

## 🗂️ (이전) 기획자 피드백 15항목 원본 표 — 참고용

> 작업 전 위치·원인 메모. 위 §6차 완료 표가 최신.
> **권장 순서**: 버그(4,10,12,13) → 밸런스(11,14) → UI(1,3,5,6,2·7,8) → 대기(9). 15는 확인 완료.

| # | 분류 | 요구 / 원인·위치 / 수정 방향 |
|---|---|---|
| 1 | UI | 좌측 메뉴 한/영 혼용 → **한글 통일**(재화/파티/덱/스트레스, 파티편집/설정/로그). 위치: `LeftPanel/.../{Money,Party,Deck,Stress}_Accordion/Button_Header/Text (TMP)`, `LeftPanel/Image/{PartyEdit,Setting,Log}/Text (TMP)`. 정적 TMP. ※일부 런타임 세팅 여부(LeftPanelView) 확인 |
| 2·7 | UI신규 | **스킬 호버 툴팁 공용 시스템**(명/코스트/효과/설명). `CardSlotView`(2) + `FellowCardView`(7) 공용. 데이터 `SkillData.description` + `SkillDatabase.GetSkill(id)`. IPointerEnter/Exit + 툴팁 패널 |
| 3 | UI | **스트레스 바를 파티 카드 HP 아래로**. `CardSlotView.cs`에 스트레스 Slider 추가 + `FellowData.OnStressChanged` 구독. 별도 `Stress_Accordion` 제거 검토 |
| 4 | 버그 | **로그 스크롤 안 됨**. `Log/LogPopup.cs`(scrollRect line 21,107). ScrollView Viewport/Content·ContentSizeFitter(Vertical)·ScrollRect.content 연결·Vertical 점검. 위치: `Canvas/LogPopup` |
| 5 | UI | **파티편집 배경 dim**. `Canvas/PartyEditPanel/Background`(현재 비활성) 활성화 + 반투명 검정 `(0,0,0,0.6)` 풀스크린 |
| 6 | UI | **파티편집 카드 아이콘 → 초상화/스프라이트**. `FellowCardView.cs`(roleBadgeImage 색상배지) → `fellow.portrait`/`fellowSprite`. (정면 초상화 에셋 없으면 스프라이트 우선) |
| 8 | UI협의 | **전체 색상톤 추가 변경 희망**(주관적). 다음 세션에 팔레트 2~3안 제시 후 협의. 현재: 배경 0.05,0.067,0.075 / 버튼 0.17,0.20,0.24 / 텍스트 0.86~0.9 |
| 9 | 기획대기 | **모집비용 통일**(현 fellow.json 30~40). **기획자 기획서 수정 예정** → 확정값 후 fellow.json+§14 반영 |
| 10 | 버그 | **팝업 열린 채 노드 클릭 시 노드 실행+팝업 잔존**. `Node/NodeSystem.cs OnNodeClicked`(line 371) 가드 없음 → 패널 열림(PanelBase alpha>0.5) 중 클릭 무시 또는 열린 패널 강제 Close. (LeftPanelToggle 게이트 로직 재사용 가능) |
| 11 | 로직 | **스킬 우선순위**. `BattleManager.Combat.cs:107~110` 무조건 `OrderByDescending(costAmount)`. → Support는 아군 HP낮으면 Heal, Tank는 Shield 우선, 딜러는 딜. `SkillData.effectType` + 아군 HP비율 활용 |
| 12 | 버그 | **파티 아코디언 열린 채 아군 사망 → 빈칸, 재오픈해야 정렬**. `LeftPanelView.cs`/`CardSlotView.cs` — `FellowData.OnDied` 구독해 사망 즉시 파티 리스트 재빌드 |
| 13 | 버그 | **패배 시 "보스 처치 엔딩" 팝업**. `BattleManager.Phases.cs:286` 패배도 `ShowEndingPanel()`(정적 "보스 처치\n\n엔딩" 텍스트, GamePlayScene). → 승리/패배 텍스트 분기(ShowEndingPanel(text)) 또는 패배 전용 패널 |
| 14 | 밸런스 | **해금 2개→축소**. 직전 세션 직업당 2개 잠금(fireball/moonlight/battle_stance/indomitable/starlight, `MetaPassiveManager`). 사용자 의도(랜덤 다양성)와 충돌 → **직업당 1개만 잠금(기본3/해금1)** 또는 잠금 제거. `_skillUnlockKey`·`All` 조정 |
| 15 | 확인✅ | **튜토리얼 조건**: PlayerPrefs `tutorial_completed`==0(미완료/키없음)일 때 [게임시작] 시 **최초 1회 자동**. `MoveScene.cs:53` `if(!IsCompleted())`. 안 뜬 건 이전 완료/스킵으로 1 저장됨 → `ResetCompletedFlag()` 또는 PlayerPrefs 삭제, 또는 [처음이신가요?] 버튼(`MoveScene.cs:40`, 완료시 노출) |

**다음 세션 시작용 복사 글**
```
[이어서 작업 — 기획자 피드백 15항목]
HANDOFF.md 의 §NEXT(기획자 피드백 15항목) 대로 작업한다.
작업 전 MEMORY.md·기획 폴더·백업 규칙 확인, 대상 파일/씬은 ~/Documents/backup/ 에 백업.
순서: 버그(4,10,12,13) → 밸런스(11,14) → UI(1,3,5,6,2·7,8) → 대기(9). 15는 확인완료.
각 항목 '위치/원인/수정' 기준으로 UnityMCP 직접 수정·검증(플레이→스크린샷). 8·9는 진행 전 사용자에게 방향/확정값 확인.
```

---

## 🟢 직전 세션 완료 (2026-06-02 ~ 06-05, 4·5차)

> 전반 **UI 서사톤(어둡고 차가움) 통일** + 좌측 패널 접기 기능군. 모두 저장·검증됨.

- **테마 통일(다크)**: 메인 메뉴(GameStartScene — 한글 라벨·임시 타이틀 "괴이탐사대"·Exit 버튼 수정), 노드맵(엘리트 호박색 구분·동선 대비), 전투 프레임(배경/좌패널/턴버튼/덱), HP 바 트랙, 스택 3컬럼(역할색 외곽선+다크박스), 손패 카드(`StackCardController` 다크), 좌측 패널(아코디언/버튼 슬레이트+밝은텍스트), 용병소 3패널(버튼16·텍스트47 일괄), 교회.
- **좌측 패널 접기**: `Assets/Scripts/UI/LeftPanelToggle.cs`(신규) + `Canvas/LeftPanel/CollapseTab`(▶ 탭, override Canvas sortingOrder 50, CanvasGroup). 접으면 ① 패널 슬라이드 숨김 ② 오른쪽 콘텐츠 offsetMin 600→0 화면채움(rightContents=NodeDisplay/RightMainArea/MercenaryRoot/RestPanel) ③ 전투 월드(InGameObjects) 좌측 이동(battleWorldShiftX). **게이트**: 용병소(Office/Recruit/Growth)·노드맵·전투에서만 가능, 그 외 PanelBase 패널 열리면 탭 숨김. `MagicStoneShopPanel.IsOpen` 추가.
- **파티편집 양방향 스왑**: `PartyEditPanel.cs` — `_selectedReserveIndex` 추가, 예비대/파티 어느쪽 먼저 선택해도 교체(`ClearSelection`/하이라이트). 동작변화: 예비대 클릭=즉시합류 → 선택후 대상클릭.
- **스킬 배정**: `SkillDatabase.PickSkillsFromPool`(풀에서 해금된 것 중 2개) + 직업당 2개 해금(`MetaPassiveManager`) ← **#14에서 축소 요청**.
- **튜토리얼 영혼석**: 진입 시 400 지급/종료 시 원복(`TutorialManager` + `BaseCurrency.SetAmount`). 백업·검증 워크플로우 확립: 플레이 진입→`NodeSystem.OnNodeClicked(0,0)`로 전투 강제→**메인카메라 직접 렌더**(월드 스프라이트)·**Scene뷰**(오버레이)·**Canvas를 임시 ScreenSpaceCamera로 바꿔 합성** 캡처.
- ⚠️ **MCP 주의**: 플레이 모드에서 프레임이 스크린샷 시점에만 진행됨 → 매프레임 Update 검증은 스크린샷으로 강제 틱 필요. / execute_code의 raw 색변경은 프리팹 인스턴스 오버라이드 직렬화 안 됨 → `EditorUtility.SetDirty`+`PrefabUtility.RecordPrefabInstancePropertyModifications` 필요.
- **백업**: `~/Documents/backup/20260602_startscene_ui/`, `~/Documents/backup/20260602_tutorial_currency_exitfix/`.

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
