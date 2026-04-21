// Skill_Prayer.cs — 기원 (서포터 전체 힐)
// 살아있는 모든 아군의 HP를 skill.power만큼 회복한다.

using UnityEngine;

public class Skill_Prayer : SkillEffect
{
    public const string Id = "skill_prayer";
    public override string SkillId => Id;

    public override void Execute(SkillContext ctx, SkillData skill)
    {
        var targets = ctx.Allies.FindAll(a => !a.isDead);
        foreach (var ally in targets)
        {
            ally.CurrentHp += skill.power;
            ctx.OnAllyHpChanged?.Invoke(ally);
            Debug.Log($"[기원] {ally.data?.displayName ?? ally.positionStack.ToString()} +{skill.power} HP (현재: {ally.CurrentHp})");
        }
    }
}
