// PartyManager.cs
// 동료 모집/사망/파티 관리 싱글톤
// 씬 전환에도 유지 (DontDestroyOnLoad)

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance { get; private set; }

    // 현재 살아있는 동료 목록
    private List<CompanionData> _activeCompanions = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 테스트용 초기 파티 (나중에 모집 시스템으로 교체)
        InitDefaultParty();
    }

    // -------------------------------------------------------
    // 공개 API
    // -------------------------------------------------------

    /// 현재 살아있는 동료 목록 반환
    public List<CompanionData> GetActiveCompanions()
        => _activeCompanions.ToList(); // 복사본 반환 (외부 변조 방지)

    /// 동료 모집 (모집 이벤트에서 호출)
    public void RecruitCompanion(CompanionData companion)
    {
        if (_activeCompanions.Contains(companion)) return;
        _activeCompanions.Add(companion);
        Debug.Log($"[PartyManager] 동료 합류: {companion.displayName}");
    }

    /// 동료 사망 (BattleManager에서 호출)
    public void RemoveCompanion(CompanionData companion)
    {
        _activeCompanions.Remove(companion);
        Debug.Log($"[PartyManager] 동료 사망/이탈: {companion.displayName} | 잔여: {_activeCompanions.Count}명");
    }

    public int CompanionCount => _activeCompanions.Count;

    // -------------------------------------------------------
    // 테스트용 기본 파티 생성 (SO 없이 런타임 생성)
    // 나중에 모집 시스템 붙으면 이 메서드 제거하고 RecruitCompanion()으로 교체
    // -------------------------------------------------------
    private void InitDefaultParty()
    {
        var defaultMembers = new[]
        {
            ("딜러A",   CompanionRole.Dealer,  CardAffinity.Gambler,     "Characters/test_allies_dealer"),
            ("탱커A",   CompanionRole.Tanker,  CardAffinity.Safety,      "Characters/test_allies_tank"),
            ("딜러B",   CompanionRole.Dealer,  CardAffinity.Opportunist, "Characters/test_allies_dealer"),
            ("서포터A", CompanionRole.Support, CardAffinity.Optimist,    "Characters/test_allies_support"),
        };

        foreach (var (name, role, affinity, path) in defaultMembers)
        {
            var c = ScriptableObject.CreateInstance<CompanionData>();
            c.id = name;
            c.displayName = name;
            c.role = role;
            c.affinity = affinity;
            c.spritePath = path;
            _activeCompanions.Add(c);
        }

        Debug.Log($"[PartyManager] 기본 파티 생성 완료: {_activeCompanions.Count}명");
    }
}