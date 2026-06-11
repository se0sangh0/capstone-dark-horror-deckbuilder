// ============================================================
// UI/LeftPanelToggle.cs
// 좌측 패널 접기/펼치기 — 왼쪽으로 슬라이드 숨김 + 가장자리 탭으로 재오픈
// 접을 때 오른쪽 콘텐츠의 좌측 여백을 0으로 펴서 화면을 꽉 채운다.
// ============================================================
//
// [동작]
//   탭 버튼을 누르면 좌측 패널이 자기 폭만큼 왼쪽으로 슬라이드되어 숨고,
//   동시에 오른쪽 콘텐츠(노드맵/전투/용병소/화툿불 등)의 좌측 여백(offsetMin.x)이
//   0 으로 펴져 화면 전체를 채운다. 탭은 화면 좌측 가장자리에 ▶ 로 남는다.
//   다시 누르면 패널이 펼쳐지고 오른쪽 여백도 원래대로 복귀, 탭은 ◀.
//   (코루틴 lerp 로 직접 애니메이션 — 외부 트윈 의존 없음)
//
// [배치]
//   이 컴포넌트는 "탭 버튼" GameObject 에 붙인다.
//   - panel         : 슬라이드할 좌측 패널 RectTransform (비우면 부모를 사용)
//   - arrowLabel    : 탭의 화살표 텍스트 (◀/▶ 토글)
//   - rightContents : 접을 때 좌측 여백을 펴서 화면을 채울 오른쪽 콘텐츠들
//   같은 GameObject 의 Button.onClick 에 Toggle() 을 자동 연결한다.
// ============================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeftPanelToggle : MonoBehaviour
{
    [Header("참조")]
    [Tooltip("슬라이드할 좌측 패널. 비우면 이 오브젝트의 부모를 사용.")]
    [SerializeField] private RectTransform panel;

    [Tooltip("탭의 화살표 텍스트 (◀ 펼침 / ▶ 접힘).")]
    [SerializeField] private TMP_Text arrowLabel;

    [Tooltip("접을 때 좌측 여백(offsetMin.x)을 0으로 펴서 화면을 채울 오른쪽 콘텐츠들.")]
    [SerializeField] private RectTransform[] rightContents;

    [Header("전투 스프라이트 (월드)")]
    [Tooltip("접을 때 왼쪽으로 함께 옮길 전투 월드 루트(아군/적 스프라이트 부모, 예: InGameObjects). 비우면 미사용.")]
    [SerializeField] private Transform battleWorldRoot;
    [Tooltip("접을 때 battleWorldRoot 를 왼쪽으로 옮길 월드 거리.")]
    [SerializeField] private float battleWorldShiftX = 2.0f;

    [SerializeField] private float slideDuration = 0.25f;

    [Header("hover 표시")]
    [Tooltip("탭 RectTransform 중심에서 이 픽셀 거리 내에 마우스가 들어오면 탭 노출. 0 이면 hover 트리거 미사용(항상 노출).")]
    [SerializeField] private float hoverRevealRadius = 180f;
    [Tooltip("hover alpha fade 시간. 0 이면 즉시.")]
    [SerializeField] private float hoverFadeDuration = 0.15f;

    private float   _shownX;
    private float   _hiddenX;
    private float[] _rightShownLeft;   // 각 오른쪽 콘텐츠의 펼침 상태 좌측 여백
    private float   _worldBaseX;        // battleWorldRoot 펼침 상태 X
    private bool    _collapsed;
    private Coroutine _slideCo;

    // 접기 차단 게이트 — 용병소(메인/모집/성장) 외의 모달 패널이 열리면 탭을 숨긴다.
    private CanvasGroup _tabGroup;
    private System.Collections.Generic.List<CanvasGroup> _blockCgs;
    private System.Collections.Generic.List<GameObject>  _blockGos;
    private MagicStoneShopPanel _magicShop;

    // hover fade — Update 에서 목표 alpha 를 갱신, 현재 alpha 를 부드럽게 보간.
    private float _hoverAlphaTarget = 1f;

    private void Awake()
    {
        if (panel == null) panel = transform.parent as RectTransform;

        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveListener(Toggle);
            btn.onClick.AddListener(Toggle);
        }
    }

    private void Start()
    {
        if (panel == null) return;
        _shownX  = panel.anchoredPosition.x;
        _hiddenX = _shownX - panel.rect.width;

        if (rightContents != null)
        {
            _rightShownLeft = new float[rightContents.Length];
            for (int i = 0; i < rightContents.Length; i++)
                if (rightContents[i] != null) _rightShownLeft[i] = rightContents[i].offsetMin.x;
        }
        if (battleWorldRoot != null) _worldBaseX = battleWorldRoot.localPosition.x;

        _tabGroup = GetComponent<CanvasGroup>();
        CacheBlockingPanels();
        UpdateArrow();
    }

    // ── 접기 차단 게이트 ────────────────────────────────────────
    private void CacheBlockingPanels()
    {
        _blockCgs = new System.Collections.Generic.List<CanvasGroup>();
        _blockGos = new System.Collections.Generic.List<GameObject>();
        foreach (var p in FindObjectsByType<PanelBase>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            // 용병소 메인/모집/성장 패널에서는 접기 허용 → 차단 목록에서 제외
            if (p is MercenaryOfficePanel || p is RecruitPanel || p is GrowthPanel) continue;
            var cg = p.GetComponent<CanvasGroup>();
            if (cg != null) _blockCgs.Add(cg);
            else            _blockGos.Add(p.gameObject);
        }
        _magicShop = FindFirstObjectByType<MagicStoneShopPanel>(FindObjectsInactive.Include);
    }

    private void Update()
    {
        if (_tabGroup == null) return;
        bool blocked = IsAnyBlockingPanelOpen();

        // 차단 패널 열리면 강제 숨김. 아니면 hover 영역 기반으로 노출.
        if (blocked)
        {
            _hoverAlphaTarget        = 0f;
            _tabGroup.blocksRaycasts = false;
            _tabGroup.interactable   = false;
        }
        else
        {
            bool revealAlways = hoverRevealRadius <= 0f;
            bool inRange      = revealAlways || IsMouseNearTab();
            _hoverAlphaTarget         = inRange ? 1f : 0f;
            _tabGroup.blocksRaycasts = inRange;
            _tabGroup.interactable    = inRange;
        }

        // alpha 부드러운 보간
        if (hoverFadeDuration <= 0f)
            _tabGroup.alpha = _hoverAlphaTarget;
        else
            _tabGroup.alpha = Mathf.MoveTowards(_tabGroup.alpha, _hoverAlphaTarget, Time.unscaledDeltaTime / hoverFadeDuration);
    }

    /// <summary>마우스가 탭 중심에서 hoverRevealRadius 픽셀 안에 있는지.</summary>
    private bool IsMouseNearTab()
    {
        var rt = transform as RectTransform;
        if (rt == null) return true; // 안전 폴백 — 항상 노출

        // 탭의 화면 좌표(스크린 픽셀). overlay/screen-space 모두 동작 (RectTransformUtility.WorldToScreenPoint 가 알맞게 처리)
        var canvas = GetComponentInParent<Canvas>();
        Camera uiCam = (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas?.worldCamera;
        Vector2 tabScreen = RectTransformUtility.WorldToScreenPoint(uiCam, rt.position);
        Vector2 mouse     = (Vector2)Input.mousePosition;

        // Canvas scale 보정 — hoverRevealRadius 가 reference resolution 기준이라 가정하지 않고, 픽셀 단위로 비교.
        return Vector2.Distance(tabScreen, mouse) <= hoverRevealRadius;
    }

    /// <summary>용병소(메인/모집/성장) 외의 모달 패널이 열려 있으면 true → 접기 탭 비활성.</summary>
    private bool IsAnyBlockingPanelOpen()
    {
        if (_blockCgs != null)
            foreach (var cg in _blockCgs)
                if (cg != null && cg.alpha > 0.5f) return true;
        if (_blockGos != null)
            foreach (var go in _blockGos)
                if (go != null && go.activeInHierarchy) return true;
        if (_magicShop != null && _magicShop.IsOpen) return true;
        return false;
    }

    /// <summary>패널 접기/펼치기 토글.</summary>
    public void Toggle()
    {
        if (panel == null) return;
        _collapsed = !_collapsed;

        if (_slideCo != null) StopCoroutine(_slideCo);
        if (isActiveAndEnabled && gameObject.activeInHierarchy)
            _slideCo = StartCoroutine(Slide());
        else
            ApplyProgress(1f); // 비활성이면 즉시 목표 적용

        UpdateArrow();
    }

    private IEnumerator Slide()
    {
        float pStart = panel.anchoredPosition.x;
        float[] rStart = CaptureRightLefts();
        _slideStartPanelX = pStart;
        _slideStartRight   = rStart;
        _slideStartWorldX  = battleWorldRoot != null ? battleWorldRoot.localPosition.x : 0f;

        float t = 0f;
        while (t < slideDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / slideDuration);
            k = 1f - (1f - k) * (1f - k) * (1f - k); // ease-out cubic
            ApplyProgress(k);
            yield return null;
        }
        ApplyProgress(1f);
        _slideCo = null;
    }

    // Slide() 가 캡처한 시작값 (ApplyProgress 가 보간에 사용)
    private float   _slideStartPanelX;
    private float[] _slideStartRight;
    private float   _slideStartWorldX;

    /// <summary>진행도 k(0~1) 로 패널 위치 + 오른쪽 여백을 목표값까지 보간 적용.</summary>
    private void ApplyProgress(float k)
    {
        float pStart = _slideCo != null ? _slideStartPanelX : panel.anchoredPosition.x;
        float pEnd   = _collapsed ? _hiddenX : _shownX;
        SetPanelX(Mathf.Lerp(pStart, pEnd, k));

        if (rightContents == null) return;
        for (int i = 0; i < rightContents.Length; i++)
        {
            var rt = rightContents[i];
            if (rt == null) continue;
            float start = (_slideStartRight != null && i < _slideStartRight.Length)
                ? _slideStartRight[i] : rt.offsetMin.x;
            float shown = (_rightShownLeft != null && i < _rightShownLeft.Length) ? _rightShownLeft[i] : 0f;
            float end   = _collapsed ? 0f : shown;
            SetLeft(rt, Mathf.Lerp(start, end, k));
        }

        // 전투 월드 루트(아군/적 스프라이트) — 접으면 왼쪽으로 이동해 화면 중앙에 재배치
        if (battleWorldRoot != null)
        {
            float wStart = _slideCo != null ? _slideStartWorldX : battleWorldRoot.localPosition.x;
            float wEnd   = _collapsed ? _worldBaseX - battleWorldShiftX : _worldBaseX;
            var wp = battleWorldRoot.localPosition;
            wp.x = Mathf.Lerp(wStart, wEnd, k);
            battleWorldRoot.localPosition = wp;
        }
    }

    private float[] CaptureRightLefts()
    {
        if (rightContents == null) return null;
        var arr = new float[rightContents.Length];
        for (int i = 0; i < rightContents.Length; i++)
            arr[i] = rightContents[i] != null ? rightContents[i].offsetMin.x : 0f;
        return arr;
    }

    private void SetPanelX(float x)
    {
        var p = panel.anchoredPosition; p.x = x; panel.anchoredPosition = p;
    }

    private static void SetLeft(RectTransform rt, float left)
    {
        var om = rt.offsetMin; om.x = left; rt.offsetMin = om;
    }

    private void UpdateArrow()
    {
        if (arrowLabel != null) arrowLabel.text = _collapsed ? "▶" : "◀"; // ▶ / ◀
    }
}
