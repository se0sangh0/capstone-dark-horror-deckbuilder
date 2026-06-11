// ============================================================
// UI/BattleResultScreen.cs  (2026-06-11)
// 전투 결과 화면 — 승리 팝업(Panel_1 + 획득 재화 + 다음으로) / 패배 게임오버(전체 어둡게 + 빨강 볼드).
// ============================================================
//
// 사용:
//   BattleResultScreen.ShowVictory(영혼석, onNext);          // 다음으로 클릭 시 onNext (전투 보상=영혼석만, 기획 §15)
//   BattleResultScreen.ShowDefeat(onContinue);             // 클릭 시 onContinue
//   - 씬 배치 불필요(자체 생성, DontDestroyOnLoad). 최상단 오버레이.
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleResultScreen : MonoBehaviour
{
    public static BattleResultScreen Instance { get; private set; }

    private CanvasGroup _group;
    private GameObject  _victory, _defeat;
    private TMP_Text    _rewardText;
    private System.Action _onNext, _onContinue;
    private Coroutine _autoRoutine;

    // 클릭 없이도 자동 진행 (클릭하면 즉시 스킵) — 사용자 요청 2026-06-11
    private const float AutoAdvanceDelay = 2.5f;

    private static readonly Color Gold = new Color(1f, 0.84f, 0.4f, 1f);
    private static readonly Color Red  = new Color(0.86f, 0.12f, 0.12f, 1f);

    public static void ShowVictory(int soulstone, System.Action onNext)
    {
        Ensure(); Instance._ShowVictory(soulstone, onNext);
    }
    public static void ShowDefeat(System.Action onContinue)
    {
        Ensure(); Instance._ShowDefeat(onContinue);
    }

    private static void Ensure()
    {
        if (Instance != null) return;
        new GameObject("BattleResultScreen").AddComponent<BattleResultScreen>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Build();
    }

    private void Build()
    {
        var canvasGo = new GameObject("ResultCanvas", typeof(Canvas), typeof(CanvasGroup), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9990;
        _group = canvasGo.GetComponent<CanvasGroup>();
        _group.alpha = 0f; _group.blocksRaycasts = false;

        var font = TMP_Settings.defaultFontAsset;

        // ── 승리 ──────────────────────────────────────────────
        _victory = NewUI("Victory", canvasGo.transform); Stretch((RectTransform)_victory.transform);
        var vdim = NewImage("Dim", _victory.transform); Stretch(vdim.rectTransform); vdim.color = new Color(0f, 0f, 0f, 0.6f); vdim.raycastTarget = true;

        var panel = NewImage("Panel", _victory.transform);
        var prt = panel.rectTransform; prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0.5f); prt.pivot = new Vector2(0.5f, 0.5f); prt.sizeDelta = new Vector2(660, 460);
        var p1 = Resources.Load<Sprite>("UI/panel_1");
        if (p1 != null) { panel.sprite = p1; panel.type = Image.Type.Sliced; panel.color = Color.white; }
        else panel.color = new Color(0.12f, 0.12f, 0.16f, 0.98f);

        var vt = NewText("Title", panel.transform, "전투 승리", font, 50, Gold, FontStyles.Bold);
        var vtr = (RectTransform)vt.transform; vtr.anchorMin = new Vector2(0, 1); vtr.anchorMax = new Vector2(1, 1); vtr.pivot = new Vector2(0.5f, 1); vtr.sizeDelta = new Vector2(-40, 80); vtr.anchoredPosition = new Vector2(0, -44); vt.alignment = TextAlignmentOptions.Center;

        var rl = NewText("RewardLabel", panel.transform, "획득 재화", font, 28, new Color(0.82f, 0.82f, 0.9f), FontStyles.Normal);
        var rlr = (RectTransform)rl.transform; rlr.anchorMin = rlr.anchorMax = new Vector2(0.5f, 0.5f); rlr.pivot = new Vector2(0.5f, 0.5f); rlr.sizeDelta = new Vector2(560, 40); rlr.anchoredPosition = new Vector2(0, 50); rl.alignment = TextAlignmentOptions.Center;

        _rewardText = NewText("RewardAmount", panel.transform, "", font, 34, Gold, FontStyles.Bold);
        var rar = (RectTransform)_rewardText.transform; rar.anchorMin = rar.anchorMax = new Vector2(0.5f, 0.5f); rar.pivot = new Vector2(0.5f, 0.5f); rar.sizeDelta = new Vector2(580, 50); rar.anchoredPosition = new Vector2(0, -4); _rewardText.alignment = TextAlignmentOptions.Center;

        var nextBtn = NewButton("NextButton", panel.transform, "다음으로", font);
        var nbr = (RectTransform)nextBtn.transform.parent; // 버튼 컨테이너
        nextBtn.onClick.AddListener(ProceedVictory);

        // ── 패배(게임오버) ────────────────────────────────────
        _defeat = NewUI("Defeat", canvasGo.transform); Stretch((RectTransform)_defeat.transform);
        var ddim = NewImage("Dim", _defeat.transform); Stretch(ddim.rectTransform); ddim.color = new Color(0f, 0f, 0f, 0.9f); ddim.raycastTarget = true;
        var ddimBtn = ddim.gameObject.AddComponent<Button>(); ddimBtn.transition = Selectable.Transition.None;
        ddimBtn.onClick.AddListener(ProceedDefeat);

        var go = NewText("GameOver", _defeat.transform, "게임오버", font, 96, Red, FontStyles.Bold);
        var gor = (RectTransform)go.transform; gor.anchorMin = gor.anchorMax = new Vector2(0.5f, 0.5f); gor.pivot = new Vector2(0.5f, 0.5f); gor.sizeDelta = new Vector2(900, 160); gor.anchoredPosition = new Vector2(0, 20); go.alignment = TextAlignmentOptions.Center; go.raycastTarget = false;

        var hint = NewText("Hint", _defeat.transform, "잠시 후 계속됩니다 (클릭 시 즉시)", font, 26, new Color(0.7f, 0.7f, 0.7f), FontStyles.Normal);
        var hr = (RectTransform)hint.transform; hr.anchorMin = hr.anchorMax = new Vector2(0.5f, 0.5f); hr.pivot = new Vector2(0.5f, 0.5f); hr.sizeDelta = new Vector2(600, 40); hr.anchoredPosition = new Vector2(0, -90); hint.alignment = TextAlignmentOptions.Center; hint.raycastTarget = false;

        _victory.SetActive(false);
        _defeat.SetActive(false);
    }

    private void _ShowVictory(int soul, System.Action onNext)
    {
        _onNext = onNext;
        if (_rewardText != null) _rewardText.text = $"영혼석 +{soul}";
        _victory.SetActive(true); _defeat.SetActive(false);
        _group.alpha = 1f; _group.blocksRaycasts = true;
        RestartAuto(ProceedVictory);
    }

    private void _ShowDefeat(System.Action onContinue)
    {
        _onContinue = onContinue;
        _defeat.SetActive(true); _victory.SetActive(false);
        _group.alpha = 1f; _group.blocksRaycasts = true;
        RestartAuto(ProceedDefeat);
    }

    // 진행 — 버튼 클릭과 자동 진행이 같은 경로를 쓴다 (중복 발화는 activeSelf 가드로 차단)
    private void ProceedVictory()
    {
        if (_victory == null || !_victory.activeSelf) return;
        var cb = _onNext; Hide(); cb?.Invoke();
    }

    private void ProceedDefeat()
    {
        if (_defeat == null || !_defeat.activeSelf) return;
        var cb = _onContinue; Hide(); cb?.Invoke();
    }

    private void RestartAuto(System.Action proceed)
    {
        if (_autoRoutine != null) StopCoroutine(_autoRoutine);
        _autoRoutine = StartCoroutine(AutoAdvance(proceed));
    }

    private System.Collections.IEnumerator AutoAdvance(System.Action proceed)
    {
        yield return new WaitForSeconds(AutoAdvanceDelay);
        _autoRoutine = null;
        proceed();
    }

    private void Hide()
    {
        if (_autoRoutine != null) { StopCoroutine(_autoRoutine); _autoRoutine = null; }
        _group.alpha = 0f; _group.blocksRaycasts = false;
        _victory.SetActive(false); _defeat.SetActive(false);
    }

    // ── 헬퍼 ──────────────────────────────────────────────────
    private static GameObject NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }
    private Image NewImage(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        return go.GetComponent<Image>();
    }
    private TMP_Text NewText(string name, Transform parent, string text, TMP_FontAsset font, float size, Color color, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.text = text; t.fontSize = size; t.color = color; t.fontStyle = style;
        t.enableWordWrapping = false; t.raycastTarget = false;
        return t;
    }
    private Button NewButton(string name, Transform parent, string label, TMP_FontAsset font)
    {
        var go = new GameObject(name + "Box", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform; rt.anchorMin = new Vector2(0.5f, 0f); rt.anchorMax = new Vector2(0.5f, 0f); rt.pivot = new Vector2(0.5f, 0f); rt.sizeDelta = new Vector2(260, 64); rt.anchoredPosition = new Vector2(0, 46);
        var img = go.GetComponent<Image>();
        var btnSprite = Resources.Load<Sprite>("Button/default_button");
        if (btnSprite != null) { img.sprite = btnSprite; img.type = Image.Type.Sliced; img.color = Color.white; }
        else img.color = new Color(0.20f, 0.20f, 0.26f, 1f);
        var btn = go.AddComponent<Button>();
        var lt = NewText(name + "Label", go.transform, label, font, 30, Gold, FontStyles.Bold);
        var ltr = (RectTransform)lt.transform; Stretch(ltr); lt.alignment = TextAlignmentOptions.Center;
        return btn;
    }
    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }
}
