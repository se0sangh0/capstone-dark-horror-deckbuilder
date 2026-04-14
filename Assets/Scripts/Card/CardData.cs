// CardData.cs
// 카드 1장의 ScriptableObject 정의.
// SO 스키마: 기획/시스템/01_데이터_테이블_SO_스키마.md — CardDefinition 섹션

using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 카드 1장의 ScriptableObject 정의 (CardDefinition v0).
/// 설치: Assets 우클릭 → Create → DarkHorror/CardDefinition
/// </summary>
[CreateAssetMenu(menuName = "DarkHorror/CardDefinition", fileName = "card_new")]
public class CardData : ScriptableObject
{
    [Header("ID / 표시")]
    [Tooltip("로직 연결용 ID. 예: card_dealer_plus2  (대문자/공백 금지)")]
    public string id;

    [Tooltip("UI 표시명. 예: 딜러 +2")]
    public string displayName;

    [Header("스택")]
    [Tooltip("카드가 기여하는 스택 유형 (Dealer / Tank / Support)")]
    public StackType stackType;

    [Tooltip("스택 기여량. 양수=증가, 음수=감소. 예: +2, -1")]
    [FormerlySerializedAs("cardPower")]   // 기존 SO 에셋 직렬화 호환
    public int stackDelta;

    [Header("성향")]
    [Tooltip("카드를 뽑는 동료의 성향. 드로우 시 stackDelta 범위 결정에 참조.")]
    public CardAffinity affinity;

    [Header("에셋 (선택)")]
    [Tooltip("카드 아트 스프라이트")]
    public Sprite cardArt;

    [TextArea(1, 3)]
    [Tooltip("디자인 메모 (선택)")]
    public string note;
}
