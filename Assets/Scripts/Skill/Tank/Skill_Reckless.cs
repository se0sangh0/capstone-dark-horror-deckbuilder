// Skill_Reckless.cs — 무모한 강타 (탱커 단일기)
// 적 1명에게 skill.power 데미지를 입힌다.

using UnityEngine;

public class Skill_Reckless : SkillEffect
{
    public const string Id = "skill_reckless";
    public override string SkillId => Id;

    public override void Execute(SkillContext ctx, SkillData skill)
    {
        var targets = ctx.Enemies.FindAll(e => !e.isDead);
        if (targets.Count == 0) { Debug.Log("[무모한 강타] 살아있는 적 없음."); return; }

        var target = targets[Random.Range(0, targets.Count)];
        target.CurrentHp -= skill.power;
        Debug.Log($"[무모한 강타] {target.displayName}에게 {skill.power} 데미지 (남은 HP: {target.CurrentHp})");
    }
}
