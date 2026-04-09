using System;
using UnityEngine;

public abstract class BaseCurrency : MonoBehaviour
{
    // 자식들이 구현해야 하는 저장용 키 값 (예: "PlayerCoins", "PlayerBalls")
    protected abstract string SaveKey { get; }

    // 공통 이벤트
    public event Action<int> OnCurrencyChanged;
    
    private int _amount;

    [Header("Settings")]
    protected virtual int StartingAmount => 0;
    
    // 공통 프로퍼티
    public int Amount
    {
        get { return _amount; }
        protected set // 자식도 값을 바꿀 수 있어야 하므로 private -> protected로 변경
        {
            if (_amount != value)
            {
                _amount = value;
                Debug.Log($"{SaveKey} 재화가 {_amount}로 변경되었습니다.");
                OnCurrencyChanged?.Invoke(_amount);
            }
        }
    }

    // 초기화 시 데이터 로드
    protected virtual void Start()
    {
        LoadCurrency();
    }

    // 공통 메서드: 추가
    public virtual void Add(int value)
    {
        if (value > 0)
        {
            Amount += value;
            SaveCurrency();
        }
    }

    // 공통 메서드: 사용
    public bool Use(int value)
    {
        if (value > 0 && Amount >= value)
        {
            Amount -= value;
            SaveCurrency();
            return true;
        }
        return false;
    }

    // 공통 메서드: 저장 (SaveKey를 이용해 동적으로 저장)
    protected void SaveCurrency()
    {
        PlayerPrefs.SetInt(SaveKey, Amount);
        PlayerPrefs.Save();
    }

    // 공통 메서드: 로드
    private void LoadCurrency()
    {
        Amount = PlayerPrefs.GetInt(SaveKey, StartingAmount);
    }
}
