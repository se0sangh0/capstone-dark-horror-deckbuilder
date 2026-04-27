// FellowData.cs
// 동료 런타임 상태 ScriptableObject — 기본 정의.
//
// ── 분리된 partial 파일 ─────────────────────────────────────────
//   FellowData.Hp.cs    : HP 시스템 (CurrentHp, InitHp, OnHpChanged, OnDied)
//   FellowData.Skill.cs : 스킬 시스템 (RefreshSkillInfo, GetSkills)

using UnityEngine;

[CreateAssetMenu(menuName = "DarkHorror/FellowData", fileName = "fellow_new")]
public partial class FellowData : ScriptableObject
{
    // ── 정의 데이터 ──────────────────────────────────────────────
    [Header("정의 데이터")]
    [Tooltip("이 동료의 CompanionData SO. 스킬/역할/스탯 정의를 참조한다.")]
    public CompanionData data;

    // ── 런타임 상태 ──────────────────────────────────────────────
    [Header("런타임 상태")]
    public StackType positionStack;
    public Sprite    fellowSprite;
    public int       currentStress   = 0;
    public int       stressResist    = 0;    // data.stressResist 에서 InitBattle 시 복사
    public int       shield          = 0;    // 데미지 흡수량; HP 감소 전에 먼저 소모됨
    public bool      isDead          = false;
    public bool      isFrozen        = false;    // 공포 경직: 이번 턴 행동 불가
    public bool      isOverBreathing = false;    // 과호흡: 스킬 코스트 +1

    // ── [강화 시스템 TODO] ─────────────────────────────────────────
    // ★ 성급 (1 / 2 / 3). BattleManager.InitBattle() 에서 data.starLevel 로 동기화.
    //
    // maxHp 배율:
    //   FellowDatabase.CreateCompanionData() 에서 이미 성급 반영된 값을 data.maxHp 에 저장.
    //   UpgradeStar() 에서 승급 시 data.maxHp 를 재계산.
    //   InitBattle 은 data.maxHp 를 그대로 읽어 CurrentHp 에 할당 (이중 스케일 금지).
    //
    // 스킬 파워 배율 (UseSkill 구현 시):
    //   int scaledPower = Mathf.RoundToInt(skill.power * skillPowerMultiplier);
    //   → SkillData SO 는 공유 객체이므로 skill.power 는 수정하지 말 것.
    public int   starLevel            = 1;    // 런타임 성급 (data.starLevel 에서 초기화)
    public float skillPowerMultiplier = 1f;   // 성급 스킬 파워 배율 (InitBattle 에서 계산)
}
