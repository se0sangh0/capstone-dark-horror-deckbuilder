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
    /// 반환 전에 각 fellow.battleSlotIndex 를 _activeFellows 내 인덱스로 stamp —
    /// 사망 후에도 슬롯 위치(0,1=전열, 2,3=후열) 가 전투 시각에 정확히 반영되도록 한다.
    /// BattleManager 에서 전투 시작 시 호출됩니다.
    /// </summary>
    public List<FellowData> GetActiveFellows()
    {
        for (int i = 0; i < _activeFellows.Count; i++)
            if (_activeFellows[i] != null) _activeFellows[i].battleSlotIndex = i;
        return _activeFellows.Where(f => f != null).ToList();
    }

    // [통합 후] GetActiveCompanions 제거 — GetActiveFellows() 가 동일한 역할 (FellowData 직접 반환).

    /// <summary>
    /// 동료를 파티에 추가한다. (모집 이벤트에서 호출)
    /// 빈 슬롯(사망으로 null) 이 있으면 그 슬롯에 채워 인덱스를 유지한다.
    /// 빈 슬롯이 없으면 끝에 추가.
    /// </summary>
    public void RecruitFellow(FellowData fellow)
    {
        if (_activeFellows.Contains(fellow))
        {
            Debug.LogWarning($"[PartyManager] 이미 파티에 있는 동료: {(!string.IsNullOrEmpty(fellow.displayName) ? fellow.displayName : fellow.name)}");
            return;
        }

        // 빈 슬롯(사망자 자리) 우선 채움 — 슬롯 인덱스 유지
        for (int i = 0; i < _activeFellows.Count; i++)
        {
            if (_activeFellows[i] == null)
            {
                _activeFellows[i] = fellow;
                Debug.Log($"[PartyManager] 동료 합류 (빈 슬롯 {i}): {(!string.IsNullOrEmpty(fellow.displayName) ? fellow.displayName : fellow.name)}");
                OnPartyChanged?.Invoke();
                return;
            }
        }

        _activeFellows.Add(fellow);
        Debug.Log($"[PartyManager] 동료 합류: {(!string.IsNullOrEmpty(fellow.displayName) ? fellow.displayName : fellow.name)} | 현재 {CompanionCount}명");
        OnPartyChanged?.Invoke();
    }

    /// <summary>
    /// 동료를 파티에서 제거하고 보관소에 저장한다. (사망 또는 이탈 시 BattleManager 가 호출)
    /// 슬롯 인덱스를 보존하기 위해 리스트에서 빼지 않고 해당 자리를 null 로 표시한다.
    /// </summary>
    public void RemoveFellow(FellowData fellow)
    {
        if (fellow == null) return;

        // 멱등 가드 — 이미 archive 에 있으면 중복 호출. ProcessDeathAndStress 가 매 턴 같은 사망 fellow 를
        // 다시 처리하더라도 archive 가 부풀지 않도록 skip.
        if (_deadFellowArchive.Contains(fellow)) return;

        fellow.ClearSkills();

        int idx = _activeFellows.IndexOf(fellow);
        if (idx >= 0) _activeFellows[idx] = null;  // 자리만 비움
        _deadFellowArchive.Add(fellow);

        Debug.Log($"[PartyManager] 동료 사망/이탈 (슬롯 {idx}): {(!string.IsNullOrEmpty(fellow.displayName) ? fellow.displayName : fellow.name)} | 잔여: {CompanionCount}명 | 보관: {_deadFellowArchive.Count}명");
        OnPartyChanged?.Invoke();
    }

    // ----------------------------------------------------------
    // P0 고정 초기 파티 (16-B §4 새 런 기준선)
    // ----------------------------------------------------------

    /// <summary>P0 초기 파티 고정 구성 — 정식 시작 파티 확정 아님 (16-B §4).</summary>
    private static readonly string[] InitialPartyIds =
        { "ally_caster_01", "ally_offender_01", "ally_defender_01", "ally_priest_01" };

    /// <summary>
    /// 새 런의 초기 파티를 재생성한다. RunSessionManager.StartNewRun 이 호출.
    /// 파티·사망 보관소를 비우고 고정 4인을 생성하며, 성향은 prototype_demo_v1
    /// 전용 시드로 매 런 동일하게 재현한다.
    /// 정의를 하나라도 찾지 못하면 false — 런 시작을 중단해야 한다 (16-B §4).
    /// </summary>
    public bool SetupInitialParty()
    {
        if (FellowDatabase.Instance == null)
        {
            Debug.LogError("[PartyManager] SetupInitialParty 실패 — FellowDatabase 없음");
            return false;
        }

        _activeFellows.Clear();
        _deadFellowArchive.Clear();

        var rng = RunSessionManager.CreateSeededRng(salt: 1);
        foreach (var id in InitialPartyIds)
        {
            var def = FellowDatabase.Instance.GetFellow(id);
            if (def == null)
            {
                Debug.LogError($"[PartyManager] SetupInitialParty 실패 — 동료 정의 '{id}' 없음");
                _activeFellows.Clear();
                return false;
            }
            _activeFellows.Add(FellowDatabase.CreateRuntimeFellow(def, SeededAffinity(rng)));
        }

        Debug.Log($"[PartyManager] 초기 파티 생성 — {string.Join(", ", InitialPartyIds)}");
        OnPartyChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// 런 마감 시 파티·사망 보관소를 비운다. RunSessionManager.FinalizeRun 이 호출.
    /// 다음 파티 구성은 StartNewRun → SetupInitialParty 가 수행한다.
    /// </summary>
    public void ClearForRunEnd()
    {
        _activeFellows.Clear();
        _deadFellowArchive.Clear();
        Debug.Log("[PartyManager] 런 마감 — 파티·사망 보관소 초기화");
        OnPartyChanged?.Invoke();
    }

    // 시드 RNG 로 성향 결정 — None 제외 4종 (매 런 동일 순서 재현)
    private static CardAffinity SeededAffinity(System.Random rng)
    {
        var values = new[] { CardAffinity.Gambler, CardAffinity.Safety, CardAffinity.Opportunist, CardAffinity.Optimist };
        return values[rng.Next(values.Length)];
    }

    /// <summary>
    /// 파티를 현재 모드(IsTutorial)에 맞춰 강제 재초기화한다.
    /// PartyManager 는 DontDestroyOnLoad 이므로 Start() 가 단 1회만 돌아 InitDefaultParty 가 재호출되지 않는다.
    /// 따라서 튜토리얼 ↔ 일반 모드 전환 시 호출자가 명시적으로 부른다.
    /// MoveScene 의 게임 시작/튜토리얼 재진입 핸들러가 호출.
    /// </summary>
    public void ForceReinitParty()
    {
        _activeFellows.Clear();
        _deadFellowArchive.Clear();
        InitDefaultParty(); // IsTutorial 보고 튜토리얼/일반 자동 분기
        Debug.Log($"[PartyManager] ForceReinitParty 완료 — 파티 {_activeFellows.Count(f => f != null)}명");
    }

    /// <summary>보관소에 있는 사망 동료 수</summary>
    public int DeadCount => _deadFellowArchive.Count;

    /// <summary>사망 보관소의 동료 목록 (읽기 전용). 교회/메타 시스템에서 부활 후보 표시용.</summary>
    public IReadOnlyList<FellowData> DeadFellows => _deadFellowArchive;

    /// <summary>
    /// 보관소에서 동료를 꺼내 빈 파티 슬롯에 복귀시킨다 (교회 노드 — 부활).
    /// HP = maxHp × hpRatio, 스트레스 = 0, 실드 = 0 으로 리셋. 스킬은 다음 전투 InitBattle 에서 재배정.
    /// 빈 슬롯이 없거나 보관소에 없는 fellow 이면 false.
    /// </summary>
    public bool ReviveFellow(FellowData fellow, float hpRatio = 0.5f)
    {
        if (fellow == null) return false;
        if (!_deadFellowArchive.Contains(fellow)) return false;

        int emptySlot = -1;
        for (int i = 0; i < _activeFellows.Count; i++)
        {
            if (_activeFellows[i] == null) { emptySlot = i; break; }
        }
        if (emptySlot < 0)
        {
            Debug.LogWarning($"[PartyManager] 부활 실패 — 빈 슬롯 없음 ({fellow.displayName})");
            return false;
        }

        _activeFellows[emptySlot] = fellow;
        _deadFellowArchive.Remove(fellow);

        fellow.isDead         = false;
        int maxHp             = fellow.maxHp > 0 ? fellow.maxHp : 100;
        fellow.CurrentHp      = Mathf.Max(1, Mathf.RoundToInt(maxHp * Mathf.Clamp01(hpRatio)));
        fellow.shield         = 0;
        fellow.currentStress  = 0;

        Debug.Log($"[PartyManager] 부활 — {(!string.IsNullOrEmpty(fellow.displayName) ? fellow.displayName : fellow.name)} (슬롯 {emptySlot}, HP {fellow.CurrentHp}/{maxHp})");
        OnPartyChanged?.Invoke();
        return true;
    }

    /// <summary>현재 파티 인원 수 (null 슬롯 = 사망 자리 제외)</summary>
    public int CompanionCount => _activeFellows.Count(f => f != null);

    /// <summary>파티 슬롯 두 개의 순서를 교환한다. null(사망 자리) 끼리 swap 도 허용 (의미 없지만 무해).</summary>
    public bool SwapFellows(int indexA, int indexB)
    {
        if (indexA == indexB) return false;
        if (indexA < 0 || indexA >= _activeFellows.Count) return false;
        if (indexB < 0 || indexB >= _activeFellows.Count) return false;
        (_activeFellows[indexA], _activeFellows[indexB]) = (_activeFellows[indexB], _activeFellows[indexA]);
        OnPartyChanged?.Invoke();
        return true;
    }

    // TODO[L·승급]: 기획서 §합성/승급 — 같은 역할+성급 동료 3명 → 소멸 + 랜덤 역할 다음 성급
    //              여기에 TryUpgradeStar(CompanionRole, int currentStar) 신설. RecruitById 의 starLevel TODO 와 연동.

    // ----------------------------------------------------------
    // 기본 파티 생성 — FellowDatabase 에서 랜덤 4명 생성
    // ----------------------------------------------------------
    private void InitDefaultParty()
    {
        // 튜토리얼 모드 — 기획 §15 §파티 구성: 캐스터/프리스트/디펜더 3명 고정 + 4번 슬롯 null.
        // 이전 일반 게임 파티가 있어도 강제 클리어 후 재생성 (기획 §5-3 데이터 격리).
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorial)
        {
            _activeFellows.Clear();
            _deadFellowArchive.Clear();
            GenerateTutorialParty();
            return;
        }

        if (_activeFellows.Count > 0) return;

        // 정상 경로는 RunSessionManager.StartNewRun → SetupInitialParty.
        // 여기 도달 = 에디터에서 GamePlayScene 직접 재생 등 세션 밖 진입 — 고정 파티 시도 후 폴백.
        if (!SetupInitialParty())
        {
            Debug.LogWarning("[PartyManager] 초기 파티 생성 실패 — 개발용 폴백 파티 4명 생성");
            for (int i = 0; i < 4; i++) _activeFellows.Add(CreateFallbackFellow(i));
            OnPartyChanged?.Invoke();
        }
    }

    /// <summary>
    /// 튜토리얼 전용 파티 생성 — 캐스터/프리스트/디펜더 3명 고정 + 4번 슬롯 null.
    /// 기획 §15 §3-1. 본 게임 파티 데이터와 격리되어야 하므로 호출 전에 _activeFellows.Clear() 가 선행되어야 함.
    /// </summary>
    private void GenerateTutorialParty()
    {
        var ids = new[] { "ally_caster_01", "ally_priest_01", "ally_defender_01" };
        if (FellowDatabase.Instance == null)
        {
            Debug.LogWarning("[PartyManager] FellowDatabase 없음 — 튜토리얼 폴백 파티 사용");
            for (int i = 0; i < 3; i++) _activeFellows.Add(CreateFallbackFellow(i));
            _activeFellows.Add(null);
            OnPartyChanged?.Invoke();
            return;
        }

        foreach (var id in ids)
        {
            var def = FellowDatabase.Instance.GetFellow(id);
            if (def == null)
            {
                Debug.LogWarning($"[PartyManager] 튜토리얼 동료 ID '{id}' 못 찾음 — 슬롯 비움");
                _activeFellows.Add(null);
                continue;
            }
            _activeFellows.Add(FellowDatabase.CreateRuntimeFellow(def, RandomAffinity()));
        }
        _activeFellows.Add(null); // slot 3 = 빈 자리

        Debug.Log($"[PartyManager] 튜토리얼 파티 생성: {_activeFellows.Count(f => f != null)}명 + 빈 슬롯 1");
        OnPartyChanged?.Invoke();
    }

    // [P0-01] 랜덤 파티 생성 제거 — 초기 파티는 SetupInitialParty 고정 4인만 사용 (16-B §4).

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
        f.runtimeFellowId = RunSessionManager.IssueFellowId(f.id); // 폴백 동료도 고유 ID 발급
        return f;
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
