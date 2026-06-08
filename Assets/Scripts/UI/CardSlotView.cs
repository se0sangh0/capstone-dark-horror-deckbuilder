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

    [Header("스트레스 (HP 아래)")]
    [Tooltip("스트레스 게이지 (0~100). HP 바 아래에 배치. 기획자 피드백 #3.")]
    [SerializeField] private Slider      stressSlider;
    [Tooltip("스트레스 점수 텍스트 (현재 스트레스 0~100).")]
    [SerializeField] private TMP_Text    stressScoreText;

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
    private Image       _hpFillImage;     // hpSlider.fillRect 의 Image (색상 동적 변경용)
    private Image       _stressFillImage; // stressSlider.fillRect 의 Image (상태별 색상)

    public void Bind(FellowData fellow)
    {
        Unbind();
        _fellow = fellow;
        if (_fellow == null) { gameObject.SetActive(false); return; }

        gameObject.SetActive(true);

        // 정적 정보
        if (nameText != null)      { nameText.text = !string.IsNullOrEmpty(_fellow.displayName) ? _fellow.displayName : _fellow.id; ApplyAutoFit(nameText, 20f, 12f); }
        if (iconImage != null)     iconImage.sprite   = _fellow.portrait != null ? _fellow.portrait : _fellow.fellowSprite;
        // Job 자리에 활성 메타 패시브 표시 (기획 §16). 미배정/미해금이면 "패시브 잠금".
        // 긴 패시브명이 칸을 넘치지 않도록 폰트 자동 축소 (max=기본 20, min=11).
        if (jobTagText != null)
        {
            string pn = MetaPassiveManager.NameOf(_fellow.activePassiveId);
            jobTagText.text = string.IsNullOrEmpty(pn) ? "패시브 잠금" : pn;
            jobTagText.enableAutoSizing = true;
            jobTagText.fontSizeMax = 20f;
            jobTagText.fontSizeMin = 11f;
            jobTagText.enableWordWrapping = false;
            jobTagText.overflowMode = TMPro.TextOverflowModes.Ellipsis;
        }
        if (affinityTagText != null) { affinityTagText.text = _fellow.AffinityLabel; ApplyAutoFit(affinityTagText, 18f, 10f); }
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

        // 스트레스 Slider 초기화 (HP 아래, #3)
        if (stressSlider != null)
        {
            stressSlider.minValue = 0;
            stressSlider.maxValue = 100;
            stressSlider.value    = _fellow.currentStress;
            if (_stressFillImage == null && stressSlider.fillRect != null)
                _stressFillImage = stressSlider.fillRect.GetComponent<Image>();
            UpdateStress(_fellow.currentStress);
        }

        // 이벤트 구독
        _fellow.OnHpChanged     += OnHpChanged;
        _fellow.OnShieldChanged += OnShieldChanged;
        _fellow.OnStressChanged += OnStressChanged;

        RefreshHp();
    }

    public void Unbind()
    {
        if (_fellow == null) return;
        _fellow.OnHpChanged     -= OnHpChanged;
        _fellow.OnShieldChanged -= OnShieldChanged;
        _fellow.OnStressChanged -= OnStressChanged;
        _fellow = null;
    }

    private void OnDisable() => Unbind();

    private void OnHpChanged(int hp)
    {
        if (hpSlider    != null) hpSlider.value   = hp;
        if (hpScoreText != null) hpScoreText.text = FormatHpScore(hp, _fellow != null ? _fellow.shield : 0);
        UpdateHpColor(hp);
    }

    private void OnShieldChanged()
    {
        if (_fellow == null || hpScoreText == null) return;
        hpScoreText.text = FormatHpScore(_fellow.CurrentHp, _fellow.shield);
    }

    // 스트레스 변경 → 게이지 갱신 (#3)
    private void OnStressChanged(int stress) => UpdateStress(stress);

    // 스트레스 바 고정 노란색 — HP 바(녹/노/빨 단계색)와 시각 구분 (사용자 요청).
    private static readonly Color StressBarColor = new Color(0.95f, 0.80f, 0.15f);

    private void UpdateStress(int stress)
    {
        if (stressSlider != null) stressSlider.value = stress;
        if (_stressFillImage != null) _stressFillImage.color = StressBarColor;
        if (stressScoreText != null) stressScoreText.text = $"{stress}/100";
    }

    private void RefreshHp()
    {
        if (_fellow == null) return;
        if (hpSlider    != null) hpSlider.value   = _fellow.CurrentHp;
        if (hpScoreText != null) hpScoreText.text = FormatHpScore(_fellow.CurrentHp, _fellow.shield);
        UpdateHpColor(_fellow.CurrentHp);
    }

    /// <summary>실드 있으면 "HP(+S)", 없으면 "HP" 만.</summary>
    private static string FormatHpScore(int hp, int shield)
        => shield > 0 ? $"{hp}(+{shield})" : hp.ToString();

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

    /// <summary>칸보다 큰 폰트로 텍스트 밑이 잘리지 않도록 오토사이즈(축소) 적용 (#12).</summary>
    private static void ApplyAutoFit(TMP_Text label, float max, float min)
    {
        if (label == null) return;
        label.enableAutoSizing  = true;
        label.fontSizeMax       = max;
        label.fontSizeMin       = min;
        label.enableWordWrapping = false;
        label.overflowMode      = TMPro.TextOverflowModes.Ellipsis;
    }

    private static void SetSkill(TMP_Text nameLabel, TMP_Text costLabel, SkillData skill)
    {
        // 호버 영역 = 스킬 항목 컨테이너(Skill1/Skill2). 이름 텍스트 한 점이 아니라 칸 전체에서 툴팁 (#2).
        GameObject hoverArea = (nameLabel != null && nameLabel.transform.parent != null)
            ? nameLabel.transform.parent.gameObject
            : (nameLabel != null ? nameLabel.gameObject : null);

        // 스킬 이름 폰트가 칸보다 커서 밑이 잘리던 문제 — 칸에 맞게 오토사이즈 (#12).
        if (nameLabel != null) ApplyAutoFit(nameLabel, 16f, 10f);

        if (skill == null)
        {
            if (nameLabel != null) nameLabel.text = "-";
            if (costLabel != null) costLabel.text = "";
            if (hoverArea != null) SkillTooltipTrigger.Ensure(hoverArea).SetSkills(); // 빈 → 호버 표시 안 함
            return;
        }
        if (nameLabel != null) nameLabel.text = skill.displayName;
        if (costLabel != null) costLabel.text = skill.costAmount.ToString();
        if (hoverArea != null) SkillTooltipTrigger.Ensure(hoverArea).SetSkills(skill); // 칸 전체 호버 툴팁
    }
}
