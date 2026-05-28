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
        yield return StartCoroutine(ExecuteAction(isAllyTurn));
        currentPhase = nextPhase;
    }

    // ----------------------------------------------------------
    // 6. 결과 처리 페이즈
    // 사망 처리, 스택 초기화, 전투 종료 여부 판정
    // ----------------------------------------------------------
    private IEnumerator HandleResultProcessing()
    {
        Debug.Log("--- [6] 결과 처리 ---");
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

        // 미행동자 순서 재정렬 — 기획 §코어루프 §동료 행동
        //   "미행동 보상: 해당 스택 +1, 다음 턴 순서 우선"
        //   적 타겟팅도 새 순서를 따라가 "먼저 행동·먼저 피격" 트레이드 자동 반영.
        if (_carryoverOrderList.Count > 0)
        {
            var aliveCarryover = _carryoverOrderList.Where(a => !a.isDead).ToList();
            foreach (var ally in aliveCarryover) allies.Remove(ally);
            allies.InsertRange(0, aliveCarryover);
            Debug.Log($"[결과 처리] 미행동자 {aliveCarryover.Count}명 → 다음 턴 순서 우선 재정렬");
            _carryoverOrderList.Clear();
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
                if (endingPanel != null)
                {
                    // 부모가 비활성(PopUp 등) 이라도 보이도록 상위 트리 모두 활성화
                    var t = endingPanel.transform;
                    while (t != null)
                    {
                        if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
                        t = t.parent;
                    }
                    endingPanel.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("[BattleManager] endingPanel 미할당 — 엔딩 UI 표시 생략");
                }

                // 엔딩 표시 후 일정 시간 대기 → 게임 리셋(파티+영혼석) + GameStartScene 복귀
                // 마석은 메타 재화이므로 초기화 제외 (런 종료 후에도 유지)
                yield return new WaitForSeconds(endingDisplayDuration);
                Debug.Log("[BattleManager] 엔딩 종료 — 게임 초기화 + GameStartScene 복귀");
                PartyManager.Instance?.ResetGame();
                SoulstoneManager.Instance?.ResetCurrency();
                SceneManager.LoadScene(gameOverSceneName);
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
            Debug.Log("[BattleManager] 아군 전멸! 게임 오버 씬으로 전환합니다.");
            AudioManager.Instance?.PlaySfxById(SfxId.Defeat);
            DisplayChange.Instance.ToggleResultDisplay(allEnemiesDead);
            PartyManager.Instance?.ResetGame();
            SoulstoneManager.Instance?.ResetCurrency();
            // 마석은 메타 재화이므로 초기화 제외 (런 종료 후에도 유지)
            SceneManager.LoadScene(gameOverSceneName);
        }

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
