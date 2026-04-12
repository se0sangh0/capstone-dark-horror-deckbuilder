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
    public List<CompanionEntity> allies = new List<CompanionEntity>();
    public List<EnemyEntity> enemies = new List<EnemyEntity>();
    
    [Header("Stacks Info")]
    public int currentTurnStackSum = 0; 
    public int enemyPowerScore = 5;     

    [Header("Settings & Timers")]
    public string gameOverSceneName = "GameStartScene"; //전멸 후 넘어가는 씬
    public float actionDelayTime = 0.5f; // 행동 시 대기 시간
    public float turnTransitionDelay = 1.0f; // 턴 종료 후 다음 턴 대기 시간
    public float gameOverDelay = 1.5f; // 게임 오버 연출 대기 시간

    private bool isPlayerTurnFinishing = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        StartCoroutine(BattleLoop());
    }

    private IEnumerator BattleLoop()
    {
        while (currentPhase != BattlePhase.BattleEnd)
        {
            yield return StartCoroutine(ExecutePhase(currentPhase));
        }
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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartMyTurn();
        }
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
        
        currentTurnStackSum = 0;

        if (CheckBattleEndCondition())
        {
            currentPhase = BattlePhase.BattleEnd;
            yield return StartCoroutine(HandleBattleEnd());
        }
        else
        {
            yield return new WaitForSeconds(turnTransitionDelay);
            EndTurnAndStartNext(); // 기존 GameManager의 턴 넘김 로직 통합
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

    #region Core Battle Logic

    public void FinishPlayerTurn()
    {
        if (currentPhase == BattlePhase.PlayerCardPlay)
        {
            isPlayerTurnFinishing = true;
            Debug.Log("UI 버튼으로 턴 종료 신호를 보냈습니다.");
        }
    }

    //턴 종료 및 시작 제어
    private void EndTurnAndStartNext()
    {
        Debug.Log("턴 종료. 다음 턴(드로우 페이즈) 시작...");
        currentPhase = BattlePhase.DrawPhase;
    }

    public void PlayCardOnStack(CardData cardData, StackType targetStack)
    {
        currentTurnStackSum += cardData.cardPower;
        Debug.Log($"[{targetStack}] 위치에 '{cardData.name}' 카드 배치됨. (누적 파워: {currentTurnStackSum})");
    }

    private void DecideInitiative()
    {
        int allyTotalStack = allies.Where(a => !a.isDead).Sum(a => a.currentStack) + currentTurnStackSum;
        Debug.Log($"선공 판정치 - 아군 스택 합: {allyTotalStack} vs 적군 스택 합: {enemyPowerScore}");

        if (allyTotalStack > enemyPowerScore) 
            isAllyFirstAttacker = true;
        else if (allyTotalStack < enemyPowerScore) 
            isAllyFirstAttacker = false;
        else
            isAllyFirstAttacker = Random.value > 0.5f;
    }

    private IEnumerator ExecuteAction(bool isAllyTurn)
    {
        if (isAllyTurn)
        {
            foreach(var ally in allies.Where(a => !a.isDead))
            {
                if (ally.currentStack > 0)
                {
                    string allyName = ally.baseData != null ? ally.baseData.name : "이름 없음";
                    Debug.Log($"{allyName}(이)가 보유한 스택({ally.currentStack})을 사용하여 행동합니다!");
                    ally.currentStack = 0; 
                    yield return new WaitForSeconds(actionDelayTime);
                }
            }
        }
        else
        {
            foreach(var enemy in enemies.Where(e => !e.isDead))
            {
                Debug.Log($"적 {enemy.enemyName}(이)가 아군을 공격합니다!");
                yield return new WaitForSeconds(actionDelayTime);
            }
        }
    }

    private void ProcessDeathAndStress()
    {
        foreach (var ally in allies.Where(a => a.currentHp <= 0 && !a.isDead))
        {
            ally.isDead = true;
            string allyName = ally.baseData != null ? ally.baseData.name : "이름 없음";
            Debug.Log($"{allyName} 사망!");
        }

        foreach (var enemy in enemies.Where(e => e.currentHp <= 0 && !e.isDead))
        {
            enemy.isDead = true;
            Debug.Log($"적 {enemy.enemyName} 처치됨.");
        }
    }

    private bool CheckBattleEndCondition()
    {
        if (enemies.Count > 0 && enemies.All(e => e.isDead)) return true;
        if (allies.Count > 0 && allies.All(a => a.isDead)) return true;
        return false;
    }

    #endregion
}
//임시 더미 데이터(수정하면 변경필요)
[System.Serializable]
public class CompanionEntity
{
    public CardData baseData;
    public StackType positionStack;     
    public int currentHp = 100;
    public int currentStress = 0;
    public int currentStack = 0; 
    public bool isDead = false;
}

[System.Serializable]
public class EnemyEntity
{
    public string enemyName = "테스트 몬스터";
    public CardData baseData;
    public int currentHp = 100;
    public bool isDead = false;
}