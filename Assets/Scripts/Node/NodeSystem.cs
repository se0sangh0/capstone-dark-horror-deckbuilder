// ============================================================
// Node/NodeSystem.cs
// 노드 맵 UI 시스템 — 자동 생성 + 타입별 분기
// ============================================================
//
// [이 파일이 하는 일]
//   로그라이크 게임의 노드 맵 화면을 관리합니다.
//   Awake 시 MapGenerator 로부터 자동 생성된 MapData 를 받아
//   인스펙터에 사전 배치된 nodeRows 의 버튼들에 RoomType 을 매핑하고,
//   클릭 시 타입별로 다른 화면(전투/화툿불/용병소/...)으로 분기합니다.
//
// [노드 클릭 → 분기 흐름]
//   1. 버튼 클릭 → OnNodeClicked(row, col)
//   2. 현재 층의 버튼이면 → 선택 + currentRowIndex++
//   3. 클릭된 노드의 RoomType 보고 분기:
//      - Combat/Elite/Boss → 전투 패널 (DisplayChanger 호출)
//      - Rest(화툿불)      → TODO 자리 (다음 사이클 E 작업)
//      - Shop(용병소)      → TODO 자리 (다음 사이클 F 작업)
//      - Event(교회)       → TODO 백로그
//   4. UpdateNodeStates() 로 버튼 색상 업데이트
//
// [인스펙터 설정]
//   - mapGenerator   : 같은 GameObject 또는 자식에 있는 MapGenerator 참조 (없으면 자동 검색)
//   - nodeRows       : 각 층의 부모 오브젝트 + 버튼들 (사전 배치 유지)
//   - nodeDisplay    : 노드 맵 화면 (전환 토글용)
//   - actionDisplay  : 전투/행동 화면 (전환 토글용)
//   - passedState / currentState / lockedState : 버튼 시각 상태
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 노드 맵 UI 시스템. 자동 생성된 RoomType 을 버튼에 매핑하고 클릭 시 타입별 분기.
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

        // 각 버튼에 매핑된 RoomType (MapGenerator 결과). buttons 와 인덱스 1:1.
        [HideInInspector] public List<RoomType> roomTypes = new();
    }

    // ----------------------------------------------------------
    // [자동 맵 생성기]
    // ----------------------------------------------------------
    [Header("자동 맵 생성 (Auto Map)")]
    [SerializeField]
    [Tooltip("MapGenerator 컴포넌트 참조. 비어있으면 같은 GameObject + 자식에서 자동 검색.")]
    private MapGenerator mapGenerator;

    private MapData generatedMap;

    // ----------------------------------------------------------
    // [노드 구조 설정]
    // ----------------------------------------------------------
    [Header("노드 구조 (Node Structure)")]
    [SerializeField]
    [Tooltip("노드 맵의 각 층. 순서대로 배치하세요. (10층 기준)")]
    private List<NodeRow> nodeRows;

    /// <summary>현재 진행 중인 층 인덱스 (0-base)</summary>
    private int currentRowIndex = 0;

    // ── 외부 노출 (EnemySpawner 의 층 기반 적 등장 결정용) ──
    public static NodeSystem Current { get; private set; }
    public int CurrentFloor => currentRowIndex;

    /// <summary>현재 노드의 RoomType — EnemySpawner 등이 노드 타입 기반 결정에 사용.</summary>
    public RoomType CurrentRoomType { get; private set; } = RoomType.Combat;

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
    [SerializeField] [Tooltip("지나간 층 — 선택한 버튼 색상 (호환 유지, 현재 미사용)")]
    private NodeVisualState passedState;

    [SerializeField] [Tooltip("현재 층 — 클릭 가능한 버튼 색상 (호환 유지, 현재 미사용)")]
    private NodeVisualState currentState;

    [SerializeField] [Tooltip("잠긴 층 — 클릭 불가 버튼 색상 (호환 유지, 현재 미사용)")]
    private NodeVisualState lockedState;

    // ── RoomType 별 색상 (배틀 흰색 / 용병소 파랑 / 화툿불 빨강 / 보스 보라) ──
    [Header("RoomType 색상")]
    [SerializeField] private Color combatColor = Color.white;
    [SerializeField] private Color eliteColor  = Color.white;
    [SerializeField] private Color shopColor   = new Color(0.24f, 0.55f, 1.00f); // 파랑
    [SerializeField] private Color restColor   = new Color(1.00f, 0.30f, 0.30f); // 빨강
    [SerializeField] private Color bossColor   = new Color(0.62f, 0.30f, 1.00f); // 보라
    [SerializeField] private Color eventColor  = new Color(0.85f, 0.85f, 0.85f); // 회색

    [Header("진행 상태별 알파 (RoomType 색상에 곱해짐)")]
    [SerializeField, Range(0f, 1f)] private float currentAlpha = 1.00f;
    [SerializeField, Range(0f, 1f)] private float passedAlpha  = 0.85f;
    [SerializeField, Range(0f, 1f)] private float lockedAlpha  = 0.35f;

    // ----------------------------------------------------------
    // [화면 전환 참조]
    // ----------------------------------------------------------
    [Header("화면 전환 (Display)")]
    [SerializeField] public GameObject[] nodeDisplay;
    [SerializeField] public GameObject[] actionDisplay;

    [Header("용병소 (Shop 노드 — 선택)")]
    [Tooltip("Shop(용병소) 노드 클릭 시 열릴 메인 패널. 비어있으면 TODO 로그만 출력하고 다음 층 진행.")]
    [SerializeField] private MercenaryOfficePanel mercenaryOfficePanel;

    [Header("화툿불 (Rest 노드 — 선택)")]
    [Tooltip("Rest(화툿불) 노드 클릭 시 열릴 패널. 9층 고정. 비어있으면 회복 없이 다음 층 진행.")]
    [SerializeField] private RestPanel restPanel;

    [Header("교회 (Event 노드)")]
    [Tooltip("Event(교회) 노드 클릭 시 열릴 패널. 비어있으면 안내 로그만 출력하고 다음 층 진행.")]
    [SerializeField] private ChurchPanel churchPanel;

    // ----------------------------------------------------------
    // 초기화
    // ----------------------------------------------------------
    void Awake()
    {
        Current = this;

        // 1) MapGenerator 자동 검색 (필요 시)
        //    Unity Object 는 fake-null 이라 ?? 연쇄가 의도대로 안 동작 → 명시적 단계 체크
        if (mapGenerator == null) mapGenerator = GetComponent<MapGenerator>();
        if (mapGenerator == null) mapGenerator = GetComponentInChildren<MapGenerator>(true);
        if (mapGenerator == null) mapGenerator = GetComponentInParent<MapGenerator>();
        if (mapGenerator == null) mapGenerator = FindFirstObjectByType<MapGenerator>(FindObjectsInactive.Include); // 씬 전체 폴백 (비활성 포함)

        // 2) 자동 맵 생성 + 버튼에 RoomType 매핑
        GenerateAndAssignRoomTypes();

        // 3) 기존 버튼 자동 등록 + 클릭 이벤트 연결
        SetupNodeData();
    }

    void OnDestroy()
    {
        if (Current == this) Current = null;
    }

    void Start()
    {
        UpdateNodeStates();
        AudioManager.Instance?.PlayBgmById(BgmId.NodeMap);
        // 튜토리얼 첫 노드맵 진입 시 인트로 모달 (1회만)
        TutorialManager.Instance?.TryShowDialogue(TutorialManager.DialogueId.NodeMapIntro);
    }

    // ----------------------------------------------------------
    // 자동 맵 생성 → 버튼별 RoomType 매핑
    // ----------------------------------------------------------
    private void GenerateAndAssignRoomTypes()
    {
        if (mapGenerator == null)
        {
            Debug.LogWarning("[NodeSystem] MapGenerator 없음 — 모든 노드 RoomType=Combat 으로 폴백.");
            ApplyFallbackAllCombat();
            return;
        }

        generatedMap = mapGenerator.GenerateMap();
        if (generatedMap == null || generatedMap.nodes.Count == 0)
        {
            Debug.LogWarning("[NodeSystem] MapGenerator 결과 비어있음 — Combat 폴백.");
            ApplyFallbackAllCombat();
            return;
        }

        // layer 별 노드 그룹화
        var byLayer = new Dictionary<int, List<RoomNode>>();
        foreach (var n in generatedMap.nodes)
        {
            if (!byLayer.TryGetValue(n.layer, out var list))
            {
                list = new List<RoomNode>();
                byLayer[n.layer] = list;
            }
            list.Add(n);
        }

        // nodeRows 와 layer 매핑
        for (int r = 0; r < nodeRows.Count; r++)
        {
            // rowParent 아래 Button 들을 미리 모음 (SetupNodeData 도 같은 일을 하므로 중복 안전)
            var row = nodeRows[r];
            row.roomTypes.Clear();

            if (row.rowParent == null) continue;

            var btns = row.rowParent.GetComponentsInChildren<Button>(true);

            byLayer.TryGetValue(r, out var layerNodes);
            int layerCount = layerNodes?.Count ?? 0;

            for (int b = 0; b < btns.Length; b++)
            {
                // layerNodes 가 부족하면 첫 번째 노드 타입으로 폴백 (또는 Combat)
                RoomType type;
                if (layerNodes != null && b < layerCount) type = layerNodes[b].roomType;
                else if (layerNodes != null && layerCount > 0) type = layerNodes[0].roomType;
                else type = RoomType.Combat;

                row.roomTypes.Add(type);
            }

            // 버튼 수 < 자동 생성 노드 수 면 데이터가 잘리고, 반대면 폴백 — 경고
            if (btns.Length != layerCount)
            {
                Debug.LogWarning($"[NodeSystem] 층 {r}: 인스펙터 버튼 {btns.Length}개 vs 자동 생성 노드 {layerCount}개 — 매핑 best-effort 처리.");
            }
        }

        Debug.Log($"[NodeSystem] 자동 맵 매핑 완료 — {generatedMap.nodes.Count} 노드 / {nodeRows.Count} 층");
    }

    /// <summary>MapGenerator 실패 시 모든 버튼 RoomType=Combat 으로 폴백.</summary>
    private void ApplyFallbackAllCombat()
    {
        for (int r = 0; r < nodeRows.Count; r++)
        {
            var row = nodeRows[r];
            row.roomTypes.Clear();
            if (row.rowParent == null) continue;
            var btns = row.rowParent.GetComponentsInChildren<Button>(true);
            for (int b = 0; b < btns.Length; b++) row.roomTypes.Add(RoomType.Combat);
        }
    }

    // ----------------------------------------------------------
    // 버튼 자동 등록
    // ----------------------------------------------------------
    private void SetupNodeData()
    {
        for (int r = 0; r < nodeRows.Count; r++)
        {
            if (nodeRows[r].rowParent == null) continue;

            Button[] childButtons = nodeRows[r].rowParent.GetComponentsInChildren<Button>(true);
            nodeRows[r].buttons.Clear();
            nodeRows[r].buttons.AddRange(childButtons);

            int row = r;
            for (int b = 0; b < nodeRows[r].buttons.Count; b++)
            {
                int col = b;
                nodeRows[r].buttons[b].onClick.AddListener(() => OnNodeClicked(row, col));
            }
        }
    }

    // ----------------------------------------------------------
    // 버튼 시각 상태 업데이트
    // ----------------------------------------------------------
    public void UpdateNodeStates()
    {
        for (int r = 0; r < nodeRows.Count; r++)
        {
            for (int b = 0; b < nodeRows[r].buttons.Count; b++)
            {
                Button btn = nodeRows[r].buttons[b];
                Image  img = btn.GetComponent<Image>();

                RoomType type      = GetRoomTypeAt(r, b);
                Color    baseColor = GetRoomColor(type);

                float alpha;
                bool  interactable;

                if (r < currentRowIndex)
                {
                    // 지나간 층 — 선택된 노드만 유지, 나머지는 어둡게
                    bool isSelected = (b == nodeRows[r].selectedButtonIndex);
                    alpha        = isSelected ? passedAlpha : lockedAlpha;
                    interactable = false;
                }
                else if (r == currentRowIndex)
                {
                    alpha        = currentAlpha;
                    interactable = true;
                }
                else
                {
                    alpha        = lockedAlpha;
                    interactable = false;
                }

                btn.interactable = interactable;
                if (img != null)
                    img.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            }
        }
    }

    private Color GetRoomColor(RoomType type) => type switch
    {
        RoomType.Combat => combatColor,
        RoomType.Elite  => eliteColor,
        RoomType.Shop   => shopColor,
        RoomType.Rest   => restColor,
        RoomType.Boss   => bossColor,
        RoomType.Event  => eventColor,
        _               => Color.white,
    };

    // ----------------------------------------------------------
    // 노드 클릭 처리 — RoomType 별 분기
    // ----------------------------------------------------------
    /// <summary>
    /// 노드 버튼 클릭 시 호출된다.
    /// <summary>
    /// 🎮 치트 — 노드 1단계 전진 (F2). RoomType 분기/패널 진입 없이 currentRowIndex 만 증가.
    /// 예: 3층에서 호출 → 4층으로 점프 (전투/용병소 등 패널은 띄우지 않음).
    /// </summary>
    public void CheatAdvanceFloor()
    {
        if (nodeRows == null || nodeRows.Count == 0)
        {
            Debug.LogWarning("[NodeSystem] 🎮 F2 치트 — nodeRows 미초기화");
            return;
        }
        if (currentRowIndex >= nodeRows.Count)
        {
            Debug.Log($"[NodeSystem] 🎮 F2 치트 — 이미 마지막 층 도달 ({currentRowIndex})");
            return;
        }
        int before = currentRowIndex;
        currentRowIndex++;
        UpdateNodeStates();
        Debug.Log($"[NodeSystem] 🎮 F2 치트 — 층 {before} → {currentRowIndex}");
    }

    /// <summary>
    /// 현재 층의 버튼이면: 선택 처리 → RoomType 별 화면 분기 → 다음 층 진행.
    /// </summary>
    public void OnNodeClicked(int row, int col)
    {
        if (row != currentRowIndex) return;

        // 1) 선택된 버튼 기록
        nodeRows[row].selectedButtonIndex = col;

        // 2) 클릭된 노드의 RoomType 조회
        RoomType type = GetRoomTypeAt(row, col);
        CurrentRoomType = type;
        Debug.Log($"[NodeSystem] 노드 클릭 — 층 {row + 1} (col={col}) | RoomType={type}");

        // 3) 진행 + 분기
        if (currentRowIndex < nodeRows.Count)
        {
            currentRowIndex++;
            DispatchByRoomType(type);
            UpdateNodeStates();
        }
    }

    /// <summary>RoomType 에 따라 적절한 화면을 켜거나 임시 진행 처리한다.</summary>
    private void DispatchByRoomType(RoomType type)
    {
        // 노드 진입 시 게임 이벤트 로그 리셋 — 이전 노드 메시지가 누적되지 않도록.
        GameLogService.Instance?.Clear();

        // 튜토리얼 모달 — 노드 유형별 1회 안내
        var tm = TutorialManager.Instance;
        if (tm != null && tm.IsTutorial)
        {
            switch (type)
            {
                case RoomType.Combat: tm.TryShowDialogue(TutorialManager.DialogueId.CombatIntro); break;
                case RoomType.Elite:  tm.TryShowDialogue(TutorialManager.DialogueId.EliteIntro);  break;
                case RoomType.Boss:   tm.TryShowDialogue(TutorialManager.DialogueId.BossIntro);   break;
                case RoomType.Shop:   tm.TryShowDialogue(TutorialManager.DialogueId.ShopIntro);   break;
                case RoomType.Event:  tm.TryShowDialogue(TutorialManager.DialogueId.ChurchIntro); break;
            }
        }

        switch (type)
        {
            // ── 전투 계열: 기존 흐름 그대로 (DisplayChanger 가 전투 패널 토글) ──
            case RoomType.Combat:
            case RoomType.Elite:
            case RoomType.Boss:
                DisplayChange.Instance.DisplayChanger(nodeDisplay, actionDisplay);
                AudioManager.Instance?.PlayBgmById(BgmId.Battle);
                break;

            // ── 화툿불 (E 작업 완료 — RestPanel 호출) ──
            //   기획 §02_MVP_노드_설계 §화툿불 — HP/스트레스 -15 회복 + 파티 편집
            //   패널 미연결 시(인스펙터 빈 경우) 로그만 남기고 다음 층 진행.
            case RoomType.Rest:
                if (restPanel != null)
                {
                    restPanel.OnExit -= HandleRestExit;
                    restPanel.OnExit += HandleRestExit;
                    restPanel.OpenFromNode();
                    AudioManager.Instance?.PlayBgmById(BgmId.Rest);
                }
                else
                {
                    Debug.Log("[NodeSystem] 화툿불 노드 — RestPanel 미연결, 회복 없이 다음 층으로 진행.");
                }
                break;

            // ── 용병소 (F 작업 완료 — MercenaryOfficePanel 호출) ──
            //   기획 §14_용병소_시스템_명세: 후보 3 / 리롤 / 고용 / 파티 편집 / 합성(백로그)
            //   패널 미연결 시(인스펙터 빈 경우) 로그만 남기고 다음 층 진행.
            case RoomType.Shop:
                if (mercenaryOfficePanel != null)
                {
                    mercenaryOfficePanel.OnExit -= HandleMercenaryExit;
                    mercenaryOfficePanel.OnExit += HandleMercenaryExit;
                    mercenaryOfficePanel.OpenFromNode();
                    AudioManager.Instance?.PlayBgmById(BgmId.Mercenary);
                }
                else
                {
                    Debug.Log("[NodeSystem] 용병소 노드 — MercenaryOfficePanel 미연결, 다음 층으로 진행.");
                }
                break;

            // ── 교회 (Event 노드 매핑) ──
            //   기획 §02_MVP_노드_설계 §교회 — HP 회복 + 스트레스 회복 + 사망 동료 부활
            //   패널 미연결 시(인스펙터 빈 경우) 로그만 남기고 다음 층 진행.
            case RoomType.Event:
                if (churchPanel != null)
                {
                    churchPanel.OnExit -= HandleChurchExit;
                    churchPanel.OnExit += HandleChurchExit;
                    churchPanel.OpenFromNode();
                    AudioManager.Instance?.PlayBgmById(BgmId.Rest);
                }
                else
                {
                    Debug.Log("[NodeSystem] 교회 노드 — ChurchPanel 미연결, 다음 층으로 진행.");
                }
                break;

            default:
                Debug.LogWarning($"[NodeSystem] 미지원 RoomType={type} — 폴백으로 전투 화면 호출.");
                DisplayChange.Instance.DisplayChanger(nodeDisplay, actionDisplay);
                break;
        }
    }

    /// <summary>용병소 패널의 "나가기" 클릭 시 호출 — 노드맵 화면 복귀.</summary>
    private void HandleMercenaryExit()
    {
        // MercenaryService.OnLeaveNode 는 MercenaryOfficePanel 내부에서 이미 호출됨.
        // 여기서는 노드맵 UI 갱신만 — 이미 currentRowIndex++ 가 OnNodeClicked 에서 처리됨.
        if (mercenaryOfficePanel != null)
            mercenaryOfficePanel.OnExit -= HandleMercenaryExit;
        UpdateNodeStates();
        AudioManager.Instance?.PlayBgmById(BgmId.NodeMap);
    }

    /// <summary>화툿불 패널의 "다음 층" 클릭 시 호출 — 노드맵 화면 복귀.</summary>
    private void HandleRestExit()
    {
        if (restPanel != null)
            restPanel.OnExit -= HandleRestExit;
        UpdateNodeStates();
        AudioManager.Instance?.PlayBgmById(BgmId.NodeMap);
    }

    /// <summary>교회 패널의 "다음 층" 클릭 시 호출 — 노드맵 화면 복귀.</summary>
    private void HandleChurchExit()
    {
        if (churchPanel != null)
            churchPanel.OnExit -= HandleChurchExit;
        UpdateNodeStates();
        AudioManager.Instance?.PlayBgmById(BgmId.NodeMap);
    }

    /// <summary>인덱스 안전한 RoomType 조회. 범위 밖이면 Combat 폴백.</summary>
    private RoomType GetRoomTypeAt(int row, int col)
    {
        if (row < 0 || row >= nodeRows.Count) return RoomType.Combat;
        var types = nodeRows[row].roomTypes;
        if (types == null || col < 0 || col >= types.Count) return RoomType.Combat;
        return types[col];
    }
}
