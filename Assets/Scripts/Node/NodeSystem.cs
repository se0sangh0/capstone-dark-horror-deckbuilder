// ============================================================
// Node/NodeSystem.cs
// 노드 맵 UI 시스템
// ============================================================
//
// [이 파일이 하는 일]
//   로그라이크 게임의 노드 맵 화면을 관리합니다.
//   여러 층(Row)에 버튼들이 배치되고, 플레이어가 버튼을 클릭하면
//   해당 노드를 선택하고 다음 층으로 진행합니다.
//
// [노드 클릭 흐름]
//   1. 버튼 클릭 → OnNodeClicked(row, col) 호출
//   2. 현재 층의 버튼이면 → 선택 처리 + 현재 층 +1
//   3. DisplayChange.Instance.DisplayChanger() 로 노드 화면 → 행동 화면 전환
//   4. UpdateNodeStates() 로 버튼 색상 업데이트
//
// [상태 표시]
//   - 지나간 층: lockedState 색상 (클릭 불가)
//   - 선택한 버튼: passedState 색상
//   - 현재 층: currentState 색상 (클릭 가능)
//   - 미래 층: lockedState 색상 (클릭 불가)
//
// [어디서 쓰이나요?]
//   - 노드 맵 씬에서 NodeSystem 오브젝트에 이 컴포넌트를 붙임
//
// [인스펙터 설정]
//   - nodeRows : 각 층의 부모 오브젝트 목록
//   - nodeDisplay / actionDisplay : 전환할 화면 오브젝트
//   - passedState / currentState / lockedState : 버튼 시각 상태
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 노드 맵 UI 시스템. 층별 노드 버튼 클릭 및 시각 상태 관리.
/// </summary>
public class NodeSystem : MonoBehaviour
{
    // ----------------------------------------------------------
    // [NodeRow] — 한 층(Row)의 정보를 담는 내부 클래스
    // ----------------------------------------------------------
    [System.Serializable]
    public class NodeRow
    {
        [Tooltip("이 층의 부모 오브젝트를 드래그하여 연결하세요.")]
        public GameObject rowParent;

        [HideInInspector] public List<Button> buttons = new();
        [HideInInspector] public int selectedButtonIndex = -1;
    }

    // ----------------------------------------------------------
    // [노드 구조 설정]
    // ----------------------------------------------------------
    [Header("노드 구조 (Node Structure)")]
    [SerializeField]
    [Tooltip("노드 맵의 각 층. 순서대로 배치하세요.")]
    private List<NodeRow> nodeRows;

    /// <summary>현재 진행 중인 층 인덱스</summary>
    private int currentRowIndex = 0;

    // ----------------------------------------------------------
    // [선 렌더링 — 현재 주석 처리됨 (추후 활성화)]
    // ----------------------------------------------------------
    [Header("선 설정 (Line Settings)")]
    [SerializeField]
    [Tooltip("LineRenderer 컴포넌트가 붙은 선 프리팹")]
    private GameObject linePrefab;

    [SerializeField]
    [Tooltip("생성된 선들을 모아둘 부모 오브젝트")]
    private Transform lineParent;

    // ----------------------------------------------------------
    // [버튼 시각 상태]
    // ----------------------------------------------------------
    [System.Serializable]
    public struct NodeVisualState
    {
        [Tooltip("버튼 색상")]
        public Color color;

        [Tooltip("버튼 스프라이트 (없으면 색상만 적용)")]
        public Sprite sprite;
    }

    [Header("시각 상태 (Visual Settings)")]
    [SerializeField] [Tooltip("지나간 층 — 선택한 버튼 색상")]
    private NodeVisualState passedState;

    [SerializeField] [Tooltip("현재 층 — 클릭 가능한 버튼 색상")]
    private NodeVisualState currentState;

    [SerializeField] [Tooltip("잠긴 층 — 클릭 불가 버튼 색상")]
    private NodeVisualState lockedState;

    // ----------------------------------------------------------
    // [화면 전환 참조]
    // ----------------------------------------------------------
    [Header("화면 전환 (Display)")]
    [SerializeField] public GameObject nodeDisplay;
    [SerializeField] public GameObject actionDisplay;

    // ----------------------------------------------------------
    // 초기화
    // ----------------------------------------------------------
    void Awake()
    {
        // 모든 층의 버튼을 자동 등록하고 클릭 이벤트 연결
        SetupNodeData();
    }

    void Start()
    {
        // 초기 버튼 시각 상태 업데이트
        UpdateNodeStates();
    }

    // ----------------------------------------------------------
    // 버튼 자동 등록
    // ----------------------------------------------------------

    /// <summary>
    /// 각 층 rowParent 하위의 Button 을 자동으로 찾아 등록하고
    /// 클릭 이벤트를 연결한다.
    /// </summary>
    private void SetupNodeData()
    {
        for (int r = 0; r < nodeRows.Count; r++)
        {
            if (nodeRows[r].rowParent == null) continue;

            // 부모 하위의 모든 Button 을 계층 순서대로 가져옴
            Button[] childButtons = nodeRows[r].rowParent.GetComponentsInChildren<Button>();
            nodeRows[r].buttons.AddRange(childButtons);

            int row = r;
            for (int b = 0; b < nodeRows[r].buttons.Count; b++)
            {
                int col = b;
                // 클릭 이벤트 자동 연결 (row, col 을 캡처)
                nodeRows[r].buttons[b].onClick.AddListener(() => OnNodeClicked(row, col));
            }
        }
    }

    // ----------------------------------------------------------
    // 버튼 시각 상태 업데이트
    // ----------------------------------------------------------

    /// <summary>
    /// 현재 진행 위치에 따라 모든 버튼의 색상과 상호작용 가능 여부를 업데이트한다.
    /// </summary>
    public void UpdateNodeStates()
    {
        for (int r = 0; r < nodeRows.Count; r++)
        {
            for (int b = 0; b < nodeRows[r].buttons.Count; b++)
            {
                Button btn = nodeRows[r].buttons[b];
                Image  img = btn.GetComponent<Image>();

                if (r < currentRowIndex)
                {
                    // 지나간 층: 선택한 버튼만 passedState, 나머지는 lockedState
                    if (b == nodeRows[r].selectedButtonIndex)
                        ApplyState(btn, img, passedState, false);
                    else
                        ApplyState(btn, img, lockedState, false);
                }
                else if (r == currentRowIndex)
                {
                    // 현재 층: 클릭 가능
                    ApplyState(btn, img, currentState, true);
                }
                else
                {
                    // 미래 층: 잠김
                    ApplyState(btn, img, lockedState, false);
                }
            }
        }
    }

    /// <summary>버튼에 색상, 스프라이트, 상호작용 여부를 적용한다.</summary>
    private void ApplyState(Button btn, Image img, NodeVisualState state, bool isInteractable)
    {
        btn.interactable = isInteractable;
        if (img != null)
        {
            img.color = state.color;
            if (state.sprite != null) img.sprite = state.sprite;
        }
    }

    // ----------------------------------------------------------
    // 노드 클릭 처리
    // ----------------------------------------------------------

    /// <summary>
    /// 노드 버튼 클릭 시 호출된다.
    /// 현재 층의 버튼이면: 선택 처리 → 다음 층으로 진행 → 화면 전환.
    /// </summary>
    /// <param name="row">클릭된 버튼의 층 인덱스</param>
    /// <param name="col">클릭된 버튼의 열 인덱스</param>
    public void OnNodeClicked(int row, int col)
    {
        if (row == currentRowIndex)
        {
            // 선택된 버튼 기록
            nodeRows[row].selectedButtonIndex = col;

            if (currentRowIndex < nodeRows.Count)
            {
                // 다음 층으로 진행
                currentRowIndex++;

                // 노드 화면 → 행동 화면 전환
                // 수정: DisplayChange.instance(소문자) → DisplayChange.Instance(대문자)
                DisplayChange.Instance.DisplayChanger(nodeDisplay, actionDisplay);

                // 버튼 시각 상태 업데이트
                UpdateNodeStates();
            }
        }
    }
}
