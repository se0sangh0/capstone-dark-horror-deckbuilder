// Skill_Draw.cs — 발도 (딜러 단일 공격)
// 적 1명에게 skill.power 데미지를 입힌다.

using UnityEngine;

public class Skill_Draw : SkillEffect
{
    public const string Id = "skill_draw";
    public override string SkillId => Id;

    public override void Execute(SkillContext ctx, SkillData skill)
    {
        var targets = ctx.Enemies.FindAll(e => !e.isDead);
        if (targets.Count == 0) { Debug.Log("[발도] 살아있는 적 없음."); return; }

        var target = targets[Random.Range(0, targets.Count)];
        target.CurrentHp -= skill.power;
        Debug.Log($"[발도] {target.displayName}에게 {skill.power} 데미지 (남은 HP: {target.CurrentHp})");
    }
}
