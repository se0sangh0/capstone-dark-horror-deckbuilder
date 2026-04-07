// CardData.cs
// 플레이어가 사용하는 카드 1장의 정의.
// 기획 기준: 기획/시스템/02_카드_설계_프레임.md
// 데이터 테이블: 기획/시스템/01_데이터_테이블_v0.md (CardDefinition 섹션)

using UnityEngine;

/// <summary>
/// 카드의 속성 구분.
/// 빌드 시 Dealer/Tanker/Support 3종.
/// </summary>
public enum CardRole
{
    Dealer  = 0,
    Tanker  = 1,
    Support = 2,
}

/// <summary>
/// 플레이어가 사용하는 카드 1장.
/// 설치: Assets 우클릭 → Create → DarkHorror/CardData
/// </summary>
[CreateAssetMenu(menuName = "DarkHorror/CardData", fileName = "card_new")]
public class CardData : ScriptableObject
{
    [Header("ID / 표시")]
    [Tooltip("로직 연결용 ID. 소문자+특수문자 금지. 예: card_dealer_plus2")]
    public string id;

    [Tooltip("UI에 표시될 카드 이름")]
    public string displayName;

    [Header("속성 / 성향")]
    [Tooltip("이 카드가 기여하는 스택의 종류")]
    public CardRole role;

    [Tooltip("동료의 성향. 스택 기여량의 생성 범위를 결정한다.")]
    public CardAffinity affinity;

    [Header("스택 기여량")]
    [Tooltip("스택 기여량. 사용 시 AffinityHelper.GenerateStack()으로 런타임 확정.")]
    [HideInInspector]
    public int stackDelta; // 런타임 생성 추적용 (Inspector 미노출)

    [Header("시각")]
    public Sprite icon;

    [Header("메모")]
    [TextArea(2, 4)]
    public string note;

    // -------------------------------------------------------
    // 런타임 메서드
    // -------------------------------------------------------

    /// <summary>
    /// 성향 규칙에 따라 스택 기여량을 생성하고 반환한다.
    /// 카드 사용 시점에 한 번만 호출한다.
    /// </summary>
    public int GenerateStack()
    {
        stackDelta = AffinityHelper.GenerateStack(affinity);
        return stackDelta;
    }

    /// <summary>카드 전하면에 표시할 성향 한글 이름을 반환한다.</summary>
    public string AffinityLabel => AffinityHelper.GetLabel(affinity);

    /// <summary>UI 카드 틀팅 색상을 반환한다.</summary>
    public UnityEngine.Color AffinityColor => AffinityHelper.GetColor(affinity);
}
