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
    public List<FellowData> allies  = new List<FellowData>();
    public List<EnemyEntity>     enemies = new List<EnemyEntity>();
    
    [Header("Card Pool")]
    public List<CardData> allCards = new List<CardData>(); 

    // -------------------------------------------------------
    // 스택 (역할별 + 이번 턴 합산)
    // 기획서 기준: 아군 점수 = Σ(동료 스택) + 이번 턴 카드 스택 합
    // -------------------------------------------------------
    // [Header("Stacks — 이번 턴")]
    // public int dealerStack  = 0;
    // public int tankStack    = 0;
    // public int supportStack = 0;

    // 적 고정 스택: 일반=3, 보스=8 (기획서 §선공판정)
    [Header("Enemy")]
    [Tooltip("일반 적=3, 보스=8  (기획/시스템/02_전투_시스템_명세.md)")]
    public int enemyPowerScore = 3;

    [Header("Settings & Timers")]
    public string gameOverSceneName = "GameStartScene";
    public float actionDelayTime = 0.5f;       // 행동 시 대기 시간
    public float turnTransitionDelay = 1.0f;   // 턴 종료 후 다음 턴 대기 시간
    public float gameOverDelay = 1.5f;         // 게임 오버 연출 대기 시간

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
        InitBattle();
        StartCoroutine(BattleLoop());
    }

    private void InitBattle()
    {
        if (PartyManager.Instance == null)
        {
            Debug.LogError("[BattleManager] PartyManager가 없습니다!");
            return;
        }

        var companions = PartyManager.Instance.GetActiveCompanions();
        Debug.Log($"[BattleManager] 전투 시작 — 동료 {companions.Count}명");

        allies.Clear();
        foreach (var companion in companions)
        {
            var fellow = ScriptableObject.CreateInstance<FellowData>();
            fellow.data = companion;
            fellow.positionStack = (StackType)(int)companion.role;
            fellow.CurrentHp = companion.maxHp;
            fellow.isDead = false;

            // ✅ Resources에서 스프라이트 로드
            if (!string.IsNullOrEmpty(companion.spritePath))
            {
                fellow.fellowSprite = Resources.Load<Sprite>(companion.spritePath);
                if (fellow.fellowSprite == null)
                    Debug.LogWarning($"[BattleManager] 스프라이트 없음: {companion.spritePath}");
            }

            allies.Add(fellow);
        }

        var pool = GenerateCardPool();
        var deck = DeckBuilder.BuildPartyDeck(companions, pool);
        GameManager.Instance.InjectDeck(deck);
        Debug.Log($"[BattleManager] 덱 주입 완료: {deck.Count}장");
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
            case BattlePhase.DrawPhase:
                yield return StartCoroutine(HandleDrawPhase());
                break;
            case BattlePhase.PlayerCardPlay:
                yield return StartCoroutine(HandlePlayerCardPlay());
                break;
            case BattlePhase.InitiativeCheck:
                HandleInitiativeCheck();
                break;
            case BattlePhase.FirstAction:
                yield return StartCoroutine(HandleActionPhase(isAllyFirstAttacker, BattlePhase.SecondAction));
                break;
            case BattlePhase.SecondAction:
                yield return StartCoroutine(HandleActionPhase(!isAllyFirstAttacker, BattlePhase.ResultProcessing));
                break;
            case BattlePhase.ResultProcessing:
                yield return StartCoroutine(HandleResultProcessing());
                break;
        }
    }

    #region Phase Handlers (모듈화된 페이즈 로직)

    private IEnumerator HandleDrawPhase()
    {
        Debug.Log("--- 1. 드로우 페이즈 ---");
        GameManager.Instance?.StartMyTurn();
        currentPhase = BattlePhase.PlayerCardPlay;
        yield return null;
    }

    private IEnumerator HandlePlayerCardPlay()
    {
        Debug.Log("--- 2. 플레이어 카드 플레이 (대기중) ---");
        isPlayerTurnFinishing = false;

        yield return new WaitUntil(() =>
            isPlayerTurnFinishing ||
            (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        );

        currentPhase = BattlePhase.InitiativeCheck;
    }

    private void HandleInitiativeCheck()
    {
        Debug.Log("--- 3. 선공 판정 ---");
        DecideInitiative();
        currentPhase = BattlePhase.FirstAction;
    }

    private IEnumerator HandleActionPhase(bool isAllyTurn, BattlePhase nextPhase)
    {
        string faction = isAllyTurn ? "아군" : "적군";
        Debug.Log($"--- 행동 페이즈 진행 ({faction}) ---");
        yield return StartCoroutine(ExecuteAction(isAllyTurn));
        currentPhase = nextPhase;
    }

    private IEnumerator HandleResultProcessing()
    {
        Debug.Log("--- 6. 결과 처리 ---");
        ProcessDeathAndStress();
        //ResetTurnStacks();
        PlayerRoleCost.Instance.SetAmount(StackType.Dealer, 0);
        PlayerRoleCost.Instance.SetAmount(StackType.Tank, 0);
        PlayerRoleCost.Instance.SetAmount(StackType.Support, 0);

        if (CheckBattleEndCondition())
        {
            currentPhase = BattlePhase.BattleEnd;
            yield return StartCoroutine(HandleBattleEnd());
        }
        else
        {
            yield return new WaitForSeconds(turnTransitionDelay);
            currentPhase = BattlePhase.DrawPhase;
        }
    }

    private IEnumerator HandleBattleEnd()
    {
        if (allies.Count > 0 && allies.All(a => a.isDead))
        {
            Debug.Log("아군 전멸! 게임 오버 씬으로 전환합니다.");
            yield return new WaitForSeconds(gameOverDelay);
            SceneManager.LoadScene(gameOverSceneName);
        }
        else
        {
            Debug.Log("전투 승리!");
            // TODO: 승리 처리 로직
        }
    }

    #endregion

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
        PlayerRoleCost.Instance.Add(targetStack, cardData.stackDelta);
        Debug.Log($"[CardPlayed] id={cardData.id} type={targetStack} delta={cardData.stackDelta:+#;-#;0}");
    }

    /// <summary>
    /// StackCardController → BattleManager 직접 반영 경로 (GameManager.OnCardUsed에서 호출).
    /// </summary>

    // -------------------------------------------------------
    // 선공 판정
    // 기획서: 아군 점수 = Σ(동료 스택) + 이번 턴 카드 스택 합
    // -------------------------------------------------------
    private void DecideInitiative()
    {
        //프로토타입 - 아군 선공 고정
        isAllyFirstAttacker = true;
        Debug.Log("[선공 판정] 프로토타입 — 아군 선공 고정");
        //Todo 선공 판정 조건 추후 구현 예정
        //int allyTotalStack = allies.Where(a => !a.isDead).Sum(a => a.carryOverStack)
                           //+ currentTurnStackSum;
        // int allyTotalStack = allies.Where(a => !a.isDead).Sum(a => a.currentStack);
        //
        // Debug.Log($"[선공 판정] 아군={allyTotalStack} vs 적={enemyPowerScore}");
        //
        // if      (allyTotalStack > enemyPowerScore) isAllyFirstAttacker = true;
        // else if (allyTotalStack < enemyPowerScore) isAllyFirstAttacker = false;
        // else
        // {
        //     isAllyFirstAttacker = Random.value > 0.5f;
        //     Debug.Log($"동점 → 코인 토스: {(isAllyFirstAttacker ? "아군 선공" : "적 선공")}");
        // }
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
                string allyName    = ally != null ? ally.positionStack.ToString() : "이름 없음";
                //int    roleStack   = GetStackForRole(ally.positionStack);
                int roleStack = PlayerRoleCost.Instance.GetAmount(ally.positionStack);
                int totalStack  = roleStack + ally.currentStack;
                //int required = ally != null ? ally.baseData.requiredStack : 3;
                int required = ally.data?.requiredStack ?? 3;

                if (totalStack >= required)
                {
                    Debug.Log($"{allyName}(이)가 스택({totalStack}/{required})으로 행동합니다!");
                    //ConsumeStackForRole(ally.positionStack, required);
                    PlayerRoleCost.Instance.Use(ally.positionStack, required);
                    ally.currentStack = 0;
                    // TODO: 실제 스킬 실행 (SkillDefinition 연동 후 교체)
                    yield return new WaitForSeconds(actionDelayTime);
                }
                else
                {
                    // 스킵 + 미행동 보상 (역할 스택 +1, 다음 턴 이월)
                    ally.currentStack+= 1;
                    Debug.Log($"[UnitSkipped] {allyName} 스택 부족 ({totalStack}/{required}) → 보너스 +1 이월 (carry={ally.currentStack})");
                }
            }
        }
        else
        {
            // foreach (var enemy in enemies.Where(e => !e.isDead))
            // {
            //     Debug.Log($"적 {enemy.enemyName}(이)가 아군을 공격합니다!");
            //     // TODO: 적 AI 행동 구현
            //     yield return new WaitForSeconds(actionDelayTime);
            // }
            foreach (var enemy in enemies.Where(e => !e.isDead))
            {
                var targets = allies.Where(a => !a.isDead).ToList();
                if (targets.Count == 0) break;

                // 랜덤 타겟 선택
                var target = targets[Random.Range(0, targets.Count)];
                ApplyDamageToAlly(target, enemy.attackPower);

                Debug.Log($"[적 행동] {enemy.enemyName} → {target.positionStack} {enemy.attackPower} 데미지");
                yield return new WaitForSeconds(actionDelayTime);
            }
        }
    }
    // ✅ 추가 — 데미지 적용 + 슬라이더 갱신
    private void ApplyDamageToAlly(FellowData target, int damage)
    {
        target.CurrentHp = Mathf.Max(0, target.CurrentHp - damage);
        UpdateAllyHpUI(target);
        Debug.Log($"[HP] {target.positionStack} HP: {target.CurrentHp}");
    }

    private void UpdateAllyHpUI(FellowData target)
    {
        if (target.HpSlider != null)
            target.HpSlider.value = target.CurrentHp;
        else
            Debug.LogWarning($"[UI] {target.positionStack}의 hpSlider가 null입니다.");
    }
    
    private List<CardData> GenerateCardPool()
    {
        var pool = new List<CardData>();

        // 역할별로 더미 CardData를 런타임에 생성
        foreach (StackType role in System.Enum.GetValues(typeof(StackType)))
        {
            for (int i = 0; i < 10; i++)
            {
                var card = ScriptableObject.CreateInstance<CardData>();
                card.id = $"card_{role}_{i}";
                card.stackType = role;
                card.stackDelta = 0; // stackDelta는 성향에서 런타임 생성하니까 0으로 고정
                pool.Add(card);
            }
        }
        return pool;
    }
    // -------------------------------------------------------
    // 사망 처리 + 스트레스 전파
    // 기획서: 동료 사망 시 생존 동료 전원 스트레스 +20
    // -------------------------------------------------------
    private void ProcessDeathAndStress()
    {
        // 이번 처리 사이클에서 사망 확정된 동료 목록 (isDead 플래그 세우기 전)
        var dyingAllies = allies.Where(a => a.CurrentHp <= 0 && !a.isDead).ToList();

        foreach (var ally in dyingAllies)
        {
            ally.isDead = true;
            Debug.Log($"[UnitDied] {ally.positionStack}");

            // 덱에서 카드 제거
            if (ally.data != null)
                GameManager.Instance?.RemoveCardsOfCompanion(ally.data);

            // ✅ PartyManager에도 사망 통보
            PartyManager.Instance?.RemoveCompanion(ally.data);

            // 스트레스 전파
            foreach (var survivor in allies.Where(a => !a.isDead))
            {
                survivor.currentStress += 20;
                Debug.Log($"[스트레스] {survivor.positionStack} +20");
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

   /* // -------------------------------------------------------
    // 내부 유틸리티
    // -------------------------------------------------------

    /// <summary>턴 종료 시 역할별 스택 및 합산값 초기화.</summary>
    // private void ResetTurnStacks()
    // {
    //     dealerStack        = 0;
    //     tankStack          = 0;
    //     supportStack       = 0;
    //     currentTurnStackSum = 0;
    // }

    // private int GetStackForRole(StackType role)
    // {
    //     return role switch
    //     {
    //         StackType.Dealer  => dealerStack,
    //         StackType.Tank    => tankStack,
    //         StackType.Support => supportStack,
    //         _                 => 0
    //     };
    // }

    /// <summary>스택 소비량이 음수가 되지 않도록 clamp. (stackMin=0, 기획/SO스키마 §CombatTuning)</summary>
    // private void ConsumeStackForRole(StackType role, int amount)
    // {
    //     switch (role)
    //     {
    //         case StackType.Dealer:  dealerStack  = Mathf.Max(0, dealerStack  - amount); break;
    //         case StackType.Tank:    tankStack    = Mathf.Max(0, tankStack    - amount); break;
    //         case StackType.Support: supportStack = Mathf.Max(0, supportStack - amount); break;
    //     }
    // }*/
   // BattleManager.cs 하단에 추가
   [ContextMenu("TEST / 아군 전체 10 데미지")]
   private void TestDamageAllAllies()
   {
       foreach (var ally in allies.Where(a => !a.isDead))
           ApplyDamageToAlly(ally, 10);
   }

   [ContextMenu("TEST / 아군 랜덤 1명 10 데미지")]
   private void TestDamageRandomAlly()
   {
       var targets = allies.Where(a => !a.isDead).ToList();
       if (targets.Count == 0) return;
       ApplyDamageToAlly(targets[Random.Range(0, targets.Count)], 10);
   }

}

// -------------------------------------------------------
// 런타임 엔티티
// -------------------------------------------------------


[System.Serializable]
public class EnemyEntity
{
    public string enemyName = "테스트 몬스터";
    public int    currentHp = 100;
    public int    attackPower = 10; // ✅ 추가
    public bool   isDead    = false;
}
