// FellowData.cs
// 동료 런타임 상태 ScriptableObject — 기본 정의.
//
// ── 분리된 partial 파일 ─────────────────────────────────────────
//   FellowData.Hp.cs    : HP 시스템 (CurrentHp, InitHp, OnHpChanged, OnDied)
//   FellowData.Skill.cs : 스킬 시스템 (RefreshSkillInfo, GetSkills)

using UnityEngine;

[CreateAssetMenu(menuName = "DarkHorror/FellowData", fileName = "fellow_new")]
public partial class FellowData : ScriptableObject
{
    // ── 정의 데이터 ──────────────────────────────────────────────
    [Header("정의 데이터")]
    [Tooltip("이 동료의 CompanionData SO. 스킬/역할/스탯 정의를 참조한다.")]
    public CompanionData data;

    // ── 런타임 상태 ──────────────────────────────────────────────
    [Header("런타임 상태")]
    public StackType positionStack;
    public Sprite    fellowSprite;
    public int       currentStress = 0;
    //public int       currentStack  = 0;
    public bool      isDead        = false;
}
