// ShieldBarUI.cs — HP 슬라이더 위에 실드를 파란색으로 표시하는 UI 컴포넌트.
// DefaultSetting.SpawnCard() 에서 Init() 을 호출해 초기화한다.

using UnityEngine;
using UnityEngine.UI;

public class ShieldBarUI : MonoBehaviour
{
    private Image      _shieldImage;
    private FellowData _fellow;
    private Slider     _slider;

    /// <summary>
    /// 슬라이더와 동료 데이터를 연결하고 파란 실드 이미지를 생성한다.
    /// DefaultSetting.SpawnCard() 에서 InitHp() 직후 호출.
    /// </summary>
    public void Init(FellowData fellow, Slider slider)
    {
        _fellow = fellow;
        _slider = slider;

        if (slider == null || slider.fillRect == null)
        {
            Debug.LogWarning("[ShieldBarUI] slider 또는 fillRect 가 없습니다.");
            return;
        }

        // Fill image 의 부모(Fill Area) 에 실드 이미지를 자식으로 추가
        var fillArea = slider.fillRect.parent;

        var shieldObj = new GameObject("ShieldFill");
        shieldObj.transform.SetParent(fillArea, false);

        _shieldImage               = shieldObj.AddComponent<Image>();
        _shieldImage.color         = new Color(0.2f, 0.6f, 1f, 0.75f);
        _shieldImage.raycastTarget = false;

        // 위치는 Refresh() 에서 anchor로 설정
        var rt = shieldObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // HP/실드 변경 시 자동 갱신
        fellow.OnHpChanged     += _ => Refresh();
        fellow.OnShieldChanged += Refresh;

        _shieldImage.gameObject.SetActive(false);
        Refresh();
    }

    private void Refresh()
    {
        if (_fellow == null || _shieldImage == null) return;

        int maxHp  = _fellow.data != null ? _fellow.data.maxHp : 100;
        int shield = _fellow.shield;
        int hp     = _fellow.CurrentHp;

        if (shield <= 0 || maxHp <= 0)
        {
            _shieldImage.gameObject.SetActive(false);
            return;
        }

        _shieldImage.gameObject.SetActive(true);

        float hpRatio     = Mathf.Clamp01((float)hp / maxHp);
        float shieldRatio = Mathf.Clamp01((float)shield / maxHp);

        // Max HP 포함 모든 상황에서 실드를 표시:
        // shieldEnd를 먼저 1f로 클램프하고, shieldStart를 오른쪽 기준으로 역산.
        // HP=Max일 때 실드가 HP바 우측부터 왼쪽으로 파란 영역으로 표시됨.
        float shieldEnd   = Mathf.Min(1f, hpRatio + shieldRatio);
        float shieldStart = Mathf.Max(0f, shieldEnd - shieldRatio);

        var rt       = _shieldImage.rectTransform;
        rt.anchorMin = new Vector2(shieldStart, 0f);
        rt.anchorMax = new Vector2(shieldEnd, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
