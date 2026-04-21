// FellowData.Hp.cs
// FellowData (partial) — HP 시스템 담당.
//
// ── 역할 ────────────────────────────────────────────────────────
//   FellowData.cs 에서 HP 관련 코드를 분리한 파일입니다.
//   partial class 이므로 FellowData 와 동일한 타입으로 컴파일됩니다.
//
// ── HP 시스템 ──────────────────────────────────────────────────
//   CurrentHp 프로퍼티로 HP 변경 시:
//   1. 0~maxHp 자동 Clamp
//   2. OnHpChanged 이벤트 → UI 슬라이더 자동 업데이트
//   3. HP 0 → OnDied 이벤트 → 사망 처리
//
// ── 호출 위치 ──────────────────────────────────────────────────
//   InitHp()         : DefaultSetting.cs 의 SpawnCard() 에서 호출
//   CurrentHp setter : BattleManager 의 ApplyDamageToAlly / 각 SkillEffect 구현체에서 호출

using UnityEngine;

public partial class FellowData
{
    // ── HP 필드 ──────────────────────────────────────────────────
    [Header("HP")]
    [SerializeField] private int _currentHp;

    // ── CurrentHp 프로퍼티 ───────────────────────────────────────
    public int CurrentHp
    {
        get => _currentHp;
        set
        {
            int maxHp  = data != null ? data.maxHp : 100;
            _currentHp = Mathf.Clamp(value, 0, maxHp);
            OnHpChanged?.Invoke(_currentHp);

            if (_currentHp <= 0 && !isDead)
            {
                isDead = true;
                OnDied?.Invoke();
            }
        }
    }

    // ── HP 슬라이더 초기화 ───────────────────────────────────────
    /// <summary>
    /// HP 슬라이더를 연결하고 초기값을 설정한다.
    /// DefaultSetting.cs 의 SpawnCard() 에서 호출됩니다.
    /// </summary>
    public void InitHp(UnityEngine.UI.Slider slider)
    {
        HpSlider = slider;
        int maxHp = data != null ? data.maxHp : 100;

        // SO 에셋에서 미리 설정된 값이 있으면 유지, 없으면(0 이하) maxHp로 초기화
        if (_currentHp <= 0)
            _currentHp = maxHp;
        else
            _currentHp = Mathf.Clamp(_currentHp, 1, maxHp);

        if (HpSlider != null)
        {
            HpSlider.maxValue = maxHp;
            HpSlider.value    = _currentHp;
        }

        OnHpChanged += hp => { if (HpSlider != null) HpSlider.value = hp; };
        OnDied      += () => Debug.Log($"[사망] {data?.displayName ?? positionStack.ToString()}");
    }

    // ── 실드 ──────────────────────────────────────────────────────
    /// <summary>실드를 추가하고 OnShieldChanged 이벤트를 발생시킨다.</summary>
    public void AddShield(int amount)
    {
        shield += amount;
        OnShieldChanged?.Invoke();
    }

    // ── 런타임 전용 (NonSerialized) ──────────────────────────────
    [System.NonSerialized] public System.Action<int>        OnHpChanged;
    [System.NonSerialized] public System.Action             OnDied;
    [System.NonSerialized] public System.Action             OnShieldChanged;
    [System.NonSerialized] public UnityEngine.UI.Slider     HpSlider;
}
