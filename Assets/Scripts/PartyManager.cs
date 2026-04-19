// ============================================================
// PartyManager.cs
// 동료 모집/사망/파티 관리 싱글톤 매니저
// ============================================================
//
// [이 파일이 하는 일]
//   현재 파티에 있는 동료(FellowData) 목록을 관리합니다.
//   - 동료 모집 (RecruitFellow)
//   - 동료 사망/이탈 (RemoveFellow) — 스킬도 함께 초기화됨
//   - 현재 살아있는 동료 목록 조회 (GetActiveFellows)
//
// [SO 에셋 사용 방법]
//   Inspector 에서 _defaultFellowAssets 에 FellowData SO 에셋을
//   (TestDealer1, TestTanker1, TestSupporter1 등) 연결하면
//   런타임에 그 에셋이 그대로 파티 멤버로 사용됩니다.
//   비어 있으면 기존처럼 런타임에 임시 인스턴스를 생성합니다.
//
// [스킬 영속 구조]
//   FellowData 인스턴스 자체를 파티 전체에서 공유합니다.
//   BattleManager 는 매 전투마다 새 인스턴스를 만들지 않고
//   이 목록의 FellowData 를 그대로 재사용합니다.
//   → 스킬은 최초 배정 후 사망 전까지 변경되지 않습니다.
//
// [씬이 바뀌어도 유지됩니다]
//   DontDestroyOnLoad 로 설정됩니다.
//
// [어디서 쓰이나요?]
//   - BattleManager.cs : GetActiveFellows() 로 FellowData 목록 조회
//   - BattleManager.Combat.cs : RemoveFellow() 로 사망 동료 제거
//
// [연결된 파일]
//   - Core/Singleton.cs : 싱글톤 기반 클래스
//   - fellow/FellowData.cs : 동료 런타임 상태 ScriptableObject
//   - Companion/CompanionData.cs : 동료 기본 정의 데이터
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
    // [SO 에셋 연결] — Inspector 에서 FellowData SO 에셋을 여기에 드래그
    // ----------------------------------------------------------
    [Header("동료 SO 에셋 (Inspector 에서 드래그로 연결)")]
    [Tooltip("기본 파티 FellowData SO 에셋 목록.\n"
           + "비어 있으면 런타임에 임시 인스턴스를 생성합니다.\n"
           + "예: TestDealer1, TestTanker1, TestSupporter1")]
    [SerializeField]
    private List<FellowData> _defaultFellowAssets = new();

    // ----------------------------------------------------------
    // [_activeFellows] — 현재 살아있는 동료 FellowData 목록
    // ----------------------------------------------------------
    [Header("현재 파티 (런타임 관리)")]
    [SerializeField]
    [Tooltip("현재 파티에 있는 동료 FellowData 목록 (런타임에서 관리됨)")]
    private List<FellowData> _activeFellows = new();

    // ----------------------------------------------------------
    // Awake — 싱글톤 등록 + 씬 유지 + 기본 파티 생성
    // ----------------------------------------------------------
    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        DontDestroyOnLoad(gameObject);
        InitDefaultParty();
    }

    // ----------------------------------------------------------
    // 공개 API
    // ----------------------------------------------------------

    /// <summary>
    /// 현재 살아있는 동료 FellowData 목록의 복사본을 반환한다.
    /// BattleManager 에서 전투 시작 시 호출됩니다.
    /// </summary>
    public List<FellowData> GetActiveFellows()
        => _activeFellows.ToList();

    /// <summary>
    /// DeckBuilder 호환용 — 동료의 CompanionData 목록을 반환한다.
    /// data 가 null 인 동료는 제외됩니다.
    /// </summary>
    public List<CompanionData> GetActiveCompanions()
        => _activeFellows
            .Where(f => f != null && f.data != null)
            .Select(f => f.data)
            .ToList();

    /// <summary>
    /// 동료를 파티에 추가한다. (모집 이벤트에서 호출)
    /// 이미 파티에 있으면 중복 추가하지 않는다.
    /// </summary>
    public void RecruitFellow(FellowData fellow)
    {
        if (_activeFellows.Contains(fellow))
        {
            Debug.LogWarning($"[PartyManager] 이미 파티에 있는 동료: {fellow.data?.displayName ?? fellow.name}");
            return;
        }
        _activeFellows.Add(fellow);
        Debug.Log($"[PartyManager] 동료 합류: {fellow.data?.displayName ?? fellow.name} | 현재 {_activeFellows.Count}명");
    }

    /// <summary>
    /// 동료를 파티에서 제거한다. (사망 또는 이탈 시 BattleManager 가 호출)
    /// ClearSkills() 를 함께 호출하여 스킬을 초기화한다.
    /// </summary>
    public void RemoveFellow(FellowData fellow)
    {
        if (fellow == null) return;
        fellow.ClearSkills();
        _activeFellows.Remove(fellow);
        Debug.Log($"[PartyManager] 동료 사망/이탈: {fellow.data?.displayName ?? fellow.name} | 잔여: {_activeFellows.Count}명");
    }

    /// <summary>
    /// 하위 호환용 — CompanionData 로 동료를 찾아 RemoveFellow 를 호출한다.
    /// </summary>
    public void RemoveCompanion(CompanionData companion)
    {
        var fellow = _activeFellows.FirstOrDefault(f => f.data == companion);
        if (fellow != null) RemoveFellow(fellow);
    }

    /// <summary>현재 파티 인원 수</summary>
    public int CompanionCount => _activeFellows.Count;

    // ----------------------------------------------------------
    // 기본 파티 생성
    // SO 에셋이 Inspector 에 연결되어 있으면 그것을 사용하고,
    // 없으면 기존처럼 런타임 임시 인스턴스를 생성합니다.
    // ----------------------------------------------------------
    private void InitDefaultParty()
    {
        if (_activeFellows.Count > 0) return;

        // ── SO 에셋이 있으면 우선 사용 ──────────────────────────
        if (_defaultFellowAssets != null && _defaultFellowAssets.Count > 0)
        {
            foreach (var fellow in _defaultFellowAssets)
            {
                if (fellow == null) continue;

                // CompanionData 가 없으면 positionStack 기반으로 런타임 생성
                if (fellow.data == null)
                {
                    var c = ScriptableObject.CreateInstance<CompanionData>();
                    c.id            = fellow.name;
                    c.displayName   = fellow.name;
                    c.role          = (CompanionRole)(int)fellow.positionStack;
                    c.affinity      = CardAffinity.Gambler;
                    c.maxHp         = 30;
                    c.requiredStack = 3;
                    fellow.data     = c;
                    Debug.Log($"[PartyManager] {fellow.name}: CompanionData 런타임 생성 (data 가 null 이었음)");
                }

                _activeFellows.Add(fellow);
            }

            Debug.Log($"[PartyManager] SO 에셋 파티 로드 완료: {_activeFellows.Count}명");
            return;
        }

        // ── SO 에셋 없음: 런타임 임시 파티 생성 (기존 방식) ─────
        // TODO: 나중에 모집 시스템으로 교체하고 이 블록은 삭제하세요.
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
            c.id            = name;
            c.displayName   = name;
            c.role          = role;
            c.affinity      = affinity;
            c.spritePath    = path;
            c.maxHp         = 30;
            c.requiredStack = 3;

            var fellow = ScriptableObject.CreateInstance<FellowData>();
            fellow.data          = c;
            fellow.positionStack = (StackType)(int)role;

            _activeFellows.Add(fellow);
        }

        Debug.Log($"[PartyManager] 런타임 기본 파티 생성 완료: {_activeFellows.Count}명");
    }

    // ----------------------------------------------------------
    // [ContextMenu] 무결성 테스트
    // ----------------------------------------------------------

    /// <summary>[에디터 테스트] 현재 파티 동료 목록을 콘솔에 출력한다.</summary>
    [ContextMenu("TEST / 현재 파티 목록 출력")]
    private void TestPrintParty()
    {
        Debug.Log($"[PartyManager] 파티 인원: {_activeFellows.Count}명");
        for (int i = 0; i < _activeFellows.Count; i++)
        {
            var f     = _activeFellows[i];
            string nm = f.data?.displayName ?? f.name;
            string sk = f.HasSkills ? "스킬 배정됨" : "스킬 없음";
            Debug.Log($"  [{i}] {nm} | 역할:{f.positionStack} | HP:{f.CurrentHp} | {sk}");
        }
    }

    /// <summary>[에디터 테스트] 파티를 기본값으로 초기화한다.</summary>
    [ContextMenu("TEST / 기본 파티로 초기화")]
    private void TestResetParty()
    {
        _activeFellows.Clear();
        InitDefaultParty();
        Debug.Log("[PartyManager] 기본 파티로 초기화 완료.");
    }
}
