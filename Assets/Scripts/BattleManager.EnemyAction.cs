// ============================================================
// BattleManager.EnemyAction.cs
// 적 턴 행동 로직 (파셜 클래스 분리 파일)
// ============================================================
//
// [왜 partial 로 분리했나요?]
//   기존 BattleManager.Combat.cs 의 적 행동 부분은 14줄짜리 단순 코드였지만,
//   이제 (1) 가중치 랜덤 스킬 선택 + (2) 타겟 결정 + (3) 다중 대상 데미지 적용
//   세 단계가 추가되어 분량이 늘어납니다.
//   기존 Combat.cs 를 비대화시키지 않고, 적 행동이라는 단일 책임을 따로 두는 게
//   읽기/수정에 유리해서 partial 로 떼어냈습니다.
//
// [Combat.cs 와의 연결 지점]
//   Combat.cs 의 ExecuteAction(false) 분기에서
//   기존의 attackPower 직타 코드 14줄을 ExecuteEnemyTurn(enemy) 호출 1줄로 교체합니다.
//
// [동작 흐름]
//   1) PickEnemySkill(enemy)   — enemy.skillIds 중 weight 가중치 랜덤 1개
//      └ 스킬 DB 없거나 skillIds 비면 null 반환 → fallback 으로 attackPower 직타
//   2) EnemySkillExecutor.ResolveTargets(skill, allies) — 살아있는 아군 중 대상 결정
//   3) 각 대상에 ApplyDamageToAlly(target, skill.power) — 실드/HP/스트레스 처리
//
// [Fallback 정책 — 안전장치]
//   - skillIds 가 빈 경우 (구 데이터 호환)
//   - EnemySkillDatabase 가 씬에 없는 경우 (사용자가 GameObject 추가 잊은 경우)
//   → 둘 다 기존 attackPower 단순 공격으로 동작 → 적이 절대 멈추지 않음
// ============================================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class BattleManager
{
    // ============================================================
    // 적 1명의 턴 행동 — Combat.cs 의 적 행동 블록에서 호출
    // ============================================================
    private void ExecuteEnemyTurn(EnemyData enemy)
    {
        if (enemy == null || enemy.isDead) return;

        // 살아있는 아군이 0명이면 행동 자체가 의미 없음
        if (allies.All(a => a.isDead)) return;

        // ── 1) 스킬 선택 ────────────────────────────────────────
        var skill = PickEnemySkill(enemy);

        if (skill == null)
        {
            // ── Fallback: 스킬 미정의 / 스킬 DB 없음 → 기존 단순 공격 ──
            var firstAlive = allies.FirstOrDefault(a => !a.isDead);
            if (firstAlive == null) return;

            ApplyDamageToAlly(firstAlive, enemy.attackPower);
            Debug.Log($"[적 행동/Fallback] {enemy.displayName} → {firstAlive.positionStack} 에게 {enemy.attackPower} 데미지 (스킬 미정의)");
            return;
        }

        // ── 2) 타겟 결정 ────────────────────────────────────────
        var targets = EnemySkillExecutor.ResolveTargets(skill, allies);
        if (targets.Count == 0)
        {
            Debug.Log($"[적 스킬] {enemy.displayName} → {skill.displayName} 타겟 없음 (전 아군 사망)");
            return;
        }

        // ── 3) 데미지 적용 ──────────────────────────────────────
        // 타겟 이름들 미리 모음 (로그용)
        string targetNames = string.Join(", ",
            targets.Select(t => t.data != null ? t.data.displayName : t.positionStack.ToString()));

        Debug.Log($"┌─────────────────────────────────────────");
        Debug.Log($"│ [적 스킬] {enemy.displayName} → {skill.displayName}");
        Debug.Log($"│  타겟 종류: {skill.targeting} ({targets.Count}명) → {targetNames}");
        Debug.Log($"│  파워: {skill.power}");
        Debug.Log($"│  설명: {skill.description}");
        Debug.Log($"└─────────────────────────────────────────");

        foreach (var t in targets)
            ApplyDamageToAlly(t, skill.power);
    }

    // ============================================================
    // 가중치 랜덤으로 적 스킬 1개 선택
    // ============================================================
    private EnemySkillData PickEnemySkill(EnemyData enemy)
    {
        if (enemy.skillIds == null || enemy.skillIds.Length == 0) return null;
        if (EnemySkillDatabase.Instance == null) return null;

        // 보유 스킬을 모두 조회 + 유효한 것만 후보에 둠
        var skills = new List<EnemySkillData>();
        int totalWeight = 0;
        foreach (var id in enemy.skillIds)
        {
            var s = EnemySkillDatabase.Instance.GetSkill(id);
            if (s == null) continue;
            int w = Mathf.Max(1, s.weight); // weight 0 이어도 최소 1로 보정
            skills.Add(s);
            totalWeight += w;
        }
        if (skills.Count == 0) return null;

        // 가중치 누적 합으로 룰렛 휠
        int roll = Random.Range(0, totalWeight);
        int acc  = 0;
        foreach (var s in skills)
        {
            acc += Mathf.Max(1, s.weight);
            if (roll < acc) return s;
        }
        return skills[skills.Count - 1]; // 부동소수 오차/엣지케이스 대비
    }
}
