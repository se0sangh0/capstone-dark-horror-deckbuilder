// MotionCategoryResolver.cs
// 공격 모션 카테고리 결정 — 스킬 effectType + actor jobClass + isRanged 플래그 기반.
//
// ── 분기 규칙 (2026-06-02 — 스킬별 isRanged 데이터 기반) ──────────
//   isRanged=true                  → Ranged    (제자리 발사 — 스킬별 지정. 동료/적 공통)
//   Damage + isRanged=false        → Melee     (전진 타격, DOTween x±0.5, 0.1s yoyo)
//   Shield / Heal / Mixed / 기타   → Stationary (제자리)
//   ※ 동료 스킬은 skills.json 의 isRanged, 적 스킬은 enemy_skills.json 의 isRanged 로 결정.
//
// ── 사용 ────────────────────────────────────────────────────────
//   var cat = MotionCategoryResolver.Resolve(jobClass, effectType, isRanged);
//   sprites.PlayAttack(cat);

public enum MotionCategory
{
    Ranged,     // 제자리 + 발사체 (TODO)
    Melee,      // 짧은 전진/복귀
    Stationary, // 제자리 sprite 전환만
}

public static class MotionCategoryResolver
{
    /// <summary>jobClass (없으면 null/빈문자열 — 적군), 스킬 effectType, isRanged 플래그로 카테고리 결정.</summary>
    public static MotionCategory Resolve(string jobClass, string effectType, bool isRanged = false)
    {
        // 명시적 원거리 플래그가 최우선 — 적 원거리 스킬 dash 제거에 사용 (Damage/Summon/Harvest/Teleport 모두 제자리).
        if (isRanged) return MotionCategory.Ranged;

        switch (effectType)
        {
            case "Damage":
                return MotionCategory.Melee;   // 원거리는 위 isRanged 분기에서 처리됨 (스킬별 데이터)
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
