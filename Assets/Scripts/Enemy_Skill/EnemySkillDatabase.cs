// ============================================================
// Enemy_Skill/EnemySkillDatabase.cs
// 적 스킬 JSON 데이터베이스 싱글톤 (적 전용)
// ============================================================
//
// [왜 동료 SkillDatabase 와 따로 만들었나요?]
//   동료 SkillDatabase 는 "역할별 랜덤 배정" 같은 동료 전용 API 가 들어있고,
//   적은 "ID 조회"만 있으면 충분합니다.
//   같은 클래스로 묶으면 동료 변경이 적에 영향을 주거나 그 반대도 생기므로 분리.
//
// [동료 SkillDatabase 와 동일한 패턴]
//   - Singleton<T> 상속
//   - Resources/Data/enemy_skills.json 을 JsonUtility 로 로드
//   - Awake 에서 1회 로드 후 Dictionary 로 ID 조회
//
// [씬 배치 — ⭐ 사용자가 직접 해야 할 작업]
//   Hierarchy 에 빈 GameObject 를 만들고 이름을 "EnemySkillDatabase" 로 한 뒤
//   이 스크립트를 Add Component 로 붙여야 작동합니다.
//   (SkillDatabase 와 동일한 GameObject 에 함께 붙여도 무방합니다.)
//
// [어디서 쓰이나요?]
//   - BattleManager.EnemyAction.cs : 적 턴 행동 시 ID로 스킬 데이터 조회
// ============================================================

using System.Collections.Generic;
using UnityEngine;

public class EnemySkillDatabase : Singleton<EnemySkillDatabase>
{
    /// <summary>스킬 ID → EnemySkillData 빠른 조회</summary>
    private Dictionary<string, EnemySkillData> _skillMap = new();

    /// <summary>JSON 경로 (Resources 기준, 확장자 제외)</summary>
    private const string JsonPath = "Data/enemy_skills";

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        DontDestroyOnLoad(gameObject);
        LoadSkills();
    }

    // ── JSON → Dictionary ────────────────────────────────────
    private void LoadSkills()
    {
        var jsonAsset = Resources.Load<TextAsset>(JsonPath);
        if (jsonAsset == null)
        {
            Debug.LogError($"[EnemySkillDatabase] JSON 파일 없음: Resources/{JsonPath}.json");
            return;
        }

        var collection = JsonUtility.FromJson<EnemySkillDataCollection>(jsonAsset.text);
        if (collection == null || collection.enemySkills == null)
        {
            Debug.LogError("[EnemySkillDatabase] enemy_skills.json 파싱 실패 — 구조 확인 필요.");
            return;
        }

        _skillMap.Clear();
        foreach (var s in collection.enemySkills)
        {
            if (string.IsNullOrEmpty(s.id))
            {
                Debug.LogWarning("[EnemySkillDatabase] id 가 비어있는 항목 건너뜀.");
                continue;
            }
            if (_skillMap.ContainsKey(s.id))
                Debug.LogWarning($"[EnemySkillDatabase] 중복 ID: {s.id} — 덮어씁니다.");

            _skillMap[s.id] = s;
        }

        LoadSpritesForSkills();

        Debug.Log($"[EnemySkillDatabase] 적 스킬 {_skillMap.Count}개 로드 완료.");
    }

    /// <summary>
    /// 로드된 모든 적 스킬을 순회하며 spritePath 가 채워진 항목의 Sprite 를 Resources 에서 로드한다.
    /// 경로가 비어있으면 skip (런타임 호출 측에서 fallback 처리).
    /// Fellow SkillDatabase 와 동일한 패턴.
    /// </summary>
    private void LoadSpritesForSkills()
    {
        int loaded = 0, missing = 0;
        foreach (var s in _skillMap.Values)
        {
            if (string.IsNullOrEmpty(s.spritePath)) continue;

            s.sprite = Resources.Load<Sprite>(s.spritePath);
            if (s.sprite == null)
            {
                Debug.LogWarning($"[EnemySkillDatabase] 스프라이트 없음: '{s.spritePath}' (id={s.id})");
                missing++;
            }
            else loaded++;
        }
        if (loaded + missing > 0)
            Debug.Log($"[EnemySkillDatabase] 스프라이트 로드: 성공 {loaded}개 / 실패 {missing}개");
    }

    // ── 공개 API ─────────────────────────────────────────────

    /// <summary>스킬 ID 로 데이터를 조회한다. 없으면 null + 경고.</summary>
    public EnemySkillData GetSkill(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (_skillMap.TryGetValue(id, out var s)) return s;

        Debug.LogWarning($"[EnemySkillDatabase] 스킬 ID '{id}' 없음.");
        return null;
    }

    /// <summary>현재 로드된 스킬 수</summary>
    public int SkillCount => _skillMap.Count;

    // ── 에디터 테스트 ────────────────────────────────────────
    [ContextMenu("TEST / 적 스킬 전체 출력")]
    private void TestPrintAll()
    {
        if (_skillMap.Count == 0) { Debug.LogWarning("[EnemySkillDatabase] 로드된 스킬 없음."); return; }
        Debug.Log($"[EnemySkillDatabase] 총 {_skillMap.Count}개:");
        foreach (var kv in _skillMap)
        {
            var s = kv.Value;
            Debug.Log($"  [{s.id}] {s.displayName} | {s.targeting} | 파워:{s.power} 가중치:{s.weight}");
        }
    }
}
