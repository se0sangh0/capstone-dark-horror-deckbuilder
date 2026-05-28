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
    [SerializeField] private Transform      reservesParent;

    [Header("파티 슬롯 (4개 고정 — Inspector 에 직접 배치)")]
    [Tooltip("파티 슬롯 4개. 인덱스 0~3 단일 순번. 비워두면 폴백으로 instantiate.")]
    [SerializeField] private FellowCardView[] partySlots = new FellowCardView[4];

    [Header("UI 라벨 / 버튼")]
    [SerializeField] private TMP_Text partyCountLabel;
    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private Button   closeButton;

    [Header("토스트")]
    [SerializeField] private float toastDuration = 2f;

    private const int    PartySize          = 4;
    private const string DefaultGuide       = "파티 슬롯을 선택해 교체하거나, 예비대 카드를 클릭해 빈 슬롯에 합류시키세요.";
    private const string UnsupportedMessage = "지원하지 않는 기능입니다";

    // 카드 인스턴스 풀 — 예비대는 가변, 파티는 고정 슬롯이라 풀 불필요
    private readonly List<FellowCardView> _reserveCards = new();

    // 현재 선택된 파티 슬롯 인덱스 (없으면 -1).
    private int _selectedPartyIndex = -1;

    private Coroutine _toastRoutine;

    protected override void Awake()
    {
        base.Awake();
        if (closeButton != null) closeButton.onClick.AddListener(Close);

        // 고정 슬롯 → 핸들러 1회 구독 (fellow 는 클릭 시점에 PartyManager 에서 동적 조회).
        if (partySlots != null)
        {
            for (int i = 0; i < partySlots.Length && i < PartySize; i++)
            {
                int capIdx = i;
                var slot = partySlots[i];
                if (slot == null) continue;
                slot.OnActionClicked += _ => HandlePartySlotClicked(capIdx);
                slot.OnRemoveClicked += _ => HandlePartyRemove(capIdx);
            }
        }
    }

    // 전투 노드 진입 중에는 파티 편집 금지 — 무시하고 Open 자체를 건너뜀.
    // BattleManager.gameObject 의 부모(RightMainArea)는 BattleNode 진입 시에만 활성화되므로
    // activeInHierarchy 가 곧 "전투 노드 안" 신호.
    public override void Open()
    {
        if (BattleManager.Instance != null
            && BattleManager.Instance.gameObject.activeInHierarchy
            && BattleManager.Instance.currentPhase != BattlePhase.BattleEnd)
        {
            Debug.Log("[PartyEditPanel] 전투 노드 진행 중 — 파티 편집 진입 차단");
            return;
        }
        base.Open();
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
    // 파티 슬롯 빌드 (항상 4칸 — Inspector 의 고정 슬롯에 Bind)
    // ----------------------------------------------------------
    private void RebuildPartySlots()
    {
        if (PartyManager.Instance == null) return;
        if (partySlots == null || partySlots.Length < PartySize) return;

        var fellows = PartyManager.Instance.GetActiveFellows();
        for (int i = 0; i < PartySize; i++)
        {
            var card = partySlots[i];
            if (card == null) continue;

            FellowData fellow = (i < fellows.Count) ? fellows[i] : null;
            card.Bind(fellow, FellowCardMode.PartySlot);
            // 핸들러는 Awake 에서 1회 구독되어 있으므로 여기서는 Bind 만.
        }
        RefreshPartySelectionVisual();
    }

    private void RefreshPartySelectionVisual()
    {
        for (int i = 0; i < PartySize; i++)
        {
            if (partySlots[i] != null)
                partySlots[i].SetSelected(i == _selectedPartyIndex);
        }
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

    /// <summary>파티 슬롯 클릭 — 선택 토글, 다른 슬롯 두 번째 클릭 시 두 슬롯 간 순서 교환.</summary>
    private void HandlePartySlotClicked(int slotIndex)
    {
        var fellow = GetPartyFellowAt(slotIndex);
        if (fellow == null)
        {
            ShowToast(UnsupportedMessage);
            return;
        }

        // 이미 다른 파티 슬롯이 선택된 상태에서 다른 채워진 슬롯 클릭 → 두 슬롯 순서 교환
        if (_selectedPartyIndex >= 0 && _selectedPartyIndex != slotIndex)
        {
            if (PartyManager.Instance != null && PartyManager.Instance.SwapFellows(_selectedPartyIndex, slotIndex))
            {
                _selectedPartyIndex = -1;
                RebuildAll();
                ShowGuide();
                return;
            }
        }

        // 같은 슬롯 재클릭 → 선택 해제, 처음 선택이면 → 선택 설정
        _selectedPartyIndex = (_selectedPartyIndex == slotIndex) ? -1 : slotIndex;
        RefreshPartySelectionVisual();
    }

    /// <summary>파티 슬롯의 [제거] 클릭 — 예비대로 빼낸다.</summary>
    private void HandlePartyRemove(int slotIndex)
    {
        var partyFellow = GetPartyFellowAt(slotIndex);
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

    private static FellowData GetPartyFellowAt(int slotIndex)
    {
        if (PartyManager.Instance == null) return null;
        var fellows = PartyManager.Instance.GetActiveFellows();
        return (slotIndex >= 0 && slotIndex < fellows.Count) ? fellows[slotIndex] : null;
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
