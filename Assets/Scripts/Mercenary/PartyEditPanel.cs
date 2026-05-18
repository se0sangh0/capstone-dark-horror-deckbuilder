// ============================================================
// Mercenary/PartyEditPanel.cs
// 파티 편집 패널 — 파티 4슬롯 + 예비대 + 교체/제거
// ============================================================
//
// [동작]
//   - 파티 슬롯 1~4 표시 (FellowCardView, PartySlot 모드 — 풀 카드)
//   - 예비대 카드 표시 (FellowCardView, Reserve 모드 — 풀 카드, ScrollRect 안에 배치)
//   - 파티 슬롯 클릭 → 선택. 다시 예비대 카드 클릭하면 교체.
//   - 파티 슬롯의 [제거] 클릭 → 그 동료를 예비대로 빼냄.
//   - 예비대 카드 클릭(파티 선택 없음) → 파티 빈 슬롯에 즉시 합류.
//   - 예비대 카드의 [제거] 클릭 → 예비대에서 영구 제거 (방출, DismissReserve)
//
// [statusLabel — 안내 + 토스트 겸용]
//   평소     : 사용법 안내 표시
//   주의 시  : "지원하지 않는 기능입니다" 2초 표시 후 안내로 자동 복귀
//
// [인스펙터 슬롯]
//   - fellowCardPrefab    : 풀 카드 (FellowCardView)
//   - partySlotsParent    : 파티 4슬롯 부모
//   - reservesParent      : 예비대 부모 (ScrollRect Content)
//   - partyCountLabel     : "파티 인원: 2/4" 표시
//   - statusLabel         : 안내/토스트 겸용 TMP_Text
//   - closeButton         : 닫기 버튼
// ============================================================

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyEditPanel : PanelBase
{
    [Header("프리팹 / 부모")]
    [SerializeField] private FellowCardView fellowCardPrefab;
    [SerializeField] private Transform      partySlotsParent;
    [SerializeField] private Transform      reservesParent;

    [Header("UI 라벨 / 버튼")]
    [SerializeField] private TMP_Text partyCountLabel;
    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private Button   closeButton;

    [Header("토스트")]
    [SerializeField] private float toastDuration = 2f;

    private const int    PartySize          = 4;
    private const string DefaultGuide       = "파티 슬롯을 선택해 교체하거나, 예비대 카드를 클릭해 빈 슬롯에 합류시키세요.";
    private const string UnsupportedMessage = "지원하지 않는 기능입니다";

    // 카드 인스턴스 풀
    private readonly List<FellowCardView> _partyCards   = new();
    private readonly List<FellowCardView> _reserveCards = new();

    // 현재 선택된 파티 슬롯 인덱스 (없으면 -1).
    private int _selectedPartyIndex = -1;

    private Coroutine _toastRoutine;

    protected override void Awake()
    {
        base.Awake();
        if (closeButton != null) closeButton.onClick.AddListener(Close);
    }

    protected override void OnOpened()
    {
        _selectedPartyIndex = -1;
        RebuildAll();
        ShowGuide();
    }

    // ----------------------------------------------------------
    // 전체 갱신
    // ----------------------------------------------------------
    private void RebuildAll()
    {
        RebuildPartySlots();
        RebuildReserves();
        RefreshHeader();
    }

    private void RefreshHeader()
    {
        if (partyCountLabel != null && PartyManager.Instance != null)
            partyCountLabel.text = $"파티 인원: {PartyManager.Instance.CompanionCount}/{PartySize}";
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
            card.OnActionClicked  += _ => HandlePartySlotClicked(capturedIndex, capturedFellow);
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
    // 예비대 빌드 — 풀 카드 (FellowCardView, Reserve 모드)
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
            var capturedFellow = list[i];
            card.OnActionClicked += _ => HandleReserveClicked(capturedIndex);
            card.OnRemoveClicked += _ => HandleReserveDismiss(capturedFellow);
            _reserveCards.Add(card);
        }
    }

    private void HandleReserveDismiss(FellowData fellow)
    {
        if (MercenaryService.Instance == null) return;
        if (!MercenaryService.Instance.DismissReserve(fellow)) return;
        _selectedPartyIndex = -1;
        RebuildAll();
        ShowGuide();
    }

    // ----------------------------------------------------------
    // 클릭 핸들러
    // ----------------------------------------------------------

    /// <summary>파티 슬롯 클릭 — 빈 슬롯은 지원 안 함, 채워진 슬롯은 선택 토글.</summary>
    private void HandlePartySlotClicked(int slotIndex, FellowData fellow)
    {
        if (fellow == null)
        {
            ShowToast(UnsupportedMessage);
            return;
        }
        _selectedPartyIndex = (_selectedPartyIndex == slotIndex) ? -1 : slotIndex;
        RefreshPartySelectionVisual();
    }

    /// <summary>파티 슬롯의 [제거] 클릭 — 예비대로 빼낸다.</summary>
    private void HandlePartyRemove(FellowData partyFellow)
    {
        if (partyFellow == null)
        {
            ShowToast(UnsupportedMessage);
            return;
        }
        if (MercenaryService.Instance == null) return;
        bool ok = MercenaryService.Instance.TryMovePartyToReserve(partyFellow);
        if (!ok)
        {
            ShowToast(UnsupportedMessage);
            return;
        }
        _selectedPartyIndex = -1;
        RebuildAll();
        ShowGuide();
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
                MercenaryService.Instance.TryAssignReserveToParty(reserveIndex);
            else
                MercenaryService.Instance.TrySwapPartyAndReserve(fellows[_selectedPartyIndex], reserveIndex);

            _selectedPartyIndex = -1;
            RebuildAll();
            ShowGuide();
            return;
        }

        // 파티 슬롯 선택 없음 → 빈 슬롯 합류 시도. 만석이면 지원 안 함.
        if (PartyManager.Instance.CompanionCount < PartySize)
        {
            MercenaryService.Instance.TryAssignReserveToParty(reserveIndex);
            RebuildAll();
            ShowGuide();
        }
        else
        {
            ShowToast(UnsupportedMessage);
        }
    }

    // ----------------------------------------------------------
    // 안내 / 토스트
    // ----------------------------------------------------------
    private void ShowGuide()
    {
        if (statusLabel == null) return;
        if (_toastRoutine != null)
        {
            StopCoroutine(_toastRoutine);
            _toastRoutine = null;
        }
        statusLabel.text = DefaultGuide;
    }

    private void ShowToast(string message)
    {
        if (statusLabel == null) return;
        statusLabel.text = message;
        if (_toastRoutine != null) StopCoroutine(_toastRoutine);
        _toastRoutine = StartCoroutine(RestoreGuideAfter(toastDuration));
    }

    private IEnumerator RestoreGuideAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (statusLabel != null) statusLabel.text = DefaultGuide;
        _toastRoutine = null;
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
