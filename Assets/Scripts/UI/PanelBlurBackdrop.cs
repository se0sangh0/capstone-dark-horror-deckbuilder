// ============================================================
// UI/PanelBlurBackdrop.cs
// 패널 뒤 화면 캡처·블러 배경 — 시선 집중용 (2026-08-21)
// ============================================================
//
// [이 파일이 하는 일]
//   씬 배치형 패널(PanelBase)이 열릴 때 뒤 화면을 캡처·블러해
//   전체 화면 배경으로 깔아 패널 UI 에 시선을 집중시킵니다.
//   EventPanel 의 캡처+다운샘플 블러 방식과 동일한 시각 언어를 사용합니다.
//
// [사용법]
//   panel.Open() 대신 PanelBlurBackdrop.CaptureThenOpen(panel) 을 호출.
//   패널이 닫히면(OnClosedEvent) 배경도 자동으로 숨고 RT 를 반납한다.
//
// [사용처]
//   - Church/ChurchPanel.OpenFromNode() — 교회 진입 시 배경 블러
// ============================================================

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PanelBlurBackdrop : MonoBehaviour
{
    private PanelBase     _owner;
    private RawImage      _image;
    private RenderTexture _rt;
    private Coroutine     _routine;

    // EventPanel 과 동일 팔레트 — 블러 위 어두운 틴트 / 캡처 실패 시 단색 딤
    private static readonly Color BlurTint    = new Color(0.55f, 0.55f, 0.58f, 1f);
    private static readonly Color DimFallback = new Color(0f, 0f, 0f, 0.62f);

    /// <summary>
    /// 뒤 화면을 캡처·블러해 배경으로 깐 뒤 패널을 연다.
    /// 캔버스가 없거나 캡처가 불가능하면 블러 없이(또는 단색 딤으로) 연다.
    /// </summary>
    public static void CaptureThenOpen(PanelBase panel)
    {
        if (panel == null) return;
        var backdrop = panel.GetComponent<PanelBlurBackdrop>();
        if (backdrop == null) backdrop = panel.gameObject.AddComponent<PanelBlurBackdrop>();
        backdrop.OpenWithBlur();
    }

    private void OpenWithBlur()
    {
        if (_owner == null) _owner = GetComponent<PanelBase>();
        if (_owner == null)
        {
            Debug.LogWarning("[PanelBlurBackdrop] PanelBase 없음 — 블러 생략");
            return;
        }

        EnsureImage();
        if (_image == null)
        {
            _owner.Open(); // 캔버스를 못 찾음 — 블러 없이 열기
            return;
        }

        if (_routine != null) StopCoroutine(_routine);
        if (isActiveAndEnabled) _routine = StartCoroutine(CaptureThenOpenRoutine());
        else _owner.Open(); // 비활성 폴백 (블러 없이)
    }

    /// <summary>패널이 그려지기 전 프레임(=뒤 화면)을 캡처·블러해 배경에 깔고 패널을 연다.</summary>
    private IEnumerator CaptureThenOpenRoutine()
    {
        yield return new WaitForEndOfFrame();

        Texture2D shot = null;
        try { shot = ScreenCapture.CaptureScreenshotAsTexture(); }
        catch (Exception e) { Debug.LogWarning($"[PanelBlurBackdrop] 화면 캡처 실패 — 단색 딤으로 대체. {e.Message}"); }

        if (shot != null)
        {
            Release();
            _rt = BuildBlur(shot);
            _image.texture = _rt;
            _image.color   = BlurTint;
            Destroy(shot);
        }
        else
        {
            _image.texture = null;
            _image.color   = DimFallback;
        }

        _routine = null;
        _image.gameObject.SetActive(true);

        // 패널 닫힘에 맞춰 배경 자동 해제 (중복 구독 방지 후 구독)
        _owner.OnClosedEvent -= HandleOwnerClosed;
        _owner.OnClosedEvent += HandleOwnerClosed;

        _owner.Open();                            // PanelBase 가 패널을 맨 앞으로 올림
        _image.transform.SetAsLastSibling();      // 배경을 그 아래(끝에서 두 번째)로
        _owner.transform.SetAsLastSibling();
    }

    private void HandleOwnerClosed()
    {
        if (_owner != null) _owner.OnClosedEvent -= HandleOwnerClosed;
        if (_image != null) _image.gameObject.SetActive(false);
        Release();
    }

    /// <summary>전체 화면 RawImage 배경을 캔버스 아래에 1회 생성 (기본 비활성).</summary>
    private void EnsureImage()
    {
        if (_image != null) return;

        var canvas = GetComponentInParent<Canvas>(true);
        if (canvas == null)
        {
            Debug.LogWarning("[PanelBlurBackdrop] 상위 Canvas 없음 — 블러 배경 생성 불가");
            return;
        }

        var go = new GameObject($"{name}_BlurBackdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        go.transform.SetParent(canvas.transform, false);
        go.layer = canvas.gameObject.layer;

        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        _image = go.GetComponent<RawImage>();
        _image.color = DimFallback;
        _image.raycastTarget = true; // 뒤 UI 클릭 차단 — 패널에 시선·입력 집중
        go.SetActive(false);
    }

    // ── 블러 — EventPanel.BuildBlur 와 동일한 다단계 다운샘플(바이리니어) 방식 ──
    /// <summary>src 를 절반씩 축소(2×2 평균)해 소형 RT 로 만든다. 전체 화면 업스케일 시 부드러운 블러가 된다.</summary>
    private static RenderTexture BuildBlur(Texture2D src)
    {
        int w = Mathf.Max(1, src.width);
        int h = Mathf.Max(1, src.height);

        RenderTexture cur = RenderTexture.GetTemporary(w, h, 0);
        cur.filterMode = FilterMode.Bilinear;
        Graphics.Blit(src, cur);

        while (w > 120 && h > 120)
        {
            w = Mathf.Max(1, w / 2);
            h = Mathf.Max(1, h / 2);
            var next = RenderTexture.GetTemporary(w, h, 0);
            next.filterMode = FilterMode.Bilinear;
            Graphics.Blit(cur, next);
            RenderTexture.ReleaseTemporary(cur);
            cur = next;
        }

        var outRt = new RenderTexture(w, h, 0) { filterMode = FilterMode.Bilinear };
        Graphics.Blit(cur, outRt);
        RenderTexture.ReleaseTemporary(cur);
        return outRt;
    }

    /// <summary>블러 RT 반납 — 메모리 누수 방지.</summary>
    private void Release()
    {
        if (_image != null) _image.texture = null;
        if (_rt != null)
        {
            _rt.Release();
            Destroy(_rt);
            _rt = null;
        }
    }

    private void OnDestroy()
    {
        if (_routine != null) { StopCoroutine(_routine); _routine = null; }
        if (_owner != null) _owner.OnClosedEvent -= HandleOwnerClosed;
        Release();
        if (_image != null) Destroy(_image.gameObject);
    }
}
