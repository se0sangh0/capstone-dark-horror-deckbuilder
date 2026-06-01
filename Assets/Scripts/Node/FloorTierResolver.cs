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
// [매핑 정책 — 2026-05-29 갱신]
//   일반 전투(Combat) 노드는 고블린만 등장. 약탈자(엘리트급)는 Elite 노드 전용으로 분리.
//   마릿수는 층 인덱스 기반 동적 계산 (RollCount):
//     1~2층: 2 고정
//     3~4층: 2~3 균등
//     5~6층: 3~4 균등
//     7~8층: 3~4 (4 가중치 70%)
//   최대 4마리 제한 — 모든 층에서 4 초과 불가.
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
        if (floorIndex >= 1 && floorIndex <= 8)
            return new[] { "enemy_goblin_01" };
        return null;
    }

    // ============================================================
    // 마릿수 결정 — 층 인덱스 기반 (2~4, 후반 가중)
    // ============================================================
    public static int RollCount(int floorIndex)
    {
        if (floorIndex <= 2) return 2;                                  // 1~2층: 2 고정
        if (floorIndex <= 4) return UnityEngine.Random.Range(2, 4);     // 3~4층: 2 or 3 (Range max exclusive)
        if (floorIndex <= 6) return UnityEngine.Random.Range(3, 5);     // 5~6층: 3 or 4
        // 7~8층: 3~4, 4 가중치 70%
        return UnityEngine.Random.value < 0.7f ? 4 : 3;
    }
}
