// ============================================================
// fellow/FellowData.cs
// 동료 런타임 상태 ScriptableObject
// ============================================================
//
// [이 파일이 하는 일]
//   전투 중 동료 1명의 "변하는 상태" 를 저장합니다.
//   (현재 HP, 스트레스, 스택, 사망 여부, 배정된 스킬 등)
//
// [HP 시스템]
//   CurrentHp 프로퍼티를 통해 HP 를 변경하면:
//   1. 0~maxHp 범위로 자동 제한 (Clamp)
//   2. OnHpChanged 이벤트 발생 → UI 슬라이더 자동 업데이트
//   3. HP 가 0 이 되면 OnDied 이벤트 발생 → 사망 처리
//
// [스킬 배정 확인 방법 (하이어라키에서 확인)]
//   전투 씬 플레이 중 Hierarchy → BattleManager 오브젝트 선택
//   → Inspector → Allies 리스트 확장 → FellowData 확장
//   → "배정된 스킬 정보" 섹션에서 스킬 이름/설명 확인 가능
//
// [스킬 배정 흐름]
//   BattleManager.InitBattle()
//   → SkillDatabase.AssignRandomSkills(역할, 2) 로 스킬 2개 선택
//   → companion.skillIds 에 저장
//   → RefreshSkillInfo() 로 Inspector 표시 갱신
//
// [어디서 쓰이나요?]
//   - BattleManager.cs : 전투 중 아군 상태 관리, 스킬 배정
//   - DefaultSetting.cs : BattleManager.Instance.allies[i] 로 직접 접근
// ============================================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 동료 1명의 런타임 상태 ScriptableObject.
/// 에셋 생성: Assets 우클릭 → Create → DarkHorror/FellowData
/// </summary>
[CreateAssetMenu(menuName = "DarkHorror/FellowData", fileName = "fellow_new")]
public class FellowData : ScriptableObject
{
    // ----------------------------------------------------------
    // [정의 데이터]
    // ----------------------------------------------------------
    [Header("정의 데이터")]
    [Tooltip("이 동료의 CompanionData SO. 스킬/역할/스탯 정의를 참조합니다.")]
    public CompanionData data;

    // ----------------------------------------------------------
    // [런타임 상태]
    // ----------------------------------------------------------
    [Header("런타임 상태")]
    [Tooltip("이 동료의 역할 스택 타입 (Dealer / Tank / Support)")]
    public StackType positionStack;

    [Tooltip("동료 카드에 표시될 스프라이트")]
    public Sprite fellowSprite;

    [Tooltip("현재 스트레스 수치")]
    public int currentStress = 0;

    [Tooltip("이월 스택 보너스 (스택 부족으로 스킵 시 +1)")]
    public int currentStack = 0;

    [Tooltip("사망 여부")]
    public bool isDead = false;

    // ----------------------------------------------------------
    // [배정된 스킬 정보 — 하이어라키 Inspector 확인용]
    //
    // 전투 시작 시 BattleManager.InitBattle() 에서 스킬이 배정되면
    // RefreshSkillInfo() 가 호출되어 아래 텍스트에 채워집니다.
    //
    // 확인 방법:
    //   플레이 중 Hierarchy → BattleManager → Inspector
    //   → Allies → 각 FellowData 확장 → 이 섹션 확인
    // ----------------------------------------------------------
    [Header("배정된 스킬 정보 (플레이 중 Inspector 에서 확인)")]

    [Tooltip("배정된 스킬 ID 목록 (읽기 전용 — 자동 채워짐)")]
    [SerializeField] private string[] _assignedSkillIds = new string[0];

    [Tooltip("배정된 스킬 요약 (읽기 전용 — 자동 채워짐)\n형식: [스킬명] 설명")]
    [SerializeField, TextArea(3, 8)]
    private string _skillSummary = "(전투 시작 전 — 스킬이 아직 배정되지 않았습니다)";

    // ----------------------------------------------------------
    // [CurrentHp 프로퍼티]
    // ----------------------------------------------------------
    [Header("HP")]
    [SerializeField]
    [Tooltip("현재 HP")]
    private int _currentHp;

    /// <summary>
    /// 현재 HP.
    /// set 시 Clamp → OnHpChanged 이벤트 → 사망 처리 자동 수행.
    /// </summary>
    public int CurrentHp
    {
        get => _currentHp;
        set
        {
            int maxHp = data != null ? data.maxHp : 100;
            _currentHp = Mathf.Clamp(value, 0, maxHp);
            OnHpChanged?.Invoke(_currentHp);

            if (_currentHp <= 0 && !isDead)
            {
                isDead = true;
                OnDied?.Invoke();
                Debug.Log($"[FellowData] {data?.displayName ?? positionStack.ToString()} 사망!");
            }
        }
    }

    // ----------------------------------------------------------
    // HP 슬라이더 초기화
    // ----------------------------------------------------------

    /// <summary>
    /// HP 슬라이더를 연결하고 초기화한다.
    /// DefaultSetting.cs 의 SpawnCard() 에서 호출됩니다.
    /// </summary>
    public void InitHp(UnityEngine.UI.Slider slider)
    {
        HpSlider = slider;
        int maxHp = data != null ? data.maxHp : 100;
        _currentHp = maxHp;

        if (HpSlider != null)
        {
            HpSlider.maxValue = maxHp;
            HpSlider.value    = _currentHp;
        }

        OnHpChanged += hp => { if (HpSlider != null) HpSlider.value = hp; };
        OnDied      += () => Debug.Log($"[FellowData] {data?.displayName ?? positionStack.ToString()} 사망 이벤트 발생");
    }

    // ----------------------------------------------------------
    // 스킬 배정 정보 갱신 (Inspector 표시 + 내부 캐시)
    // BattleManager.InitBattle() 에서 스킬 배정 후 호출됩니다.
    // ----------------------------------------------------------

    /// <summary>
    /// 스킬 배정 후 Inspector 표시를 갱신한다.
    /// BattleManager.InitBattle() 에서 companion.skillIds 설정 직후 호출됩니다.
    /// </summary>
    public void RefreshSkillInfo()
    {
        if (data == null || data.skillIds == null || data.skillIds.Length == 0)
        {
            _assignedSkillIds = new string[0];
            _skillSummary     = "(배정된 스킬 없음)";
            return;
        }

        // ID 목록 캐시
        _assignedSkillIds = data.skillIds.ToArray();

        // 스킬 요약 텍스트 생성
        if (SkillDatabase.Instance == null)
        {
            _skillSummary = $"스킬 ID: {string.Join(", ", data.skillIds)}\n(SkillDatabase 없음 — 씬에 추가 필요)";
            return;
        }

        var lines = new List<string>();
        for (int i = 0; i < data.skillIds.Length; i++)
        {
            var skill = SkillDatabase.Instance.GetSkill(data.skillIds[i]);
            if (skill != null)
            {
                // 두 번째 스킬은 "(주석처리됨)" 표시
                string activeTag = (i == 0) ? "[활성]" : "[비활성-테스트용]";
                lines.Add($"{activeTag} 스킬{i + 1}: {skill.displayName}");
                lines.Add($"  효과: {skill.effectType} | 대상: {skill.targeting} | 파워: {skill.power}");
                lines.Add($"  설명: {skill.description}");
            }
            else
            {
                lines.Add($"  스킬{i + 1}: ID '{data.skillIds[i]}' 를 찾을 수 없음");
            }
        }

        _skillSummary = string.Join("\n", lines);
        Debug.Log($"[FellowData] {data.displayName} 스킬 정보 갱신:\n{_skillSummary}");
    }

    // ----------------------------------------------------------
    // 스킬 조회
    // ----------------------------------------------------------

    /// <summary>
    /// 이 동료가 보유한 스킬 데이터 목록을 반환한다.
    /// SkillDatabase 가 없거나 skillIds 가 비어있으면 빈 목록 반환.
    /// </summary>
    public List<SkillData> GetSkills()
    {
        var result = new List<SkillData>();

        if (data == null || data.skillIds == null || data.skillIds.Length == 0)
            return result;

        if (SkillDatabase.Instance == null)
        {
            Debug.LogWarning("[FellowData] SkillDatabase 를 찾을 수 없습니다.");
            return result;
        }

        foreach (var id in data.skillIds)
        {
            var skill = SkillDatabase.Instance.GetSkill(id);
            if (skill != null) result.Add(skill);
        }

        return result;
    }

    // ----------------------------------------------------------
    // [NonSerialized] 런타임 전용 (Inspector 저장 안 됨)
    // ----------------------------------------------------------

    /// <summary>HP 변경 시 발생 이벤트</summary>
    [System.NonSerialized] public System.Action<int> OnHpChanged;

    /// <summary>사망 시 발생 이벤트</summary>
    [System.NonSerialized] public System.Action OnDied;

    /// <summary>연결된 HP 슬라이더</summary>
    [System.NonSerialized] public UnityEngine.UI.Slider HpSlider;
}
