// ============================================================
// Card/DeckBuilder.cs
// 파티 덱 자동 생성 유틸리티
// ============================================================
//
// [이 파일이 하는 일]
//   동료 목록과 카드 풀을 받아서 파티 전체의 덱을 자동으로 만들어 줍니다.
//
//   동료마다 역할에 맞는 카드 10장씩을 골라 덱에 넣고,
//   Fisher-Yates 알고리즘으로 무작위로 섞어 반환합니다.
//
// [덱 구성 규칙 (기획서 기준)]
//   - 동료 1명당 10장 (CardsPerCompanion = 10)
//   - 파티 4명 기준: 시작 덱 40장
//   - 역할이 일치하는 카드만 포함됨
//
// [MonoBehaviour 아님]
//   이 클래스는 GameObject 에 붙이지 않아도 됩니다.
//   정적 메서드(static) 로만 구성되어 어디서나 호출 가능합니다.
//
// [어디서 쓰이나요?]
//   - BattleManager.InitBattle() 에서 덱 구성 시 호출
//   - GameManager.InjectDeck() 에 전달됨
// ============================================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 동료 리스트를 받아 파티 덱을 자동 생성하는 정적 유틸리티 클래스.
/// </summary>
public static class DeckBuilder
{
    // ----------------------------------------------------------
    // [상수]
    // ----------------------------------------------------------

    /// <summary>동료 1명당 덱에 들어가는 카드 수 (MVP 고정값)</summary>
    public const int CardsPerCompanion = 10;

    // ----------------------------------------------------------
    // 파티 덱 전체 구축 (공개 메서드)
    // ----------------------------------------------------------

    /// <summary>
    /// 파티 전체의 덱을 생성한다.
    /// 각 동료의 역할에 맞는 카드를 CardsPerCompanion 장씩 뽑아
    /// 셔플된 덱으로 반환한다.
    /// </summary>
    /// <param name="companions">파티 동료 목록</param>
    /// <param name="allCards">전체 카드 풀 (BattleManager 가 생성)</param>
    /// <returns>셔플된 덱 (카드 + 소유 동료 정보 포함)</returns>
    public static List<(CardData card, CompanionData owner)> BuildPartyDeck(
        List<CompanionData> companions,
        List<CardData> allCards)
    {
        var deck = new List<(CardData, CompanionData)>();

        foreach (var companion in companions)
        {
            // 이 동료 역할에 맞는 카드만 필터링
            var matched = GetMatchingCards(companion, allCards);

            if (matched.Count == 0)
            {
                Debug.LogWarning($"[DeckBuilder] {companion.displayName}({companion.id}) 에 맞는 카드를 찾지 못했습니다.");
                continue;
            }

            // CardsPerCompanion 장을 뽑아 덱에 추가
            var companionCards = DrawCards(matched, CardsPerCompanion);
            foreach (var card in companionCards)
                deck.Add((card, companion));
        }

        // Fisher-Yates 셔플
        Shuffle(deck);

        Debug.Log($"[DeckBuilder] 덱 생성 완료: {deck.Count}장 (동료 {companions.Count}명 × {CardsPerCompanion}장)");
        return deck;
    }

    // ----------------------------------------------------------
    // 카드 필터링 (역할 일치)
    // ----------------------------------------------------------

    /// <summary>
    /// 동료의 역할(role)에 맞는 카드만 필터링하여 반환한다.
    /// </summary>
    private static List<CardData> GetMatchingCards(CompanionData companion, List<CardData> allCards)
    {
        var matched = allCards
            .Where(c => (int)c.stackType == (int)companion.role)
            .ToList();

        Debug.Log($"[DeckBuilder] {companion.displayName} (role={(int)companion.role}) → 매칭 카드 {matched.Count}장");
        return matched;
    }

    // ----------------------------------------------------------
    // 카드 뽑기 (복원 추첨)
    // ----------------------------------------------------------

    /// <summary>
    /// 카드 풀에서 count 장을 복원 추첨으로 뽑는다.
    /// 풀의 카드가 count 보다 적으면 순환하여 채운다.
    /// </summary>
    private static List<CardData> DrawCards(List<CardData> pool, int count)
    {
        var result = new List<CardData>(count);
        for (int i = 0; i < count; i++)
            result.Add(pool[i % pool.Count]);
        return result;
    }

    // ----------------------------------------------------------
    // 셔플 (Fisher-Yates 알고리즘)
    // ----------------------------------------------------------

    /// <summary>
    /// Fisher-Yates 알고리즘으로 리스트를 무작위로 섞는다.
    /// </summary>
    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // ----------------------------------------------------------
    // 디버그 출력
    // ----------------------------------------------------------

    /// <summary>덱 구성 내역을 콘솔에 상세 출력한다.</summary>
    public static void LogDeck(List<(CardData card, CompanionData owner)> deck)
    {
        Debug.Log($"[DeckBuilder] 전체 덱: {deck.Count}장");
        foreach (var (card, owner) in deck)
        {
            Debug.Log($"  [{owner.displayName} / {AffinityHelper.GetLabel(owner.affinity)}] "
                    + $"카드:{card.id} | 역할:{card.stackType}");
        }
    }
}
