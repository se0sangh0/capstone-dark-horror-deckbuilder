using TMPro;
using UnityEngine;
public class EnemyRoleCost : RoleCostBase
{
    public static EnemyRoleCost Instance;
    protected override string OwnerPrefix => "Enemy";
    [SerializeField] private TMP_Text[] _costTexts;

    void Awake()
    {
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
        for (int i = 0; i < 3; i++)
        {
            _costTexts[i].text = GetAmount((StackType)i).ToString();
        }
    }
}