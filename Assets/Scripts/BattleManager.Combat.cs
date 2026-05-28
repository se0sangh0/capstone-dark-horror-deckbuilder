// ============================================================
// BattleManager.Combat.cs
// 전투 로직 (파셜 클래스 분리 파일)
// ============================================================
//
// [이 파일이 하는 일]
//   실제 전투 계산과 관련된 모든 로직이 담겨 있습니다:
//   - 선공 판정 (DecideInitiative): 아군 선공 고정 (기획 2026.05.11 결정 — 선공 판정 폐기)
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
    // 선공 판정 — 기획 2026.05.11 결정사항
    //   §02_전투_시스템_명세 §선공 판정 — "선공 판정 폐기, 아군 선으로 지정"
    //   기존 아군/적 스킬 코스트 합산 비교 + 동점 코인 토스 로직 전부 폐기.
    //   메소드 골격(_initiativeDecided 1회 호출 보장)만 유지.
    // ===========================================================

    /// <summary>
    /// 아군 선공을 고정 지정한다. (기획 2026.05.11 결정 — 선공 판정 폐기)
    /// 전투 1회만 실행.
    /// </summary>
    private void DecideInitiative()
    {
        isAllyFirstAttacker = true;
        Debug.Log("[선공 판정] 아군 선공 고정 (기획 2026.05.11 — 선공 판정 폐기)");
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
                    GameLog.Event($"{ally.displayName ?? ally.positionStack.ToString()}이(가) 공포에 질려 행동하지 못한다.", LogCategory.Status);
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

                // ── ✨ 스킬 선택 — 발동 가능한 것 중 코스트가 가장 높은 1개 ──
                // 기획 §스택 소비 & 스킬 발동 규칙: "보유 스킬 중 공용 스택으로 발동 가능한 것 중 가장 높은 스킬 코스트 우선"
                // 또한 §MVP 고정: "동료/적 모두 기본 턴당 1회 행동"
                // → foreach 모든 스킬 시도 ❌ → 단일 best skill 만 발동 ✅
                int currentStack = PlayerRoleCost.Instance.GetAmount(allyRole);
	
                SkillData bestSkill = skills
                    .Where(s => currentStack >= s.costAmount + panicCostBonus)
                    .OrderByDescending(s => s.costAmount)
                    .FirstOrDefault();

                if (bestSkill != null)
                {
                    int effectiveCost = bestSkill.costAmount + panicCostBonus;
                    PlayerRoleCost.Instance.Use(allyRole, effectiveCost);
                    string allyName = !string.IsNullOrEmpty(ally.displayName) ? ally.displayName : allyRole.ToString();
                    int afterStack  = currentStack - effectiveCost;
                    GameLog.Event($"{allyName}이(가) [{bestSkill.displayName}]을(를) 사용했다!", LogCategory.Skill);
                    Debug.Log($"[아군 행동] {allyName} ({allyRole}) → {bestSkill.displayName}  (스택 {effectiveCost} 사용 / {currentStack}→{afterStack}{(panicCostBonus > 0 ? $", 과호흡 +{panicCostBonus}" : "")})");
                    UseSkill(ally, bestSkill);
                    usedAny = true;
                    yield return new WaitForSeconds(actionDelayTime);
                }
                else
                {
                    foreach (var s in skills)
                        Debug.Log($"[아군 스킵-개별스킬] {allyRole} {s.displayName} — 스택 부족 ({currentStack}/{s.costAmount + panicCostBonus})");
                }

                // 어떤 스킬도 발동 못 한 경우에만 미행동 보너스 (기획 §코어루프 §동료 행동)
                //  ─ 역할 스택 +1 (이월)
                //  ─ 다음 턴 순서 우선 (allies 리스트 앞으로 재정렬, HandleResultProcessing 에서 처리)
                if (!usedAny)
                {
                    _carryoverBonus.TryGetValue(allyRole, out int prev);
                    _carryoverBonus[allyRole] = prev + 1;
                    _carryoverOrderList.Add(ally);
                    Debug.Log($"[아군 미행동] {allyRole} — 모든 스킬 발동 불가 → 다음 턴 이월 +1, 순서 우선");
                }
            }
        }
        else
        {
            // 적 스킬 시스템(EnemyAction.cs) 위임 — 가중치 랜덤 스킬 + 타겟팅 분기 + 다중 대상 데미지
            // 기획 §11_적_스킬_시트: 약탈자/보스의 다중 스킬·다중 타겟 처리는 ExecuteEnemyTurn 이 담당.
            // skillIds 비었거나 EnemySkillDatabase 없으면 ExecuteEnemyTurn 내부 fallback 으로 attackPower 직타.
            var liveEnemies = enemies.Where(e => !e.isDead).ToList();
            Debug.Log($"[적 행동] 살아있는 적 수: {liveEnemies.Count}");

            foreach (var enemy in liveEnemies)
            {
                if (allies.All(a => a.isDead)) break;
                ExecuteEnemyTurn(enemy);
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
        AudioManager.Instance?.PlaySfxById(SfxId.HurtAlly);
        // 피격 알림 — 쉴드 흡수량/HP 감소량 분리 발행. UI 가 노란(흡수)/빨강(HP) popup 각각 표시.
        if (damage > 0)
        {
            int preAbsorbed = target.shield > 0 ? Mathf.Min(target.shield, damage) : 0;
            int preHpLoss   = damage - preAbsorbed;
            target.OnDamaged?.Invoke(preAbsorbed, preHpLoss);
        }
        int remaining = damage;
        // 동료 표시명 — data.displayName 우선, 없으면 역할명
        string targetName = !string.IsNullOrEmpty(target.displayName) ? target.displayName : target.positionStack.ToString();

        // ── 실드 흡수 ────────────────────────────────────────────
        if (target.shield > 0)
        {
            int absorbed  = Mathf.Min(target.shield, remaining);
            target.shield -= absorbed;
            remaining     -= absorbed;
            target.OnShieldChanged?.Invoke();
            GameLog.Event($"{targetName}의 실드가 {absorbed}을(를) 흡수!", LogCategory.Shield);
            Debug.Log($"  └ [실드 흡수] {targetName} — {absorbed} 흡수 (남은 실드: {target.shield})");
        }

        // ── HP 감소 ──────────────────────────────────────────────
        if (remaining > 0)
        {
            int beforeHp = target.CurrentHp;
            int maxHp    = target.maxHp > 0 ? target.maxHp : 100;
            target.CurrentHp = Mathf.Max(0, target.CurrentHp - remaining);
            UpdateAllyHpUI(target);
            GameLog.Event($"{targetName}이(가) {remaining}의 피해를 입었다!", LogCategory.Damage);
            Debug.Log($"  └ [데미지] {targetName} ({target.positionStack}) ← {remaining} 데미지  (HP: {beforeHp} → {target.CurrentHp}/{maxHp})");
        }

        // ── 스트레스 증가 (기획서 §스트레스: damage×0.25 - stressResist, 최소 0) ──
        int stressGain = Mathf.Max(0, Mathf.RoundToInt(damage * 0.25f) - target.stressResist);

        // TODO[M·압박 디버프]: 기획 §04 §51~99 압박 — 피격 시 스트레스 추가 +10%
        //                     이미 51~99 구간일 때만 적용. 수치 (1.10) 확정 시 활성화.
        // if (target.currentStress >= 51 && target.currentStress <= 99)
        //     stressGain = Mathf.RoundToInt(stressGain * 1.10f);

        target.currentStress = Mathf.Min(100, target.currentStress + stressGain);
        GameLog.Event($"{targetName}의 스트레스 +{stressGain} ({target.currentStress}/100)", LogCategory.Status);
        Debug.Log($"  └ [스트레스] {targetName} +{stressGain} → {target.currentStress}");

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
            GameLog.Event($"{ally.displayName ?? ally.positionStack.ToString()}이(가) 패닉! 공포 경직 발동.", LogCategory.Status);
            Debug.Log($"[패닉] {ally.positionStack} — 공포 경직 발동! (다음 턴 행동 불가)");
        }
        else
        {
            ally.isOverBreathing = true;
            GameLog.Event($"{ally.displayName ?? ally.positionStack.ToString()}이(가) 과호흡! 다음 턴 스킬 코스트 +1.", LogCategory.Status);
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
                pool.Add(card);
            }
        }

        return pool;
    }

    // ===========================================================
    // 탈진 페널티 — 손패 0 + 덱 0 인 턴의 결과 처리에서 호출
    // 기획 §02 §1) Hand Empty — "정해진 페널티(데미지 또는 스트레스)"
    // → OR 해석상 스트레스만 채택. 수치는 임시값.
    // ===========================================================
    private void ApplyExhaustionPenaltyIfNeeded()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsExhausted()) return;

        var live = allies.Where(a => !a.isDead).ToList();
        if (live.Count == 0) return;

        GameLog.Event($"탈진! 살아있는 동료 전원 스트레스 +{exhaustionStressPenalty}.", LogCategory.Status);
        Debug.Log($"[탈진] 손패 0 + 덱 0 — 살아있는 동료 {live.Count}명 스트레스 +{exhaustionStressPenalty}");
        foreach (var ally in live)
        {
            ally.currentStress = Mathf.Min(100, ally.currentStress + exhaustionStressPenalty);
            TriggerPanicIfNeeded(ally);
        }
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
            GameLog.Event($"{ally.displayName ?? ally.positionStack.ToString()}이(가) 쓰러졌다.", LogCategory.Death);
            Debug.Log($"[사망] {ally.positionStack} 사망 처리됨.");

            GameManager.Instance?.RemoveCardsOfFellow(ally);
            PartyManager.Instance?.RemoveFellow(ally);

            foreach (var survivor in allies.Where(a => !a.isDead))
            {
                survivor.currentStress = Mathf.Min(100, survivor.currentStress + 20);
                TriggerPanicIfNeeded(survivor);
                GameLog.Event($"{survivor.displayName ?? survivor.positionStack.ToString()}의 스트레스 +20 (동료 사망)", LogCategory.Status);
                Debug.Log($"[스트레스] {survivor.positionStack} +20 (동료 사망 패널티) → {survivor.currentStress}");
            }
        }

        var dyingEnemies = enemies.Where(e => e.isDead && e.CurrentHp <= 0).ToList();
        Debug.Log($"[ProcessDeath] 이번 턴 사망한 적: {dyingEnemies.Count}명");
        foreach (var enemy in dyingEnemies)
        {
            Debug.Log($"  └ {enemy.displayName} | HP:{enemy.CurrentHp} | isDead:{enemy.isDead}");
            // 기획 §15 보상 — 적 처치 시 영혼석 드롭 (고블린 8 / 약탈자 12 / 보스 20)
            // Pool 이 있으면 시각 연출 후 가산, 없으면(폴백) 즉시 가산.
            if (enemy.soulstoneDrop > 0)
            {
                Vector3 dropPos = FindEnemyWorldPos(enemy);
                if (SoulstoneDropPool.Instance != null)
                {
                    SoulstoneDropPool.Instance.SpawnAt(dropPos, enemy.soulstoneDrop);
                }
                else if (SoulstoneManager.Instance != null)
                {
                    SoulstoneManager.Instance.Add(enemy.soulstoneDrop);
                }
                GameLog.Event($"{enemy.displayName} 처치! 영혼석 +{enemy.soulstoneDrop}", LogCategory.Reward);
                Debug.Log($"  └ [보상] {enemy.displayName} 처치 → 영혼석 +{enemy.soulstoneDrop}");
            }
        }

        // 보스 사망 시 모든 소환체(까마귀 등 isPassive) 동반 사망 처리.
        // 기획: "보스가 죽으면 까마귀도 동반으로 죽는다"
        bool bossDiedThisTurn = dyingEnemies.Any(e => e.tier == EnemyTier.Boss);
        if (bossDiedThisTurn)
        {
            var aliveSummons = enemies.Where(e => e != null && !e.isDead && e.isPassive).ToList();
            foreach (var s in aliveSummons)
            {
                GameLog.Event($"{s.displayName}이(가) 보스와 함께 사라졌다.", LogCategory.Death);
                Debug.Log($"  └ [보스 동반 사망] {s.displayName} — 보스 사망에 의해 강제 제거");
                s.CurrentHp = 0; // setter 가 isDead=true + OnDied 처리
            }
        }
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
        // 기획 백로그 §5 성급 — 데미지 배율 1.25^(star-1) 적용 (배율은 FellowData 가 보유)
        int scaledPower = Mathf.RoundToInt(skill.power * user.skillPowerMultiplier);

        // 기획 §04 §51~99 압박 — 스킬 퍼포먼스 -N% (기본 -10%, 인스펙터 조정 가능)
        if (user.currentStress >= 51 && user.currentStress <= 99)
        {
            float multiplier = 1f - pressureSkillPenaltyPercent / 100f;
            int before = scaledPower;
            scaledPower = Mathf.Max(1, Mathf.RoundToInt(scaledPower * multiplier));
            Debug.Log($"[압박 디버프] {user.displayName} 스트레스 {user.currentStress} → 스킬 파워 {before}→{scaledPower} (-{pressureSkillPenaltyPercent}%)");
        }
        // if (user.currentStress >= 51 && user.currentStress <= 99)
        //     scaledPower = Mathf.RoundToInt(scaledPower * (1f - 0.10f)); // N=10% 가정
        string userName = user.displayName ?? user.positionStack.ToString();

        Debug.Log($"┌─────────────────────────────────────────");
        Debug.Log($"│ [스킬 사용] {userName} ({user.starLevel}★)  →  {skill.displayName}");
		Debug.Log($"│  파워: {skill.power} × {user.skillPowerMultiplier:F2} = {scaledPower}"); 
        Debug.Log($"│  효과: {skill.effectType}  |  대상: {skill.targeting}  |  파워: {skill.power}  (배율: ×{user.skillPowerMultiplier:F2})");
        Debug.Log($"│  사용 스택값: {skill.costAmount} ({skill.costType})");
        Debug.Log($"│  설명: {skill.description}");
        if (skill.statusEffect != "None")
            Debug.Log($"│  상태이상: {skill.statusEffect}  (수치: {skill.statusValue})");
        Debug.Log($"└─────────────────────────────────────────");

        // 모션 트리거 — View 가 effectType + jobClass 보고 Ranged/Melee/Stationary 결정
        user.OnSkillCast?.Invoke(skill.effectType);

        switch (skill.effectType)
        {
            case "Damage": ApplySkillDamage(skill, scaledPower);        break;
            case "Heal":   ApplySkillHeal(user, skill, scaledPower);    break;
            case "Shield": ApplySkillShield(user, skill, scaledPower);  break;
            case "Buff":   Debug.Log($"[UseSkill] Buff — 추후 구현 예정 (상태이상: {skill.statusEffect})");   break;
            case "Debuff": Debug.Log($"[UseSkill] Debuff — 추후 구현 예정 (상태이상: {skill.statusEffect})"); break;
            default:       Debug.LogWarning($"[UseSkill] 알 수 없는 effectType: '{skill.effectType}'");       break;
        }
    }

    /// <summary>
    /// 스킬 데미지 적용. 기획 §02 §자동 타겟팅 §아군 공격 — "전열(앞)에 배치된 적 우선".
    /// 예외 타겟팅(LowestHpEnemy / HighestHpEnemy) 은 스킬 description 에 명시된 경우에만 사용.
    /// </summary>
    private void ApplySkillDamage(SkillData skill, int power)
    {
        var liveEnemies = enemies.Where(e => !e.isDead).ToList();
        if (liveEnemies.Count == 0) { Debug.Log("[ApplySkillDamage] 살아있는 적 없음."); return; }

        switch (skill.targeting)
        {
            // 기획 §아군 공격: 전열 우선.
            //   보스는 후열 강제 배치(DefaultSetting.RelayoutCards) → 비-보스(앞열) 먼저 타겟.
            //   비-보스끼리는 원본 인덱스 순. 보스밖에 안 남으면 보스 타격.
            case "SingleEnemy":
                {
                    var front = liveEnemies
                        .OrderBy(e => e.tier == EnemyTier.Boss ? 1 : 0)
                        .ThenBy(e => enemies.IndexOf(e))
                        .First();
                    DealDamageToEnemy(front, power);
                }
                break;

            // 예외 타겟팅 — 체력 비율 최저 적 (보스 처형 등 향후 스킬에서 사용)
            case "LowestHpEnemy":
                {
                    var target = liveEnemies.OrderBy(e => EnemyHpRatio(e)).First();
                    DealDamageToEnemy(target, power);
                }
                break;

            // 예외 타겟팅 — 체력 비율 최고 적 (보스 우선 공격 등 향후 스킬에서 사용)
            case "HighestHpEnemy":
                {
                    var target = liveEnemies.OrderByDescending(e => EnemyHpRatio(e)).First();
                    DealDamageToEnemy(target, power);
                }
                break;

            case "AllEnemies":
                {
                    // 기획: 전체 데미지 스킬은 총 power 를 적 수로 분산 (적 1명당 power/적수)
                    int targetCount = liveEnemies.Count;
                    int splitPower  = targetCount > 0 ? Mathf.Max(1, power / targetCount) : power;
                    Debug.Log($"[ApplySkillDamage] AllEnemies 분산 — 총 {power} ÷ {targetCount}명 = {splitPower}/적");
                    foreach (var e in liveEnemies)
                        DealDamageToEnemy(e, splitPower);
                }
                break;

            default:
                Debug.LogWarning($"[ApplySkillDamage] 미지원 targeting: '{skill.targeting}'");
                break;
        }
    }

    /// <summary>
    /// 적 1명에게 데미지를 적용하고 로그를 남긴다.
    /// hitCountToDie > 0 (까마귀 등) 인 적은 HP 무관 hit-count 기반 처치.
    /// </summary>
    private void DealDamageToEnemy(EnemyData target, int power)
    {
        AudioManager.Instance?.PlaySfxById(SfxId.AttackSword);
        AudioManager.Instance?.PlaySfxById(SfxId.HurtEnemy);
        // 적은 쉴드 없음 — 흡수=0, HP 감소=power
        if (power > 0) target.OnDamaged?.Invoke(0, power);
        // 기획 §11 §3 까마귀: "공격 횟수 2회, 데미지 수치 무관"
        // HP 바 가시성을 위해 hit 카운트에 비례해 CurrentHp 도 줄여준다.
        // (예: maxHp=2, hitCountToDie=2 → 1 hit 마다 HP 1씩 감소)
        if (target.hitCountToDie > 0)
        {
            target.currentHits++;
            int remaining = Mathf.Max(0, target.maxHp - target.currentHits);
            target.CurrentHp = remaining; // setter 가 0 이면 isDead + OnDied 처리
            Debug.Log($"  └ [공격] {target.displayName} ← {target.currentHits}/{target.hitCountToDie} 히트 (HP {remaining}/{target.maxHp})");
            if (target.currentHits >= target.hitCountToDie)
            {
                GameLog.Event($"{target.displayName} 처치!", LogCategory.Death);
                Debug.Log($"  └ [처치] {target.displayName} — 공격 {target.hitCountToDie}회로 처치됨");
            }
            return;
        }

        // 기본: HP 기반 처치
        int beforeHp = target.CurrentHp;
        target.CurrentHp -= power;
        GameLog.Event($"{target.displayName}이(가) {power}의 피해를 입었다!", LogCategory.Damage);
        Debug.Log($"  └ [데미지] {target.displayName} ← {power} 데미지  (HP: {beforeHp} → {target.CurrentHp}/{target.maxHp})");
    }

    /// <summary>적의 현재 HP 비율을 0~1 사이로 반환한다. maxHp 가 0 이하면 0.</summary>
    private static float EnemyHpRatio(EnemyData e)
    {
        return e.maxHp > 0 ? (float)e.CurrentHp / e.maxHp : 0f;
    }

    /// <summary>아군의 현재 HP 비율을 0~1 사이로 반환한다. data 가 null 이면 maxHp 100 가정.</summary>
    private static float AllyHpRatio(FellowData a)
    {
        int max = a.maxHp > 0 ? a.maxHp : 100;
        return max > 0 ? (float)a.CurrentHp / max : 0f;
    }

    /// <summary>스킬 회복 적용. Self / SingleAlly / AllAllies 분기.</summary>
    private void ApplySkillHeal(FellowData user, SkillData skill, int power)
    {
        AudioManager.Instance?.PlaySfxById(SfxId.Heal);
        var liveAllies = allies.Where(a => !a.isDead).ToList();

        switch (skill.targeting)
        {
            case "Self":
                user.CurrentHp += power;
                UpdateAllyHpUI(user);
                GameLog.Event($"{user.displayName ?? user.positionStack.ToString()}의 HP +{power} 회복.", LogCategory.Heal);
                Debug.Log($"[ApplySkillHeal] {user.displayName ?? user.positionStack.ToString()} 자신 +{power} HP (현재: {user.CurrentHp})");
                break;
            // 기획 §02 §자동 타겟팅 §아군 지원 — "HP 비율 최저 아군 우선" (혼자 생존 시 자기 자신)
            case "SingleAlly":
                if (liveAllies.Count == 0) break;
                var healTarget = liveAllies.OrderBy(a => AllyHpRatio(a)).First();
                healTarget.CurrentHp += power;
                UpdateAllyHpUI(healTarget);
                GameLog.Event($"{healTarget.displayName ?? healTarget.positionStack.ToString()}의 HP +{power} 회복.", LogCategory.Heal);
                Debug.Log($"[ApplySkillHeal] {healTarget.displayName ?? healTarget.positionStack.ToString()} +{power} HP (현재: {healTarget.CurrentHp})");
                break;
            case "AllAllies":
                foreach (var ally in liveAllies)
                {
                    ally.CurrentHp += power;
                    UpdateAllyHpUI(ally);
                    GameLog.Event($"{ally.displayName ?? ally.positionStack.ToString()}의 HP +{power} 회복.", LogCategory.Heal);
                    Debug.Log($"[ApplySkillHeal] {ally.displayName ?? ally.positionStack.ToString()} +{power} HP (현재: {ally.CurrentHp})");
                }
                break;
            default:
                Debug.LogWarning($"[ApplySkillHeal] 미지원 targeting: '{skill.targeting}'");
                break;
        }
    }

    /// <summary>실드 스킬 적용. targeting 에 따라 Self / SingleAlly / AllAllies 분기.</summary>
    private void ApplySkillShield(FellowData user, SkillData skill, int power)
    {
        var liveAllies = allies.Where(a => !a.isDead).ToList();

        switch (skill.targeting)
        {
            case "Self":
                user.shield += power;
                user.OnShieldChanged?.Invoke();
                GameLog.Event($"{user.displayName ?? user.positionStack.ToString()}의 실드 +{power}.", LogCategory.Shield);
                Debug.Log($"[ApplySkillShield] {user.displayName ?? user.positionStack.ToString()} 자신 +{power} 실드 (현재: {user.shield})");
                break;
            // 기획 §02 §자동 타겟팅 §아군 지원 — "HP 비율 최저 아군 우선" (정책 통일)
            case "SingleAlly":
                if (liveAllies.Count == 0) break;
                var shieldTarget = liveAllies.OrderBy(a => AllyHpRatio(a)).First();
                shieldTarget.shield += power;
                shieldTarget.OnShieldChanged?.Invoke();
                GameLog.Event($"{shieldTarget.displayName ?? shieldTarget.positionStack.ToString()}의 실드 +{power}.", LogCategory.Shield);
                Debug.Log($"[ApplySkillShield] {shieldTarget.displayName ?? shieldTarget.positionStack.ToString()} +{power} 실드 (현재: {shieldTarget.shield})");
                break;
            case "AllAllies":
                foreach (var ally in liveAllies)
                {
                    ally.shield += power;
                    ally.OnShieldChanged?.Invoke();
                    GameLog.Event($"{ally.displayName ?? ally.positionStack.ToString()}의 실드 +{power}.", LogCategory.Shield);
                    Debug.Log($"[ApplySkillShield] {ally.displayName ?? ally.positionStack.ToString()} +{power} 실드 (현재: {ally.shield})");
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
            Debug.Log($"  {ally.positionStack} | HP: {ally.CurrentHp}/{ally.maxHp} | 사망: {ally.isDead} | 스트레스: {ally.currentStress} | 실드: {ally.shield}");
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
            string name = !string.IsNullOrEmpty(ally.displayName) ? ally.displayName : ally.positionStack.ToString();
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

    /// <summary>EnemyData 에 바인딩된 BattleCardView 의 world position 을 반환. 없으면 (0,0,0).</summary>
    private Vector3 FindEnemyWorldPos(EnemyData target)
    {
        if (target == null) return Vector3.zero;
        var views = Object.FindObjectsByType<BattleCardView>(FindObjectsSortMode.None);
        foreach (var v in views)
            if (v != null && v.Enemy == target) return v.transform.position;
        return Vector3.zero;
    }
}
