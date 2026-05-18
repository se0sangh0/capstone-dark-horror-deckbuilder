// EnemyCardDeathHider.cs
// 적 카드 사망 시 GameObject 자동 비활성화 — AllyCardDeathHider 의 적군 버전.
//
// ── 동작 ────────────────────────────────────────────────────────
//   EnemyData.OnDied 이벤트가 발동되면 즉시 SetActive(false) 처리.
//   ClearSpawnedObjects 로 Destroy 시 OnDestroy 에서 이벤트 자동 해제.
//
// ── 사용 ────────────────────────────────────────────────────────
//   DefaultSetting.SpawnObject() 가 적 카드 생성 시 AddComponent + Bind 호출.

using UnityEngine;

public class EnemyCardDeathHider : MonoBehaviour
{
    private EnemyData _enemy;

    public void Bind(EnemyData enemy)
    {
        if (_enemy != null) _enemy.OnDied -= HandleDied;
        _enemy = enemy;
        if (_enemy != null) _enemy.OnDied += HandleDied;
    }

    private void HandleDied()
    {
        if (this != null && gameObject != null)
            gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_enemy != null) _enemy.OnDied -= HandleDied;
    }
}
