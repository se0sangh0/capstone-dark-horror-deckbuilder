// ============================================================
// Node/FloorTierResolver.cs
// 현재 층 인덱스 → 등장 적 tier 매핑
// ============================================================
//
// [왜 이 파일이 필요한가요?]
//   사용자 요청: "배틀 노드를 지날때마다 상위 적이 출현하게 만들어야 함"
//   기존 EnemySpawner 는 노드별로 Inspector 에서 tier 를 일일이 정해야 했지만,
//   이 클래스를 쓰면 NodeSystem.CurrentFloor 를 읽어 자동 결정합니다.
//
// [매핑 정책 — 기획 §11_맵_노드 + §10_적_스킬_시트 반영]
//   floorIndex 0  (1층 시작 노드)        → Weak    (고블린)
//   floorIndex 1~2 (2~3층)              → Weak    (고블린)
//   floorIndex 3~7 (4~8층)              → Normal  (약탈자)
//   floorIndex 8  (9층 화툿불 — 적 안 뜸 — 호출되지 않음)
//   floorIndex 9  (10층 보스)            → Boss    (거두는 자)
//
// [범위 가드]
//   floorIndex 가 음수면 Weak, 최대보다 크면 Boss 로 클램프 (예외 안 던짐).
// ============================================================

public static class FloorTierResolver
{
    /// <summary>0-base 층 인덱스를 받아 등장시킬 적 tier 를 반환한다 (단순 폴백용).</summary>
    public static EnemyTier ResolveTier(int floorIndex)
    {
        // 10층(인덱스 9) 이상은 보스
        if (floorIndex >= 9) return EnemyTier.Boss;

        // 4~9층(인덱스 3~8) 은 일반 (약탈자급)
        if (floorIndex >= 3) return EnemyTier.Normal;

        // 1~3층(인덱스 0~2) 은 약함 (고블린)
        return EnemyTier.Weak;
    }

    // ============================================================
    // ✨ 층별 정확한 적 ID 리스트 (혼합 구성 가능)
    // ============================================================
    //
    // [사용자 정의 등장 패턴 — 5층 구조]
    //   1층: 고블린 2마리
    //   2층: 약탈자 1 + 고블린 2
    //   3층: 약탈자 3
    //   4층: 고블린 1
    //   5층: 보스(거두는 자) 1
    //
    // [floorIndex 기준]
    //   NodeSystem.CurrentFloor 는 노드 클릭 직후 ++ 되어 1부터 시작 → 1=1층.
    //
    // [반환값]
    //   매핑이 있으면 적 ID 배열 / 없으면 null (EnemySpawner 에서 tier 기반 폴백)
    // ============================================================
    public static string[] ResolveEnemyIds(int floorIndex)
    {
        switch (floorIndex)
        {
            case 1: return new[] { "enemy_goblin_01", "enemy_goblin_01" };                                           // 1층 — 고블린×2
            case 2: return new[] { "enemy_raider_01", "enemy_goblin_01", "enemy_goblin_01" };                        // 2층 — 약탈자×1 + 고블린×2
            case 3: return new[] { "enemy_raider_01", "enemy_raider_01", "enemy_raider_01" };                        // 3층 — 약탈자×3
            case 4: return new[] { "enemy_goblin_01" };                                                              // 4층 — 고블린×1
            case 5: return new[] { "enemy_reaper_boss" };                                                            // 5층 — 보스
            default: return null;                                                                                     // 정의 없음 → tier 기반 폴백
        }
    }
}
