// ============================================================
// Mercenary/RecruitPanel.cs
// 동료 모집 패널 — 후보 3 + 영혼석 + 리롤 + 예비대 미니뷰
// ============================================================
//
// [구성]
//   상단: 보유 영혼석 + 리롤 버튼 (비용 표시)
//   중단: 후보 카드 3 (FellowCardView 프리팹 인스턴스)
//   하단: 예비대 미니뷰 (9칸, FellowCardView 프리팹 — 모드 Reserve)
//   닫기: 메인 패널 복귀
//
// [인스펙터 슬롯]
//   - fellowCardPrefab     : FellowCardView 가 부착된 카드 프리팹
//   - candidatesParent     : 후보 카드 부모 (Horizontal Layout Group 권장)
//   - reservesParent       : 예비대 카드 부모 (Grid Layout Group 권장)
//   - soulstoneLabel       : 보유 영혼석 표시 TMP_Text
//   - rerollButton         : 리롤 버튼
//   - rerollCostLabel      : 리롤 버튼 안 비용 라벨
//   - closeButton          : 닫기 버튼
// ============================================================

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecruitPanel : MercenaryPanelBase
{
    [Header("프리팹 / 부모 컨테이너")]
    [SerializeField] private FellowCardView fellowCardPrefab;
    [SerializeField] private Transform      candidatesParent;
    [SerializeField] private Transform      reservesParent;

    [Header("상단 UI")]
    [SerializeField] private TMP_Text soulstoneLabel;
    [SerializeField] private Button   rerollButton;
    [SerializeField] private TMP_Text rerollCostLabel;

    [Header("닫기")]
    [SerializeField] private Button closeButton;

    // 런타임 인스턴스 풀
    private readonly List<FellowCardView> _candidateCards = new();
    private readonly List<FellowCardView> _reserveCards   = new();

    protected override void Awake()
    {
        base.Awake();
        if (rerollButton != null) rerollButton.onClick.AddListener(HandleReroll);
        if (closeButton  != null) closeButton.onClick.AddListener(Close);
    }

    protected override void OnOpened()
    {
        SubscribeCurrency(true);
        RebuildAll();
    }

    protected override void OnClosed()
    {
        SubscribeCurrency(false);
    }

    // ----------------------------------------------------------
    // 영혼석 변동 즉시 반영
    // ----------------------------------------------------------
    private void SubscribeCurrency(bool subscribe)
    {
        if (SoulstoneManager.Instance == null) return;
        if (subscribe) SoulstoneManager.Instance.OnCurrencyChanged += HandleCurrencyChanged;
        else           SoulstoneManager.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
    }

    private void HandleCurrencyChanged(int newAmount)
    {
        RefreshHeader();
        RefreshCandidateButtonsAffordability();
    }

    // ----------------------------------------------------------
    // 전체 갱신 — 진입/리롤/고용 후 호출
    // ----------------------------------------------------------
    private void RebuildAll()
    {
        RefreshHeader();
        RebuildCandidates();
        RebuildReserves();
    }

    private void RefreshHeader()
    {
        if (soulstoneLabel != null)
            soulstoneLabel.text = SoulstoneManager.Instance != null
                ? SoulstoneManager.Instance.Amount.ToString()
                : "0";

        if (rerollCostLabel != null && MercenaryService.Instance != null)
            rerollCostLabel.text = $"리롤 ({MercenaryService.Instance.NextRerollCost})";

        if (rerollButton != null)
            rerollButton.interactable = CanAffordReroll();
    }

    private bool CanAffordReroll()
    {
        if (MercenaryService.Instance == null || SoulstoneManager.Instance == null) return false;
        return SoulstoneManager.Instance.Amount >= MercenaryService.Instance.NextRerollCost;
    }

    // ----------------------------------------------------------
    // 후보 3카드 빌드
    // ----------------------------------------------------------
    private void RebuildCandidates()
    {
        ClearCardList(_candidateCards);
        if (fellowCardPrefab == null || candidatesParent == null) return;
        if (MercenaryService.Instance == null) return;

        var list = MercenaryService.Instance.Candidates;
        for (int i = 0; i < list.Count; i++)
        {
            var card = Instantiate(fellowCardPrefab, candidatesParent);
            card.Bind(list[i], FellowCardMode.Recruit);
            int capturedIndex = i;
            card.OnActionClicked += _ => HandleHire(capturedIndex);
            _candidateCards.Add(card);
        }
        RefreshCandidateButtonsAffordability();
    }

    private void RefreshCandidateButtonsAffordability()
    {
        if (MercenaryService.Instance == null || SoulstoneManager.Instance == null) return;
        var list = MercenaryService.Instance.Candidates;
        for (int i = 0; i < _candidateCards.Count && i < list.Count; i++)
        {
            var f = list[i];
            bool canAfford = f != null && SoulstoneManager.Instance.Amount >= f.recruitCost;
            _candidateCards[i].SetInteractable(canAfford);
        }
    }

    // ----------------------------------------------------------
    // 예비대 미니뷰
    // ----------------------------------------------------------
    private void RebuildReserves()
    {
        ClearCardList(_reserveCards);
        if (fellowCardPrefab == null || reservesParent == null) return;
        if (MercenaryService.Instance == null) return;

        var list = MercenaryService.Instance.Reserves;
        for (int i = 0; i < list.Count; i++)
        {
            var card = Instantiate(fellowCardPrefab, reservesParent);
            card.Bind(list[i], FellowCardMode.Reserve);
            _reserveCards.Add(card);
        }
    }

    // ----------------------------------------------------------
    // 액션
    // ----------------------------------------------------------
    private void HandleHire(int candidateIndex)
    {
        if (MercenaryService.Instance == null) return;
        bool ok = MercenaryService.Instance.TryHire(candidateIndex);
        if (ok) RebuildAll();
    }

    private void HandleReroll()
    {
        if (MercenaryService.Instance == null) return;
        bool ok = MercenaryService.Instance.TryReroll();
        if (ok) RebuildAll();
    }

    // ----------------------------------------------------------
    // 유틸
    // ----------------------------------------------------------
    private static void ClearCardList(List<FellowCardView> list)
    {
        foreach (var c in list)
            if (c != null) Destroy(c.gameObject);
        list.Clear();
    }
}
