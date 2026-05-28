// AudioManager.cs
// BGM / SFX 음량 관리 + ID 기반 재생 싱글톤.
//
// ── 역할 ────────────────────────────────────────────────────────
//   - 배경음악(BGM) AudioSource 보유 (loop 재생)
//   - 효과음(SFX) AudioSource 보유 (PlayOneShot 용)
//   - 음량 0~1 PlayerPrefs 저장/복원
//   - SoundDatabase (Resources/Audio/SoundDatabase.asset) 자동 로드
//   - PlayBgmById(BgmId) / PlaySfxById(SfxId) — 매핑 기반 재생
//
// ── 사용 예 ─────────────────────────────────────────────────────
//   AudioManager.Instance.PlayBgmById(BgmId.Battle);
//   AudioManager.Instance.PlaySfxById(SfxId.CardDraw);
//   AudioManager.Instance.SetBgmVolume(0.5f);
//
// ── 영속 ────────────────────────────────────────────────────────
//   persistAcrossScenes 체크박스 → 씬 전환 시 유지.

using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    private const string KEY_BGM = "audio_bgm_volume";
    private const string KEY_SFX = "audio_sfx_volume";

    private const string RES_PATH = "Audio/SoundDatabase";

    [Header("AudioSource (씬 인스펙터에서 자식 AudioSource 연결)")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("사운드 데이터 (비워두면 Resources/Audio/SoundDatabase 자동 로드)")]
    [SerializeField] private SoundDatabase database;

    public float BgmVolume { get; private set; } = 1f;
    public float SfxVolume { get; private set; } = 1f;

    private BgmId _currentBgm = BgmId.None;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        BgmVolume = PlayerPrefs.GetFloat(KEY_BGM, 1f);
        SfxVolume = PlayerPrefs.GetFloat(KEY_SFX, 1f);

        if (bgmSource != null) bgmSource.volume = BgmVolume;
        if (sfxSource != null) sfxSource.volume = SfxVolume;

        if (database == null)
            database = Resources.Load<SoundDatabase>(RES_PATH);
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

    // ─── 직접 클립 재생 ───────────────────────────────────────────
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
        _currentBgm = BgmId.None;
    }

    public void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, SfxVolume);
    }

    // ─── ID 기반 재생 ────────────────────────────────────────────
    /// <summary>현재 BGM 이 동일하면 재시작하지 않는다 (씬 전환 시 끊김 방지).</summary>
    public void PlayBgmById(BgmId id)
    {
        if (id == BgmId.None) { StopBgm(); return; }
        if (_currentBgm == id && bgmSource != null && bgmSource.isPlaying) return;
        if (database == null) return;
        var clip = database.GetBgm(id);
        if (clip == null) return;
        PlayBgm(clip);
        _currentBgm = id;
    }

    public void PlaySfxById(SfxId id)
    {
        if (id == SfxId.None || database == null) return;
        var clip = database.GetSfx(id);
        if (clip != null) PlaySfx(clip);
    }
}
