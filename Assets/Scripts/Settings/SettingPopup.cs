// SettingPopup.cs
// 설정 팝업 컨트롤러. PanelBase 의 DOTween 페이드 패턴을 따름.
//
// ── 항목 ────────────────────────────────────────────────────────
//   - BGM 음량 슬라이더 (0~1)
//   - SFX 음량 슬라이더 (0~1)
//   - 화면 모드 드롭다운 (전체화면 / 전체화면 창모드 / 창모드) — 해상도 1920x1080 고정
//   - "메인화면으로" 버튼 → GameStartScene 로드
//   - 게임 종료 / 재화 초기화 버튼
//   - 닫기 버튼 → Close() 호출
//
// ── 인스펙터 ───────────────────────────────────────────────────
//   SettingPopup.prefab 루트에 부착하고 슬라이더/드롭다운/버튼 연결.
//   CanvasGroup alpha=0 으로 두면 PanelBase 가 페이드 인 처리.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class SettingPopup : PanelBase
{
    private const string KEY_SCREEN_MODE = "settings_screen_mode";
    private const string KEY_RESOLUTION  = "settings_resolution_idx";
    private const string MAIN_SCENE      = "GameStartScene";

    // 드롭다운 인덱스 → FullScreenMode 매핑 (UI 표시 순서와 일치)
    private static readonly FullScreenMode[] ScreenModes =
    {
        FullScreenMode.ExclusiveFullScreen, // 0 — 전체화면
        FullScreenMode.FullScreenWindow,    // 1 — 전체화면 (창모드)
        FullScreenMode.Windowed,            // 2 — 창모드
    };

    private static readonly string[] ScreenModeLabels =
    {
        "전체화면",
        "전체화면 (창모드)",
        "창모드",
    };

    // 해상도 옵션 — 추가하려면 여기 배열만 늘리면 됨. (16:9 기준 + 일반 PC 해상도)
    private static readonly Vector2Int[] Resolutions =
    {
        new Vector2Int(3840, 2160), // 4K
        new Vector2Int(2560, 1440), // QHD
        new Vector2Int(1920, 1080), // Full HD
        new Vector2Int(1600,  900),
        new Vector2Int(1366,  768),
        new Vector2Int(1280,  720), // HD
    };

    [Header("음량")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("화면 모드")]
    [Tooltip("TMP_Dropdown — 옵션은 OnOpened 에서 자동으로 채움 (한국어 라벨 3개).")]
    [SerializeField] private TMP_Dropdown screenModeDropdown;

    [Header("해상도")]
    [Tooltip("TMP_Dropdown — Resolutions 배열에서 자동으로 채움 (\"1920x1080\" 등).")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    [Header("버튼")]
    [SerializeField] private Button toMainButton;
    [SerializeField] private Button resetPrefsButton;
    [SerializeField] private Button closeButton;

    // ── 열릴 때마다 현재 값 동기화 + 리스너 등록 ────────────────────
    protected override void OnOpened()
    {
        if (AudioManager.Instance != null)
        {
            if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(AudioManager.Instance.BgmVolume);
            if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(AudioManager.Instance.SfxVolume);
        }

        if (screenModeDropdown != null)
        {
            // 옵션을 매번 다시 채워서 prefab 에 옵션이 비어 있어도 안전.
            screenModeDropdown.ClearOptions();
            var opts = new List<TMP_Dropdown.OptionData>(ScreenModeLabels.Length);
            foreach (var label in ScreenModeLabels) opts.Add(new TMP_Dropdown.OptionData(label));
            screenModeDropdown.AddOptions(opts);

            int savedIdx = PlayerPrefs.GetInt(KEY_SCREEN_MODE, ResolveCurrentScreenModeIndex());
            savedIdx = Mathf.Clamp(savedIdx, 0, ScreenModes.Length - 1);
            screenModeDropdown.SetValueWithoutNotify(savedIdx);
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            var opts = new List<TMP_Dropdown.OptionData>(Resolutions.Length);
            foreach (var r in Resolutions) opts.Add(new TMP_Dropdown.OptionData($"{r.x}x{r.y}"));
            resolutionDropdown.AddOptions(opts);

            int savedIdx = PlayerPrefs.GetInt(KEY_RESOLUTION, ResolveCurrentResolutionIndex());
            savedIdx = Mathf.Clamp(savedIdx, 0, Resolutions.Length - 1);
            resolutionDropdown.SetValueWithoutNotify(savedIdx);
        }

        // 씬 컨텍스트별 ToMainButton 처리.
        // GameStartScene 에서는 GameStartScene 의 Exit 버튼과 중복이라 숨김.
        // 다른 씬(게임 진행 중)에서는 표시 + 라벨 "메인화면으로" 로 강제.
        if (toMainButton != null)
        {
            bool isMainScene = SceneManager.GetActiveScene().name == MAIN_SCENE;
            toMainButton.gameObject.SetActive(!isMainScene);
            if (!isMainScene)
            {
                var label = toMainButton.GetComponentInChildren<TMP_Text>(true);
                if (label != null) label.text = "메인화면으로";
            }
        }

        BindListeners(true);
    }

    // ── 닫히기 직전 리스너 해제 ────────────────────────────────────
    protected override void OnClosed() => BindListeners(false);

    private void BindListeners(bool subscribe)
    {
        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.RemoveListener(OnBgmChanged);
            if (subscribe) bgmSlider.onValueChanged.AddListener(OnBgmChanged);
        }
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
            if (subscribe) sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        }
        if (screenModeDropdown != null)
        {
            screenModeDropdown.onValueChanged.RemoveListener(OnScreenModeChanged);
            if (subscribe) screenModeDropdown.onValueChanged.AddListener(OnScreenModeChanged);
        }
        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
            if (subscribe) resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }
        if (toMainButton != null)
        {
            toMainButton.onClick.RemoveListener(OnToMain);
            if (subscribe) toMainButton.onClick.AddListener(OnToMain);
        }
        if (resetPrefsButton != null)
        {
            resetPrefsButton.onClick.RemoveListener(OnResetPrefs);
            if (subscribe) resetPrefsButton.onClick.AddListener(OnResetPrefs);
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
            if (subscribe) closeButton.onClick.AddListener(Close);
        }
    }

    private void OnBgmChanged(float v)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetBgmVolume(v);
    }

    private void OnSfxChanged(float v)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetSfxVolume(v);
    }

    private void OnScreenModeChanged(int idx)
    {
        idx = Mathf.Clamp(idx, 0, ScreenModes.Length - 1);
        var res = GetCurrentResolution();
        Screen.SetResolution(res.x, res.y, ScreenModes[idx]);
        PlayerPrefs.SetInt(KEY_SCREEN_MODE, idx);
    }

    private void OnResolutionChanged(int idx)
    {
        idx = Mathf.Clamp(idx, 0, Resolutions.Length - 1);
        var res = Resolutions[idx];
        Screen.SetResolution(res.x, res.y, Screen.fullScreenMode);
        PlayerPrefs.SetInt(KEY_RESOLUTION, idx);
    }

    private Vector2Int GetCurrentResolution()
    {
        int idx = resolutionDropdown != null
            ? resolutionDropdown.value
            : PlayerPrefs.GetInt(KEY_RESOLUTION, 0);
        idx = Mathf.Clamp(idx, 0, Resolutions.Length - 1);
        return Resolutions[idx];
    }

    private static int ResolveCurrentScreenModeIndex()
    {
        var current = Screen.fullScreenMode;
        for (int i = 0; i < ScreenModes.Length; i++)
            if (ScreenModes[i] == current) return i;
        return 1; // 폴백: FullScreenWindow
    }

    private static int ResolveCurrentResolutionIndex()
    {
        int w = Screen.width, h = Screen.height;
        for (int i = 0; i < Resolutions.Length; i++)
            if (Resolutions[i].x == w && Resolutions[i].y == h) return i;
        return 0; // 폴백: 첫 번째
    }

    private void OnToMain() => SceneManager.LoadScene(MAIN_SCENE);

    // 영혼석/마석 PlayerPrefs 만 삭제 후 활성 매니저에 즉시 반영.
    // 풀스크린/볼륨 같은 다른 prefs 는 유지.
    private void OnResetPrefs()
    {
        if (SoulstoneManager.Instance != null) SoulstoneManager.Instance.ResetCurrency();
        if (ManastoneManager.Instance != null) ManastoneManager.Instance.ResetCurrency();
        Debug.Log("[SettingPopup] 영혼석/마석 PlayerPrefs 리셋 완료");
    }
}
