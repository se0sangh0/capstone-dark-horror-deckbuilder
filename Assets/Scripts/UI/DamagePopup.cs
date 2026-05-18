// DamagePopup.cs
// 메이플식 데미지 숫자 — 위로 떠오르며 페이드 아웃 후 자동 파괴.
//
// ── 사용 ────────────────────────────────────────────────────────
//   var popup = Instantiate(damagePopupPrefab, parentTransform);
//   popup.Show(123);
//
// ── 인스펙터 ───────────────────────────────────────────────────
//   text       : 데미지 숫자를 표시할 TMP_Text
//   floatHeight: 위로 떠오를 거리 (기본 60)
//   duration   : 애니메이션 시간 (기본 0.9s)

using UnityEngine;
using TMPro;
using DG.Tweening;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private float    floatHeight = 60f;
    [SerializeField] private float    duration    = 0.9f;
    [SerializeField] private Color    color       = new Color(1f, 0.4f, 0.4f); // 빨강 톤

    public void Show(int damage)
    {
        if (text == null) text = GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            text.text  = damage.ToString();
            text.color = color;
        }

        // 위로 떠오르며 페이드 아웃
        var seq = DOTween.Sequence();
        seq.Append(transform.DOLocalMoveY(transform.localPosition.y + floatHeight, duration).SetEase(Ease.OutCubic));
        if (text != null) seq.Join(text.DOFade(0f, duration).SetEase(Ease.InCubic));
        seq.OnComplete(() => Destroy(gameObject));
    }
}
