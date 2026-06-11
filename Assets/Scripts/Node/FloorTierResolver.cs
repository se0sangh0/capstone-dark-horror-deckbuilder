// ============================================================
// Node/FloorTierResolver.cs
// 현재 층 인덱스 → 등장 적 풀 + 마릿수 범위 매핑 (Combat 노드 전용)
// ============================================================
//
// [왜 이 파일이 필요한가요?]
//   기획 §11_맵_노드: 층이 올라갈수록 난이도 상승.
//   Boss/Elite 는 NodeSystem.CurrentRoomType 으로 EnemySpawner 가 직접 처리하고,
//   이 클래스는 일반 전투(Combat) 노드의 층별 적 구성만 책임진다.
//
// [매핑 정책 — 2026-06-09 갱신 (MVP 6층 고정 일렬)]
//   MVP 맵 전투(Combat) 노드 = layer 1·3 (CurrentFloor = currentRowIndex = 1, 3) 두 곳.
//   일반 전투는 고블린만 등장, 각 2마리(RollCount ≤3 → 2).
//   약탈자(enemy_raider_01)는 일반 전투엔 미등장(의도) — 엘리트로만 등장:
//     튜토리얼 Elite 노드 + MVP 랜덤노드(Event)의 엘리트 결과(발표용 가중치 0이라 현재 미발생).
//   ※ RollCount 의 floor>3(3~4마리) 분기는 튜토리얼/향후 맵용으로 보존.
//
// [tier 폴백 — Combat 노드 한정]
//   GetEnemyPool 이 비어있을 때 EnemySpawner 가 사용. Boss tier 는 절대 반환 안 함
//   (일반 노드에 보스가 spawn 되어 엔딩이 잘못 트리거되는 버그 방지).
// ============================================================

public static class FloorTierResolver
{
    /// <summary>0-base 층 인덱스를 받아 등장시킬 적 tier 를 반환한다 (Combat 노드 폴백용).</summary>
    public static EnemyTier ResolveTier(int floorIndex)
    {
        // Combat 노드 폴백은 절대 Boss 를 반환하지 않는다 — 일반 노드에 보스가 spawn 되어
        // 엔딩이 잘못 트리거되는 버그 방지. 보스 등장은 RoomType.Boss 노드에서만.
        if (floorIndex >= 4) return EnemyTier.Normal;
        return EnemyTier.Weak;
    }

    // ============================================================
    // 일반 전투 노드 — 적 ID 풀 (Combat 노드만 사용)
    // ============================================================
    //
    // [반환값]
    //   풀 (unique IDs). EnemySpawner 가 RollCount() 마릿수만큼 풀에서 랜덤 선택.
    //   매핑이 없으면 null → tier 기반 폴백.
    //
    // [현 정책]
    //   모든 층 = 고블린만. 약탈자는 Elite 노드에서만 등장.
    //   추후 신규 일반 적 추가 시 풀에 합류.
    // ============================================================
    public static string[] GetEnemyPool(int floorIndex)
    {
        if (floorIndex >= 1 && floorIndex <= 6)
            return new[] { "enemy_goblin_01" };
        return null;
    }

    // ============================================================
    // 마릿수 결정 — 2026-06-09 (MVP 6층 고정 일렬)
    //   MVP 전투는 floor 1·3 → 둘 다 2마리. floor>3(3~4마리)는 튜토리얼/향후 맵용 보존.
    // ============================================================
    public static int RollCount(int floorIndex)
    {
        if (floorIndex <= 3) return 2;                  // MVP 전투(floor 1·3): 2마리
        return UnityEngine.Random.Range(3, 5);          // 향후 후반층: 3 or 4마리
    }
}
