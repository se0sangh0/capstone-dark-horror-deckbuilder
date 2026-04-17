using UnityEngine;

[CreateAssetMenu(menuName = "DarkHorror/EnemyRuntime", fileName = "enemy_runtime_new")]
public class EnemyRuntime : ScriptableObject
{
    [Header("정의 데이터")]
    public EnemyData data;

    [Header("런타임 상태")]
    public int currentHp = 0;
    public bool isDead = false;
    public Sprite enemySprite;

    public void Init()
    {
        if (data == null) return;

        currentHp = data.maxHp;
        isDead = false;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHp = Mathf.Max(0, currentHp - amount);

        if (currentHp <= 0)
            Die();
    }

    public void Die()
    {
        isDead = true;
    }
}