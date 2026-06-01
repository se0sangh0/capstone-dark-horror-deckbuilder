// ============================================================
// Tutorial/TutorialManager.cs
// 튜토리얼 진입/진행/완료 상태 관리 — 싱글톤
// ============================================================
//
// [기획 §15_튜토리얼_명세]
//   - 최초 실행 → 메인 메뉴 → 튜토리얼 자동 진입
//   - 재실행 → 메인 메뉴 [처음이신가요?] 버튼으로 재진입 가능
//   - 진행: 4단계 (드로우 / 스킬 자동 / 적 타격 / 미행동 보상)
//   - 종료: 스킵 또는 고블린 처치 → PlayerPrefs 완료 플래그 = 1
//
// [PlayerPrefs 키]
//   "tutorial_completed" : 0 = 미완료 / 1 = 완료
//
// [사용 흐름]
//   - MoveScene 의 게임 시작 시: TutorialManager.IsCompleted() 체크 후 분기
//   - PartyManager / EnemySpawner: TutorialManager.Instance?.IsTutorial 분기
//   - TutorialGuidePanel: CurrentStep 으로 단계별 메시지 표시
// ============================================================

using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    public const string PrefsKey = "tutorial_completed";

    /// <summary>현재 튜토리얼 모드 진행 중인지. PartyManager/EnemySpawner 등이 분기에 사용.</summary>
    public bool IsTutorial { get; private set; }

    /// <summary>현재 단계 (0~3). 가이드 패널이 텍스트 결정에 사용.</summary>
    public int CurrentStep { get; private set; }

    public const int TotalSteps = 4;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬 전환 사이에도 유지
    }

    // ── PlayerPrefs ──────────────────────────────────────────────
    /// <summary>튜토리얼 완료 플래그가 true 인지.</summary>
    public static bool IsCompleted() => PlayerPrefs.GetInt(PrefsKey, 0) == 1;

    /// <summary>튜토리얼 완료 플래그를 true 로 저장.</summary>
    public static void MarkCompleted()
    {
        PlayerPrefs.SetInt(PrefsKey, 1);
        PlayerPrefs.Save();
        Debug.Log("[TutorialManager] 튜토리얼 완료 플래그 저장됨.");
    }

    /// <summary>플래그 리셋 (디버그/QA 용). 다음 실행 시 다시 튜토리얼 자동 진입.</summary>
    public static void ResetCompletedFlag()
    {
        PlayerPrefs.DeleteKey(PrefsKey);
        PlayerPrefs.Save();
        Debug.Log("[TutorialManager] 튜토리얼 플래그 리셋됨.");
    }

    // ── 진행 제어 ────────────────────────────────────────────────
    /// <summary>튜토리얼 모드 시작. 다음 씬 진입 전 호출.</summary>
    public void StartTutorial()
    {
        IsTutorial  = true;
        CurrentStep = 0;
        Debug.Log("[TutorialManager] 튜토리얼 시작");
    }

    /// <summary>다음 단계로 진행. CurrentStep++.</summary>
    public void NextStep()
    {
        CurrentStep++;
        OnStepAdvanced?.Invoke();
        Debug.Log($"[TutorialManager] 단계 진행 → {CurrentStep}/{TotalSteps}");
    }

    /// <summary>특정 단계 인덱스 보다 작거나 같을 때만 진행 — 자동 트리거가 중복 호출돼도 안전.</summary>
    public void TryAdvanceTo(int stepIndex)
    {
        if (!IsTutorial) return;
        if (CurrentStep > stepIndex) return; // 이미 지난 단계
        if (CurrentStep == stepIndex) NextStep(); // 현재 = 이 단계 → 다음으로
        // CurrentStep < stepIndex 면 사용자가 더 빠른 단계를 보고 있는 중 — 통과
    }

    /// <summary>단계가 바뀔 때 GuidePanel 이 메시지 갱신용으로 구독.</summary>
    [System.NonSerialized] public System.Action OnStepAdvanced;

    // ── 진행형 모달 다이얼로그 시퀀스 (기획 §15 + 2026-05-29 모달 전환) ──
    /// <summary>다이얼로그 id. 한 번 표시되면 _shownDialogues 에 저장되어 같은 세션 동안 재표시 안 됨.</summary>
    public enum DialogueId
    {
        NodeMapIntro    = 0,   // 첫 노드맵 표시 시
        CombatIntro     = 1,   // 첫 전투 진입 (카드 사용 + 턴 종료 안내)
        EnemyTurnIntro  = 2,   // 적 행동 페이즈 첫 진입
        ResultIntro     = 3,   // 결과 처리 첫 진입 (미행동 보상)
        CombatVictory   = 4,   // 첫 전투 승리 → 다음 노드 안내
        ShopIntro       = 5,
        ChurchIntro     = 6,
        EliteIntro      = 7,
        BossIntro       = 8,
    }

    private static readonly string[] DialogueMessages = new[]
    {
        // 0 — NodeMapIntro
        "환영합니다.\n\n화면 좌측의 노드맵에서 다음 노드를 클릭해 진행해보세요.",

        // 1 — CombatIntro
        "전투 노드입니다.\n\n손패의 카드를 클릭해 스택을 채우고, [턴 종료] 버튼을 눌러 진행하세요.\n동료들이 스택에 맞춰 자동으로 스킬을 사용합니다.",

        // 2 — EnemyTurnIntro
        "적도 자신의 차례에 행동합니다.\n\nHP / 스트레스 변화를 확인하세요.",

        // 3 — ResultIntro
        "이번 턴에 행동하지 않은 동료는 다음 턴에 스택 +1 보너스를 받고\n행동 순서가 우선됩니다. 단, 먼저 피격되는 리스크도 있어요.",

        // 4 — CombatVictory
        "전투 승리!\n\n노드맵에서 다음 노드를 클릭해 계속 진행하세요.",

        // 5 — ShopIntro (용병소)
        "용병소입니다. 각 버튼을 눌러 기능을 확인해 보세요.\n\n" +
        "[모집] — 새 동료 후보를 영입하거나 후보를 리롤(영혼석 소비)할 수 있어요.\n" +
        "[성장] — 같은 직업·성급 동료 3명을 합성해 별을 올려 강화합니다.\n" +
        "[나가기] — 다음 노드로 진행합니다.",

        // 6 — ChurchIntro (교회)
        "교회입니다. 영혼석으로 회복 기능을 사용하거나 사망 동료를 부활시킬 수 있어요.\n\n" +
        "[HP 회복] — 살아있는 동료의 HP 를 회복합니다 (영혼석 소비).\n" +
        "[스트레스 회복] — 동료의 스트레스를 감소시킵니다 (영혼석 소비).\n" +
        "[부활 카드] — 사망 동료 카드의 [부활] 버튼으로 파티에 복귀시킵니다.\n" +
        "[다음 층] — 다음 노드로 진행합니다.",

        // 7 — EliteIntro
        "엘리트 전투입니다.\n\n일반 고블린 대신 강력한 약탈자(攻 20 / HP 250)가 등장합니다.\n" +
        "일반 전투와 같은 방식으로 진행하되, 카드를 더 신중하게 사용하세요.",

        // 8 — BossIntro
        "마지막... 거두는 자입니다.\n\n보스는 매우 강력해 한 번에 처치하기 어렵습니다.\n" +
        "튜토리얼에서는 보스의 압도감을 체험하는 것이 목표예요. 행운을 빕니다."
    };

    private readonly System.Collections.Generic.HashSet<DialogueId> _shownDialogues = new System.Collections.Generic.HashSet<DialogueId>();

    /// <summary>다이얼로그가 표시된 적이 있으면 true.</summary>
    public bool HasShown(DialogueId id) => _shownDialogues.Contains(id);

    /// <summary>
    /// 모달 다이얼로그 1회 표시 시도. 이미 보였거나 튜토리얼 모드 아니면 무시.
    /// TutorialDialogPopup 이 활성 상태여야 모달 표시됨 (씬 안에 인스턴스 필요).
    /// </summary>
    public void TryShowDialogue(DialogueId id)
    {
        if (!IsTutorial) return;
        if (_shownDialogues.Contains(id)) return;
        _shownDialogues.Add(id);

        int idx = (int)id;
        if (idx < 0 || idx >= DialogueMessages.Length) return;
        string msg = DialogueMessages[idx];

        var popup = TutorialGuidePanel.Instance;
        if (popup == null)
        {
            Debug.LogWarning($"[TutorialManager] TutorialGuidePanel 없음 — 다이얼로그 표시 실패 ({id})");
            return;
        }
        popup.Show(msg, id);
        Debug.Log($"[TutorialManager] 다이얼로그 표시: {id}");
    }

    public override string ToString() => $"IsTutorial={IsTutorial}, shown={_shownDialogues.Count}";

    /// <summary>튜토리얼 종료. markComplete=true 면 완료 플래그 저장.</summary>
    public void EndTutorial(bool markComplete = true)
    {
        IsTutorial  = false;
        CurrentStep = 0;
        if (markComplete) MarkCompleted();
        Debug.Log($"[TutorialManager] 튜토리얼 종료 (markComplete={markComplete})");
    }
}
