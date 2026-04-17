// FellowData.cs
// 동료 런타임 상태 ScriptableObject.
// 스킬 정의: Assets/Scripts/Skill/SkillDefinition.cs 참조

using UnityEngine;

[CreateAssetMenu(menuName = "DarkHorror/FellowData", fileName = "fellow_new")]
public class FellowData : ScriptableObject
{
    [Header("정의 데이터")]
    [Tooltip("이 동료의 CompanionData SO. 스킬/역할/스탯 정의를 참조한다.")]
    public CompanionData data;

    [Header("런타임 상태")]
    public StackType positionStack;
    public Sprite fellowSprite;
    public int currentStress = 0;
    public int currentStack = 0;
    public bool isDead = false;
    [SerializeField] private int _currentHp;

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
            }
        }
    }
    // FellowData.cs — 초기화 시 슬라이더 이벤트 연결
    public void InitHp(UnityEngine.UI.Slider slider)
    {
        HpSlider = slider;
        int maxHp = data != null ? data.maxHp : 100;
        _currentHp = maxHp;

        if (HpSlider != null)
        {
            HpSlider.maxValue = maxHp;
            HpSlider.value = _currentHp;
        }

        OnHpChanged += hp => { if (HpSlider != null) HpSlider.value = hp; };
        OnDied += () => Debug.Log($"[사망] {positionStack}");
    }

    [System.NonSerialized] public System.Action<int> OnHpChanged;
    [System.NonSerialized] public System.Action OnDied;
    // ✅ 추가 — SO에 직렬화 안 하고 런타임에만 참조
    [System.NonSerialized] public UnityEngine.UI.Slider HpSlider;
}
