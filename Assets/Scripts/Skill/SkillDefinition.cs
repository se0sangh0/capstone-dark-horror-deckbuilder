// SkillDefinition.cs
// 스킬 1개의 ScriptableObject 정의 및 관련 열거형.
// SO 스키마: 기획/시스템/10_동료_스킬_데이터.md — SkillDefinition 섹션

using UnityEngine;

// ──────────────────────────────────────────
// 스킬 관련 열거형
// ──────────────────────────────────────────

/// <summary>
/// 스킬 타겟팅 범위.
/// </summary>
public enum Targeting
{
    Self        = 0,  // 시전자 자신
    SingleEnemy = 1,  // 적 1명
    AllEnemies  = 2,  // 적 전체 (power / 적 수 분산)
    SingleAlly  = 3,  // 아군 1명
    AllAllies   = 4,  // 아군 전체
}

/// <summary>
/// 스킬 효과 유형.
/// </summary>
public enum EffectType
{
    Damage = 0,
    Heal   = 1,
    Shield = 2,
    Buff   = 3,
    Debuff = 4,
}

/// <summary>
/// 스킬에 부가될 수 있는 상태이상 (선택 — 확장용).
/// </summary>
public enum StatusEffect
{
    None       = 0,
    Bleed      = 1,  // 출혈: 매 턴 DoT
    Taunt      = 2,  // 도발: 적이 이 유닛을 우선 공격
    StressHeal = 3,  // 스트레스 회복
}

// ──────────────────────────────────────────
// SkillDefinition (ScriptableObject)
// ──────────────────────────────────────────

/// <summary>
/// 동료 스킬 1개의 ScriptableObject 정의 (SkillDefinition v0).
/// 설치: Assets 우클릭 → Create → DarkHorror/SkillDefinition
/// </summary>
[CreateAssetMenu(menuName = "DarkHorror/SkillDefinition", fileName = "skill_new")]
public class SkillDefinition : ScriptableObject
{
    [Header("ID / 표시")]
    [Tooltip("로직 연결용 ID. 예: skill_magic_missile  (대문자/공백 금지)")]
    public string id;

    [Tooltip("UI 표시명. 예: 매직 미사일")]
    public string displayName;

    [Tooltip("스킬 분류 힌트. 예: 단일 / 범위")]
    public string skillGroup;

    [Header("코스트")]
    [Tooltip("소모할 스택 유형 (Dealer / Tank / Support)")]
    public StackType costType;

    [Tooltip("소모 스택 수치")]
    public int costAmount;

    [Header("효과")]
    [Tooltip("스킬 타겟팅 범위")]
    public Targeting targeting;

    [Tooltip("스킬 효과 유형")]
    public EffectType effectType;

    [Tooltip("기본 효과 수치 (데미지 / 힐 / 실드량). 코스트별 기준: 기획/시스템/10_동료_스킬_데이터.md 참조")]
    public int power;

    [Header("AI 힌트 (선택)")]
    [Tooltip("AI 행동 우선순위. 높을수록 먼저 선택. 기본 0")]
    public int aiPriority;

    [Header("상태이상 (선택 — 확장용)")]
    [Tooltip("부가 상태이상 유형. MVP에서는 None으로 둔다.")]
    public StatusEffect statusEffect;

    [Tooltip("상태이상 수치 (예: Bleed 5 → 매 턴 5 피해). statusEffect == None이면 무시.")]
    public int statusValue;

    [Header("설명 (선택)")]
    [TextArea(1, 3)]
    [Tooltip("인게임 스킬 설명 텍스트")]
    public string description;
}
