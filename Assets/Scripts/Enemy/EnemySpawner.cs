using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Tooltip("이 노드(전투)에 등장할 적 ID 목록. 비어있으면 tier 기반 랜덤.")]
    [SerializeField] private List<string> enemyIds = new();

    [Tooltip("랜덤 모드용 — tier")]
    [SerializeField] private EnemyTier randomTier = EnemyTier.Normal;

    [Tooltip("랜덤 모드용 — 등장 마릿수")]
    [SerializeField, Range(1, 5)] private int randomCount = 3;

    private void OnEnable()
    {
        // BattleManager 의 OnEnable 보다 먼저 실행돼야 함 (Script Execution Order 설정 권장)
        SpawnIntoBattleManager();
    }

    private void SpawnIntoBattleManager()
    {
        var bm = BattleManager.Instance;
        if (bm == null) bm = FindObjectOfType<BattleManager>(true);   // ← 이 줄 추가
        if (bm == null) {
            Debug.LogError("[EnemySpawner] 씬에 BattleManager 없음.");
            return;
        }
        if (EnemyDatabase.Instance == null) {
            Debug.LogError("[EnemySpawner] EnemyDatabase 없음.");
            return;
        }

        // ── 층 + RoomType 기반 적 결정 ──────────────────────────
        // 우선순위:
        //   1) Inspector enemyIds 채워져있으면 → 그것 우선 (수동 강제)
        //   2) NodeSystem.CurrentRoomType 보고 분기:
        //      · Boss   → enemy_reaper_boss
        //      · Elite  → 약탈자 ×3 (기획 §10 §엘리트 — 약탈자 1종만 정의됨)
        //      · Combat → FloorTierResolver.ResolveEnemyIds(floor) 층별 매핑
        //   3) NodeSystem 없으면 → tier 기반 랜덤 폴백
        EnemyTier tierToUse = randomTier;
        string[] floorIds   = null;
        RoomType currentRoom = RoomType.Combat;

        if (NodeSystem.Current != null)
        {
            int floor    = NodeSystem.Current.CurrentFloor;
            currentRoom  = NodeSystem.Current.CurrentRoomType;

            switch (currentRoom)
            {
                case RoomType.Boss:
                    floorIds = new[] { "enemy_reaper_boss" };
                    tierToUse = EnemyTier.Boss;
                    break;

                case RoomType.Elite:
                    // TODO[엘리트 구성]: 기획 §10_적_스킬_시트 §엘리트는 약탈자 1종만 정의됨.
                    //                  마릿수/구성 확정되면 본문 수정.
                    floorIds = new[] { "enemy_raider_01", "enemy_raider_01", "enemy_raider_01" };
                    tierToUse = EnemyTier.Normal;
                    break;

                case RoomType.Combat:
                default:
                    floorIds = FloorTierResolver.ResolveEnemyIds(floor);
                    tierToUse = FloorTierResolver.ResolveTier(floor);
                    break;
            }

            Debug.Log($"[EnemySpawner] 층 {floor} / RoomType={currentRoom} | 매핑 적: {(floorIds == null ? "(없음 → tier 폴백)" : string.Join(", ", floorIds))} | tier 폴백: {tierToUse}");
        }

        // 우선순위: 1)Inspector → 2)RoomType/층 매핑 → 3)tier 랜덤
        string[] idsToSpawn = null;
        if (enemyIds != null && enemyIds.Count > 0)       idsToSpawn = enemyIds.ToArray();
        else if (floorIds != null && floorIds.Length > 0) idsToSpawn = floorIds;

        var spawned = new List<EnemyData>();
        if (idsToSpawn != null)
        {
            foreach (var id in idsToSpawn)
            {
                var def = EnemyDatabase.Instance.GetEnemy(id);
                if (def != null) spawned.Add(EnemyDatabase.CreateRuntimeEnemy(def));
            }
        }
        else
        {
            for (int i = 0; i < randomCount; i++)
            {
                var def = EnemyDatabase.Instance.GetRandomEnemy(tierToUse);
                if (def != null) spawned.Add(EnemyDatabase.CreateRuntimeEnemy(def));
            }
        }
        bm.enemies = spawned;
        Debug.Log($"[EnemySpawner] 적 {spawned.Count}마리 주입 완료 (기존 목록 덮어씀).");
    }
}

