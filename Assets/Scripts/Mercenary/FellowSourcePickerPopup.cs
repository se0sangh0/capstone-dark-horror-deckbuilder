// ============================================================
// Mercenary/FellowSourcePickerPopup.cs
// 동료 명단 모달 — 파티/예비대 영역을 동시에 표시
// ============================================================
//
// [모드]
//   Synthesize : GrowthPanel — 빈 합성 슬롯 채울 동료 선택
//                · 카드 [선택] → onPicked 콜백 + 자동 Close
//                · 카드 [X]    → "지원하지 않는 기능입니다" 토스트
//   Recruit    : RecruitPanel — 파티/예비대 전체 명단 열람
//                · 파티 카드   [선택]/[X] → 토스트
//                · 예비대 카드 [선택]      → 토스트
//                · 예비대 카드 [X]         → 방출 (DismissReserve)
//
// [공통]
//   [×] 닫기 또는 배경 클릭 → onCanceled 콜백 + Close
//   파티 영역과 예비대 영역이 각자 카드 그리드를 가짐 (탭 없음)
//
// [인스펙터 슬롯]
//   - titleLabel         : 상단 제목 TMP_Text
//   - toastLabel         : 하단 토스트 TMP_Text (2초 안내)
//   - partyGridParent    : 파티 카드 그리드 부모 (Grid Layout)
//   - reserveGridParent  : 예비대 카드 그리드 부모 (Grid Layout)
//   - fellowCardPrefab   : CardPrefab (FellowCardView)
//   - closeButton        : [×] 닫기 Button
//   - backgroundButton   : (선택) 어두운 배경 Button — 클릭 시 닫기
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FellowSourcePickerPopup : PanelBase
{
    public enum PickerMode { Synthesize, Recruit }

    [Header("UI")]
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text toastLabel;
    [SerializeField] private Transform partyGridParent;
    [SerializeField] private Transform reserveGridParent;
    [SerializeField] private FellowCardView fellowCardPrefab;
    [SerializeField] private Button   closeButton;
    [SerializeField] private Button   backgroundButton;

    [Header("토스트")]
    [SerializeField] private float toastDuration = 2f;

    private const string UnsupportedMessage = "지원하지 않는 기능입니다";

    private readonly List<FellowCardView> _partyCards   = new();
    private readonly List<FellowCardView> _reserveCards = new();
    private HashSet<FellowData> _excluded;
    private Action<FellowData>  _onPicked;
    private Action              _onCanceled;
    private PickerMode          _mode = PickerMode.Synthesize;
    private Coroutine           _toastRoutine;

    protected override void Awake()
    {
        base.Awake();
        if (closeButton      != null) closeButton.onClick.AddListener(HandleCancel);
        if (backgroundButton != null) backgroundButton.onClick.AddListener(HandleCancel);
        HideToast();
    }

    // ----------------------------------------------------------
    // 진입점 — GrowthPanel (합성 소스 선택)
    // ----------------------------------------------------------
    public void OpenForSlot(int slotIndex, HashSet<FellowData> excluded, Action<FellowData> onPicked, Action onCanceled)
    {
        _mode       = PickerMode.Synthesize;
        _excluded   = excluded ?? new HashSet<FellowData>();
        _onPicked   = onPicked;
        _onCanceled = onCanceled;

        if (titleLabel != null) titleLabel.text = $"슬롯 {slotIndex + 1} 에 넣을 동료 선택";
        Open();
    }

    // ----------------------------------------------------------
    // 진입점 — RecruitPanel (파티/예비대 명단 열람)
    // ----------------------------------------------------------
    public void OpenForRecruit(Action onClosed = null)
    {
        _mode       = PickerMode.Recruit;
        _excluded   = new HashSet<FellowData>();
        _onPicked   = null;
        _onCanceled = onClosed;

        if (titleLabel != null) titleLabel.text = "동료 명단";
        Open();
    }

    protected override void OnOpened()
    {
        HideToast();
        RebuildAll();
    }

    protected override void OnClosed()
    {
        ClearCardList(_partyCards);
        ClearCardList(_reserveCards);
        HideToast();
    }

    // ----------------------------------------------------------
    // 카드 그리드 빌드 — 파티/예비대 동시
    // ----------------------------------------------------------
    private void RebuildAll()
    {
        RebuildPartyCards();
        RebuildReserveCards();
    }

    private void RebuildPartyCards()
    {
        ClearCardList(_partyCards);
        if (fellowCardPrefab == null || partyGridParent == null) return;
        if (PartyManager.Instance == null) return;

        foreach (var fellow in PartyManager.Instance.GetActiveFellows())
        {
            if (fellow == null) continue;
            var card = Instantiate(fellowCardPrefab, partyGridParent);
            card.Bind(fellow, FellowCardMode.SynthesizeSlot);
            WirePartyCard(card, fellow);
            _partyCards.Add(card);
        }
    }

    private void RebuildReserveCards()
    {
        ClearCardList(_reserveCards);
        if (fellowCardPrefab == null || reserveGridParent == null) return;
        if (MercenaryService.Instance == null) return;

        foreach (var fellow in MercenaryService.Instance.Reserves)
        {
            if (fellow == null) continue;
            var card = Instantiate(fellowCardPrefab, reserveGridParent);
            card.Bind(fellow, FellowCardMode.SynthesizeSlot);
            WireReserveCard(card, fellow);
            _reserveCards.Add(card);
        }
    }

    private void WirePartyCard(FellowCardView card, FellowData fellow)
    {
        if (_mode == PickerMode.Synthesize)
        {
            bool isExcluded = _excluded.Contains(fellow);
            card.SetInteractable(!isExcluded);
            card.SetSelected(isExcluded);
            if (!isExcluded) card.OnActionClicked += _ => HandlePicked(fellow);
            card.OnRemoveClicked += _ => ShowToast(UnsupportedMessage);
            return;
        }
        // Recruit: 파티 카드는 선택/X 모두 지원 안 함
        card.OnActionClicked += _ => ShowToast(UnsupportedMessage);
        card.OnRemoveClicked += _ => ShowToast(UnsupportedMessage);
    }

    private void WireReserveCard(FellowCardView card, FellowData fellow)
    {
        if (_mode == PickerMode.Synthesize)
        {
            bool isExcluded = _excluded.Contains(fellow);
            card.SetInteractable(!isExcluded);
            card.SetSelected(isExcluded);
            if (!isExcluded) card.OnActionClicked += _ => HandlePicked(fellow);
            card.OnRemoveClicked += _ => ShowToast(UnsupportedMessage);
            return;
        }
        // Recruit: 예비대 선택=토스트, X=방출
        card.OnActionClicked += _ => ShowToast(UnsupportedMessage);
        card.OnRemoveClicked += _ => HandleReserveDismiss(fellow);
    }

    private void HandleReserveDismiss(FellowData fellow)
    {
        if (MercenaryService.Instance == null) return;
        if (!MercenaryService.Instance.DismissReserve(fellow)) return;
        RebuildReserveCards();
    }

    private static void ClearCardList(List<FellowCardView> list)
    {
        foreach (var c in list)
            if (c != null) Destroy(c.gameObject);
        list.Clear();
    }

    // ----------------------------------------------------------
    // 토스트
    // ----------------------------------------------------------
    private void ShowToast(string message)
    {
        if (toastLabel == null) return;
        toastLabel.text = message;
        toastLabel.gameObject.SetActive(true);
        if (_toastRoutine != null) StopCoroutine(_toastRoutine);
        _toastRoutine = StartCoroutine(HideToastAfter(toastDuration));
    }

    private IEnumerator HideToastAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        HideToast();
        _toastRoutine = null;
    }

    private void HideToast()
    {
        if (toastLabel == null) return;
        toastLabel.text = string.Empty;
        toastLabel.gameObject.SetActive(false);
    }

    // ----------------------------------------------------------
    // 선택 / 취소 (Synthesize 모드 전용)
    // ----------------------------------------------------------
    private void HandlePicked(FellowData fellow)
    {
        var cb = _onPicked;
        _onPicked   = null;
        _onCanceled = null;
        Close();
        cb?.Invoke(fellow);
    }

    private void HandleCancel()
    {
        var cb = _onCanceled;
        _onPicked   = null;
        _onCanceled = null;
        Close();
        cb?.Invoke();
    }
}
