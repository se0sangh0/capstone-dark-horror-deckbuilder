using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.SceneManagement; // 씬 전환을 위해 추가
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

    [Header("Scene Transition")]
    public string gameOverSceneName = "GameStartScene"; // 전환할 씬 이름을 인스펙터에서 설정하세요.

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
                Debug.Log("--- 1. 드로우 페이즈 ---");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.StartMyTurn();
                }
                currentPhase = BattlePhase.PlayerCardPlay;
                break;

            case BattlePhase.PlayerCardPlay:
                Debug.Log("--- 2. 플레이어 카드 플레이 (대기중) ---");
                isPlayerTurnFinishing = false;
                
                // 버튼 클릭 또는 스페이스바 입력 대기
                yield return new WaitUntil(() => 
                    isPlayerTurnFinishing || 
                    (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                );
                
                currentPhase = BattlePhase.InitiativeCheck;
                break;

            case BattlePhase.InitiativeCheck:
                Debug.Log("--- 3. 선공 판정 ---");
                DecideInitiative();
                currentPhase = BattlePhase.FirstAction;
                break;

            case BattlePhase.FirstAction:
                Debug.Log($"--- 4. 선공 측 행동 ({(isAllyFirstAttacker ? "아군" : "적군")}) ---");
                yield return StartCoroutine(ExecuteAction(isAllyFirstAttacker));
                currentPhase = BattlePhase.SecondAction;
                break;

            case BattlePhase.SecondAction:
                Debug.Log($"--- 5. 후공 측 행동 ({(!isAllyFirstAttacker ? "아군" : "적군")}) ---");
                yield return StartCoroutine(ExecuteAction(!isAllyFirstAttacker));
                currentPhase = BattlePhase.ResultProcessing;
                break;

            case BattlePhase.ResultProcessing:
                Debug.Log("--- 6. 결과 처리 ---");
                ProcessDeathAndStress();
                
                currentTurnStackSum = 0;

                if (CheckBattleEndCondition())
                {
                    currentPhase = BattlePhase.BattleEnd;
                    
                    // 아군 전멸 시 씬 전환 로직 추가
                    if (allies.Count > 0 && allies.All(a => a.isDead))
                    {
                        Debug.Log("아군 전멸! 게임 오버 씬으로 전환합니다.");
                        yield return new WaitForSeconds(1.5f); // 사망 연출을 위해 잠시 대기
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

    public void FinishPlayerTurn()
    {
        if (currentPhase == BattlePhase.PlayerCardPlay)
        {
            isPlayerTurnFinishing = true;
            Debug.Log("UI 버튼으로 턴 종료 신호를 보냈습니다.");
        }
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
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }
        else
        {
            foreach(var enemy in enemies.Where(e => !e.isDead))
            {
                Debug.Log($"적 {enemy.enemyName}(이)가 아군을 공격합니다!");
                yield return new WaitForSeconds(0.5f);
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
        if (allies.Count > 0 && allies.All(a => a.isDead)) return true; //
        return false;
    }
}

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
    public int currentHp = 100;
    public bool isDead = false;
}