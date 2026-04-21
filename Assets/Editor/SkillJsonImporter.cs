#if UNITY_EDITOR
// Editor/SkillJsonImporter.cs
// skills.json 이 변경될 때마다 SkillDefinition SO 에셋을 자동으로 동기화한다.
// 별도 메뉴 실행 없이 JSON 저장만 하면 즉시 반영된다.

using System.IO;
using UnityEditor;
using UnityEngine;

public class SkillJsonImporter : AssetPostprocessor
{
    private const string WatchPath  = "Assets/Resources/Data/skills.json";
    private const string BaseFolder = "Assets/Scripts/Skill/SO";

    static void OnPostprocessAllAssets(
        string[] imported, string[] deleted, string[] moved, string[] movedFrom)
    {
        foreach (var path in imported)
        {
            if (path == WatchPath)
            {
                SyncJsonToAssets();
                return;
            }
        }
    }

    static void SyncJsonToAssets()
    {
        var jsonAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(WatchPath);
        if (jsonAsset == null)
        {
            Debug.LogError("[SkillJsonImporter] skills.json 을 찾을 수 없습니다.");
            return;
        }

        var collection = JsonUtility.FromJson<SkillDataCollection>(jsonAsset.text);
        if (collection?.skills == null || collection.skills.Count == 0)
        {
            Debug.LogError("[SkillJsonImporter] JSON 파싱 실패 또는 스킬이 없습니다.");
            return;
        }

        int created = 0, updated = 0;

        foreach (var skill in collection.skills)
        {
            if (string.IsNullOrEmpty(skill.id)) continue;

            string roleFolder = $"{BaseFolder}/{skill.costType}";
            if (!AssetDatabase.IsValidFolder(roleFolder))
                AssetDatabase.CreateFolder(BaseFolder, skill.costType);

            string assetPath = $"{roleFolder}/{skill.id}.asset";

            var so = AssetDatabase.LoadAssetAtPath<SkillDefinition>(assetPath);
            bool isNew = so == null;
            if (isNew) so = ScriptableObject.CreateInstance<SkillDefinition>();

            so.id          = skill.id;
            so.displayName = skill.displayName;
            so.skillGroup  = skill.skillGroup;
            so.costAmount  = skill.costAmount;
            so.power       = skill.power;
            so.aiPriority  = skill.aiPriority;
            so.statusValue = skill.statusValue;
            so.description = skill.description;

            if (System.Enum.TryParse(skill.costType,     out StackType    st)) so.costType     = st;
            if (System.Enum.TryParse(skill.targeting,    out Targeting    tg)) so.targeting    = tg;
            if (System.Enum.TryParse(skill.effectType,   out EffectType   et)) so.effectType   = et;
            if (System.Enum.TryParse(skill.statusEffect, out StatusEffect se)) so.statusEffect = se;

            if (isNew) { AssetDatabase.CreateAsset(so, assetPath); created++; }
            else       { EditorUtility.SetDirty(so); updated++; }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[SkillJsonImporter] 자동 동기화 완료 — {created}개 생성, {updated}개 업데이트");
    }
}
#endif
