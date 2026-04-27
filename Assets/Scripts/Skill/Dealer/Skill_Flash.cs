// Skill_Flash.cs — 일섬 (딜러 강공격 단일)
// 적 1명에게 skill.power 고데미지를 입힌다.

using UnityEngine;

public class Skill_Flash : SkillEffect
{
    public const string Id = "skill_flash";
    public override string SkillId => Id;

    public override void Execute(SkillContext ctx, SkillData skill)
    {
        var targets = ctx.Enemies.FindAll(e => !e.isDead);
        if (targets.Count == 0) { Debug.Log("[일섬] 살아있는 적 없음."); return; }

        var target = targets[Random.Range(0, targets.Count)];
        target.CurrentHp -= skill.power;
        Debug.Log($"[일섬] {target.displayName}에게 {skill.power} 데미지 (남은 HP: {target.CurrentHp})");
    }
}
