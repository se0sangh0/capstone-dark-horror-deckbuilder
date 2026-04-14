using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class RoleCostBase : MonoBehaviour
{
    protected abstract string OwnerPrefix { get; }

    protected virtual Dictionary<StackType, int> StartingAmounts => new()
    {
        { StackType.Dealer, 0 },
        { StackType.Tank, 0 },
        { StackType.Support, 0 }
    };

    public event Action<StackType, int> OnCostChanged;

    private readonly Dictionary<StackType, int> _costs = new();

    private string GetSaveKey(StackType role) => $"{OwnerPrefix}{role}";

    protected virtual void Start()
    {
        foreach (StackType role in Enum.GetValues(typeof(StackType)))
        {
            LoadCost(role);
            UpdateUI(role, GetAmount(role)); // 초기 UI 세팅
        }
    }

    public int GetAmount(StackType role) =>
        _costs.TryGetValue(role, out int val) ? val : 0;

    public virtual void Add(StackType role, int value)
    {
        if (value <= 0) return;
        SetAmount(role, GetAmount(role) + value);
        SaveCost(role);
    }

    public bool Use(StackType role, int value)
    {
        if (value <= 0 || GetAmount(role) < value) return false;
        SetAmount(role, GetAmount(role) - value);
        SaveCost(role);
        return true;
    }

    private void SetAmount(StackType role, int newValue)
    {
        if (GetAmount(role) == newValue) return;
        _costs[role] = newValue;
        Debug.Log($"{GetSaveKey(role)} 코스트가 {newValue}로 변경되었습니다.");
        OnCostChanged?.Invoke(role, newValue);
    }
    
    protected abstract void UpdateUI(StackType role, int newValue);

    private void SaveCost(StackType role)
    {
        PlayerPrefs.SetInt(GetSaveKey(role), GetAmount(role));
        PlayerPrefs.Save();
    }

    private void LoadCost(StackType role)
    {
        int defaultVal = StartingAmounts.GetValueOrDefault(role);
        //int defaultVal = StartingAmounts.TryGetValue(role, out int def) ? def : 0;
        _costs[role] = PlayerPrefs.GetInt(GetSaveKey(role), defaultVal);
    }
}

