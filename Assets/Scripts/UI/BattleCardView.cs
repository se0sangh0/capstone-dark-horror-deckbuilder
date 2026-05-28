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

    private Image              _fillImage;
    private FellowData         _fellow;
    private EnemyData          _enemy;
    private int                _lastHp     = -1;
    private int                _lastShield = -1;
    private BattleCardSprites  _sprites; // 같은 GameObject 의 모션 컴포넌트 (있으면 사용)

    /// <summary>바인딩된 동료 데이터 (적 카드면 null).</summary>
    public FellowData Fellow => _fellow;
    /// <summary>바인딩된 적 데이터 (아군 카드면 null).</summary>
    public EnemyData  Enemy  => _enemy;

    // ── 외부에서 호출 ─────────────────────────────────────────────
    public void BindFellow(FellowData fellow)
    {
        Unbind();
        _fellow = fellow;
        if (nameText != null) nameText.text = !string.IsNullOrEmpty(fellow.displayName) ? fellow.displayName : fellow.id;
        ResolveSlider();
        fellow.OnHpChanged     += OnFellowHpChanged;
        fellow.OnShieldChanged += OnShieldChanged;
        fellow.OnDamaged       += OnDamaged;
        fellow.OnSkillCast     += HandleSkillCast;
        _lastHp     = fellow.CurrentHp;
        _lastShield = fellow.shield;
        Refresh(_lastHp, fellow.maxHp);
    }

    public void BindEnemy(EnemyData enemy)
    {
        Unbind();
        _enemy = enemy;
        if (nameText != null) nameText.text = !string.IsNullOrEmpty(enemy.displayName) ? enemy.displayName : enemy.name;
        ResolveSlider();
        enemy.OnHpChanged += OnEnemyHpChanged;
        enemy.OnDamaged   += OnDamaged;
        enemy.OnSkillCast += HandleSkillCast;
        _lastHp = enemy.CurrentHp;
        Refresh(_lastHp, enemy.maxHp);
    }

    private void Unbind()
    {
        if (_fellow != null)
        {
            _fellow.OnHpChanged     -= OnFellowHpChanged;
            _fellow.OnShieldChanged -= OnShieldChanged;
            _fellow.OnDamaged       -= OnDamaged;
            _fellow.OnSkillCast     -= HandleSkillCast;
        }
        if (_enemy  != null)
        {
            _enemy.OnHpChanged -= OnEnemyHpChanged;
            _enemy.OnDamaged   -= OnDamaged;
            _enemy.OnSkillCast -= HandleSkillCast;
        }
        _fellow = null; _enemy = null; _lastHp = -1;
    }

    /// <summary>OnDamaged — 쉴드 흡수(노랑)/HP 감소(빨강) popup 분리 표시 + Hit 모션 1회.</summary>
    private void OnDamaged(int absorbed, int hpLoss)
    {
        if (absorbed <= 0 && hpLoss <= 0) return;

        // 둘 다 발생(부분 흡수)이면 노란 popup 을 살짝 위에 띄워 겹침 회피
        bool both = absorbed > 0 && hpLoss > 0;
        if (absorbed > 0) SpawnPopup(absorbed, PopupKind.ShieldAbsorb, extraYOffset: both ? 0.5f : 0f);
        if (hpLoss   > 0) SpawnPopup(hpLoss,   PopupKind.Damage);

        EnsureSprites()?.PlayHit();
    }

    private void OnShieldChanged()
    {
        if (_fellow != null)
        {
            int curShield = _fellow.shield;
            // 실드 증가량 팝업 (감소는 데미지 흡수 — HurtAlly 측 데미지 팝업과 중복 방지 위해 미표시)
            if (_lastShield >= 0 && curShield > _lastShield)
                SpawnPopup(curShield - _lastShield, PopupKind.Shield);
            _lastShield = curShield;
        }
        Refresh(_lastHp, _fellow != null ? _fellow.maxHp : 100);
    }

    private void OnDestroy() => Unbind();

    private void OnFellowHpChanged(int hp) => HandleHpChanged(hp, _fellow != null ? _fellow.maxHp : 100);
    private void OnEnemyHpChanged(int hp)  => HandleHpChanged(hp, _enemy  != null ? _enemy.maxHp  : 100);

    private void HandleHpChanged(int hp, int maxHp)
    {
        // 데미지(감소)는 OnDamaged 이벤트에서 일괄 처리 — 쉴드 흡수 케이스 누락 방지 + 중복 popup 방지.
        // 여기서는 회복(증가) 케이스만 처리.
        if (_lastHp >= 0 && hp > _lastHp)
            SpawnPopup(hp - _lastHp, PopupKind.Heal);
        _lastHp = hp;
        Refresh(hp, maxHp);
    }

    private BattleCardSprites EnsureSprites()
    {
        if (_sprites == null) _sprites = GetComponent<BattleCardSprites>();
        return _sprites;
    }

    /// <summary>
    /// OnSkillCast 이벤트 핸들러. effectType + actor 의 jobClass 로 카테고리 결정 후
    /// BattleCardSprites.PlayAttack(cat) 호출. (적군은 jobClass 없음 — null 전달)
    /// </summary>
    private void HandleSkillCast(string effectType)
    {
        string jobClass = _fellow != null ? _fellow.jobClass : null;
        var cat = MotionCategoryResolver.Resolve(jobClass, effectType);
        EnsureSprites()?.PlayAttack(cat);
    }

    private void Refresh(int hp, int maxHp)
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHp;
            hpSlider.value    = hp;
        }
        if (hpScoreText != null)
        {
            // 아군만 쉴드 보유 — 쉴드 > 0 이면 "(+N)" 추가, 0 이면 표기 생략
            int shield = _fellow != null ? _fellow.shield : 0;
            hpScoreText.text = shield > 0 ? $"{hp}/{maxHp} (+{shield})" : $"{hp}/{maxHp}";
        }
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
        // 안전망 — 첫 호출 시 Slider.fillRect 가 미할당이었거나 캐싱 실패한 경우 재시도.
        if (_fillImage == null) ResolveSlider();
        if (_fillImage == null)
        {
            Debug.LogWarning($"[BattleCardView] '{name}' Slider.fillRect 미할당 — HP 색 변화 동작 불가. Slider 인스펙터 확인.", this);
            return;
        }
        float ratio = maxHp > 0 ? (float)hp / maxHp : 0f;
        _fillImage.color =
              ratio > 0.5f  ? new Color(0.30f, 0.78f, 0.30f)   // 녹
            : ratio > 0.25f ? new Color(0.95f, 0.80f, 0.20f)   // 노랑
            :                 new Color(0.85f, 0.25f, 0.25f);  // 빨강
    }

    // 카드 본체 시작 오프셋 (월드/캔버스 단위). 상태 영역(이름·HP) 보다 아래에서 출발.
    private const float DamagePopupXOffset    = 0.8f;  // 좌측 쏠림 보정 — 카드 중앙에 맞춤
    private const float DamagePopupYOffset    = 0.3f;
    // 위로 떠오를 거리 (월드 단위). 상태 영역까지만 살짝 올라가도록 작게.
    private const float DamagePopupFloatHeight = 0.8f;

    // ── AOE cascade — 같은 시점에 다수 카드에서 팝업이 동시 폭발하는 가독성 문제 완화 ──
    //   윈도우 안에 연속 스폰되면 인덱스를 누적해 startDelay 를 점차 늘려 cascade 시각화.
    //   윈도우를 벗어나면 인덱스 0 으로 리셋.
    private static float _lastPopupSpawnTime  = -10f;
    private static int   _popupStaggerIndex   = 0;
    private const  float PopupBurstWindow     = 0.15f; // 이 시간 안에 들어오면 같은 burst
    private const  float PopupStaggerStep     = 0.05f; // 인덱스당 추가 지연
    private const  float PopupStaggerMaxDelay = 0.30f; // 최대 지연 캡 (애니 0.9s 안에 끝나야 함)

    private void SpawnPopup(int amount, PopupKind kind, float extraYOffset = 0f)
    {
        if (damagePopupPrefab == null)
        {
            Debug.LogWarning($"[BattleCardView] '{name}' damagePopupPrefab 미연결 — 인스펙터 확인", this);
            return;
        }

        // anchor 우선, 없으면 자식 Canvas, 그것도 없으면 자신의 transform
        Transform parent = damagePopupAnchor;
        if (parent == null)
        {
            var canvas = GetComponentInChildren<Canvas>(true);
            parent = canvas != null ? canvas.transform : transform;
        }

        var popup = Instantiate(damagePopupPrefab, parent);

        // WorldSpace Canvas 안의 형제 텍스트가 0.01 스케일을 쓰면 popup 도 맞춤
        // (안 맞추면 World 1 = Unity 1 스케일로 spawn 돼서 카드 100배 크기로 화면 밖)
        var siblingText = parent.GetComponentInChildren<TMP_Text>(true);
        if (siblingText != null && siblingText.transform != popup.transform)
            popup.transform.localScale = siblingText.transform.localScale;

        // 카드 머리 위 시작점 — localPosition 으로 부모(캔버스) 로컬 좌표계 사용.
        // anchoredPosition 은 WorldSpace 캔버스 스케일 1에서 단위가 어긋나서 사용 안 함.
        popup.transform.localPosition = new Vector3(DamagePopupXOffset, DamagePopupYOffset + extraYOffset, 0f);

        // AOE cascade — burst 윈도우 안이면 인덱스 누적, 아니면 리셋
        float now = Time.unscaledTime;
        if (now - _lastPopupSpawnTime > PopupBurstWindow) _popupStaggerIndex = 0;
        else                                              _popupStaggerIndex++;
        _lastPopupSpawnTime = now;
        float delay = Mathf.Min(_popupStaggerIndex * PopupStaggerStep, PopupStaggerMaxDelay);

        popup.Show(amount, kind, DamagePopupFloatHeight, delay);
    }
}
