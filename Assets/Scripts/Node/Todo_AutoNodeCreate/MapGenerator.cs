// ============================================================
// MapGenerator.cs — 자동 맵 생성기
// ============================================================
//
// [이 파일이 하는 일]
//   2026-06-08 노드 재설계 — 시작 + 6층 = 총 7층.
//
// [확정 규칙]
//   - 총 7층 (layer 0~6): 시작(0) + 전투 4(1~4) + 화톳불(5) + 보스(6)
//   - layer 0   = 시작 — NodeSystem 에서 '현재 위치' 마커 처리(클릭 불가)
//   - layer 1~4 = 전투(Combat), 층당 3갈래 분기 (모두 Combat)
//   - layer 5   = Rest(화톳불) 고정 (보스 직전)
//   - layer 6   = Boss 고정 (마지막)
//   - 이벤트 노드(진입 시 용병소/교회/엘리트 가중 랜덤) = 중간층 30% (기획 §12: 전투 70 / 이벤트 30)
//     · 현재 이벤트 결과 가중치는 용병소 100 / 교회 0 / 엘리트 0 → 용병소만 (NodeSystem 처리)
//
// [호출자]
//   NodeSystem.Awake() → mapGen.GenerateMap() → 결과를 nodeRows 에 매핑
// ============================================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("맵 설정 (기획 §11 확정)")]
    [Tooltip("총 층 수 — 2026-06-08: 시작(1) + 6층 = 7층 고정")]
    public int totalLayers = 7;

    [Tooltip("층당 분기 수 — 기획: 3갈래 (1·9·10층은 강제로 1개)")]
    public int branchPerLayer = 3;

    [Tooltip("0 이면 매 실행마다 새 맵, 그 외는 시드 고정")]
    public int fixedSeed = 0;

    [Header("이벤트 노드 (용병소/교회/엘리트 랜덤) — 2026-06-08")]
    [Tooltip("중간 전투층에서 '이벤트 노드'(진입 시 용병소/교회/엘리트 가중 랜덤)가 나올 확률(%). 기획 §12: 전투 70 / 이벤트 30.")]
    [Range(0, 100)]
    public int eventWeight = 30;

    private MapData mapData;
    private System.Random rng;

    // ============================================================
    // 공개 API — 맵 생성
    // ============================================================
    public MapData GenerateMap()
    {
        rng = fixedSeed == 0 ? new System.Random() : new System.Random(fixedSeed);

        // 튜토리얼 모드 — 기획 §15 + 2026-05-29 결정: 5노드 일렬 (Combat→Shop→Event→Elite→Boss)
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorial)
        {
            return GenerateTutorialMap();
        }

        mapData = new MapData
        {
            totalLayers = totalLayers,
            maxNodesPerLayer = branchPerLayer,
        };

        // 1) 층별 노드 생성
        var layers = BuildLayers();

        // 2) 층 간 연결
        ConnectLayers(layers);

        // 3) 시작 노드 접근 가능
        if (layers.Count > 0 && layers[0].Count > 0)
            layers[0][0].isAccessible = true;

        Debug.Log($"[MapGenerator] 맵 생성 완료 — {totalLayers}층 / 총 노드 {mapData.nodes.Count}개");
        return mapData;
    }

    /// <summary>
    /// 튜토리얼 전용 맵 — 5개 노드 일렬 (분기 없음). 사용자가 게임 전체 흐름 체험.
    /// 1층: Combat (고블린) → 2층: Shop (용병소) → 3층: Event (교회) → 4층: Elite (약탈자) → 5층: Boss (즉사 시나리오)
    /// </summary>
    private MapData GenerateTutorialMap()
    {
        const int tutorialLayers = 5;
        var sequence = new[] { RoomType.Combat, RoomType.Shop, RoomType.Event, RoomType.Elite, RoomType.Boss };

        mapData = new MapData
        {
            totalLayers = tutorialLayers,
            maxNodesPerLayer = 1,
        };

        int nodeId = 0;
        var nodes = new System.Collections.Generic.List<RoomNode>();
        for (int layer = 0; layer < tutorialLayers; layer++)
        {
            var node = new RoomNode
            {
                id          = nodeId++,
                layer       = layer,
                nextNodeIds = new System.Collections.Generic.List<int>(),
                roomType    = sequence[layer],
                position    = new Vector2(0.5f, (float)layer / (tutorialLayers - 1)),
            };
            nodes.Add(node);
            mapData.nodes.Add(node);
        }
        // 일렬 연결
        for (int i = 0; i < nodes.Count - 1; i++)
            nodes[i].nextNodeIds.Add(nodes[i + 1].id);

        nodes[0].isAccessible = true;
        Debug.Log("[MapGenerator] 튜토리얼 맵 생성 — 5노드 일렬 (Combat→Shop→Event→Elite→Boss)");
        return mapData;
    }

    // ============================================================
    // 1) 층별 노드 생성 (연속 같은 타입 방지 포함)
    // ============================================================
    private List<List<RoomNode>> BuildLayers()
    {
        var layers = new List<List<RoomNode>>();
        int nodeId = 0;

        for (int layer = 0; layer < totalLayers; layer++)
        {
            var layerNodes = new List<RoomNode>();

            int nodeCount = IsFixedSingleNodeLayer(layer) ? 1 : branchPerLayer;

            for (int i = 0; i < nodeCount; i++)
            {
                RoomType type = ResolveRoomType(layer); // 고정층 또는 전투/이벤트(비활성)
                var node = new RoomNode
                {
                    id          = nodeId++,
                    layer       = layer,
                    nextNodeIds = new List<int>(),
                    roomType    = type,
                    position    = new Vector2(
                        (i + 1f) / (nodeCount + 1f),
                        nodeCount > 1 ? (float)layer / (totalLayers - 1) : 0.5f
                    ),
                };
                layerNodes.Add(node);
                mapData.nodes.Add(node);
            }

            layers.Add(layerNodes);
        }

        return layers;
    }

    /// <summary>시작·화톳불·보스 층은 단일 노드 고정.</summary>
    private bool IsFixedSingleNodeLayer(int layer)
    {
        // layer 0 = 시작, layer totalLayers-2 = 화톳불(layer5), layer totalLayers-1 = 보스(layer6)
        return layer == 0
            || layer == totalLayers - 2
            || layer == totalLayers - 1;
    }

    // ============================================================
    // 룸 타입 결정 — 고정층(시작/화톳불/보스) + 중간층 전투/이벤트(비활성)
    // ============================================================
    private RoomType ResolveRoomType(int layer)
    {
        if (layer == 0)                  return RoomType.Combat; // 시작 (NodeSystem 마커 처리)
        if (layer == totalLayers - 2)    return RoomType.Rest;   // 화톳불
        if (layer == totalLayers - 1)    return RoomType.Boss;   // 보스

        // 중간 전투층 — eventWeight%(기본 30) 확률로 이벤트(랜덤) 노드, 그 외 전투.
        // 이벤트 결과(용병소/교회/엘리트)는 NodeSystem 진입 시 가중 랜덤으로 결정.
        if (eventWeight > 0 && rng.Next(100) < eventWeight) return RoomType.Event;
        return RoomType.Combat;
    }

    // ============================================================
    // 2) 층 간 연결 — 모든 다음 층 노드가 최소 1개 부모를 갖도록 보장
    // ============================================================
    private void ConnectLayers(List<List<RoomNode>> layers)
    {
        for (int layer = 0; layer < totalLayers - 1; layer++)
        {
            var current = layers[layer];
            var next    = layers[layer + 1];

            var connectedNext = new HashSet<int>();

            foreach (var node in current)
            {
                int connectCount = rng.Next(1, Mathf.Min(3, next.Count) + 1);
                var candidates   = next.OrderBy(n => Mathf.Abs(n.position.x - node.position.x)).ToList();

                for (int c = 0; c < connectCount && c < candidates.Count; c++)
                {
                    int cid = candidates[c].id;
                    if (!node.nextNodeIds.Contains(cid))
                    {
                        node.nextNodeIds.Add(cid);
                        connectedNext.Add(cid);
                    }
                }
            }

            // 고립된 다음 노드 강제 연결
            foreach (var nextNode in next)
            {
                if (connectedNext.Contains(nextNode.id)) continue;
                var randomPrev = current[rng.Next(current.Count)];
                if (!randomPrev.nextNodeIds.Contains(nextNode.id))
                    randomPrev.nextNodeIds.Add(nextNode.id);
            }
        }
    }
}
