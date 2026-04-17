// ============================================================
// Stack/RoleCostBase.cs
// 역할별 스택 코스트 제네릭 베이스 클래스
// ============================================================
//
// [이 파일이 하는 일]
//   딜러/탱커/서포터 의 "스택(점수)" 을 관리하는
//   공통 로직이 담긴 베이스 클래스입니다.
//
//   아군(PlayerRoleCost) 과 적군(EnemyRoleCost) 이 이 클래스를
//   상속받아 각자의 스택을 따로 관리합니다.
//
// [제네릭 CRTP 패턴이란?]
//   RoleCostBase<T> 에서 T 는 자기 자신의 타입입니다.
//   예: PlayerRoleCost : RoleCostBase<PlayerRoleCost>
//   이렇게 하면 PlayerRoleCost.Instance, EnemyRoleCost.Instance 가
//   각각 독립된 전역 참조로 만들어집니다.
//
// [어디서 쓰이나요?]
//   - Stack/PlayerRoleCost.cs : 아군 스택 관리
//   - Stack/EnemyRoleCost.cs : 적군 스택 관리
//   - BattleManager.cs : GetAmount(), Use(), SetAmount() 호출
//   - GameManager.cs : OnCardUsed 에서 Add() 호출
//
// [PlayerPrefs 저장]
//   스택 값은 PlayerPrefs 에 저장됩니다.
//   저장 키: "{OwnerPrefix}{StackType}" 형식 (예: "PlayerDealer")
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 역할별 스택 코스트 관리 제네릭 베이스 클래스 (CRTP 패턴).
/// T 에 자신의 타입을 지정하면 Instance 프로퍼티가 자동 제공된다.
/// </summary>
/// <typeparam name="T">상속 클래스 자신의 타입</typeparam>
public abstract class RoleCostBase<T> : MonoBehaviour where T : RoleCostBase<T>
{
    // ----------------------------------------------------------
    // [Instance] — 전역 싱글톤 참조
    // PlayerRoleCost.Instance, EnemyRoleCost.Instance 로 접근 가능
    // ----------------------------------------------------------
    public static T Instance { get; private set; }

    // ----------------------------------------------------------
    // [OwnerPrefix] — 저장 키 접두사 (자식 클래스가 구현)
    // PlayerRoleCost → "Player", EnemyRoleCost → "Enemy"
    // ----------------------------------------------------------
    protected abstract string OwnerPrefix { get; }

    // ----------------------------------------------------------
    // [StartingAmounts] — 역할별 초기 스택 값
    // 자식 클래스에서 override 가능 (기본: 전부 0)
    // ----------------------------------------------------------
    protected virtual Dictionary<StackType, int> StartingAmounts => new()
    {
        { StackType.Dealer,  0 },
        { StackType.Tank,    0 },
        { StackType.Support, 0 }
    };

    // ----------------------------------------------------------
    // [OnCostChanged] — 스택 값이 바뀔 때마다 발생하는 이벤트
    // UI 업데이트 함수가 이 이벤트를 구독합니다.
    // ----------------------------------------------------------
    public event Action OnCostChanged;

    // ----------------------------------------------------------
    // [내부 저장소] — StackType → 현재 스택 값 딕셔너리
    // ----------------------------------------------------------
    private readonly Dictionary<StackType, int> _costs = new();

    /// <summary>PlayerPrefs 저장 키 생성 헬퍼</summary>
    private string GetSaveKey(StackType role) => $"{OwnerPrefix}{role}";

    // ----------------------------------------------------------
    // Awake — 싱글톤 등록 + UI 이벤트 구독
    // ----------------------------------------------------------
    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            DontDestroyOnLoad(gameObject);

            // UI 업데이트 이벤트 구독 (스택 바뀔 때마다 자동 호출)
            OnCostChanged += UpdateUI;
        }
        else
        {
            // 중복 인스턴스 제거
            Destroy(gameObject);
        }
    }

    // ----------------------------------------------------------
    // Start — 초기화: PlayerPrefs 에서 값 로드 + UI 세팅
    // ----------------------------------------------------------
    protected virtual void Start()
    {
        foreach (StackType role in Enum.GetValues(typeof(StackType)))
        {
            // TODO: 아래 줄은 테스트 코드 — 실제 게임에서는 삭제하세요!
            PlayerPrefs.SetInt(GetSaveKey(role), 0);
            LoadCost(role);
        }

        // 초기 UI 반영
        UpdateUI();
    }

    // ----------------------------------------------------------
    // 공개 API
    // ----------------------------------------------------------

    /// <summary>역할의 현재 스택 값을 반환한다.</summary>
    public int GetAmount(StackType role) =>
        _costs.TryGetValue(role, out int val) ? val : 0;

    /// <summary>
    /// 역할의 스택을 value 만큼 더한다.
    /// 음수로 내려가면 0 으로 고정된다.
    /// </summary>
    public virtual void Add(StackType role, int value)
    {
        int newAmount = GetAmount(role) + value;
        SetAmount(role, Mathf.Max(0, newAmount));
        SaveCost(role);
    }

    /// <summary>
    /// 역할의 스택을 value 만큼 소비한다.
    /// 스택이 부족하면 false 를 반환하고 소비하지 않는다.
    /// </summary>
    public bool Use(StackType role, int value)
    {
        if (value <= 0 || GetAmount(role) < value) return false;
        SetAmount(role, GetAmount(role) - value);
        SaveCost(role);
        return true;
    }

    /// <summary>
    /// 역할의 스택을 newValue 로 직접 설정한다.
    /// 값이 달라질 때만 이벤트를 발생시킨다.
    /// </summary>
    public void SetAmount(StackType role, int newValue)
    {
        if (GetAmount(role) == newValue) return;
        _costs[role] = newValue;
        Debug.Log($"[{OwnerPrefix}RoleCost] {role} 스택 → {newValue}");
        OnCostChanged?.Invoke();
    }

    // ----------------------------------------------------------
    // [UpdateUI] — 자식 클래스가 구현 (스택 변경 시 UI 갱신)
    // ----------------------------------------------------------
    protected abstract void UpdateUI();

    // ----------------------------------------------------------
    // 저장/로드 (PlayerPrefs)
    // ----------------------------------------------------------

    private void SaveCost(StackType role)
    {
        PlayerPrefs.SetInt(GetSaveKey(role), GetAmount(role));
        PlayerPrefs.Save();
    }

    private void LoadCost(StackType role)
    {
        int defaultVal = StartingAmounts.GetValueOrDefault(role);
        _costs[role] = PlayerPrefs.GetInt(GetSaveKey(role), defaultVal);
    }

    // ----------------------------------------------------------
    // [ContextMenu] 무결성 테스트
    // ----------------------------------------------------------

    /// <summary>[에디터 테스트] 현재 역할별 스택 값을 콘솔에 출력한다.</summary>
    [ContextMenu("TEST / 현재 스택 값 출력")]
    private void TestPrintCosts()
    {
        Debug.Log($"[{OwnerPrefix}RoleCost] 현재 스택 값:");
        foreach (StackType role in Enum.GetValues(typeof(StackType)))
            Debug.Log($"  {role}: {GetAmount(role)}");
    }
}
