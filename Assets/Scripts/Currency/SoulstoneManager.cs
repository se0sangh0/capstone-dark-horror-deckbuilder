using UnityEngine;
using TMPro;

public class SoulstoneManager : BaseCurrency
{
    [SerializeField] private TMP_Text amountText;
    public static SoulstoneManager Instance { get; private set; }

    protected override int StartingAmount => 10;

    // 부모가 요구하는 저장 키 값 정의 (여기만 다름!)
    protected override string SaveKey => "SoulStone";

    private void Awake()
    {
        OnCurrencyChanged += UpdateText;
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

    void UpdateText(int amount)
    {
        if (amountText is null) return;

        amountText.text = $"{amount:N0}";
    }
}
