// BattleCardMover.cs
// 근접 공격 시 적/아군 위치까지 이동했다가 원래 자리로 복귀.
//
// ── 사용 ────────────────────────────────────────────────────────
//   var mover = attackerObj.GetComponent<BattleCardMover>();
//   mover?.AttackTo(targetObj.transform);
//
// ── 동작 ────────────────────────────────────────────────────────
//   originPos 자동 캐싱 → 타겟 위치 옆까지 DOTween 이동 → 공격 모션 (BattleCardSprites) →
//   잠깐 머무름 → 원래 자리로 복귀.
//
// ── 인스펙터 ───────────────────────────────────────────────────
//   moveDuration  : 이동 시간 (기본 0.18s)
//   holdDuration  : 적 앞에서 머무는 시간 (기본 0.20s)
//   approachGap   : 타겟에서 멈출 거리 (양수, 타겟 쪽으로 다 가지 않고 살짝 떨어짐)

using UnityEngine;
using DG.Tweening;

public class BattleCardMover : MonoBehaviour
{
    [SerializeField] private float moveDuration = 0.18f;
    [SerializeField] private float holdDuration = 0.20f;
    [SerializeField] private float approachGap  = 0.10f;

    private Vector3 _origin;
    private bool    _originCached;
    private Sequence _seq;

    private void CacheOrigin()
    {
        if (_originCached) return;
        _origin       = transform.position;
        _originCached = true;
    }

    /// <summary>근접 공격: 타겟까지 이동 → 공격 모션 → 원위치 복귀.</summary>
    public void AttackTo(Transform target)
    {
        if (target == null) return;
        CacheOrigin();

        // 타겟 방향으로 approachGap 만큼 떨어진 지점
        Vector3 toTarget   = target.position - _origin;
        Vector3 approach   = target.position - toTarget.normalized * approachGap;
        approach.z = _origin.z; // 2D 평면 유지

        if (_seq != null && _seq.IsActive()) _seq.Kill();
        _seq = DOTween.Sequence();
        _seq.Append(transform.DOMove(approach, moveDuration).SetEase(Ease.OutQuad));
        _seq.AppendCallback(() => GetComponent<BattleCardView>()?.PlayAttackMotion());
        _seq.AppendInterval(holdDuration);
        _seq.Append(transform.DOMove(_origin, moveDuration).SetEase(Ease.InOutQuad));
    }

    /// <summary>원거리 공격: 이동 없이 공격 모션만.</summary>
    public void AttackInPlace()
    {
        GetComponent<BattleCardView>()?.PlayAttackMotion();
    }

    private void OnDisable()
    {
        if (_seq != null && _seq.IsActive()) _seq.Kill();
        if (_originCached) transform.position = _origin;
    }
}
