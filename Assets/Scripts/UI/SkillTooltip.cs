// ============================================================
// UI/SkillTooltip.cs
// 공용 스킬 호버 툴팁 (기획자 피드백 #2·#7)
// ============================================================
//
// [구성]
//   SkillTooltipController : 씬에 1개 존재하는 싱글톤. 단일 툴팁 패널을 보유하고
//                            Show/Hide 로 표시한다. 본문은 리치텍스트 1개에 그린다.
//   SkillTooltipTrigger    : 스킬 라벨 GameObject 에 부착(런타임)되어 마우스 호버 시
//                            보유 스킬 정보를 컨트롤러에 넘긴다.
//
// [사용]
//   - CardSlotView (좌측 파티 카드, #2) : 스킬1/스킬2 라벨에 Attach(label, skill).
//   - FellowCardView (용병소/파티편집, #7) : skillsLabel 에 Attach(label, skills[]).
//
// [요구]
//   - 스킬 라벨 TMP 의 raycastTarget = true (기본값) + 상위 Canvas 에 GraphicRaycaster.
//   - 씬에 SkillTooltipController 가 부착된 패널 1개.
// ============================================================

using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class SkillTooltipController : MonoBehaviour
{
    public static SkillTooltipController Instance { get; private set; }

    [Header("툴팁 패널 (이 컴포넌트가 붙은 오브젝트 또는 자식)")]
    [SerializeField] private RectTransform panel;     // 표시/숨김 대상
    [SerializeField] private TMP_Text      bodyText;  // 명/코스트/효과/설명을 모은 본문

    [Tooltip("커서로부터의 오프셋(px). 패널이 커서를 가리지 않도록.")]
    [SerializeField] private Vector2 cursorOffset = new Vector2(18f, -18f);

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>스킬 목록을 받아 본문을 채우고 커서 근처에 표시.</summary>
    public void Show(IList<SkillData> skills, Vector2 screenPos)
    {
        if (panel == null || skills == null || skills.Count == 0) return;

        if (bodyText != null) bodyText.text = BuildBody(skills);

        panel.gameObject.SetActive(true);
        panel.position = screenPos + cursorOffset;
        ClampToScreen();
    }

    public void Hide()
    {
        if (panel != null) panel.gameObject.SetActive(false);
    }

    private static string BuildBody(IList<SkillData> skills)
    {
        var sb = new StringBuilder(256);
        for (int i = 0; i < skills.Count; i++)
        {
            var s = skills[i];
            if (s == null) continue;
            if (i > 0) sb.Append('\n');
            string name = !string.IsNullOrEmpty(s.displayName) ? s.displayName : s.id;
            sb.Append("<b>").Append(name).Append("</b>  <color=#FFC107>코스트 ").Append(s.costAmount).Append("</color>");
            sb.Append('\n').Append("<color=#9FB4C8>").Append(EffectLabel(s)).Append("</color>");
            if (!string.IsNullOrEmpty(s.description))
                sb.Append('\n').Append(s.description);
        }
        return sb.ToString();
    }

    /// <summary>effectType + 주요 수치를 한글 한 줄로.</summary>
    private static string EffectLabel(SkillData s)
    {
        string target = TargetLabel(s.targeting);
        switch (s.effectType)
        {
            case "Heal":              return $"{target} 회복 {s.power}";
            case "Shield":            return $"{target} 실드 {s.power}";
            case "Buff":              return $"{target} 강화";
            case "Debuff":            return $"{target} 약화";
            case "MixedDamageShield": return $"{target} 피해 {s.power} + 전체 실드 {s.shieldPower}";
            case "MixedDamageTaunt":  return $"{target} 피해 {s.power} + 도발 {s.tauntTurns}턴";
            default:                  return $"{target} 피해 {s.power}";
        }
    }

    private static string TargetLabel(string targeting) => targeting switch
    {
        "Self"        => "자신",
        "SingleEnemy" => "단일 적",
        "AllEnemies"  => "광역 적",
        "SingleAlly"  => "단일 아군",
        "AllAllies"   => "전체 아군",
        _             => "",
    };

    // 화면 밖으로 넘치면 안쪽으로 당김.
    private void ClampToScreen()
    {
        if (panel == null) return;
        var corners = new Vector3[4];
        panel.GetWorldCorners(corners);
        float w = corners[2].x - corners[0].x;
        float h = corners[2].y - corners[0].y;
        var p = panel.position;
        if (corners[2].x > Screen.width)  p.x -= (corners[2].x - Screen.width);
        if (corners[0].x < 0)             p.x -= corners[0].x;
        if (corners[2].y > Screen.height) p.y -= (corners[2].y - Screen.height);
        if (corners[0].y < 0)             p.y -= corners[0].y;
        panel.position = p;
    }
}

/// <summary>
/// 스킬 라벨에 런타임 부착되는 호버 트리거. 호버 시 보유 스킬 정보를 툴팁으로 표시.
/// CardSlotView / FellowCardView 의 Ensure(...) 헬퍼로 부착·갱신한다.
/// </summary>
public class SkillTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private readonly List<SkillData> _skills = new();

    /// <summary>표시할 스킬 목록을 교체.</summary>
    public void SetSkills(params SkillData[] skills)
    {
        _skills.Clear();
        if (skills != null)
            foreach (var s in skills)
                if (s != null) _skills.Add(s);
    }

    public void SetSkills(IEnumerable<SkillData> skills)
    {
        _skills.Clear();
        if (skills != null)
            foreach (var s in skills)
                if (s != null) _skills.Add(s);
    }

    public bool HasAny => _skills.Count > 0;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!HasAny) return;
        SkillTooltipController.Instance?.Show(_skills, eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SkillTooltipController.Instance?.Hide();
    }

    /// <summary>대상 GameObject 에 트리거가 없으면 추가하고 반환. 영역 호버를 위해 raycast 대상도 보장.</summary>
    public static SkillTooltipTrigger Ensure(GameObject target)
    {
        if (target == null) return null;
        var t = target.GetComponent<SkillTooltipTrigger>();
        if (t == null) t = target.AddComponent<SkillTooltipTrigger>();
        // 컨테이너(그래픽 없음)에 붙는 경우, 영역 전체가 호버되도록 투명 raycast Image 추가 (#2).
        if (target.GetComponent<UnityEngine.UI.Graphic>() == null)
        {
            var img = target.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0f, 0f, 0f, 0f); // 완전 투명
            img.raycastTarget = true;
        }
        return t;
    }
}
