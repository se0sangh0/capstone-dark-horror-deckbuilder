// BattleCardView.cs
// 전투 카드(MyObject / EnemyObject) 의 이름·HP 점수·HP 색상·데미지 팝업 통합 뷰.
//
// ── 사용 흐름 ───────────────────────────────────────────────────
//   DefaultSetting.SpawnCard() 가 BattleCardView 를 찾아 BindFellow/BindEnemy 호출.
//   이후 FellowData/EnemyData.OnHpChanged 이벤트로 자동 갱신:
//     - HP 스코어 텍스트  "현재/최대"
//     - HP Fill 색상      ratio>0.5 녹 / >0.25 노랑 / 이하 빨강
//     - 데미지 팝업        감소분이 있을 때 DamagePopup 생성
//
// ── 인스펙터 ───────────────────────────────────────────────────
//   nameText           : 카드 위 캐릭터 이름 텍스트
//   hpScoreText        : HP 슬라이더 옆/아래 "현재/최대" 텍스트
//   damagePopupPrefab  : 데미지 숫자가 떠오르는 DamagePopup prefab
//   damagePopupAnchor  : 팝업이 생성될 위치 (보통 카드 중앙). 비어있으면 transform.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleCardView : MonoBehaviour
{
    [Header("기본 표시")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text hpScoreText;

    [Header("HP Slider (자동 검색 가능)")]
    [SerializeField] private Slider hpSlider;

    [Header("데미지 팝업")]
    [SerializeField] private DamagePopup damagePopupPrefab;
    [SerializeField] private Transform   damagePopupAnchor;

    private Image       _fillImage;
    private FellowData  _fellow;
    private EnemyData   _enemy;
    private int         _lastHp = -1;

    // ── 외부에서 호출 ─────────────────────────────────────────────
    public void BindFellow(FellowData fellow)
    {
        Unbind();
        _fellow = fellow;
        if (nameText != null) nameText.text = !string.IsNullOrEmpty(fellow.displayName) ? fellow.displayName : fellow.id;
        ResolveSlider();
        fellow.OnHpChanged += OnFellowHpChanged;
        _lastHp = fellow.CurrentHp;
        Refresh(_lastHp, fellow.maxHp);
    }

    public void BindEnemy(EnemyData enemy)
    {
        Unbind();
        _enemy = enemy;
        if (nameText != null) nameText.text = !string.IsNullOrEmpty(enemy.displayName) ? enemy.displayName : enemy.name;
        ResolveSlider();
        enemy.OnHpChanged += OnEnemyHpChanged;
        _lastHp = enemy.CurrentHp;
        Refresh(_lastHp, enemy.maxHp);
    }

    private void Unbind()
    {
        if (_fellow != null) _fellow.OnHpChanged -= OnFellowHpChanged;
        if (_enemy  != null) _enemy.OnHpChanged  -= OnEnemyHpChanged;
        _fellow = null; _enemy = null; _lastHp = -1;
    }

    private void OnDestroy() => Unbind();

    private void OnFellowHpChanged(int hp) => HandleHpChanged(hp, _fellow != null ? _fellow.maxHp : 100);
    private void OnEnemyHpChanged(int hp)  => HandleHpChanged(hp, _enemy  != null ? _enemy.maxHp  : 100);

    private void HandleHpChanged(int hp, int maxHp)
    {
        // 감소면 데미지 팝업
        if (_lastHp >= 0 && hp < _lastHp)
        {
            int damage = _lastHp - hp;
            SpawnDamagePopup(damage);
        }
        _lastHp = hp;
        Refresh(hp, maxHp);
    }

    private void Refresh(int hp, int maxHp)
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHp;
            hpSlider.value    = hp;
        }
        if (hpScoreText != null) hpScoreText.text = $"{hp}/{maxHp}";
        UpdateHpColor(hp, maxHp);
    }

    private void ResolveSlider()
    {
        if (hpSlider == null) hpSlider = GetComponentInChildren<Slider>(true);
        if (_fillImage == null && hpSlider != null && hpSlider.fillRect != null)
            _fillImage = hpSlider.fillRect.GetComponent<Image>();
    }

    private void UpdateHpColor(int hp, int maxHp)
    {
        if (_fillImage == null) return;
        float ratio = maxHp > 0 ? (float)hp / maxHp : 0f;
        _fillImage.color =
              ratio > 0.5f  ? new Color(0.30f, 0.78f, 0.30f)   // 녹
            : ratio > 0.25f ? new Color(0.95f, 0.80f, 0.20f)   // 노랑
            :                 new Color(0.85f, 0.25f, 0.25f);  // 빨강
    }

    private void SpawnDamagePopup(int damage)
    {
        if (damagePopupPrefab == null) return;

        // anchor 우선, 없으면 자식 Canvas, 그것도 없으면 자신의 transform
        Transform parent = damagePopupAnchor;
        if (parent == null)
        {
            var canvas = GetComponentInChildren<Canvas>(true);
            parent = canvas != null ? canvas.transform : transform;
        }

        var popup = Instantiate(damagePopupPrefab, parent);
        popup.transform.localPosition = Vector3.zero;
        popup.Show(damage);
    }
}
