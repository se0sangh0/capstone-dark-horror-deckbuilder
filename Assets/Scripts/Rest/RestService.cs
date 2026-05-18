// ============================================================
// Rest/RestService.cs
// 화툿불(휴식) 노드 — HP/스트레스 회복 로직
// ============================================================
//
// [기획 참조]
//   §04_스트레스_디버프_표 §기본 회복 — "화툿불/휴식 노드: -15"
//   §02_MVP_노드_설계 §화툿불 — "체력/스트레스 회복, 다음 전투 전 정비"
//
// [수치 정책]
//   기획에 -15 단일 수치만 명시되어 HP/스트레스 동일 적용.
//   (사용자 결정 Q1·a — 회복량 동일)
//
// [사용처]
//   RestPanel.OnOpened() 에서 자동 호출 (Q2·a — 자동 회복).
// ============================================================

using System.Linq;
using UnityEngine;

public static class RestService
{
    /// <summary>화툿불 회복량 (HP +N / 스트레스 -N). 기획 §04 §기본 회복.</summary>
    public const int RecoveryAmount = 15;

    /// <summary>회복 결과 통계 — UI 표시용.</summary>
    public struct RecoveryResult
    {
        public int affectedCount;   // 적용받은 살아있는 동료 수
        public int totalHpRecovered;
        public int totalStressRelieved;
    }

    /// <summary>
    /// 살아있는 파티원 전체에게 HP +RecoveryAmount / 스트레스 -RecoveryAmount 적용.
    /// HP 는 maxHp 까지 clamp, 스트레스는 0 까지 clamp.
    /// </summary>
    public static RecoveryResult ApplyRecovery()
    {
        var result = new RecoveryResult();
        if (PartyManager.Instance == null) return result;

        var fellows = PartyManager.Instance.GetActiveFellows()
            .Where(f => f != null && !f.isDead)
            .ToList();

        foreach (var f in fellows)
        {
            // HP 회복 — CurrentHp setter 가 OnHpChanged 발생 → 슬라이더 자동 갱신
            int maxHp     = f.maxHp > 0 ? f.maxHp : 100;
            int beforeHp  = f.CurrentHp;
            f.CurrentHp   = Mathf.Min(maxHp, beforeHp + RecoveryAmount);
            int hpGained  = f.CurrentHp - beforeHp;

            // 스트레스 회복
            int beforeStress = f.currentStress;
            f.currentStress  = Mathf.Max(0, beforeStress - RecoveryAmount);
            int stressRelieved = beforeStress - f.currentStress;

            result.affectedCount++;
            result.totalHpRecovered    += hpGained;
            result.totalStressRelieved += stressRelieved;

            Debug.Log($"[Rest] {(!string.IsNullOrEmpty(f.displayName) ? f.displayName : f.positionStack.ToString())} — HP {beforeHp}→{f.CurrentHp} (+{hpGained}), 스트레스 {beforeStress}→{f.currentStress} (-{stressRelieved})");
        }

        Debug.Log($"[Rest] 회복 완료 — {result.affectedCount}명, 총 HP+{result.totalHpRecovered} / 스트레스-{result.totalStressRelieved}");
        return result;
    }
}
