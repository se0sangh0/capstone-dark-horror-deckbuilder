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

    // ----------------------------------------------------------
    // 아코디언 토글
    // 버튼의 onClick 이벤트에 이 메서드를 연결하세요.
    // ----------------------------------------------------------

    /// <summary>
    /// 아코디언을 열거나 닫는다.
    /// 열릴 때: 내용물의 실제 높이를 계산하여 0.4초 동안 펼침.
    /// 닫힐 때: 0.4초 동안 높이를 0으로 줄임.
    /// </summary>
    public void ToggleAccordion()
    {
        isOpened = !isOpened;

        if (isOpened)
        {
            // 내용물의 실제 총 높이를 자동으로 계산
            float targetHeight = LayoutUtility.GetPreferredHeight(contentPanel);

            // 계산된 높이까지 0.4초 동안 부드럽게 늘어남
            contentPanel.DOSizeDelta(
                new Vector2(contentPanel.sizeDelta.x, targetHeight), 0.4f);
        }
        else
        {
            // 높이를 0으로 0.4초 동안 줄어듦 (접힘)
            contentPanel.DOSizeDelta(
                new Vector2(contentPanel.sizeDelta.x, 0f), 0.4f);
        }
    }
}
