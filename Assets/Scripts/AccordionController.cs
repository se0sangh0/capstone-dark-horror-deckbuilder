// ============================================================
// AccordionController.cs
// 아코디언 UI 애니메이션 컨트롤러
// ============================================================
//
// [이 파일이 하는 일]
//   버튼을 클릭하면 내용물(패널)이 펼쳐지고 접히는
//   아코디언 UI 효과를 만들어 줍니다.
//
// [어떻게 동작하나요?]
//   - 처음: 패널 높이 = 0 (접힌 상태)
//   - 버튼 클릭: 내용물의 실제 높이를 자동 계산 후 0.4초 동안 펼쳐짐
//   - 다시 클릭: 0.4초 동안 다시 접힘
//
// [DOTween 라이브러리 필요]
//   이 스크립트는 DOTween 에셋을 사용합니다.
//   DOTween 이 없으면 에러가 발생합니다.
//
// [어디서 쓰이나요?]
//   - 인게임 메뉴, 동료 정보 패널 등 접었다 폈다 하는 UI 에 사용
//
// [인스펙터 설정]
//   - contentPanel : 크기가 변해야 할 내용물 패널(RectTransform) 연결
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 아코디언 방식으로 펼치고 접히는 UI 컨트롤러.
/// </summary>
public class AccordionController : MonoBehaviour
{
    // ----------------------------------------------------------
    // [contentPanel] — 펼치고 접을 내용물 패널
    // Inspector 에서 RectTransform 을 연결하세요.
    // ----------------------------------------------------------
    [Tooltip("펼치고 접힐 내용물 패널(RectTransform)을 연결하세요.")]
    public RectTransform contentPanel;

    /// <summary>현재 열려있는지 여부</summary>
    private bool isOpened = false;

    /// <summary>현재 펼쳐져 있는지 외부 조회용 (기본 펼침 1회 처리 등).</summary>
    public bool IsOpened => isOpened;

    // ----------------------------------------------------------
    // 아코디언 토글
    // 버튼의 onClick 이벤트에 이 메서드를 연결하세요.
    // ----------------------------------------------------------

    /// <summary>아코디언을 토글한다. 버튼 onClick 에 연결. 0.4초 애니메이션.</summary>
    public void ToggleAccordion() => SetOpen(!isOpened, instant: false);

    /// <summary>
    /// 아코디언 열림/닫힘을 명시적으로 설정.
    /// 열릴 때: 내용물의 실제 높이를 계산. 닫힐 때: 0.
    /// instant=true 면 애니메이션 없이 즉시 적용(기본 펼침 등 초기화용).
    /// </summary>
    public void SetOpen(bool open, bool instant = false)
    {
        isOpened = open;
        if (contentPanel == null) return;

        DOTween.Kill(contentPanel);

        // 펼칠 땐 자식 레이아웃을 먼저 확정해야 GetPreferredHeight 가 정확.
        if (open) LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel);
        float target = open ? LayoutUtility.GetPreferredHeight(contentPanel) : 0f;

        if (instant)
        {
            contentPanel.sizeDelta = new Vector2(contentPanel.sizeDelta.x, target);
            // 즉시 펼침은 단일 프레임에 반영되어야 하므로 스크롤 콘텐츠 전체를 재계산 —
            // 그래야 형제 아코디언(덱/스트레스)이 새 높이만큼 아래로 재배치된다.
            // (DOTween 토글 경로는 매 프레임 레이아웃이 자동 갱신되어 불필요)
            var sr = contentPanel.GetComponentInParent<ScrollRect>();
            var rebuildRoot = (sr != null && sr.content != null) ? sr.content : contentPanel;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rebuildRoot);
        }
        else
            contentPanel.DOSizeDelta(new Vector2(contentPanel.sizeDelta.x, target), 0.4f);
    }
}
