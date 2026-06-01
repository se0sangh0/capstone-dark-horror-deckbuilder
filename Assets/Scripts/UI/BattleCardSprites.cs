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
    [Header("스프라이트 (Idle/Attack)")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite attackSprite;
    // 피격은 애니/스프라이트 교체 없이 DOTween 흰색 깜빡임(PlayHitFlash)으로만 처리 (포켓몬 도트식).

    [Header("렌더 대상 (Renderer 또는 Image 중 하나)")]
    [SerializeField] private SpriteRenderer       targetSpriteRenderer;
    [SerializeField] private MeshRenderer         targetMeshRenderer;
    [SerializeField] private UnityEngine.UI.Image targetImage;

    [Header("모션 시간")]
    [SerializeField] private float motionDuration = 0.25f;

    [Header("Melee 모션")]
    [Tooltip("Melee 시 fallback 이동량 (target 못 찾을 때만 사용)")]
    [SerializeField] private float meleeDashDistance = 0.5f;
    [Tooltip("Melee 한 방향 이동 시간 (forward/back 각각). 너무 짧으면 안 보임 — 0.2~0.3 권장.")]
    [SerializeField] private float meleeDashDuration = 0.25f;
    [Tooltip("적 위치까지 이동 비율. 1.0 = 정확한 적 위치, 0.85 = 약간 못 미친 위치 (겹침 방지).")]
    [Range(0.5f, 1.0f)]
    [SerializeField] private float meleeDashApproachRatio = 0.85f;
    [Tooltip("적 앞에서 공격 모션 재생 대기 시간. Animator State Attack speed=0.5 일 때 ~1.0초 권장.")]
    [SerializeField] private float attackHoldDuration = 1.0f;

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
    // 기본 트리거명 (JSON 에 attack1Anim/attack2Anim 미지정 시 폴백)
    private const string DefaultAttack1 = "Attack";
    private const string DefaultAttack2 = "Attack2";
    // 피격(Hit)은 애니메이터 트리거를 쓰지 않음 — DOTween 깜빡임으로만 처리.
    // 런타임 트리거 해시 — JSON 에서 받은 이름으로 결정 (AttachAnimator 에서 세팅)
    private int _hashAttack1;
    private int _hashAttack2;
    private bool _hasAttack1;
    private bool _hasAttack2;

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
    /// 카테고리별 공격 모션. skillIndex 로 Attack/Attack2 분기 (1번 스킬 → Attack, 2번 스킬 → Attack2).
    /// Attack2 파라미터가 컨트롤러에 없는 종(Defender/Offender/Priest)은 자동으로 Attack 폴백.
    ///   Ranged     — sprite 교체만 (제자리, 추후 발사체)
    ///   Melee      — sprite 교체 + DOTween 짧은 전진/복귀 (방향: 아군 +X, 적군 -X)
    ///   Stationary — sprite 교체만
    /// </summary>
    public void PlayAttack(MotionCategory cat, int skillIndex = 0)
    {
        bool hasAnim = _animator != null && _animator.runtimeAnimatorController != null;

        if (cat == MotionCategory.Melee)
        {
            // 순차 흐름: forward dash → 적 앞에서 SetTrigger + 모션 재생 대기 → back dash
            PlayMeleeAttackSequence(skillIndex, hasAnim);
        }
        else
        {
            // Ranged/Stationary: 즉시 트리거 (제자리)
            if (hasAnim) TriggerAttackImmediate(skillIndex);
            else if (attackSprite != null) PlayTemporary(attackSprite);
        }
    }

    /// <summary>제자리에서 즉시 Attack(또는 Attack2) 트리거. Melee 가 아닌 카테고리에서 사용.</summary>
    private void TriggerAttackImmediate(int skillIndex)
    {
        if (skillIndex >= 1 && _hasAttack2) _animator.SetTrigger(_hashAttack2);
        else if (_hasAttack1)               _animator.SetTrigger(_hashAttack1);
    }

    /// <summary>현재 근접 공격 시퀀스 (dash + 모션 + 복귀) 가 진행 중인지. BattleManager 가 턴 전환 시 polling.</summary>
    public bool IsAttacking => _meleeSeq != null && _meleeSeq.IsActive();

    /// <summary>씬 안의 모든 BattleCardSprites 중 하나라도 공격 시퀀스 진행 중이면 true.</summary>
    public static bool AnyAttacking()
    {
        var all = UnityEngine.Object.FindObjectsByType<BattleCardSprites>(FindObjectsSortMode.None);
        foreach (var c in all) if (c != null && c.IsAttacking) return true;
        return false;
    }

    /// <summary>
    /// DefaultSetting 에서 RuntimeAnimatorController 주입 직후 호출.
    /// JSON 에 지정된 트리거 이름을 받아 해시 캐싱 + 컨트롤러 보유 여부 검사.
    /// 이름이 비어있으면 기본값 (Attack / Attack2) 사용.
    /// </summary>
    public void AttachAnimator(Animator animator, string attack1Name = null, string attack2Name = null)
    {
        _animator = animator;

        string n1 = string.IsNullOrEmpty(attack1Name) ? DefaultAttack1 : attack1Name;
        string n2 = string.IsNullOrEmpty(attack2Name) ? DefaultAttack2 : attack2Name;
        _hashAttack1 = Animator.StringToHash(n1);
        _hashAttack2 = Animator.StringToHash(n2);

        _hasAttack1 = false;
        _hasAttack2 = false;
        if (animator != null)
        {
            foreach (var p in animator.parameters)
            {
                if (p.nameHash == _hashAttack1) _hasAttack1 = true;
                if (p.nameHash == _hashAttack2) _hasAttack2 = true;
            }
        }
    }

    // ============================================================
    // 근접 공격 시퀀스 — 세븐나이츠 스타일 순차 흐름:
    //   ① forward dash    (meleeDashDuration)        : 적 위치까지 이동
    //   ② attack 모션 재생 (attackHoldDuration)        : 적 앞에서 SetTrigger + 대기
    //   ③ back dash       (meleeDashDuration)        : 원위치 복귀
    // 타겟 못 찾으면 fallback 으로 인스펙터 meleeDashDistance 사용.
    // ============================================================
    private void PlayMeleeAttackSequence(int skillIndex, bool hasAnim)
    {
        if (_meleeSeq != null && _meleeSeq.IsActive()) _meleeSeq.Kill(complete: true);

        Vector3 origin = transform.localPosition;
        float dx;
        Transform target = FindMeleeTarget();
        if (target != null)
        {
            float worldDx = target.position.x - transform.position.x;
            float parentScaleX = (transform.parent != null && transform.parent.lossyScale.x != 0f)
                ? transform.parent.lossyScale.x : 1f;
            float localDx = worldDx / parentScaleX;
            dx = localDx * meleeDashApproachRatio;
        }
        else
        {
            float dir = _isEnemyFaction ? -1f : 1f;
            dx = meleeDashDistance * dir;
        }

        _meleeSeq = DOTween.Sequence();
        // ① forward
        _meleeSeq.Append(transform.DOLocalMoveX(origin.x + dx, meleeDashDuration).SetEase(Ease.OutQuad));
        // ② SetTrigger + 모션 재생 대기 (Animator 없으면 sprite 교체 fallback)
        _meleeSeq.AppendCallback(() => {
            if (hasAnim) TriggerAttackImmediate(skillIndex);
            else if (attackSprite != null) PlayTemporary(attackSprite);
        });
        _meleeSeq.AppendInterval(attackHoldDuration);
        // ③ back
        _meleeSeq.Append(transform.DOLocalMoveX(origin.x, meleeDashDuration).SetEase(Ease.InOutQuad));
        _meleeSeq.OnKill(() => transform.localPosition = origin);
    }

    /// <summary>
    /// 가장 가까운 적/아군 카드의 transform 반환. 아군이면 "EnemyObject*", 적이면 "MyObject*".
    /// 없으면 null (PlayMeleeDash 가 fallback 거리 사용).
    /// </summary>
    private Transform FindMeleeTarget()
    {
        string prefix = _isEnemyFaction ? "MyObject" : "EnemyObject";
        var all = UnityEngine.Object.FindObjectsByType<BattleCardSprites>(FindObjectsSortMode.None);
        Transform best = null;
        float bestDist = float.MaxValue;
        foreach (var c in all)
        {
            if (c == this) continue;
            if (c == null || !c.gameObject.activeInHierarchy) continue;
            if (!c.name.StartsWith(prefix)) continue;
            float d = Mathf.Abs(c.transform.position.x - transform.position.x);
            if (d < bestDist) { bestDist = d; best = c.transform; }
        }
        return best;
    }

    public void PlayHit()
    {
        // 피격 피드백 = DOTween 흰색 깜빡임 (포켓몬 도트식). 애니메이터 Hit 트리거/sprite 교체 불필요.
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

    // ── 지속 tint (상태이상 표시용) ────────────────────────────────
    /// <summary>
    /// 지속 색조 적용 — DoT / 디버프 상태이상 시각화. DoT 활성 동안 카드 sprite 에 tint.
    /// 사망 침몰(PlayDeathFall) / 일시 색 변화와 별개. ClearPersistentTint 로 해제.
    /// </summary>
    public void SetPersistentTint(Color c)
    {
        CacheOriginalColors();
        SetCardColor(c);
    }

    /// <summary>지속 tint 해제 — 원래 색 복원.</summary>
    public void ClearPersistentTint()
    {
        if (_colorsCached) RestoreCardColor();
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
