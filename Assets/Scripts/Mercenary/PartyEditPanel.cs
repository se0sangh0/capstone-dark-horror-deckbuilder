// ============================================================
// Mercenary/PartyEditPanel.cs
// 파티 편집 패널 — 파티 4슬롯 + 예비대 + 교체/제거
// ============================================================
//
// [동작]
//   - 파티 슬롯 1~4 표시 (FellowCardView, PartySlot 모드)
//   - 예비대 카드 표시 (FellowCardView, Reserve 모드)
//   - 파티 슬롯 클릭 → 선택. 다시 예비대 카드 클릭하면 교체.
//   - 파티 슬롯의 [제거] 클릭 → 그 동료를 예비대로 빼냄.
//   - 예비대 카드 클릭(파티 선택 없음) → 파티 빈 슬롯에 즉시 합류.
//   - 닫기 → 메인 패널 복귀
//
// [인스펙터 슬롯]
//   - fellowCardPrefab    : 카드 프리팹
//   - partySlotsParent    : 파티 4슬롯 부모 (Horizontal Layout)
//   - reservesParent      : 예비대 부모 (Grid Layout)
//   - partyCountLabel     : "파티 인원: 2/4" 같은 표시
//   - statusLabel         : 안내 라벨 (선택/교체 안내)
//   - closeButton         : 닫기 버튼
// ============================================================

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyEditPanel : MercenaryPanelBase
{
    [Header("프리팹 / 부모")]
    [SerializeField] private FellowCardView fellowCardPrefab;
    [SerializeField] private Transform      partySlotsParent;
    [SerializeField] private Transform      reservesParent;

    [Header("UI 라벨 / 버튼")]
    [SerializeField] private TMP_Text partyCountLabel;
    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private Button   closeButton;

    private const int PartySize = 4;

    // 카드 인스턴스 풀
    private readonly List<FellowCardView> _partyCards   = new();
    private readonly List<FellowCardView> _reserveCards = new();

    // 현재 선택된 파티 슬롯 인덱스 (없으면 -1). 예비대 카드 클릭 시 교체 대상.
    private int _selectedPartyIndex = -1;

    protected override void Awake()
    {
        base.Awake();
        if (closeButton != null) closeButton.onClick.AddListener(Close);
    }

    protected override void OnOpened()
    {
        _selectedPartyIndex = -1;
        RebuildAll();
    }

    // ----------------------------------------------------------
    // 전체 갱신
    // ----------------------------------------------------------
    private void RebuildAll()
    {
        RebuildPartySlots();
        RebuildReserves();
        RefreshHeader();
        RefreshStatus();
    }

    private void RefreshHeader()
    {
        if (partyCountLabel != null && PartyManager.Instance != null)
            partyCountLabel.text = $"파티 인원: {PartyManager.Instance.CompanionCount}/{PartySize}";
    }

    private void RefreshStatus()
    {
        if (statusLabel == null) return;
        if (_selectedPartyIndex < 0)
            statusLabel.text = "파티 슬롯을 선택해 교체하거나, 예비대에서 카드를 클릭해 빈 슬롯에 합류시키세요.";
        else
            statusLabel.text = "예비대에서 카드를 클릭하면 선택된 파티원과 교체됩니다. (파티 슬롯 다시 클릭 → 선택 해제)";
    }

    // ----------------------------------------------------------
    // 파티 슬롯 빌드 (항상 4칸 — 빈 슬롯은 BindEmpty)
    // ----------------------------------------------------------
    private void RebuildPartySlots()
    {
        ClearCardList(_partyCards);
        if (fellowCardPrefab == null || partySlotsParent == null) return;
        if (PartyManager.Instance == null) return;

        var fellows = PartyManager.Instance.GetActiveFellows();
        for (int i = 0; i < PartySize; i++)
        {
            var card = Instantiate(fellowCardPrefab, partySlotsParent);
            FellowData fellow = (i < fellows.Count) ? fellows[i] : null;
            card.Bind(fellow, FellowCardMode.PartySlot);

            int capturedIndex = i;
            FellowData capturedFellow = fellow;
            card.OnActionClicked  += _ => HandlePartySlotClicked(capturedIndex);
            card.OnRemoveClicked  += _ => HandlePartyRemove(capturedFellow);
            _partyCards.Add(card);
        }
        RefreshPartySelectionVisual();
    }

    private void RefreshPartySelectionVisual()
    {
        for (int i = 0; i < _partyCards.Count; i++)
            _partyCards[i].SetSelected(i == _selectedPartyIndex);
    }

    // ----------------------------------------------------------
    // 예비대 빌드
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
            int capturedIndex = i;
            card.OnActionClicked += _ => HandleReserveClicked(capturedIndex);
            _reserveCards.Add(card);
        }
    }

    // ----------------------------------------------------------
    // 클릭 핸들러
    // ----------------------------------------------------------

    /// <summary>파티 슬롯 클릭 — 선택 토글.</summary>
    private void HandlePartySlotClicked(int slotIndex)
    {
        _selectedPartyIndex = (_selectedPartyIndex == slotIndex) ? -1 : slotIndex;
        RefreshPartySelectionVisual();
        RefreshStatus();
    }

    /// <summary>파티 슬롯의 [제거] 클릭 — 예비대로 빼낸다.</summary>
    private void HandlePartyRemove(FellowData partyFellow)
    {
        if (partyFellow == null || MercenaryService.Instance == null) return;
        bool ok = MercenaryService.Instance.TryMovePartyToReserve(partyFellow);
        if (!ok)
        {
            Debug.Log("[PartyEdit] 제거 실패 — 예비대 만석");
            return;
        }
        _selectedPartyIndex = -1;
        RebuildAll();
    }

    /// <summary>예비대 카드 클릭 — 선택된 파티원과 교체, 없으면 빈 슬롯 합류.</summary>
    private void HandleReserveClicked(int reserveIndex)
    {
        if (MercenaryService.Instance == null) return;

        // 파티 슬롯 선택 상태 → 교체
        if (_selectedPartyIndex >= 0)
        {
            var fellows = PartyManager.Instance.GetActiveFellows();
            if (_selectedPartyIndex >= fellows.Count)
            {
                // 빈 파티 슬롯이 선택된 경우 → 합류로 폴백
                MercenaryService.Instance.TryAssignReserveToParty(reserveIndex);
            }
            else
            {
                MercenaryService.Instance.TrySwapPartyAndReserve(fellows[_selectedPartyIndex], reserveIndex);
            }
            _selectedPartyIndex = -1;
            RebuildAll();
            return;
        }

        // 파티 슬롯 선택 없음 → 빈 슬롯 합류 시도 (만석이면 무시)
        if (PartyManager.Instance.CompanionCount < PartySize)
        {
            MercenaryService.Instance.TryAssignReserveToParty(reserveIndex);
            RebuildAll();
        }
        else
        {
            if (statusLabel != null) statusLabel.text = "파티가 가득 찼습니다. 교체할 파티원을 먼저 선택하세요.";
        }
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
