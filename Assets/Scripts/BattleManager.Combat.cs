// ============================================================
// BattleManager.Combat.cs
// 전투 로직 (파셜 클래스 분리 파일)
// ============================================================
//
// [이 파일이 하는 일]
//   실제 전투 계산과 관련된 모든 로직이 담겨 있습니다:
//   - 선공 판정 (DecideInitiative)
//   - 행동 실행 (ExecuteAction): 스택 소비 및 스킬 실행
//   - 데미지 적용 (ApplyDamageToAlly): HP 감소 + UI 업데이트
//   - 카드 풀 생성 (GenerateCardPool)
//   - 사망/스트레스 처리 (ProcessDeathAndStress)
//   - 전투 종료 판정 (CheckBattleEndCondition)
//   - 에디터 테스트 메서드
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
    // 기획서: 아군 점수 = Σ(동료 스택) + 이번 턴 카드 스택 합
    // ===========================================================

    /// <summary>
    /// 아군과 적의 스택 점수를 비교하여 선공을 결정한다.
    /// 현재: 프로토타입 — 아군 선공 고정.
    /// TODO: 실제 스택 합산 비교 로직으로 교체 예정.
    /// </summary>
    private void DecideInitiative()
    {
        // 프로토타입: 아군 선공 고정
        isAllyFirstAttacker = true;
        Debug.Log("[선공 판정] 프로토타입 — 아군 선공 고정");

        // TODO: 아래 실제 판정 로직 (스택 합산 구현 후 활성화)
        // int allyTotalStack = allies.Where(a => !a.isDead).Sum(a => a.currentStack);
        // if      (allyTotalStack > enemyPowerScore) isAllyFirstAttacker = true;
        // else if (allyTotalStack < enemyPowerScore) isAllyFirstAttacker = false;
        // else
        // {
        //     isAllyFirstAttacker = Random.value > 0.5f;
        //     Debug.Log($"동점 → 코인 토스: {(isAllyFirstAttacker ? "아군 선공" : "적 선공")}");
        // }
    }

    // ===========================================================
    // 행동 실행
    // - 아군 턴: 스택이 충분하면 행동, 부족하면 이월 보너스 +1
    // - 적군 턴: 랜덤 아군에게 attackPower 데미지
    // ===========================================================

    /// <summary>
    /// isAllyTurn=true 이면 아군 행동, false 이면 적군 행동을 실행한다.
    /// </summary>
    private IEnumerator ExecuteAction(bool isAllyTurn)
    {
        if (isAllyTurn)
        {
            // 살아있는 아군 각각 행동 처리
            foreach (var ally in allies.Where(a => !a.isDead))
            {
                StackType allyRole  = ally.positionStack;
                int roleStack    = PlayerRoleCost.Instance.GetAmount(ally.positionStack);
                //int totalStack   = roleStack + ally.currentStack;  // 이월 스택 포함
                int required     = ally.data?.requiredStack ?? 3;

                //if (totalStack >= required)
                if(roleStack >= required)
                {
                    // 스택 충분: 행동 실행
                    //Debug.Log($"[아군 행동] {allyName} — 스택({totalStack}/{required}) 충족, 행동 실행!");
                    Debug.Log($"[아군 행동] {allyRole} — 스택({roleStack}/{required}) 충족, 행동 실행!");
                    PlayerRoleCost.Instance.Use(ally.positionStack, required);
                    //ally.currentStack = 0;  // 이월 스택 소모

                    // ── 스킬 실행 ────────────────────────────────
                    var skills = ally.GetSkills();
                    if (skills.Count > 0)
                    {
                        UseSkill(ally, skills[0]);                               // 스킬 1 — 활성
                        // if (skills.Count > 1) UseSkill(ally, skills[1]);     // 스킬 2 — 테스트용 주석 처리
                    }
                    else
                    {
                        Debug.LogWarning($"[BattleManager] {allyRole} — 사용 가능한 스킬 없음.");
                    }

                    yield return new WaitForSeconds(actionDelayTime);
                }
                else
                {
                    // 스택 부족: 스킵 + 이월 보너스 +1
                   //ally.currentStack += 1;
                   PlayerRoleCost.Instance.Add(allyRole, 1);
                   //Debug.Log($"[아군 스킵] {allyName} — 스택 부족 ({totalStack}/{required}) → 이월 보너스 +1 (누적: {ally.currentStack})");
                   Debug.Log($"[아군 스킵] {allyRole} — 스택 부족 ({PlayerRoleCost.Instance.GetAmount(allyRole)}/{required}) → 이월 보너스 +1)");
                }
            }
        }
        else
        {
            // ── 살아있는 적 목록 기준으로 순서대로 공격 ─────────────
            // 주의: enemies[0] 고정이 아닌 liveEnemies 순서대로 처리
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
    // FellowData.CurrentHp 에 데미지를 적용하고 UI 를 갱신한다.
    // ===========================================================

    /// <summary>
    /// 아군에게 데미지를 입히고 HP 슬라이더 UI 를 업데이트한다.
    /// DefaultSetting.cs 에서 InitHp(slider) 로 슬라이더를 연결한 후 호출된다.
    /// </summary>
    private void ApplyDamageToAlly(FellowData target, int damage)
    {
        // CurrentHp setter 가 자동으로 Clamp, OnHpChanged 이벤트, 사망 처리를 담당
        target.CurrentHp = Mathf.Max(0, target.CurrentHp - damage);
        UpdateAllyHpUI(target);
        Debug.Log($"[HP 변화] {target.positionStack} HP: {target.CurrentHp}");
    }

    /// <summary>
    /// 아군 HP 슬라이더 UI 를 현재 HP 값으로 갱신한다.
    /// HpSlider 가 null 이면 경고 출력.
    /// </summary>
    private void UpdateAllyHpUI(FellowData target)
    {
        if (target.HpSlider != null)
            target.HpSlider.value = target.CurrentHp;
        else
            Debug.LogWarning($"[BattleManager] {target.positionStack} 의 HpSlider 가 null 입니다. DefaultSetting.InitHp() 가 호출되었는지 확인하세요.");
    }

    // ===========================================================
    // 카드 풀 생성
    // 역할별 더미 CardData 를 런타임에 생성합니다.
    // ===========================================================

    /// <summary>
    /// 역할별 더미 CardData 를 런타임에 생성하여 카드 풀을 반환한다.
    /// DeckBuilder 에 전달하여 파티 덱을 구성한다.
    /// </summary>
    private List<CardData> GenerateCardPool()
    {
        var pool = new List<CardData>();

        foreach (StackType role in System.Enum.GetValues(typeof(StackType)))
        {
            for (int i = 0; i < 10; i++)
            {
                var card = ScriptableObject.CreateInstance<CardData>();
                card.id        = $"card_{role}_{i}";
                card.stackType = role;
                card.stackDelta = 0; // 실제 스택 값은 성향 기반으로 런타임 생성됨
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
    /// - 아군 사망: isDead=true, 덱 카드 제거, 생존자 스트레스 +20
    /// - 적 사망: isDead=true
    /// </summary>
    private void ProcessDeathAndStress()
    {
        // 이번 사이클에서 새로 사망한 아군 목록
        var dyingAllies = allies.Where(a => a.isDead && a.CurrentHp <= 0).ToList();

        foreach (var ally in dyingAllies)
        {
            ally.isDead = true;
            Debug.Log($"[사망] {ally.positionStack} 사망 처리됨.");

            // 덱에서 해당 동료 카드 제거
            if (ally.data != null)
                GameManager.Instance?.RemoveCardsOfCompanion(ally.data);

            // PartyManager 에 사망 통보 — 스킬 초기화(ClearSkills) 포함
            PartyManager.Instance?.RemoveFellow(ally);

            // 생존 아군 전원 스트레스 +20
            foreach (var survivor in allies.Where(a => !a.isDead))
            {
                survivor.currentStress += 20;
                Debug.Log($"[스트레스] {survivor.positionStack} +20 (동료 사망 패널티)");
            }
        }

        // ── 적 사망 처리 ─────────────────────────────────────────
        // CurrentHp setter가 isDead=true를 설정하므로 isDead만 체크
        // ※ CurrentHp=0 이지만 InitHp() 미호출된 적 오감지 방지:
        //    → isDead가 true인 것만 처리 (setter 경유 사망만 유효)
        var dyingEnemies = enemies.Where(e => e.isDead && e.CurrentHp <= 0).ToList();

        Debug.Log($"[ProcessDeath] 이번 턴 사망한 적: {dyingEnemies.Count}명");
        foreach (var enemy in dyingEnemies)
            Debug.Log($"  └ {enemy.displayName} | HP:{enemy.CurrentHp} | isDead:{enemy.isDead}");
    }

    // ===========================================================
    // 전투 종료 판정
    // ===========================================================

    /// <summary>
    /// 전투 종료 조건을 판정한다.
    /// 적 전멸 또는 아군 전멸 시 true 를 반환한다.
    /// </summary>
    private bool CheckBattleEndCondition()
    {
        // ── 디버깅: 전투 종료 판정 전 전체 적 상태 출력 ──────────
        DebugPrintEnemyStates("[CheckBattleEnd]");

        bool allEnemiesDead = enemies.Count > 0 && enemies.All(e => e.isDead);
        bool allAlliesDead  = allies.Count  > 0 && allies.All(a  => a.isDead);

        if (allEnemiesDead)
            Debug.Log("[CheckBattleEnd] → 적 전멸 조건 충족!");
        if (allAlliesDead)
            Debug.Log("[CheckBattleEnd] → 아군 전멸 조건 충족!");

        return allEnemiesDead || allAlliesDead;
    }

    // ===========================================================
    // [ContextMenu] 에디터 테스트 메서드
    // Inspector 에서 컴포넌트 우클릭 → TEST 항목으로 실행
    // ===========================================================

    // ===========================================================
    // 스킬 실행
    // ===========================================================

    /// <summary>동료가 스킬을 사용한다. effectType 에 따라 Damage / Heal / Shield 분기.</summary>
    private void UseSkill(FellowData user, SkillData skill)
    {
        string userName = user.data?.displayName ?? user.positionStack.ToString();

        Debug.Log($"┌─────────────────────────────────────────");
        Debug.Log($"│ [스킬 사용] {userName}  →  {skill.displayName}");
        Debug.Log($"│  효과: {skill.effectType}  |  대상: {skill.targeting}  |  파워: {skill.power}");
        Debug.Log($"│  설명: {skill.description}");
        if (skill.statusEffect != "None")
            Debug.Log($"│  상태이상: {skill.statusEffect}  (수치: {skill.statusValue})");
        Debug.Log($"└─────────────────────────────────────────");

        switch (skill.effectType)
        {
            case "Damage": ApplySkillDamage(skill);        break;
            case "Heal":   ApplySkillHeal(user, skill);    break;
            case "Shield": Debug.Log($"[UseSkill] Shield — 추후 구현 예정 (파워: {skill.power})");            break;
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
            string status = e.isDead ? "💀 사망" : "✅ 생존";
            Debug.Log($"  [{i}] {e.displayName} | HP:{e.CurrentHp}/{e.maxHp} | isDead:{e.isDead} | {status}");
        }
    }
    
    // ===========================================================
    // [ContextMenu] 에디터 테스트 메서드
    // Inspector 에서 컴포넌트 우클릭 → TEST 항목으로 실행
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
            Debug.Log($"  {ally.positionStack} | HP: {ally.CurrentHp}/{ally.data?.maxHp ?? 0} | 사망: {ally.isDead} | 스트레스: {ally.currentStress}");
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
                Debug.Log($"    {tag} {skills[i].displayName}  |  {skills[i].effectType}  {skills[i].targeting}  파워:{skills[i].power}");
            }
        }

        if (errors == 0) Debug.Log($"[BattleManager] ✓ 무결성 검사 통과! ({allies.Count}명)");
        else             Debug.LogError($"[BattleManager] ✗ 무결성 검사 실패: {errors}개 오류.");
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
}
