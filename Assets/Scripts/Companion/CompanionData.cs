// ============================================================
// Companion/CompanionData.cs
// 동료 1명의 ScriptableObject 정의 데이터
// ============================================================
//
// [이 파일이 하는 일]
//   동료 1명의 "변하지 않는 기본 정보" 를 저장합니다.
//   (이름, 역할, 성향, HP, 스택 요구량, 스킬 목록 등)
//
//   런타임 상태(현재 HP, 스트레스 등)는 FellowData.cs 에서 관리합니다.
//
// [스킬 시스템 변경]
//   이전: skill1 (SkillDefinition SO 직접 참조)
//   현재: skillIds[] (JSON 스킬 DB 에서 ID 로 조회)
//   → SkillDatabase.Instance.GetSkill(id) 로 런타임에 조회합니다.
//
// [에셋 생성 방법]
//   Assets 우클릭 → Create → DarkHorror/CompanionData
//
// [어디서 쓰이나요?]
//   - PartyManager.cs : 파티 동료 목록 관리
//   - BattleManager.cs : 전투 시작 시 FellowData 생성에 사용
//   - DeckBuilder.cs : 덱 구성 시 역할/성향 참조
//   - FellowData.cs : data 필드로 참조
// ============================================================

using UnityEngine;

// ----------------------------------------------------------
// [CompanionRole 열거형]
// 동료의 역할군을 나타냅니다.
// StackType 열거형과 값이 일치하여 스택 속성을 결정합니다.
// ----------------------------------------------------------
/// <summary>동료 역할군. StackType 과 인덱스가 일치한다.</summary>
public enum CompanionRole
{
    Dealer  = 0,  // 딜러: 공격 담당
    Tanker  = 1,  // 탱커: 방어 담당
    Support = 2,  // 서포터: 치유/지원 담당
}

/// <summary>
/// 동료 1명의 ScriptableObject 정의.
/// 에셋 생성: Assets 우클릭 → Create → DarkHorror/CompanionData
/// </summary>
[CreateAssetMenu(menuName = "DarkHorror/CompanionData", fileName = "companion_new")]
public class CompanionData : ScriptableObject
{
    // ----------------------------------------------------------
    // [ID / 표시]
    // ----------------------------------------------------------
    [Header("ID / 표시")]
    [Tooltip("로직 연결용 고유 ID. 예: ally_dealer_01  (대문자/공백 금지)")]
    public string id;

    [Tooltip("UI 에 표시할 이름. 예: 딜러 아리")]
    public string displayName;

    // ----------------------------------------------------------
    // [역할 / 성향]
    // ----------------------------------------------------------
    [Header("역할 / 성향")]
    [Tooltip("동료의 역할군. 덱 구성 시 이 역할에 맞는 카드가 포함됩니다.")]
    public CompanionRole role;

    [Tooltip("동료의 성향. 카드 스택 범위를 결정합니다.")]
    public CardAffinity affinity;

    // ----------------------------------------------------------
    // [스탯]
    // ----------------------------------------------------------
    [Header("스탯 (Stats)")]
    [Tooltip("최대 HP")]
    public int maxHp = 30;

    [Tooltip("행동에 필요한 최소 스택 (이 수치 이상이면 행동 가능)")]
    public int requiredStack = 3;

    // ----------------------------------------------------------
    // [스킬 목록]
    // skills.json 의 스킬 ID 를 입력하세요.
    // 런타임에 SkillDatabase.Instance.GetSkill(id) 로 조회됩니다.
    // ----------------------------------------------------------
    [Header("스킬 (Skills)")]
    [Tooltip("보유 스킬 ID 목록. Resources/Data/skills.json 의 id 값을 입력하세요.")]
    public string[] skillIds = new string[0];

    // ----------------------------------------------------------
    // [시각]
    // ----------------------------------------------------------
    [Header("시각 (Visual)")]
    [Tooltip("초상화 스프라이트")]
    public Sprite portrait;

    [Header("캐릭터 이미지")]
    [Tooltip("Resources 폴더 기준 스프라이트 경로. 예: Characters/ally_dealer_01")]
    public string spritePath;

    // ----------------------------------------------------------
    // [메모]
    // ----------------------------------------------------------
    [Header("메모 (Notes)")]
    [TextArea(2, 4)]
    [Tooltip("디자인 메모")]
    public string note;

    // ----------------------------------------------------------
    // 런타임 프로퍼티
    // ----------------------------------------------------------

    /// <summary>성향 한글 표시 이름. UI 동료 슬롯에 표시할 때 사용.</summary>
    public string AffinityLabel => AffinityHelper.GetLabel(affinity);

    /// <summary>성향별 UI 색상. 동료 슬롯 테두리 색상에 적용.</summary>
    public UnityEngine.Color AffinityColor => AffinityHelper.GetColor(affinity);

    /// <summary>현재 스택이 행동 가능 스택 이상인지 확인한다.</summary>
    public bool CanAct(int currentStack) => currentStack >= requiredStack;
}
