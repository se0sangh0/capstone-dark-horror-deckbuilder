using TMPro;
using UnityEngine;
public class EnemyRoleCost : RoleCostBase
{
    protected override string OwnerPrefix => "Enemy";
    [SerializeField] private TMP_Text[] _costTexts;
    protected override void UpdateUI(StackType role, int newValue)
    {
        int index = (int)role;
        if (_costTexts == null || index >= _costTexts.Length || _costTexts[index] == null)
        {
            Debug.LogWarning($"PlayerRoleCost: {role}에 대한 TMP_Text가 할당되지 않았습니다.");
            return;
        }
        _costTexts[index].text = newValue.ToString();
    }

}