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
                string allyName  = ally.positionStack.ToString();
                int roleStack    = PlayerRoleCost.Instance.GetAmount(ally.positionStack);
                int totalStack   = roleStack + ally.currentStack;  // 이월 스택 포함
                int required     = ally.data?.requiredStack ?? 3;

                if (totalStack >= required)
                {
                    // 스택 충분: 행동 실행
                    Debug.Log($"[아군 행동] {allyName} — 스택({totalStack}/{required}) 충족, 행동 실행!");
                    PlayerRoleCost.Instance.Use(ally.positionStack, required);
                    ally.currentStack = 0;  // 이월 스택 소모

                    // TODO: 실제 스킬 실행 (SkillDefinition / SkillDatabase 연동 후 교체)
                    yield return new WaitForSeconds(actionDelayTime);
                }
                else
                {
                    // 스택 부족: 스킵 + 이월 보너스 +1
                    ally.currentStack += 1;
                    Debug.Log($"[아군 스킵] {allyName} — 스택 부족 ({totalStack}/{required}) → 이월 보너스 +1 (누적: {ally.currentStack})");
                }
            }
        }
        else
        {
            // 살아있는 적 각각 행동 처리
            foreach (var enemy in enemies.Where(e => !e.isDead))
            {
                var targets = allies.Where(a => !a.isDead).ToList();
                if (targets.Count == 0) break;

                // 랜덤 아군 타겟 선택
                var target = targets[Random.Range(0, targets.Count)];
                ApplyDamageToAlly(target, enemy.attackPower);

                Debug.Log($"[적 행동] {enemy.enemyName} → {target.positionStack} 에게 {enemy.attackPower} 데미지");
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
        var dyingAllies = allies.Where(a => a.CurrentHp <= 0 && !a.isDead).ToList();

        foreach (var ally in dyingAllies)
        {
            ally.isDead = true;
            Debug.Log($"[사망] {ally.positionStack} 사망 처리됨.");

            // 덱에서 해당 동료 카드 제거
            if (ally.data != null)
                GameManager.Instance?.RemoveCardsOfCompanion(ally.data);

            // PartyManager 에도 사망 통보
            PartyManager.Instance?.RemoveCompanion(ally.data);

            // 생존 아군 전원 스트레스 +20
            foreach (var survivor in allies.Where(a => !a.isDead))
            {
                survivor.currentStress += 20;
                Debug.Log($"[스트레스] {survivor.positionStack} +20 (동료 사망 패널티)");
            }
        }

        // 적 사망 처리
        foreach (var enemy in enemies.Where(e => e.currentHp <= 0 && !e.isDead))
        {
            enemy.isDead = true;
            Debug.Log($"[적 사망] {enemy.enemyName} 처치됨.");
        }
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
        if (enemies.Count > 0 && enemies.All(e => e.isDead)) return true;
        if (allies.Count  > 0 && allies.All(a  => a.isDead)) return true;
        return false;
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
}
