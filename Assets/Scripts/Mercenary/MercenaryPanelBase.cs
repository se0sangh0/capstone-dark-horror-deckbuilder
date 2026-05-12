// ============================================================
// Mercenary/MercenaryPanelBase.cs
// 용병소 모든 패널의 공통 베이스 — DOTween 페이드 토글
// ============================================================
//
// [부착 방법]
//   각 패널(메인/모집/성장/파티편집) GameObject 에
//   CanvasGroup 컴포넌트가 있으면 페이드 가능. 없으면 SetActive 만.
// ============================================================

using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public abstract class MercenaryPanelBase : MonoBehaviour
{
    [Header("페이드 설정")]
    [SerializeField] protected CanvasGroup canvasGroup;
    [SerializeField] protected float       fadeDuration = 0.18f;

    /// <summary>패널이 완전히 닫힌 직후 발생. 메인 ↔ 서브 전환 등에서 외부 구독.</summary>
    public event System.Action OnClosedEvent;

    protected virtual void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        gameObject.SetActive(false);
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }

    public virtual void Open()
    {
        gameObject.SetActive(true);
        OnOpened();
        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, fadeDuration);
        }
    }

    public virtual void Close()
    {
        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
            canvasGroup.DOFade(0f, fadeDuration).OnComplete(() =>
            {
                OnClosed();
                gameObject.SetActive(false);
                OnClosedEvent?.Invoke();
            });
        }
        else
        {
            OnClosed();
            gameObject.SetActive(false);
            OnClosedEvent?.Invoke();
        }
    }

    /// <summary>패널이 열린 직후 호출 — 데이터 바인딩 등.</summary>
    protected virtual void OnOpened() { }

    /// <summary>패널이 닫히기 직전 호출 — 리스너 정리 등.</summary>
    protected virtual void OnClosed() { }
}
