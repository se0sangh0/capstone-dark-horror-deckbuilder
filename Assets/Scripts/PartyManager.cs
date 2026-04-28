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
    //임시주석처리(삭제예정)
    // ----------------------------------------------------------
    // [SO 에셋 연결] — Inspector 에서 FellowData SO 에셋을 여기에 드래그
    // ----------------------------------------------------------
    //[Header("동료 SO 에셋 (Inspector 에서 드래그로 연결)")]
    /*[Tooltip("기본 파티 FellowData SO 에셋 목록.\n"
           + "비어 있으면 런타임에 임시 인스턴스를 생성합니다.\n"
           + "예: TestDealer1, TestTanker1, TestSupporter1")]*/
    //[SerializeField]
    //private List<FellowData> _defaultFellowAssets = new();

    // ----------------------------------------------------------
    // [_activeFellows] — 현재 살아있는 동료 FellowData 목록
    // ----------------------------------------------------------
    [Header("현재 파티 (런타임 관리)")]
    [SerializeField]
    [Tooltip("현재 파티에 있는 동료 FellowData 목록 (런타임에서 관리됨)")]
    private List<FellowData> _activeFellows = new();

    // 사망한 동료 보관소 — 게임 리셋 전까지 유지
    private List<FellowData> _deadFellowArchive = new();

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
    /// 동료를 파티에서 제거하고 보관소에 저장한다. (사망 또는 이탈 시 BattleManager 가 호출)
    /// </summary>
    public void RemoveFellow(FellowData fellow)
    {
        if (fellow == null) return;
        fellow.ClearSkills();
        _activeFellows.Remove(fellow);
        _deadFellowArchive.Add(fellow);
        Debug.Log($"[PartyManager] 동료 사망/이탈: {fellow.data?.displayName ?? fellow.name} | 잔여: {_activeFellows.Count}명 | 보관: {_deadFellowArchive.Count}명");
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
    }

    /// <summary>보관소에 있는 사망 동료 수</summary>
    public int DeadCount => _deadFellowArchive.Count;

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

    // ── [강화 시스템 TODO] 승급(성급 올리기) ────────────────────────
    // 기획서 §합성/승급:
    //   같은 역할군 + 같은 성급 동료 3명 → 소멸, 랜덤 역할의 다음 성급 동료 획득
    //
    // 구현 예정 흐름:
    //
    //   public bool TryUpgradeStar(CompanionRole role, int currentStar)
    //   {
    //       var candidates = _activeFellows
    //           .Where(f => f.data?.role == role && f.starLevel == currentStar && !f.isDead)
    //           .Take(3).ToList();
    //
    //       if (candidates.Count < 3) return false;
    //
    //       // 3명 제거
    //       foreach (var f in candidates) RemoveFellow(f);
    //
    //       // 다음 성급 동료 생성 (랜덤 역할)
    //       string[] roles = { "Dealer", "Tanker", "Support" };
    //       string newRole = roles[UnityEngine.Random.Range(0, roles.Length)];
    //       var def = FellowDatabase.Instance.GetRandomFellow(newRole);
    //       if (def == null) return false;
    //
    //       var newData = FellowDatabase.CreateCompanionData(def, CardAffinity.Gambler);
    //       newData.starLevel = currentStar + 1;
    //
    //       // starLevel 에 따라 maxHp 배율 재계산 (1.5^(star-1))
    //       float mult = UnityEngine.Mathf.Pow(1.5f, newData.starLevel - 1);
    //       newData.maxHp = UnityEngine.Mathf.RoundToInt(
    //           (FellowDatabase.Instance.GetFellow(def.id)?.maxHp ?? newData.maxHp) * mult
    //       );
    //
    //       var newFellow = ScriptableObject.CreateInstance<FellowData>();
    //       newFellow.data          = newData;
    //       newFellow.positionStack = (StackType)(int)newData.role;
    //       newFellow.starLevel     = newData.starLevel;
    //       RecruitFellow(newFellow);
    //
    //       Debug.Log($"[승급] {role} {currentStar}★ ×3 → {newRole} {currentStar + 1}★ 획득!");
    //       return true;
    //   }

    // ----------------------------------------------------------
    // 기본 파티 생성
    // SO 에셋이 Inspector 에 연결되어 있으면 그것을 사용하고,
    // 없으면 FellowDatabase 에서 랜덤 4명을 생성한다.
    // ----------------------------------------------------------
    private void InitDefaultParty()
    {
        if (_activeFellows.Count > 0) return;
        //임시주석처리(삭제예정)
        // ── SO 에셋이 있으면 우선 사용 ──────────────────────────
        /*if (_defaultFellowAssets != null && _defaultFellowAssets.Count > 0)
        {
            bool dbReady = FellowDatabase.Instance != null;

            foreach (var fellow in _defaultFellowAssets)
            {
                if (fellow == null) continue;

                if (fellow.data == null)
                {
                    CompanionData c = null;

                    if (dbReady)
                    {
                        string roleStr = fellow.positionStack.ToString();
                        var def = FellowDatabase.Instance.GetRandomFellow(roleStr);
                        if (def != null)
                        {
                            c = FellowDatabase.CreateCompanionData(def, RandomAffinity());
                            Debug.Log($"[PartyManager] {fellow.name}: FellowDatabase 에서 CompanionData 생성 ({def.id})");
                        }
                    }

                    if (c == null)
                    {
                        c               = ScriptableObject.CreateInstance<CompanionData>();
                        c.id            = fellow.name;
                        c.displayName   = fellow.name;
                        c.role          = (CompanionRole)(int)fellow.positionStack;
                        c.affinity      = RandomAffinity();
                        c.maxHp         = 80;
                        c.stressResist  = 0;
                        c.recruitCost   = 30;
                    }

                    fellow.data = c;
                }

                _activeFellows.Add(fellow);
            }

            Debug.Log($"[PartyManager] SO 에셋 파티 로드 완료: {_activeFellows.Count}명");
            return;
        }*/

        // ── SO 에셋 없음: 랜덤 4명 생성 ─────────────────────────
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
            //임시주석처리(삭제예정)
            /*for (int i = 0; i < allDefs.Count && added < count; i++)
            {
                var def    = allDefs[i];
                var c      = FellowDatabase.CreateCompanionData(def, RandomAffinity());
                //임시주석처리(삭제예정)
                /*var fellow = ScriptableObject.CreateInstance<FellowData>();
                fellow.data          = c;
                fellow.positionStack = (StackType)(int)c.role;
                _activeFellows.Add(fellow);*/
                //FellowDatabase.CreateRuntimeFellow(def, RandomAffinity());
                //added++;
            //}
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
                var c = ScriptableObject.CreateInstance<CompanionData>();
                c.id            = $"fallback_{_activeFellows.Count}";
                c.displayName   = $"동료 {_activeFellows.Count + 1}";
                c.affinity      = RandomAffinity();
                c.maxHp         = 80;
                c.stressResist  = 0;
                c.recruitCost   = 30;

                var fellow = ScriptableObject.CreateInstance<FellowData>();
                fellow.data          = c;
                fellow.positionStack = StackType.Dealer;
                _activeFellows.Add(fellow);
            }
        }
        else
        {
            // FellowDatabase 없음: 최소 폴백
            for (int i = 0; i < count; i++)
            {
                var c = ScriptableObject.CreateInstance<CompanionData>();
                c.id            = $"fallback_{i}";
                c.displayName   = $"동료 {i + 1}";
                c.affinity      = RandomAffinity();
                c.maxHp         = 80;
                c.stressResist  = 0;
                c.recruitCost   = 30;

                var fellow = ScriptableObject.CreateInstance<FellowData>();
                fellow.data          = c;
                fellow.positionStack = StackType.Dealer;
                _activeFellows.Add(fellow);
            }
            Debug.LogWarning("[PartyManager] FellowDatabase 없음 — 폴백 파티 생성.");
        }

        Debug.Log($"[PartyManager] 랜덤 파티 생성 완료: {_activeFellows.Count}명");
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
    //신규추가(모집용 공개 API(용병소 노드가 부를 단일 진입점))
    public bool RecruitById(string fellowId, CardAffinity affinity)
    {
        var def = FellowDatabase.Instance?.GetFellow(fellowId);
        if (def == null) return false;
        var fellow = FellowDatabase.CreateRuntimeFellow(def, affinity);
        RecruitFellow(fellow);
        return true;
    }
}
