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
        Debug.Log("--- [1] 드로우 페이즈 ---");
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
    // 아군과 적군 중 누가 먼저 행동할지 결정합니다.
    // ----------------------------------------------------------
    private void HandleInitiativeCheck()
    {
        Debug.Log("--- [3] 선공 판정 ---");
        DecideInitiative();
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

        // 이번 턴 스택 전부 초기화
        // PlayerRoleCost.Instance.SetAmount(StackType.Dealer,  0);
        // PlayerRoleCost.Instance.SetAmount(StackType.Tank,    0);
        // PlayerRoleCost.Instance.SetAmount(StackType.Support, 0);

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
        bool allAlliesDead = allies.Count > 0 && allies.All(a => a.isDead);

        if (allAlliesDead)
        {
            Debug.Log("[BattleManager] 아군 전멸! 게임 오버 씬으로 전환합니다.");
            yield return new WaitForSeconds(gameOverDelay);
            SceneManager.LoadScene(gameOverSceneName);
        }
        else
        {
            Debug.Log("[BattleManager] 전투 승리!");
            // TODO: 승리 처리 로직 (보상, 다음 씬 이동 등)
        }
    }
}
