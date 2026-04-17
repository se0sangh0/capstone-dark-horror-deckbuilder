using UnityEngine;

public enum EnemyTier
{
    Weak = 0,
    Normal = 1,
    Boss = 2,
}

[CreateAssetMenu(menuName = "DarkHorror/EnemyData", fileName = "enemy_new")]
public class EnemyData : ScriptableObject
{
    [Header("ID / 표시")]
    public string id;
    public string displayName;

    [Header("전투 정보")]
    public EnemyTier tier = EnemyTier.Normal;
    public int maxHp = 50;
    public int attackPower = 15;
    public int attackCost = 1;

    [Header("스킬")]
    public string skillId;

    [Header("시각")]
    public Sprite portrait;
    public string spritePath;

    [Header("메모")]
    [TextArea(2, 4)]
    public string note;

    public int StackValue => attackCost;
}