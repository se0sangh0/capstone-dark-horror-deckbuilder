// ============================================================
// Skill/SkillData.cs
// JSON 스킬 데이터 직렬화 클래스
// ============================================================
//
// [이 파일이 하는 일]
//   스킬 정보를 JSON 파일에서 읽어올 때 사용하는 "데이터 그릇" 입니다.
//   Resources/Data/skills.json 파일의 내용을 이 클래스에 담습니다.
//
// [왜 JSON을 쓰나요?]
//   나중에 스킬이 100개, 200개로 늘어나도 JSON 파일만 수정하면 되어서
//   훨씬 편하게 관리할 수 있습니다. 스크립트를 고칠 필요가 없어요!
//
// [연결된 파일]
//   - Skill/SkillDatabase.cs : 이 데이터를 로드하고 관리하는 매니저
//   - Resources/Data/skills.json : 실제 스킬 데이터가 담긴 JSON 파일
//   - Fellow/FellowData.cs : skillIds[] 로 스킬 ID 참조 + GetSkills() 로 런타임 조회
// ============================================================

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스킬 1개의 JSON 데이터 클래스.
/// skills.json 의 각 항목이 이 클래스 1개에 대응됩니다.
/// </summary>
[System.Serializable]
public class SkillData
{
    // ----------------------------------------------------------
    // [ID / 표시]
    // ----------------------------------------------------------

    /// <summary>스킬 고유 ID. 예: "skill_slash_01"</summary>
    public string id;

    /// <summary>UI 에 표시될 스킬 이름. 예: "베기"</summary>
    public string displayName;

    /// <summary>스킬 분류 힌트. 예: "단일", "범위"</summary>
    public string skillGroup;

    // ----------------------------------------------------------
    // [코스트]
    // ----------------------------------------------------------

    /// <summary>
    /// 소모할 스택 유형 문자열. "Dealer", "Tank", "Support" 중 하나.
    /// 사용 시 StackType 으로 파싱됩니다.
    /// </summary>
    public string costType;

    /// <summary>소모 스택 수치. 예: 3</summary>
    public int costAmount;

    // ----------------------------------------------------------
    // [효과]
    // ----------------------------------------------------------

    /// <summary>
    /// 타겟팅 범위 문자열.
    /// "Self", "SingleEnemy", "AllEnemies", "SingleAlly", "AllAllies"
    /// </summary>
    public string targeting;

    /// <summary>
    /// 스킬 효과 유형 문자열.
    /// "Damage", "Heal", "Shield", "Buff", "Debuff"
    /// </summary>
    public string effectType;

    /// <summary>기본 효과 수치. 예: 데미지 15, 힐 10. Mixed 타입에서는 주 효과(데미지)의 수치.</summary>
    public int power;

    /// <summary>
    /// 원거리 스킬 여부 (모션 포지션). true=Ranged(제자리 발사), false=Melee(전진 타격).
    /// Damage 계열에만 의미 있음. Heal/Shield/Mixed 는 effectType 기준 Stationary 처리되므로 무관.
    /// MotionCategoryResolver 가 이 값을 최우선으로 사용. 미지정(JSON 없음) 시 false.
    /// </summary>
    public bool isRanged;

    /// <summary>
    /// 보조 실드 수치 — effectType="MixedDamageShield" 일 때만 사용 (전장의 방패).
    /// AllEnemies 데미지 power 적용 후, AllAllies 에 이 만큼 실드 부여.
    /// </summary>
    public int shieldPower;

    /// <summary>
    /// 도발 지속 턴 — effectType="MixedDamageTaunt" 일 때만 사용 (워크라이).
    /// 적이 이 만큼 턴 동안 시전자를 우선 타격. SingleEnemy 류 타겟팅에만 영향, AllAllies 류는 그대로.
    /// </summary>
    public int tauntTurns;

    // ----------------------------------------------------------
    // [AI 힌트]
    // ----------------------------------------------------------

    /// <summary>AI 우선순위. 높을수록 먼저 사용함. 기본값: 0</summary>
    public int aiPriority;

    // ----------------------------------------------------------
    // [상태이상 (확장용)]
    // ----------------------------------------------------------

    /// <summary>
    /// 부가 상태이상 문자열.
    /// "None", "Bleed", "Taunt", "StressHeal"
    /// </summary>
    public string statusEffect;

    /// <summary>상태이상 수치. 예: Bleed → 매 턴 피해량</summary>
    public int statusValue;

    // ----------------------------------------------------------
    // [설명]
    // ----------------------------------------------------------

    /// <summary>인게임 스킬 설명 텍스트</summary>
    public string description;

    /// <summary>
    /// 스킬 전용 스프라이트 리소스 경로 (Resources 폴더 기준).
    /// 예: "Sprites/Skills/skill_fireball". 비어있으면 기본 fallback 사용.
    /// </summary>
    public string spritePath;

    /// <summary>
    /// 런타임에 spritePath 를 기반으로 SkillDatabase 가 채우는 실제 Sprite.
    /// JsonUtility 는 [NonSerialized] 필드를 무시하므로 JSON 직렬화에 영향 없음.
    /// 비어있으면 null — 호출 측에서 fallback 처리 필요.
    /// </summary>
    [System.NonSerialized] public Sprite sprite;
}

/// <summary>
/// JSON 파일 전체를 담는 래퍼 클래스.
/// skills.json 최상위 구조: { "skills": [ ... ] }
/// </summary>
[System.Serializable]
public class SkillDataCollection
{
    /// <summary>모든 스킬 데이터 목록</summary>
    public List<SkillData> skills;
}
