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
    public int currentHp = 100;
    public int currentStress = 0;
    public int currentStack = 0;
    public bool isDead = false;
    
    // ✅ 추가 — SO에 직렬화 안 하고 런타임에만 참조
    [System.NonSerialized] public UnityEngine.UI.Slider HpSlider;
}
