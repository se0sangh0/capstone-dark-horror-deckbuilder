using System.Collections.Generic;

[System.Serializable]
public class EnemyDef
{
    public string id;
    public string displayName;
    public string tier;        // "Weak" / "Normal" / "Boss"
    public int    maxHp;
    public int    attackPower;
    public int    attackCost;
    public string skillId;
    public string spritePath;  // Resources 기준, 확장자 X
}

[System.Serializable]
public class EnemyDefCollection { public List<EnemyDef> enemies; }