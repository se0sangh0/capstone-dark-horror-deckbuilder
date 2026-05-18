// AudioManager.cs
// BGM / SFX 음량 관리 싱글톤.
//
// ── 역할 ────────────────────────────────────────────────────────
//   - 배경음악(BGM) AudioSource 보유 (loop 재생)
//   - 효과음(SFX) AudioSource 보유 (PlayOneShot 용)
//   - 음량 0~1 PlayerPrefs 저장/복원
//
// ── 사용 예 ─────────────────────────────────────────────────────
//   AudioManager.Instance.SetBgmVolume(0.5f);
//   AudioManager.Instance.PlayBgm(clip);
//   AudioManager.Instance.PlaySfx(clip);
//
// ── 영속 ────────────────────────────────────────────────────────
//   persistAcrossScenes 체크박스 → 씬 전환 시 유지.

using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    private const string KEY_BGM = "audio_bgm_volume";
    private const string KEY_SFX = "audio_sfx_volume";

    [Header("AudioSource (씬 인스펙터에서 자식 AudioSource 연결)")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    public float BgmVolume { get; private set; } = 1f;
    public float SfxVolume { get; private set; } = 1f;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        BgmVolume = PlayerPrefs.GetFloat(KEY_BGM, 1f);
        SfxVolume = PlayerPrefs.GetFloat(KEY_SFX, 1f);

        if (bgmSource != null) bgmSource.volume = BgmVolume;
        if (sfxSource != null) sfxSource.volume = SfxVolume;
    }

    public void SetBgmVolume(float v)
    {
        BgmVolume = Mathf.Clamp01(v);
        if (bgmSource != null) bgmSource.volume = BgmVolume;
        PlayerPrefs.SetFloat(KEY_BGM, BgmVolume);
    }

    public void SetSfxVolume(float v)
    {
        SfxVolume = Mathf.Clamp01(v);
        if (sfxSource != null) sfxSource.volume = SfxVolume;
        PlayerPrefs.SetFloat(KEY_SFX, SfxVolume);
    }

    public void PlayBgm(AudioClip clip)
    {
        if (bgmSource == null || clip == null) return;
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBgm()
    {
        if (bgmSource != null) bgmSource.Stop();
    }

    public void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, SfxVolume);
    }
}
