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
//   sprites?.PlayAttack(MotionCategory.Melee);
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
using DG.Tweening;

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

    [Header("Melee 모션")]
    [Tooltip("Melee 시 transform localPosition.x 이동량")]
    [SerializeField] private float meleeDashDistance = 0.5f;
    [Tooltip("Melee 한 방향 이동 시간 (전진/복귀 각각)")]
    [SerializeField] private float meleeDashDuration = 0.1f;

    [Header("Facing (자동: 렌더러 transform 만 flip — Canvas 자식 안 건드림)")]
    [Tooltip("비워두면 targetMeshRenderer → targetSpriteRenderer → targetImage 순으로 자동 선택")]
    [SerializeField] private Transform flipTarget;

    private Coroutine _motionRoutine;
    private Sequence  _meleeSeq;
    private Sequence  _hitFlashSeq;
    private Sequence  _deathSeq;
    private Sequence  _teleportSeq;
    // 진영 — Melee 시 dash 방향 결정 (아군: +X, 적군: -X)
    private bool _isEnemyFaction;

    // ── 색상 캐시 (피격 깜빡임 + 사망 페이드 복구용) ──────────────
    private Color _originalSpriteColor = Color.white;
    private Color _originalImageColor  = Color.white;
    private Color _originalMaterialColor = Color.white;
    private bool  _colorsCached;
    // URP 는 _BaseColor, Built-in 은 _Color — 둘 다 시도
    private static readonly int MatColorIdUrp    = Shader.PropertyToID("_BaseColor");
    private static readonly int MatColorIdLegacy = Shader.PropertyToID("_Color");

    // ── Animator ───────────────────────────────────────────────────
    //   DefaultSetting 에서 AttachAnimator() 로 주입.
    //   주입되지 않으면 sprite 교체 방식(idleSprite/attackSprite/hitSprite) 으로 fallback.
    private Animator _animator;
    private static readonly int HashAttack = Animator.StringToHash("Attack");
    private static readonly int HashHit    = Animator.StringToHash("Hit");

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

    /// <summary>
    /// 카테고리별 공격 모션.
    ///   Ranged     — sprite 교체만 (제자리, 추후 발사체)
    ///   Melee      — sprite 교체 + DOTween 짧은 전진/복귀 (방향: 아군 +X, 적군 -X)
    ///   Stationary — sprite 교체만
    /// </summary>
    public void PlayAttack(MotionCategory cat)
    {
        // Animator 가 주입돼 있으면 trigger, 아니면 sprite 교체 fallback
        if (_animator != null && _animator.runtimeAnimatorController != null)
            _animator.SetTrigger(HashAttack);
        else if (attackSprite != null)
            PlayTemporary(attackSprite);

        if (cat == MotionCategory.Melee) PlayMeleeDash();
    }

    /// <summary>DefaultSetting 에서 RuntimeAnimatorController 주입 직후 호출.</summary>
    public void AttachAnimator(Animator animator)
    {
        _animator = animator;
    }

    private void PlayMeleeDash()
    {
        if (_meleeSeq != null && _meleeSeq.IsActive()) _meleeSeq.Kill(complete: true);

        float dir = _isEnemyFaction ? -1f : 1f;
        float dx  = meleeDashDistance * dir;

        Vector3 origin = transform.localPosition;
        _meleeSeq = DOTween.Sequence();
        _meleeSeq.Append(transform.DOLocalMoveX(origin.x + dx, meleeDashDuration).SetEase(Ease.OutQuad));
        _meleeSeq.Append(transform.DOLocalMoveX(origin.x,      meleeDashDuration).SetEase(Ease.InOutQuad));
        _meleeSeq.OnKill(() => transform.localPosition = origin);
    }

    public void PlayHit()
    {
        if (_animator != null && _animator.runtimeAnimatorController != null)
            _animator.SetTrigger(HashHit);
        else if (hitSprite != null)
            PlayTemporary(hitSprite);

        // 흰색 깜빡임 — Animator/Sprite 와 동시 실행
        PlayHitFlash();
    }

    /// <summary>
    /// 피격 시 포켓몬 도트식 깜빡임 — 카드 렌더러를 빠르게 보였다 사라졌다 4회 토글 (0.08s 간격).
    /// SpriteRenderer/Image/MeshRenderer.enabled 모두 토글하여 셰이더 종류 무관하게 동작.
    /// </summary>
    private void PlayHitFlash()
    {
        if (_hitFlashSeq != null && _hitFlashSeq.IsActive()) _hitFlashSeq.Kill();

        _hitFlashSeq = DOTween.Sequence();
        for (int i = 0; i < 4; i++)
        {
            _hitFlashSeq.AppendCallback(() => SetCardVisible(false));
            _hitFlashSeq.AppendInterval(0.08f);
            _hitFlashSeq.AppendCallback(() => SetCardVisible(true));
            _hitFlashSeq.AppendInterval(0.08f);
        }
        // 인터럽트(사망 등)로 kill 되어도 마지막엔 반드시 visible 상태로
        _hitFlashSeq.OnKill(() => SetCardVisible(true));
    }

    private void SetCardVisible(bool visible)
    {
        if (targetMeshRenderer   != null) targetMeshRenderer.enabled   = visible;
        if (targetSpriteRenderer != null) targetSpriteRenderer.enabled = visible;
        if (targetImage          != null) targetImage.enabled          = visible;
    }

    /// <summary>
    /// 사망 침몰 — 포켓몬 블랙2 풍 "땅 속으로 들어가는" 느낌.
    /// 카드 본체가 아래로 내려가면서 위쪽부터 잘려나가고 마지막에 밑부분이 사라진다.
    /// (= 아래쪽 가장자리가 점차 내려가고, 위쪽 가장자리도 동시에 내려와 카드가 짧아지며 침몰)
    /// </summary>
    public void PlayDeathFall(System.Action onComplete, float duration = 0.6f)
    {
        CacheOriginalColors();

        if (_hitFlashSeq != null && _hitFlashSeq.IsActive()) _hitFlashSeq.Kill();
        RestoreCardColor();

        if (_deathSeq != null && _deathSeq.IsActive()) _deathSeq.Kill();

        Vector3 origin        = transform.localPosition;
        Vector3 originalScale = transform.localScale;

        float meshLocalHeight = 1f;
        var r = GetComponent<Renderer>();
        if (r != null)
        {
            float lossyY = Mathf.Max(0.0001f, transform.lossyScale.y);
            meshLocalHeight = r.bounds.size.y / lossyY;
        }
        // 카드 한 칸 높이만큼 아래로 내려간 위치에서 완전 침몰
        float bottomEdgeStartY = origin.y - meshLocalHeight * 0.5f * originalScale.y;
        float bottomEdgeEndY   = bottomEdgeStartY - meshLocalHeight * originalScale.y;

        _deathSeq = DOTween.Sequence();
        _deathSeq.Append(
            DOTween.To(() => 1f, v =>
            {
                // v: 1 → 0
                var s = originalScale;
                s.y = originalScale.y * v;
                transform.localScale = s;

                // 아래쪽 가장자리가 시작점 → 한 칸 아래로 점차 이동 (땅속으로 잠김)
                float currentBottomEdge = Mathf.Lerp(bottomEdgeEndY, bottomEdgeStartY, v);
                var p = origin;
                p.y = currentBottomEdge + meshLocalHeight * 0.5f * originalScale.y * v;
                transform.localPosition = p;
            }, 0f, duration).SetEase(Ease.InQuad)
        );
        _deathSeq.Join(BuildFadeTween(0f, duration));
        _deathSeq.OnComplete(() => onComplete?.Invoke());
    }

    // ── 색상 처리 헬퍼 ──────────────────────────────────────────────
    private void CacheOriginalColors()
    {
        if (_colorsCached) return;
        if (targetSpriteRenderer != null) _originalSpriteColor = targetSpriteRenderer.color;
        if (targetImage          != null) _originalImageColor  = targetImage.color;
        if (targetMeshRenderer   != null && targetMeshRenderer.material != null)
        {
            var mat = targetMeshRenderer.material;
            if      (mat.HasProperty(MatColorIdUrp))    _originalMaterialColor = mat.GetColor(MatColorIdUrp);
            else if (mat.HasProperty(MatColorIdLegacy)) _originalMaterialColor = mat.GetColor(MatColorIdLegacy);
        }
        _colorsCached = true;
    }

    private void SetCardColor(Color c)
    {
        if (targetSpriteRenderer != null) targetSpriteRenderer.color = c;
        if (targetImage          != null) targetImage.color          = c;
        if (targetMeshRenderer   != null && targetMeshRenderer.material != null)
        {
            var mat = targetMeshRenderer.material;
            if      (mat.HasProperty(MatColorIdUrp))    mat.SetColor(MatColorIdUrp, c);
            else if (mat.HasProperty(MatColorIdLegacy)) mat.SetColor(MatColorIdLegacy, c);
        }
    }

    private void RestoreCardColor()
    {
        if (targetSpriteRenderer != null) targetSpriteRenderer.color = _originalSpriteColor;
        if (targetImage          != null) targetImage.color          = _originalImageColor;
        if (targetMeshRenderer   != null && targetMeshRenderer.material != null)
        {
            var mat = targetMeshRenderer.material;
            if      (mat.HasProperty(MatColorIdUrp))    mat.SetColor(MatColorIdUrp,    _originalMaterialColor);
            else if (mat.HasProperty(MatColorIdLegacy)) mat.SetColor(MatColorIdLegacy, _originalMaterialColor);
        }
    }

    /// <summary>SpriteRenderer/Image/MeshRenderer.material 중 첫 비-null 대상에 대해 alpha 페이드 tween 반환.</summary>
    private Tween BuildFadeTween(float targetAlpha, float duration)
    {
        if (targetSpriteRenderer != null) return targetSpriteRenderer.DOFade(targetAlpha, duration);
        if (targetImage          != null) return targetImage.DOFade(targetAlpha, duration);
        if (targetMeshRenderer   != null && targetMeshRenderer.material != null)
        {
            var mat = targetMeshRenderer.material;
            int id = mat.HasProperty(MatColorIdUrp) ? MatColorIdUrp :
                     mat.HasProperty(MatColorIdLegacy) ? MatColorIdLegacy : 0;
            if (id != 0)
            {
                Color from = mat.GetColor(id);
                Color to   = new Color(from.r, from.g, from.b, targetAlpha);
                return mat.DOColor(to, id, duration);
            }
        }
        return null;
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

    // ── Facing ─────────────────────────────────────────────────────
    //   아군은 오른쪽(적 방향), 적은 왼쪽(아군 방향) 보도록 카드 비주얼만 flip.
    //   HP 바/이름 텍스트 등 Canvas 자식은 건드리지 않기 위해
    //   렌더러 transform 의 localScale.x 만 부호 반전한다.
    //   원본 스프라이트가 "오른쪽 향함" 기준일 때:
    //     SetFacing(false) → 그대로 (아군 사용)
    //     SetFacing(true)  → flip   (적군 사용)
    //   임시 스프라이트라 원본 향이 반대면 호출부에서 boolean 만 뒤집으면 됨.
    public void SetFacing(bool faceLeft)
    {
        var t = ResolveFlipTarget();
        if (t == null) return;
        var s = t.localScale;
        s.x = Mathf.Abs(s.x) * (faceLeft ? -1f : 1f);
        t.localScale = s;

        // UI Canvas 는 카드 flip 의 영향을 받지 않도록 역방향 보정.
        // (HP 텍스트가 좌우반전되어 읽기 어려워지는 것 방지)
        var canvas = GetComponentInChildren<Canvas>(true);
        if (canvas != null && canvas.transform != t)
        {
            var cs = canvas.transform.localScale;
            cs.x = Mathf.Abs(cs.x) * (faceLeft ? -1f : 1f);
            canvas.transform.localScale = cs;
        }
    }

    /// <summary>Melee dash 방향용 진영 설정. 아군은 +X 로 전진, 적군은 -X.</summary>
    public void ConfigureFaction(bool isEnemy)
    {
        _isEnemyFaction = isEnemy;
    }

    private void OnDestroy()
    {
        if (_meleeSeq     != null && _meleeSeq.IsActive())     _meleeSeq.Kill();
        if (_hitFlashSeq  != null && _hitFlashSeq.IsActive())  _hitFlashSeq.Kill();
        if (_deathSeq     != null && _deathSeq.IsActive())     _deathSeq.Kill();
        if (_teleportSeq  != null && _teleportSeq.IsActive())  _teleportSeq.Kill();
    }

    /// <summary>
    /// 텔레포트 연출 — fade out → 잠시 비가시 → fade in. 같은 자리에서 발생.
    /// 보스 순간이동 스킬(거두는 자) 등에서 사용.
    /// </summary>
    public void PlayTeleport(float fadeDuration = 0.3f, float waitDuration = 0.2f, System.Action onComplete = null)
    {
        CacheOriginalColors();

        if (_teleportSeq != null && _teleportSeq.IsActive()) _teleportSeq.Kill();

        _teleportSeq = DOTween.Sequence();
        var outTween = BuildFadeTween(0f, fadeDuration);
        if (outTween != null) _teleportSeq.Append(outTween);

        if (waitDuration > 0f) _teleportSeq.AppendInterval(waitDuration);

        var inTween = BuildFadeTween(1f, fadeDuration);
        if (inTween != null) _teleportSeq.Append(inTween);

        // 안전망 — 중단/완료 시 원본 색 복원 (다른 트윈 인터럽트 대비)
        _teleportSeq.OnKill(() => RestoreCardColor());
        _teleportSeq.OnComplete(() => onComplete?.Invoke());
    }

    private Transform ResolveFlipTarget()
    {
        if (flipTarget          != null) return flipTarget;
        if (targetMeshRenderer  != null) return targetMeshRenderer.transform;
        if (targetSpriteRenderer != null) return targetSpriteRenderer.transform;
        if (targetImage         != null) return targetImage.transform;
        return null;
    }
}
