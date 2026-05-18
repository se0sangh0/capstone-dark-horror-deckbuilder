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
//   - Fellow/FellowData.cs : 동료 정의 + 런타임 상태 통합 SO
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
    // [_activeFellows] — 현재 살아있는 동료 FellowData 목록
    // ----------------------------------------------------------
    [Header("현재 파티 (런타임 관리)")]
    [SerializeField]
    [Tooltip("현재 파티에 있는 동료 FellowData 목록 (런타임에서 관리됨)")]
    private List<FellowData> _activeFellows = new();

    // 사망한 동료 보관소 — 게임 리셋 전까지 유지
    private List<FellowData> _deadFellowArchive = new();

    /// <summary>파티 멤버가 변경(모집/사망/리셋)될 때마다 발생.
    /// LeftPanelView 등 UI가 구독해 자동 갱신한다.</summary>
    public event System.Action OnPartyChanged;

    // ----------------------------------------------------------
    // Awake — 싱글톤 등록 + 씬 유지
    // InitDefaultParty 는 Start() 에서 호출한다.
    // FellowDatabase 가 Awake() 에서 초기화되므로, Start() 에서
    // 호출해야 FellowDatabase.Instance 가 유효하다.
    // ----------------------------------------------------------
    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (Instance != this) return;
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

    // [통합 후] GetActiveCompanions 제거 — GetActiveFellows() 가 동일한 역할 (FellowData 직접 반환).

    /// <summary>
    /// 동료를 파티에 추가한다. (모집 이벤트에서 호출)
    /// 이미 파티에 있으면 중복 추가하지 않는다.
    /// </summary>
    public void RecruitFellow(FellowData fellow)
    {
        if (_activeFellows.Contains(fellow))
        {
            Debug.LogWarning($"[PartyManager] 이미 파티에 있는 동료: {(!string.IsNullOrEmpty(fellow.displayName) ? fellow.displayName : fellow.name)}");
            return;
        }
        _activeFellows.Add(fellow);
        Debug.Log($"[PartyManager] 동료 합류: {(!string.IsNullOrEmpty(fellow.displayName) ? fellow.displayName : fellow.name)} | 현재 {_activeFellows.Count}명");
        OnPartyChanged?.Invoke();
    }

    /// <summary>
    /// 동료를 파티에서 제거하고 보관소에 저장한다. (사망 또는 이탈 시 BattleManager 가 호출)
    /// </summary>
    public void RemoveFellow(FellowData fellow)
    {
        if (fellow == null) return;
        fellow.ClearSkills();
        _activeFellows.Remove(fellow);
        _deadFellowArchive.Add(fellow);
        Debug.Log($"[PartyManager] 동료 사망/이탈: {(!string.IsNullOrEmpty(fellow.displayName) ? fellow.displayName : fellow.name)} | 잔여: {_activeFellows.Count}명 | 보관: {_deadFellowArchive.Count}명");
        OnPartyChanged?.Invoke();
    }

    /// <summary>
    /// 게임을 리셋한다. 사망 보관소를 지우고 4명의 랜덤 파티를 새로 생성한다.
    /// BattleManager 가 아군 전멸 시 게임 오버 씬 전환 직전에 호출한다.
    /// </summary>
    public void ResetGame()
    {
        _deadFellowArchive.Clear();
        _activeFellows.Clear();
        GenerateRandomParty(4);
        Debug.Log($"[PartyManager] 게임 리셋 완료. 새 파티 {_activeFellows.Count}명 생성.");
        OnPartyChanged?.Invoke();
    }

    /// <summary>보관소에 있는 사망 동료 수</summary>
    public int DeadCount => _deadFellowArchive.Count;

    /// <summary>현재 파티 인원 수</summary>
    public int CompanionCount => _activeFellows.Count;

    // TODO[L·승급]: 기획서 §합성/승급 — 같은 역할+성급 동료 3명 → 소멸 + 랜덤 역할 다음 성급
    //              여기에 TryUpgradeStar(CompanionRole, int currentStar) 신설. RecruitById 의 starLevel TODO 와 연동.

    // ----------------------------------------------------------
    // 기본 파티 생성 — FellowDatabase 에서 랜덤 4명 생성
    // ----------------------------------------------------------
    private void InitDefaultParty()
    {
        if (_activeFellows.Count > 0) return;
        GenerateRandomParty(4);
    }

    // ----------------------------------------------------------
    // 랜덤 파티 생성 — FellowDatabase 에서 count 명을 랜덤 선택.
    // 각 동료의 성향은 생성 시 랜덤으로 고정된다.
    // ----------------------------------------------------------
    private void GenerateRandomParty(int count)
    {
        bool dbReady = FellowDatabase.Instance != null;

        if (dbReady)
        {
            // 전체 동료 목록을 섞어서 count 명 선택
            var allDefs = new List<FellowDef>();
            foreach (var role in new[] { "Dealer", "Tanker", "Support" })
                allDefs.AddRange(FellowDatabase.Instance.GetFellowsByRole(role));

            Shuffle(allDefs);
            
            int added = 0;
            for (int i = 0; i < allDefs.Count && added < count; i++)
            {
                var def    = allDefs[i];
                var fellow = FellowDatabase.CreateRuntimeFellow(def, RandomAffinity());
                _activeFellows.Add(fellow);
                added++;
            }

            // DB 에 동료가 부족하면 폴백으로 채움
            while (_activeFellows.Count < count)
            {
                _activeFellows.Add(CreateFallbackFellow(_activeFellows.Count));
            }
        }
        else
        {
            // FellowDatabase 없음: 최소 폴백
            for (int i = 0; i < count; i++)
                _activeFellows.Add(CreateFallbackFellow(i));
            Debug.LogWarning("[PartyManager] FellowDatabase 없음 — 폴백 파티 생성.");
        }

        Debug.Log($"[PartyManager] 랜덤 파티 생성 완료: {_activeFellows.Count}명");
        OnPartyChanged?.Invoke();
    }

    /// <summary>FellowDatabase 가 없거나 동료가 부족할 때 쓰는 최소 폴백 동료.</summary>
    private static FellowData CreateFallbackFellow(int index)
    {
        var f = ScriptableObject.CreateInstance<FellowData>();
        f.id            = $"fallback_{index}";
        f.affinity      = RandomAffinity();
        f.gender        = UnityEngine.Random.value < 0.5f ? Gender.Male : Gender.Female;
        f.displayName   = NameDatabase.Instance != null
            ? NameDatabase.Instance.GetRandomName(f.gender)
            : $"동료 {index + 1}";
        f.maxHp         = 80;
        f.stressResist  = 0;
        f.recruitCost   = 30;
        f.positionStack = StackType.Dealer;
        f.role          = CompanionRole.Dealer;
        f.CurrentHp     = f.maxHp; // 풀피로 시작 (LeftPanel 전투 외 UI 에서 0 표시 방지)
        return f;
    }

    // Fisher-Yates 셔플
    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // 랜덤 성향 — None 제외 4종 중 균등 선택
    private static CardAffinity RandomAffinity()
    {
        var values = new[] { CardAffinity.Gambler, CardAffinity.Safety, CardAffinity.Opportunist, CardAffinity.Optimist };
        return values[UnityEngine.Random.Range(0, values.Length)];
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
            string nm = !string.IsNullOrEmpty(f.displayName) ? f.displayName : f.name;
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
    /// <summary>
    /// 용병소가 호출하는 모집 진입점. starLevel 은 모집·시작 파티 1★ 기본,
    /// 합성 결과를 파티 직접 합류 시키는 흐름에서는 상위 성급도 가능.
    /// </summary>
    public bool RecruitById(string fellowId, CardAffinity affinity, int starLevel = 1)
    {
        var def = FellowDatabase.Instance?.GetFellow(fellowId);
        if (def == null) return false;
        var fellow = FellowDatabase.CreateRuntimeFellow(def, affinity, starLevel);
        RecruitFellow(fellow);
        return true;
    }
}
