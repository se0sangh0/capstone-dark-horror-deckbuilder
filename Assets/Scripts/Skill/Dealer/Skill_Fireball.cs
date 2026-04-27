// Skill_Fireball.cs — 파이어볼 (딜러 전체 공격)
// 살아있는 모든 적에게 skill.power 데미지를 입힌다.

using UnityEngine;

public class Skill_Fireball : SkillEffect
{
    public const string Id = "skill_fireball";
    public override string SkillId => Id;

    public override void Execute(SkillContext ctx, SkillData skill)
    {
        var targets = ctx.Enemies.FindAll(e => !e.isDead);
        if (targets.Count == 0) { Debug.Log("[파이어볼] 살아있는 적 없음."); return; }

        foreach (var e in targets)
        {
            e.CurrentHp -= skill.power;
            Debug.Log($"[파이어볼] {e.displayName}에게 {skill.power} 데미지 (남은 HP: {e.CurrentHp})");
        }
    }
}
