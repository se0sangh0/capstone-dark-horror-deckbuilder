// ============================================================
// BattleManager.Combat.cs
// 전투 로직 (파셜 클래스 분리 파일)
// ============================================================
//
// [이 파일이 하는 일]
//   실제 전투 계산과 관련된 모든 로직이 담겨 있습니다:
//   - 선공 판정 (DecideInitiative): 아군 스킬 코스트 합 vs enemyPowerScore
//   - 행동 실행 (ExecuteAction): 패닉 체크 + 스택 소비 + 스킬 실행
//   - 데미지 적용 (ApplyDamageToAlly): 실드 흡수 → HP 감소 → 스트레스 증가
//   - 패닉 처리 (TriggerPanicIfNeeded): 스트레스 100 → 공포경직 or 과호흡
//   - 카드 풀 생성 (GenerateCardPool)
//   - 사망/스트레스 처리 (ProcessDeathAndStress)
//   - 전투 종료 판정 (CheckBattleEndCondition)
//
// [이 파일에는 없는 것]
//   필드 선언, 초기화, 공개 API → BattleManager.cs
//   페이즈 핸들러 → BattleManager.Phases.cs
// ============================================================

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// partial 키워드: 이 파일이 BattleManager 클래스의 일부임을 선언
public partial class BattleManager
{
    // ===========================================================
    // 선공 판정
    // 아군 살아있는 동료 스킬 코스트 합 vs enemyPowerScore 비교.
    // 동점이면 코인 토스.
    // ===========================================================

    /// <summary>
    /// 아군 스킬 코스트 합과 enemyPowerScore 를 비교하여 선공을 결정한다.
    /// </summary>
    private void DecideInitiative()
    {
        int allyScore = allies
            .Where(a => !a.isDead)
            .SelectMany(a => a.GetSkills())
            .Sum(s => s.costAmount);

        if (allyScore > enemyPowerScore)
        {
            isAllyFirstAttacker = true;
        }
        else if (allyScore < enemyPowerScore)
        {
            isAllyFirstAttacker = false;
        }
        else
        {
            isAllyFirstAttacker = Random.value > 0.5f;
            Debug.Log($"[선공 판정] 동점({allyScore}:{enemyPowerScore}) → 코인 토스: {(isAllyFirstAttacker ? "아군" : "적")} 선공");
            return;
        }

        Debug.Log($"[선공 판정] 아군 스킬 코스트 합:{allyScore} vs 적:{enemyPowerScore} → {(isAllyFirstAttacker ? "아군" : "적")} 선공");
    }

    // ===========================================================
    // 행동 실행
    // - 아군 턴: 패닉 체크 → 스택 확인 → 스킬 실행 / 이월
    // - 적군 턴: 살아있는 적 순서대로 아군[0] 에게 공격
    // ===========================================================

    /// <summary>
    /// isAllyTurn=true 이면 아군 행동, false 이면 적군 행동을 실행한다.
    /// </summary>
    private IEnumerator ExecuteAction(bool isAllyTurn)
    {
    	if (isAllyTurn)
        {
            foreach (var ally in allies.Where(a => !a.isDead))
            {
                // 공포 경직: 이번 턴 행동 불가
                if (ally.isFrozen)
                {
                    ally.isFrozen = false;
                    Debug.Log($"[공포 경직] {ally.positionStack} — 이번 턴 행동 불가");
                    continue;
                }

                StackType allyRole = ally.positionStack;
                var skills         = ally.GetSkills();

                if (skills.Count == 0)
                {
                    Debug.LogWarning($"[BattleManager] {allyRole} — 사용 가능한 스킬 없음.");
                    continue;
                }

                // 과호흡 페널티: 이번 턴 첫 스킬 시도에만 +1, 이후 해소
                int panicCostBonus = ally.isOverBreathing ? 1 : 0;
                if (ally.isOverBreathing) ally.isOverBreathing = false;

                // 이번 턴에 이 동료가 스킬을 하나라도 발동했는지 추적
                bool usedAny = false;

                // 보유 스킬을 순서대로 시도
                foreach (var skill in skills)
                {
                    int roleStack     = PlayerRoleCost.Instance.GetAmount(allyRole);
                    int required      = skill.costAmount;
                    int effectiveCost = required + panicCostBonus;

                    if (roleStack >= effectiveCost)
                    {
                        PlayerRoleCost.Instance.Use(allyRole, effectiveCost);
                        Debug.Log($"[아군 행동] {allyRole} {skill.displayName} — 스택({roleStack}/{effectiveCost}) 충족, 스킬 실행 (과호흡 보너스: {panicCostBonus})");
                        UseSkill(ally, skill);
                        usedAny = true;
                        yield return new WaitForSeconds(actionDelayTime);
                    }
                    else
                    {
                        Debug.Log($"[아군 스킵-개별스킬] {allyRole} {skill.displayName} — 스택 부족 ({roleStack}/{effectiveCost})");
                    }

                    // 과호흡 페널티는 첫 시도에만 적용
                    panicCostBonus = 0;
                }

                // 어떤 스킬도 발동 못 한 경우에만 미행동 보너스 +1 (기획 §109)
                if (!usedAny)
                {
                    _carryoverBonus.TryGetValue(allyRole, out int prev);
                    _carryoverBonus[allyRole] = prev + 1;
                    Debug.Log($"[아군 미행동] {allyRole} — 모든 스킬 발동 불가 → 다음 턴 이월 +1");
                }
            }
        }
        else
        {
            // 살아있는 적 순서대로 아군[0] 공격
            var liveEnemies = enemies.Where(e => !e.isDead).ToList();
            Debug.Log($"[적 행동] 살아있는 적 수: {liveEnemies.Count}");

            foreach (var enemy in liveEnemies)
            {
                var targets = allies.Where(a => !a.isDead).ToList();
                if (targets.Count == 0) break;

                var target = targets[0];
                ApplyDamageToAlly(target, enemy.attackPower);
                Debug.Log($"[적 행동] {enemy.displayName} → {target.positionStack} 에게 {enemy.attackPower} 데미지");
                yield return new WaitForSeconds(actionDelayTime);
            }
        }
    }

    // ===========================================================
    // 데미지 적용
    // 실드 → HP 순서로 데미지를 흡수하고 스트레스를 증가시킨다.
    // ===========================================================

    /// <summary>
    /// 아군에게 데미지를 입힌다.
    /// 실드가 있으면 먼저 소모하고, 남은 데미지를 HP 에 적용한다.
    /// 피격 시 스트레스가 증가하며 100 도달 시 패닉이 발동된다.
    /// </summary>
    private void ApplyDamageToAlly(FellowData target, int damage)
    {
        int remaining = damage;

        // ── 실드 흡수 ────────────────────────────────────────────
        if (target.shield > 0)
        {
            int absorbed  = Mathf.Min(target.shield, remaining);
            target.shield -= absorbed;
            remaining     -= absorbed;
            target.OnShieldChanged?.Invoke();
            Debug.Log($"[실드] {target.positionStack} — {absorbed} 흡수 (남은 실드: {target.shield})");
        }

        // ── HP 감소 ──────────────────────────────────────────────
        if (remaining > 0)
        {
            target.CurrentHp = Mathf.Max(0, target.CurrentHp - remaining);
            UpdateAllyHpUI(target);
            Debug.Log($"[HP 변화] {target.positionStack} HP: {target.CurrentHp}");
        }

        // ── 스트레스 증가 (기획서 §스트레스: damage×0.25 - stressResist, 최소 0) ──
        int stressGain = Mathf.Max(0, Mathf.RoundToInt(damage * 0.25f) - target.stressResist);
        target.currentStress = Mathf.Min(100, target.currentStress + stressGain);
        Debug.Log($"[스트레스] {target.positionStack} +{stressGain} → 현재: {target.currentStress}");

        TriggerPanicIfNeeded(target);
    }

    /// <summary>
    /// 아군 HP 슬라이더 UI 를 현재 HP 값으로 갱신한다.
    /// </summary>
    private void UpdateAllyHpUI(FellowData target)
    {
        if (target.HpSlider != null)
            target.HpSlider.value = target.CurrentHp;
        else
            Debug.LogWarning($"[BattleManager] {target.positionStack} 의 HpSlider 가 null 입니다. DefaultSetting.InitHp() 가 호출되었는지 확인하세요.");
    }

    // ===========================================================
    // 패닉 처리
    // 스트레스 100 도달 시 공포 경직 or 과호흡 중 하나를 발동한다.
    // ===========================================================

    /// <summary>
    /// 스트레스가 100 이상이면 패닉을 발동한다.
    /// 스트레스를 50 으로 줄이고 50% 확률로 공포 경직 / 과호흡 중 하나 적용.
    /// </summary>
    private void TriggerPanicIfNeeded(FellowData ally)
    {
        if (ally.currentStress < 100) return;

        ally.currentStress = 50;

        if (Random.value > 0.5f)
        {
            ally.isFrozen = true;
            Debug.Log($"[패닉] {ally.positionStack} — 공포 경직 발동! (다음 턴 행동 불가)");
        }
        else
        {
            ally.isOverBreathing = true;
            Debug.Log($"[패닉] {ally.positionStack} — 과호흡 발동! (다음 턴 스킬 코스트 +1)");
        }
    }

    // ===========================================================
    // 카드 풀 생성
    // ===========================================================

    /// <summary>
    /// 역할별 더미 CardData 를 런타임에 생성하여 카드 풀을 반환한다.
    /// </summary>
    private List<CardData> GenerateCardPool()
    {
        var pool = new List<CardData>();

        foreach (StackType role in System.Enum.GetValues(typeof(StackType)))
        {
            for (int i = 0; i < 10; i++)
            {
                var card = ScriptableObject.CreateInstance<CardData>();
                card.id         = $"card_{role}_{i}";
                card.stackType  = role;
                card.stackDelta = 0;
                pool.Add(card);
            }
        }

        return pool;
    }

    // ===========================================================
    // 사망 처리 + 스트레스 전파
    // 기획서: 동료 사망 시 생존 동료 전원 스트레스 +20
    // ===========================================================

    /// <summary>
    /// 이번 턴에 HP 가 0 이하가 된 아군/적군을 처리한다.
    /// </summary>
    private void ProcessDeathAndStress()
    {
        var dyingAllies = allies.Where(a => a.isDead && a.CurrentHp <= 0).ToList();

        foreach (var ally in dyingAllies)
        {
            ally.isDead = true;
            Debug.Log($"[사망] {ally.positionStack} 사망 처리됨.");

            if (ally.data != null)
                GameManager.Instance?.RemoveCardsOfCompanion(ally.data);

            PartyManager.Instance?.RemoveFellow(ally);

            foreach (var survivor in allies.Where(a => !a.isDead))
            {
                survivor.currentStress = Mathf.Min(100, survivor.currentStress + 20);
                TriggerPanicIfNeeded(survivor);
                Debug.Log($"[스트레스] {survivor.positionStack} +20 (동료 사망 패널티) → {survivor.currentStress}");
            }
        }

        var dyingEnemies = enemies.Where(e => e.isDead && e.CurrentHp <= 0).ToList();
        Debug.Log($"[ProcessDeath] 이번 턴 사망한 적: {dyingEnemies.Count}명");
        foreach (var enemy in dyingEnemies)
            Debug.Log($"  └ {enemy.displayName} | HP:{enemy.CurrentHp} | isDead:{enemy.isDead}");
    }

    // ===========================================================
    // 전투 종료 판정
    // ===========================================================

    /// <summary>
    /// 적 전멸 또는 아군 전멸 시 true 를 반환한다.
    /// </summary>
    private bool CheckBattleEndCondition()
    {
        DebugPrintEnemyStates("[CheckBattleEnd]");

        bool allEnemiesDead = enemies.Count > 0 && enemies.All(e => e.isDead);
        bool allAlliesDead  = allies.Count  > 0 && allies.All(a  => a.isDead);

        if (allEnemiesDead) Debug.Log("[CheckBattleEnd] → 적 전멸 조건 충족!");
        if (allAlliesDead)  Debug.Log("[CheckBattleEnd] → 아군 전멸 조건 충족!");

        return allEnemiesDead || allAlliesDead;
    }

    // ===========================================================
    // 스킬 실행
    // ===========================================================

    /// <summary>동료가 스킬을 사용한다. SkillExecutor가 skill.id로 구현체를 찾아 실행한다.</summary>
    private void UseSkill(FellowData user, SkillData skill)
    {
        string userName = user.data?.displayName ?? user.positionStack.ToString();

        // ── [강화 시스템 TODO] 성급 스킬 파워 배율 ─────────────────
        // 현재는 skill.power 를 그대로 사용한다.
        // 성급 시스템 구현 후 아래 코드로 교체할 것:
        //
        //   int scaledPower = Mathf.RoundToInt(skill.power * user.skillPowerMultiplier);
        //
        // 이후 ApplySkillDamage / ApplySkillHeal 호출 시
        // skill.power 대신 scaledPower 를 전달하도록 오버로드를 추가한다.
        // (SkillData SO 는 공유 객체이므로 skill.power 직접 수정 금지)

        Debug.Log($"┌─────────────────────────────────────────");
        Debug.Log($"│ [스킬 사용] {userName} ({user.starLevel}★)  →  {skill.displayName}");
        Debug.Log($"│  효과: {skill.effectType}  |  대상: {skill.targeting}  |  파워: {skill.power}  (배율: ×{user.skillPowerMultiplier:F2})");
        Debug.Log($"│  설명: {skill.description}");
        if (skill.statusEffect != "None")
            Debug.Log($"│  상태이상: {skill.statusEffect}  (수치: {skill.statusValue})");
        Debug.Log($"└─────────────────────────────────────────");

        switch (skill.effectType)
        {
            case "Damage": ApplySkillDamage(skill);        break;
            case "Heal":   ApplySkillHeal(user, skill);    break;
            case "Shield": ApplySkillShield(user, skill);  break;
            case "Buff":   Debug.Log($"[UseSkill] Buff — 추후 구현 예정 (상태이상: {skill.statusEffect})");   break;
            case "Debuff": Debug.Log($"[UseSkill] Debuff — 추후 구현 예정 (상태이상: {skill.statusEffect})"); break;
            default:       Debug.LogWarning($"[UseSkill] 알 수 없는 effectType: '{skill.effectType}'");       break;
        }
    }

    /// <summary>스킬 데미지 적용. SingleEnemy / AllEnemies 분기.</summary>
    private void ApplySkillDamage(SkillData skill)
    {
        var liveEnemies = enemies.Where(e => !e.isDead).ToList();
        if (liveEnemies.Count == 0) { Debug.Log("[ApplySkillDamage] 살아있는 적 없음."); return; }

        switch (skill.targeting)
        {
            case "SingleEnemy":
                var target = liveEnemies[Random.Range(0, liveEnemies.Count)];
                target.CurrentHp -= skill.power;
                Debug.Log($"[ApplySkillDamage] {target.name} → {skill.power} 데미지 (남은 HP: {target.CurrentHp})");
                break;
            case "AllEnemies":
                foreach (var e in liveEnemies)
                {
                    e.CurrentHp -= skill.power;
                    Debug.Log($"[ApplySkillDamage] {e.name} → {skill.power} 데미지 (남은 HP: {e.CurrentHp})");
                }
                break;
            default:
                Debug.LogWarning($"[ApplySkillDamage] 미지원 targeting: '{skill.targeting}'");
                break;
        }
    }

    /// <summary>스킬 회복 적용. Self / SingleAlly / AllAllies 분기.</summary>
    private void ApplySkillHeal(FellowData user, SkillData skill)
    {
        var liveAllies = allies.Where(a => !a.isDead).ToList();

        switch (skill.targeting)
        {
            case "Self":
                user.CurrentHp += skill.power;
                UpdateAllyHpUI(user);
                Debug.Log($"[ApplySkillHeal] {user.data?.displayName ?? user.positionStack.ToString()} 자신 +{skill.power} HP (현재: {user.CurrentHp})");
                break;
            case "SingleAlly":
                if (liveAllies.Count == 0) break;
                var healTarget = liveAllies.OrderBy(a => a.CurrentHp).First();
                healTarget.CurrentHp += skill.power;
                UpdateAllyHpUI(healTarget);
                Debug.Log($"[ApplySkillHeal] {healTarget.data?.displayName ?? healTarget.positionStack.ToString()} +{skill.power} HP (현재: {healTarget.CurrentHp})");
                break;
            case "AllAllies":
                foreach (var ally in liveAllies)
                {
                    ally.CurrentHp += skill.power;
                    UpdateAllyHpUI(ally);
                    Debug.Log($"[ApplySkillHeal] {ally.data?.displayName ?? ally.positionStack.ToString()} +{skill.power} HP (현재: {ally.CurrentHp})");
                }
                break;
            default:
                Debug.LogWarning($"[ApplySkillHeal] 미지원 targeting: '{skill.targeting}'");
                break;
        }
    }

    /// <summary>실드 스킬 적용. targeting 에 따라 Self / SingleAlly / AllAllies 분기.</summary>
    private void ApplySkillShield(FellowData user, SkillData skill)
    {
        var liveAllies = allies.Where(a => !a.isDead).ToList();

        switch (skill.targeting)
        {
            case "Self":
                user.shield += skill.power;
                user.OnShieldChanged?.Invoke();
                Debug.Log($"[ApplySkillShield] {user.data?.displayName ?? user.positionStack.ToString()} 자신 +{skill.power} 실드 (현재: {user.shield})");
                break;
            case "SingleAlly":
                if (liveAllies.Count == 0) break;
                var shieldTarget = liveAllies[Random.Range(0, liveAllies.Count)];
                shieldTarget.shield += skill.power;
                shieldTarget.OnShieldChanged?.Invoke();
                Debug.Log($"[ApplySkillShield] {shieldTarget.data?.displayName ?? shieldTarget.positionStack.ToString()} +{skill.power} 실드 (현재: {shieldTarget.shield})");
                break;
            case "AllAllies":
                foreach (var ally in liveAllies)
                {
                    ally.shield += skill.power;
                    ally.OnShieldChanged?.Invoke();
                    Debug.Log($"[ApplySkillShield] {ally.data?.displayName ?? ally.positionStack.ToString()} +{skill.power} 실드 (현재: {ally.shield})");
                }
                break;
            default:
                Debug.LogWarning($"[ApplySkillShield] 미지원 targeting: '{skill.targeting}'");
                break;
        }
    }

    private void DebugPrintEnemyStates(string context = "")
    {
        Debug.Log($"{context} ── 적 상태 전체 ({enemies.Count}명) ──────────────");
        for (int i = 0; i < enemies.Count; i++)
        {
            var e = enemies[i];
            if (e == null)
            {
                Debug.LogError($"  [{i}] NULL ← Inspector 리스트에 빈 슬롯 있음!");
                continue;
            }
            string status = e.isDead ? "사망" : "생존";
            Debug.Log($"  [{i}] {e.displayName} | HP:{e.CurrentHp}/{e.maxHp} | isDead:{e.isDead} | {status}");
        }
    }

    // ===========================================================
    // [ContextMenu] 에디터 테스트 메서드
    // ===========================================================

    /// <summary>[에디터 테스트] 살아있는 아군 전체에게 10 데미지를 입힌다.</summary>
    [ContextMenu("TEST / 아군 전체 10 데미지")]
    private void TestDamageAllAllies()
    {
        foreach (var ally in allies.Where(a => !a.isDead))
            ApplyDamageToAlly(ally, 10);
    }

    /// <summary>[에디터 테스트] 살아있는 아군 중 랜덤 1명에게 10 데미지를 입힌다.</summary>
    [ContextMenu("TEST / 아군 랜덤 1명 10 데미지")]
    private void TestDamageRandomAlly()
    {
        var targets = allies.Where(a => !a.isDead).ToList();
        if (targets.Count == 0)
        {
            Debug.LogWarning("[BattleManager] 살아있는 아군이 없습니다.");
            return;
        }
        ApplyDamageToAlly(targets[Random.Range(0, targets.Count)], 10);
    }

    /// <summary>[에디터 테스트] 현재 아군 HP 상태를 콘솔에 출력한다.</summary>
    [ContextMenu("TEST / 아군 HP 상태 출력")]
    private void TestPrintAlliesHP()
    {
        Debug.Log($"[BattleManager] 아군 HP 상태 ({allies.Count}명):");
        foreach (var ally in allies)
            Debug.Log($"  {ally.positionStack} | HP: {ally.CurrentHp}/{ally.data?.maxHp ?? 0} | 사망: {ally.isDead} | 스트레스: {ally.currentStress} | 실드: {ally.shield}");
    }

    /// <summary>[에디터 테스트] 현재 전투 페이즈 및 선공 정보를 출력한다.</summary>
    [ContextMenu("TEST / 전투 상태 출력")]
    private void TestPrintBattleState()
    {
        Debug.Log($"[BattleManager] 현재 페이즈: {currentPhase}");
        Debug.Log($"[BattleManager] 선공: {(isAllyFirstAttacker ? "아군" : "적군")}");
        Debug.Log($"[BattleManager] 아군 {allies.Count}명 / 적군 {enemies.Count}명");
    }

    /// <summary>[에디터 테스트] 아군 스킬 배정 무결성 검사.</summary>
    [ContextMenu("TEST / 스킬 배정 무결성 검사")]
    private void TestSkillAssignmentIntegrity()
    {
        Debug.Log("[BattleManager] ── 스킬 배정 무결성 검사 시작 ──");
        int errors = 0;

        if (SkillDatabase.Instance == null)
        { Debug.LogError("  [오류] SkillDatabase 인스턴스 없음 — 씬에 배치하세요."); return; }

        if (allies.Count == 0)
        { Debug.LogWarning("  [경고] allies 없음 — 전투 시작 후 테스트하세요."); return; }

        foreach (var ally in allies)
        {
            string name = ally.data?.displayName ?? ally.positionStack.ToString();
            if (ally.data == null)    { Debug.LogError($"  [오류] {name}: data(CompanionData) null"); errors++; continue; }
            if (!ally.HasSkills)      { Debug.LogError($"  [오류] {name}: 스킬 배정 안 됨 (HasSkills == false)"); errors++; continue; }

            var skills = ally.GetSkills();
            Debug.Log($"  [{name}] 스킬 {skills.Count}개:");
            for (int i = 0; i < skills.Count; i++)
            {
                string tag = (i == 0) ? "[활성]         " : "[비활성-테스트용]";
                Debug.Log($"    {tag} {skills[i].displayName}  |  {skills[i].effectType}  {skills[i].targeting}  파워:{skills[i].power}  코스트:{skills[i].costAmount}");
            }
        }

        if (errors == 0) Debug.Log($"[BattleManager] 무결성 검사 통과! ({allies.Count}명)");
        else             Debug.LogError($"[BattleManager] 무결성 검사 실패: {errors}개 오류.");
    }

    /// <summary>[에디터 테스트] 아군 [0] 스킬 강제 실행.</summary>
    [ContextMenu("TEST / 아군 [0] 스킬 강제 실행")]
    private void TestForceUseFirstAllySkill()
    {
        var live = allies.Where(a => !a.isDead).ToList();
        if (live.Count == 0) { Debug.LogWarning("[BattleManager] 살아있는 아군 없음."); return; }
        var skills = live[0].GetSkills();
        if (skills.Count == 0) { Debug.LogWarning("[BattleManager] 배정된 스킬 없음."); return; }
        UseSkill(live[0], skills[0]);
    }

    /// <summary>[에디터 테스트] 아군 [0] 에게 패닉 강제 발동.</summary>
    [ContextMenu("TEST / 아군 [0] 패닉 강제 발동")]
    private void TestForcePanic()
    {
        var live = allies.Where(a => !a.isDead).ToList();
        if (live.Count == 0) { Debug.LogWarning("[BattleManager] 살아있는 아군 없음."); return; }
        live[0].currentStress = 100;
        TriggerPanicIfNeeded(live[0]);
        Debug.Log($"[TEST] {live[0].positionStack} — 패닉 발동 (경직:{live[0].isFrozen} 과호흡:{live[0].isOverBreathing})");
    }
}
