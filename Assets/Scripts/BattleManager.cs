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
using System.Linq;
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
    public List<EnemyData> enemies = new();

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

    /// <summary>이번 전투에서 선공 판정이 완료되었는지 여부 (매 전투 1회만 판정)</summary>
    private bool _initiativeDecided = false;

    /// <summary>스택 부족으로 스킵한 역할의 이월 보너스 (턴 종료 시 다음 턴으로 전달)</summary>
    private readonly Dictionary<StackType, int> _carryoverBonus = new();


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
        // 전투 재진입 시 이전 전투의 BattleEnd 상태가 남지 않도록 페이즈를 리셋
        currentPhase = BattlePhase.DrawPhase;

        if (PartyManager.Instance == null)
        {
            Debug.LogError("[BattleManager] PartyManager 가 없습니다! 씬에 PartyManager 를 배치하세요.");
            return;
        }

        // FellowData 인스턴스를 새로 만들지 않고 PartyManager 의 것을 재사용한다.
        // → 스킬이 이미 배정된 동료는 기존 스킬을 유지한다.
        var fellows = PartyManager.Instance.GetActiveFellows();
        Debug.Log($"[BattleManager] 전투 시작 — 동료 {fellows.Count}명");

        _initiativeDecided = false;
        _carryoverBonus.Clear();

        allies.Clear();
        foreach (var fellow in fellows)
        {
            if (fellow == null) continue;

            // CompanionData 가 없으면 전투에서 제외
            if (fellow.data == null)
            {
                Debug.LogWarning($"[BattleManager] {fellow.name}: data(CompanionData) 가 null — 전투에서 제외됨.");
                continue;
            }

            // ── 상태 초기화 (HP·사망·스트레스·패닉) — 스킬은 유지 ──
            fellow.isDead          = false;
            fellow.currentStress   = 0;
            fellow.shield          = 0;
            fellow.isFrozen        = false;
            fellow.isOverBreathing = false;
            fellow.stressResist    = fellow.data.stressResist;
            fellow.positionStack   = (StackType)(int)fellow.data.role;

            // ── [강화 시스템 TODO] 성급 초기화 ──────────────────────
            // data.starLevel / data.maxHp 는 FellowDatabase.CreateCompanionData()
            // 또는 UpgradeStar() 에서 이미 올바른 값으로 설정되어 있다.
            // → 여기서는 런타임 필드를 data 에서 동기화만 한다.
            // → maxHp 에 배율을 다시 곱하면 이중 스케일링 발생 → 절대 금지.
            //
            // 스킬 파워 배율 (UseSkill 에서 사용):
            //   1★ → ×1.00   2★ → ×1.50   3★ → ×2.25
            fellow.starLevel            = fellow.data.starLevel;
            fellow.skillPowerMultiplier = UnityEngine.Mathf.Pow(1.5f, fellow.starLevel - 1);

            // HP 는 data.maxHp (이미 성급 배율 반영) 를 그대로 사용
            fellow.CurrentHp = fellow.data.maxHp;

            // 스프라이트 로드
            if (!string.IsNullOrEmpty(fellow.data.spritePath) && fellow.fellowSprite == null)
            {
                fellow.fellowSprite = Resources.Load<Sprite>(fellow.data.spritePath);
                if (fellow.fellowSprite == null)
                    Debug.LogWarning($"[BattleManager] 스프라이트 없음: {fellow.data.spritePath}");
            }

            // ── 스킬 배정 — 이미 있으면 그대로 유지 (전투 간 고정) ──
            if (SkillDatabase.Instance != null)
            {
                if (!fellow.HasSkills)
                {
                    // 최초 배정: 역할에 맞는 스킬 2개를 랜덤으로 선택
                    var ids = SkillDatabase.Instance.AssignRandomSkills(fellow.positionStack, 2);
                    fellow.AssignSkills(ids);
                }
                else
                {
                    // 이미 배정된 스킬 유지 — Inspector 표시만 갱신
                    Debug.Log($"[BattleManager] {fellow.data.displayName} — 기존 스킬 유지 (재배정 없음)");
                    fellow.RefreshSkillInfo();
                }
            }
            else
            {
                Debug.LogWarning("[BattleManager] SkillDatabase 가 없어서 스킬 배정을 건너뜁니다. 씬에 SkillDatabase 를 배치하세요.");
            }

            allies.Add(fellow);
        }
        // ── enemies SO → 런타임 독립 인스턴스로 교체 ─────────────────
        // Inspector에서 같은 SO를 여러 슬롯에 넣어도
        // 각각 독립된 메모리 객체로 분리되어 HP가 공유되지 않음
        var originalEnemies = enemies.ToList();  // Inspector 원본 보존
        enemies.Clear();

        foreach (var original in originalEnemies)
        {
            if (original == null) continue;

            // SO 에셋을 복사해 런타임 전용 인스턴스 생성
            var runtimeEnemy = Instantiate(original);
            runtimeEnemy.name = original.name + "_runtime"; // Inspector에서 구분용
            runtimeEnemy.InitHp();                          // 복사본 HP 초기화

            enemies.Add(runtimeEnemy);
            Debug.Log($"[BattleManager] 적 런타임 인스턴스 생성: {runtimeEnemy.displayName} | HP:{runtimeEnemy.CurrentHp}/{runtimeEnemy.maxHp}");
        }

        // 카드 풀 생성 → 덱 구성 → GameManager 에 주입
        var companions = PartyManager.Instance.GetActiveCompanions();
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
