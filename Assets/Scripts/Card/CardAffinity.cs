// CardAffinity.cs
// 동료 성향 4종 enum 정의
// 스택 생성 범위: 기획/시스템/03_동료_성향_세부기준.md 기준

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 동료 성향 4종.
/// 각 성향은 카드 스택 기여량(-5 ~ +5)의 생성 범위를 결정한다.
/// </summary>
public enum CardAffinity
{
    None        = 0,

    /// <summary>도박사 — 극단 선택, 큰 위험·큰 보상 (범위: -5 또는 +5)</summary>
    Gambler     = 1,

    /// <summary>안전주의자 — 신중한 선택, 위험 최소화 (범위: -1 ~ +3)</summary>
    Safety      = 2,

    /// <summary>기회주의자 — 유연한 대처, 리스크·보상 균형 (범위: -3 ~ +4)</summary>
    Opportunist = 3,

    /// <summary>낙천가 — 모든 가능성 개방, 자유로운 성향 (범위: -5 ~ +5)</summary>
    Optimist    = 4,
}

/// <summary>
/// 성향별 스택 생성 범위 정의.
/// 기획서 원문: 기획/시스템/03_동료_성향_세부기준.md
/// </summary>
[System.Serializable]
public struct AffinityRange
{
    public int min;
    public int max;
    /// <summary>true일 경우 min/max 값만 생성 (도박사 전용)</summary>
    public bool extremeOnly;

    public AffinityRange(int min, int max, bool extremeOnly = false)
    {
        this.min = min;
        this.max = max;
        this.extremeOnly = extremeOnly;
    }
}

/// <summary>
/// 성향 관련 유틸리티 모음.
/// 표시 이름(한글), 스택 범위, 색상 반환.
/// </summary>
public static class AffinityHelper
{
    // -----------------------------------------------------------
    // 1. 한글 표시 이름
    // -----------------------------------------------------------
    private static readonly Dictionary<CardAffinity, string> Labels = new()
    {
        { CardAffinity.Gambler,     "도박사"    },
        { CardAffinity.Safety,      "안전주의자" },
        { CardAffinity.Opportunist, "기회주의자" },
        { CardAffinity.Optimist,    "낙천가"    },
        { CardAffinity.None,        "없음"      },
    };

    public static string GetLabel(CardAffinity affinity)
        => Labels.TryGetValue(affinity, out var label) ? label : "없음";

    // -----------------------------------------------------------
    // 2. 성향별 스택 생성 범위
    // 기획 원문 기준:
    //   도박사      → -5 또는 +5 (극단값 2개 중 하나)
    //   안전주의자  → -1 ~ +3
    //   기회주의자  → -3 ~ +4
    //   낙천가      → -5 ~ +5
    // -----------------------------------------------------------
    private static readonly Dictionary<CardAffinity, AffinityRange> Ranges = new()
    {
        { CardAffinity.Gambler,     new AffinityRange(-5,  5, extremeOnly: true) },
        { CardAffinity.Safety,      new AffinityRange(-1,  3) },
        { CardAffinity.Opportunist, new AffinityRange(-3,  4) },
        { CardAffinity.Optimist,    new AffinityRange(-5,  5) },
        { CardAffinity.None,        new AffinityRange( 0,  0) },
    };

    public static AffinityRange GetRange(CardAffinity affinity)
        => Ranges.TryGetValue(affinity, out var range) ? range : new AffinityRange(0, 0);

    /// <summary>
    /// 성향 규칙에 따라 스택 기여량을 랜덤 생성한다.
    /// 도박사: -5 또는 +5 중 하나 (50:50)
    /// 나머지: 범위 내 균등 확률
    /// </summary>
    public static int GenerateStack(CardAffinity affinity)
    {
        var range = GetRange(affinity);

        if (range.extremeOnly)
        {
            // 도박사 — 극단값 2개 중 균등 선택
            return Random.value < 0.5f ? range.min : range.max;
        }

        // 나머지 성향 — 범위 내 균등 확률 (inclusive)
        return Random.Range(range.min, range.max + 1);
    }

    // -----------------------------------------------------------
    // 3. 성향별 UI 컬러 (디자인 토큰 기반)
    // accent/brass  #C39A52  — 도박사 (황동, 고위험)
    // state/success #4F8F63  — 안전주의자 (녹색, 안정)
    // accent/teal   #4CB3B3  — 기회주의자 (청록, 균형)
    // accent/purple #7B5EA7  — 낙천가 (보라, 자유)
    // -----------------------------------------------------------
    public static Color GetColor(CardAffinity affinity)
    {
        return affinity switch
        {
            CardAffinity.Gambler     => new Color(0.765f, 0.604f, 0.322f), // #C39A52
            CardAffinity.Safety      => new Color(0.310f, 0.561f, 0.388f), // #4F8F63
            CardAffinity.Opportunist => new Color(0.298f, 0.702f, 0.702f), // #4CB3B3
            CardAffinity.Optimist    => new Color(0.482f, 0.369f, 0.655f), // #7B5EA7
            _                        => Color.white,
        };
    }
}
