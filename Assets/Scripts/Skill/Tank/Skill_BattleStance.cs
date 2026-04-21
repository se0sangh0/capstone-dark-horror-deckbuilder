// Skill_BattleStance.cs — 전투 태세 (탱커 강화 전체 실드)
// 살아있는 모든 아군에게 skill.power 실드를 부여한다.

using UnityEngine;

public class Skill_BattleStance : SkillEffect
{
    public const string Id = "skill_battle_stance";
    public override string SkillId => Id;

    public override void Execute(SkillContext ctx, SkillData skill)
    {
        var targets = ctx.Allies.FindAll(a => !a.isDead);
        foreach (var ally in targets)
        {
            ally.AddShield(skill.power);
            Debug.Log($"[전투 태세] {ally.data?.displayName ?? ally.positionStack.ToString()} 실드 +{skill.power} (현재: {ally.shield})");
        }
    }
}
