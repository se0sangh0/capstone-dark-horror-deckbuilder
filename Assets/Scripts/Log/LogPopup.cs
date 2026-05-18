// LogPopup.cs
// Debug.Log 수집된 내용을 표시하는 팝업. PanelBase 의 페이드 패턴을 따름.
//
// ── 인스펙터 ───────────────────────────────────────────────────
//   logText      : ScrollView 안의 TMP_Text (로그 한 줄씩 갱신)
//   scrollRect   : ScrollRect (새 로그 추가 시 하단으로 스크롤)
//   closeButton  : 닫기 버튼
//   clearButton  : (선택) 로그 비우기 버튼

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LogPopup : PanelBase
{
    [Header("로그 표시")]
    [SerializeField] private TMP_Text   logText;
    [SerializeField] private ScrollRect scrollRect;

    [Header("버튼")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button clearButton;

    protected override void OnOpened()
    {
        RefreshAll();
        if (GameLogService.Instance != null)
            GameLogService.Instance.OnLogAdded += OnLogAdded;

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
            closeButton.onClick.AddListener(Close);
        }
        if (clearButton != null)
        {
            clearButton.onClick.RemoveListener(OnClearClicked);
            clearButton.onClick.AddListener(OnClearClicked);
        }
    }

    protected override void OnClosed()
    {
        if (GameLogService.Instance != null)
            GameLogService.Instance.OnLogAdded -= OnLogAdded;

        if (closeButton != null) closeButton.onClick.RemoveListener(Close);
        if (clearButton != null) clearButton.onClick.RemoveListener(OnClearClicked);
    }

    private void OnLogAdded(string _) => RefreshAll();

    private void RefreshAll()
    {
        if (logText == null) return;
        logText.text = GameLogService.Instance != null ? GameLogService.Instance.GetAll() : "";
        // 다음 프레임에 스크롤 끝으로 — Content 레이아웃 갱신 이후
        if (scrollRect != null) Canvas.ForceUpdateCanvases();
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;
    }

    private void OnClearClicked()
    {
        if (GameLogService.Instance != null) GameLogService.Instance.Clear();
    }
}
