// ============================================================
// Skill/SkillDefinition.cs
// 스킬 관련 열거형 정의 및 ScriptableObject 스킬 정의
// ============================================================
//
// [이 파일이 하는 일]
//   스킬 시스템에서 공통으로 사용하는 열거형(Enum)을 정의합니다:
//   - Targeting   : 스킬이 누구를 대상으로 하는지
//   - EffectType  : 스킬의 효과 종류 (데미지, 힐, 실드 등)
//   - StatusEffect: 부가 상태이상 종류
//
//   또한 에디터에서 직접 만들 수 있는 ScriptableObject 스킬 정의도 포함합니다.
//   (JSON 방식: Skill/SkillData.cs + Resources/Data/skills.json 참조)
//
// [두 가지 스킬 정의 방식]
//   1. ScriptableObject (이 파일의 SkillDefinition): 에디터에서 소수 스킬 작성 시 편리
//   2. JSON (SkillData.cs + skills.json): 스킬이 많아질 때 유리 (추천)
//
// [어디서 쓰이나요?]
//   - Skill/SkillData.cs : JSON 스킬 데이터에서 동일한 개념 사용
//   - Companion/CompanionData.cs : skillIds[] 로 JSON 스킬 참조
// ============================================================

using UnityEngine;

// ----------------------------------------------------------
// [Targeting 열거형]
// 스킬이 영향을 주는 대상 범위
// ----------------------------------------------------------
/// <summary>스킬 타겟팅 범위</summary>
public enum Targeting
{
    Self        = 0,  // 시전자 자신
    SingleEnemy = 1,  // 적 1명
    AllEnemies  = 2,  // 적 전체 (파워를 적 수로 나눔)
    SingleAlly  = 3,  // 아군 1명
    AllAllies   = 4,  // 아군 전체
}

// ----------------------------------------------------------
// [EffectType 열거형]
// 스킬 효과의 종류
// ----------------------------------------------------------
/// <summary>스킬 효과 유형</summary>
public enum EffectType
{
    Damage = 0,  // 데미지: 대상 HP 감소
    Heal   = 1,  // 힐: 대상 HP 회복
    Shield = 2,  // 실드: 피해 흡수
    Buff   = 3,  // 버프: 긍정 상태 부여
    Debuff = 4,  // 디버프: 부정 상태 부여
}

// ----------------------------------------------------------
// [StatusEffect 열거형]
// 스킬에 부가될 수 있는 상태이상 (확장용)
// ----------------------------------------------------------
/// <summary>스킬 부가 상태이상 종류 (확장용)</summary>
public enum StatusEffect
{
    None       = 0,  // 없음
    Bleed      = 1,  // 출혈: 매 턴 지속 데미지
    Taunt      = 2,  // 도발: 적이 이 유닛을 우선 공격
    StressHeal = 3,  // 스트레스 회복
}

// ----------------------------------------------------------
// [SkillDefinition ScriptableObject]
// 에디터에서 직접 스킬 에셋을 만들 때 사용
// 스킬이 많아지면 JSON 방식(SkillData.cs) 으로 전환을 권장
// ----------------------------------------------------------

/// <summary>
/// 동료 스킬 1개의 ScriptableObject 정의.
/// 에셋 생성: Assets 우클릭 → Create → DarkHorror/SkillDefinition
/// </summary>
[CreateAssetMenu(menuName = "DarkHorror/SkillDefinition", fileName = "skill_new")]
public class SkillDefinition : ScriptableObject
{
    [Header("ID / 표시")]
    [Tooltip("로직 연결용 ID. skills.json 의 id 와 동일하게 맞추는 것을 권장합니다.")]
    public string id;

    [Tooltip("UI 표시명. 예: 베기")]
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

    [Tooltip("기본 효과 수치 (데미지 / 힐 / 실드량)")]
    public int power;

    [Header("AI 힌트 (선택)")]
    [Tooltip("AI 행동 우선순위. 높을수록 먼저 선택. 기본 0")]
    public int aiPriority;

    [Header("상태이상 (선택 — 확장용)")]
    [Tooltip("부가 상태이상 유형. MVP에서는 None으로 둔다.")]
    public StatusEffect statusEffect;

    [Tooltip("상태이상 수치")]
    public int statusValue;

    [Header("설명 (선택)")]
    [TextArea(1, 3)]
    [Tooltip("인게임 스킬 설명 텍스트")]
    public string description;
}
