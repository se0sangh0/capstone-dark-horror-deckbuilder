// ============================================================
// Event/EventService.cs
// `?` 노드 선택지 이벤트 — 코스트 지불 + 결과 추첨 + 효과 적용
// ============================================================
//
// [흐름]
//   EventPanel 이 선택지 클릭 시 ResolveChoice(choice) 호출
//   → ① 코스트 지불 (영혼석/HP/스트레스)
//   → ② outcomes 가중 랜덤으로 결과 1건 추첨
//   → ③ 결과의 effects 를 게임 상태에 적용
//   → 선택된 EventOutcome 반환 (패널이 resultText 표시)
//
// [적용 범위]
//   영혼석 / 스트레스 / HP 는 완전 적용.
//   동료 합류·스택 선지급·성향 재굴림·오브제·오염도는 관련 시스템이
//   미결(§6)이거나 별도 연동이 필요해 GameLog + Debug 로그로 남기고
//   TODO 로 표시한다. (자원 3종만으로도 리스크-리턴 선택은 성립한다)
// ============================================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EventService
{
    /// <summary>영혼석 코스트를 지불할 수 있는지. (HP/스트레스 코스트는 항상 지불 가능으로 본다)</summary>
    public static bool CanAfford(EventChoice choice)
    {
        if (choice == null) return false;
        if (choice.costType == EventCostType.SoulStone)
            return (SoulstoneManager.Instance?.Amount ?? 0) >= choice.costAmount;
        return true;
    }

    /// <summary>
    /// 선택지를 확정한다. 코스트 지불 → 결과 추첨 → 효과 적용 → 선택된 결과 반환.
    /// 코스트를 못 내면 null 반환(패널에서 사전 차단되지만 안전용).
    /// </summary>
    public static EventOutcome ResolveChoice(EventDefinition evt, EventChoice choice)
    {
        if (choice == null) return null;
        if (!PayCost(choice)) return null;

        var outcome = RollOutcome(choice);
        if (outcome != null)
        {
            foreach (var eff in outcome.effects) ApplyEffect(eff);
            if (!string.IsNullOrEmpty(outcome.resultText))
                GameLog.Event(outcome.resultText, LogCategory.Status);
        }
        return outcome;
    }

    // ── 코스트 지불 ─────────────────────────────────────────────
    private static bool PayCost(EventChoice choice)
    {
        switch (choice.costType)
        {
            case EventCostType.SoulStone:
                if (choice.costAmount <= 0) return true;
                if (SoulstoneManager.Instance == null) return false;
                if (!SoulstoneManager.Instance.Use(choice.costAmount))
                {
                    GameLog.Event($"영혼석 부족 (필요 {choice.costAmount}).", LogCategory.Default);
                    return false;
                }
                GameLog.Event($"영혼석 -{choice.costAmount}", LogCategory.Reward);
                return true;

            case EventCostType.Hp:
                ApplyHp(-Mathf.Abs(choice.costAmount), EventTarget.All);
                return true;

            case EventCostType.Stress:
                ApplyStress(Mathf.Abs(choice.costAmount), EventTarget.All);
                return true;

            default:
                return true; // None
        }
    }

    // ── 결과 가중 추첨 ─────────────────────────────────────────
    private static EventOutcome RollOutcome(EventChoice choice)
    {
        var outs = choice.outcomes;
        if (outs == null || outs.Count == 0) return null;
        if (outs.Count == 1) return outs[0];

        int total = 0;
        foreach (var o in outs) total += Mathf.Max(0, o.weightPercent);
        if (total <= 0) return outs[0];

        int roll = Random.Range(0, total);
        int cum = 0;
        foreach (var o in outs)
        {
            cum += Mathf.Max(0, o.weightPercent);
            if (roll < cum) return o;
        }
        return outs[outs.Count - 1];
    }

    // ── 효과 적용 ──────────────────────────────────────────────
    private static void ApplyEffect(EventEffect eff)
    {
        if (eff == null) return;
        switch (eff.type)
        {
            case EventEffectType.None:
                break;

            case EventEffectType.SoulStone:
                if (eff.value > 0) { SoulstoneManager.Instance?.Add(eff.value); GameLog.Event($"영혼석 +{eff.value}", LogCategory.Reward); }
                else if (eff.value < 0) { SoulstoneManager.Instance?.Use(-eff.value); GameLog.Event($"영혼석 {eff.value}", LogCategory.Reward); }
                break;

            case EventEffectType.Stress:
                ApplyStress(eff.value, eff.target);
                break;

            case EventEffectType.Hp:
                ApplyHp(eff.value, eff.target);
                break;

            // ── 미결/별도 연동 필요 — 로그만 남긴다 (TODO) ──
            case EventEffectType.RecruitRandom:
                GameLog.Event("[TODO] 동료 합류 효과 — 용병소 모집 로직 연동 예정.", LogCategory.Reward);
                Debug.Log($"[EventService] TODO RecruitRandom (성급 {eff.value}) — PartyManager 연동 필요.");
                break;

            case EventEffectType.NextBattleStack:
                GameLog.Event($"[TODO] 다음 전투 스택 선지급 +{eff.value} — 전투 초기 스택 연동 예정.", LogCategory.Status);
                Debug.Log($"[EventService] TODO NextBattleStack (+{eff.value}).");
                break;

            case EventEffectType.RerollAffinity:
                GameLog.Event("[TODO] 성향 재굴림 — 대상 선택 UI + Affinity 재설정 연동 예정.", LogCategory.Status);
                Debug.Log("[EventService] TODO RerollAffinity — 대상 선택 필요.");
                break;

            case EventEffectType.ObtainObject:
                GameLog.Event($"[TODO] 오브제 획득 (OBJ-{eff.value:00}) — §09 오브제 시스템 연동 예정.", LogCategory.Reward);
                Debug.Log($"[EventService] TODO ObtainObject id={eff.value}.");
                break;

            case EventEffectType.Corruption:
                GameLog.Event($"[TODO] 오염도 {(eff.value >= 0 ? "+" : "")}{eff.value} — §10 오염도 시스템(백로그).", LogCategory.Status);
                Debug.Log($"[EventService] TODO Corruption {eff.value}.");
                break;

            case EventEffectType.NarrativeHint:
                GameLog.Event("암시 텍스트를 얻었다.", LogCategory.Default);
                break;
        }
    }

    // ── 파티 대상 스트레스 (+면 증가 / -면 감소) ─────────────────
    private static void ApplyStress(int delta, EventTarget target)
    {
        if (delta == 0) return;
        var targets = ResolveTargets(target);
        if (targets.Count == 0) return;
        foreach (var f in targets) f.currentStress += delta;
        string sign = delta >= 0 ? "+" : "";
        GameLog.Event($"{targets.Count}명 스트레스 {sign}{delta}", LogCategory.Status);
    }

    // ── 파티 대상 HP (+면 회복 / -면 피해) ──────────────────────
    private static void ApplyHp(int delta, EventTarget target)
    {
        if (delta == 0) return;
        var targets = ResolveTargets(target);
        if (targets.Count == 0) return;
        foreach (var f in targets) f.CurrentHp += delta;
        string sign = delta >= 0 ? "+" : "";
        GameLog.Event($"{targets.Count}명 HP {sign}{delta}", delta >= 0 ? LogCategory.Heal : LogCategory.Damage);
    }

    /// <summary>효과 대상 동료 목록. ChosenOne 은 (선택 UI 미구현) RandomOne 으로 폴백.</summary>
    private static List<FellowData> ResolveTargets(EventTarget target)
    {
        var alive = PartyManager.Instance?.GetActiveFellows()
                        .Where(f => f != null && !f.isDead).ToList()
                    ?? new List<FellowData>();
        if (alive.Count == 0) return alive;

        switch (target)
        {
            case EventTarget.All:
                return alive;

            case EventTarget.RandomOne:
            case EventTarget.ChosenOne: // 대상 선택 UI 미구현 → 랜덤 1명 폴백
                return new List<FellowData> { alive[Random.Range(0, alive.Count)] };

            case EventTarget.LowestHp:
                return new List<FellowData> { alive.OrderBy(f => f.CurrentHp).First() };

            default: // None
                return new List<FellowData>();
        }
    }
}
