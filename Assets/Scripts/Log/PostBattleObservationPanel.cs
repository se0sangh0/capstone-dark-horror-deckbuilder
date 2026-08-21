// ============================================================
// Log/PostBattleObservationPanel.cs
// 전투 후 현장 관찰 표시 오버레이 (P0-03)
// ============================================================
//
// [이 파일이 하는 일]
//   지정 인카운터의 전투 승리 뒤 현장 관찰을 하이라이트로 표시합니다.
//   제목 + 두 문장 이내 본문 + 단일 계속 입력.
//
// [계약 — 16-A §2 핵심 현장 단서 / 16-B §3·§6]
//   - 표시일 뿐이다: 선택지·보상·추가 전투·재화·상태 변화를 만들지 않는다.
//   - 기록(BattleResolved 사후 관찰 필드)은 표시 전에 이미 생성돼 있다.
//     이 패널은 결과·재화·기록을 직접 적용하지 않는다.
//   - 계속 버튼·빈 공간 클릭·스페이스 키 모두 진행(미선택 스킵 허용).
//     연타·재표시해도 콜백은 1회 — 기록과 결과를 다시 만들지 않는다.
//
// 사용:
//   PostBattleObservationPanel.Show(obs.title, obs.screenText, onContinue);
//   - 씬 배치 불필요(자체 생성, DontDestroyOnLoad). BattleResultScreen 과 같은 패턴.
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class PostBattleObservationPanel : MonoBehaviour
{
    public static PostBattleObservationPanel Instance { get; private set; }

    private CanvasGroup _group;
    private GameObject  _root;
    private TMP_Text    _titleText, _bodyText;
    private System.Action _onContinue;

    private static readonly Color DimColor   = new Color(0f, 0f, 0f, 0.55f);
    private static readonly Color BoxBg      = new Color(0.10f, 0.09f, 0.11f, 0.97f);
    private static readonly Color BoxBorder  = new Color(0.72f, 0.62f, 0.38f, 1f); // 하이라이트 테두리
    private static readonly Color TitleColor = new Color(0.93f, 0.88f, 0.72f, 1f);
    private static readonly Color BodyColor  = new Color(0.88f, 0.86f, 0.82f, 1f);
    private static readonly Color BtnBg      = new Color(0.20f, 0.17f, 0.12f, 0.95f);

    /// <summary>현장 관찰을 표시한다. 계속 입력 시 onContinue 가 정확히 1회 호출된다.</summary>
    public static void Show(string title, string body, System.Action onContinue)
    {
        Ensure();
        Instance._Show(title, body, onContinue);
    }

    private static void Ensure()
    {
        if (Instance != null) return;
        new GameObject("PostBattleObservationPanel").AddComponent<PostBattleObservationPanel>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Build();
    }

    private void Update()
    {
        // 스페이스 키 = 계속 (표시 중일 때만)
        if (_group != null && _group.blocksRaycasts
            && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Proceed();
        }
    }

    private void _Show(string title, string body, System.Action onContinue)
    {
        _onContinue = onContinue;
        if (_titleText != null) _titleText.text = title ?? "";
        if (_bodyText  != null) _bodyText.text  = body ?? "";
        _root.SetActive(true);
        _group.alpha = 1f;
        _group.blocksRaycasts = true;
    }

    // 진행 — 버튼·빈 공간·스페이스가 같은 경로. activeSelf 가드로 중복 발화 차단.
    private void Proceed()
    {
        if (_root == null || !_root.activeSelf) return;
        var cb = _onContinue;
        _onContinue = null;
        Hide();
        cb?.Invoke();
    }

    private void Hide()
    {
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
        _root.SetActive(false);
    }

    // ── UI 구성 (런타임) ────────────────────────────────────────
    private void Build()
    {
        var canvasGo = new GameObject("ObservationCanvas", typeof(Canvas), typeof(CanvasGroup), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000; // BattleResultScreen(9990) 위 — 승리 팝업 다음 순서로 표시됨
        _group = canvasGo.GetComponent<CanvasGroup>();
        _group.alpha = 0f;
        _group.blocksRaycasts = false;

        var font = TMP_Settings.defaultFontAsset;

        _root = new GameObject("Observation", typeof(RectTransform));
        _root.transform.SetParent(canvasGo.transform, false);
        Stretch((RectTransform)_root.transform);

        // 딤 — 빈 공간 클릭도 계속 처리 (미선택 스킵 허용)
        var dim = NewImage("Dim", _root.transform);
        Stretch(dim.rectTransform);
        dim.color = DimColor;
        dim.raycastTarget = true;
        var dimBtn = dim.gameObject.AddComponent<Button>();
        dimBtn.transition = Selectable.Transition.None;
        dimBtn.onClick.AddListener(Proceed);

        // 중앙 박스 — 하이라이트 테두리
        var box = NewImage("Box", _root.transform);
        var brt = box.rectTransform;
        brt.anchorMin = brt.anchorMax = brt.pivot = new Vector2(0.5f, 0.5f);
        brt.sizeDelta = new Vector2(720f, 320f);
        box.color = BoxBg;
        box.raycastTarget = true; // 박스 클릭이 딤 버튼으로 새지 않게
        var outline = box.gameObject.AddComponent<Outline>();
        outline.effectColor     = BoxBorder;
        outline.effectDistance  = new Vector2(3f, 3f);
        outline.useGraphicAlpha = false;

        // 제목
        _titleText = NewText("Title", box.transform, font, 32f, TitleColor, FontStyles.Bold);
        var trt = (RectTransform)_titleText.transform;
        trt.anchorMin = new Vector2(0f, 1f); trt.anchorMax = new Vector2(1f, 1f);
        trt.pivot = new Vector2(0.5f, 1f);
        trt.anchoredPosition = new Vector2(0f, -24f);
        trt.sizeDelta = new Vector2(-56f, 48f);
        _titleText.alignment = TextAlignmentOptions.Center;

        // 본문 (두 문장 이내)
        _bodyText = NewText("Body", box.transform, font, 25f, BodyColor, FontStyles.Normal);
        var bort = (RectTransform)_bodyText.transform;
        bort.anchorMin = new Vector2(0f, 1f); bort.anchorMax = new Vector2(1f, 1f);
        bort.pivot = new Vector2(0.5f, 1f);
        bort.anchoredPosition = new Vector2(0f, -86f);
        bort.sizeDelta = new Vector2(-64f, 140f);
        _bodyText.alignment = TextAlignmentOptions.Top;
        _bodyText.enableWordWrapping = true;

        // 단일 계속 버튼
        var btnGo = new GameObject("ContinueButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        btnGo.transform.SetParent(box.transform, false);
        var btnRt = (RectTransform)btnGo.transform;
        btnRt.anchorMin = new Vector2(0.5f, 0f); btnRt.anchorMax = new Vector2(0.5f, 0f);
        btnRt.pivot = new Vector2(0.5f, 0f);
        btnRt.anchoredPosition = new Vector2(0f, 22f);
        btnRt.sizeDelta = new Vector2(200f, 56f);
        btnGo.GetComponent<Image>().color = BtnBg;
        btnGo.AddComponent<Button>().onClick.AddListener(Proceed);

        var label = NewText("Label", btnGo.transform, font, 26f, TitleColor, FontStyles.Normal);
        Stretch((RectTransform)label.transform);
        label.text = "계속";
        label.alignment = TextAlignmentOptions.Center;

        _root.SetActive(false);
    }

    // ── 헬퍼 (BattleResultScreen 과 동일 스타일) ──
    private static Image NewImage(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        return go.GetComponent<Image>();
    }

    private static TMP_Text NewText(string name, Transform parent, TMP_FontAsset font, float size, Color color, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.fontSize = size; t.color = color; t.fontStyle = style;
        t.enableWordWrapping = false;
        t.raycastTarget = false;
        return t;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }
}
