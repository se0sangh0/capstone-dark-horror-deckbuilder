// ============================================================
// Mercenary/FellowCardView.cs
// 동료 카드 프리팹에 붙는 공용 뷰 컴포넌트
// ============================================================
//
// [사용 흐름]
//   1. 카드 프리팹(빈 GameObject + RectTransform + Image 배경)을 만든다.
//   2. 자식으로 배치할 UI (배지/이름/성향/별/비용/버튼들)를 만든다.
//   3. 루트에 이 FellowCardView 를 부착한다.
//   4. 인스펙터의 슬롯에 자식들을 연결한다. (모두 선택. 비어있으면 해당 항목만 표시 생략)
//   5. RecruitPanel / PartyEditPanel / GrowthPanel 이 이 프리팹을 Instantiate 해서 Bind() 호출.
//
// [모드]
//   - Recruit       : 후보 카드 — 비용 + 고용 버튼 노출
//   - Reserve       : 예비대 카드 — 정보만, 클릭 가능
//   - PartySlot     : 파티 슬롯 카드 — 정보만, 클릭 가능 (편집용)
//   - SynthesizeSlot: 합성 선택 카드 — 토글 (선택 시 외곽선 강조)
//
// [Outline 강조]
//   selectionOutline (CanvasGroup 또는 Image) 가 연결되어 있으면
//   IsSelected 상태에 따라 alpha/색상 토글된다.
// ============================================================

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum FellowCardMode
{
    Recruit,        // 후보 — 비용 + 고용 버튼 노출
    Reserve,        // 예비대 — 정보만 + 클릭
    PartySlot,      // 파티 슬롯 — 정보만 + 클릭
    SynthesizeSlot, // 합성 슬롯 — 토글 선택
}

[DisallowMultipleComponent]
public class FellowCardView : MonoBehaviour
{
    // ----------------------------------------------------------
    // 인스펙터 — 카드 안 UI 요소 연결 (모두 선택)
    // ----------------------------------------------------------
    [Header("표시 (Display) — 자식 UI 연결")]
    [Tooltip("역할(딜러/탱커/서포터) 배지 Image. role 색상에 따라 채워짐.")]
    [SerializeField] private Image     roleBadgeImage;

    [Tooltip("역할 텍스트 (선택 — 배지 안에 글자로 표시할 때)")]
    [SerializeField] private TMP_Text  roleLabel;

    [Tooltip("동료 displayName 표시")]
    [SerializeField] private TMP_Text  nameLabel;

    [Tooltip("성향 라벨 (도박사/안전주의자/기회주의자/낙천가)")]
    [SerializeField] private TMP_Text  affinityLabel;

    [Tooltip("성급 표시 (별 1~3개)")]
    [SerializeField] private TMP_Text  starLabel;

    [Tooltip("비용 표시 — Recruit 모드에서만 노출")]
    [SerializeField] private TMP_Text  costLabel;

    [Tooltip("HP 표시 (선택)")]
    [SerializeField] private TMP_Text  hpLabel;

    [Header("버튼")]
    [Tooltip("메인 액션 버튼 (Recruit:고용 / Reserve:선택 / PartySlot:선택)")]
    [SerializeField] private Button    actionButton;

    [Tooltip("메인 버튼 안 라벨 (Recruit 모드에서 \"고용 (N)\" 표시)")]
    [SerializeField] private TMP_Text  actionButtonLabel;

    [Tooltip("제거 버튼 (선택 — PartyEdit/Reserve 에서 사용 가능)")]
    [SerializeField] private Button    removeButton;

    [Header("선택 강조")]
    [Tooltip("선택 시 표시할 외곽선/배경. CanvasGroup 우선, 없으면 GameObject SetActive.")]
    [SerializeField] private GameObject selectionOutline;

    [Header("역할별 색상")]
    [SerializeField] private Color colorDealer  = new Color(0.78f, 0.20f, 0.27f);
    [SerializeField] private Color colorTanker  = new Color(0.45f, 0.45f, 0.65f);
    [SerializeField] private Color colorSupport = new Color(0.35f, 0.72f, 0.72f);

    // ----------------------------------------------------------
    // 런타임 상태
    // ----------------------------------------------------------
    public FellowData      BoundFellow { get; private set; }
    public FellowCardMode  Mode        { get; private set; }
    public bool            IsSelected  { get; private set; }

    /// <summary>메인 액션 버튼 클릭 콜백. 호출자(패널)가 등록.</summary>
    public event Action<FellowCardView> OnActionClicked;

    /// <summary>제거 버튼 클릭 콜백 (있을 때만).</summary>
    public event Action<FellowCardView> OnRemoveClicked;

    // ----------------------------------------------------------
    // 초기화 — 버튼 리스너 연결
    // ----------------------------------------------------------
    private void Awake()
    {
        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => OnActionClicked?.Invoke(this));
        }
        if (removeButton != null)
        {
            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(() => OnRemoveClicked?.Invoke(this));
        }
    }

    // ----------------------------------------------------------
    // 공개 API — 바인딩
    // ----------------------------------------------------------

    /// <summary>
    /// 동료 데이터로 카드 표시를 갱신한다.
    /// fellow=null 이면 빈 슬롯 모드 (회색 외곽 / 라벨 비움).
    /// </summary>
    public void Bind(FellowData fellow, FellowCardMode mode, int? costOverride = null)
    {
        BoundFellow = fellow;
        Mode        = mode;
        IsSelected  = false;

        if (selectionOutline != null) selectionOutline.SetActive(false);

        if (fellow == null)
        {
            ShowEmpty();
            return;
        }

        // ── 역할 색상 ──
        if (roleBadgeImage != null)
            roleBadgeImage.color = ColorForRole(fellow.role);
        if (roleLabel != null)
            roleLabel.text = ShortRoleLabel(fellow.role);

        // ── 텍스트 ──
        if (nameLabel != null)     nameLabel.text     = !string.IsNullOrEmpty(fellow.displayName) ? fellow.displayName : fellow.id;
        if (affinityLabel != null) affinityLabel.text = fellow.AffinityLabel;
        if (starLabel != null)     starLabel.text     = new string('★', Mathf.Clamp(fellow.starLevel, 1, 3));
        if (hpLabel != null)       hpLabel.text       = $"HP {fellow.maxHp}";

        // ── 모드별 버튼/비용 표시 ──
        int costShown = costOverride ?? fellow.recruitCost;
        bool showCost   = mode == FellowCardMode.Recruit;
        bool showAction = mode != FellowCardMode.Reserve || true; // 모든 모드에서 메인 클릭 허용 — false 강제 모드 없음
        bool showRemove = mode == FellowCardMode.PartySlot || mode == FellowCardMode.Reserve;

        if (costLabel != null)
        {
            costLabel.gameObject.SetActive(showCost);
            costLabel.text = showCost ? costShown.ToString() : string.Empty;
        }
        if (actionButton != null)
        {
            actionButton.gameObject.SetActive(showAction);
            if (actionButtonLabel != null)
                actionButtonLabel.text = ActionLabelForMode(mode, costShown);
        }
        if (removeButton != null)
            removeButton.gameObject.SetActive(showRemove);
    }

    /// <summary>빈 슬롯 표시 — 라벨 비우고 버튼/외곽선 끔.</summary>
    public void BindEmpty(FellowCardMode mode)
    {
        Bind(null, mode);
    }

    /// <summary>선택 토글 — SynthesizeSlot / PartySlot 등에서 호출.</summary>
    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        if (selectionOutline != null) selectionOutline.SetActive(selected);
    }

    /// <summary>인터랙티브 가능 여부 — 영혼석 부족 등 외부 조건으로 끌 때.</summary>
    public void SetInteractable(bool interactable)
    {
        if (actionButton != null) actionButton.interactable = interactable;
    }

    // ----------------------------------------------------------
    // 내부 헬퍼
    // ----------------------------------------------------------
    private void ShowEmpty()
    {
        if (nameLabel != null)     nameLabel.text     = string.Empty;
        if (affinityLabel != null) affinityLabel.text = string.Empty;
        if (starLabel != null)     starLabel.text     = string.Empty;
        if (costLabel != null)     costLabel.text     = string.Empty;
        if (hpLabel != null)       hpLabel.text       = string.Empty;
        if (roleLabel != null)     roleLabel.text     = string.Empty;
        if (roleBadgeImage != null) roleBadgeImage.color = new Color(1, 1, 1, 0.15f);
        if (actionButton != null)
        {
            actionButton.gameObject.SetActive(false);
            if (actionButtonLabel != null) actionButtonLabel.text = string.Empty;
        }
        if (removeButton != null) removeButton.gameObject.SetActive(false);
    }

    private Color ColorForRole(CompanionRole role) => role switch
    {
        CompanionRole.Dealer  => colorDealer,
        CompanionRole.Tanker  => colorTanker,
        CompanionRole.Support => colorSupport,
        _                     => Color.white,
    };

    private static string ShortRoleLabel(CompanionRole role) => role switch
    {
        CompanionRole.Dealer  => "딜러",
        CompanionRole.Tanker  => "탱커",
        CompanionRole.Support => "서포터",
        _                     => "?",
    };

    private static string ActionLabelForMode(FellowCardMode mode, int cost) => mode switch
    {
        FellowCardMode.Recruit        => $"고용 ({cost})",
        FellowCardMode.Reserve        => "선택",
        FellowCardMode.PartySlot      => "선택",
        FellowCardMode.SynthesizeSlot => "선택",
        _                             => "선택",
    };
}
