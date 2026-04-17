// ============================================================
// PartyManager.cs
// 동료 모집/사망/파티 관리 싱글톤 매니저
// ============================================================
//
// [이 파일이 하는 일]
//   현재 파티에 있는 동료 목록을 관리합니다.
//   - 동료 모집 (RecruitCompanion)
//   - 동료 사망/이탈 (RemoveCompanion)
//   - 현재 살아있는 동료 목록 조회 (GetActiveCompanions)
//
// [씬이 바뀌어도 유지됩니다]
//   파티는 전투 씬이 바뀌어도 유지되어야 하므로
//   DontDestroyOnLoad 로 설정됩니다.
//   (Singleton Inspector 에서 "씬이 바뀌어도 유지" 체크 필요)
//
// [어디서 쓰이나요?]
//   - BattleManager.cs : GetActiveCompanions() 로 동료 목록 조회
//   - BattleManager.cs : RemoveCompanion() 으로 사망 동료 제거
//
// [연결된 파일]
//   - Core/Singleton.cs : 싱글톤 기반 클래스
//   - Companion/CompanionData.cs : 동료 데이터 ScriptableObject
//   - BattleManager.cs : 전투 시작 시 파티 목록 사용
// ============================================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 동료 모집/사망/파티 관리 싱글톤 매니저.
/// PartyManager.Instance 로 전역 접근 가능.
/// </summary>
public class PartyManager : Singleton<PartyManager>
{
    // ----------------------------------------------------------
    // [_activeCompanions] — 현재 살아있는 동료 목록
    // Inspector 에서 확인할 수 있도록 SerializeField 적용
    // ----------------------------------------------------------
    [SerializeField]
    [Tooltip("현재 파티에 있는 동료 목록 (런타임에서 관리됨)")]
    private List<CompanionData> _activeCompanions = new();

    // ----------------------------------------------------------
    // Awake — 싱글톤 등록 + 씬 유지 + 기본 파티 생성
    // ----------------------------------------------------------
    protected override void Awake()
    {
        // 반드시 base.Awake() 먼저 호출 (싱글톤 중복 제거 처리)
        base.Awake();

        // 중복 파괴된 경우 초기화하지 않음
        if (Instance != this) return;

        // 씬 전환 시에도 파티 유지
        DontDestroyOnLoad(gameObject);

        // 테스트용 기본 파티 생성 (나중에 모집 시스템으로 교체)
        InitDefaultParty();
    }

    // ----------------------------------------------------------
    // 공개 API
    // ----------------------------------------------------------

    /// <summary>
    /// 현재 살아있는 동료 목록의 복사본을 반환한다.
    /// (외부에서 목록을 직접 변조하지 못하도록 복사본 반환)
    /// </summary>
    public List<CompanionData> GetActiveCompanions()
        => _activeCompanions.ToList();

    /// <summary>
    /// 동료를 파티에 추가한다. (모집 이벤트에서 호출)
    /// 이미 파티에 있으면 중복 추가하지 않는다.
    /// </summary>
    public void RecruitCompanion(CompanionData companion)
    {
        if (_activeCompanions.Contains(companion))
        {
            Debug.LogWarning($"[PartyManager] 이미 파티에 있는 동료: {companion.displayName}");
            return;
        }
        _activeCompanions.Add(companion);
        Debug.Log($"[PartyManager] 동료 합류: {companion.displayName} | 현재 {_activeCompanions.Count}명");
    }

    /// <summary>
    /// 동료를 파티에서 제거한다. (사망 또는 이탈 시 BattleManager 가 호출)
    /// </summary>
    public void RemoveCompanion(CompanionData companion)
    {
        _activeCompanions.Remove(companion);
        Debug.Log($"[PartyManager] 동료 사망/이탈: {companion?.displayName} | 잔여: {_activeCompanions.Count}명");
    }

    /// <summary>현재 파티 인원 수</summary>
    public int CompanionCount => _activeCompanions.Count;

    // ----------------------------------------------------------
    // 테스트용 기본 파티 생성
    // TODO: 나중에 모집 시스템으로 교체하고 이 메서드는 삭제하세요.
    // ScriptableObject 없이 런타임에 직접 생성합니다.
    // ----------------------------------------------------------
    private void InitDefaultParty()
    {
        // 이미 파티가 있으면 초기화 안 함
        if (_activeCompanions.Count > 0) return;

        // (이름, 역할, 성향, 스프라이트 경로) 형식
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
            c.id          = name;
            c.displayName = name;
            c.role        = role;
            c.affinity    = affinity;
            c.spritePath  = path;
            c.maxHp       = 30;
            c.requiredStack = 3;
            _activeCompanions.Add(c);
        }

        Debug.Log($"[PartyManager] 기본 파티 생성 완료: {_activeCompanions.Count}명");
    }

    // ----------------------------------------------------------
    // [ContextMenu] 무결성 테스트
    // ----------------------------------------------------------

    /// <summary>[에디터 테스트] 현재 파티 동료 목록을 콘솔에 출력한다.</summary>
    [ContextMenu("TEST / 현재 파티 목록 출력")]
    private void TestPrintParty()
    {
        Debug.Log($"[PartyManager] 파티 인원: {_activeCompanions.Count}명");
        for (int i = 0; i < _activeCompanions.Count; i++)
        {
            var c = _activeCompanions[i];
            Debug.Log($"  [{i}] {c.displayName} | 역할:{c.role} | 성향:{c.affinity} | HP:{c.maxHp}");
        }
    }

    /// <summary>[에디터 테스트] 파티를 기본값으로 초기화한다.</summary>
    [ContextMenu("TEST / 기본 파티로 초기화")]
    private void TestResetParty()
    {
        _activeCompanions.Clear();
        InitDefaultParty();
        Debug.Log("[PartyManager] 기본 파티로 초기화 완료.");
    }
}
