// ============================================================
// Mercenary/GrowthPanel.cs
// 동료 성장 패널 — 예비대 3명 합성 → 상위 성급
// ============================================================
//
// [동작]
//   1. 예비대 리스트에서 카드 클릭 → 합성 슬롯에 추가 (최대 3)
//   2. 같은 슬롯/리스트 카드 다시 클릭 → 선택 해제
//   3. 슬롯 3 채워지면 합성 버튼 활성화
//   4. 합성 버튼 클릭 → MercenaryService.TrySynthesize → 결과 동료 예비대로
//
// [규칙]
//   - 같은 성급 3명만 가능 (기획 백로그 §4)
//   - 비용 무료 (사용자 결정)
//   - 결과는 예비대로 추가 (3 제거 후 1 추가 → 슬롯 부족 0)
//
// [인스펙터 슬롯]
//   - fellowCardPrefab    : 카드 프리팹
//   - reservesParent      : 예비대 카드 부모 (Grid Layout)
//   - synthSlotsParent    : 합성 슬롯 3 컨테이너 (Horizontal Layout, 3칸 고정)
//   - synthSlot1/2/3      : 합성 슬롯 카드 (FellowCardView 가 부착된, 미리 배치된 3개)
//   - synthesizeButton    : 합성 실행 버튼
//   - resultPreview       : 결과 카드 (FellowCardView, 미리 배치)
//   - statusLabel         : 안내 라벨 ("3명을 선택해주세요" 등)
//   - closeButton         : 닫기 버튼
// ============================================================

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GrowthPanel : MercenaryPanelBase
{
    [Header("프리팹 / 부모")]
    [SerializeField] private FellowCardView fellowCardPrefab;
    [SerializeField] private Transform      reservesParent;

    [Header("합성 슬롯 (씬에 미리 배치된 3개)")]
    [SerializeField] private FellowCardView synthSlot1;
    [SerializeField] private FellowCardView synthSlot2;
    [SerializeField] private FellowCardView synthSlot3;

    [Header("결과 미리보기")]
    [SerializeField] private FellowCardView resultPreview;

    [Header("버튼 / 라벨")]
    [SerializeField] private Button   synthesizeButton;
    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private Button   closeButton;

    // 합성 슬롯에 선택된 예비대 인덱스. -1 이면 빈 슬롯.
    private readonly int[] _selectedReserveIndices = new int[3] { -1, -1, -1 };

    // 예비대 카드 인스턴스 풀
    private readonly List<FellowCardView> _reserveCards = new();

    protected override void Awake()
    {
        base.Awake();
        if (synthesizeButton != null) synthesizeButton.onClick.AddListener(HandleSynthesize);
        if (closeButton      != null) closeButton.onClick.AddListener(Close);

        // 합성 슬롯 클릭 → 해당 슬롯 비우기 (선택 해제)
        WireSynthSlotClick(synthSlot1, 0);
        WireSynthSlotClick(synthSlot2, 1);
        WireSynthSlotClick(synthSlot3, 2);
    }

    private void WireSynthSlotClick(FellowCardView slot, int slotIndex)
    {
        if (slot == null) return;
        slot.OnActionClicked += _ => ClearSynthSlot(slotIndex);
    }

    protected override void OnOpened()
    {
        ResetSelection();
        RebuildReserves();
        RefreshSynthSlots();
        RefreshResultPreview();
        RefreshStatus();
    }

    // ----------------------------------------------------------
    // 예비대 리빌드
    // ----------------------------------------------------------
    private void RebuildReserves()
    {
        foreach (var c in _reserveCards)
            if (c != null) Destroy(c.gameObject);
        _reserveCards.Clear();

        if (fellowCardPrefab == null || reservesParent == null) return;
        if (MercenaryService.Instance == null) return;

        var list = MercenaryService.Instance.Reserves;
        for (int i = 0; i < list.Count; i++)
        {
            var card = Instantiate(fellowCardPrefab, reservesParent);
            card.Bind(list[i], FellowCardMode.SynthesizeSlot);
            int capturedIndex = i;
            card.OnActionClicked += _ => HandleReserveClicked(capturedIndex);
            _reserveCards.Add(card);
        }
        RefreshReserveSelectionVisual();
    }

    private void RefreshReserveSelectionVisual()
    {
        for (int i = 0; i < _reserveCards.Count; i++)
        {
            bool selected = System.Array.IndexOf(_selectedReserveIndices, i) >= 0;
            _reserveCards[i].SetSelected(selected);
        }
    }

    // ----------------------------------------------------------
    // 예비대 카드 클릭 — 합성 슬롯에 추가 / 이미 선택돼있으면 해제
    // ----------------------------------------------------------
    private void HandleReserveClicked(int reserveIndex)
    {
        // 이미 선택돼 있으면 해제
        int already = System.Array.IndexOf(_selectedReserveIndices, reserveIndex);
        if (already >= 0)
        {
            _selectedReserveIndices[already] = -1;
        }
        else
        {
            // 빈 슬롯 첫 자리에 채움
            int empty = System.Array.IndexOf(_selectedReserveIndices, -1);
            if (empty < 0) return; // 슬롯 다 차있음 — 무시
            _selectedReserveIndices[empty] = reserveIndex;
        }

        RefreshReserveSelectionVisual();
        RefreshSynthSlots();
        RefreshResultPreview();
        RefreshStatus();
    }

    private void ClearSynthSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 3) return;
        _selectedReserveIndices[slotIndex] = -1;
        RefreshReserveSelectionVisual();
        RefreshSynthSlots();
        RefreshResultPreview();
        RefreshStatus();
    }

    // ----------------------------------------------------------
    // 합성 슬롯 표시 갱신
    // ----------------------------------------------------------
    private void RefreshSynthSlots()
    {
        BindSynthSlot(synthSlot1, _selectedReserveIndices[0]);
        BindSynthSlot(synthSlot2, _selectedReserveIndices[1]);
        BindSynthSlot(synthSlot3, _selectedReserveIndices[2]);
    }

    private void BindSynthSlot(FellowCardView slot, int reserveIndex)
    {
        if (slot == null) return;
        if (reserveIndex < 0 || MercenaryService.Instance == null)
        {
            slot.BindEmpty(FellowCardMode.SynthesizeSlot);
            return;
        }
        var fellow = MercenaryService.Instance.GetReserve(reserveIndex);
        slot.Bind(fellow, FellowCardMode.SynthesizeSlot);
        slot.SetSelected(true);
    }

    // ----------------------------------------------------------
    // 결과 미리보기 + 합성 가능 여부 / 안내 라벨
    // ----------------------------------------------------------
    private void RefreshResultPreview()
    {
        if (resultPreview == null) return;
        // 미리보기는 단순 빈 카드 표시 — 실제 결과는 합성 후에만 (4가지 케이스 + 랜덤이라 정확한 미리보기 불가)
        resultPreview.BindEmpty(FellowCardMode.Reserve);
    }

    private void RefreshStatus()
    {
        int filled = CountFilledSlots();
        bool canSynth = filled == 3 && AllSameStar();

        if (statusLabel != null)
        {
            if (filled < 3)
                statusLabel.text = $"{3 - filled}명을 더 선택해주세요";
            else if (!AllSameStar())
                statusLabel.text = "같은 성급의 동료 3명을 선택해주세요";
            else
                statusLabel.text = "합성 준비 완료";
        }
        if (synthesizeButton != null) synthesizeButton.interactable = canSynth;
    }

    private int CountFilledSlots()
    {
        int n = 0;
        for (int i = 0; i < 3; i++) if (_selectedReserveIndices[i] >= 0) n++;
        return n;
    }

    private bool AllSameStar()
    {
        if (MercenaryService.Instance == null) return false;
        int? baseStar = null;
        for (int i = 0; i < 3; i++)
        {
            int idx = _selectedReserveIndices[i];
            if (idx < 0) return false;
            var f = MercenaryService.Instance.GetReserve(idx);
            if (f == null) return false;
            if (baseStar == null) baseStar = f.starLevel;
            else if (f.starLevel != baseStar.Value) return false;
        }
        return true;
    }

    // ----------------------------------------------------------
    // 합성 실행
    // ----------------------------------------------------------
    private void HandleSynthesize()
    {
        if (MercenaryService.Instance == null) return;

        bool ok = MercenaryService.Instance.TrySynthesize(
            _selectedReserveIndices[0],
            _selectedReserveIndices[1],
            _selectedReserveIndices[2],
            out var result);

        if (!ok) return;

        // 결과 카드 표시
        if (resultPreview != null && result != null)
            resultPreview.Bind(result, FellowCardMode.Reserve);

        // 선택 리셋 + 예비대 재빌드
        ResetSelection();
        RebuildReserves();
        RefreshSynthSlots();
        RefreshStatus();
    }

    private void ResetSelection()
    {
        for (int i = 0; i < 3; i++) _selectedReserveIndices[i] = -1;
    }
}
