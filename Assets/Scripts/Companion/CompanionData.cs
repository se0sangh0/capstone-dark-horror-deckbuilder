// CompanionData.cs
// 동료 1명의 상시 설정치.
// 기획 기준: 기획/시스템/03_동료_설계_프레임.md
// 데이터 테이블: 기획/시스템/01_데이터_테이블_v0.md (AllyDefinition 섹션)

using UnityEngine;

/// <summary>
/// 동료의 역할군.
/// 카드의 CardRole과 일치하여 스택 속성을 결정한다.
/// </summary>
public enum CompanionRole
{
    Dealer  = 0,
    Tanker  = 1,
    Support = 2,
}

/// <summary>
/// 동료 1명의 ScriptableObject 정의.
/// 설치: Assets 우클릭 → Create → DarkHorror/CompanionData
/// </summary>
[CreateAssetMenu(menuName = "DarkHorror/CompanionData", fileName = "companion_new")]
public class CompanionData : ScriptableObject
{
    [Header("ID / 표시")]
    [Tooltip("로직 연결용 ID. 예: ally_dealer_01")]
    public string id;

    [Tooltip("UI 표시명")]
    public string displayName;

    [Header("역할 / 성향")]
    [Tooltip("동료 역할군. 덱 자동 등록 시 이 역할의 카드가 덧붙운다.")]
    public CompanionRole role;

    [Tooltip("동료 성향. 사용 카드의 스택 범위를 결정한다.")]
    public CardAffinity affinity;

    [Header("스택즈 / HP")]
    [Tooltip("최대 HP")]
    public int maxHp = 30;

    [Tooltip("스택 요구량 (MVP: 이 수치 이상이면 행동 가능)")]
    public int requiredStack = 3;

    [Header("스킬")]
    [Tooltip("스킬 1 (SkillDefinition SO 참조)")]
    public SkillDefinition skill1;

    // [Tooltip("스킬 2 (SkillDefinition SO 참조) — 테스트 후 활성화")]
    // public SkillDefinition skill2;

    [Header("시각")]
    public Sprite portrait;

    [Header("메모")]
    [TextArea(2, 4)]
    public string note;
    
    // CompanionData.cs에 추가
    [Header("캐릭터 이미지")]
    public string spritePath; // Resources 폴더 기준 경로

    // -------------------------------------------------------
    // 런타임 프로퍼티
    // -------------------------------------------------------

    /// <summary>성향 한글 표시 이름. UI 동료 슬롯에 표시할 때 사용.</summary>
    public string AffinityLabel => AffinityHelper.GetLabel(affinity);

    /// <summary>성향별 UI 색상. 동료 슬롯 테두리 색상에 적용.</summary>
    public UnityEngine.Color AffinityColor => AffinityHelper.GetColor(affinity);

    /// <summary>스택이 충분한지 확인한다. (행동 가능 여부)</summary>
    public bool CanAct(int currentStack) => currentStack >= requiredStack;
}
