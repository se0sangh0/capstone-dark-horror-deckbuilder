// DeckBuilder.cs
// 동료 성향과 역할을 바탕으로 덱을 자동 생성한다.
// 기획 기준: 기획/시스템/02_카드_설계_프레임.md
//   - 동료 1명당 10장, 파티 4인 기준 시작 덱 40장 고정
//   - 역할과 성향에 맞는 카드만 덱에 삽입

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 동료 리스트를 받아 덱을 자동 생성하는 유틸리티.
/// MonoBehaviour 없이 정적매서드로 동작한다 — 년제 없이 어디서나 호출 가능.
/// </summary>
public static class DeckBuilder
{
    /// <summary>동료 1명당 덱에 들어가는 카드 수 (MVP 고정값)</summary>
    public const int CardsPerCompanion = 10;

    // -------------------------------------------------------
    // 파일릭: 전체 뜛 구축
    // -------------------------------------------------------

    /// <summary>
    /// 파티 전체의 덱을 생성한다.
    /// 각 동료의 역할과 성향에 맞는 카드만 10장씩 삽입하여 반환한다.
    /// </summary>
    /// <param name="companions">파티 동료 리스트 (MVP: 4명 고정)</param>
    /// <param name="allCards">전체 카드 풀 (Resources 또는 Inspector에서 주입)</param>
    /// <returns>셔플된 전체 덱 (CompanionData 태그 동반)</returns>
    public static List<(CardData card, CompanionData owner)> BuildPartyDeck(
        List<CompanionData> companions,
        List<CardData> allCards)
    {
        var deck = new List<(CardData, CompanionData)>();

        foreach (var companion in companions)
        {
            var matched = GetMatchingCards(companion, allCards);

            if (matched.Count == 0)
            {
                Debug.LogWarning($"[DeckBuilder] {companion.displayName}({companion.id})"
                    + " 에 맞는 카드를 찾지 못했습니다.");
                continue;
            }

            var companionCards = DrawCards(matched, CardsPerCompanion);
            foreach (var card in companionCards)
                deck.Add((card, companion));
        }

        Shuffle(deck);
        return deck;
    }

    // -------------------------------------------------------
    // 동반 메서드
    // -------------------------------------------------------

    /// <summary>
    /// 동료의 역할과 성향에 맞는 카드를 필터링한다.
    /// 역할(Role)은 일치 필수, 성향(Affinity)은 동일하거나 None이면 통과.
    /// </summary>
    private static List<CardData> GetMatchingCards(
        CompanionData companion,
        List<CardData> allCards)
    {
        return allCards
            .Where(c => (int)c.stackType == (int)companion.role)
            .ToList();
    }

    /// <summary>
    /// 리스트에서 count장을 복원 추첨로 선택한다.
    /// 풀이 count보다 적으면 여러 번 순환 사용.
    /// </summary>
    private static List<CardData> DrawCards(List<CardData> pool, int count)
    {
        var result = new List<CardData>(count);
        for (int i = 0; i < count; i++)
            result.Add(pool[i % pool.Count]);
        return result;
    }

    /// <summary>Fisher-Yates 셔플 (Unity Random 사용)</summary>
    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // -------------------------------------------------------
    // 디버그 도우미
    // -------------------------------------------------------

    /// <summary>덱 구성 내역을 콘솔에 출력한다.</summary>
    public static void LogDeck(List<(CardData card, CompanionData owner)> deck)
    {
        Debug.Log($"[DeckBuilder] 전체 덱: {deck.Count}장");
        foreach (var (card, owner) in deck)
        {
            Debug.Log($"  [{owner.displayName} / {AffinityHelper.GetLabel(owner.affinity)}] "
                    + $"{card.cardArt} | 역할:{card.stackType}");
        }
    }
}
