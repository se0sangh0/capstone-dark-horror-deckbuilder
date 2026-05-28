// UIButtonSfxInstaller.cs
// 씬 안의 모든 UnityEngine.UI.Button 에 클릭 SFX 리스너를 자동 부착.
//
// ── 동작 ────────────────────────────────────────────────────────
//   1) 씬 로드 시 활성/비활성 포함 모든 Button 검색
//   2) Button.onClick 에 AudioManager.PlaySfxById(SfxId.ButtonClick) 리스너 추가
//   3) 중복 부착 방지: 이미 등록된 Button 은 HashSet 으로 추적
//
// ── 배치 ────────────────────────────────────────────────────────
//   AudioManager 와 동일한 GameObject (혹은 임의의 persistent 오브젝트) 에 부착.
//   sceneLoaded 이벤트로 새 씬마다 재스캔.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIButtonSfxInstaller : MonoBehaviour
{
    private static readonly HashSet<Button> _installed = new();

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        ScanAndInstall();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ScanAndInstall();
    }

    /// <summary>외부에서 동적으로 생성된 버튼이 있는 경우 호출 가능.</summary>
    public static void Rescan()
    {
        var buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Install(buttons);
    }

    private void ScanAndInstall()
    {
        var buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Install(buttons);
    }

    private static void Install(Button[] buttons)
    {
        if (buttons == null) return;
        foreach (var btn in buttons)
        {
            if (btn == null || _installed.Contains(btn)) continue;
            btn.onClick.AddListener(PlayClickSfx);
            _installed.Add(btn);
        }
    }

    private static void PlayClickSfx()
    {
        AudioManager.Instance?.PlaySfxById(SfxId.ButtonClick);
    }
}
