// FellowData.Skill.cs
// FellowData (partial) — 스킬 배정 및 조회 담당.
//
// ── 역할 ────────────────────────────────────────────────────────
//   FellowData.cs 에서 스킬 관련 코드를 분리한 파일입니다.
//   partial class 이므로 FellowData 와 동일한 타입으로 컴파일됩니다.
//
// ── 스킬 영속 구조 ──────────────────────────────────────────────
//   _persistedSkillIds : 전투 간 유지되는 스킬 ID 배열.
//     - 최초 전투 시작 시 AssignSkills() 로 배정됩니다.
//     - 이후 전투에서는 기존 스킬을 그대로 재사용합니다.
//     - 사망 시 ClearSkills() 를 호출하여 초기화합니다.
//
// ── 스킬 배정 흐름 ──────────────────────────────────────────────
//   BattleManager.InitBattle()
//   → fellow.HasSkills 가 false 인 경우에만
//   → SkillDatabase.AssignRandomSkills() 로 스킬 2개 선택
//   → fellow.AssignSkills(ids) 로 저장 + Inspector 갱신
//
// ── 사망 시 흐름 ────────────────────────────────────────────────
//   BattleManager.ProcessDeathAndStress()
//   → PartyManager.RemoveFellow(ally)
//   → fellow.ClearSkills() 호출 → 스킬 초기화

using System.Collections.Generic;
using UnityEngine;

public partial class FellowData
{
    // ── 영속 스킬 ID (전투 간 유지) ──────────────────────────────
    [Header("스킬 (Skills)")]
    [Tooltip("배정된 스킬 ID 목록. 전투 간 유지되며 사망 시 초기화됩니다.")]
    [SerializeField] private string[] _persistedSkillIds = new string[0];

    // ── Inspector 표시용 요약 ────────────────────────────────────
    [Tooltip("배정된 스킬 요약 (자동 갱신 — 직접 수정 불가)")]
    [SerializeField, TextArea(4, 10)]
    private string _skillSummary = "(스킬 미배정)";

    // ── 공개 프로퍼티 ────────────────────────────────────────────
    /// <summary>스킬이 하나 이상 배정되어 있는지 여부</summary>
    public bool HasSkills => _persistedSkillIds != null && _persistedSkillIds.Length > 0;

    // ── 스킬 배정 ────────────────────────────────────────────────
    /// <summary>
    /// 스킬 ID 배열을 배정하고 Inspector 표시를 갱신한다.
    /// BattleManager.InitBattle() 에서 HasSkills == false 일 때만 호출된다.
    /// </summary>
    public void AssignSkills(string[] ids)
    {
        _persistedSkillIds = ids ?? new string[0];
        RefreshSkillInfo();
    }

    // ── 스킬 초기화 (사망 시) ────────────────────────────────────
    /// <summary>
    /// 동료 사망 시 스킬을 초기화한다.
    /// PartyManager.RemoveFellow() 에서 호출된다.
    /// </summary>
    public void ClearSkills()
    {
        _persistedSkillIds = new string[0];
        _skillSummary      = "(사망 — 스킬 초기화됨)";
        Debug.Log($"[FellowData] {data?.displayName ?? positionStack.ToString()} 스킬 초기화 (사망)");
    }

    // ── Inspector 표시 갱신 ──────────────────────────────────────
    /// <summary>
    /// 현재 _persistedSkillIds 를 기반으로 Inspector 요약 표시를 갱신한다.
    /// AssignSkills() 또는 BattleManager.InitBattle() 에서 호출된다.
    /// </summary>
    public void RefreshSkillInfo()
    {
        if (!HasSkills)
        {
            _skillSummary = "(배정된 스킬 없음)";
            return;
        }

        if (SkillDatabase.Instance == null)
        {
            _skillSummary = $"ID: {string.Join(", ", _persistedSkillIds)}\n(SkillDatabase 없음 — 씬에 추가 필요)";
            return;
        }

        var lines = new List<string>();
        for (int i = 0; i < _persistedSkillIds.Length; i++)
        {
            var skill = SkillDatabase.Instance.GetSkill(_persistedSkillIds[i]);
            if (skill == null) { lines.Add($"  스킬{i + 1}: ID '{_persistedSkillIds[i]}' 없음"); continue; }

            string tag = (i == 0) ? "[활성]         " : "[비활성-테스트용]";
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
    /// 배정된 스킬 데이터 목록을 반환한다.
    /// HasSkills == false 이거나 SkillDatabase 가 없으면 빈 목록 반환.
    /// </summary>
    public List<SkillData> GetSkills()
    {
        var result = new List<SkillData>();
        if (!HasSkills) return result;

        if (SkillDatabase.Instance == null)
        {
            Debug.LogWarning("[FellowData.Skill] SkillDatabase 없음 — 씬에 SkillDatabase 오브젝트를 추가하세요.");
            return result;
        }

        foreach (var id in _persistedSkillIds)
        {
            var skill = SkillDatabase.Instance.GetSkill(id);
            if (skill != null) result.Add(skill);
        }
        return result;
    }
}
