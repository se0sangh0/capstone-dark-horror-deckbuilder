// ============================================================
// Card/CardData.cs
// 카드 1장의 ScriptableObject 정의
// ============================================================
//
// [이 파일이 하는 일]
//   게임에서 사용하는 카드 1장의 "기본 정보"를 저장하는
//   ScriptableObject 에셋입니다.
//   카드가 어떤 역할(딜러/탱커/서포터)에 속하고
//   스택에 얼마나 기여하는지를 정의합니다.
//
// [에셋 생성 방법]
//   Assets 우클릭 → Create → DarkHorror/CardDefinition
//
// [어디서 쓰이나요?]
//   - Card/DeckBuilder.cs : 파티 덱 구성 시 카드 필터링에 사용
//   - GameManager.cs : 드로우 덱에 저장됨 (card, owner) 쌍
//   - StackCardController.cs : 카드 UI 세팅 시 stackType, stackDelta 참조
//   - BattleManager.cs : GenerateCardPool() 에서 런타임 생성
// ============================================================

using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 카드 1장의 ScriptableObject 정의 (CardDefinition v0).
/// 에셋 생성: Assets 우클릭 → Create → DarkHorror/CardDefinition
/// </summary>
[CreateAssetMenu(menuName = "DarkHorror/CardDefinition", fileName = "card_new")]
public class CardData : ScriptableObject
{
    // ----------------------------------------------------------
    // [ID / 표시]
    // ----------------------------------------------------------
    [Header("ID / 표시")]
    [Tooltip("로직 연결용 고유 ID. 예: card_dealer_plus2  (대문자/공백 금지)")]
    public string id;

    [Tooltip("UI 표시명. 예: 딜러 +2")]
    public string displayName;

    // ----------------------------------------------------------
    // [스택]
    // ----------------------------------------------------------
    [Header("스택 (Stack)")]
    [Tooltip("카드가 기여하는 스택 유형 (Dealer / Tank / Support)")]
    public StackType stackType;

    [Tooltip("스택 기여량. 양수=스택 증가, 음수=스택 감소. 예: +2, -1")]
    [FormerlySerializedAs("cardPower")]   // 기존 SO 에셋 직렬화 호환 유지
    public int stackDelta;

    // ----------------------------------------------------------
    // [에셋 (선택)]
    // ----------------------------------------------------------
    [Header("에셋 (선택)")]
    [Tooltip("카드 아트 스프라이트 (없어도 동작함)")]
    public Sprite cardArt;

    [TextArea(1, 3)]
    [Tooltip("디자인 메모 (선택)")]
    public string note;
}
