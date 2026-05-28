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

public enum PopupKind
{
    Damage,        // -N 빨강
    Heal,          // +N 초록
    Shield,        // +N 파랑 (쉴드 획득)
    ShieldAbsorb,  // -N 노랑 (쉴드 흡수)
}

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private float    floatHeight = 60f;
    [SerializeField] private float    duration    = 0.9f;

    [Header("색상 (Kind 별)")]
    [SerializeField] private Color    damageColor       = new Color(1f, 0.4f, 0.4f);  // 빨강
    [SerializeField] private Color    healColor         = new Color(0.35f, 0.9f, 0.4f); // 초록
    [SerializeField] private Color    shieldColor       = new Color(0.4f, 0.7f, 1f);  // 파랑 (쉴드 획득)
    [SerializeField] private Color    shieldAbsorbColor = new Color(1f, 0.9f, 0.3f);  // 노랑 (쉴드 흡수)

    public void Show(int amount) => Show(amount, PopupKind.Damage, floatHeight);
    public void Show(int amount, float floatHeightOverride) => Show(amount, PopupKind.Damage, floatHeightOverride);

    /// <summary>
    /// kind 에 따라 부호와 색상을 적용. floatHeightOverride>0 이면 사용.
    /// startDelay>0 이면 그 시간만큼 텍스트를 숨긴 채 대기 후 떠오름 (AOE cascade 용).
    /// </summary>
    public void Show(int amount, PopupKind kind, float floatHeightOverride = -1f, float startDelay = 0f)
    {
        if (text == null) text = GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            bool isNegative = kind == PopupKind.Damage || kind == PopupKind.ShieldAbsorb;
            string prefix = isNegative ? "-" : "+";
            text.text  = $"{prefix}{Mathf.Abs(amount)}";
            Color c = kind switch
            {
                PopupKind.Damage       => damageColor,
                PopupKind.Heal         => healColor,
                PopupKind.Shield       => shieldColor,
                PopupKind.ShieldAbsorb => shieldAbsorbColor,
                _ => damageColor,
            };
            // startDelay 동안은 알파 0 으로 숨김 — 시작 시점에 1 로 복원
            if (startDelay > 0f) c.a = 0f;
            text.color = c;
        }

        float h = floatHeightOverride > 0f ? floatHeightOverride : floatHeight;

        var seq = DOTween.Sequence();
        if (startDelay > 0f)
        {
            seq.AppendInterval(startDelay);
            if (text != null)
            {
                seq.AppendCallback(() =>
                {
                    var c = text.color; c.a = 1f; text.color = c;
                });
            }
        }
        // 위로 떠오르며 페이드 아웃
        seq.Append(transform.DOLocalMoveY(transform.localPosition.y + h, duration).SetEase(Ease.OutCubic));
        if (text != null) seq.Join(text.DOFade(0f, duration).SetEase(Ease.InCubic));
        seq.OnComplete(() => Destroy(gameObject));
    }
}
