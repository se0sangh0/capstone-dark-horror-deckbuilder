# HANDOFF — 다음 세션 인수인계

> 마지막 갱신: 2026-05-28 / 새 세션 진입 시 이 문서를 먼저 읽고 작업 이어받기.

---

## 🟢 직전 세션 (2026-05-27 / 2026-05-28) 에서 완료한 작업

| # | 분야 | 항목 | 상세 |
|---|---|---|---|
| 1 | **데이터 / 자산** | 동료 스킬 스프라이트 Loader | `SkillData.cs`, `SkillDatabase.cs` — `skills.json.spritePath` → `Resources.Load<Sprite>` 자동 로딩 |
| 2 | **데이터 / 자산** | 적 스킬 스프라이트 Loader | `EnemySkillData.cs`, `EnemySkillDatabase.cs` — 동일 패턴 |
| 3 | **보상** | 노드 타입별 마석 차등 | `BattleManager.Phases.cs:247` — 일반 +10 / 엘리트 +20 / 보스 +30 |
| 4 | **UI 결정** | 마석 UI 노출 유지 | 코드 변경 없음. LeftPanel 표시 그대로 |
| 5 | **연출** | DamagePopup AOE cascade stagger | `DamagePopup.cs`, `BattleCardView.cs` — 좌→우 0.05s 간격 |
| 7 | **연출** | 보스 텔레포트 비가시 fade | `BattleCardSprites.cs`, `BattleManager.EnemyAction.cs` — fade out 0.3s → 0.2s hold → fade in 0.3s |
| 8 | **재화** | 영혼석 드롭 Pool/Fx (코드만) | `SoulstoneDropFx.cs`, `SoulstoneDropPool.cs`, `LeftPanelView.cs`, `BattleManager.Combat.cs` — prefab 미할당 시 폴백 (즉시 +) |

### 이전 세션 누적 (참고용)
- **카드/덱**: 드로우 flip+슬라이드, 손패 한도 초과 황금 마킹, 살아있는 카드 기준 한도, 카드 선택 솟구침+외곽선
- **사망 처리**: 침몰 트윈, 동료 사망 카드 제거, 보스 사망 시 까마귀 동반, RemoveCardsOfFellow 중복 가드
- **보스/까마귀**: 부름 cooldown 3턴, 필드 존재 시 룰렛 제외, 만료 패널티 = 각 아군 40 고정, Teleport pendingTeleport 상태머신
- **HP UI**: 슬라이더 두께·색·위치, 색상 안전망, HpScoreText 폭 확장, 적 텍스트 좌우반전 보정
- **노드/엔딩**: 엔딩 패널 자동 생성·할당, 보스 노드 클리어 시만 표시, GameStartScene 복귀
- **재화 (이전)**: 영혼석 적별 드롭 (고블린 8/약탈자 12/보스 20/까마귀 0), 시작 20, 게임 종료 시 영혼석만 리셋
- **전투 시스템**: AllEnemies 분산, 적→아군 고정, 압박 디버프 (스트레스 51~99 시 -10%), 턴 카운터 표시
- **치트**: F1 스택 999 / 영혼석·마석 +10000, F2 노드 1단계 전진
- **기타**: TMP 디폴트 폰트 NanumGothicLight SDF, 카드 클릭 잠금 (PlayerCardPlay 외 무시)

---

## ✅ 직전 세션 Play 검증 체크리스트 (미완료 — 다음 세션에서 먼저)

| 항목 | 확인 포인트 |
|---|---|
| **마석 차등** | 일반 +10 / 엘리트 +20 / 보스 +30 |
| **AOE Cascade** | 캐스터 파이어볼 시 적 카드들 데미지 팝업 좌→우 cascade (0.05s) |
| **쉴드 + HP 동시** | 디펜더 피격 시 노랑+빨강 팝업 분리 + cascade 자연스러운지 |
| **보스 텔레포트** | 까마귀 만료 후 다음 보스 턴에 fade out → 0.2s 비가시 → fade in (총 0.8s) |
| **영혼석 드롭** | 적 처치 시 영혼석 카운터 즉시 +N (시각 연출 없이 폴백 정상) |
| **하얀 구 잔여** | 화면 정중앙 잔여 X (방금 삭제) |
| **마석 UI** | LeftPanel 정상 표시 (이전과 동일) |

---

## 🚧 남은 작업 — 우선순위 표

### 🟥 작음 (즉시 가능, 30분 내)
| # | 작업 | 위치 / 메모 |
|---|---|---|
| 9 | **합성 시스템 UI 검증** | `MercenaryService.TrySynthesize` 로직은 ✅. UI 입력 (3명 선택) 흐름이 실제 동작하는지 Play 모드 검증 |
| 8a | **영혼석 드롭 prefab 1개 + Pool 연결** | 그래픽 준비되면 `SoulstoneDropPool` 에 prefab 할당. 코드 다 준비됨 |

### 🟧 중간 (1~2시간)
| # | 작업 | 위치 / 메모 |
|---|---|---|
| 6 | **튜토리얼 시스템 코드** | 기획 §15_튜토리얼 작성됨 / 코드 0. PlayerPrefs 자동 진입, [처음이신가요?] 메뉴 UI, 스킵, 4단계 가이드. **MVP 출시 전 필수** |

### 🟦 큼 (반나절~)
| # | 작업 | 위치 / 메모 |
|---|---|---|
| 10 | **교회 노드** | `NodeSystem` 의 `RoomType.Event` 또는 신규 `RoomType.Church` 추가. 스트레스 -25 회복 (지금 미구현) |
| 11 | **마석 사용처 메타 시스템** | 백로그 — 아티팩트 강화 / 패시브 해금 / 동료 스킬. 치환 비율 미결 |
| 12 | **DoT (지속 피해)** | 백로그 — 사후 도트뎀 (사망 후 효과 제외) |
| 13 | **역할별 중증 디버프** | 백로그 — 패닉 이후 전투 종료까지 유지 |

---

## 📂 핵심 위치 메모

| 종류 | 경로 |
|---|---|
| 프로젝트 루트 | `/Users/kosungmo/Desktop/Project/` |
| 기획 문서 | `/Users/kosungmo/Desktop/Project/기획/` (시스템/MVP/백로그/코어/서사) |
| 백업 | `~/Documents/backup/YYYY-MM-DD_HHMMSS_<작업명>/` |
| Unity 씬 | `Assets/Scenes/GamePlayScene.unity`, `GameStartScene.unity` |
| 핵심 prefab | `Assets/Prefab/MyObject.prefab`, `EnemyObject.prefab`, `UI/CardPrefab.prefab`, `UI/GamePlayScene_RightMainArea.prefab` |
| 핵심 코드 | `Assets/Scripts/BattleManager*.cs` (Phases/Combat/EnemyAction), `GameManager.cs`, `Mercenary/MercenaryService.cs`, `Enemy/EnemyDatabase.cs` |
| 메모리 | `/Users/kosungmo/.claude/projects/-Users-kosungmo-Desktop-Project/memory/MEMORY.md` (인덱스) |

---

## 💾 직전 세션 백업 폴더

```
~/Documents/backup/2026-05-27_000949_skill_sprite_loader/
~/Documents/backup/2026-05-27_001256_enemy_skill_sprite_loader/
~/Documents/backup/2026-05-27_001802_battle_reward_tiering/
~/Documents/backup/2026-05-27_002500_damage_popup_aoe_stagger/
~/Documents/backup/2026-05-27_003229_boss_teleport_fx/
~/Documents/backup/2026-05-27_003653_soulstone_drop_fx/
~/Documents/backup/2026-05-28_handoff_update/
```

---

## 🛠️ 새 세션 시작 시 행동 가이드

1. `MEMORY.md` 전수 확인 (특히 `feedback_workflow`, `feedback_session_safety`, `feedback_backup_before_task`, `feedback_mcp_inspector_autonomous`, `feedback_handoff_protocol`)
2. **이 HANDOFF.md 읽기** (지금 이 문서)
3. MCP 연결 확인 — `mcp__UnityMCP__manage_editor telemetry_status`
4. 사용자 첫 메시지 대기 후 위 "남은 작업" 표와 매칭. 명확하지 않으면 짧은 확인 질문 1개만
5. 작업 시작 전 백업 → 진행 → 컴파일 검증 → 보고

---

## ⚠️ 진행 중 / 미해결 이슈

- **직전 세션 Play 검증 미완** — 위 체크리스트 7개 항목 다음 세션에서 우선 확인
- **영혼석 드롭 Fx prefab 미할당** — `SoulstoneDropPool.prefab` 슬롯 비어있음. 폴백 (즉시 +) 으로 동작 중. 그래픽 준비되면 prefab 1개 만들고 슬롯 할당
- **EndingPanel 인스펙터 할당** — `BattleManager.endingPanel` 슬롯 자동 할당 완료. Unity refresh 후 정상 인식 확인 필요
- **NanumGothic 폰트** — TMP Settings 의 defaultFontAsset 변경 완료. 이미 생성된 TMP 텍스트는 인스펙터 명시 폰트가 우선이라 자동 변경 안 됨 — 일괄 변경 시 별도 작업
- **카드 드로우 애니메이션 `cardStackAnchor`** — `GameManager.cardStackAnchor` 에 `GamePlayScene_RightMainArea/Deck` 자동 할당 완료. Play 모드 동작 확인 필요
