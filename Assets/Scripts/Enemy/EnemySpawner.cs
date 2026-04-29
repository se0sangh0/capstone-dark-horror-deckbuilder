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

        var spawned = new List<EnemyData>();
        if (enemyIds != null && enemyIds.Count > 0) {
            foreach (var id in enemyIds) {
                var def = EnemyDatabase.Instance.GetEnemy(id);
                if (def != null) spawned.Add(EnemyDatabase.CreateRuntimeEnemy(def));
            }
        } else {
            for (int i = 0; i < randomCount; i++) {
                var def = EnemyDatabase.Instance.GetRandomEnemy(randomTier);
                if (def != null) spawned.Add(EnemyDatabase.CreateRuntimeEnemy(def));
            }
        }
        bm.enemies = spawned;
        Debug.Log($"[EnemySpawner] JSON 적 {spawned.Count}마리 주입 완료 (기존 목록 덮어씀).");
    }
}

