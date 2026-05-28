// SoundDatabase.cs
// BgmId / SfxId → AudioClip 매핑을 보관하는 ScriptableObject.
//
// ── 배치 ────────────────────────────────────────────────────────
//   Assets/Resources/Audio/SoundDatabase.asset
//   AudioManager.Awake 에서 Resources.Load 로 자동 로드.
//
// ── 인스펙터에서 클립 할당 ──────────────────────────────────────
//   bgmEntries / sfxEntries 배열에 (id, clip) 쌍을 등록.
//   id 가 중복되면 마지막 항목이 우선한다 (Dictionary 빌드 시 덮어쓰기).

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundDatabase", menuName = "Audio/SoundDatabase")]
public class SoundDatabase : ScriptableObject
{
    [Serializable]
    public class BgmEntry
    {
        public BgmId    id;
        public AudioClip clip;
    }

    [Serializable]
    public class SfxEntry
    {
        public SfxId    id;
        public AudioClip clip;
    }

    [SerializeField] private List<BgmEntry> bgmEntries = new();
    [SerializeField] private List<SfxEntry> sfxEntries = new();

    private Dictionary<BgmId, AudioClip> _bgmMap;
    private Dictionary<SfxId, AudioClip> _sfxMap;

    private void EnsureMaps()
    {
        if (_bgmMap == null)
        {
            _bgmMap = new Dictionary<BgmId, AudioClip>(bgmEntries.Count);
            foreach (var e in bgmEntries)
            {
                if (e == null || e.clip == null) continue;
                _bgmMap[e.id] = e.clip;
            }
        }
        if (_sfxMap == null)
        {
            _sfxMap = new Dictionary<SfxId, AudioClip>(sfxEntries.Count);
            foreach (var e in sfxEntries)
            {
                if (e == null || e.clip == null) continue;
                _sfxMap[e.id] = e.clip;
            }
        }
    }

    public AudioClip GetBgm(BgmId id)
    {
        EnsureMaps();
        return _bgmMap.TryGetValue(id, out var clip) ? clip : null;
    }

    public AudioClip GetSfx(SfxId id)
    {
        EnsureMaps();
        return _sfxMap.TryGetValue(id, out var clip) ? clip : null;
    }
}
