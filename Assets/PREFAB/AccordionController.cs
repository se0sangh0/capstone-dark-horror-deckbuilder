using UnityEngine;
using UnityEngine.UI; // UI 크기 자동 계산 기능을 쓰기 위해 꼭 추가해야 합니다!
using DG.Tweening;

public class AccordionController : MonoBehaviour
{
    // 크기가 변해야 할 내용물(Image_Content)을 끌어다 넣을 빈칸
    public RectTransform contentPanel; 
    
    // 열림/닫힘 상태 기억
    private bool isOpened = false; 

    // 버튼을 누를 때마다 실행될 마법의 주문
    public void ToggleAccordion()
    {
        isOpened = !isOpened; // 상태를 반대로 뒤집음 (열림<->닫힘)

        if (isOpened)
        {
            // ★마법의 코드: 안에 들어있는 내용물(카드 등)의 진짜 총 높이를 자동 계산합니다!
            float targetHeight = LayoutUtility.GetPreferredHeight(contentPanel);

            // 계산된 진짜 높이(targetHeight)만큼 0.4초 동안 부드럽게 키움
            contentPanel.DOSizeDelta(new Vector2(contentPanel.sizeDelta.x, targetHeight), 0.4f);
        }
        else
        {
            // 닫혀야 하면 높이를 다시 0으로 0.4초 동안 줄임
            contentPanel.DOSizeDelta(new Vector2(contentPanel.sizeDelta.x, 0f), 0.4f);
        }
    }
}