// EnemyData.cs
// 적 데이터 + 런타임 상태 통합 ScriptableObject (partial 루트)
//
// ── 분리된 partial 파일 ─────────────────────────────────────────
//   EnemyData.Hp.cs    : HP 시스템 (CurrentHp, InitHp, TakeDamage, OnHpChanged, OnDied)
//   EnemyData.Skill.cs : 스킬 조회 (GetSkill)

using System.Collections.Generic;
using UnityEngine;

public enum EnemyTier
{
    Weak   = 0,
    Normal = 1,
    Boss   = 2,
}

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
    public string skillId;              // (구) 단일 스킬 — 호환 유지용
    public string[] skillIds;           // (신규) 다중 적 스킬 ID — BattleManager.EnemyAction 가 가중치 랜덤 선택

    [Header("시각")]
    public Sprite portrait;
    public string spritePath;

    [Tooltip("RuntimeAnimatorController 의 Resources 기준 경로. 비면 Animator 비활성.")]
    public string animatorPath;

    [Header("메모")]
    [TextArea(2, 4)]
    public string note;

    // ── 소환체/특수 적 (기획 §11 §3 보스 까마귀) ──────────────
    [Header("소환체 메커니즘 (까마귀 등)")]
    [Tooltip("hit-count 기반 처치. 0 = 비활성(HP 기반), >0 = 이만큼 맞으면 사망")]
    public int hitCountToDie = 0;

    [Tooltip("소환 후 N턴 생존. 0 = 영구, >0 = 턴 카운터")]
    public int summonLifeTurns = 0;

    /// <summary>처치 시 영혼석 드롭량. 기획 §15 보상 시스템 명세.</summary>
    public int soulstoneDrop = 0;

    [Tooltip("수명 만료 시 파티 전체에 분산되는 데미지 (1마리당)")]
    public int expirePenaltyPower = 0;

    [Tooltip("true 면 행동 페이즈에서 스킵 (까마귀처럼 공격 안 함)")]
    public bool isPassive = false;

    // ── 런타임 상태 ──────────────────────────────────────────────
    [Header("런타임 상태")]
    public bool   isDead      = false;
    public Sprite enemySprite;

    /// <summary>이번 전투에서 이미 1회 사용된 스킬 ID 집합 (조건부 강제 스킬용).</summary>
    [System.NonSerialized] public HashSet<string> usedOnceSkills = new HashSet<string>();

    /// <summary>현재까지 누적된 hit 수 (hitCountToDie > 0 일 때만 유효).</summary>
    [System.NonSerialized] public int currentHits = 0;

    /// <summary>남은 생존 턴 (summonLifeTurns > 0 일 때만 유효). 0 도달 시 만료.</summary>
    [System.NonSerialized] public int currentLifeTurns = 0;

    /// <summary>보스 상태머신용 — 까마귀 만료 후 다음 턴에 순간이동 강제 발동.</summary>
    [System.NonSerialized] public bool pendingTeleport = false;

    /// <summary>적 스킬별 남은 쿨다운 (턴). 0 또는 키 없음 = 사용 가능.</summary>
    [System.NonSerialized] private Dictionary<string, int> _skillCooldowns = new Dictionary<string, int>();

    public int GetSkillCooldown(string skillId)
    {
        if (string.IsNullOrEmpty(skillId)) return 0;
        return _skillCooldowns != null && _skillCooldowns.TryGetValue(skillId, out int v) ? v : 0;
    }

    public void StartSkillCooldown(string skillId, int turns)
    {
        if (string.IsNullOrEmpty(skillId) || turns <= 0) return;
        if (_skillCooldowns == null) _skillCooldowns = new Dictionary<string, int>();
        _skillCooldowns[skillId] = turns;
    }

    /// <summary>매 턴 종료 시 호출 — 모든 쿨다운 -1, 0 이하면 제거.</summary>
    public void TickSkillCooldowns()
    {
        if (_skillCooldowns == null || _skillCooldowns.Count == 0) return;
        var keys = new List<string>(_skillCooldowns.Keys);
        foreach (var k in keys)
        {
            int next = _skillCooldowns[k] - 1;
            if (next <= 0) _skillCooldowns.Remove(k);
            else           _skillCooldowns[k] = next;
        }
    }

    /// <summary>현재 HP 비율 (0~1). maxHp 가 0 이하면 0.</summary>
    public float HpRatio => maxHp > 0 ? (float)CurrentHp / maxHp : 0f;

    public int StackValue => attackCost;
}