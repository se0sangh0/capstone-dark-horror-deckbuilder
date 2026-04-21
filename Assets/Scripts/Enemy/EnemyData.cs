// EnemyData.cs
// 적 데이터 + 런타임 상태 통합 ScriptableObject (partial 루트)
//
// ── 분리된 partial 파일 ─────────────────────────────────────────
//   EnemyData.Hp.cs    : HP 시스템 (CurrentHp, InitHp, TakeDamage, OnHpChanged, OnDied)
//   EnemyData.Skill.cs : 스킬 조회 (GetSkill)

using UnityEngine;

public enum EnemyTier
{
    Weak   = 0,
    Normal = 1,
    Boss   = 2,
}

[CreateAssetMenu(menuName = "DarkHorror/EnemyData", fileName = "enemy_new")]
public partial class EnemyData : ScriptableObject
{
    // ── 정의 데이터 ──────────────────────────────────────────────
    [Header("ID / 표시")]
    public string id;
    public string displayName;

    [Header("전투 정보")]
    public EnemyTier tier        = EnemyTier.Normal;
    public int       maxHp       = 50;
    public int       attackPower = 15;
    public int       attackCost  = 1;

    [Header("스킬")]
    public string skillId;

    [Header("시각")]
    public Sprite portrait;
    public string spritePath;

    [Header("메모")]
    [TextArea(2, 4)]
    public string note;

    // ── 런타임 상태 ──────────────────────────────────────────────
    [Header("런타임 상태")]
    public bool   isDead      = false;
    public Sprite enemySprite;          // 기존 EnemyRuntime.enemySprite 이전

    public int StackValue => attackCost;
}