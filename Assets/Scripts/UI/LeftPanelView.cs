// LeftPanelView.cs
// LeftPanel.prefab 루트에 부착하는 좌측 사이드 패널 컨트롤러.
//
// ── 역할 ────────────────────────────────────────────────────────
//   - 파티 카드(최대 4슬롯) 표시 — CardSlotView 4개 위임
//   - 덱 구성 요약 — 성향별 그룹(파티에 존재하는 성향만)
//   - 캐릭별 스트레스 행 (이름 + Bar + 점수)
//   - 하단 버튼 — 설정 / 로그 (도움말 제거됨)
//
// ── 갱신 트리거 ────────────────────────────────────────────────
//   - OnEnable 시 PartyManager.Instance.GetActiveFellows() 로 멤버 동기화
//   - FellowData.OnHpChanged / OnShieldChanged / OnStressChanged 이벤트 구독
//   - 외부에서 Refresh() 직접 호출도 가능 (모집/사망 등 파티 멤버 변경 시)

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeftPanelView : MonoBehaviour
{
    [Header("파티 카드 슬롯 (Card_Base_1~4)")]
    [SerializeField] private CardSlotView[] cardSlots = new CardSlotView[4];

    [Header("덱 구성 요약")]
    [Tooltip("성향별 줄을 출력할 TMP_Text. 줄바꿈 join.")]
    [SerializeField] private TMP_Text deckSummaryText;

    [Header("스트레스 행")]
    [Tooltip("StressEntry 프리팹/노드. 멤버 수만큼 활성화하여 사용.")]
    [SerializeField] private StressEntry[] stressEntries = new StressEntry[4];

    [Header("재화 텍스트 (LeftPanel 내부)")]
    [Tooltip("영혼석 ValueText (Item_SoulStone > ValueText). SoulstoneManager 이벤트로 자동 갱신.")]
    [SerializeField] private TMP_Text soulstoneText;
    [Tooltip("마석 ValueText (Magic_SoulStone > ValueText). ManastoneManager 이벤트로 자동 갱신.")]
    [SerializeField] private TMP_Text manastoneText;

    [Header("하단 버튼")]
    [SerializeField] private Button settingButton;
    [SerializeField] private Button logButton;
    // 팝업은 PopupManager 싱글톤이 관리 — 직접 GameObject 참조 불필요.

    private readonly List<FellowData> _bound = new();

    [System.Serializable]
    public class StressEntry
    {
        public GameObject root;          // 행 전체 (활성/비활성)
        public TMP_Text   nameText;
        public TMP_Text   scoreText;     // "38/100"
        public TMP_Text   conditionText; // "안정/압박/패닉" — 검은색 고정
        public Image      conditionBg;   // 상태 박스 배경(Tag_BG) — 상태색 적용
    }

    // ──────────────────────────────────────────────────────────────
    private void OnEnable()
    {
        WireButtons();
        Refresh();
        SubscribeParty(true);
        SubscribeCurrency(true);
    }

    // PartyManager.Start() 가 InitDefaultParty() 를 호출한 직후 한 번 더 갱신.
    // OnEnable 시점에는 아직 파티가 비어 있을 수 있다.
    private void Start()
    {
        Refresh();
        SubscribeParty(true);    // PartyManager 가 Start 에 늦게 등장한 경우 대비
        SubscribeCurrency(true); // SoulstoneManager/ManastoneManager 도 동일
    }

    private void OnDisable()
    {
        SubscribeParty(false);
        SubscribeCurrency(false);
        UnbindAll();
    }

    // 재화 매니저 이벤트 구독 — 인스펙터 amountText 연결 의존 제거
    private void SubscribeCurrency(bool subscribe)
    {
        if (SoulstoneManager.Instance != null)
        {
            SoulstoneManager.Instance.OnCurrencyChanged -= UpdateSoulstoneText;
            if (subscribe)
            {
                SoulstoneManager.Instance.OnCurrencyChanged += UpdateSoulstoneText;
                UpdateSoulstoneText(SoulstoneManager.Instance.Amount); // 초기값 즉시 반영
            }
        }
        if (ManastoneManager.Instance != null)
        {
            ManastoneManager.Instance.OnCurrencyChanged -= UpdateManastoneText;
            if (subscribe)
            {
                ManastoneManager.Instance.OnCurrencyChanged += UpdateManastoneText;
                UpdateManastoneText(ManastoneManager.Instance.Amount);
            }
        }
    }

    private void UpdateSoulstoneText(int amount)
    {
        if (soulstoneText != null) soulstoneText.text = amount.ToString("N0");
    }

    private void UpdateManastoneText(int amount)
    {
        if (manastoneText != null) manastoneText.text = amount.ToString("N0");
    }

    // PartyManager.OnPartyChanged 구독/해제 — 멤버 변경 시 자동 Refresh.
    private void SubscribeParty(bool subscribe)
    {
        if (PartyManager.Instance == null) return;
        PartyManager.Instance.OnPartyChanged -= Refresh;
        if (subscribe) PartyManager.Instance.OnPartyChanged += Refresh;
    }

    private void WireButtons()
    {
        if (settingButton != null)
        {
            settingButton.onClick.RemoveListener(OpenSetting);
            settingButton.onClick.AddListener(OpenSetting);
        }
        if (logButton != null)
        {
            logButton.onClick.RemoveListener(OpenLog);
            logButton.onClick.AddListener(OpenLog);
        }
    }

    private void OpenSetting()
    {
        if (PopupManager.Instance != null) PopupManager.Instance.OpenSetting();
    }

    private void OpenLog()
    {
        if (PopupManager.Instance != null) PopupManager.Instance.OpenLog();
    }

    /// <summary>
    /// 외부에서 호출 가능한 강제 갱신. 파티 멤버 변경(모집/사망) 시 호출.
    /// </summary>
    public void Refresh()
    {
        UnbindAll();

        var party = PartyManager.Instance != null
            ? PartyManager.Instance.GetActiveFellows()
            : new List<FellowData>();

        // 카드 슬롯
        for (int i = 0; i < cardSlots.Length; i++)
        {
            var slot = cardSlots[i];
            if (slot == null) continue;

            if (i < party.Count)
            {
                slot.Bind(party[i]);
            }
            else
            {
                slot.Unbind();
                slot.gameObject.SetActive(false);
            }
        }

        // 스트레스 이벤트 구독 + 행 표시
        foreach (var f in party)
        {
            f.OnStressChanged += OnStressChanged;
            _bound.Add(f);
        }
        RefreshStressRows(party);

        // 덱 요약
        RefreshDeckSummary(party);
    }

    private void UnbindAll()
    {
        foreach (var f in _bound)
            if (f != null) f.OnStressChanged -= OnStressChanged;
        _bound.Clear();
    }

    private void OnStressChanged(int _) => RefreshStressRows(_bound);

    private void RefreshStressRows(IList<FellowData> party)
    {
        for (int i = 0; i < stressEntries.Length; i++)
        {
            var entry = stressEntries[i];
            if (entry == null || entry.root == null) continue;

            if (i < party.Count)
            {
                var f = party[i];
                entry.root.SetActive(true);
                if (entry.nameText      != null) entry.nameText.text      = !string.IsNullOrEmpty(f.displayName) ? f.displayName : f.id;
                if (entry.scoreText     != null) entry.scoreText.text     = $"{f.currentStress}/100";
                if (entry.conditionText != null)
                {
                    entry.conditionText.text  = GetStressLabel(f.currentStress);
                    entry.conditionText.color = Color.black; // 텍스트는 검은색 고정
                }
                if (entry.conditionBg != null)
                    entry.conditionBg.color = GetStressColor(f.currentStress);
            }
            else
            {
                entry.root.SetActive(false);
            }
        }
    }

    private static Color GetStressColor(int stress)
    {
        if (stress >= 100) return new Color(0.85f, 0.25f, 0.25f);    // 패닉 — 빨강
        if (stress >= 51)  return new Color(0.95f, 0.65f, 0.25f);    // 압박 — 주황
        return new Color(0.45f, 0.75f, 0.45f);                       // 안정 — 녹색
    }

    private static string GetStressLabel(int stress)
    {
        if (stress >= 100) return "패닉";
        if (stress >= 51)  return "압박";
        return "안정";
    }

    private void RefreshDeckSummary(IList<FellowData> party)
    {
        if (deckSummaryText == null) return;

        // 파티에 존재하는 성향만, 등장 순서 유지
        var order = new List<CardAffinity>();
        var groups = new Dictionary<CardAffinity, List<string>>();

        foreach (var f in party)
        {
            var aff = f.affinity;
            if (!groups.ContainsKey(aff))
            {
                groups[aff] = new List<string>();
                order.Add(aff);
            }
            groups[aff].Add(!string.IsNullOrEmpty(f.displayName) ? f.displayName : f.id);
        }

        var lines = new List<string>();
        foreach (var aff in order)
        {
            var names = string.Join(", ", groups[aff]);
            lines.Add($"{AffinityHelper.GetLabel(aff)}: {groups[aff].Count}({names})");
        }

        deckSummaryText.text = lines.Count > 0
            ? string.Join("\n", lines)
            : "(파티 없음)";
    }
}
