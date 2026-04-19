// ============================================================
// Node/DisplayChange.cs
// 노드 화면 ↔ 전투 화면 전환 싱글톤 매니저
// ============================================================
//
// [이 파일이 하는 일]
//   노드 선택 화면(nodeDisplay) 과
//   전투/행동 화면(actionDisplay) 을 켜고 끄는 역할을 합니다.
//   쉽게 말해 "어떤 화면을 보여줄지" 를 결정합니다.
//
// [어디서 쓰이나요?]
//   - Node/NodeSystem.cs : 노드 클릭 시 DisplayChange.Instance.DisplayChanger() 호출
//
// [변경 기록]
//   이전 코드: public static DisplayChange instance (소문자)
//   수정 후:   public static DisplayChange Instance (대문자, 표준 명명 규칙)
//   NodeSystem.cs 도 함께 수정되었습니다.
//
// [연결된 파일]
//   - Core/Singleton.cs : 싱글톤 기반 클래스
//   - Node/NodeSystem.cs : 노드 버튼 클릭 시 이 클래스를 호출
// ============================================================

using TMPro;
using UnityEngine;

/// <summary>
/// 노드 화면 ↔ 행동 화면 전환 싱글톤 매니저.
/// DisplayChange.Instance 로 전역 접근 가능.
/// </summary>
public class DisplayChange : Singleton<DisplayChange>
{
    // ----------------------------------------------------------
    // [nodeDisplay] — 노드 선택 화면 오브젝트
    // [actionDisplay] — 전투/행동 화면 오브젝트
    // Inspector 에서 각각 연결하세요.
    // ----------------------------------------------------------
    
    [Tooltip("노드 선택 화면 오브젝트를 연결하세요.")]
    [SerializeField] private GameObject[] nodeDisplay;

    
    [Tooltip("전투/행동 화면 오브젝트를 연결하세요.")]
    [SerializeField] private GameObject[] actionDisplay;
    
    [Tooltip("결과 팝업 화면 오브젝트를 연결하세요.")]
    [SerializeField] private GameObject resultPopup;
    [SerializeField] private TMP_Text textDisplay;
    // ----------------------------------------------------------
    // 내부 Inspector 참조로 화면 전환 (외부 매개변수 없이 호출)
    // ----------------------------------------------------------

    /// <summary>
    /// Inspector 에 연결된 nodeDisplay ↔ actionDisplay 를 토글한다.
    /// 둘 다 연결되어 있을 때만 동작한다.
    /// </summary>
    public void ToggleDisplay()
    {
        if (nodeDisplay != null && actionDisplay != null)
        {
            DisplayChanger(nodeDisplay, actionDisplay);
        }
        else
        {
            Debug.LogWarning("[DisplayChange] nodeDisplay 또는 actionDisplay 가 연결되지 않았습니다.");
        }
    }
    //(다른 클래스에서 불러오는 메서드)action <-> node 디스플레이 활성화/비활성화 상태 변경 메서드
    

    // ----------------------------------------------------------
    // 외부에서 직접 오브젝트를 지정하여 전환 (NodeSystem 에서 사용)
    // ----------------------------------------------------------

    /// <summary>
    /// node 와 action 오브젝트의 활성/비활성 상태를 서로 반전시킨다.
    /// NodeSystem.OnNodeClicked() 에서 호출됩니다.
    /// </summary>
    /// <param name="node">노드 화면 오브젝트</param>
    /// <param name="action">행동 화면 오브젝트</param>
    public void DisplayChanger(GameObject[] node, GameObject[] action)
    {
        foreach(GameObject n in node) n.SetActive(!n.activeSelf);
        foreach(GameObject a in action)a.SetActive(!a.activeSelf);
    }

    public void ToggleResultDisplay(bool result)
    {
        textDisplay.text = result ? "Win" : "Lose";
        textDisplay.color = result ? Color.green : Color.red;
        
        resultPopup.SetActive(!resultPopup.activeSelf);
    }
}
