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

    [Header("Line Settings")]
    [SerializeField] private GameObject linePrefab; // LineRenderer 컴포넌트가 붙은 프리팹
    [SerializeField] private Transform lineParent;   // 선들을 모아둘 부모 객체
    
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
        //CreateAllPaths();
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
    
    // // 모든 노드 사이의 경로를 미리 생성하는 함수
    // private void CreateAllPaths()
    // {
    //     Debug.Log($"경로 생성 시작: 층수 {nodeRows.Count}"); // 로그 확인
    //     // 마지막 층은 다음 층이 없으므로 Count - 1 까지만 반복
    //     for (int r = 0; r < nodeRows.Count - 1; r++)
    //     {
    //         Debug.Log($"{r}층 버튼 개수: {nodeRows[r].buttons.Count}"); // 여기서 0이 나오면 안 됨
    //         
    //         NodeRow currentRow = nodeRows[r];
    //         NodeRow nextRow = nodeRows[r + 1];
    //
    //         foreach (Button startBtn in currentRow.buttons)
    //         {
    //             foreach (Button endBtn in nextRow.buttons)
    //             {
    //                 SpawnLine(startBtn.transform.position, endBtn.transform.position);
    //             }
    //         }
    //     }
    // }
    //
    // private void SpawnLine(Vector3 start, Vector3 end)
    // {
    //     GameObject lineObj = Instantiate(linePrefab, lineParent);
    //     LineRenderer lr = lineObj.GetComponent<LineRenderer>();
    //     
    //     lr.positionCount = 2;
    //     lr.SetPosition(0, start);
    //     lr.SetPosition(1, end);
    //     
    //     // 선이 버튼 뒤로 가도록 Z축 조정 (Canvas가 World Space일 때 유용)
    //     // lr.transform.localPosition = new Vector3(0, 0, 10f); 
    // }

}