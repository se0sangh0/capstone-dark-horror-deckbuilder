// ============================================================
// Fellow/NameDatabase.cs
// JSON 동료 이름 풀 싱글톤 매니저
// 기획 §6 (이름 생성 규칙) + 10_동료_스킬_데이터 §판타지 세계관 이름 풀 v1
// ============================================================
//
// [이 파일이 하는 일]
//   Resources/Data/names.json 을 로드해 남/여 각각 FirstName + LastName
//   풀을 메모리에 올려두고, gender 기준으로 무작위 이름을 조합해 반환합니다.
//
// [기획서 풀 구조 (10_동료_스킬_데이터.md)]
//   AllyNameTableSO
//   ├── string[] firstNames   // 10개
//   └── string[] lastNames    // 10개
//   ↑ male/female 별도 테이블 → 남녀 각각 10×10 = 100가지 조합, 총 200가지
//
// [주요 API]
//   - GetRandomName(Gender) : "First Last" 형식의 무작위 이름 반환
//
// [어디서 쓰이나요?]
//   - FellowDatabase.CreateRuntimeFellow() : 동료 생성 시 displayName 채움
//   - PartyManager.CreateFallbackFellow()  : 폴백 동료의 이름 할당
// ============================================================

using System.Collections.Generic;
using UnityEngine;

public class NameDatabase : Singleton<NameDatabase>
{
    [System.Serializable]
    private class NameCollection
    {
        public List<string> maleFirstNames;
        public List<string> maleLastNames;
        public List<string> femaleFirstNames;
        public List<string> femaleLastNames;
    }

    private const string JsonPath = "Data/names";

    private List<string> _maleFirst   = new();
    private List<string> _maleLast    = new();
    private List<string> _femaleFirst = new();
    private List<string> _femaleLast  = new();

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        DontDestroyOnLoad(gameObject);
        LoadNames();
    }

    private void LoadNames()
    {
        var asset = Resources.Load<TextAsset>(JsonPath);
        if (asset == null)
        {
            Debug.LogError($"[NameDatabase] JSON 파일을 찾을 수 없습니다: Resources/{JsonPath}.json");
            return;
        }

        var col = JsonUtility.FromJson<NameCollection>(asset.text);
        if (col == null)
        {
            Debug.LogError("[NameDatabase] names.json 파싱 실패 — 구조 확인.");
            return;
        }

        _maleFirst   = col.maleFirstNames   ?? new List<string>();
        _maleLast    = col.maleLastNames    ?? new List<string>();
        _femaleFirst = col.femaleFirstNames ?? new List<string>();
        _femaleLast  = col.femaleLastNames  ?? new List<string>();

        Debug.Log($"[NameDatabase] 이름 풀 로드 완료 — 남:{_maleFirst.Count}×{_maleLast.Count} / 여:{_femaleFirst.Count}×{_femaleLast.Count}");
    }

    /// <summary>
    /// gender 에 맞는 First + Last 를 조합한 무작위 이름을 반환한다.
    /// 풀이 비어있으면 폴백 이름.
    /// </summary>
    public string GetRandomName(Gender gender)
    {
        var firstPool = gender == Gender.Female ? _femaleFirst : _maleFirst;
        var lastPool  = gender == Gender.Female ? _femaleLast  : _maleLast;

        string first = firstPool.Count > 0 ? firstPool[Random.Range(0, firstPool.Count)] : "Unknown";
        string last  = lastPool.Count  > 0 ? lastPool [Random.Range(0, lastPool.Count) ] : "Wanderer";
        return $"{first} {last}";
    }

    // ----------------------------------------------------------
    // [ContextMenu] 에디터 테스트
    // ----------------------------------------------------------
    [ContextMenu("TEST / 랜덤 이름 10개 출력")]
    private void TestPrintNames()
    {
        for (int i = 0; i < 10; i++)
        {
            var g = (Gender)(i % 2);
            Debug.Log($"  [{g}] {GetRandomName(g)}");
        }
    }
}
