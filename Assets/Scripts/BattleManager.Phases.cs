// ============================================================
// BattleManager.Phases.cs
// 전투 페이즈 핸들러 (파셜 클래스 분리 파일)
// ============================================================
//
// [이 파일이 하는 일]
//   전투의 각 단계(페이즈)를 처리하는 함수들이 담겨 있습니다.
//   BattleManager.cs 의 ExecutePhase() 에서 호출됩니다.
//
// [페이즈 흐름]
//   1. HandleDrawPhase      → 카드를 손에 뽑음
//   2. HandlePlayerCardPlay → 플레이어 입력 대기
//   3. HandleInitiativeCheck → 선공/후공 결정
//   4. HandleActionPhase    → 선공 팀 행동
//   5. HandleActionPhase    → 후공 팀 행동
//   6. HandleResultProcessing → 결과 처리 및 다음 턴 준비
//
// [이 파일에는 없는 것]
//   필드 선언, 초기화, 공개 API → BattleManager.cs
//   전투 로직 (선공 판정, 데미지, 사망 처리) → BattleManager.Combat.cs
// ============================================================

using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// partial 키워드: 이 파일이 BattleManager 클래스의 일부임을 선언
public partial class BattleManager
{
    // ===========================================================
    // 페이즈 핸들러 모음
    // ===========================================================

    // ----------------------------------------------------------
    // 1. 드로우 페이즈
    // 카드를 화면에 뽑아 슬롯에 배치합니다.
    // ----------------------------------------------------------
    private IEnumerator HandleDrawPhase()
    {
        AdvanceTurnCounter();
        Debug.Log($"--- [1] 드로우 페이즈 (턴 {CurrentTurn}) ---");
        GameManager.Instance?.StartMyTurn();
        // 튜토리얼 1단계 — 카드 드로우 (첫 드로우 페이즈에만 트리거)
        TutorialManager.Instance?.TryAdvanceTo(0);
        currentPhase = BattlePhase.PlayerCardPlay;
        yield return null;
    }

    // ----------------------------------------------------------
    // 2. 플레이어 카드 플레이 페이즈
    // 플레이어가 카드를 선택하고 턴 종료를 누를 때까지 대기합니다.
    // 스페이스바로도 턴을 종료할 수 있습니다 (테스트용).
    // ----------------------------------------------------------
    private IEnumerator HandlePlayerCardPlay()
    {
        Debug.Log("--- [2] 플레이어 카드 플레이 (입력 대기 중) ---");
        isPlayerTurnFinishing = false;

        // 플레이어가 턴 종료를 누르거나 스페이스바를 누를 때까지 대기
        yield return new WaitUntil(() =>
            isPlayerTurnFinishing ||
            (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        );

        currentPhase = BattlePhase.InitiativeCheck;
    }

    // ----------------------------------------------------------
    // 3. 선공 판정 페이즈
    // 전투 1회에 한해 선공을 결정한다. 이미 결정된 경우 유지.
    // ----------------------------------------------------------
    private void HandleInitiativeCheck()
    {
        Debug.Log("--- [3] 선공 판정 ---");
        if (!_initiativeDecided)
        {
            DecideInitiative();
            _initiativeDecided = true;
        }
        else
        {
            Debug.Log($"[선공 판정] 이미 결정됨 — {(isAllyFirstAttacker ? "아군" : "적")} 선공 유지");
        }
        currentPhase = BattlePhase.FirstAction;
    }

    // ----------------------------------------------------------
    // 4~5. 행동 페이즈
    // isAllyTurn=true 이면 아군 행동, false 이면 적군 행동
    // ----------------------------------------------------------
    private IEnumerator HandleActionPhase(bool isAllyTurn, BattlePhase nextPhase)
    {
        string faction = isAllyTurn ? "아군" : "적군";
        Debug.Log($"--- [행동] {faction} 행동 페이즈 ---");
        // 튜토리얼 모달 — 적 행동 페이즈 첫 진입 시
        if (!isAllyTurn) TutorialManager.Instance?.TryShowDialogue(TutorialManager.DialogueId.EnemyTurnIntro);
        yield return StartCoroutine(ExecuteAction(isAllyTurn));
        currentPhase = nextPhase;
    }

    // ----------------------------------------------------------
    // 6. 결과 처리 페이즈
    // 사망 처리, 스택 초기화, 전투 종료 여부 판정 (+ 튜토리얼 4단계 트리거)
    // ----------------------------------------------------------
    private IEnumerator HandleResultProcessing()
    {
        Debug.Log("--- [6] 결과 처리 ---");
        // 튜토리얼 모달 — 첫 결과 처리 진입 시 (미행동 보상 안내)
        TutorialManager.Instance?.TryShowDialogue(TutorialManager.DialogueId.ResultIntro);
        ProcessDeathAndStress();

        // 매 턴 끝: 잔여 스택은 유지, 이월 보너스(+1)만 더해줌
		if (PlayerRoleCost.Instance != null)
		{
    		foreach (var kv in _carryoverBonus)
    		{
        		PlayerRoleCost.Instance.Add(kv.Key, kv.Value);  // SET이 아니라 ADD
    		}
    		_carryoverBonus.Clear();
    		Debug.Log("[결과 처리] 미행동 보너스 반영 (스택 누적 유지)");
		}

        // 미행동자 다음 턴 행동 우선 — 기획 §코어루프 §동료 행동 (2026-05-29 갱신)
        //   "미행동 보상: 해당 스택 +1, 다음 턴 순서 우선"
        //   ★ 행동 순서만 우선, 진형(allies 리스트) 은 변경하지 않는다 — 적 FrontFirst 타겟 고정 유지.
        //   _carryoverOrderList 는 Clear 하지 않고 다음 턴 ExecuteAction(true) 가 priority 큐로 사용.
        if (_carryoverOrderList.Count > 0)
        {
            int aliveCount = _carryoverOrderList.Count(a => a != null && !a.isDead);
            Debug.Log($"[결과 처리] 미행동자 {aliveCount}명 → 다음 턴 행동 순서 우선 (진형 유지)");
        }

        // 손패 한도 초과 — 사망 후 손패에 사망 동료 카드가 없던 경우 누적된 pending count 만큼 랜덤 파괴.
        // 기획 §02 §동료 사망 처리: "사용자가 해당 턴에 사용하지 않으면 결과 처리 단계에서 N개를 랜덤 파괴한다."
        GameManager.Instance?.ProcessPendingDiscard();

        // 탈진 페널티 (기획 §02 §1) Hand Empty / §03 §탈진 — 손패 0 + 덱 0 시 스트레스 페널티)
        ApplyExhaustionPenaltyIfNeeded();

        // 적 스킬 쿨다운 -1 (까마귀 부름 등 cooldownTurns 가진 스킬용).
        foreach (var e in enemies)
        {
            if (e != null) e.TickSkillCooldowns();
        }

        // 도발 카운터 -1 — 워크라이로 걸린 도발 시간 흐름 (2026-05-29 추가)
        foreach (var e in enemies)
        {
            if (e == null || e.tauntTurnsLeft <= 0) continue;
            e.tauntTurnsLeft--;
            if (e.tauntTurnsLeft <= 0)
            {
                e.taunter = null;
                Debug.Log($"[도발 해제] {e.displayName}");
            }
        }

        // DoT 처리 — 매 턴 끝 아군 dot 누적 적용 후 카운터 -1 (기획 §11 §독침 도트)
        foreach (var a in allies)
        {
            if (a == null || a.isDead || a.dotTurnsLeft <= 0) continue;
            int dmg = a.dotPerTurn;
            ApplyDamageToAlly(a, dmg);
            a.dotTurnsLeft--;
            if (a.dotTurnsLeft <= 0)
            {
                a.dotPerTurn = 0;
                a.OnDotChanged?.Invoke(); // UI 초록 tint 해제
                Debug.Log($"[DoT 해제] {a.displayName}");
            }
        }

        // 까마귀 등 소환체 수명 카운터 -1 + 만료 처리 (기획 §11 §3 보스 까마귀)
        ProcessSummonExpiration();

        // 전투 종료 여부 판정
        if (CheckBattleEndCondition())
        {
            currentPhase = BattlePhase.BattleEnd;
            yield return StartCoroutine(HandleBattleEnd());
        }
        else
        {
            // 다음 턴까지 잠깐 대기 후 드로우 페이즈로 돌아감
            yield return new WaitForSeconds(turnTransitionDelay);
            currentPhase = BattlePhase.DrawPhase;
        }
    }

    // ----------------------------------------------------------
    // 7. 전투 종료 페이즈
    // 아군 전멸 → 게임 오버 씬으로 이동
    // 적군 전멸 → 승리 처리
    // ----------------------------------------------------------
    private IEnumerator HandleBattleEnd()
    {
        bool allEnemiesDead = enemies.Count > 0 && enemies.All(a => a.isDead);

        yield return new WaitForSeconds(gameOverDelay);
        DisplayChange.Instance.ToggleResultDisplay(allEnemiesDead);
        yield return new WaitForSeconds(gameOverDelay);

        // 튜토리얼 모드 분기 — 기획 §15 + 2026-05-29 5노드 시퀀스
        bool isTutorial = TutorialManager.Instance != null && TutorialManager.Instance.IsTutorial;
        if (isTutorial)
        {
            bool isBossRoom = NodeSystem.Current != null && NodeSystem.Current.CurrentRoomType == RoomType.Boss;

            if (allEnemiesDead)
            {
                // 일반 적 전투 승리 — 다음 노드로 진행 (일반 흐름과 동일)
                GameLog.Event("전투 승리!", LogCategory.Reward);
                Debug.Log("[BattleManager] 튜토리얼 일반 전투 승리 — 다음 노드");
                AudioManager.Instance?.PlaySfxById(SfxId.Victory);
                DisplayChange.Instance.ToggleResultDisplay(allEnemiesDead);
                DisplayChange.Instance.ToggleDisplay();
                AudioManager.Instance?.PlayBgmById(BgmId.NodeMap);
                // 첫 전투 승리 직후 — 다음 노드 클릭 안내 (1회)
                TutorialManager.Instance?.TryShowDialogue(TutorialManager.DialogueId.CombatVictory);
            }
            else if (isBossRoom)
            {
                // 보스 노드 전멸 — 튜토리얼 완료 (1턴에 전멸시킨 시나리오 그대로 진행)
                GameLog.Event("튜토리얼 완료!", LogCategory.Reward);
                Debug.Log("[BattleManager] 튜토리얼 보스 노드 전멸 — 완료 플래그 저장 후 메뉴 복귀");
                AudioManager.Instance?.PlaySfxById(SfxId.Defeat);
                yield return new WaitForSeconds(gameOverDelay);
                TutorialManager.Instance.EndTutorial(markComplete: true);
                SceneManager.LoadScene("GameStartScene");
            }
            else
            {
                // 일반 노드 패배 — 자동 재시작 (튜토리얼 처음부터)
                GameLog.Event("다시 도전!", LogCategory.Status);
                Debug.Log("[BattleManager] 튜토리얼 일반 노드 패배 — 파티 재생성 후 같은 씬 리로드");
                AudioManager.Instance?.PlaySfxById(SfxId.Defeat);
                yield return new WaitForSeconds(gameOverDelay);
                PartyManager.Instance?.ForceReinitParty();
                SceneManager.LoadScene("GamePlayScene");
            }
            yield break;
        }

        if (allEnemiesDead)
        {
            GameLog.Event("전투에서 승리했다!", LogCategory.Reward);
            Debug.Log("[BattleManager] 전투 승리!");
            AudioManager.Instance?.PlaySfxById(SfxId.Victory);
            // 영혼석 보상은 적 처치 즉시 ProcessDeathAndStress 에서 지급됨 (기획 §15).
            // GrantBattleReward 는 추가 보상(예: 클리어 보너스) 자리로 남겨둠.
            GrantBattleReward();
            GrantStressRecovery();

            // 보스 클리어 판정 — 사용자 기획: "보스 오브젝트가 나온 노드 이겼을 때 기준".
            // enemies 에 Boss tier 가 있었고 + RoomType.Boss 노드 였을 때만 엔딩 (둘 다 만족 필수).
            bool bossWasInBattle = enemies.Any(e => e != null && e.tier == EnemyTier.Boss);
            bool isBossRoom      = NodeSystem.Current != null && NodeSystem.Current.CurrentRoomType == RoomType.Boss;
            if (bossWasInBattle && isBossRoom)
            {
                GameLog.Event("보스를 쓰러트렸다!", LogCategory.Reward);
                Debug.Log("[BattleManager] 🎉 보스 클리어 — 엔딩 진입");
                ShowEndingPanel();

                // 엔딩 표시 후 → 로그라이크 루프(기획 §16): 메인 메뉴로 가지 않고
                //   예비대/파티/영혼석 초기화 + 마석 유지 + 패시브 해금 화면 → 새 런 첫 노드.
                yield return new WaitForSeconds(endingDisplayDuration);
                Debug.Log("[BattleManager] 보스 클리어 — 로그라이크 루프: 리셋 후 새 런 시작 (마석 유지)");
                StartNextRunLoop();
            }
            else
            {
                DisplayChange.Instance.ToggleResultDisplay(allEnemiesDead);
                DisplayChange.Instance.ToggleDisplay();
                AudioManager.Instance?.PlayBgmById(BgmId.NodeMap);
            }
        }
        else
        {
            GameLog.Event("전원 쓰러졌다…", LogCategory.Death);
            Debug.Log("[BattleManager] 아군 전멸 — 엔딩 팝업 표시 후 로그라이크 루프");
            AudioManager.Instance?.PlaySfxById(SfxId.Defeat);
            DisplayChange.Instance.ToggleResultDisplay(allEnemiesDead);
            // 기획 §16 — 패배도 보스 클리어와 동일하게 엔딩 팝업(글) 표시 후 로그라이크 루프 (마석 유지).
            ShowEndingPanel();
            yield return new WaitForSeconds(endingDisplayDuration);
            Debug.Log("[BattleManager] 전멸 — 로그라이크 루프: 리셋 후 새 런 시작 (마석 유지)");
            StartNextRunLoop();
        }

    }

    /// <summary>
    /// 엔딩/결과 팝업 표시 (보스 클리어·전멸 공통). 부모 트리가 비활성이어도 보이도록 상위까지 활성화.
    /// 글/연출은 endingPanel 에 붙여 사용 (기획 §16).
    /// </summary>
    private void ShowEndingPanel()
    {
        if (endingPanel == null)
        {
            Debug.LogWarning("[BattleManager] endingPanel 미할당 — 엔딩 UI 표시 생략");
            return;
        }
        var t = endingPanel.transform;
        while (t != null)
        {
            if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
            t = t.parent;
        }
        endingPanel.SetActive(true);
    }

    /// <summary>
    /// 로그라이크 메타 루프 (기획 §16) — 보스 클리어/전멸 공통.
    /// 예비대·파티·영혼석 초기화(마석은 유지) → 새 런 첫 노드 전 패시브 해금 화면 표시 플래그 →
    /// GamePlayScene 재로드(노드맵 재생성).
    /// </summary>
    private void StartNextRunLoop()
    {
        MetaPassiveManager.ShowShopOnNextLoad = true;     // 새 런 첫 노드 전 마석 상점 자동 표시
        MercenaryService.Instance?.ResetForNewRun();      // 예비대/후보/리롤 초기화
        PartyManager.Instance?.ResetGame();               // 파티(+사망보관소) 초기화
        SoulstoneManager.Instance?.ResetCurrency();       // 영혼석 기본값 (마석은 PlayerPrefs 유지)
        SceneManager.LoadScene("GamePlayScene");          // 새 런 (노드맵 재생성)
    }

    // ============================================================
    // 전투 승리 보상 지급 — 노드 타입별 마석 차등
    // ============================================================
    //   영혼석은 BattleManager.Combat 에서 적 처치 시 즉시 지급되므로 여기서는 처리하지 않음.
    //   마석은 메타 재화 — 노드 타입별 차등 지급:
    //     일반 전투 (RoomType.Combat) → +10
    //     엘리트 (RoomType.Elite)    → +20
    //     보스   (RoomType.Boss)     → +30
    // ============================================================
    private void GrantBattleReward()
    {
        int floor = NodeSystem.Current != null ? NodeSystem.Current.CurrentFloor : 0;
        RoomType room = NodeSystem.Current != null ? NodeSystem.Current.CurrentRoomType : RoomType.Combat;

        int reward = room switch
        {
            RoomType.Boss  => 30,
            RoomType.Elite => 20,
            _              => 10,   // Combat 및 그 외 안전 폴백
        };

        if (ManastoneManager.Instance != null)
        {
            ManastoneManager.Instance.Add(reward);
            GameLog.Event($"마석 +{reward} 획득.", LogCategory.Reward);
            Debug.Log($"[BattleManager] 전투 승리 (층 {floor} / {room}) — 노드 보상 마석 +{reward}");
        }
    }
    
    // 기획 §스트레스 §기본 회복 — 전투 승리: -10
    private void GrantStressRecovery()
    {
        foreach (var ally in allies.Where(a => !a.isDead))
            ally.currentStress = Mathf.Max(0, ally.currentStress - 10);
        GameLog.Event("생존한 동료들의 스트레스가 회복되었다 (-10).", LogCategory.Heal);
        Debug.Log("[BattleManager] 전투 승리 보상 — 생존 동료 스트레스 -10");
    }
}
