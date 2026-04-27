// ============================================================
// fellow/FellowDatabase.cs
// JSON 동료 데이터베이스 싱글톤 매니저
// ============================================================
//
// [이 파일이 하는 일]
//   게임이 시작될 때 Resources/Data/fellow.json 파일을 읽어서
//   모든 동료 정의 데이터를 메모리에 올려둡니다.
//   이후 어디서나 ID 로 빠르게 조회하거나 역할별 랜덤 선택이 가능합니다.
//
// [주요 API]
//   - GetFellow(id)              : ID 로 FellowDef 1개 조회
//   - GetFellowsByRole(role)     : 역할에 맞는 FellowDef 목록 조회
//   - GetRandomFellow(role)      : 역할에 맞는 FellowDef 랜덤 1개 조회
//   - CreateCompanionData(def)   : FellowDef → CompanionData SO 생성
//   - ParseRole(string)          : 역할 문자열 → CompanionRole 변환
//
// [어디서 쓰이나요?]
//   - PartyManager.cs : 기본 파티 생성 시 동료 정의 로드
//
// [연결된 파일]
//   - fellow/FellowDef.cs          : fellow.json 역직렬화 구조체
//   - Companion/CompanionData.cs   : 동료 정의 SO
//   - Resources/Data/fellow.json   : 실제 동료 데이터 파일
//   - Core/Singleton.cs            : 싱글톤 기반 클래스
//
// [인스펙터 설정]
//   씬에 빈 GameObject 를 만들고 FellowDatabase 컴포넌트를 붙이세요.
// ============================================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// JSON 파일에서 동료 정의를 로드하고 ID 조회 및 역할별 선택을 제공하는 싱글톤.
/// </summary>
public class FellowDatabase : Singleton<FellowDatabase>
{
    // ----------------------------------------------------------
    // [내부 상태]
    // ----------------------------------------------------------

    /// <summary>동료 ID → FellowDef 빠른 조회 딕셔너리</summary>
    private Dictionary<string, FellowDef> _fellowMap = new();

    /// <summary>JSON 파일 경로 (Resources 폴더 기준, 확장자 제외)</summary>
    private const string JsonPath = "Data/fellow";

    // ----------------------------------------------------------
    // Awake — 싱글톤 등록 + 동료 데이터 로드
    // ----------------------------------------------------------
    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        DontDestroyOnLoad(gameObject);
        LoadFellows();
    }

    // ----------------------------------------------------------
    // 내부 — JSON 로드
    // ----------------------------------------------------------
    private void LoadFellows()
    {
        var asset = Resources.Load<TextAsset>(JsonPath);
        if (asset == null)
        {
            Debug.LogError($"[FellowDatabase] JSON 파일을 찾을 수 없습니다: Resources/{JsonPath}.json");
            return;
        }

        var collection = JsonUtility.FromJson<FellowDefCollection>(asset.text);
        if (collection == null || collection.fellows == null)
        {
            Debug.LogError("[FellowDatabase] fellow.json 파싱 실패 — 구조를 확인하세요.");
            return;
        }

        _fellowMap.Clear();
        int skipCount = 0;

        foreach (var def in collection.fellows)
        {
            if (string.IsNullOrEmpty(def.id))
            {
                Debug.LogWarning("[FellowDatabase] id 가 비어있는 항목 건너뜀.");
                skipCount++;
                continue;
            }

            bool validRole = def.role == "Dealer" || def.role == "Tanker" || def.role == "Support";
            if (!validRole)
            {
                Debug.LogWarning($"[FellowDatabase] {def.id}: role '{def.role}' 이 유효하지 않음 — 건너뜀.");
                skipCount++;
                continue;
            }

            _fellowMap[def.id] = def;
        }

        Debug.Log($"[FellowDatabase] 동료 데이터 로드 완료: {_fellowMap.Count}개 (건너뜀: {skipCount}개)");
    }

    // ----------------------------------------------------------
    // 공개 API — 조회
    // ----------------------------------------------------------

    /// <summary>ID 로 FellowDef 를 조회한다. 없으면 null 반환.</summary>
    public FellowDef GetFellow(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        _fellowMap.TryGetValue(id, out var def);
        return def;
    }

    /// <summary>역할 문자열에 맞는 FellowDef 목록을 반환한다.</summary>
    public List<FellowDef> GetFellowsByRole(string role)
        => _fellowMap.Values.Where(f => f.role == role).ToList();

    /// <summary>역할 문자열에 맞는 FellowDef 를 랜덤으로 1개 반환한다. 없으면 null.</summary>
    public FellowDef GetRandomFellow(string role)
    {
        var list = GetFellowsByRole(role);
        if (list.Count == 0) return null;
        return list[Random.Range(0, list.Count)];
    }

    // ----------------------------------------------------------
    // 공개 유틸리티 — CompanionData 생성
    // ----------------------------------------------------------

    /// <summary>
    /// FellowDef 에서 런타임 CompanionData SO 를 생성한다.
    /// affinity 는 호출자가 지정한다.
    /// </summary>
    public static CompanionData CreateCompanionData(FellowDef def, CardAffinity affinity)
    {
        var c = ScriptableObject.CreateInstance<CompanionData>();
        c.id            = def.id;
        c.jobClass      = def.jobClass;
        c.displayName   = def.displayName;
        c.role          = ParseRole(def.role);
        c.affinity      = affinity;
        c.stressResist  = def.stressResist;
        c.recruitCost   = def.recruitCost;
        c.skillIds      = def.skillIds ?? new string[0];
        c.starLevel     = def.starLevel > 0 ? def.starLevel : 1;

        // ── [강화 시스템 TODO] ──────────────────────────────────────
        // 성급에 따라 maxHp 를 배율로 계산한다.
        //   1★ → baseHp × 1.00
        //   2★ → baseHp × 1.50
        //   3★ → baseHp × 2.25
        //
        // 현재는 JSON 의 maxHp 가 1★ 기준값이므로 직접 할당.
        // 승급 시 PartyManager.UpgradeStar() 에서 아래 로직으로 재계산:
        //   int baseHp  = FellowDatabase.Instance.GetFellow(c.id)?.maxHp ?? c.maxHp;
        //   float mult  = Mathf.Pow(1.5f, c.starLevel - 1);
        //   c.maxHp     = Mathf.RoundToInt(baseHp * mult);
        c.maxHp = def.maxHp > 0 ? def.maxHp : 80;

        return c;
    }

    /// <summary>역할 문자열을 CompanionRole 열거형으로 변환한다.</summary>
    public static CompanionRole ParseRole(string role)
    {
        if (role == "Tanker")  return CompanionRole.Tanker;
        if (role == "Support") return CompanionRole.Support;
        return CompanionRole.Dealer;
    }

    // ----------------------------------------------------------
    // [ContextMenu] 에디터 테스트
    // ----------------------------------------------------------

    /// <summary>[에디터 테스트] 로드된 동료 데이터 목록을 콘솔에 출력한다.</summary>
    [ContextMenu("TEST / 동료 데이터 목록 출력")]
    private void TestPrintFellows()
    {
        Debug.Log($"[FellowDatabase] 로드된 동료 수: {_fellowMap.Count}");
        foreach (var kv in _fellowMap)
        {
            var f = kv.Value;
            string skills = f.skillIds != null ? string.Join(", ", f.skillIds) : "없음";
            Debug.Log($"  [{f.id}] {f.displayName} | 직업:{f.jobClass} | 역할:{f.role} | HP:{f.maxHp} | 스트레스저항:{f.stressResist} | 모집:{f.recruitCost} | 스킬:{skills}");
        }
    }

    /// <summary>[에디터 테스트] 동료 데이터 무결성 검사.</summary>
    [ContextMenu("TEST / 무결성 검사")]
    private void TestIntegrityCheck()
    {
        Debug.Log("[FellowDatabase] 무결성 검사 시작 ──");
        int errors = 0;

        foreach (var kv in _fellowMap)
        {
            var f = kv.Value;
            if (string.IsNullOrEmpty(f.displayName))
            { Debug.LogError($"  [오류] {f.id}: displayName 이 비어있습니다."); errors++; }

            if (f.maxHp <= 0)
            { Debug.LogError($"  [오류] {f.id}: maxHp 가 0 이하입니다 ({f.maxHp})."); errors++; }

            bool validRole = f.role == "Dealer" || f.role == "Tanker" || f.role == "Support";
            if (!validRole)
            { Debug.LogError($"  [오류] {f.id}: role '{f.role}' 이 유효하지 않습니다."); errors++; }
        }

        if (errors == 0) Debug.Log($"[FellowDatabase] ✓ 무결성 검사 통과! ({_fellowMap.Count}개)");
        else             Debug.LogError($"[FellowDatabase] ✗ 무결성 검사 실패: {errors}개 오류.");
    }
}
