// EnemyData.Hp.cs
// EnemyData (partial) — HP 시스템 담당.
//
// ── 역할 ────────────────────────────────────────────────────────
//   EnemyData.cs 에서 HP 관련 코드를 분리한 파일입니다.
//   기존 EnemyRuntime.cs 의 TakeDamage / Die 로직을 흡수합니다.
//
// ── HP 시스템 ──────────────────────────────────────────────────
//   CurrentHp 프로퍼티로 HP 변경 시:
//   1. 0~maxHp 자동 Clamp
//   2. OnHpChanged 이벤트 → UI 슬라이더 자동 업데이트
//   3. HP 0 → OnDied 이벤트 → 사망 처리
//
// ── 호출 위치 ──────────────────────────────────────────────────
//   InitHp()          : BattleManager 또는 EnemySpawner 의 적 생성 시 호출
//   TakeDamage()      : BattleManager 의 ApplyDamageToEnemy 에서 호출
//   CurrentHp setter  : 직접 회복/감소가 필요한 경우 사용

using UnityEngine;

public partial class EnemyData
{
    // ── HP 필드 ──────────────────────────────────────────────────
    [Header("HP")]
    [System.NonSerialized] private int _currentHp;

    // ── CurrentHp 프로퍼티 ───────────────────────────────────────
    public int CurrentHp
    {
        get => _currentHp;
        set
        {
            _currentHp = Mathf.Clamp(value, 0, maxHp);
            OnHpChanged?.Invoke(_currentHp);

            if (_currentHp <= 0 && !isDead)
            {
                isDead = true;
                OnDied?.Invoke();
            }
        }
    }

    // ── HP 초기화 ────────────────────────────────────────────────
    /// <summary>
    /// HP 슬라이더를 연결하고 초기 상태로 설정한다.
    /// BattleManager 또는 EnemySpawner 의 적 스폰 시 호출됩니다.
    /// slider 가 없는 경우 null 전달 가능.
    /// </summary>
    public void InitHp(UnityEngine.UI.Slider slider = null)
    {
        HpSlider   = slider;
        _currentHp = maxHp;
        isDead     = false;

        if (HpSlider != null)
        {
            HpSlider.maxValue = maxHp;
            HpSlider.value    = _currentHp;
        }

        OnHpChanged = null;
        OnDied      = null;

        OnHpChanged += hp => { if (HpSlider != null) HpSlider.value = hp; };
        OnDied      += () => Debug.Log($"[적 사망] {displayName} (tier: {tier})");
    }

    // ── 데미지 처리 ──────────────────────────────────────────────
    /// <summary>
    /// 기존 EnemyRuntime.TakeDamage() 를 대체한다.
    /// isDead 상태에서는 무시됩니다.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (isDead) return;
        CurrentHp -= amount;
    }

    // ── 런타임 전용 (NonSerialized) ──────────────────────────────
    [System.NonSerialized] public System.Action<int>    OnHpChanged;
    [System.NonSerialized] public System.Action         OnDied;
    [System.NonSerialized] public UnityEngine.UI.Slider HpSlider;
}
