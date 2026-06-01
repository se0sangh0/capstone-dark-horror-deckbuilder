// ============================================================
// Fellow/FellowData.cs
// 동료 1명의 정의 + 런타임 상태 통합 ScriptableObject
// ============================================================
//
// [통합 이력]
//   2026-05: CompanionData → FellowData 통합.
//   구 CompanionData 의 모든 정의 필드를 흡수. data 필드 제거.
//   기존 `fellow.data.xxx` 접근은 모두 `fellow.xxx` 로 평탄화됨.
//
// ── 분리된 partial 파일 ─────────────────────────────────────────
//   FellowData.Hp.cs    : HP 시스템 (CurrentHp, InitHp, OnHpChanged, OnDied)
//   FellowData.Skill.cs : 스킬 시스템 (RefreshSkillInfo, GetSkills)
// ============================================================

using UnityEngine;

// ----------------------------------------------------------
// [CompanionRole 열거형]
// 통합 전 Companion/CompanionData.cs 에 있었던 enum.
// FellowData.cs 로 이전 — CompanionData 클래스가 사라져도 살아남아야 함.
// StackType 과 인덱스 일치 (Dealer=0/Tanker=1/Support=2) → 캐스팅으로 변환.
// ----------------------------------------------------------
public enum CompanionRole
{
    Dealer  = 0,
    Tanker  = 1,
    Support = 2,
}

// ----------------------------------------------------------
// [Gender 열거형]
// 기획 §6 (이름 생성 규칙): AllyNameGenerator 가 gender 기준으로
// 남자/여자 이름 테이블에서 displayName 을 조합한다.
// ----------------------------------------------------------
public enum Gender
{
    Male   = 0,
    Female = 1,
}

[CreateAssetMenu(menuName = "DarkHorror/FellowData", fileName = "fellow_new")]
public partial class FellowData : ScriptableObject
{
    // ── 정의 데이터 (구 CompanionData 흡수) ─────────────────────
    [Header("ID / 표시")]
    [Tooltip("로직 연결용 고유 ID. 예: ally_caster_01")]
    public string id;

    [Tooltip("UI 표시 이름. 런타임 이름 생성기로 채워질 수 있음.")]
    public string displayName;

    [Header("역할 / 성향")]
    [Tooltip("역할군. 덱 구성/스택 결정에 사용.")]
    public CompanionRole role;

    [Tooltip("성향. 카드 스택 범위(0 제외) 결정.")]
    public CardAffinity affinity;

    [Tooltip("성별. AllyNameGenerator 가 이름 풀 선택에 사용. UI 표기는 미사용(기획 §6).")]
    public Gender gender;

    [Header("직업")]
    [Tooltip("직업명 — 캐스터/오펜더/디펜더/어택커/프리스트")]
    public string jobClass;

    [Header("스탯")]
    [Tooltip("최대 HP (성급 배율이 이미 반영된 최종값)")]
    public int maxHp = 100;

    [Tooltip("모집 비용 (영혼석)")]
    public int recruitCost = 30;

    [Header("스킬")]
    [Tooltip("보유 스킬 ID 목록. Resources/Data/skills.json 의 id 참조.")]
    public string[] skillIds = new string[0];

    [Header("시각")]
    public Sprite portrait;
    public string spritePath;

    [Tooltip("RuntimeAnimatorController 의 Resources 기준 경로. 비면 Animator 비활성.")]
    public string animatorPath;

    [Tooltip("모션 트리거 이름. 비면 기본값 Idle/Attack/Attack2 사용.")]
    public string idleAnim;
    public string attack1Anim;
    public string attack2Anim;

    // ── 런타임 상태 ──────────────────────────────────────────────
    [Header("런타임 상태")]
    public StackType positionStack;
    public Sprite    fellowSprite;
    // currentStress → FellowData.Stress.cs (프로퍼티 + OnStressChanged 이벤트)
    public int       stressResist    = 0;    // 모집 시 FellowDef 에서 복사
    public int       shield          = 0;    // 데미지 흡수량; HP 감소 전에 먼저 소모됨
    public bool      isDead          = false;
    public bool      isFrozen        = false;    // 공포 경직: 이번 턴 행동 불가
    public bool      isOverBreathing = false;    // 과호흡: 스킬 코스트 +1
    // 역할별 중증 디버프 (기획 §04) — 첫 패닉 시 부착, 전투 종료까지 유지 (InitBattle 에서 리셋).
    //   딜러: 받는 피해 +30% / 탱커: 부여 실드 -50% / 서포터: 광역힐→단일힐 강제
    public bool      hasSevereDebuff = false;

    // 거합 집중 (오펜더 시그니처 패시브, 기획 §16) — 전투 한정 콤보 상태. InitBattle 에서 리셋.
    [System.NonSerialized] public int comboTargetIid = 0; // 직전 단일 공격 대상 적 InstanceID (0=없음)
    [System.NonSerialized] public int comboStacks    = 0; // 동일 대상 연속 타격 수

    // 메타 패시브 (기획 §16) — 런 시작 시 해금된 풀에서 무작위 1개 배정. 런 내내 유지.
    //   null = 미배정(해금된 패시브 없음). InitBattle 에서 미배정이면 RollPassive 로 배정.
    [System.NonSerialized] public string activePassiveId = null;

    /// <summary>
    /// PartyEditPanel 배치 순번 (0~3). 0=맨앞, 3=맨뒤 — 행 구분 없음.
    /// 기획 §02 §피격 순서 = 배치 순서. 사망 시 빈 자리는 뒤에서 앞으로 압축.
    /// 사망/노드 진행과 무관하게 영구 보존. PartyManager.GetActiveFellows 가 stamp.
    /// 전투 시각 배치(DefaultSetting / RelayoutCards) 가 이 값으로 슬롯 위치 결정.
    /// </summary>
    [System.NonSerialized] public int battleSlotIndex = -1;

    // ── 성급 시스템 (기획 백로그 §5 — TODO[L] 자리 PartyManager 참고) ──
    // 1★ → ×1.00 / 2★ → ×1.40(hp) ×1.25(power) / 3★ → ×1.96 ×1.5625
    // CreateRuntimeFellow 에서 일괄 계산 → maxHp 와 skillPowerMultiplier 에 반영.
    public int   starLevel            = 1;
    public float skillPowerMultiplier = 1f;
    public float hpMultiplier         = 1f;   // 정보용 — maxHp 에 이미 곱해져 있음

    // ── 헬퍼 ────────────────────────────────────────────────────
    /// <summary>성향 한글 라벨 — UI 동료 슬롯 표시용.</summary>
    public string AffinityLabel => AffinityHelper.GetLabel(affinity);

    /// <summary>성향별 UI 색상 — 동료 슬롯 테두리.</summary>
    public Color AffinityColor => AffinityHelper.GetColor(affinity);
}
