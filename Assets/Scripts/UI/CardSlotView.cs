// CardSlotView.cs
// LeftPanel 의 동료 카드 한 슬롯 (Card_Base_N) 에 부착하는 뷰 컴포넌트.
//
// ── 표시 항목 ───────────────────────────────────────────────────
//   - 이름 (Name)
//   - 아이콘 (Icon_Image)
//   - HP 게이지 (Slider) + 점수(HP_score)
//   - 실드 게이지 (ShieldBarUI 가 Slider 의 Fill Area 자식으로 자동 생성)
//   - 직업 텍스트 (Job)
//   - 성향 태그 (affinityTagBg / affinityTagText: 라벨·색)
//   - 스킬 2개 (Skill1 / Skill2: 이름 + 코스트)
//
// ── HP 바 구조 ──────────────────────────────────────────────────
//   배틀 카드와 동일한 Slider 방식.
//   Bind 시 Slider 의 max/value 를 초기화하고 FellowData.OnHpChanged 를 구독한다.
//   FellowData.InitHp() 는 호출하지 않는다 — 배틀과 LeftPanel 두 곳이 같은
//   FellowData 를 공유하므로, InitHp 의 HpSlider 단일 필드를 덮어쓰지 않기 위함.
//   대신 자체 OnHpChanged 핸들러를 등록해 LeftPanel Slider 만 갱신한다.
//
// ── 바인딩 ──────────────────────────────────────────────────────
//   LeftPanelView 에서 Bind(FellowData) 로 연결한다.
//   FellowData 의 OnHpChanged / OnShieldChanged / OnStressChanged 이벤트를 구독하여
//   값 변경 시 자동 갱신된다. Unbind() 에서 모두 해제.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardSlotView : MonoBehaviour
{
    [Header("기본")]
    [SerializeField] private Image       iconImage;
    [SerializeField] private TMP_Text    nameText;

    [Header("HP / Shield")]
    [SerializeField] private Slider      hpSlider;          // 배틀과 동일한 Slider 방식
    [SerializeField] private TMP_Text    hpScoreText;       // HP_score

    [Header("태그")]
    [SerializeField] private TMP_Text    jobTagText;        // Job 노드 (TMP_Text)
    [SerializeField] private TMP_Text    affinityTagText;   // affinityTagBg > affinityTagText
    [SerializeField] private Image       affinityTagBg;     // 성향 색상 배경

    [Header("스킬")]
    [SerializeField] private TMP_Text    skill1NameText;
    [SerializeField] private TMP_Text    skill1CostText;
    [SerializeField] private TMP_Text    skill2NameText;
    [SerializeField] private TMP_Text    skill2CostText;

    private FellowData  _fellow;
    private ShieldBarUI _shieldUI;
    private Image       _hpFillImage; // hpSlider.fillRect 의 Image (색상 동적 변경용)

    public void Bind(FellowData fellow)
    {
        Unbind();
        _fellow = fellow;
        if (_fellow == null) { gameObject.SetActive(false); return; }

        gameObject.SetActive(true);

        // 정적 정보
        if (nameText != null)      nameText.text      = !string.IsNullOrEmpty(_fellow.displayName) ? _fellow.displayName : _fellow.id;
        if (iconImage != null)     iconImage.sprite   = _fellow.portrait != null ? _fellow.portrait : _fellow.fellowSprite;
        if (jobTagText != null)    jobTagText.text    = !string.IsNullOrEmpty(_fellow.jobClass) ? _fellow.jobClass : _fellow.role.ToString();
        if (affinityTagText != null) affinityTagText.text = _fellow.AffinityLabel;
        if (affinityTagBg != null) affinityTagBg.color   = _fellow.AffinityColor;

        // 스킬
        var skills = _fellow.GetSkills();
        SetSkill(skill1NameText, skill1CostText, skills.Count > 0 ? skills[0] : null);
        SetSkill(skill2NameText, skill2CostText, skills.Count > 1 ? skills[1] : null);

        // HP Slider 초기화
        if (hpSlider != null)
        {
            int maxHp = _fellow.maxHp > 0 ? _fellow.maxHp : 100;
            hpSlider.maxValue = maxHp;
            hpSlider.value    = _fellow.CurrentHp;

            // Fill Image 캐시 (색상 동적 변경용)
            if (_hpFillImage == null && hpSlider.fillRect != null)
                _hpFillImage = hpSlider.fillRect.GetComponent<Image>();

            // Shield UI 자동 부착 (최초 1회만)
            if (_shieldUI == null)
            {
                _shieldUI = hpSlider.GetComponent<ShieldBarUI>();
                if (_shieldUI == null) _shieldUI = hpSlider.gameObject.AddComponent<ShieldBarUI>();
                _shieldUI.Init(_fellow, hpSlider);
            }
        }

        // 이벤트 구독
        _fellow.OnHpChanged += OnHpChanged;

        RefreshHp();
    }

    public void Unbind()
    {
        if (_fellow == null) return;
        _fellow.OnHpChanged -= OnHpChanged;
        _fellow = null;
    }

    private void OnDisable() => Unbind();

    private void OnHpChanged(int hp)
    {
        if (hpSlider    != null) hpSlider.value   = hp;
        if (hpScoreText != null) hpScoreText.text = hp.ToString();
        UpdateHpColor(hp);
    }

    private void RefreshHp()
    {
        if (_fellow == null) return;
        if (hpSlider    != null) hpSlider.value   = _fellow.CurrentHp;
        if (hpScoreText != null) hpScoreText.text = _fellow.CurrentHp.ToString();
        UpdateHpColor(_fellow.CurrentHp);
    }

    // 포켓몬식 HP 색상: >50% 초록 / 25~50% 노랑 / ≤25% 빨강
    private void UpdateHpColor(int hp)
    {
        if (_hpFillImage == null || _fellow == null) return;
        int maxHp = _fellow.maxHp > 0 ? _fellow.maxHp : 100;
        float ratio = (float)hp / maxHp;

        _hpFillImage.color =
              ratio > 0.5f  ? new Color(0.30f, 0.78f, 0.30f)   // 초록
            : ratio > 0.25f ? new Color(0.95f, 0.80f, 0.20f)   // 노랑
            :                 new Color(0.85f, 0.25f, 0.25f);  // 빨강
    }

    private static void SetSkill(TMP_Text nameLabel, TMP_Text costLabel, SkillData skill)
    {
        if (skill == null)
        {
            if (nameLabel != null) nameLabel.text = "-";
            if (costLabel != null) costLabel.text = "";
            return;
        }
        if (nameLabel != null) nameLabel.text = skill.displayName;
        if (costLabel != null) costLabel.text = skill.costAmount.ToString();
    }
}
