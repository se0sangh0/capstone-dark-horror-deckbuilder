// ============================================================
// Run/RunSessionManager.cs
// 런 시작·마감·상태 소유권 싱글톤 (P0-01)
// ============================================================
//
// [이 파일이 하는 일]
//   한 번의 탐사(런)의 수명 주기를 단일 창구로 관리합니다.
//   - StartNewRun()      : 새 런 초기화 (정해진 순서, 실패 시 진행 차단)
//   - FinalizeRun(result): 런 마감 — 같은 런에서 정확히 한 번만 처리
//   - IssueFellowId()    : 런타임 동료 고유 ID 발급 (런 내 재사용 없음)
//
// [기획 참조 — 16-B 구현·데이터 계약 §4 상태 소유권·초기화]
//   - StartNewRun 순서: MercenaryService.ResetForNewRun → PartyManager 초기
//     파티 재생성 → SoulstoneManager.ResetCurrency → RunSession 기록 초기화.
//     중간 단계 실패 시 오류를 표시하고 1층 입력을 열지 않는다.
//   - FinalizeRun 만 런 번호 증가·RunSession 폐기·런 상태 초기화를 수행한다.
//     같은 런에서 두 번 호출되면 두 번째 호출은 무시한다.
//   - 영속 상태(runNumber·opening_completed·tutorial_completed·마석)는
//     런 마감에서 삭제하지 않는다.
//   - P0 는 활성 런 중단 복귀를 지원하지 않는다. 확인하지 않은 런은 포기하며
//     런 번호를 증가시키지 않는다. (중단 복귀는 P1-01)
//
// [영속 PlayerPrefs 키 소유권]
//   런/영속 경계에 걸친 키는 전부 이 파일의 상수로만 선언합니다.
//   다른 시스템이 임의 키를 추가하지 않습니다 (16 §3 작업 규칙).
//
// [어디서 쓰이나요?]
//   - MoveScene.cs            : 시작하기 → StartNewRun
//   - BattleManager.Phases.cs : 보스 클리어/전멸 → FinalizeRun
//   - DebugToolPanel.cs       : 디버그 새 런
//   - FellowDatabase.cs       : CreateRuntimeFellow → IssueFellowId
// ============================================================

using UnityEngine;

/// <summary>런 종료 결과. FinalizeRun(result) 인자.</summary>
public enum RunResult
{
    Victory = 0,  // 6층 보스 클리어
    Defeat  = 1   // 파티 전멸
}

/// <summary>
/// 런 시작·진행·마감과 상태 소유권 단일 창구.
/// 씬 배치 없이 첫 접근 시 자동 생성된다 (DontDestroyOnLoad).
/// </summary>
public class RunSessionManager : MonoBehaviour
{
    // ----------------------------------------------------------
    // [영속 키] — 런/영속 경계 키의 단일 소유 지점
    // ----------------------------------------------------------
    /// <summary>완료(보고서 확인)한 런 수. FinalizeRun 에서만 +1.</summary>
    public const string RunCompletedCountKey = "run_completed_count";
    /// <summary>오프닝 완료 플래그 — P0-06 오프닝 흐름이 사용 (여기서 키만 소유).</summary>
    public const string OpeningCompletedKey = "opening_completed";

    // prototype_demo_v1 고정 시드 (16-B §2 고정 시드).
    // 초기 파티 성향 재현 등 "매 런 동일 재현" 이 필요한 곳의 시드 원천.
    public const int PrototypeSeed = 0x5EED0001;

    // ----------------------------------------------------------
    // [싱글톤 — 자동 부트스트랩]
    // 씬에 미리 배치할 필요 없이 게임 시작 시 스스로 생성된다.
    // ----------------------------------------------------------
    public static RunSessionManager Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("RunSessionManager");
        go.AddComponent<RunSessionManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ----------------------------------------------------------
    // [런 세션 상태] — 휘발성 (앱 종료 시 소멸 = 확인 안 한 런 포기)
    // ----------------------------------------------------------

    /// <summary>현재 활성 런이 있는지. StartNewRun 성공 ~ FinalizeRun 사이 true.</summary>
    public bool IsRunActive { get; private set; }

    /// <summary>활성 런의 번호 (완료 런 수 + 1). 활성 런이 없으면 다음에 시작할 번호.</summary>
    public int CurrentRunNumber => CompletedRunCount + 1;

    /// <summary>영속 — 보고서 확인까지 마친 런 수.</summary>
    public int CompletedRunCount => PlayerPrefs.GetInt(RunCompletedCountKey, 0);

    // ── 사건 기록·현장 관찰 (P0-03) — RunSession 소유 (16-B §4 상태 소유권) ──

    /// <summary>이번 런의 사건 기록 (추가 전용). 조사관 수첩·탐사 보고서가 읽는다.</summary>
    public RunRecord Records { get; private set; } = new RunRecord();

    /// <summary>
    /// 다음 전투 승리 뒤 표시할 현장 관찰 ID. 전투 진입 시 노드/경로가 예약하고
    /// 전투 결과 처리에서 1회 소비된다. 지정 인카운터에만 붙는다 (16-B §3).
    /// </summary>
    public string PendingObservationId { get; private set; }

    // 층별 BattleResolved 1건 가드 (16-B §3: 전투 한 번당 BattleResolved 한 번)
    private readonly System.Collections.Generic.HashSet<int> _battleRecordedFloors = new();

    // RunResolved 1건 가드
    private bool _runResolvedRecorded;

    // 이번 런에서 발급한 동료 ID 수 (런 내 재사용 금지 카운터)
    private int _fellowIdCounter;

    // StartNewRun 초기화 진행 중 표시 — 초기 파티 생성 시점에도
    // IssueFellowId 가 새 런 번호로 ID 를 발급하게 한다.
    private bool _initializingRun;

    // 세션 밖(튜토리얼·에디터 직접 플레이 등)에서 생성된 동료용 폴백 카운터
    private static int _bootFellowIdCounter;

    // ----------------------------------------------------------
    // StartNewRun — 새 런 초기화 (16-B §4 StartNewRun)
    // ----------------------------------------------------------

    /// <summary>
    /// 새 런을 시작한다. 계약된 순서로 전체 초기화가 성공했을 때만 true.
    /// false 를 반환하면 호출자는 1층(GamePlayScene) 입력을 열면 안 된다.
    /// 활성 런이 남아있으면(마감 전 앱 재시작·디버그 새 런 등) 포기 처리하고
    /// 런 번호를 증가시키지 않는다.
    /// </summary>
    public bool StartNewRun()
    {
        // 확인하지 않은 런 포기 — 카운터 증가 없음 (16-B §4 마감과 영속 경계)
        if (IsRunActive)
        {
            Debug.Log($"[RunSession] 마감되지 않은 런 #{CurrentRunNumber} 포기 — 새 런으로 대체");
            IsRunActive = false;
        }

        try
        {
            // 이번 런의 ID 발급 준비 — 초기 파티도 새 런 번호 ID 를 받는다.
            _fellowIdCounter = 0;
            _initializingRun = true;

            // 타이틀 씬 등에서 호출될 수 있으므로 필수 매니저가 없으면 생성한다.
            // (PartyManager 는 DontDestroyOnLoad, FellowDatabase 는 씬 인스턴스가
            //  다시 로드되면 자연 교체된다)
            EnsureCoreManagers();

            // ① 용병소 — 예비대/후보/리롤 초기화
            if (MercenaryService.Instance != null)
                MercenaryService.Instance.ResetForNewRun();

            // ② 파티 — P0 고정 초기 파티 재생성 (실패 시 런 시작 중단)
            if (PartyManager.Instance == null)
            {
                Debug.LogError("[RunSession] StartNewRun 실패 — PartyManager 없음");
                return false;
            }
            if (!PartyManager.Instance.SetupInitialParty())
            {
                Debug.LogError("[RunSession] StartNewRun 실패 — 초기 파티 생성 실패 (FellowDatabase/정의 확인)");
                return false;
            }

            // ③ 영혼석 — 시작값으로 초기화.
            //    타이틀 씬 등 SoulstoneManager 인스턴스가 없는 시점엔 저장 키만 비워
            //    다음 씬 로드 시 StartingAmount 로 시작하게 한다.
            if (SoulstoneManager.Instance != null)
                SoulstoneManager.Instance.ResetCurrency();
            else
                PlayerPrefs.DeleteKey(SoulstoneManager.PrefsKey);

            // ④ RunSession 경로·수첩 기록 초기화 (경로 기록은 P0-02 에서 이 자리에 추가)
            Records = new RunRecord();
            PendingObservationId = null;
            _battleRecordedFloors.Clear();
            _runResolvedRecorded = false;

            // ⑤ 세션 활성화 — 1층 생성은 호출자의 GamePlayScene 로드가 수행
            IsRunActive = true;
            Debug.Log($"[RunSession] 새 런 시작 — 런 #{CurrentRunNumber} (완료 런 수 {CompletedRunCount})");
            return true;
        }
        catch (System.Exception e)
        {
            // 일부 상태만 초기화된 채 플레이를 계속하지 않는다 (16-B §4)
            IsRunActive = false;
            Debug.LogError($"[RunSession] StartNewRun 중 예외 — 런 시작 중단: {e}");
            return false;
        }
        finally
        {
            _initializingRun = false;
        }
    }

    /// <summary>
    /// StartNewRun 에 필요한 매니저가 씬에 없으면 생성한다.
    /// 앱 재시작 직후 타이틀 씬에서는 GamePlayScene 소속 싱글톤이 아직 없다.
    /// </summary>
    private static void EnsureCoreManagers()
    {
        if (FellowDatabase.Instance == null)
        {
            new GameObject("FellowDatabase (auto)").AddComponent<FellowDatabase>();
            Debug.Log("[RunSession] FellowDatabase 자동 생성 (타이틀 진입 경로)");
        }
        if (PartyManager.Instance == null)
        {
            new GameObject("PartyManager (auto)").AddComponent<PartyManager>();
            Debug.Log("[RunSession] PartyManager 자동 생성 (타이틀 진입 경로)");
        }
    }

    // ----------------------------------------------------------
    // FinalizeRun — 런 마감 (16-B §4 마감과 영속 경계)
    // ----------------------------------------------------------

    /// <summary>
    /// 런을 마감한다. 같은 런에서 정확히 한 번만 처리되며 두 번째 호출은 무시된다.
    /// 수행: 완료 런 수 +1(영속) → 런 상태 초기화(파티·예비대·영혼석) → 세션 폐기.
    /// 영속 상태(마석·tutorial_completed·opening_completed·설정)는 건드리지 않는다.
    /// </summary>
    public bool FinalizeRun(RunResult result)
    {
        if (!IsRunActive)
        {
            Debug.Log($"[RunSession] FinalizeRun({result}) 무시 — 활성 런 없음 (중복 마감 가드)");
            return false;
        }

        int finishedRunNumber = CurrentRunNumber;

        // ① 세션 폐기를 먼저 확정 — 아래 초기화 중 예외가 나도 재마감(이중 정산)은 불가
        IsRunActive = false;

        // ② 완료 런 수 +1 (영속, 정확히 한 번)
        PlayerPrefs.SetInt(RunCompletedCountKey, CompletedRunCount + 1);
        PlayerPrefs.Save();

        // ③ 런 상태 초기화 — 휘발성 상태만. 다음 파티 구성은 StartNewRun 이 수행.
        MercenaryService.Instance?.ResetForNewRun();
        PartyManager.Instance?.ClearForRunEnd();
        if (SoulstoneManager.Instance != null)
            SoulstoneManager.Instance.ResetCurrency();
        else
            PlayerPrefs.DeleteKey(SoulstoneManager.PrefsKey);

        // ④ 사건 기록·관찰 예약 폐기 (보고서 생성 뒤 초기화 — 16-B §4)
        Records = new RunRecord();
        PendingObservationId = null;
        _battleRecordedFloors.Clear();
        _runResolvedRecorded = false;

        Debug.Log($"[RunSession] 런 #{finishedRunNumber} 마감 ({result}) — 완료 런 수 {CompletedRunCount}");
        return true;
    }

    // ----------------------------------------------------------
    // 전투 결과·현장 관찰 기록 (P0-03, 16-B §3 진행 확정 시점)
    // 순서: 승리 확정 → 영혼석 반영(처치 시) → BattleResolved 1건 생성
    //       → 조건부 현장 관찰 표시 → 다음 이동
    // ----------------------------------------------------------

    /// <summary>
    /// 다음 전투의 현장 관찰을 예약한다 (전투 진입 시 호출, null = 관찰 없음).
    /// 관찰은 지정 인카운터에만 붙는다 — 같은 전투 프로필의 다른 전투에
    /// 자동으로 붙이지 않는다 (16-B §3).
    /// </summary>
    public void SetPendingObservation(string observationId)
    {
        PendingObservationId = observationId;
        if (!string.IsNullOrEmpty(observationId))
            Debug.Log($"[RunSession] 현장 관찰 예약 — {observationId}");
    }

    /// <summary>
    /// 예약된 현장 관찰을 1회 소비해 반환한다 (없으면 null).
    /// 재호출·재표시로 같은 관찰이 두 번 처리되지 않는다.
    /// </summary>
    public FieldObservation ConsumePendingObservation()
    {
        var obs = FieldObservationCatalog.GetById(PendingObservationId);
        PendingObservationId = null;
        return obs;
    }

    /// <summary>
    /// 전투 결과 확정 기록 — 전투 한 번당 BattleResolved 정확히 1건.
    /// 영혼석은 처치 시 이미 반영된 상태여야 한다 (영혼석 먼저 → 기록).
    /// 현장 관찰이 있으면 표시 전에 사후 관찰 필드로 포함한다 (16-A §2).
    /// 활성 런이 없으면(세션 밖 직접 플레이) 기록하지 않는다.
    /// </summary>
    public bool RecordBattleResolved(int floor, string enemySummary, bool victory, int soulstoneGained, string observationNotebookText)
    {
        if (!IsRunActive) return false;
        if (!_battleRecordedFloors.Add(floor))
        {
            Debug.Log($"[RunSession] BattleResolved 중복 무시 — {floor}층");
            return false;
        }

        var entry = new RunRecordEntry
        {
            type  = RunRecordType.BattleResolved,
            floor = floor,
            title = victory ? "전투 승리" : "전멸",
        };
        if (!string.IsNullOrEmpty(enemySummary))
            entry.lines.Add(enemySummary);
        if (victory && soulstoneGained > 0)
            entry.lines.Add($"영혼석 {soulstoneGained}개 획득");
        if (victory && !string.IsNullOrEmpty(observationNotebookText))
            entry.lines.Add(observationNotebookText); // 사후 관찰 — 표시 전에 기록에 포함

        return Records.Add(entry, dedupKey: $"battle_F{floor}");
    }

    /// <summary>런 종료 기록 — 클리어/전멸과 최종 도달 구역. 런당 1건만 생성.</summary>
    public bool RecordRunResolved(bool victory, int reachedFloor)
    {
        if (!IsRunActive || _runResolvedRecorded) return false;
        _runResolvedRecorded = true;
        return Records.Add(new RunRecordEntry
        {
            type  = RunRecordType.RunResolved,
            floor = reachedFloor,
            title = victory ? "클리어" : "전멸",
            lines = { $"최종 도달: {reachedFloor}층" },
        }, dedupKey: "run_resolved");
    }

    // ----------------------------------------------------------
    // 고유 동료 ID (16-B §4 새 런 기준선)
    // ----------------------------------------------------------

    /// <summary>
    /// 런타임 동료 고유 ID 를 발급한다. 한 런 안에서 재사용하지 않으며,
    /// 다음 런에서는 같은 정의 ID 도 새 ID 를 받는다.
    /// 예: "run003_ally_caster_01_002"
    /// 세션 밖(튜토리얼 등)에서는 "boot_" 접두 폴백 ID 를 발급한다.
    /// </summary>
    public static string IssueFellowId(string definitionId)
    {
        string defPart = string.IsNullOrEmpty(definitionId) ? "unknown" : definitionId;
        if (Instance != null && (Instance.IsRunActive || Instance._initializingRun))
        {
            Instance._fellowIdCounter++;
            return $"run{Instance.CurrentRunNumber:D3}_{defPart}_{Instance._fellowIdCounter:D3}";
        }
        _bootFellowIdCounter++;
        return $"boot_{defPart}_{_bootFellowIdCounter:D3}";
    }

    // ----------------------------------------------------------
    // 고정 시드 유틸 (16-B §2)
    // ----------------------------------------------------------

    /// <summary>
    /// prototype_demo_v1 전용 시드에서 파생된 결정적 RNG.
    /// 같은 salt 면 매 런 같은 순서를 재현한다 (초기 파티 성향 등).
    /// </summary>
    public static System.Random CreateSeededRng(int salt) => new System.Random(PrototypeSeed ^ salt);
}
