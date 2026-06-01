// ============================================================
// Tutorial/TutorialGuidePanel.cs
// 튜토리얼 모달 다이얼로그 패널 (진행형)
// ============================================================
//
// [동작 변경 — 2026-05-29 모달 전환]
//   기존: 하단 고정 패널 + 자동 단계 진행
//   현재: 중앙 모달 — 외부 호출자가 시점에 맞춰 Show(message) 로 1회씩 표시
//
// [모달 동작]
//   - 표시 중에는 CanvasGroup blocksRaycasts=true 로 다른 입력 차단
//   - 사용자가 [확인] 클릭하면 닫힘
//   - [메인 메뉴로] 클릭하면 튜토리얼 완료 처리 + GameStartScene 복귀
//
// [인스펙터 슬롯]
//   - canvasGroup     : CanvasGroup (자동 GetComponent 폴백)
//   - messageText     : 메시지 TMP_Text
//   - nextButton      : 확인 버튼
//   - skipButton      : 메인 메뉴로 돌아가기 버튼
//   - mainMenuSceneName: GameStartScene 등 메뉴 씬 이름
// ============================================================

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class TutorialGuidePanel : MonoBehaviour
{
    [Header("UI 슬롯")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text    messageText;
    [SerializeField] private Button      nextButton;
    [SerializeField] private Button      skipButton;

    [Header("하이라이트 (외곽선 박스 — 기획 §4-2)")]
    [Tooltip("강조용 외곽선 프레임. 다이얼로그 대상 위로 이동/리사이즈됨.")]
    [SerializeField] private RectTransform highlightBox;
    [Tooltip("손패(좌측 카드 영역) — CombatIntro / ResultIntro 에서 강조.")]
    [SerializeField] private RectTransform handTarget;
    [Tooltip("적 카드 영역 — EnemyTurn / Elite / Boss 에서 강조 (선택).")]
    [SerializeField] private RectTransform enemyTarget;
    [Tooltip("외곽선이 대상보다 바깥으로 얼마나 더 커질지(px).")]
    [SerializeField] private float highlightPadding = 12f;

    [Header("씬 이름")]
    [SerializeField] private string mainMenuSceneName = "GameStartScene";

    /// <summary>씬 안의 유일 인스턴스 — TutorialManager 가 정적 호출.</summary>
    public static TutorialGuidePanel Instance { get; private set; }

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[TutorialGuidePanel] 중복 인스턴스 — 새로 등록");
        }
        Instance = this;
    }

    private void Start()
    {
        if (nextButton != null) nextButton.onClick.AddListener(OnConfirmClicked);
        if (skipButton != null) skipButton.onClick.AddListener(OnMainMenuClicked);
        if (highlightBox != null) highlightBox.gameObject.SetActive(false);
        SetVisible(false); // 시작은 숨김 — 호출자가 Show 로 표시
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>모달 표시 — 하이라이트 없음(호환용).</summary>
    public void Show(string message) => Show(message, (TutorialManager.DialogueId)(-1));

    /// <summary>모달 표시 — 메시지 갱신 + CanvasGroup 활성 + 다이얼로그별 외곽선 강조. 튜토리얼 모드가 아니면 무시.</summary>
    public void Show(string message, TutorialManager.DialogueId id)
    {
        if (TutorialManager.Instance == null || !TutorialManager.Instance.IsTutorial) return;
        if (messageText != null) messageText.text = message;
        SetVisible(true);
        ApplyHighlight(id);
    }

    /// <summary>모달 숨김 + 외곽선 숨김.</summary>
    public void Hide()
    {
        SetVisible(false);
        if (highlightBox != null) highlightBox.gameObject.SetActive(false);
    }

    // ── 하이라이트 (외곽선 박스) ──────────────────────────────────
    /// <summary>다이얼로그 id 에 매핑된 대상 위로 외곽선 박스를 이동/표시. 대상 없으면 박스 숨김(모달만).</summary>
    private void ApplyHighlight(TutorialManager.DialogueId id)
    {
        if (highlightBox == null) return;
        RectTransform target = ResolveTarget(id);
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            highlightBox.gameObject.SetActive(false);
            return;
        }
        highlightBox.gameObject.SetActive(true);
        PlaceHighlight(target, highlightPadding);
    }

    /// <summary>다이얼로그 → 강조 대상 매핑. 미지정은 null(모달만).</summary>
    private RectTransform ResolveTarget(TutorialManager.DialogueId id)
    {
        switch (id)
        {
            case TutorialManager.DialogueId.CombatIntro:    return handTarget;
            case TutorialManager.DialogueId.ResultIntro:    return handTarget;
            case TutorialManager.DialogueId.EnemyTurnIntro: return enemyTarget;
            case TutorialManager.DialogueId.EliteIntro:     return enemyTarget;
            case TutorialManager.DialogueId.BossIntro:      return enemyTarget;
            default:                                        return null;
        }
    }

    /// <summary>
    /// 외곽선 박스를 대상 RectTransform 위로 정확히 배치.
    /// 스크린좌표를 거쳐 캔버스 스케일/렌더모드와 무관하게 동작 (오버레이=null 카메라).
    /// highlightBox 는 중앙 앵커(0.5,0.5)·중앙 피벗, 풀스크린 부모 아래에 두는 것을 전제로 한다.
    /// </summary>
    private void PlaceHighlight(RectTransform target, float pad)
    {
        RectTransform parent = highlightBox.parent as RectTransform;
        if (parent == null) return;

        Camera cam = null;
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = canvas.worldCamera;

        Vector3[] wc = new Vector3[4];
        target.GetWorldCorners(wc); // 0=BL, 2=TR (월드)

        Vector2 blLocal, trLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent, RectTransformUtility.WorldToScreenPoint(cam, wc[0]), cam, out blLocal);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent, RectTransformUtility.WorldToScreenPoint(cam, wc[2]), cam, out trLocal);

        Vector2 size = new Vector2(Mathf.Abs(trLocal.x - blLocal.x), Mathf.Abs(trLocal.y - blLocal.y))
                       + Vector2.one * (pad * 2f);
        highlightBox.anchoredPosition = (blLocal + trLocal) * 0.5f;
        highlightBox.sizeDelta = size;
    }

    /// <summary>CanvasGroup 토글 — alpha + interactable + blocksRaycasts 일괄.</summary>
    private void SetVisible(bool visible)
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha          = visible ? 1f : 0f;
        canvasGroup.interactable   = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    private void OnConfirmClicked()
    {
        AudioManager.Instance?.PlaySfxById(SfxId.Confirm);
        Hide();
    }

    private void OnMainMenuClicked()
    {
        AudioManager.Instance?.PlaySfxById(SfxId.Confirm);
        if (TutorialManager.Instance != null) TutorialManager.Instance.EndTutorial(markComplete: true);
        Debug.Log("[TutorialGuidePanel] [메인 메뉴로] 클릭 — 메뉴 복귀");
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
