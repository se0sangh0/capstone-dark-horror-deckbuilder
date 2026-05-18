// SettingPopup.cs
// 설정 팝업 컨트롤러. PanelBase 의 DOTween 페이드 패턴을 따름.
//
// ── 항목 ────────────────────────────────────────────────────────
//   - BGM 음량 슬라이더 (0~1)
//   - SFX 음량 슬라이더 (0~1)
//   - 창모드 토글 (FullScreen on/off, 1920x1080 고정)
//   - "메인화면으로" 버튼 → GameStartScene 로드
//   - 닫기 버튼 → Close() 호출
//
// ── 인스펙터 ───────────────────────────────────────────────────
//   SettingPopup.prefab 루트에 부착하고 슬라이더/토글/버튼 연결.
//   CanvasGroup alpha=0 으로 두면 PanelBase 가 페이드 인 처리.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingPopup : PanelBase
{
    private const string KEY_FULLSCREEN = "settings_fullscreen";
    private const string MAIN_SCENE     = "GameStartScene";

    [Header("음량")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("창모드")]
    [SerializeField] private Toggle fullscreenToggle;

    [Header("버튼")]
    [SerializeField] private Button toMainButton;
    [SerializeField] private Button closeButton;

    // ── 열릴 때마다 현재 값 동기화 + 리스너 등록 ────────────────────
    protected override void OnOpened()
    {
        if (AudioManager.Instance != null)
        {
            if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(AudioManager.Instance.BgmVolume);
            if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(AudioManager.Instance.SfxVolume);
        }

        bool isFullscreen = PlayerPrefs.GetInt(KEY_FULLSCREEN, Screen.fullScreen ? 1 : 0) == 1;
        if (fullscreenToggle != null) fullscreenToggle.SetIsOnWithoutNotify(isFullscreen);

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
        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
            if (subscribe) fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }
        if (toMainButton != null)
        {
            toMainButton.onClick.RemoveListener(OnToMain);
            if (subscribe) toMainButton.onClick.AddListener(OnToMain);
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

    private void OnFullscreenChanged(bool isOn)
    {
        Screen.SetResolution(1920, 1080, isOn);
        PlayerPrefs.SetInt(KEY_FULLSCREEN, isOn ? 1 : 0);
    }

    private void OnToMain() => SceneManager.LoadScene(MAIN_SCENE);
}
