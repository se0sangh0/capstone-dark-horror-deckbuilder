using System;
using TMPro;
using UnityEngine;

public class PlayerRoleCost : RoleCostBase
{
    public static PlayerRoleCost Instance;
    protected override string OwnerPrefix => "Player";
    [SerializeField] private TMP_Text[] _costTexts;

    void Awake()
    {
        OnCostChanged += UpdateUI;
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    protected override void UpdateUI()
    {
        // int index = (int)role;
        // if (_costTexts == null || index >= _costTexts.Length || _costTexts[index] == null)
        // {
        //     Debug.LogWarning($"PlayerRoleCost: {role}에 대한 TMP_Text가 할당되지 않았습니다.");
        //     return;
        // }

        for (int i = 0; i < 3; i++)
        {
            _costTexts[i].text = GetAmount((StackType)i).ToString();
        }
        // _costTexts[index].text = newValue.ToString();
        // Debug.Log("value: " + _costTexts[index].text);
    }

}