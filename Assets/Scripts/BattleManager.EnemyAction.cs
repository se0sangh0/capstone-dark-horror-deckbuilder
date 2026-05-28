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

        // 까마귀 같은 패시브 소환체는 행동 안 함 (기획 §11 §3)
        if (enemy.isPassive)
        {
            Debug.Log($"[적 행동/스킵] {enemy.displayName} — passive 소환체");
            return;
        }

        // 살아있는 아군이 0명이면 행동 자체가 의미 없음
        if (allies.All(a => a.isDead)) return;

        AudioManager.Instance?.PlaySfxById(SfxId.EnemySkill);

        // ── 1) 스킬 선택 ────────────────────────────────────────
        var skill = PickEnemySkill(enemy);

        // 선택된 스킬에 cooldown 설정이 있으면 즉시 시작 (다음 N 턴 동안 룰렛 제외).
        if (skill != null && skill.cooldownTurns > 0)
        {
            enemy.StartSkillCooldown(skill.id, skill.cooldownTurns);
            Debug.Log($"[적 스킬/쿨다운 시작] {enemy.displayName} → {skill.displayName} | {skill.cooldownTurns}턴 동안 사용 불가");
        }

        // 모션 트리거 — View 가 effectType 기반 카테고리 결정 (적은 jobClass 없음 → Damage 면 Melee)
        // Fallback (skill null) 도 attackPower 직타이므로 "Damage" 로 발행.
        enemy.OnSkillCast?.Invoke(skill != null ? skill.effectType : "Damage");

        if (skill == null)
        {
            // ── Fallback: 스킬 미정의 / 스킬 DB 없음 → 기존 단순 공격 ──
            var firstAlive = allies.FirstOrDefault(a => !a.isDead);
            if (firstAlive == null) return;

            ApplyDamageToAlly(firstAlive, enemy.attackPower);
            GameLog.Event($"{enemy.displayName}이(가) {firstAlive.displayName ?? firstAlive.positionStack.ToString()}을(를) 공격!", LogCategory.Skill);
            Debug.Log($"[적 행동/Fallback] {enemy.displayName} → {firstAlive.positionStack} 에게 {enemy.attackPower} 데미지 (스킬 미정의)");
            return;
        }

        // ── 2) effectType 별 분기 ──────────────────────────────
        if (skill.effectType == "Summon")
        {
            ExecuteSummonSkill(enemy, skill);
            return;
        }
        if (skill.effectType == "Teleport")
        {
            ExecuteTeleportSkill(enemy, skill);
            return;
        }

        // ── 3) 타겟 결정 + 데미지 적용 ──────────────────────────
        var targets = EnemySkillExecutor.ResolveTargets(skill, allies);
        if (targets.Count == 0)
        {
            Debug.Log($"[적 스킬] {enemy.displayName} → {skill.displayName} 타겟 없음 (전 아군 사망)");
            return;
        }

        string targetNames = string.Join(", ",
            targets.Select(t => !string.IsNullOrEmpty(t.displayName) ? t.displayName : t.positionStack.ToString()));

        GameLog.Event($"{enemy.displayName}이(가) [{skill.displayName}]을(를) 사용했다!", LogCategory.Skill);
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
    // 소환 스킬 실행 — effectType="Summon" 전용
    //   기획 §11 §3 보스 까마귀 부름: 까마귀 2마리 소환
    //   summonEnemyId 의 적을 summonCount 마릿수만큼 enemies 리스트에 추가.
    // ============================================================
    private void ExecuteSummonSkill(EnemyData caster, EnemySkillData skill)
    {
        if (string.IsNullOrEmpty(skill.summonEnemyId))
        {
            Debug.LogWarning($"[Summon] {skill.id} — summonEnemyId 비어있음.");
            return;
        }
        if (EnemyDatabase.Instance == null)
        {
            Debug.LogWarning("[Summon] EnemyDatabase 없음.");
            return;
        }

        var def = EnemyDatabase.Instance.GetEnemy(skill.summonEnemyId);
        if (def == null)
        {
            Debug.LogWarning($"[Summon] 적 정의 없음: {skill.summonEnemyId}");
            return;
        }

        int count = Mathf.Max(1, skill.summonCount);

        Debug.Log($"┌─────────────────────────────────────────");
        Debug.Log($"│ [적 소환] {caster.displayName} → {skill.displayName}");
        Debug.Log($"│  소환 대상: {def.displayName} × {count}");
        Debug.Log($"└─────────────────────────────────────────");

        for (int i = 0; i < count; i++)
        {
            var summoned = EnemyDatabase.CreateRuntimeEnemy(def);
            // 소환된 턴 끝 ResultProcessing 에서 -1 되어도 정확히 summonLifeTurns 후 만료되도록 +1 보정
            summoned.currentLifeTurns = summoned.summonLifeTurns + 1;
            enemies.Add(summoned);
            RaiseEnemySpawned(summoned); // 시각 카드/이펙트/사운드 구독자에게 알림
            GameLog.Event($"{summoned.displayName}이(가) 등장!", LogCategory.Skill);
            Debug.Log($"  └ [소환됨] {summoned.displayName} (수명 {summoned.summonLifeTurns}턴 / {summoned.hitCountToDie} hit 처치)");
        }

        // TODO[K]: 보스 상태머신 — 까마귀 소환했으므로 다음 만료까지 추적.
        //          pendingTeleport 는 까마귀 만료 시 ProcessSummonExpiration 에서 켠다.
    }

    // ============================================================
    // 가중치 랜덤으로 적 스킬 1개 선택 — 강제 트리거 우선
    // ============================================================
    private EnemySkillData PickEnemySkill(EnemyData enemy)
    {
        // ── 1) 강제 스킬 (조건부 1회 발동) 우선 체크 ────────────
        var forced = TryGetForcedSkill(enemy);
        if (forced != null)
        {
            enemy.usedOnceSkills.Add(forced.id);
            Debug.Log($"[적 행동/강제] {enemy.displayName} → {forced.displayName} 발동 (1회 한정)");
            return forced;
        }

        // ── 2) 가중치 룰렛 (1회 스킬·weight 0 스킬 제외) ──────────
        if (enemy.skillIds == null || enemy.skillIds.Length == 0) return null;
        if (EnemySkillDatabase.Instance == null) return null;

        var skills = new List<EnemySkillData>();
        int totalWeight = 0;
        foreach (var id in enemy.skillIds)
        {
            var s = EnemySkillDatabase.Instance.GetSkill(id);
            if (s == null) continue;

            // weight 0 = 가중치 룰렛 제외 (강제 트리거 전용 스킬 표시)
            if (s.weight <= 0) continue;

            // 1회 한정 스킬이 이미 사용됐다면 룰렛에서 제외
            if (enemy.usedOnceSkills.Contains(id)) continue;

            // 쿨다운 중이면 룰렛에서 제외 (예: 까마귀 부름 사용 후 3턴)
            if (enemy.GetSkillCooldown(id) > 0)
            {
                Debug.Log($"[적 스킬/쿨다운] {enemy.displayName} → {s.displayName} 잔여 {enemy.GetSkillCooldown(id)}턴 — 룰렛 제외");
                continue;
            }

            // 소환 스킬이고 소환 대상이 이미 필드에 살아있으면 룰렛에서 제외
            // (예: 까마귀가 필드에 1마리라도 살아있으면 까마귀 부름 안 함 → 다른 스킬 우선)
            if (s.effectType == "Summon" && !string.IsNullOrEmpty(s.summonEnemyId))
            {
                int sameIdCount  = enemies.Count(e => e != null && e.id == s.summonEnemyId);
                int aliveIdCount = enemies.Count(e => e != null && !e.isDead && e.id == s.summonEnemyId);
                Debug.Log($"[적 스킬/소환체크] {s.displayName} 대상='{s.summonEnemyId}' | enemies 매칭 {sameIdCount}개 (살아있음 {aliveIdCount})");
                if (aliveIdCount > 0)
                {
                    Debug.Log($"[적 스킬/소환제외] {enemy.displayName} → {s.displayName} — 룰렛 제외");
                    continue;
                }
            }

            skills.Add(s);
            totalWeight += s.weight;
        }
        if (skills.Count == 0 || totalWeight <= 0) return null;

        // 디버그: 룰렛 후보 목록 + 가중치 (까마귀 부름 안 나오는 케이스 추적용)
        Debug.Log($"[적 스킬/룰렛 후보] {enemy.displayName}: " + string.Join(", ", skills.Select(x => $"{x.displayName}(w={x.weight})")) + $" | totalWeight={totalWeight}");

        // 가중치 누적 합으로 룰렛 휠
        int roll = Random.Range(0, totalWeight);
        int acc  = 0;
        foreach (var s in skills)
        {
            acc += s.weight;
            if (roll < acc) return s;
        }
        return skills[skills.Count - 1]; // 부동소수 오차/엣지케이스 대비
    }

    // ============================================================
    // 소환체 수명 카운터 + 만료 처리 — HandleResultProcessing 에서 매 턴 호출
    // ============================================================
    //   기획 §11 §3 보스 까마귀:
    //     소환 후 3턴 카운터, 0 도달 시:
    //       1) 패널티 발동 — 1마리당 expirePenaltyPower 데미지를 파티 전체에 분산 적용
    //       2) 까마귀 사망 처리
    //       3) 보스에 pendingTeleport 플래그 → 다음 보스 턴에 K(순간이동) 강제 발동
    //   까마귀가 hit-count 로 처치된 경우는 만료가 아니므로 패널티/플래그 안 발동.
    // ============================================================
    private void ProcessSummonExpiration()
    {
        var summonsAlive = enemies
            .Where(e => e != null && !e.isDead && e.summonLifeTurns > 0)
            .ToList();
        if (summonsAlive.Count == 0) return;

        var aliveAllies = allies.Where(a => !a.isDead).ToList();
        if (aliveAllies.Count == 0) return; // 패널티 대상 없음

        foreach (var summon in summonsAlive)
        {
            summon.currentLifeTurns--;
            if (summon.currentLifeTurns > 0)
            {
                Debug.Log($"[소환체] {summon.displayName} 남은 수명 {summon.currentLifeTurns}턴");
                continue;
            }

            // ── 수명 만료 — 패널티 발동 + 사망 처리 ──
            // 적 → 아군 데미지는 고정 (분산 X). 각 아군 모두 expirePenaltyPower 데미지.
            GameLog.Event($"{summon.displayName} 만료! 각 아군에게 {summon.expirePenaltyPower}의 피해.", LogCategory.Damage);
            Debug.Log($"[소환체 만료] {summon.displayName} — 패널티 {summon.expirePenaltyPower} 데미지 (각 아군 고정)");

            if (summon.expirePenaltyPower > 0)
            {
                foreach (var ally in aliveAllies)
                    ApplyDamageToAlly(ally, summon.expirePenaltyPower);
            }

            // CurrentHp=0 setter 가 isDead=true + OnDied 자동 발동 → 시각 침몰 트윈 트리거
            summon.CurrentHp = 0;

            // 소환체 만료 → 보스(거두는 자)에 순간이동 예약 플래그
            //   기획 §11 §3: "3턴 내 처치 실패 시 → 까마귀 패널티 발동 → 순간이동 상태 진입"
            var boss = enemies.FirstOrDefault(e =>
                e != null && !e.isDead && e.tier == EnemyTier.Boss);
            if (boss != null)
            {
                boss.pendingTeleport = true;
                Debug.Log($"[상태머신] {boss.displayName} — 다음 턴 순간이동 예약됨 (pendingTeleport=true)");
            }
        }
    }

    // ============================================================
    // 적별 조건부 강제 스킬 — 기획 §11_적_스킬_시트 §행동 패턴
    // ============================================================
    private EnemySkillData TryGetForcedSkill(EnemyData enemy)
    {
        if (EnemySkillDatabase.Instance == null || enemy == null) return null;

        // [I] 약탈자 — HP ≤ 30% 시 도끼던지기 강제 발동 (1회 한정)
        //     기획 §11 §2 약탈자 §행동 패턴
        if (enemy.id == "enemy_raider_01")
        {
            const string AXE_THROW = "enemy_skill_raider_throw";
            if (enemy.HpRatio <= 0.30f && !enemy.usedOnceSkills.Contains(AXE_THROW))
                return EnemySkillDatabase.Instance.GetSkill(AXE_THROW);
        }

        // [J] 보스 — 필드에 까마귀가 한 마리도 없으면 까마귀 부름 강제 발동.
        //     사용자 기획: "필드에 까마귀가 없을 시 소환 (룰렛 무관, 확정 발동)"
        //     단 cooldown 잔여가 있으면 강제 안 함 (cooldown 우선).
        if (enemy.tier == EnemyTier.Boss && !enemy.pendingTeleport)
        {
            const string SUMMON = "enemy_skill_reaper_summon";
            if (enemy.GetSkillCooldown(SUMMON) <= 0)
            {
                bool crowAlive = enemies.Any(e => e != null && !e.isDead && e.id == "enemy_crow_01");
                if (!crowAlive)
                {
                    var summon = EnemySkillDatabase.Instance.GetSkill(SUMMON);
                    if (summon != null)
                    {
                        Debug.Log($"[적 스킬/강제] {enemy.displayName} → 까마귀 부름 (필드에 까마귀 없음 → 확정 발동)");
                        return summon;
                    }
                }
            }
        }

        // [K] 보스 — 까마귀 부름 실패(수명 만료) 후 다음 턴 순간이동 강제 발동
        //     기획 §11 §3 거두는 자 §행동 패턴 [순간이동 상태]
        if (enemy.tier == EnemyTier.Boss && enemy.pendingTeleport)
        {
            const string TELEPORT = "enemy_skill_reaper_teleport";
            var skill = EnemySkillDatabase.Instance.GetSkill(TELEPORT);
            if (skill != null)
            {
                enemy.pendingTeleport = false; // 1회 발동 후 플래그 해제 → 기본 상태 복귀
                return skill;
            }
            Debug.LogWarning($"[K·Teleport] {TELEPORT} 스킬 데이터 없음 — 플래그 해제 후 일반 행동.");
            enemy.pendingTeleport = false;
        }

        return null;
    }

    // ============================================================
    // 순간이동 스킬 — effectType="Teleport"
    //   기획 §11 §3 거두는 자: 파티 배치 순서를 역순으로 변경
    //   [탱커, 딜러, 서포터, 힐러] → [힐러, 서포터, 딜러, 탱커]
    //   allies 리스트 전체 reverse (사망 자리 빈칸 포함).
    // ============================================================
    private void ExecuteTeleportSkill(EnemyData caster, EnemySkillData skill)
    {
        Debug.Log($"┌─────────────────────────────────────────");
        Debug.Log($"│ [적 스킬·순간이동] {caster.displayName} → {skill.displayName}");
        Debug.Log($"│  효과: 파티 배치 순서 역전");
        Debug.Log($"└─────────────────────────────────────────");

        string before = string.Join(", ",
            allies.Select(a => !string.IsNullOrEmpty(a?.displayName) ? a.displayName : (a?.positionStack.ToString() ?? "?")));
        allies.Reverse();
        string after = string.Join(", ",
            allies.Select(a => !string.IsNullOrEmpty(a?.displayName) ? a.displayName : (a?.positionStack.ToString() ?? "?")));
        GameLog.Event($"{caster.displayName}이(가) 진형을 뒤바꿨다!", LogCategory.Skill);
        Debug.Log($"  └ [순간이동] 배치 역전: [{before}] → [{after}]");

        // 보스 카드 비가시 연출 — fade out → 대기 → fade in.
        // 시각적으로 "보스가 순간이동하는" 느낌만 추가 (실제 위치는 동일).
        var bossCardSprites = FindCardSprites(caster);
        if (bossCardSprites != null)
            bossCardSprites.PlayTeleport();
        else
            Debug.LogWarning($"[Teleport] {caster.displayName} 의 BattleCardView 를 찾지 못해 연출 생략.");
    }

    /// <summary>EnemyData 에 바인딩된 BattleCardView 의 BattleCardSprites 를 반환. 없으면 null.</summary>
    private BattleCardSprites FindCardSprites(EnemyData target)
    {
        if (target == null) return null;
        var views = Object.FindObjectsByType<BattleCardView>(FindObjectsSortMode.None);
        foreach (var v in views)
            if (v != null && v.Enemy == target)
                return v.GetComponent<BattleCardSprites>();
        return null;
    }
}
