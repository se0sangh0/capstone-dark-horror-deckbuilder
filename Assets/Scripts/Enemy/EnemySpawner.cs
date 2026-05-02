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

    [Tooltip("✨ NodeSystem 이 씬에 있으면 자동으로 층 기반 tier 가 적용됩니다. " +
             "이 옵션은 NodeSystem 이 없는 단일 전투씬 테스트에서만 의미가 있고, " +
             "true 면 NodeSystem 없이도 층 기반 폴백을 시도합니다.")]
    [SerializeField] private bool useFloorBasedTier = false;

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

        // ── 층 기반 적 결정 ──────────────────────────────────────
        // (옛 가드 제거: 1차 전투 후 2차 전투 진입 시 적이 안 나오던 버그 fix)
        // 우선순위:
        //   1) Inspector 의 enemyIds 가 채워져 있으면 → 그것을 그대로 사용 (수동 강제)
        //   2) NodeSystem 살아있으면 → FloorTierResolver.ResolveEnemyIds(floor) 정확 매핑
        //   3) 위 둘 다 없으면 → tier 기반 랜덤 풀 (테스트용 폴백)
        EnemyTier tierToUse  = randomTier;
        string[] floorIds    = null;
        if (NodeSystem.Current != null)
        {
            int floor = NodeSystem.Current.CurrentFloor;
            floorIds  = FloorTierResolver.ResolveEnemyIds(floor);
            tierToUse = FloorTierResolver.ResolveTier(floor);
            Debug.Log($"[EnemySpawner] NodeSystem 감지 → 층 {floor} | 매핑 적: {(floorIds == null ? "(없음 → tier 폴백)" : string.Join(", ", floorIds))} | tier 폴백: {tierToUse}");
        }
        else if (useFloorBasedTier)
        {
            Debug.LogWarning("[EnemySpawner] useFloorBasedTier=true 이지만 NodeSystem 없음 → randomTier 로 폴백.");
        }

        // 우선순위 1) Inspector 직접 지정 → 우선순위 2) FloorTierResolver 매핑 → 우선순위 3) tier 랜덤
        string[] idsToSpawn = null;
        if (enemyIds != null && enemyIds.Count > 0)      idsToSpawn = enemyIds.ToArray();
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
        Debug.Log($"[EnemySpawner] 적 {spawned.Count}마리 주입 완료.");
    }
}