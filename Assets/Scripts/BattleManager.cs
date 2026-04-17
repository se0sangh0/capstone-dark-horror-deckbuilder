// ============================================================
// BattleManager.cs
// 전투 흐름 제어 싱글톤 매니저 (파셜 클래스 메인 파일)
// ============================================================
//
// [이 파일이 하는 일]
//   전투 전체 흐름을 관리합니다. 크게 아래와 같이 나뉩니다:
//
//   BattleManager.cs (이 파일)
//     → 필드 선언, 초기화(InitBattle), 메인 루프(BattleLoop),
//       공개 API (FinishPlayerTurn, PlayCardOnStack)
//
//   BattleManager.Phases.cs
//     → 각 페이즈 핸들러 (드로우, 카드플레이, 선공판정, 행동, 결과처리)
//
//   BattleManager.Combat.cs
//     → 전투 로직 (선공 판정, 행동 실행, 데미지, 사망 처리)
//
// [전투 페이즈 순서]
//   DrawPhase → PlayerCardPlay → InitiativeCheck
//   → FirstAction → SecondAction → ResultProcessing → (반복)
//
// [어디서 쓰이나요?]
//   - DefaultSetting.cs : BattleManager.Instance.allies 직접 접근 (동적 참조)
//   - GameManager.cs : BattleManager.Instance.FinishPlayerTurn() 호출
//   - Stack/PlayerRoleCost.cs : 스택 소비 처리
//
// [연결된 파일]
//   - Core/Singleton.cs : 싱글톤 기반 클래스
//   - BattleManager.Phases.cs : 페이즈 핸들러
//   - BattleManager.Combat.cs : 전투 로직
//   - PartyManager.cs : 전투 시작 시 동료 목록 조회
//   - GameManager.cs : 덱 주입 및 카드 슬롯 세팅
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ----------------------------------------------------------
// [BattlePhase 열거형]
// 전투 진행 순서를 나타냅니다.
// ----------------------------------------------------------
/// <summary>전투 진행 단계</summary>
public enum BattlePhase
{
    DrawPhase,          // 1. 드로우: 카드를 손에 뽑음
    PlayerCardPlay,     // 2. 카드 플레이: 플레이어가 카드 선택
    InitiativeCheck,    // 3. 선공 판정: 누가 먼저 공격할지 결정
    FirstAction,        // 4. 선공 행동
    SecondAction,       // 5. 후공 행동
    ResultProcessing,   // 6. 결과 처리: 사망/스트레스/턴 초기화
    BattleEnd           // 7. 전투 종료
}

/// <summary>
/// 전투 흐름 제어 싱글톤 매니저 (파셜 클래스).
/// BattleManager.Instance 로 전역 접근 가능.
/// </summary>
public partial class BattleManager : Singleton<BattleManager>
{
    // ----------------------------------------------------------
    // [전투 상태]
    // ----------------------------------------------------------
    [Header("전투 상태 (Battle State)")]
    [Tooltip("현재 전투 페이즈")]
    public BattlePhase currentPhase;

    [Tooltip("true 이면 아군이 선공, false 이면 적이 선공")]
    public bool isAllyFirstAttacker;

    // ----------------------------------------------------------
    // [엔티티 데이터]
    // allies : 아군 동료 FellowData 목록 (DefaultSetting.cs 가 직접 접근)
    // enemies : 적 엔티티 목록
    // ----------------------------------------------------------
    [Header("엔티티 데이터 (Entities)")]
    [Tooltip("아군 동료 데이터 목록. DefaultSetting.cs 에서 직접 참조됩니다.")]
    public List<FellowData> allies  = new();

    [Tooltip("적 엔티티 목록")]
    public List<EnemyEntity> enemies = new();

    // ----------------------------------------------------------
    // [카드 풀]
    // ----------------------------------------------------------
    [Header("카드 풀 (Card Pool)")]
    [Tooltip("전투에 사용될 전체 카드 풀")]
    public List<CardData> allCards = new();

    // ----------------------------------------------------------
    // [적 기본 스택]
    // ----------------------------------------------------------
    [Header("적 설정 (Enemy)")]
    [Tooltip("적의 선공 판정 기준 스택. 일반 적=3, 보스=8 (기획서 §선공판정 참조)")]
    public int enemyPowerScore = 3;

    // ----------------------------------------------------------
    // [타이밍 설정]
    // ----------------------------------------------------------
    [Header("타이밍 설정 (Timers)")]
    [Tooltip("씬 전환 실패 시 돌아갈 씬 이름")]
    public string gameOverSceneName = "GameStartScene";

    [Tooltip("행동 실행 후 대기 시간 (초)")]
    public float actionDelayTime = 0.5f;

    [Tooltip("턴 종료 후 다음 턴 시작 전 대기 시간 (초)")]
    public float turnTransitionDelay = 1.0f;

    [Tooltip("게임 오버 연출 대기 시간 (초)")]
    public float gameOverDelay = 1.5f;

    // ----------------------------------------------------------
    // [내부 상태]
    // ----------------------------------------------------------

    /// <summary>플레이어가 턴 종료를 눌렀는지 여부</summary>
    private bool isPlayerTurnFinishing = false;

    // ----------------------------------------------------------
    // Start — 전투 초기화 및 메인 루프 시작
    // ----------------------------------------------------------
    private void Start()
    {
        // InitBattle();
        // StartCoroutine(BattleLoop());
    }

    private void OnEnable()
    {
        InitBattle();
        StartCoroutine(BattleLoop());
    }

    // ----------------------------------------------------------
    // 전투 초기화
    // PartyManager 에서 동료 목록을 가져와 FellowData 를 생성하고
    // 덱을 구성하여 GameManager 에 주입합니다.
    // ----------------------------------------------------------
    private void InitBattle()
    {
        if (PartyManager.Instance == null)
        {
            Debug.LogError("[BattleManager] PartyManager 가 없습니다! 씬에 PartyManager 를 배치하세요.");
            return;
        }

        var companions = PartyManager.Instance.GetActiveCompanions();
        Debug.Log($"[BattleManager] 전투 시작 — 동료 {companions.Count}명");

        // 동료 FellowData 생성 및 초기화
        allies.Clear();
        foreach (var companion in companions)
        {
            // 런타임 FellowData 생성 (ScriptableObject)
            var fellow = ScriptableObject.CreateInstance<FellowData>();
            fellow.data           = companion;
            fellow.positionStack  = (StackType)(int)companion.role;
            fellow.CurrentHp      = companion.maxHp;
            fellow.isDead         = false;

            // Resources 폴더에서 스프라이트 로드
            if (!string.IsNullOrEmpty(companion.spritePath))
            {
                fellow.fellowSprite = Resources.Load<Sprite>(companion.spritePath);
                if (fellow.fellowSprite == null)
                    Debug.LogWarning($"[BattleManager] 스프라이트 없음: {companion.spritePath}");
            }

            // -------------------------------------------------------
            // 스킬 랜덤 배정 (직업에 맞는 스킬 2개, 중복 없음)
            // -------------------------------------------------------
            if (SkillDatabase.Instance != null)
            {
                // 이 동료의 역할에 맞는 스킬 2개를 랜덤으로 선택하여 배정
                companion.skillIds = SkillDatabase.Instance.AssignRandomSkills(fellow.positionStack, 2);

                // Inspector 에서 배정된 스킬을 확인할 수 있도록 요약 정보 갱신
                fellow.RefreshSkillInfo();
            }
            else
            {
                Debug.LogWarning("[BattleManager] SkillDatabase 가 없어서 스킬 배정을 건너뜁니다. 씬에 SkillDatabase 를 배치하세요.");
            }

            allies.Add(fellow);
        }

        // 카드 풀 생성 → 덱 구성 → GameManager 에 주입
        var pool = GenerateCardPool();
        var deck = DeckBuilder.BuildPartyDeck(companions, pool);
        GameManager.Instance.InjectDeck(deck);
        Debug.Log($"[BattleManager] 덱 주입 완료: {deck.Count}장");
    }

    // ----------------------------------------------------------
    // 메인 루프 — 페이즈를 순서대로 실행
    // ----------------------------------------------------------

    /// <summary>전투 종료 전까지 페이즈를 반복 실행하는 코루틴</summary>
    private IEnumerator BattleLoop()
    {
        while (currentPhase != BattlePhase.BattleEnd)
            yield return StartCoroutine(ExecutePhase(currentPhase));
    }

    /// <summary>현재 페이즈에 맞는 핸들러를 실행한다</summary>
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
    

    // -------------------------------------------------------
    // 공개 API
    // ----------------------------------------------------------

    /// <summary>
    /// UI 버튼을 눌러 플레이어 턴을 종료할 때 호출한다.
    /// GameManager.EndMyTurn() 에서도 호출됩니다.
    /// </summary>
    public void FinishPlayerTurn()
    {
        if (currentPhase == BattlePhase.PlayerCardPlay)
        {
            isPlayerTurnFinishing = true;
            Debug.Log("[BattleManager] 플레이어 턴 종료 신호 수신.");
        }
    }

    /// <summary>
    /// Inspector 에서 CardData SO 를 직접 사용하는 경로.
    /// stackDelta 로 역할별 스택에 반영한다.
    /// </summary>
    public void PlayCardOnStack(CardData cardData, StackType targetStack)
    {
        PlayerRoleCost.Instance.Add(targetStack, cardData.stackDelta);
        Debug.Log($"[BattleManager] 카드 사용됨: id={cardData.id} type={targetStack} delta={cardData.stackDelta:+#;-#;0}");
    }

    /// <summary>
    /// StackCardController → BattleManager 직접 반영 경로 (GameManager.OnCardUsed에서 호출).
    /// </summary>

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
}

// ============================================================
// EnemyEntity — 적 런타임 엔티티 데이터 (직렬화 가능)
// ============================================================

/// <summary>
/// 런타임 적 엔티티 데이터.
/// Inspector 에서 enemies 리스트에 직접 입력할 수 있습니다.
/// </summary>
[System.Serializable]
public class EnemyEntity
{
    [Tooltip("적 이름")]
    public string enemyName = "테스트 몬스터";

    [Tooltip("현재 HP")]
    public int currentHp = 100;

    [Tooltip("공격력 — 아군에게 가하는 데미지")]
    public int attackPower = 10;

    [Tooltip("사망 여부")]
    public bool isDead = false;
}
