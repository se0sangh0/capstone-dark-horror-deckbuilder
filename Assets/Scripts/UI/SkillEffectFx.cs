// ============================================================
// UI/SkillEffectFx.cs
// 스킬 ID → 이펙트(Resources/SkillFX/{key}) 매핑 + 스폰 (2026-06-10)
// ============================================================
//
// 사용: SkillEffectFx.Play(skill.id, casterPos, targetPos)  — BattleManager.UseSkill 에서 호출.
//   이펙트 종류(FxKind)에 따라 위치/이동 결정:
//     Projectile : 시전자 지팡이→타겟 으로 이동(날아감). 예) 파이어볼, 매직미사일.
//     AtTarget   : 타겟(적) 위치 고정. 예) 발도·일섬·무모한강타(타격).
//     AtCaster   : 시전자 위치 고정. 예) 방어준비·전투태세·불굴·기원·별부름(버프/힐).
//   좌우 반전: 아군은 오른쪽(적)을 향해 공격 → 원본이 왼쪽을 보는 이펙트는 flip.
//     nativeRight = 원본 프레임이 '오른쪽'을 향하는가. 공격 방향(시전자→타겟)과 다르면 반전.
//   매핑에 없는 스킬은 조용히 무시. 프레임은 1회 로드 후 캐시.
//
// 이펙트 자산: Effect/*.mp4 → ffmpeg colorkey(흰배경 제거) → Resources/SkillFX/{key}/f_##.png
// ============================================================

using System.Collections.Generic;
using UnityEngine;

public static class SkillEffectFx
{
    private enum FxKind { Projectile, AtTarget, AtCaster }

    // skillId → (폴더키, fps, 월드높이, 종류, 원본이오른쪽향함).
    private struct Fx
    {
        public string  key; public float fps; public float height; public FxKind kind; public bool nativeRight;
        public Fx(string k, float f, float h, FxKind kd, bool nr){ key=k; fps=f; height=h; kind=kd; nativeRight=nr; }
    }

    // 투사체 발사 오프셋(몸 중앙) / 적 조준 높이(상체). — 지팡이 끝(높이 1.15 + 전방 0.55)은 어색해 몸쪽으로 변경 (사용자 요청 2026-06-11)
    private static readonly Vector3 StaffUp     = new Vector3(0f, 0.7f, 0f);  // 몸통(가슴) 높이
    private const  float            StaffFwd    = 0f;                          // 전방 오프셋 제거 — 몸에서 발사
    private static readonly Vector3 EnemyBody   = new Vector3(0f, 0.7f, 0f);  // 적 상체 조준

    private static readonly Dictionary<string, Fx> Registry = new Dictionary<string, Fx>
    {
        // 투사체 — 지팡이→적 이동
        { "skill_fireball",      new Fx("fireball",        22f, 2.8f, FxKind.Projectile, true)  }, // 폭발(머리) 오른쪽
        { "skill_magic_missile", new Fx("magic_missile",   17f, 2.4f, FxKind.Projectile, false) }, // 화살촉 왼쪽 → 반전
        // 타격 — 적 위치 (근접 휘두름 타이밍에 발동)
        { "skill_reckless",      new Fx("reckless_strike", 17f, 2.2f, FxKind.AtTarget, false)   }, // 타격부 왼쪽 → 반전
        { "skill_draw",          new Fx("iaido",           17f, 2.8f, FxKind.AtTarget, false)   }, // 발도, 왼쪽 → 반전
        { "skill_flash",         new Fx("flash_slash",     17f, 2.4f, FxKind.AtTarget, false)   }, // 일섬, 왼쪽 → 반전
        // 버프/힐 — 시전자 위치 (별부름은 아군 힐이라 시전자 측, 대칭이라 반전 불필요)
        { "skill_starlight",     new Fx("star_call",       17f, 2.6f, FxKind.AtCaster, true)    },
        { "skill_guard",         new Fx("defense_ready",   17f, 2.2f, FxKind.AtCaster, true)    },
        { "skill_battle_stance", new Fx("battle_stance",   33f, 2.6f, FxKind.AtCaster, true)    },
        { "skill_indomitable",   new Fx("indomitable",     33f, 2.4f, FxKind.AtCaster, true)    },
        { "skill_prayer",        new Fx("prayer",          14f, 2.4f, FxKind.AtCaster, true)    },
    };

    private static readonly Dictionary<string, Sprite[]> _cache = new Dictionary<string, Sprite[]>();

    private static Sprite[] LoadFrames(string key)
    {
        if (!_cache.TryGetValue(key, out var arr))
        {
            arr = Resources.LoadAll<Sprite>("SkillFX/" + key);
            if (arr != null) System.Array.Sort(arr, (a, b) => string.CompareOrdinal(a.name, b.name));
            _cache[key] = arr;
        }
        return arr;
    }

    /// <summary>스킬 이펙트 재생. 종류에 따라 지팡이→타겟 이동 / 타겟 고정 / 시전자 고정. 공격 방향에 맞춰 좌우 반전.</summary>
    public static void Play(string skillId, Vector3 casterPos, Vector3 targetPos, int sortingOrder = 50)
    {
        if (string.IsNullOrEmpty(skillId) || !Registry.TryGetValue(skillId, out var fx)) return;
        var frames = LoadFrames(fx.key);
        if (frames == null || frames.Length == 0)
        {
            Debug.LogWarning($"[SkillEffectFx] 프레임 없음: SkillFX/{fx.key} (skill {skillId})");
            return;
        }

        // 공격 방향: 타겟이 시전자보다 오른쪽이면 오른쪽 공격(아군 기본). 원본 방향과 다르면 반전.
        bool desiredRight = targetPos.x >= casterPos.x;
        bool flipX = desiredRight != fx.nativeRight;
        float sideX = desiredRight ? 1f : -1f;

        Vector3 from, to;
        switch (fx.kind)
        {
            case FxKind.Projectile:
                from = casterPos + StaffUp + new Vector3(StaffFwd * sideX, 0f, 0f); // 지팡이 끝
                to   = targetPos + EnemyBody;                                       // 적 상체
                break;
            case FxKind.AtTarget:
                from = to = targetPos;
                break;
            default: // AtCaster
                from = to = casterPos;
                break;
        }

        var go = new GameObject("SkillFX_" + fx.key);
        var player = go.AddComponent<SkillEffectPlayer>();
        player.Play(frames, fx.fps, from, to, fx.height, sortingOrder, flipX);
    }

    /// <summary>이 스킬에 이펙트가 등록돼 있는지.</summary>
    public static bool Has(string skillId) => !string.IsNullOrEmpty(skillId) && Registry.ContainsKey(skillId);

    /// <summary>이펙트 재생 길이(초) = 프레임수 / fps. 원거리 '이펙트 끝나고 데미지' 대기에 사용. 미등록=0.</summary>
    public static float GetDuration(string skillId)
    {
        if (string.IsNullOrEmpty(skillId) || !Registry.TryGetValue(skillId, out var fx)) return 0f;
        var frames = LoadFrames(fx.key);
        if (frames == null || frames.Length == 0) return 0f;
        float fps = fx.fps > 0f ? fx.fps : 18f;
        return frames.Length / fps;
    }
}
