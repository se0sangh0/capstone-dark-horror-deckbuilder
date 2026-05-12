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

    // ── 소환체 메커니즘 (기획 §11 §3 까마귀) — 일반 적은 0 또는 false ──
    public int    hitCountToDie;       // 0 = HP 기반, >0 = N hit 으로 처치
    public int    summonLifeTurns;     // 0 = 영구, >0 = 턴 카운터
    public int    expirePenaltyPower;  // 수명 만료 시 파티 분산 데미지 (1마리당)
    public bool   isPassive;           // true 면 행동 페이즈 스킵
}

[System.Serializable]
public class EnemyDefCollection { public List<EnemyDef> enemies; }