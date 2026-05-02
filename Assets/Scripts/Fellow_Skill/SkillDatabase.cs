// ============================================================
// Skill/SkillDatabase.cs
// JSON 스킬 데이터베이스 싱글톤 매니저
// ============================================================
//
// [이 파일이 하는 일]
//   게임이 시작될 때 Resources/Data/skills.json 파일을 읽어서
//   모든 스킬 정보를 메모리에 올려둡니다.
//   이후 어디서나 스킬 ID 로 빠르게 조회하거나,
//   역할(Dealer/Tank/Support)에 맞는 스킬을 랜덤 배정할 수 있습니다.
//
// [주요 API]
//   - GetSkill(id)                     : ID 로 스킬 1개 조회
//   - GetSkillsByRole(role)             : 역할에 맞는 스킬 목록 조회
//   - AssignRandomSkills(role, count)   : 역할에 맞는 스킬 랜덤 배정 (중복 없음)
//
// [어디서 쓰이나요?]
//   - BattleManager.cs : 전투 시작 시 동료에게 스킬 랜덤 배정
//   - FellowData.GetSkills() : 동료의 스킬 목록 런타임 조회
//
// [연결된 파일]
//   - Skill/SkillData.cs : 스킬 1개의 데이터 구조
//   - Resources/Data/skills.json : 실제 스킬 데이터 파일
//   - Core/Singleton.cs : 싱글톤 기반 클래스
//
// [인스펙터 설정]
//   씬에 빈 GameObject 를 만들고 SkillDatabase 컴포넌트를 붙이세요.
// ============================================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// JSON 파일에서 스킬 데이터를 로드하고, ID 조회 및 역할별 랜덤 배정을 제공하는 싱글톤.
/// </summary>
public class SkillDatabase : Singleton<SkillDatabase>
{
    // ----------------------------------------------------------
    // [내부 상태]
    // ----------------------------------------------------------

    /// <summary>스킬 ID → SkillData 빠른 조회 딕셔너리</summary>
    private Dictionary<string, SkillData> _skillMap = new();

    /// <summary>JSON 파일 경로 (Resources 폴더 기준, 확장자 제외)</summary>
    private const string JsonPath = "Data/skills";

    // ----------------------------------------------------------
    // Awake — 싱글톤 등록 + 스킬 로드
    // ----------------------------------------------------------
    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        DontDestroyOnLoad(gameObject);
        LoadSkills();
    }

    // ----------------------------------------------------------
    // 스킬 로드 (JSON → Dictionary)
    // ----------------------------------------------------------

    /// <summary>Resources/Data/skills.json 을 읽어 _skillMap 에 등록한다.</summary>
    private void LoadSkills()
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>(JsonPath);

        if (jsonAsset == null)
        {
            Debug.LogError($"[SkillDatabase] JSON 파일을 찾을 수 없습니다: Resources/{JsonPath}.json");
            return;
        }

        var collection = JsonUtility.FromJson<SkillDataCollection>(jsonAsset.text);

        if (collection == null || collection.skills == null)
        {
            Debug.LogError("[SkillDatabase] JSON 파싱에 실패했습니다. 파일 형식을 확인하세요.");
            return;
        }

        _skillMap.Clear();
        foreach (var skill in collection.skills)
        {
            if (string.IsNullOrEmpty(skill.id))
            {
                Debug.LogWarning("[SkillDatabase] ID 가 비어있는 스킬이 있습니다. 건너뜁니다.");
                continue;
            }
            if (_skillMap.ContainsKey(skill.id))
                Debug.LogWarning($"[SkillDatabase] 중복 스킬 ID: {skill.id} — 덮어씁니다.");

            _skillMap[skill.id] = skill;
        }

        Debug.Log($"[SkillDatabase] 스킬 {_skillMap.Count}개 로드 완료.");
    }

    // ----------------------------------------------------------
    // 공개 API — 조회
    // ----------------------------------------------------------

    /// <summary>
    /// 스킬 ID 로 스킬 데이터를 반환한다.
    /// 없으면 null 반환 + 경고 출력.
    /// </summary>
    public SkillData GetSkill(string id)
    {
        if (_skillMap.TryGetValue(id, out var skill)) return skill;
        Debug.LogWarning($"[SkillDatabase] 스킬 ID '{id}' 를 찾을 수 없습니다.");
        return null;
    }

    /// <summary>
    /// 역할(StackType)에 맞는 스킬 목록을 반환한다.
    /// skills.json 의 costType 문자열("Dealer","Tank","Support")과 비교한다.
    /// </summary>
    /// <param name="role">Dealer / Tank / Support</param>
    public List<SkillData> GetSkillsByRole(StackType role)
    {
        // StackType.Dealer → "Dealer" 문자열로 변환하여 costType 과 비교
        string roleStr = role.ToString();
        return _skillMap.Values
                        .Where(s => s.costType == roleStr)
                        .ToList();
    }

    /// <summary>현재 로드된 전체 스킬 수</summary>
    public int SkillCount => _skillMap.Count;

    // ----------------------------------------------------------
    // 공개 API — 랜덤 배정
    // ----------------------------------------------------------

    /// <summary>
    /// 역할에 맞는 스킬 중 count 개를 중복 없이 랜덤으로 골라
    /// ID 배열로 반환한다.
    ///
    /// 동작 예:
    ///   Dealer 동료 → costType="Dealer" 인 스킬들 중 2개 랜덤 선택
    ///
    /// 스킬이 count 보다 적으면 있는 것 모두 반환한다.
    /// </summary>
    /// <param name="role">동료 역할</param>
    /// <param name="count">배정할 스킬 수 (기본 2)</param>
    /// <returns>선택된 스킬 ID 배열 (중복 없음)</returns>
    public string[] AssignRandomSkills(StackType role, int count = 2)
    {
        var candidates = GetSkillsByRole(role);

        if (candidates.Count == 0)
        {
            Debug.LogWarning($"[SkillDatabase] '{role}' 역할에 맞는 스킬이 없습니다. skills.json 을 확인하세요.");
            return new string[0];
        }

        // Fisher-Yates 셔플로 순서를 섞은 뒤 앞에서 count 개 선택 (중복 없음)
        var shuffled = candidates.ToList();
        ShuffleList(shuffled);

        int take = Mathf.Min(count, shuffled.Count);
        string[] result = shuffled.Take(take).Select(s => s.id).ToArray();

        Debug.Log($"[SkillDatabase] '{role}' 스킬 배정: {string.Join(", ", result)} (후보 {candidates.Count}개 중 {take}개 선택)");
        return result;
    }

    // ----------------------------------------------------------
    // 내부 유틸리티
    // ----------------------------------------------------------

    /// <summary>Fisher-Yates 셔플 (Unity Random 사용)</summary>
    private static void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // ----------------------------------------------------------
    // [ContextMenu] 에디터 테스트
    // ----------------------------------------------------------

    /// <summary>[에디터 테스트] 로드된 모든 스킬을 콘솔에 출력한다.</summary>
    [ContextMenu("TEST / 로드된 스킬 전체 출력")]
    private void TestPrintAllSkills()
    {
        if (_skillMap.Count == 0)
        {
            Debug.LogWarning("[SkillDatabase] 로드된 스킬이 없습니다.");
            return;
        }

        Debug.Log($"[SkillDatabase] 총 {_skillMap.Count}개 스킬:");
        foreach (var kvp in _skillMap)
        {
            var s = kvp.Value;
            Debug.Log($"  [{s.id}] {s.displayName} | 역할:{s.costType} 코스트:{s.costAmount} | 효과:{s.effectType} 파워:{s.power}");
        }
    }

    /// <summary>[에디터 테스트] 역할별 스킬 목록을 분류하여 출력한다.</summary>
    [ContextMenu("TEST / 역할별 스킬 목록 출력")]
    private void TestPrintByRole()
    {
        foreach (StackType role in System.Enum.GetValues(typeof(StackType)))
        {
            var skills = GetSkillsByRole(role);
            Debug.Log($"[SkillDatabase] [{role}] 스킬 {skills.Count}개:");
            foreach (var s in skills)
                Debug.Log($"    → [{s.id}] {s.displayName} (파워:{s.power}, 대상:{s.targeting})");
        }
    }

    /// <summary>[에디터 테스트] 각 역할에서 스킬 2개를 랜덤 배정하여 결과를 출력한다.</summary>
    [ContextMenu("TEST / 역할별 랜덤 스킬 배정 시뮬레이션")]
    private void TestAssignRandomSkills()
    {
        foreach (StackType role in System.Enum.GetValues(typeof(StackType)))
        {
            var ids = AssignRandomSkills(role, 2);
            Debug.Log($"[SkillDatabase] [{role}] 배정 결과: {string.Join(", ", ids)}");
            foreach (var id in ids)
            {
                var s = GetSkill(id);
                if (s != null)
                    Debug.Log($"    → {s.displayName}: {s.description}");
            }
        }
    }

    /// <summary>[에디터 테스트] JSON 파일을 다시 로드한다.</summary>
    [ContextMenu("TEST / JSON 다시 로드")]
    private void TestReload()
    {
        LoadSkills();
        Debug.Log($"[SkillDatabase] 다시 로드 완료: {_skillMap.Count}개");
    }

    /// <summary>[무결성 테스트] 모든 스킬의 필수 필드가 채워져 있는지 확인한다.</summary>
    [ContextMenu("TEST / 스킬 데이터 무결성 검사")]
    private void TestIntegrity()
    {
        int errorCount = 0;
        Debug.Log("[SkillDatabase] 무결성 검사 시작...");

        foreach (var kvp in _skillMap)
        {
            var s = kvp.Value;

            if (string.IsNullOrEmpty(s.displayName))
            { Debug.LogError($"  [오류] {s.id}: displayName 이 비어있습니다."); errorCount++; }

            if (string.IsNullOrEmpty(s.costType))
            { Debug.LogError($"  [오류] {s.id}: costType 이 비어있습니다."); errorCount++; }

            if (s.costAmount < 0)
            { Debug.LogError($"  [오류] {s.id}: costAmount 가 음수입니다 ({s.costAmount})."); errorCount++; }

            if (s.power < 0)
            { Debug.LogError($"  [오류] {s.id}: power 가 음수입니다 ({s.power})."); errorCount++; }

            // costType 이 유효한 역할인지 확인
            bool validRole = s.costType == "Dealer" || s.costType == "Tank" || s.costType == "Support";
            if (!validRole)
            { Debug.LogError($"  [오류] {s.id}: costType '{s.costType}' 이 유효하지 않습니다. (Dealer/Tank/Support 중 하나여야 함)"); errorCount++; }
        }

        if (errorCount == 0)
            Debug.Log($"[SkillDatabase] 무결성 검사 통과! 총 {_skillMap.Count}개 스킬 이상 없음.");
        else
            Debug.LogError($"[SkillDatabase] 무결성 검사 실패: {errorCount}개 오류 발견.");
    }
}
