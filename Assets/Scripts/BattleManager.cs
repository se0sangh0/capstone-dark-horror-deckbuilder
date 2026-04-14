// BattleManager.cs
// 전투 흐름 제어 및 스택 관리.
// 규칙 기준: 기획/시스템/02_전투_시스템_명세.md

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Linq;

public enum BattlePhase
{
    DrawPhase,
    PlayerCardPlay,
    InitiativeCheck,
    FirstAction,
    SecondAction,
    ResultProcessing,
    BattleEnd
}

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Battle State")]
    public BattlePhase currentPhase;
    public bool isAllyFirstAttacker;

    [Header("Entities Data")]
    public List<CompanionEntity> allies  = new List<CompanionEntity>();
    public List<EnemyEntity>     enemies = new List<EnemyEntity>();

    // -------------------------------------------------------
    // 스택 (역할별 + 이번 턴 합산)
    // 기획서 기준: 아군 점수 = Σ(동료 스택) + 이번 턴 카드 스택 합
    // -------------------------------------------------------
    [Header("Stacks — 이번 턴")]
    public int dealerStack  = 0;
    public int tankStack    = 0;
    public int supportStack = 0;
    /// <summary>이번 턴 카드 기여량 합산 (선공 판정용)</summary>
    public int currentTurnStackSum = 0;

    // 적 고정 스택: 일반=3, 보스=8 (기획서 §선공판정)
    [Header("Enemy")]
    [Tooltip("일반 적=3, 보스=8  (기획/시스템/02_전투_시스템_명세.md)")]
    public int enemyPowerScore = 3;

    [Header("Scene Transition")]
    public string gameOverSceneName = "GameStartScene";

    private bool isPlayerTurnFinishing = false;

    // -------------------------------------------------------
    // 초기화
    // -------------------------------------------------------
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        StartCoroutine(BattleLoop());
    }

    // -------------------------------------------------------
    // 메인 루프
    // -------------------------------------------------------
    private IEnumerator BattleLoop()
    {
        while (currentPhase != BattlePhase.BattleEnd)
            yield return StartCoroutine(ExecutePhase(currentPhase));
    }

    private IEnumerator ExecutePhase(BattlePhase phase)
    {
        switch (phase)
        {
            // ── 1. 드로우 페이즈 ─────────────────────────────────
            case BattlePhase.DrawPhase:
                Debug.Log("--- 1. 드로우 페이즈 ---");
                GameManager.Instance?.StartMyTurn();
                currentPhase = BattlePhase.PlayerCardPlay;
                break;

            // ── 2. 플레이어 카드 플레이 ──────────────────────────
            case BattlePhase.PlayerCardPlay:
                Debug.Log("--- 2. 플레이어 카드 플레이 (대기중) ---");
                isPlayerTurnFinishing = false;

                yield return new WaitUntil(() =>
                    isPlayerTurnFinishing ||
                    (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                );

                currentPhase = BattlePhase.InitiativeCheck;
                break;

            // ── 3. 선공 판정 ─────────────────────────────────────
            case BattlePhase.InitiativeCheck:
                Debug.Log("--- 3. 선공 판정 ---");
                DecideInitiative();
                currentPhase = BattlePhase.FirstAction;
                break;

            // ── 4. 선공 측 행동 ──────────────────────────────────
            case BattlePhase.FirstAction:
                Debug.Log($"--- 4. 선공 측 행동 ({(isAllyFirstAttacker ? "아군" : "적군")}) ---");
                yield return StartCoroutine(ExecuteAction(isAllyFirstAttacker));
                currentPhase = BattlePhase.SecondAction;
                break;

            // ── 5. 후공 측 행동 ──────────────────────────────────
            case BattlePhase.SecondAction:
                Debug.Log($"--- 5. 후공 측 행동 ({(!isAllyFirstAttacker ? "아군" : "적군")}) ---");
                yield return StartCoroutine(ExecuteAction(!isAllyFirstAttacker));
                currentPhase = BattlePhase.ResultProcessing;
                break;

            // ── 6. 결과 처리 ─────────────────────────────────────
            case BattlePhase.ResultProcessing:
                Debug.Log("--- 6. 결과 처리 ---");
                ProcessDeathAndStress();
                ResetTurnStacks();

                if (CheckBattleEndCondition())
                {
                    currentPhase = BattlePhase.BattleEnd;

                    if (allies.Count > 0 && allies.All(a => a.isDead))
                    {
                        Debug.Log("아군 전멸! 게임 오버 씬으로 전환합니다.");
                        yield return new WaitForSeconds(1.5f);
                        SceneManager.LoadScene(gameOverSceneName);
                    }
                }
                else
                {
                    yield return new WaitForSeconds(1.0f);
                    currentPhase = BattlePhase.DrawPhase;
                }
                break;
        }
    }

    // -------------------------------------------------------
    // 공개 API
    // -------------------------------------------------------

    public void FinishPlayerTurn()
    {
        if (currentPhase == BattlePhase.PlayerCardPlay)
        {
            isPlayerTurnFinishing = true;
            Debug.Log("UI 버튼으로 턴 종료 신호를 보냈습니다.");
        }
    }

    /// <summary>
    /// CardData SO를 사용하는 경로 (Inspector 연동 시 사용).
    /// stackDelta 로 역할별 스택에 반영한다.
    /// </summary>
    public void PlayCardOnStack(CardData cardData, StackType targetStack)
    {
        AddStack(targetStack, cardData.stackDelta);
        Debug.Log($"[CardPlayed] id={cardData.id} type={targetStack} delta={cardData.stackDelta:+#;-#;0}");
    }

    /// <summary>
    /// StackCardController → BattleManager 직접 반영 경로 (GameManager.OnCardUsed에서 호출).
    /// </summary>
    public void AddStack(StackType type, int delta)
    {
        switch (type)
        {
            case StackType.Dealer:  dealerStack  += delta; break;
            case StackType.Tank:    tankStack    += delta; break;
            case StackType.Support: supportStack += delta; break;
        }
        currentTurnStackSum += delta;
        Debug.Log($"[스택 갱신] {type} {delta:+#;-#;0} → D={dealerStack} T={tankStack} S={supportStack} (합:{currentTurnStackSum})");
    }

    // -------------------------------------------------------
    // 선공 판정
    // 기획서: 아군 점수 = Σ(동료 스택) + 이번 턴 카드 스택 합
    // -------------------------------------------------------
    private void DecideInitiative()
    {
        int allyTotalStack = allies.Where(a => !a.isDead).Sum(a => a.carryOverStack)
                           + currentTurnStackSum;

        Debug.Log($"[선공 판정] 아군={allyTotalStack} vs 적={enemyPowerScore}");

        if      (allyTotalStack > enemyPowerScore) isAllyFirstAttacker = true;
        else if (allyTotalStack < enemyPowerScore) isAllyFirstAttacker = false;
        else
        {
            isAllyFirstAttacker = Random.value > 0.5f;
            Debug.Log($"동점 → 코인 토스: {(isAllyFirstAttacker ? "아군 선공" : "적 선공")}");
        }
    }

    // -------------------------------------------------------
    // 행동 실행
    // 스택 부족 시: 스킵 + 미행동 보상 +1 (기획서 §MVP고정)
    // -------------------------------------------------------
    private IEnumerator ExecuteAction(bool isAllyTurn)
    {
        if (isAllyTurn)
        {
            foreach (var ally in allies.Where(a => !a.isDead))
            {
                string allyName    = ally.baseData != null ? ally.baseData.displayName : "이름 없음";
                int    roleStack   = GetStackForRole(ally.positionStack);
                int    totalStack  = roleStack + ally.carryOverStack;
                int    required    = ally.baseData != null ? ally.baseData.requiredStack : 3;

                if (totalStack >= required)
                {
                    Debug.Log($"{allyName}(이)가 스택({totalStack}/{required})으로 행동합니다!");
                    ConsumeStackForRole(ally.positionStack, required);
                    ally.carryOverStack = 0;
                    // TODO: 실제 스킬 실행 (SkillDefinition 연동 후 교체)
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    // 스킵 + 미행동 보상 (역할 스택 +1, 다음 턴 이월)
                    ally.carryOverStack += 1;
                    Debug.Log($"[UnitSkipped] {allyName} 스택 부족 ({totalStack}/{required}) → 보너스 +1 이월 (carry={ally.carryOverStack})");
                }
            }
        }
        else
        {
            foreach (var enemy in enemies.Where(e => !e.isDead))
            {
                Debug.Log($"적 {enemy.enemyName}(이)가 아군을 공격합니다!");
                // TODO: 적 AI 행동 구현
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    // -------------------------------------------------------
    // 사망 처리 + 스트레스 전파
    // 기획서: 동료 사망 시 생존 동료 전원 스트레스 +20
    // -------------------------------------------------------
    private void ProcessDeathAndStress()
    {
        // 이번 처리 사이클에서 사망 확정된 동료 목록 (isDead 플래그 세우기 전)
        var dyingAllies = allies.Where(a => a.currentHp <= 0 && !a.isDead).ToList();

        foreach (var ally in dyingAllies)
        {
            ally.isDead = true;
            string allyName = ally.baseData != null ? ally.baseData.displayName : "이름 없음";
            Debug.Log($"[UnitDied] {allyName}");

            // 생존 동료 전원 스트레스 +20 (즉시 적용, 데미지 계산 후)
            foreach (var survivor in allies.Where(a => !a.isDead))
            {
                survivor.currentStress += 20;
                string survivorName = survivor.baseData != null ? survivor.baseData.displayName : "이름 없음";
                Debug.Log($"[스트레스] {survivorName} +20 → {survivor.currentStress}");
            }
        }

        // 적 사망 처리
        foreach (var enemy in enemies.Where(e => e.currentHp <= 0 && !e.isDead))
        {
            enemy.isDead = true;
            Debug.Log($"[UnitDied] 적 {enemy.enemyName} 처치됨.");
        }
    }

    // -------------------------------------------------------
    // 전투 종료 판정
    // -------------------------------------------------------
    private bool CheckBattleEndCondition()
    {
        if (enemies.Count > 0 && enemies.All(e => e.isDead)) return true;
        if (allies.Count  > 0 && allies.All(a => a.isDead))  return true;
        return false;
    }

    // -------------------------------------------------------
    // 내부 유틸리티
    // -------------------------------------------------------

    /// <summary>턴 종료 시 역할별 스택 및 합산값 초기화.</summary>
    private void ResetTurnStacks()
    {
        dealerStack        = 0;
        tankStack          = 0;
        supportStack       = 0;
        currentTurnStackSum = 0;
    }

    private int GetStackForRole(StackType role)
    {
        return role switch
        {
            StackType.Dealer  => dealerStack,
            StackType.Tank    => tankStack,
            StackType.Support => supportStack,
            _                 => 0
        };
    }

    /// <summary>스택 소비량이 음수가 되지 않도록 clamp. (stackMin=0, 기획/SO스키마 §CombatTuning)</summary>
    private void ConsumeStackForRole(StackType role, int amount)
    {
        switch (role)
        {
            case StackType.Dealer:  dealerStack  = Mathf.Max(0, dealerStack  - amount); break;
            case StackType.Tank:    tankStack    = Mathf.Max(0, tankStack    - amount); break;
            case StackType.Support: supportStack = Mathf.Max(0, supportStack - amount); break;
        }
    }
}

// -------------------------------------------------------
// 런타임 엔티티
// -------------------------------------------------------

[System.Serializable]
public class CompanionEntity
{
    /// <summary>동료 SO 데이터 (역할·성향·requiredStack 포함)</summary>
    public CompanionData baseData;

    /// <summary>이 동료가 점유하는 스택 슬롯 유형</summary>
    public StackType positionStack;

    public int currentHp     = 100;
    public int currentStress = 0;

    /// <summary>
    /// 미행동 보상으로 누적된 이월 스택.
    /// 선공 판정 기여 + 다음 턴 행동 임계값에 합산.
    /// </summary>
    public int carryOverStack = 0;

    public bool isDead = false;
}

[System.Serializable]
public class EnemyEntity
{
    public string enemyName = "테스트 몬스터";
    public int    currentHp = 100;
    public bool   isDead    = false;
}
