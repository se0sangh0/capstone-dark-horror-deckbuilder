// ============================================================
// UI/SkillEffectPlayer.cs
// 스킬 이펙트 프레임 애니메이션 재생기 (2026-06-10)
// ============================================================
//
// mp4 → ffmpeg 흰배경 키잉 → Resources/SkillFX/{key}/f_##.png (알파 스프라이트) 를
// SpriteRenderer 로 프레임 순차 재생 후 자동 파괴. 월드 공간(전투 캐릭터 위)에 스폰.
// 투사체(Projectile)는 from→to 로 이동하며 재생, 타격/버프는 한 위치에 고정.
// SkillEffectFx.Play(...) 가 GameObject 생성 + 이 컴포넌트로 재생을 시작한다.
// ============================================================

using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SkillEffectPlayer : MonoBehaviour
{
    private Sprite[] _frames;
    private float    _fps;
    private float    _elapsed;
    private int      _shown = -1;
    private Vector3  _from, _to;
    private bool     _travel;
    private SpriteRenderer _sr;

    /// <summary>프레임 시퀀스를 fromPos→toPos(이동) 로 worldHeight 크기로 재생(1회) 후 파괴. from==to 면 고정.
    /// flipX=true 면 좌우 반전(아군은 오른쪽을 향해 공격하므로 원본이 왼쪽을 보는 이펙트에 사용).</summary>
    public void Play(Sprite[] frames, float fps, Vector3 fromPos, Vector3 toPos, float worldHeight, int sortingOrder, bool flipX = false)
    {
        _frames  = frames;
        _fps     = fps > 0f ? fps : 18f;
        _elapsed = 0f;
        _shown   = -1;
        _from    = fromPos;
        _to      = toPos;
        _travel  = (toPos - fromPos).sqrMagnitude > 0.0001f;

        _sr = GetComponent<SpriteRenderer>();
        _sr.sortingOrder = sortingOrder;
        _sr.flipX = flipX;
        transform.position = fromPos;

        if (_frames != null && _frames.Length > 0)
        {
            var f0 = _frames[0];
            float spriteH = f0.rect.height / f0.pixelsPerUnit;
            float s = spriteH > 0.0001f ? worldHeight / spriteH : 1f;
            transform.localScale = new Vector3(s, s, 1f);
            _sr.sprite = f0;
            _shown = 0;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (_frames == null || _frames.Length == 0) return;

        _elapsed += Time.deltaTime;
        int frame = Mathf.FloorToInt(_elapsed * _fps);
        if (frame >= _frames.Length)
        {
            Destroy(gameObject);
            return;
        }
        if (frame != _shown)
        {
            _shown = frame;
            _sr.sprite = _frames[frame];
        }
        if (_travel)
        {
            float p = _frames.Length > 1 ? Mathf.Clamp01((float)frame / (_frames.Length - 1)) : 1f;
            transform.position = Vector3.Lerp(_from, _to, p);
        }
    }
}
