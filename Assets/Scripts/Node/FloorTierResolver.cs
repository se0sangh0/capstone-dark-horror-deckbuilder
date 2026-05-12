// ============================================================
// Node/FloorTierResolver.cs
// 현재 층 인덱스 → 등장 적 tier / ID 매핑 (Combat 노드 전용)
// ============================================================
//
// [왜 이 파일이 필요한가요?]
//   기획 §11_맵_노드: 층이 올라갈수록 난이도 상승.
//   Boss/Elite 는 NodeSystem.CurrentRoomType 으로 EnemySpawner 가 직접 처리하고,
//   이 클래스는 일반 전투(Combat) 노드의 층별 적 구성만 책임진다.
//
// [매핑 정책 — 기획 §11_맵_노드 + §10_적_스킬_시트 반영, 10층 자동맵]
//   floorIndex 1~3 → Weak    (고블린)
//   floorIndex 4~8 → Normal  (약탈자)
//   floorIndex 9   → 화툿불 (적 안 뜸, 호출되지 않음)
//   floorIndex 10  → Boss   (이 파일이 처리하지 않음 — EnemySpawner 분기)
//
// [범위 가드]
//   floorIndex 가 음수면 Weak, 9 이상이면 Normal 로 클램프 (예외 안 던짐).
// ============================================================

public static class FloorTierResolver
{
    /// <summary>0-base 층 인덱스를 받아 등장시킬 적 tier 를 반환한다 (Combat 노드 폴백용).</summary>
    public static EnemyTier ResolveTier(int floorIndex)
    {
        // 10층 이상은 보스 (이쪽으론 안 옴 — EnemySpawner 가 RoomType=Boss 로 분기)
        if (floorIndex >= 10) return EnemyTier.Boss;

        // 4~9층은 일반 (약탈자급)
        if (floorIndex >= 4) return EnemyTier.Normal;

        // 1~3층은 약함 (고블린)
        return EnemyTier.Weak;
    }

    // ============================================================
    // 층별 적 ID 리스트 — 자동 생성 10층 맵 기준 (Combat 노드만 사용)
    // ============================================================
    //
    // [floorIndex 기준]
    //   NodeSystem.CurrentFloor 는 노드 클릭 직후 ++ 되어 1부터 시작 → 1=1층.
    //   layer 0 (1층) = Combat 고정, layer 8 (9층) = Rest, layer 9 (10층) = Boss.
    //
    // [반환값]
    //   매핑이 있으면 적 ID 배열 / 없으면 null (EnemySpawner 에서 tier 기반 폴백)
    //
    // TODO[밸런스]: 마릿수/구성은 MVP 전투 테스트 후 확정. 현재는 임시값.
    // ============================================================
    public static string[] ResolveEnemyIds(int floorIndex)
    {
        switch (floorIndex)
        {
            case 1: return new[] { "enemy_goblin_01", "enemy_goblin_01" };                                          // 1층 — 고블린×2
            case 2: return new[] { "enemy_goblin_01", "enemy_goblin_01", "enemy_goblin_01" };                       // 2층 — 고블린×3
            case 3: return new[] { "enemy_raider_01", "enemy_goblin_01", "enemy_goblin_01" };                       // 3층 — 약탈자×1 + 고블린×2
            case 4: return new[] { "enemy_raider_01", "enemy_raider_01" };                                          // 4층 — 약탈자×2
            case 5: return new[] { "enemy_raider_01", "enemy_raider_01", "enemy_goblin_01" };                       // 5층 — 약탈자×2 + 고블린×1
            case 6: return new[] { "enemy_raider_01", "enemy_raider_01", "enemy_raider_01" };                       // 6층 — 약탈자×3
            case 7: return new[] { "enemy_raider_01", "enemy_raider_01", "enemy_raider_01" };                       // 7층 — 약탈자×3
            case 8: return new[] { "enemy_raider_01", "enemy_raider_01", "enemy_raider_01" };                       // 8층 — 약탈자×3
            // case 9: 화툿불 — 호출되지 않음
            // case 10: Boss — EnemySpawner 가 RoomType=Boss 로 직접 처리
            default: return null;                                                                                    // 정의 없음 → tier 기반 폴백
        }
    }
}
