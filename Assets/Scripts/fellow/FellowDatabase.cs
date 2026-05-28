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
//   - CreateRuntimeFellow(def, affinity) : FellowDef → FellowData SO 직접 생성 (성급 배율 포함)
//   - ParseRole(string)                  : 역할 문자열 → CompanionRole 변환
//
// [어디서 쓰이나요?]
//   - PartyManager.cs : 기본 파티 생성 시 동료 정의 로드
//
// [연결된 파일]
//   - fellow/FellowDef.cs          : fellow.json 역직렬화 구조체
//   - Fellow/FellowData.cs         : 동료 정의 + 런타임 통합 SO
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
    // 공개 유틸리티 — FellowData 생성 (통합 후)
    // ----------------------------------------------------------

    /// <summary>역할 문자열을 CompanionRole 열거형으로 변환한다.</summary>
    public static CompanionRole ParseRole(string role)
    {
        if (role == "Tanker")  return CompanionRole.Tanker;
        if (role == "Support") return CompanionRole.Support;
        return CompanionRole.Dealer;
    }

    /// <summary>성별 문자열을 Gender 열거형으로 변환한다. 비어있거나 모르는 값은 랜덤.</summary>
    public static Gender ParseGender(string gender)
    {
        if (gender == "Male")   return Gender.Male;
        if (gender == "Female") return Gender.Female;
        return Random.value < 0.5f ? Gender.Male : Gender.Female;
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
    
    // ----------------------------------------------------------
    // 모집 / 시작 파티 / 합성 모두 여기로 진입.
    //   FellowDef → FellowData 직접 생성 (단일 SO, 성급 배율 자동 계산)
    //   starLevel 매개변수: 모집·시작 파티는 1, 합성 결과는 2/3 등.
    // ----------------------------------------------------------
    public static FellowData CreateRuntimeFellow(FellowDef def, CardAffinity affinity, int starLevel = 1)
    {
        var f = ScriptableObject.CreateInstance<FellowData>();

        // ── 정의 데이터 복사 ──
        f.id            = def.id;
        f.jobClass      = def.jobClass;
        f.role          = ParseRole(def.role);
        f.gender        = ParseGender(def.gender);
        f.affinity      = affinity;
        f.stressResist  = def.stressResist;
        f.recruitCost   = def.recruitCost;
        f.skillIds      = def.skillIds ?? new string[0];
        f.spritePath    = def.spritePath;
        f.animatorPath  = def.animatorPath;   // TODO: sprite 4프레임 작업 + AnimatorController 준비 후 JSON 채움

        // 이름 생성 폐기 — 캐릭터 식별은 직업명(jobClass)으로 통일.
        // UI 의 nameText 들이 displayName 을 그대로 읽으므로 여기서 한 번에 직업명으로 채움.
        f.displayName = !string.IsNullOrEmpty(def.jobClass) ? def.jobClass : def.displayName;

        // 합성 등 외부 지정 starLevel 우선, 없으면 def.starLevel (보통 1), 그것도 0 이하면 1
        f.starLevel     = starLevel > 0 ? starLevel : (def.starLevel > 0 ? def.starLevel : 1);

        // ── 성급 체력 배율 (기획 백로그 §5) ──
        // 1★ → ×1.00 / 2★ → ×1.40 / 3★ → ×1.96 (1.4^(star-1))
        int   baseHp = def.maxHp > 0 ? def.maxHp : 80;
        float mult   = Mathf.Pow(1.4f, f.starLevel - 1);
        f.maxHp        = Mathf.RoundToInt(baseHp * mult);
        f.hpMultiplier = mult;

        // ── 스킬 파워 배율 (기획 백로그 §5) ──
        // 1★ → ×1.00 / 2★ → ×1.25 / 3★ → ×1.5625 (1.25^(star-1))
        f.skillPowerMultiplier = Mathf.Pow(1.25f, f.starLevel - 1);

        // ── 런타임 상태 초기값 ──
        f.positionStack = (StackType)(int)f.role;

        // ── HP 초기값 — maxHp 로 풀피 시작 ──
        // 기존엔 BattleManager 전투 카드 생성 시 InitHp() 가 채워주지만
        // LeftPanel 같은 전투 외 UI 는 그 전에 표시되어 0 으로 보임.
        f.CurrentHp = f.maxHp;

        // ── 초상화 sprite 로드 (Resources 경로 기반) ──
        // FellowDef.spritePath 가 비어 있지 않으면 Resources 에서 로드해 portrait/fellowSprite 둘 다 채움.
        // (LeftPanel CardSlotView 가 portrait → fellowSprite 순으로 fallback)
        if (!string.IsNullOrEmpty(def.spritePath))
        {
            var sprite = Resources.Load<Sprite>(def.spritePath);
            if (sprite != null)
            {
                f.portrait     = sprite;
                f.fellowSprite = sprite;
            }
        }

        // ── 스킬 즉시 배정 ──
        // 기존엔 첫 전투 BattleManager.InitBattle 에서만 배정되어
        // LeftPanel 같은 전투 외 UI 에 스킬이 표시되지 않았다.
        // 동료 생성 시점에 배정해 어디서든 즉시 조회 가능하게 한다.
        //
        // 배정 우선순위 (기획 §08 직업군_스킬_테이블):
        //   1순위 — fellow.json 의 skillIds (직업별 고정 매핑)
        //   2순위 — SkillDatabase.AssignRandomSkills (역할군 랜덤 fallback)
        // BattleManager.InitBattle 의 동일 분기와 통일.
        if (!f.HasSkills)
        {
            string[] ids = (f.skillIds != null && f.skillIds.Length > 0)
                ? f.skillIds
                : (SkillDatabase.Instance != null
                    ? SkillDatabase.Instance.AssignRandomSkills(f.positionStack, 2)
                    : null);

            if (ids != null && ids.Length > 0)
                f.AssignSkills(ids);
        }

        return f;
    }
}
