// FellowDef.cs
// fellow.json 역직렬화 전용 데이터 구조체.
//
// ── 역할 ────────────────────────────────────────────────────────
//   Resources/Data/fellow.json 에서 JsonUtility 로 읽어들이는
//   런타임 전용 구조체입니다.
//   FellowDatabase.cs 에서 파싱하여 GetFellow(id) 로 조회됩니다.
//
// ── fellow.json 구조 예시 ────────────────────────────────────────
//   {
//     "fellows": [
//       {
//         "id": "ally_caster_01",
//         "jobClass": "마법사",
//         "displayName": "딜러 아리",
//         "role": "Dealer",
//         "maxHp": 80,
//         "stressResist": 5,
//         "recruitCost": 30,
//         "skillIds": ["skill_fire_01", "skill_fire_02"]
//       }
//     ]
//   }

using System.Collections.Generic;

[System.Serializable]
public class FellowDef
{
    public string   id;
    public string   jobClass;
    public string   displayName;
    public string   role;           // "Dealer" / "Tanker" / "Support"
    public int      maxHp;
    public int      stressResist;
    public int      recruitCost;
    public int      starLevel;      // 1 / 2 / 3  (기본값 1★)

    // ── [강화 시스템 TODO] ─────────────────────────────────────────
    // ★ 성급 시스템 (롤토체스 방식)
    //
    // JSON 에서 starLevel 은 항상 1 로 저장한다 (베이스 정의값).
    // 런타임 성급은 FellowData.starLevel 에서 별도 관리한다.
    //
    // 성급별 스탯 배율 (기획서 §합성/승급):
    //   1★ → maxHp × 1.00,  스킬 파워 × 1.00
    //   2★ → maxHp × 1.50,  스킬 파워 × 1.50
    //   3★ → maxHp × 2.25,  스킬 파워 × 2.25  (1.5² 복리)
    //
    // 승급 조건 (기획서 §합성/승급):
    //   같은 역할군 + 같은 성급 동료 3명 → 소멸 후 랜덤 역할 다음 성급 동료 획득
    //   ex) 1★ 딜러 × 3  →  2★ 랜덤 역할
    //       2★ 탱커 × 3  →  3★ 랜덤 역할

    public string[] skillIds;
    public string spritePath;
}

[System.Serializable]
public class FellowDefCollection
{
    public List<FellowDef> fellows;
}
