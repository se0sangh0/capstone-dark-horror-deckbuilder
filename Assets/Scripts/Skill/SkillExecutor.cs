// SkillExecutor.cs — 스킬 ID를 개별 스킬 스크립트에 연결하는 디스패처
// BattleManager.Combat.cs의 UseSkill()에서 단일 진입점으로 호출됩니다.

using System;
using System.Collections.Generic;
using UnityEngine;

public static class SkillExecutor
{
    private static readonly Dictionary<string, SkillEffect> _registry = new()
    {
        [Skill_Draw.Id]          = new Skill_Draw(),
        [Skill_Fireball.Id]      = new Skill_Fireball(),
        [Skill_Flash.Id]         = new Skill_Flash(),
        [Skill_MagicMissile.Id]  = new Skill_MagicMissile(),
        [Skill_BattleStance.Id]  = new Skill_BattleStance(),
        [Skill_Guard.Id]         = new Skill_Guard(),
        [Skill_Indomitable.Id]   = new Skill_Indomitable(),
        [Skill_Reckless.Id]      = new Skill_Reckless(),
        [Skill_Prayer.Id]        = new Skill_Prayer(),
        [Skill_Starlight.Id]     = new Skill_Starlight(),
    };

    /// <summary>스킬을 실행한다. ID에 맞는 SkillEffect를 찾아 Execute를 호출.</summary>
    public static void Execute(
        FellowData user,
        SkillData skill,
        List<FellowData> allies,
        List<EnemyData> enemies,
        Action<FellowData> onAllyHpChanged)
    {
        string userName = user.data?.displayName ?? user.positionStack.ToString();
        Debug.Log($"┌─────────────────────────────────────────");
        Debug.Log($"│ [스킬 사용] {userName}  →  {skill.displayName}");
        Debug.Log($"│  효과: {skill.effectType}  |  대상: {skill.targeting}  |  파워: {skill.power}");
        Debug.Log($"└─────────────────────────────────────────");

        if (_registry.TryGetValue(skill.id, out var effect))
        {
            effect.Execute(new SkillContext
            {
                User            = user,
                Allies          = allies,
                Enemies         = enemies,
                OnAllyHpChanged = onAllyHpChanged,
            }, skill);
        }
        else
        {
            Debug.LogWarning($"[SkillExecutor] 등록되지 않은 스킬 ID: '{skill.id}'");
        }
    }
}
