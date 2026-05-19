// BattleCardSprites.cs
// 전투 카드(MyObject / EnemyObject) 의 상태별 스프라이트 전환.
//
// ── 모션 3종 ────────────────────────────────────────────────────
//   Idle    : 대기 상태 (기본 sprite)
//   Attack  : 공격 시 (짧게 표시 후 자동 복귀)
//   Hit     : 데미지 받음 (짧게 표시 후 자동 복귀)
//
// ── 사용 ────────────────────────────────────────────────────────
//   var sprites = newObj.GetComponent<BattleCardSprites>();
//   sprites?.PlayAttack();
//   sprites?.PlayHit();
//   sprites?.SetIdle();
//
// ── 인스펙터 ───────────────────────────────────────────────────
//   idleSprite, attackSprite, hitSprite : 각 상태 스프라이트
//   targetRenderer (선택) : MeshRenderer (3D Quad) — null 이면 자동 검색
//   targetImage    (선택) : UI Image     — Renderer 없을 때 사용
//   motionDuration : 공격/피격 모션 유지 시간 (기본 0.25s)

using System.Collections;
using UnityEngine;

public class BattleCardSprites : MonoBehaviour
{
    [Header("스프라이트 (Idle/Attack/Hit)")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite attackSprite;
    [SerializeField] private Sprite hitSprite;

    [Header("렌더 대상 (Renderer 또는 Image 중 하나)")]
    [SerializeField] private SpriteRenderer       targetSpriteRenderer;
    [SerializeField] private MeshRenderer         targetMeshRenderer;
    [SerializeField] private UnityEngine.UI.Image targetImage;

    [Header("모션 시간")]
    [SerializeField] private float motionDuration = 0.25f;

    private Coroutine _motionRoutine;

    private void Awake()
    {
        if (targetSpriteRenderer == null) targetSpriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        if (targetMeshRenderer   == null) targetMeshRenderer   = GetComponentInChildren<MeshRenderer>(true);
        if (targetImage          == null) targetImage          = GetComponentInChildren<UnityEngine.UI.Image>(true);
        SetIdle();
    }

    public void SetIdle()
    {
        if (idleSprite != null) ApplySprite(idleSprite);
    }

    public void PlayAttack()
    {
        if (attackSprite != null) PlayTemporary(attackSprite);
    }

    public void PlayHit()
    {
        if (hitSprite != null) PlayTemporary(hitSprite);
    }

    private void PlayTemporary(Sprite sprite)
    {
        if (_motionRoutine != null) StopCoroutine(_motionRoutine);
        _motionRoutine = StartCoroutine(MotionRoutine(sprite));
    }

    private IEnumerator MotionRoutine(Sprite sprite)
    {
        ApplySprite(sprite);
        yield return new WaitForSeconds(motionDuration);
        SetIdle();
        _motionRoutine = null;
    }

    private void ApplySprite(Sprite sprite)
    {
        if (targetSpriteRenderer != null) targetSpriteRenderer.sprite = sprite;
        if (targetImage          != null) targetImage.sprite          = sprite;
        if (targetMeshRenderer   != null && targetMeshRenderer.material != null && sprite != null)
            targetMeshRenderer.material.mainTexture = sprite.texture;
    }
}
