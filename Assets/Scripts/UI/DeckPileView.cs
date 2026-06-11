// ============================================================
// UI/DeckPileView.cs  (2026-06-11)
// 전투 우하단 덱 더미 — 남은 드로우 덱 장수 표시.
// ============================================================
//
// [구성] DeckPile GO(카드 뒷면 이미지 3장 겹침) + CountText(TMP).
//   이 컴포넌트는 GameManager.RemainingDeckCount 를 폴링해 숫자만 갱신한다.
//   DeckPile 의 RectTransform 은 GameManager.cardStackAnchor 로도 연결되어
//   드로우 애니메이션(덱 → 손패)의 출발점이 된다.
// ============================================================

using TMPro;
using UnityEngine;

public class DeckPileView : MonoBehaviour
{
    [SerializeField] private TMP_Text countText;

    private int _last = -1;

    private void Update()
    {
        int n = GameManager.Instance != null ? GameManager.Instance.RemainingDeckCount : 0;
        if (n == _last) return;
        _last = n;
        if (countText != null) countText.text = n.ToString();
    }
}
