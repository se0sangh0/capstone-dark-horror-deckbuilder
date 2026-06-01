// ============================================================
// Currency/MagicStoneShopPanel.cs
// 마석 상점 — 메인 메뉴에서 마석으로 영구 패시브 해금 (기획 §16 §5)
// ============================================================
//
// UI 를 런타임에 코드로 자가 생성한다 (씬에는 빈 GameObject + 컴포넌트만 두면 됨).
//   - 전체 딤 + 중앙 패널 + 제목 + 보유 마석 + 패시브 3카드(이름/설명/비용/버튼) + 닫기
//   - [해금] → MetaPassiveManager.TryUnlock → 카드 갱신
//   - ManastoneManager.OnCurrencyChanged 구독으로 보유량/버튼 실시간 갱신
//
// 메인 메뉴 [마석 상점] 버튼의 onClick 에 Open() 을 연결한다.
// ============================================================

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MagicStoneShopPanel : MonoBehaviour
{
    private bool _built;
    private GameObject _root;          // 딤+패널 전체 루트 (토글 대상)
    private TMP_Text _balanceText;
    private readonly List<CardRow> _rows = new List<CardRow>();

    private struct CardRow
    {
        public string id;
        public Button button;
        public TMP_Text buttonLabel;
    }

    private void Awake()
    {
        Build();
        SetVisible(false);
    }

    private void Start()
    {
        // 로그라이크 루프 — 새 런 첫 노드 진입 전 패시브 해금 화면 자동 표시 (기획 §16)
        if (MetaPassiveManager.ConsumeShowShopOnLoad()) Open();
    }

    private void OnEnable()
    {
        if (ManastoneManager.Instance != null)
            ManastoneManager.Instance.OnCurrencyChanged += OnManaChanged;
    }

    private void OnDisable()
    {
        if (ManastoneManager.Instance != null)
            ManastoneManager.Instance.OnCurrencyChanged -= OnManaChanged;
    }

    private void OnManaChanged(int _) => Refresh();

    // ── 외부 진입점 ─────────────────────────────────────────────
    public void Open()
    {
        if (!_built) Build();
        SetVisible(true);
        Refresh();
        AudioManager.Instance?.PlaySfxById(SfxId.Confirm);
    }

    public void Close()
    {
        SetVisible(false);
        AudioManager.Instance?.PlaySfxById(SfxId.Confirm);
    }

    private void SetVisible(bool v)
    {
        if (_root != null) _root.SetActive(v);
    }

    // ── 해금 처리 ───────────────────────────────────────────────
    private void OnBuyClicked(string id)
    {
        if (MetaPassiveManager.TryUnlock(id))
        {
            AudioManager.Instance?.PlaySfxById(SfxId.Confirm);
            Debug.Log($"[마석상점] 해금 성공: {id}");
        }
        Refresh();
    }

    private void Refresh()
    {
        int mana = ManastoneManager.Instance != null ? ManastoneManager.Instance.Amount : 0;
        if (_balanceText != null) _balanceText.text = $"보유 마석: {mana}";

        foreach (var row in _rows)
        {
            bool unlocked = MetaPassiveManager.IsUnlocked(row.id);
            int  cost     = MetaPassiveManager.CostOf(row.id);
            bool canAfford = mana >= cost;

            if (unlocked)
            {
                row.button.interactable = false;
                row.buttonLabel.text = "해금 완료";
            }
            else
            {
                row.button.interactable = canAfford;
                row.buttonLabel.text = $"해금 ({cost})";
            }
        }
    }

    // ============================================================
    // 런타임 UI 생성
    // ============================================================
    private void Build()
    {
        if (_built) return;
        _built = true;

        var font = TMP_Settings.defaultFontAsset;

        // 루트 (딤) — 전체화면, 입력 차단
        _root = NewUI("ShopRoot", transform);
        var rootRT = (RectTransform)_root.transform;
        Stretch(rootRT);
        var dim = _root.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.72f);
        dim.raycastTarget = true;

        // 중앙 패널
        var panel = NewUI("Panel", _root.transform);
        var panelRT = (RectTransform)panel.transform;
        panelRT.anchorMin = panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.sizeDelta = new Vector2(940, 640);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.12f, 0.12f, 0.16f, 0.98f);

        // 제목
        var title = NewText("Title", panel.transform, "마석 상점", font, 40, FontStyles.Bold);
        var titleRT = (RectTransform)title.transform;
        titleRT.anchorMin = new Vector2(0f, 1f); titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.sizeDelta = new Vector2(0, 60); titleRT.anchoredPosition = new Vector2(0, -20);
        title.alignment = TextAlignmentOptions.Center;

        // 보유 마석
        var bal = NewText("Balance", panel.transform, "보유 마석: 0", font, 28, FontStyles.Normal);
        var balRT = (RectTransform)bal.transform;
        balRT.anchorMin = new Vector2(0f, 1f); balRT.anchorMax = new Vector2(1f, 1f);
        balRT.pivot = new Vector2(0.5f, 1f);
        balRT.sizeDelta = new Vector2(0, 40); balRT.anchoredPosition = new Vector2(0, -84);
        bal.alignment = TextAlignmentOptions.Center;
        bal.color = new Color(0.9f, 0.78f, 0.3f, 1f);
        _balanceText = bal;

        // 스크롤 영역 (제목/보유마석 아래 ~ 닫기버튼 위)
        var scrollGO = NewUI("Scroll", panel.transform);
        var scrollRT = (RectTransform)scrollGO.transform;
        scrollRT.anchorMin = new Vector2(0f, 0f); scrollRT.anchorMax = new Vector2(1f, 1f);
        scrollRT.offsetMin = new Vector2(20, 92);    // 하단 닫기 버튼 공간
        scrollRT.offsetMax = new Vector2(-20, -126); // 상단 제목/보유마석 공간
        var scroll = scrollGO.AddComponent<ScrollRect>();
        scroll.horizontal = false; scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 28f;

        var viewport = NewUI("Viewport", scrollGO.transform);
        var vpRT = (RectTransform)viewport.transform; Stretch(vpRT);
        var vpImg = viewport.AddComponent<Image>(); vpImg.color = new Color(1f, 1f, 1f, 0.02f);
        var mask = viewport.AddComponent<Mask>(); mask.showMaskGraphic = false;

        var content = NewUI("Content", viewport.transform);
        var contentRT = (RectTransform)content.transform;
        contentRT.anchorMin = new Vector2(0f, 1f); contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot = new Vector2(0.5f, 1f);
        scroll.viewport = vpRT; scroll.content = contentRT;

        // 카드 (전체폭, 세로 스택) — 섹션 헤더 + 카드
        var infos = MetaPassiveManager.All;
        float cardH = 84f, gap = 8f;
        float y = -4f;
        y = AddSection(content.transform, font, "전투 패시브", y);
        foreach (var info in infos)
            if (info.kind == MetaPassiveManager.Kind.Passive) { BuildCard(content.transform, info, font, y, cardH); y -= (cardH + gap); }
        y -= 6f;
        y = AddSection(content.transform, font, "스킬 해금", y);
        foreach (var info in infos)
            if (info.kind == MetaPassiveManager.Kind.Skill) { BuildCard(content.transform, info, font, y, cardH); y -= (cardH + gap); }
        contentRT.sizeDelta = new Vector2(0, -y + 8f);

        // 닫기 버튼
        var closeBtn = NewButton("CloseButton", panel.transform, "닫기", font, out var closeLabel);
        var cbRT = (RectTransform)closeBtn.transform;
        cbRT.anchorMin = cbRT.anchorMax = new Vector2(0.5f, 0f);
        cbRT.pivot = new Vector2(0.5f, 0f);
        cbRT.sizeDelta = new Vector2(200, 56); cbRT.anchoredPosition = new Vector2(0, 24);
        closeBtn.GetComponent<Button>().onClick.AddListener(Close);
    }

    /// <summary>스크롤 콘텐츠에 섹션 헤더를 y 위치에 배치하고, 다음 배치 y(헤더 높이만큼 내려간 값)를 반환.</summary>
    private float AddSection(Transform content, TMP_FontAsset font, string text, float y)
    {
        var t = NewText("Section", content, text, font, 24, FontStyles.Bold);
        var rt = (RectTransform)t.transform;
        rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(1f, 1f); rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(-16, 34); rt.anchoredPosition = new Vector2(0, y);
        t.alignment = TextAlignmentOptions.Left;
        t.color = new Color(0.9f, 0.78f, 0.3f, 1f);
        return y - 34f - 4f;
    }

    private void BuildCard(Transform content, MetaPassiveManager.Info info, TMP_FontAsset font, float y, float h)
    {
        var card = NewUI("Card_" + info.id, content);
        var rt = (RectTransform)card.transform;
        rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(-16, h); rt.anchoredPosition = new Vector2(0, y);
        var img = card.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.26f, 1f);

        // 이름 (상단)
        var nameT = NewText("Name", card.transform, info.name, font, 23, FontStyles.Bold);
        var nrt = (RectTransform)nameT.transform;
        nrt.anchorMin = new Vector2(0f, 1f); nrt.anchorMax = new Vector2(0.74f, 1f);
        nrt.pivot = new Vector2(0f, 1f);
        nrt.offsetMin = new Vector2(16, -34); nrt.offsetMax = new Vector2(0, -8);
        nameT.alignment = TextAlignmentOptions.Left;

        // 설명 (하단)
        var descT = NewText("Desc", card.transform, info.desc, font, 16, FontStyles.Normal);
        var drt = (RectTransform)descT.transform;
        drt.anchorMin = new Vector2(0f, 0f); drt.anchorMax = new Vector2(0.74f, 1f);
        drt.pivot = new Vector2(0f, 0.5f);
        drt.offsetMin = new Vector2(16, 8); drt.offsetMax = new Vector2(0, -36);
        descT.alignment = TextAlignmentOptions.TopLeft;
        descT.color = new Color(0.82f, 0.82f, 0.88f, 1f);
        descT.enableWordWrapping = true;

        // 해금 버튼 (오른쪽)
        var btn = NewButton("Buy", card.transform, "해금", font, out var label);
        label.fontSize = 22;
        var brt = (RectTransform)btn.transform;
        brt.anchorMin = new Vector2(1f, 0.5f); brt.anchorMax = new Vector2(1f, 0.5f);
        brt.pivot = new Vector2(1f, 0.5f);
        brt.sizeDelta = new Vector2(150, 60); brt.anchoredPosition = new Vector2(-14, 0);
        string id = info.id;
        btn.GetComponent<Button>().onClick.AddListener(() => OnBuyClicked(id));

        _rows.Add(new CardRow { id = info.id, button = btn.GetComponent<Button>(), buttonLabel = label });
    }

    // ── UI 생성 헬퍼 ────────────────────────────────────────────
    private static GameObject NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static TMP_Text NewText(string name, Transform parent, string text, TMP_FontAsset font, float size, FontStyles style)
    {
        var go = NewUI(name, parent);
        var t = go.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.text = text; t.fontSize = size; t.fontStyle = style;
        t.color = Color.white; t.raycastTarget = false;
        Stretch((RectTransform)go.transform);
        return t;
    }

    private static GameObject NewButton(string name, Transform parent, string label, TMP_FontAsset font, out TMP_Text labelText)
    {
        var go = NewUI(name, parent);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.32f, 0.5f, 0.86f, 1f);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.disabledColor = new Color(0.35f, 0.35f, 0.4f, 1f);
        btn.colors = colors;

        labelText = NewText("Label", go.transform, label, font, 26, FontStyles.Bold);
        labelText.alignment = TextAlignmentOptions.Center;
        return go;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }
}
