// ============================================================
// Event/EventCatalog.cs
// `?` 노드 선택지 이벤트 런타임 카탈로그 — 로드 + 무작위 추첨(런 내 중복 방지)
// ============================================================
//
// [로드 우선순위]
//   ① Resources/Events/ 아래의 EventDefinition 에셋 (Generator 로 구운 것)
//   ② 에셋이 하나도 없으면 EventCatalogData.BuildAll() 코드 정의로 폴백
//      → 에셋을 아직 굽지 않았어도 즉시 동작한다.
//
// [무작위 규칙] (기획 §2 — 런 안에서 중복 노출하지 않는다)
//   GetRandom(floor) : 아직 소비하지 않은 이벤트 중 층 조건에 맞는 것을 균등 추첨.
//   전부 소비했으면 소비 기록을 비우고 다시 순환한다 (소프트락 방지).
//   ResetRun()       : 새 런 시작 시 소비 기록 초기화.
// ============================================================

using System.Collections.Generic;
using UnityEngine;

public static class EventCatalog
{
    private const string ResourcesFolder = "Events";

    private static List<EventDefinition> _all;                 // 로드된 전체 정의 (캐시)
    private static readonly HashSet<string> _consumed = new(); // 이번 런에서 소비한 이벤트 id

    /// <summary>전체 이벤트 정의를 로드(최초 1회 캐시). 에셋 우선, 없으면 코드 폴백.</summary>
    public static IReadOnlyList<EventDefinition> All
    {
        get
        {
            if (_all != null && _all.Count > 0) return _all;

            _all = new List<EventDefinition>();

            var loaded = Resources.LoadAll<EventDefinition>(ResourcesFolder);
            if (loaded != null && loaded.Length > 0)
            {
                _all.AddRange(loaded);
                Debug.Log($"[EventCatalog] Resources/{ResourcesFolder} 에서 이벤트 {_all.Count}종 로드.");
            }
            else
            {
                _all.AddRange(EventCatalogData.BuildAll());
                Debug.Log($"[EventCatalog] SO 에셋 없음 — 코드 정의(EventCatalogData) {_all.Count}종으로 폴백. " +
                          "영구 에셋을 원하면 Tools ▸ DarkHorror ▸ 이벤트 카탈로그 생성 을 실행하세요.");
            }

            return _all;
        }
    }

    /// <summary>새 런 시작 시 호출 — 소비 기록을 비운다.</summary>
    public static void ResetRun()
    {
        _consumed.Clear();
        Debug.Log("[EventCatalog] 런 소비 기록 초기화.");
    }

    /// <summary>
    /// 아직 소비하지 않은 이벤트 중 층 조건(1-base floor)에 맞는 것을 균등 추첨한다.
    /// 반환 직전 소비 처리하여 같은 런에서 다시 나오지 않게 한다.
    /// 후보가 없으면 소비 기록을 비우고 재순환한다. 정의가 아예 없으면 null.
    /// </summary>
    public static EventDefinition GetRandom(int floor = 1)
    {
        var all = All;
        if (all == null || all.Count == 0)
        {
            Debug.LogWarning("[EventCatalog] 등록된 이벤트가 없습니다.");
            return null;
        }

        var pool = BuildPool(all, floor);

        // 전부 소비했으면 순환 리셋 후 재구성
        if (pool.Count == 0)
        {
            _consumed.Clear();
            pool = BuildPool(all, floor);
        }

        // 층 조건조차 만족하는 게 없으면(잘못된 min/max) 층 무시하고 전체에서 추첨
        if (pool.Count == 0)
        {
            foreach (var e in all) if (e != null) pool.Add(e);
        }
        if (pool.Count == 0) return null;

        var picked = pool[Random.Range(0, pool.Count)];
        if (picked != null && !string.IsNullOrEmpty(picked.id)) _consumed.Add(picked.id);
        return picked;
    }

    /// <summary>소비하지 않았고 층 조건에 맞는 후보 목록.</summary>
    private static List<EventDefinition> BuildPool(IReadOnlyList<EventDefinition> all, int floor)
    {
        var pool = new List<EventDefinition>();
        foreach (var e in all)
        {
            if (e == null) continue;
            if (!string.IsNullOrEmpty(e.id) && _consumed.Contains(e.id)) continue;
            if (!e.MatchesFloor(floor)) continue;
            pool.Add(e);
        }
        return pool;
    }
}
