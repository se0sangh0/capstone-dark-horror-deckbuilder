// ============================================================
// Event/EventModels.cs
// `?` 노드 선택지 이벤트 — 직렬화 데이터 모델 (열거형 + 선택지/결과/효과)
// ============================================================
//
// [기획 참조]
//   기획_통합/06_이벤트_노드.md §4 이벤트 명세(19종) / §5 데이터 구조
//
// [구조 개요]
//   EventDefinition (SO)
//   └── choices : EventChoice[] (2~3)
//        ├── label      : 선택지 버튼에 표시되는 문구 ("물자를 챙긴다")
//        ├── costType/costAmount : 선택 즉시 지불하는 코스트 (없음/영혼석/HP/스트레스)
//        └── outcomes : EventOutcome[] (1~2, weightPercent 합 100)
//             ├── weightPercent : 확률 가중치
//             ├── effects : EventEffect[] — 한 결과가 복합 효과를 가질 수 있음
//             │              (예: 💎+10 AND 전원 🧠+10)
//             └── resultText : 결과 1줄 텍스트 (다크 호러 톤 플레이버)
//
// ⚠️ 기획서 §5 의 EventOutcome 단일 효과 스케치를 확장:
//    "💎+10 + 전원 🧠+10" 같은 복합 효과 표현을 위해 effects 리스트를 둔다.
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>선택지 선택 즉시 지불하는 코스트 종류.</summary>
public enum EventCostType
{
    None,       // 무비용 (소프트락 방지 선택지)
    SoulStone,  // 💎 영혼석
    Hp,         // ❤️ HP
    Stress,     // 🧠 스트레스 (선지불형은 드묾)
}

/// <summary>결과가 게임 상태에 가하는 효과 종류.</summary>
public enum EventEffectType
{
    None,               // 변화 없음 (플레이버만)
    SoulStone,          // 💎 영혼석 ±value
    Stress,             // 🧠 스트레스 ±value (target 대상)
    Hp,                 // ❤️ HP ±value (target 대상)
    RecruitRandom,      // 랜덤 동료 예비대 합류 (value=성급, 0=랜덤)
    NextBattleStack,    // 다음 전투 시작 시 스택 부여 (value=스택량)
    RerollAffinity,     // 동료 성향 재굴림
    ObtainObject,       // 오브제 획득 (EVT-18) — §09 연계
    Corruption,         // 오염도 ±value (EVT-18/19) — §10 백로그
    NarrativeHint,      // 암시/서사 텍스트 획득 (기계 효과 없음)

    // ── P0-04 추가 (16-A §4 EVT-01 계약) — 끝에만 추가 (직렬화 번호 유지) ──
    HpLossNoKill,       // ❤️ HP -value, 적용 후 HP = max(1, HP-value) — 사망·전멸을 만들지 않음
    StressCapped,       // 🧠 스트레스 +value, 적용 후 = min(99, +value) — stressResist·패닉 판정 미적용
}

/// <summary>효과 적용 대상.</summary>
public enum EventTarget
{
    None,       // 대상 없음 (영혼석/오염도 등 파티 무관)
    All,        // 현재 파티 전원
    RandomOne,  // 랜덤 1명
    ChosenOne,  // 플레이어가 선택한 동료 (미구현 시 RandomOne 폴백)
    LowestHp,   // 최저 HP 동료
}

/// <summary>결과 1건이 가하는 개별 효과. 하나의 EventOutcome 이 여러 개를 가질 수 있다.</summary>
[Serializable]
public class EventEffect
{
    public EventEffectType type   = EventEffectType.None;
    public int             value  = 0;
    public EventTarget     target = EventTarget.All;

    public EventEffect() { }
    public EventEffect(EventEffectType type, int value, EventTarget target)
    {
        this.type = type; this.value = value; this.target = target;
    }
}

/// <summary>선택지 1개의 결과 분기. weightPercent 로 가중 랜덤 선택된다.</summary>
[Serializable]
public class EventOutcome
{
    [Range(0, 100)]
    [Tooltip("이 결과가 뽑힐 가중치(%). 한 선택지 내 outcomes 의 합은 100.")]
    public int weightPercent = 100;

    [Tooltip("이 결과가 가하는 효과 목록 (복합 가능).")]
    public List<EventEffect> effects = new();

    [TextArea(1, 4)]
    [Tooltip("결과 1줄 텍스트 (다크 호러 톤).")]
    public string resultText = "";

    public EventOutcome() { }
    public EventOutcome(int weightPercent, string resultText, params EventEffect[] effects)
    {
        this.weightPercent = weightPercent;
        this.resultText    = resultText;
        this.effects       = new List<EventEffect>(effects ?? Array.Empty<EventEffect>());
    }
}

/// <summary>이벤트의 선택지 1개. 버튼 1개로 표시된다.</summary>
[Serializable]
public class EventChoice
{
    [Tooltip("버튼에 표시되는 선택지 문구.")]
    public string label = "";

    [Header("코스트 (선택 즉시 지불)")]
    public EventCostType costType   = EventCostType.None;
    public int           costAmount = 0;

    [Tooltip("결과 분기 (1~2). weightPercent 합 100.")]
    public List<EventOutcome> outcomes = new();

    public EventChoice() { }
    public EventChoice(string label, EventCostType costType, int costAmount, params EventOutcome[] outcomes)
    {
        this.label      = label;
        this.costType   = costType;
        this.costAmount = costAmount;
        this.outcomes   = new List<EventOutcome>(outcomes ?? Array.Empty<EventOutcome>());
    }

    /// <summary>영혼석 코스트가 있으면 그 금액, 없으면 0.</summary>
    public int SoulStoneCost => costType == EventCostType.SoulStone ? costAmount : 0;
}
