// ============================================================
// Rest/RestPanel.cs
// 화툿불 노드 UI 패널 — 자동 회복 + 파티 편집 진입점 + 다음 층
// ============================================================
//
// [흐름]
//   NodeSystem.DispatchByRoomType(Rest) → RestPanel.OpenFromNode()
//   OnOpened 즉시 RestService.ApplyRecovery() 호출 (자동 회복)
//   회복 결과 라벨 표시
//   [파티 편집] 클릭 → 자신 닫고 PartyEditPanel 열기 → 닫히면 자신 다시 열기
//   [다음 층]   클릭 → 자신 닫고 OnExit 발생 → NodeSystem 이 노드맵 복귀
//
// [PartyEditPanel 공유]
//   용병소와 동일한 PartyEditPanel 인스턴스를 인스펙터 슬롯으로 참조.
//   ⚠️ 씬 구조: PartyEditPanel 은 Canvas 직속 자식이어야 함 (MercenaryRoot 자식 X)
//
// [인스펙터 슬롯]
//   - canvasGroup        : (자동)
//   - titleLabel         : "화툿불" 제목 TMP_Text
//   - recoveryResultLabel: 회복 결과 표시 TMP_Text
//   - partyEditButton    : "파티 편집" Button
//   - nextNodeButton     : "다음 층으로" Button
//   - partyEditPanel     : 공유 PartyEditPanel (씬 인스턴스)
// ============================================================

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RestPanel : PanelBase
{
    [Header("UI 라벨 / 버튼")]
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text recoveryResultLabel;
    [SerializeField] private Button   partyEditButton;
    [SerializeField] private Button   nextNodeButton;

    [Header("공유 패널")]
    [Tooltip("용병소와 동일한 PartyEditPanel 인스턴스 참조 (Canvas 직속).")]
    [SerializeField] private PartyEditPanel partyEditPanel;

    /// <summary>"다음 층" 클릭 시 NodeSystem 이 구독해 노드맵으로 복귀시킨다.</summary>
    public event Action OnExit;

    protected override void Awake()
    {
        base.Awake();
        if (partyEditButton != null) partyEditButton.onClick.AddListener(HandlePartyEdit);
        if (nextNodeButton  != null) nextNodeButton.onClick.AddListener(HandleNextNode);
    }

    /// <summary>NodeSystem 이 호출. 화툿불 진입 — 자동 회복 + 페이드 인.</summary>
    public void OpenFromNode()
    {
        Open();
    }

    protected override void OnOpened()
    {
        // 자동 회복 적용 (사용자 결정 Q2·a)
        var result = RestService.ApplyRecovery();
        RefreshRecoveryLabel(result);
    }

    private void RefreshRecoveryLabel(RestService.RecoveryResult result)
    {
        if (titleLabel != null) titleLabel.text = "화툿불";
        if (recoveryResultLabel == null) return;

        if (result.affectedCount <= 0)
        {
            recoveryResultLabel.text = "회복할 동료가 없습니다.";
            return;
        }
        recoveryResultLabel.text =
            $"휴식 완료 — {result.affectedCount}명에게 HP +{RestService.RecoveryAmount} / 스트레스 -{RestService.RecoveryAmount}";
    }

    // ----------------------------------------------------------
    // 파티 편집 진입 — 자신 닫고 PartyEditPanel 열기, 닫히면 자신 다시 열기
    // ----------------------------------------------------------
    private void HandlePartyEdit()
    {
        if (partyEditPanel == null)
        {
            Debug.LogWarning("[RestPanel] PartyEditPanel 미연결 — 인스펙터 슬롯 확인");
            return;
        }

        Action onSubClosed = null;
        onSubClosed = () =>
        {
            partyEditPanel.OnClosedEvent -= onSubClosed;
            Open(); // 화툿불로 복귀
        };
        partyEditPanel.OnClosedEvent += onSubClosed;

        Close();
        partyEditPanel.Open();
    }

    // ----------------------------------------------------------
    // 다음 층 — 노드맵 복귀
    // ----------------------------------------------------------
    private void HandleNextNode()
    {
        OnExit?.Invoke();
        Close();
    }
}
