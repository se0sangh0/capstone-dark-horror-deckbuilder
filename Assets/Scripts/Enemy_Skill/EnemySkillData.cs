// ============================================================
// Enemy_Skill/EnemySkillData.cs
// 적 스킬 1개의 JSON 데이터 그릇 (적 전용)
// ============================================================
//
// [왜 동료 SkillData 와 따로 만들었나요?]
//   동료 스킬(SkillData)은 "역할(Dealer/Tank/Support) 스택을 소비"하는 개념이지만,
//   적 스킬은 코스트 개념이 없고 "행동 패턴(가중치) + 타겟팅" 만 필요합니다.
//   필드 의미가 다른데 같은 클래스에 우겨 넣으면 헷갈리므로 분리했습니다.
//
// [어디에 쓰이나요?]
//   - Resources/Data/enemy_skills.json 의 각 항목 1개에 대응
//   - EnemySkillDatabase 가 로드해서 ID → 데이터 조회용으로 보관
//   - BattleManager.EnemyAction.cs 가 turn 마다 weight 가중치 랜덤으로 1개 선택
//
// [연결된 파일]
//   - Enemy/EnemySkillDatabase.cs : 이 데이터를 로드/조회
//   - Resources/Data/enemy_skills.json : 실제 데이터
//   - BattleManager.EnemyAction.cs : 턴마다 가중치 랜덤 선택 + 실행
// ============================================================

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySkillData
{
    // ── ID / 표시 ──────────────────────────────────────────
    /// <summary>고유 ID. 예: "enemy_skill_goblin_dagger"</summary>
    public string id;

    /// <summary>UI 표시명. 예: "단검 휘두르기"</summary>
    public string displayName;

    /// <summary>분류 힌트. "단일" / "광역" 등</summary>
    public string skillGroup;

    // ── 효과 ───────────────────────────────────────────────
    /// <summary>
    /// 타겟팅 종류. (적 → 아군 방향 기준)
    /// "FrontFirst"  : 살아있는 아군 중 배치 1번 (전열)
    /// "FrontTwo"    : 살아있는 아군 중 1·2번 동시 (보스 휘두르기)
    /// "BackLast"    : 살아있는 아군 중 마지막 (후열, 고블린 독침)
    /// "AllAllies"   : 살아있는 아군 전체
    /// "RandomAlly"  : 살아있는 아군 중 랜덤 1명
    /// </summary>
    public string targeting;

    /// <summary>
    /// "Damage" : 대상에게 데미지 (MVP 기본)
    /// "Summon" : summonEnemyId 의 적을 summonCount 만큼 enemies 리스트에 추가 (기획 §11 §3 까마귀)
    /// 추후 Debuff/DoT 확장 자리.
    /// </summary>
    public string effectType;

    /// <summary>기본 효과 수치 (각 대상에게 적용될 데미지). Summon 의 경우 사용 안 함.</summary>
    public int power;

    /// <summary>
    /// 원거리 스킬 여부. true 면 BattleCardSprites 가 dash 시퀀스 없이 제자리에서 공격 모션만 재생한다.
    /// 예) 고블린 독침, 약탈자 도끼 던지기, 보스 까마귀 부름/수확/순간이동.
    /// </summary>
    public bool isRanged;

    /// <summary>
    /// DoT(지속 피해) — 매 턴 끝 적용될 데미지. 0 이면 DoT 없음.
    /// 기획 §11 §독침: 즉시 power 데미지 + 다음 dotTurns 턴 동안 매 턴 dotPower 데미지.
    /// </summary>
    public int dotPower;

    /// <summary>DoT 지속 턴 수. 0 이면 DoT 미부착.</summary>
    public int dotTurns;

    // ── 소환 스킬 (effectType = "Summon" 일 때만 사용) ──────────
    /// <summary>소환할 적의 ID (enemies.json 의 id). 예: "enemy_crow_01"</summary>
    public string summonEnemyId;

    /// <summary>소환할 적의 마릿수. 0/음수면 1로 보정.</summary>
    public int summonCount;

    // ── 행동 패턴 ───────────────────────────────────────────
    /// <summary>
    /// 가중치 랜덤 선택용 무게.
    /// 같은 적이 보유한 스킬들의 weight 합 중 비율만큼 발동 확률.
    /// 예) 고블린: 단검(70) + 독침(30) → 단검 70% / 독침 30%
    /// </summary>
    public int weight;
    
    // ── 코스트 (기획 §선공 판정 — 적 스킬 코스트 합산용) ──
    public int costAmount;

    /// <summary>
    /// 쿨다운 (턴 수). 사용 직후 N 턴 동안 가중치 룰렛에서 제외된다.
    /// 0 = 쿨다운 없음 (매 턴 사용 가능). 예) 까마귀 부름 = 3.
    /// </summary>
    public int cooldownTurns;

    // ── 설명 ───────────────────────────────────────────────
    /// <summary>인게임/디버그 설명</summary>
    public string description;

    /// <summary>
    /// 적 스킬 전용 스프라이트 리소스 경로 (Resources 폴더 기준).
    /// 예: "Sprites/EnemySkills/enemy_skill_goblin_dagger". 비어있으면 기본 fallback 사용.
    /// </summary>
    public string spritePath;

    /// <summary>
    /// 런타임에 spritePath 기반으로 EnemySkillDatabase 가 채우는 실제 Sprite.
    /// JsonUtility 는 [NonSerialized] 를 무시하므로 JSON 직렬화에 영향 없음.
    /// 비어있으면 null — 호출 측에서 fallback 처리 필요.
    /// </summary>
    [System.NonSerialized] public Sprite sprite;
}

/// <summary>enemy_skills.json 최상위 래퍼: { "enemySkills": [ ... ] }</summary>
[System.Serializable]
public class EnemySkillDataCollection
{
    public List<EnemySkillData> enemySkills;
}
