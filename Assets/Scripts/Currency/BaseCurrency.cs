// ============================================================
// Currency/BaseCurrency.cs
// 재화(화폐) 제네릭 베이스 클래스
// ============================================================
//
// [이 파일이 하는 일]
//   게임 안의 재화(돈, 아이템 수량 등)를 관리하는
//   공통 로직이 담긴 베이스 클래스입니다.
//
//   마나석(ManastoneManager), 영혼석(SoulstoneManager) 이
//   이 클래스를 상속받아 각자의 재화를 관리합니다.
//
// [제네릭 CRTP 패턴]
//   BaseCurrency<T> 에서 T 는 자기 자신의 타입입니다.
//   예: ManastoneManager : BaseCurrency<ManastoneManager>
//   이렇게 하면 ManastoneManager.Instance 와
//   SoulstoneManager.Instance 가 독립된 전역 참조로 만들어집니다.
//
// [어디서 쓰이나요?]
//   - Currency/ManastoneManager.cs : 마나석 관리
//   - Currency/SoulstoneManager.cs : 영혼석 관리
//
// [PlayerPrefs 저장]
//   재화 값은 PlayerPrefs 에 저장됩니다.
//   저장 키: 자식 클래스의 SaveKey 프로퍼티 값 (예: "ManaStone")
// ============================================================

using System;
using UnityEngine;

/// <summary>
/// 재화 관리 제네릭 베이스 클래스 (CRTP 패턴).
/// T 에 자신의 타입을 지정하면 Instance 프로퍼티가 자동 제공된다.
/// </summary>
/// <typeparam name="T">상속 클래스 자신의 타입</typeparam>
public abstract class BaseCurrency<T> : MonoBehaviour where T : BaseCurrency<T>
{
    // ----------------------------------------------------------
    // [Instance] — 전역 싱글톤 참조
    // ManastoneManager.Instance, SoulstoneManager.Instance 로 접근 가능
    // ----------------------------------------------------------
    public static T Instance { get; private set; }

    // ----------------------------------------------------------
    // [SaveKey] — PlayerPrefs 저장 키 (자식 클래스가 구현)
    // ManastoneManager → "ManaStone"
    // SoulstoneManager → "SoulStone"
    // ----------------------------------------------------------
    protected abstract string SaveKey { get; }

    // ----------------------------------------------------------
    // [OnCurrencyChanged] — 재화 값이 바뀔 때마다 발생하는 이벤트
    // 매개변수: 새로운 재화 값 (int)
    // ----------------------------------------------------------
    public event Action<int> OnCurrencyChanged;

    // ----------------------------------------------------------
    // [StartingAmount] — 게임 시작 시 기본 재화량
    // 자식 클래스에서 override 가능 (기본: 0)
    // ----------------------------------------------------------
    protected virtual int StartingAmount => 0;

    // ----------------------------------------------------------
    // [Amount] — 현재 재화 값 프로퍼티
    // set 시 이벤트 자동 발생
    // ----------------------------------------------------------
    private int _amount;

    public int Amount
    {
        get => _amount;
        protected set
        {
            if (_amount != value)
            {
                _amount = value;
                Debug.Log($"[{SaveKey}] 재화 변경 → {_amount}");
                OnCurrencyChanged?.Invoke(_amount);
            }
        }
    }

    // ----------------------------------------------------------
    // Awake — 싱글톤 등록 + UI 이벤트 구독
    // ----------------------------------------------------------
    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            DontDestroyOnLoad(gameObject);

            // UI 텍스트 업데이트 이벤트 구독
            OnCurrencyChanged += UpdateText;
        }
        else
        {
            // 중복 인스턴스 제거
            Destroy(gameObject);
        }
    }

    // ----------------------------------------------------------
    // Start — 초기화: PlayerPrefs 에서 값 로드
    // ----------------------------------------------------------
    protected virtual void Start()
    {
        LoadCurrency();
    }

    // ----------------------------------------------------------
    // 공개 API
    // ----------------------------------------------------------

    /// <summary>재화를 value 만큼 추가한다. (value > 0 일 때만 동작)</summary>
    public virtual void Add(int value)
    {
        if (value > 0)
        {
            Amount += value;
            SaveCurrency();
        }
    }

    /// <summary>
    /// 재화를 value 만큼 소비한다.
    /// 재화가 충분하면 true, 부족하면 false 를 반환한다.
    /// </summary>
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

    // ----------------------------------------------------------
    // [UpdateText] — 자식 클래스가 구현 (재화 변경 시 UI 텍스트 갱신)
    // ----------------------------------------------------------
    protected abstract void UpdateText(int amount);

    // ----------------------------------------------------------
    // 저장/로드 (PlayerPrefs)
    // ----------------------------------------------------------

    protected void SaveCurrency()
    {
        PlayerPrefs.SetInt(SaveKey, Amount);
        PlayerPrefs.Save();
    }

    private void LoadCurrency()
    {
        Amount = PlayerPrefs.GetInt(SaveKey, StartingAmount);
    }

    // ----------------------------------------------------------
    // [ContextMenu] 무결성 테스트
    // ----------------------------------------------------------

    /// <summary>[에디터 테스트] 현재 재화 값을 콘솔에 출력한다.</summary>
    [ContextMenu("TEST / 현재 재화 값 출력")]
    private void TestPrintAmount()
    {
        Debug.Log($"[{SaveKey}] 현재 재화: {Amount}");
    }

    /// <summary>[에디터 테스트] 재화 +10 추가 테스트.</summary>
    [ContextMenu("TEST / 재화 +10 추가")]
    private void TestAdd10()
    {
        Add(10);
        Debug.Log($"[{SaveKey}] 10 추가 후 재화: {Amount}");
    }
}
