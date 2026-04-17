// ============================================================
// Card/CardAffinity.cs
// 동료 성향(Affinity) 시스템
// ============================================================
//
// [이 파일이 하는 일]
//   동료마다 가진 "성향" 에 따라 카드의 스택 기여량 범위가 달라집니다.
//
//   성향별 스택 범위:
//   - 도박사(Gambler)     : -5 또는 +5 (극단적인 선택)
//   - 안전주의자(Safety)  : -1 ~ +3 (안전한 선택)
//   - 기회주의자(Opportunist): -3 ~ +4 (균형 선택)
//   - 낙천가(Optimist)    : -5 ~ +5 (모든 가능성)
//
// [어디서 쓰이나요?]
//   - Companion/CompanionData.cs : 동료의 성향 필드
//   - GameManager.cs : 카드 스택 값 생성 시 사용
//   - Card/DeckBuilder.cs : 덱 구성 로그에서 사용
// ============================================================

using System.Collections.Generic;
using UnityEngine;

// ----------------------------------------------------------
// [CardAffinity 열거형]
// 동료 성향 4종
// ----------------------------------------------------------
/// <summary>
/// 동료 성향 4종.
/// 각 성향은 카드 스택 기여량(-5 ~ +5)의 생성 범위를 결정한다.
/// </summary>
public enum CardAffinity
{
    None        = 0,  // 없음 (낙천가와 동일 처리)

    /// <summary>도박사 — 극단적 선택, 큰 위험·큰 보상 (범위: -5 또는 +5)</summary>
    Gambler     = 1,

    /// <summary>안전주의자 — 신중한 선택, 위험 최소화 (범위: -1 ~ +3)</summary>
    Safety      = 2,

    /// <summary>기회주의자 — 유연한 대처, 리스크·보상 균형 (범위: -3 ~ +4)</summary>
    Opportunist = 3,

    /// <summary>낙천가 — 모든 가능성 개방, 자유로운 성향 (범위: -5 ~ +5)</summary>
    Optimist    = 4,
}

// ----------------------------------------------------------
// [AffinityRange 구조체]
// 성향별 스택 생성 최솟값/최댓값 정의
// ----------------------------------------------------------
/// <summary>
/// 성향별 스택 생성 범위.
/// extremeOnly=true 이면 min/max 값만 생성 (도박사 전용).
/// </summary>
[System.Serializable]
public struct AffinityRange
{
    public int  min;
    public int  max;

    /// <summary>true 이면 min/max 값만 생성 (도박사 전용)</summary>
    public bool extremeOnly;

    public AffinityRange(int min, int max, bool extremeOnly = false)
    {
        this.min         = min;
        this.max         = max;
        this.extremeOnly = extremeOnly;
    }
}

// ----------------------------------------------------------
// [AffinityHelper 정적 클래스]
// 성향 관련 유틸리티 (표시 이름, 스택 생성, 색상)
// ----------------------------------------------------------
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
        { CardAffinity.Gambler,     "도박사"     },
        { CardAffinity.Safety,      "안전주의자"  },
        { CardAffinity.Opportunist, "기회주의자"  },
        { CardAffinity.Optimist,    "낙천가"     },
        { CardAffinity.None,        "없음"       },
    };

    /// <summary>성향의 한글 표시 이름을 반환한다.</summary>
    public static string GetLabel(CardAffinity affinity)
        => Labels.TryGetValue(affinity, out var label) ? label : "없음";

    // -----------------------------------------------------------
    // 2. 성향별 스택 생성 범위
    // -----------------------------------------------------------
    private static readonly Dictionary<CardAffinity, AffinityRange> Ranges = new()
    {
        { CardAffinity.Gambler,     new AffinityRange(-5, 5, extremeOnly: true) },
        { CardAffinity.Safety,      new AffinityRange(-1, 3) },
        { CardAffinity.Opportunist, new AffinityRange(-3, 4) },
        { CardAffinity.Optimist,    new AffinityRange(-5, 5) },
        { CardAffinity.None,        new AffinityRange( 0, 0) },
    };

    /// <summary>성향의 스택 생성 범위를 반환한다.</summary>
    public static AffinityRange GetRange(CardAffinity affinity)
        => Ranges.TryGetValue(affinity, out var range) ? range : new AffinityRange(0, 0);

    /// <summary>
    /// 성향 규칙에 따라 스택 기여량을 랜덤 생성한다.
    /// 도박사: -5 또는 +5 중 하나 (50:50).
    /// 나머지: 범위 내 균등 확률.
    /// </summary>
    public static int GenerateStack(CardAffinity affinity)
    {
        var range = GetRange(affinity);

        if (range.extremeOnly)
        {
            // 도박사: 극단값 2개 중 균등 선택 (min=-5, max=+5)
            return Random.value < 0.5f ? range.min : range.max;
        }

        // 나머지 성향: 범위 내 균등 확률 (min~max 포함)
        return Random.Range(range.min, range.max + 1);
    }

    // -----------------------------------------------------------
    // 3. 성향별 UI 색상
    // 디자인 토큰 기반 색상 코드:
    //   도박사      #C39A52 (황동, 고위험)
    //   안전주의자  #4F8F63 (녹색, 안정)
    //   기회주의자  #4CB3B3 (청록, 균형)
    //   낙천가      #7B5EA7 (보라, 자유)
    // -----------------------------------------------------------

    /// <summary>성향별 UI 색상을 반환한다. (없으면 흰색)</summary>
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
