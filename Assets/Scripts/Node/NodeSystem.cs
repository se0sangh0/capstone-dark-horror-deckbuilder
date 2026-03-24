using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeSystem : MonoBehaviour
{
    [System.Serializable]
    public class NodeRow
    {
        public GameObject rowParent; // 인스펙터에서 각 층의 부모 객체만 드래그
        [HideInInspector] public List<Button> buttons = new List<Button>();
        [HideInInspector] public int selectedButtonIndex = -1;
    }

    [Header("Node Structure")]
    [SerializeField] private List<NodeRow> nodeRows; 
    private int currentRowIndex = 0;

    [System.Serializable]
    public struct NodeVisualState
    {
        public Color color;
        public Sprite sprite;
    }

    [Header("Visual Settings")]
    [SerializeField] private NodeVisualState passedState;
    [SerializeField] private NodeVisualState currentState;
    [SerializeField] private NodeVisualState lockedState;
    
    [SerializeField] public GameObject nodeDisplay;
    [SerializeField] public GameObject actionDisplay;

    void Awake()
    {
        // 1. 자동 등록 프로세스
        SetupNodeData();
    }

    void Start()
    {
        UpdateNodeStates();
    }

    private void SetupNodeData()
    {
        for (int r = 0; r < nodeRows.Count; r++)
        {
            if (nodeRows[r].rowParent == null) continue;

            // 부모 객체 아래에 있는 모든 Button을 순서대로 가져옴
            // (주의: 계층 구조 순서대로 가져옵니다)
            Button[] childButtons = nodeRows[r].rowParent.GetComponentsInChildren<Button>();
            nodeRows[r].buttons.AddRange(childButtons);

            int row = r;
            for (int b = 0; b < nodeRows[r].buttons.Count; b++)
            {
                int col = b;
                // 클릭 이벤트 자동 연결
                nodeRows[r].buttons[b].onClick.AddListener(() => OnNodeClicked(row, col));
            }
        }
    }

    public void UpdateNodeStates()
    {
        for (int r = 0; r < nodeRows.Count; r++)
        {
            for (int b = 0; b < nodeRows[r].buttons.Count; b++)
            {
                Button btn = nodeRows[r].buttons[b];
                Image img = btn.GetComponent<Image>();

                if (r < currentRowIndex)
                {
                    // 클릭된 특정 버튼만 연두색
                    if (b == nodeRows[r].selectedButtonIndex)
                        ApplyState(btn, img, passedState, false);
                    else
                        ApplyState(btn, img, lockedState, false);
                }
                else if (r == currentRowIndex)
                {
                    ApplyState(btn, img, currentState, true);
                }
                else
                {
                    ApplyState(btn, img, lockedState, false);
                }
            }
        }
    }

    private void ApplyState(Button btn, Image img, NodeVisualState state, bool isInteractable)
    {
        btn.interactable = isInteractable;
        if (img != null)
        {
            img.color = state.color;
            if (state.sprite != null) img.sprite = state.sprite;
        }
    }
    // 노드 클릭 시 
    public void OnNodeClicked(int row, int col)
    {
        if (row == currentRowIndex)
        {
            nodeRows[row].selectedButtonIndex = col;
            if (currentRowIndex < nodeRows.Count)
            {
                currentRowIndex++;//싱글턴 패턴 사용
                DisplayChange.instance.DisplayChanger(nodeDisplay,actionDisplay);
                UpdateNodeStates();
            }
        }
    }

}