// ============================================================
// Debugging/DebugToolPanel.cs  (#14, 2026-06-11)
// 게임 검증용 디버그 툴 — F1 토글 팝업 (에디터/개발 빌드 전용)
// ============================================================
//
// [열기/닫기]  F1
// [단축키]     전투 중: 1·2=아군 전원 스킬1·2 / 3·4=적 전원 스킬1·2 / 5·6=적(보스) 스킬3·4 — 전부 모션+효과 실제 적용
// [섹션]
//   재화  : 영혼석 +100/-100/0 · 마석 +50/-50/0
//   전투  : 즉시 승리 / 즉시 패배 / 풀힐·스트레스0 / 스택 999
//   진행  : 층 전진(구 F2) / 새 런 시작
//   스킬  : 단축키 1~4 와 동일한 버튼
//
// 씬 배치 불필요 — RuntimeInitializeOnLoad 로 자가 생성(DontDestroyOnLoad).
// 구 CheatInput(F1 즉발 치트 / F2 층 전진)을 대체한다.
// ============================================================
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class DebugToolPanel : MonoBehaviour
{
    /// <summary>패널 빈 영역을 잡고 끌면 창 이동 (버튼 위에서는 버튼 클릭 우선).</summary>
    private class DragMove : MonoBehaviour, IDragHandler
    {
        public RectTransform target;
        public void OnDrag(PointerEventData e)
        {
            if (target != null) target.anchoredPosition += e.delta;
        }
    }

    private GameObject _root;
    private bool _built;

    private static readonly Color Gold = new Color(1f, 0.84f, 0.4f, 1f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindFirstObjectByType<DebugToolPanel>(FindObjectsInactive.Include) != null) return;
        var go = new GameObject("DebugToolPanel");
        DontDestroyOnLoad(go);
        go.AddComponent<DebugToolPanel>();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.f1Key.wasPressedThisFrame)
            Toggle();

        // 전투 중 스킬/연출 테스트 단축키 — 1·2 아군 / 3·4 적 스킬1·2 / 5·6 적(보스) 스킬3·4
        var bm = BattleManager.Instance;
        if (bm == null || !bm.DebugInBattle) return;
        if (Keyboard.current.digit1Key.wasPressedThisFrame) bm.DebugCastAllAllySkills(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) bm.DebugCastAllAllySkills(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) bm.DebugCastAllEnemySkills(0);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) bm.DebugCastAllEnemySkills(1);
        if (Keyboard.current.digit5Key.wasPressedThisFrame) bm.DebugCastAllEnemySkills(2);
        if (Keyboard.current.digit6Key.wasPressedThisFrame) bm.DebugCastAllEnemySkills(3);
    }

    public void Toggle()
    {
        if (!_built) Build();
        _root.SetActive(!_root.activeSelf);
    }

    // ── 버튼 동작 ───────────────────────────────────────────────
    private static void Soul(int delta)
    {
        var m = SoulstoneManager.Instance; if (m == null) return;
        m.SetAmount(Mathf.Max(0, m.Amount + delta));
    }
    private static void SoulZero() => SoulstoneManager.Instance?.SetAmount(0);
    private static void Mana(int delta)
    {
        var m = ManastoneManager.Instance; if (m == null) return;
        m.SetAmount(Mathf.Max(0, m.Amount + delta));
    }
    private static void ManaZero() => ManastoneManager.Instance?.SetAmount(0);

    private static void StackMax()
    {
        if (PlayerRoleCost.Instance == null) { Debug.LogWarning("[디버그] PlayerRoleCost 없음"); return; }
        foreach (StackType role in System.Enum.GetValues(typeof(StackType)))
            PlayerRoleCost.Instance.SetAmount(role, 999);
        Debug.Log("[디버그] 모든 스택 999");
    }

    private static bool InBattle(out BattleManager bm)
    {
        bm = BattleManager.Instance;
        if (bm != null && bm.DebugInBattle) return true;
        Debug.LogWarning("[디버그] 전투 중이 아님 — 전투 치트 무시");
        return false;
    }

    private static void NewRun()
    {
        // BattleManager.StartNextRunLoop 와 동일 절차 (노드맵에서도 동작하도록 독립 구현)
        MercenaryService.Instance?.ResetForNewRun();
        PartyManager.Instance?.ResetGame();
        SoulstoneManager.Instance?.ResetCurrency();
        SceneManager.LoadScene("GamePlayScene");
    }

    // ── UI 생성 ────────────────────────────────────────────────
    private void Build()
    {
        _built = true;
        var font = TMP_Settings.defaultFontAsset;
        var panelSprite = Resources.Load<Sprite>("UI/panel_1");
        var btnSprite   = Resources.Load<Sprite>("Button/default_button");

        var canvasGo = new GameObject("DebugCanvas", typeof(Canvas), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 11000;
        _root = canvasGo;

        var box = new GameObject("Box", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        box.transform.SetParent(canvasGo.transform, false);
        var brt = (RectTransform)box.transform;
        brt.anchorMin = brt.anchorMax = new Vector2(1f, 0.5f); brt.pivot = new Vector2(1f, 0.5f);
        brt.sizeDelta = new Vector2(560, 880); brt.anchoredPosition = new Vector2(-24, 0);
        var bimg = box.GetComponent<Image>();
        if (panelSprite != null) { bimg.sprite = panelSprite; bimg.type = Image.Type.Sliced; bimg.color = Color.white; }
        else bimg.color = new Color(0.1f, 0.1f, 0.14f, 0.97f);
        bimg.raycastTarget = true;
        box.AddComponent<DragMove>().target = brt;   // 빈 영역 드래그로 창 이동

        var vlg = box.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(36, 36, 30, 26);
        vlg.spacing = 10;
        vlg.childControlWidth = true; vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;

        Label(box.transform, font, "디버그 툴  (F1)", 30, Gold, bold: true);

        Label(box.transform, font, "─ 재화 ─", 20, new Color(0.8f, 0.8f, 0.85f), bold: false);
        Row(box.transform, font, btnSprite,
            ("영혼석 +100", () => Soul(+100)), ("영혼석 -100", () => Soul(-100)), ("영혼석 0", SoulZero));
        Row(box.transform, font, btnSprite,
            ("마석 +50", () => Mana(+50)), ("마석 -50", () => Mana(-50)), ("마석 0", ManaZero));

        Label(box.transform, font, "─ 전투 (전투 중에만) ─", 20, new Color(0.8f, 0.8f, 0.85f), bold: false);
        Row(box.transform, font, btnSprite,
            ("즉시 승리", () => { if (InBattle(out var b)) b.DebugKillAllEnemies(); }),
            ("즉시 패배", () => { if (InBattle(out var b)) b.DebugKillAllAllies(); }));
        Row(box.transform, font, btnSprite,
            ("풀힐+스트레스0", () => { if (InBattle(out var b)) b.DebugFullHeal(); }),
            ("스택 999", StackMax));

        Label(box.transform, font, "─ 진행 ─", 20, new Color(0.8f, 0.8f, 0.85f), bold: false);
        Row(box.transform, font, btnSprite,
            ("층 전진", () => NodeSystem.Current?.CheatAdvanceFloor()),
            ("새 런 시작", NewRun));

        Label(box.transform, font, "─ 스킬/연출 테스트 (전투 중 단축키 1~6) ─", 20, new Color(0.8f, 0.8f, 0.85f), bold: false);
        Row(box.transform, font, btnSprite,
            ("아군 스킬1 [1]", () => { if (InBattle(out var b)) b.DebugCastAllAllySkills(0); }),
            ("아군 스킬2 [2]", () => { if (InBattle(out var b)) b.DebugCastAllAllySkills(1); }));
        Row(box.transform, font, btnSprite,
            ("적 스킬1 [3]", () => { if (InBattle(out var b)) b.DebugCastAllEnemySkills(0); }),
            ("적 스킬2 [4]", () => { if (InBattle(out var b)) b.DebugCastAllEnemySkills(1); }));
        Row(box.transform, font, btnSprite,
            ("적 스킬3(보스) [5]", () => { if (InBattle(out var b)) b.DebugCastAllEnemySkills(2); }),
            ("적 스킬4(보스) [6]", () => { if (InBattle(out var b)) b.DebugCastAllEnemySkills(3); }));
        Row(box.transform, font, btnSprite,
            ("적 랜덤 행동", () => { if (InBattle(out var b)) b.DebugEnemyTurnOnce(); }),
            ("피격 모션", () => { if (InBattle(out var b)) b.DebugHitMotionAll(); }));

        Row(box.transform, font, btnSprite, ("닫기 (F1)", () => _root.SetActive(false)));

        _root.SetActive(false);
    }

    private static void Label(Transform parent, TMP_FontAsset font, string text, float size, Color color, bool bold)
    {
        var go = new GameObject("Label", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.text = text; t.fontSize = size; t.color = color;
        t.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
    }

    private static void Row(Transform parent, TMP_FontAsset font, Sprite btnSprite, params (string label, System.Action onClick)[] buttons)
    {
        var row = new GameObject("Row", typeof(RectTransform));
        row.transform.SetParent(parent, false);
        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10; hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true; hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = false;

        foreach (var (label, onClick) in buttons)
        {
            var go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(row.transform, false);
            var img = go.GetComponent<Image>();
            if (btnSprite != null) { img.sprite = btnSprite; img.type = Image.Type.Sliced; img.color = Color.white; }
            else img.color = new Color(0.22f, 0.22f, 0.28f, 1f);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 48; le.minHeight = 44;
            var btn = go.AddComponent<Button>();
            var act = onClick;
            btn.onClick.AddListener(() => act?.Invoke());

            var lt = new GameObject("Label", typeof(RectTransform));
            lt.transform.SetParent(go.transform, false);
            var lrt = (RectTransform)lt.transform;
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
            var t = lt.AddComponent<TextMeshProUGUI>();
            if (font != null) t.font = font;
            t.text = label; t.fontSize = 20; t.color = Gold; t.fontStyle = FontStyles.Bold;
            t.alignment = TextAlignmentOptions.Center; t.raycastTarget = false;
        }
    }
}
#endif
