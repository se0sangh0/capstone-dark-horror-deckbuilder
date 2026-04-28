using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyDatabase : Singleton<EnemyDatabase>
{
    private Dictionary<string, EnemyDef> _enemyMap = new();
    private const string JsonPath = "Data/enemies";

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;
        DontDestroyOnLoad(gameObject);
        LoadEnemies();   // FellowDatabase.LoadFellows 와 똑같은 패턴
    }

    /// <summary>ID 로 EnemyDef 를 조회한다. 없으면 null 반환.</summary>
    public EnemyDef GetEnemy(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        _enemyMap.TryGetValue(id, out var def);
        return def;
    }

    /// <summary>tier 에 맞는 EnemyDef 목록을 반환한다.</summary>
    public List<EnemyDef> GetEnemiesByTier(EnemyTier tier)
    {
        string tierStr = tier.ToString();   // EnemyTier.Normal → "Normal"
        return _enemyMap.Values
            .Where(e => e.tier == tierStr)
            .ToList();
    }

    /// <summary>tier 에 맞는 EnemyDef 를 랜덤으로 1개 반환한다. 없으면 null.</summary>
    public EnemyDef GetRandomEnemy(EnemyTier tier)
    {
        var list = GetEnemiesByTier(tier);
        if (list.Count == 0) return null;
        return list[Random.Range(0, list.Count)];
    }

    /// JSON → 런타임 EnemyData 인스턴스 생성. enemySprite 까지 박아준다.
    public static EnemyData CreateRuntimeEnemy(EnemyDef def)
    {
        var e = ScriptableObject.CreateInstance<EnemyData>();
        e.id          = def.id;
        e.displayName = def.displayName;
        e.tier        = ParseTier(def.tier);
        e.maxHp       = def.maxHp > 0 ? def.maxHp : 50;
        e.attackPower = def.attackPower;
        e.attackCost  = def.attackCost;
        e.skillId     = def.skillId;
        e.spritePath  = def.spritePath;
        e.isDead      = false;

        // ⭐ 핵심: enemySprite 를 Resources.Load 로 자동 채움
        if (!string.IsNullOrEmpty(def.spritePath))
            e.enemySprite = Resources.Load<Sprite>(def.spritePath);

        e.InitHp();   // 기존 EnemyData.Hp.cs 의 InitHp 사용
        return e;
    }

    private static EnemyTier ParseTier(string s)
    {
        if (s == "Weak") return EnemyTier.Weak;
        if (s == "Boss") return EnemyTier.Boss;
        return EnemyTier.Normal;
    }
    private void LoadEnemies()
    {
        var asset = Resources.Load<TextAsset>(JsonPath);
        if (asset == null)
        {
            Debug.LogError($"[EnemyDatabase] JSON 파일을 찾을 수 없습니다: Resources/{JsonPath}.json");
            return;
        }

        var collection = JsonUtility.FromJson<EnemyDefCollection>(asset.text);
        if (collection == null || collection.enemies == null)
        {
            Debug.LogError("[EnemyDatabase] enemies.json 파싱 실패 — 구조를 확인하세요.");
            return;
        }

        _enemyMap.Clear();
        int skipCount = 0;

        foreach (var def in collection.enemies)
        {
            if (string.IsNullOrEmpty(def.id))
            {
                Debug.LogWarning("[EnemyDatabase] id 가 비어있는 항목 건너뜀.");
                skipCount++;
                continue;
            }

            bool validTier = def.tier == "Weak" || def.tier == "Normal" || def.tier == "Boss";
            if (!validTier)
            {
                Debug.LogWarning($"[EnemyDatabase] {def.id}: tier '{def.tier}' 이 유효하지 않음 — 건너뜀.");
                skipCount++;
                continue;
            }

            _enemyMap[def.id] = def;
        }

        Debug.Log($"[EnemyDatabase] 적 데이터 로드 완료: {_enemyMap.Count}개 (건너뜀: {skipCount}개)");
    }
}
