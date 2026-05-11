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
    public string skillId;     // (구) 단일 스킬 — 호환을 위해 보존
    public string[] skillIds;  // (신규) 다중 적 스킬 ID — enemy_skills.json 의 id 와 매칭
    public string spritePath;  // Resources 기준, 확장자 X
}

[System.Serializable]
public class EnemyDefCollection { public List<EnemyDef> enemies; }