// ============================================================
// Enemy_Skill/EnemySkillExecutor.cs
// 적 스킬 → 타겟 아군 목록을 결정하는 정적 헬퍼
// ============================================================
//
// [왜 이렇게 분리했나요?]
//   "데미지를 누구에게 줄 것인가?" 라는 타겟 결정 로직만 따로 빼서,
//   BattleManager 가 "결정된 타겟에 데미지 적용" 만 책임지도록 단순화.
//   동료 SkillExecutor 와 다른 점은 적 스킬은 사용자(EnemyData)와 대상(FellowData)이
//   서로 다른 타입이라 별도 디스패처가 필요했다는 점입니다.
//
// [실제 데미지 적용은 어디서?]
//   타겟이 정해지면 BattleManager.ApplyDamageToAlly 가 데미지/실드/스트레스를
//   계산합니다. 그 메서드는 partial 의 private 이라 외부에서 못 부르므로,
//   여기서는 "타겟 목록만 반환" 하고, 실제 호출은 BattleManager.EnemyAction 안에서
//   합니다. (호출 흐름이 분리돼서 디버깅하기도 쉬워집니다.)
//
// [추가 시 주의]
//   새 targeting 종류 추가하려면 enemy_skills.json 에서 사용하기 전에
//   여기 switch 문에 case 를 먼저 넣어야 합니다.
// ============================================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EnemySkillExecutor
{
    /// <summary>
    /// 적 스킬의 targeting 문자열을 보고, 데미지를 받을 살아있는 아군 목록을 반환한다.
    /// 살아있는 아군이 0명이면 빈 리스트 반환.
    /// caster 에 taunt 가 활성이고 SingleEnemy/FrontFirst/BackLast 류 타겟팅이면 taunter 가 우선 (워크라이).
    /// AllAllies / FrontTwo 류는 도발 영향 없음 ("도발 불가" 폴백 = 데미지만).
    /// </summary>
    public static List<FellowData> ResolveTargets(EnemySkillData skill, List<FellowData> allies, EnemyData caster = null)
    {
        var live = allies.Where(a => !a.isDead).ToList();
        if (live.Count == 0) return new List<FellowData>();

        // 도발 우선 — 단일 타겟 류 (FrontFirst, BackLast, RandomAlly) 일 때만 적용. taunter 가 살아있고 allies 에 포함되어야 함.
        bool single = skill.targeting == "FrontFirst" || skill.targeting == "BackLast" || skill.targeting == "RandomAlly";
        if (single && caster != null && caster.tauntTurnsLeft > 0
            && caster.taunter != null && !caster.taunter.isDead && live.Contains(caster.taunter))
        {
            return new List<FellowData> { caster.taunter };
        }

        switch (skill.targeting)
        {
            // 전열 1번 = 살아있는 아군 인덱스 0
            case "FrontFirst":
                return new List<FellowData> { live[0] };

            // 전열 1·2번 — 살아있는 아군이 1명뿐이면 1명만
            case "FrontTwo":
                return live.Take(2).ToList();

            // 후열 마지막 = 살아있는 아군 마지막 인덱스
            case "BackLast":
                return new List<FellowData> { live[live.Count - 1] };

            case "AllAllies":
                return live;

            case "RandomAlly":
                return new List<FellowData> { live[Random.Range(0, live.Count)] };

            default:
                Debug.LogWarning($"[EnemySkillExecutor] 미지원 targeting: '{skill.targeting}' — 빈 타겟 반환");
                return new List<FellowData>();
        }
    }
}
