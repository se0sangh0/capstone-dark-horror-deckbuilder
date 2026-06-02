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
//       공개 API (FinishPlayerTurn)
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
using TMPro;
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

    /// <summary>
    /// 전투 중 새로운 적이 enemies 리스트에 추가됐을 때 발행.
    /// 시각 카드 spawn(DefaultSetting), 소환 이펙트, 사운드 등 구독자가 알아서 반응.
    /// </summary>
    public event System.Action<EnemyData> OnEnemySpawned;

    /// <summary>BattleManager 내부에서만 호출 (소환 등). 구독자 전체에 알림.</summary>
    internal void RaiseEnemySpawned(EnemyData spawned) => OnEnemySpawned?.Invoke(spawned);

    // ----------------------------------------------------------
    // [카드 풀]
    // ----------------------------------------------------------
    [Header("카드 풀 (Card Pool)")]
    [Tooltip("전투에 사용될 전체 카드 풀")]
    public List<CardData> allCards = new();

    // ----------------------------------------------------------
    // [타이밍 설정]
    // ----------------------------------------------------------
    [Header("타이밍 설정 (Timers)")]

    [Tooltip("행동 실행 후 대기 시간 (초). 카드 사이 짧은 텀.")]
    public float actionDelayTime = 0.5f;

    [Tooltip("스킬 발동(OnSkillCast) → 데미지/힐/실드 적용 사이 대기. 모션의 dashForward + holdAttack 합과 동일하게 맞춤. 기본 1.25초.")]
    public float impactDelay = 1.25f;

    [Tooltip("10층 보스 클리어 시 표시할 엔딩 패널. 미할당 시 콘솔 로그만.")]
    [SerializeField] private GameObject endingPanel;

    [Tooltip("엔딩 패널 표시 후 GameStartScene 복귀까지 대기 시간 (초)")]
    [SerializeField] private float endingDisplayDuration = 4.0f;

    [Tooltip("기획 §04 §51~99 압박 — 스킬 퍼포먼스 감소율 (% 단위, 기준 -5~-15)")]
    [Range(0f, 50f)]
    [SerializeField] private float pressureSkillPenaltyPercent = 10f;

    [Tooltip("기획 §04 §51~99 압박 — 피격 시 받는 스트레스 추가 증가율 (% 단위, 기본 +10)")]
    [Range(0f, 50f)]
    [SerializeField] private float pressureStressGainPercent = 10f;

    [Tooltip("현재 턴 번호 표시용 TMP 텍스트 (예: show_turn). 미할당 시 표시 생략.")]
    [SerializeField] private TMP_Text turnDisplayText;

    // 현재 턴 번호 — InitBattle 에서 0 으로 초기화, 매 DrawPhase 진입 시 ++.
    public int CurrentTurn { get; private set; } = 0;

    // ── 턴 표시 ────────────────────────────────────────────
    public void AdvanceTurnCounter()
    {
        CurrentTurn++;
        UpdateTurnDisplay();
    }

    private void UpdateTurnDisplay()
    {
        if (turnDisplayText != null)
            turnDisplayText.text = $"턴 {CurrentTurn}";
    }

    [Tooltip("턴 종료 후 다음 턴 시작 전 대기 시간 (초)")]
    public float turnTransitionDelay = 1.0f;

    [Tooltip("게임 오버 연출 대기 시간 (초)")]
    public float gameOverDelay = 1.5f;

    // ----------------------------------------------------------
    // [탈진 페널티]
    // 기획 §02_전투_시스템_명세 §1) Hand Empty — "데미지 또는 스트레스" (OR)
    // 기획 §03_카드_설계_프레임 §탈진 — "1차 밸런스 단계 예정"
    // → OR 해석에 따라 스트레스만 채택. 수치는 임시값(밸런스 후 재조정).
    // 변수명은 기획 미명시 → 임의 (exhaustionStressPenalty).
    // ----------------------------------------------------------
    [Header("탈진 페널티 (Exhaustion — 임시값)")]
    [Tooltip("덱 고갈 + 손패 모두 사용한 턴의 결과 처리 시 살아있는 동료 전원에게 적용되는 스트레스 증가량")]
    public int exhaustionStressPenalty = 5;

    // ----------------------------------------------------------
    // [내부 상태]
    // ----------------------------------------------------------

    /// <summary>플레이어가 턴 종료를 눌렀는지 여부</summary>
    private bool isPlayerTurnFinishing = false;

    /// <summary>이번 전투에서 선공 판정이 완료되었는지 여부 (매 전투 1회만 판정)</summary>
    private bool _initiativeDecided = false;

    /// <summary>스택 부족으로 스킵한 역할의 이월 보너스 (턴 종료 시 다음 턴으로 전달)</summary>
    private readonly Dictionary<StackType, int> _carryoverBonus = new();

    /// <summary>
    /// 이번 턴 미행동한 동료들 — 다음 턴 결과 처리 시 allies 리스트의 앞으로 재정렬.
    /// 기획 §코어루프 §동료 행동 — "예시: 1-2-3-4 → 3 미행동 → 다음 턴 3-1-2-4"
    /// 복수 미행동 시 미행동 발생 순서대로 앞에 stable 정렬 (기획 미명시 → 자연 확장).
    /// </summary>
    private readonly List<FellowData> _carryoverOrderList = new();

    /// <summary>InitBattle 정보 로그를 첫 전투에서만 출력하기 위한 플래그</summary>
    private static bool _firstInitLogged = false;


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
        // 엔딩 패널은 보스 클리어 시에만 표시 — 진입 시 무조건 비활성화 (부모 PopUp 활성 시 자동 노출 방지)
        if (endingPanel != null) endingPanel.SetActive(false);

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
		foreach (StackType role in System.Enum.GetValues(typeof(StackType)))
    		PlayerRoleCost.Instance.SetAmount(role, 0);

        if (PartyManager.Instance == null)
        {
            Debug.LogError("[BattleManager] PartyManager 가 없습니다! 씬에 PartyManager 를 배치하세요.");
            return;
        }

        // FellowData 인스턴스를 새로 만들지 않고 PartyManager 의 것을 재사용한다.
        // → 스킬이 이미 배정된 동료는 기존 스킬을 유지한다.
        var fellows = PartyManager.Instance.GetActiveFellows();
        if (!_firstInitLogged) Debug.Log($"[BattleManager] 전투 시작 — 동료 {fellows.Count}명");

        _initiativeDecided = false;
        _carryoverBonus.Clear();
        CurrentTurn = 0;
        UpdateTurnDisplay();

        allies.Clear();
        foreach (var fellow in fellows)
        {
            if (fellow == null) continue;

            // [통합] FellowData 자체에 정의 데이터가 흡수되어 data null 체크 불필요.
			
            // ── 상태 초기화 — 스킬·HP·스트레스는 유지, 일시 상태(실드/패닉)만 리셋 ──
            // (HP/스트레스는 기획상 화톳불 노드에서만 회복되므로 전투 진입 때 리셋 안 함)
            fellow.isDead          = false;
            // fellow.currentStress = 0;   ← 매 전투 리셋 안 함 (스트레스 유지)
            fellow.shield          = 0;
            fellow.isFrozen        = false;
            fellow.isOverBreathing = false;
            fellow.hasSevereDebuff = false;   // 역할별 중증 디버프 — 전투마다 리셋 (기획 §04)
            fellow.comboTargetIid  = 0;       // 거합 집중 콤보 — 전투마다 리셋 (기획 §16)
            fellow.comboStacks     = 0;
            // 메타 패시브 — 런 시작 1회 배정 (미배정이면 해금된 풀에서 무작위). 인스턴스 유지.
            if (string.IsNullOrEmpty(fellow.activePassiveId))
            {
                fellow.activePassiveId = MetaPassiveManager.RollPassive(fellow.jobClass);
                if (!string.IsNullOrEmpty(fellow.activePassiveId))
                    Debug.Log($"[MetaPassive] {fellow.jobClass} 패시브 배정: {MetaPassiveManager.NameOf(fellow.activePassiveId)}");
            }
            fellow.positionStack   = (StackType)(int)fellow.role;

            // ── 성급 배율 (기획 백로그 §5 성급 설계안) ─────────────────
            // starLevel / maxHp 는 FellowDatabase.CreateRuntimeFellow() 또는 승급 시
            // 이미 올바른 값으로 설정되어 있다. 여기서는 배율 정보만 동기화.
            // → maxHp 에 배율을 다시 곱하면 이중 스케일링 발생 → 절대 금지.
            //
            //  데미지 배율 1.25^(star-1) → UseSkill 에서 skill.power 에 곱해짐
            //  체력 배율  1.4^(star-1)  → maxHp 에 이미 곱해져 있음 (정보용으로 동기화만)
            fellow.skillPowerMultiplier = UnityEngine.Mathf.Pow(1.25f, fellow.starLevel - 1);
            fellow.hpMultiplier         = UnityEngine.Mathf.Pow(1.4f,  fellow.starLevel - 1);

            // HP 유지 — 매 전투마다 풀 회복하면 안 됨 (이전 전투 HP 유지가 정상)
            // CurrentHp 가 0(미초기화) 일 때만 maxHp 로 시작, 이미 값 있으면 그대로
            if (fellow.CurrentHp <= 0)
                fellow.CurrentHp = fellow.maxHp;

            // 스프라이트 로드
            if (!string.IsNullOrEmpty(fellow.spritePath) && fellow.fellowSprite == null)
            {
                fellow.fellowSprite = Resources.Load<Sprite>(fellow.spritePath);
                if (fellow.fellowSprite == null)
                    Debug.LogWarning($"[BattleManager] 스프라이트 없음: {fellow.spritePath}");
            }

            // ── 스킬 배정 — 이미 있으면 그대로 유지 (전투 간 고정) ──
            if (SkillDatabase.Instance != null)
            {
                if (!fellow.HasSkills)
                {
                    // 최초 배정 — 풀(직업 4개)에서 해금된 것 중 2개 랜덤 (기획 §10/§16).
                    //   FellowDatabase.CreateRuntimeFellow 와 동일 헬퍼로 통일 — 로직 분기 방지.
                    string[] ids = SkillDatabase.Instance.PickSkillsFromPool(fellow.skillIds, fellow.positionStack, 2);
                    fellow.AssignSkills(ids);
                }
                else
                {
                    if (!_firstInitLogged) Debug.Log($"[BattleManager] {fellow.displayName} — 기존 스킬 유지 (재배정 없음)");
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
            if (!_firstInitLogged) Debug.Log($"[BattleManager] 적 런타임 인스턴스 생성: {runtimeEnemy.displayName} | HP:{runtimeEnemy.CurrentHp}/{runtimeEnemy.maxHp}");
        }

        // 카드 풀 생성 → 덱 구성 → GameManager 에 주입
        var partyFellows = PartyManager.Instance.GetActiveFellows();
        var pool = GenerateCardPool();
        var deck = DeckBuilder.BuildPartyDeck(partyFellows, pool);
        GameManager.Instance.InjectDeck(deck);
        if (!_firstInitLogged) Debug.Log($"[BattleManager] 덱 주입 완료: {deck.Count}장");

        // 첫 전투 정보 로그는 1회만 출력 (반복 노이즈 제거)
        _firstInitLogged = true;
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
