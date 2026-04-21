// Skill_Starlight.cs — 별부름 (서포터 단일 힐)
// 살아있는 아군 중 HP가 가장 낮은 1명을 skill.power만큼 회복한다.

using System.Linq;
using UnityEngine;

public class Skill_Starlight : SkillEffect
{
    public const string Id = "skill_starlight";
    public override string SkillId => Id;

    public override void Execute(SkillContext ctx, SkillData skill)
    {
        var liveAllies = ctx.Allies.FindAll(a => !a.isDead);
        if (liveAllies.Count == 0) { Debug.Log("[별부름] 살아있는 아군 없음."); return; }

        // HP가 가장 낮은 아군을 우선 회복
        var target = liveAllies.OrderBy(a => a.CurrentHp).First();
        target.CurrentHp += skill.power;
        ctx.OnAllyHpChanged?.Invoke(target);
        Debug.Log($"[별부름] {target.data?.displayName ?? target.positionStack.ToString()} +{skill.power} HP (현재: {target.CurrentHp})");
    }
}
