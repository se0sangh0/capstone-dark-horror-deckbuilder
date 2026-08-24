// ============================================================
// Event/Editor/EventCatalogGenerator.cs
// EventCatalogData(코드 정의) → Resources/Events/*.asset 일괄 생성 (에디터 전용)
// ============================================================
//
// [사용법]
//   Unity 상단 메뉴 ▸ Tools ▸ DarkHorror ▸ 이벤트 카탈로그 생성 (19종)
//   → Assets/Resources/Events/ 에 evt_*.asset 19개가 생성/갱신된다.
//   런타임(EventCatalog)은 이 에셋들을 우선 로드한다.
//
// [주의]
//   이미 있는 evt_*.asset 은 덮어써서 코드 정의와 동기화한다.
//   에셋에서 직접 수정한 값이 있다면 재생성 시 사라지니, 확정된 수정은
//   EventCatalogData.cs 에 반영하는 것을 권장.
// ============================================================

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class EventCatalogGenerator
{
    private const string ResourcesDir = "Assets/Resources/Events";

    [MenuItem("Tools/DarkHorror/이벤트 카탈로그 생성 (19종)")]
    public static void Generate()
    {
        // 폴더 보장
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(ResourcesDir))
            AssetDatabase.CreateFolder("Assets/Resources", "Events");

        var defs = EventCatalogData.BuildAll();
        int created = 0, updated = 0;

        foreach (var def in defs)
        {
            if (def == null || string.IsNullOrEmpty(def.id)) continue;
            string path = $"{ResourcesDir}/{def.id}.asset";

            var existing = AssetDatabase.LoadAssetAtPath<EventDefinition>(path);
            if (existing != null)
            {
                // 기존 에셋에 값 복사 (참조/GUID 유지)
                EditorUtility.CopySerialized(def, existing);
                EditorUtility.SetDirty(existing);
                Object.DestroyImmediate(def); // 임시 인스턴스 폐기
                updated++;
            }
            else
            {
                AssetDatabase.CreateAsset(def, path);
                created++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[EventCatalogGenerator] 완료 — 생성 {created} / 갱신 {updated} (총 {defs.Count}종) @ {ResourcesDir}");
        EditorUtility.DisplayDialog("이벤트 카탈로그 생성",
            $"이벤트 SO {created + updated}종을 {ResourcesDir} 에 생성/갱신했습니다.\n(생성 {created}, 갱신 {updated})", "확인");
    }

    [MenuItem("Tools/DarkHorror/이벤트 카탈로그 폴더 열기")]
    public static void PingFolder()
    {
        var folder = AssetDatabase.LoadAssetAtPath<Object>(ResourcesDir);
        if (folder != null) { Selection.activeObject = folder; EditorGUIUtility.PingObject(folder); }
        else Debug.LogWarning($"[EventCatalogGenerator] {ResourcesDir} 폴더가 아직 없습니다. 먼저 '이벤트 카탈로그 생성'을 실행하세요.");
    }
}
