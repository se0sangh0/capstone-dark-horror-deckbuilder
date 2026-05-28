// MotionCategoryResolver.cs
// 공격 모션 카테고리 결정 — 스킬 effectType + actor jobClass + 진영 기반.
//
// ── 분기 규칙 (2026-05-26 합의) ────────────────────────────────
//   Damage + jobClass=="캐스터"  → Ranged    (제자리, 추후 발사체)
//   Damage + jobClass≠캐스터     → Melee     (DOTween x±0.5, 0.1s yoyo)
//   Shield / Heal                  → Stationary (제자리)
//   적군은 jobClass 없음 → Damage 면 일단 전부 Melee
//
// ── 사용 ────────────────────────────────────────────────────────
//   var cat = MotionCategoryResolver.Resolve(jobClass, effectType);
//   sprites.PlayAttack(cat);

public enum MotionCategory
{
    Ranged,     // 제자리 + 발사체 (TODO)
    Melee,      // 짧은 전진/복귀
    Stationary, // 제자리 sprite 전환만
}

public static class MotionCategoryResolver
{
    /// <summary>jobClass (없으면 null/빈문자열 — 적군), 스킬 effectType 으로 카테고리 결정.</summary>
    public static MotionCategory Resolve(string jobClass, string effectType)
    {
        switch (effectType)
        {
            case "Damage":
                return jobClass == "캐스터" ? MotionCategory.Ranged : MotionCategory.Melee;
            case "Heal":
            case "Shield":
            case "Buff":
            case "Debuff":
                return MotionCategory.Stationary;
            default:
                return MotionCategory.Stationary;
        }
    }
}
