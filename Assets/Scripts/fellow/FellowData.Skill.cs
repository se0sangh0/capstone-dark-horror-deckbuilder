// FellowData.Skill.cs
// FellowData (partial) — 스킬 배정 및 조회 담당.
//
// ── 역할 ────────────────────────────────────────────────────────
//   FellowData.cs 에서 스킬 관련 코드를 분리한 파일입니다.
//   partial class 이므로 FellowData 와 동일한 타입으로 컴파일됩니다.
//
// ── 배정된 스킬 확인 방법 (Inspector) ──────────────────────────
//   플레이 중 Hierarchy → BattleManager → Inspector
//   → Allies 리스트 → 각 FellowData 확장
//   → "배정된 스킬 정보" 섹션에서 스킬명/설명 확인 가능
//
// ── 스킬 배정 흐름 ──────────────────────────────────────────────
//   BattleManager.InitBattle()
//   → SkillDatabase.AssignRandomSkills(역할, 2) 로 스킬 2개 선택
//   → companion.skillIds 에 저장
//   → fellow.RefreshSkillInfo() 로 Inspector 표시 갱신

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class FellowData
{
    // ── 배정된 스킬 정보 (Inspector 확인용) ──────────────────────
    // 전투 시작 시 RefreshSkillInfo() 가 자동으로 채워줍니다.
    // Hierarchy → BattleManager → Allies → FellowData 에서 확인하세요.
    [Header("배정된 스킬 정보 (플레이 중 Inspector 에서 확인)")]

    [Tooltip("배정된 스킬 ID 목록 (자동 채워짐)")]
    [SerializeField] private string[] _assignedSkillIds = new string[0];

    [Tooltip("배정된 스킬 요약 (자동 채워짐)\n[활성] = 전투에서 사용 중\n[비활성] = 주석 처리된 테스트용")]
    [SerializeField, TextArea(4, 10)]
    private string _skillSummary = "(전투 시작 전 — 스킬 미배정)";

    // ── 스킬 정보 갱신 ───────────────────────────────────────────
    /// <summary>
    /// 스킬 배정 후 Inspector 표시를 갱신한다.
    /// BattleManager.InitBattle() 에서 skillIds 설정 직후 호출됩니다.
    /// </summary>
    public void RefreshSkillInfo()
    {
        if (data == null || data.skillIds == null || data.skillIds.Length == 0)
        {
            _assignedSkillIds = System.Array.Empty<string>();
            _skillSummary     = "(배정된 스킬 없음)";
            return;
        }

        _assignedSkillIds = data.skillIds.ToArray();

        if (SkillDatabase.Instance == null)
        {
            _skillSummary = $"ID: {string.Join(", ", data.skillIds)}\n(SkillDatabase 없음 — 씬에 추가 필요)";
            return;
        }

        var lines = new List<string>();
        for (int i = 0; i < data.skillIds.Length; i++)
        {
            var skill = SkillDatabase.Instance.GetSkill(data.skillIds[i]);
            if (skill == null) { lines.Add($"  스킬{i + 1}: ID '{data.skillIds[i]}' 없음"); continue; }

            // 첫 번째 스킬만 [활성], 나머지는 [비활성-테스트용]
            string tag = (i == 0) ? "[활성]      " : "[비활성-테스트용]";
            lines.Add($"{tag} 스킬{i + 1}: {skill.displayName}");
            lines.Add($"   효과: {skill.effectType}  대상: {skill.targeting}  파워: {skill.power}");
            lines.Add($"   설명: {skill.description}");
        }

        _skillSummary = string.Join("\n", lines);

        string name = data?.displayName ?? positionStack.ToString();
        Debug.Log($"[FellowData] {name} 스킬 배정 확인:\n{_skillSummary}");
    }

    // ── 스킬 조회 ────────────────────────────────────────────────
    /// <summary>
    /// 이 동료가 보유한 스킬 데이터 목록을 반환한다.
    /// SkillDatabase 가 없거나 skillIds 가 비어있으면 빈 목록 반환.
    /// </summary>
    public List<SkillData> GetSkills()
    {
        var result = new List<SkillData>();
        if (data == null || data.skillIds == null || data.skillIds.Length == 0) return result;

        if (SkillDatabase.Instance == null)
        {
            Debug.LogWarning("[FellowData.Skill] SkillDatabase 없음 — 씬에 SkillDatabase 오브젝트를 추가하세요.");
            return result;
        }

        foreach (var id in data.skillIds)
        {
            var skill = SkillDatabase.Instance.GetSkill(id);
            if (skill != null) result.Add(skill);
        }
        return result;
    }
}
