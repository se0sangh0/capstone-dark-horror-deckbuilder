using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// MapGenerator.cs
public class MapGenerator : MonoBehaviour
{
    [Header("맵 설정")]
    public int totalLayers = 15;
    public int minNodesPerLayer = 1;
    public int maxNodesPerLayer = 3;
    public int fixedSeed = 0; // 0이면 랜덤

    // 룸 타입 가중치 (합계 = 100)
    [Header("룸 타입 확률 (%)")]
    public int combatWeight = 45;
    public int eliteWeight = 10;
    public int shopWeight = 10;
    public int restWeight = 15;
    public int eventWeight = 15;
    public int treasureWeight = 5;

    private MapData mapData;
    private System.Random rng;

    public MapData GenerateMap()
    {
        rng = fixedSeed == 0 ? new System.Random() : new System.Random(fixedSeed);
        mapData = new MapData();
        mapData.totalLayers = totalLayers;

        // 1) 각 층의 노드 개수 결정 및 노드 생성
        List<List<RoomNode>> layers = new List<List<RoomNode>>();
        int nodeId = 0;

        for (int layer = 0; layer < totalLayers; layer++)
        {
            List<RoomNode> layerNodes = new List<RoomNode>();

            // 첫/마지막 층은 단일 노드 (시작/보스)
            int nodeCount = (layer == 0 || layer == totalLayers - 1)
                ? 1
                : rng.Next(minNodesPerLayer, maxNodesPerLayer + 1);

            for (int i = 0; i < nodeCount; i++)
            {
                RoomNode node = new RoomNode
                {
                    id = nodeId++,
                    layer = layer,
                    nextNodeIds = new List<int>(),
                    roomType = DetermineRoomType(layer),
                    // UI 위치: 층은 Y축, 같은 층 노드는 X축 분산
                    position = new Vector2(
                        (i + 1f) / (nodeCount + 1f),  // 0~1 정규화
                        (float)layer / (totalLayers - 1)
                    )
                };
                layerNodes.Add(node);
                mapData.nodes.Add(node);
            }
            layers.Add(layerNodes);
        }

        // 2) 층 간 연결 (경로가 끊기지 않도록)
        for (int layer = 0; layer < totalLayers - 1; layer++)
        {
            List<RoomNode> current = layers[layer];
            List<RoomNode> next = layers[layer + 1];

            // 모든 다음 층 노드가 최소 1개 이상 연결되도록 보장
            HashSet<int> connectedNext = new HashSet<int>();

            foreach (RoomNode node in current)
            {
                int connectCount = rng.Next(1, Mathf.Min(3, next.Count) + 1);
                List<RoomNode> candidates = GetSortedCandidates(node, next);

                for (int c = 0; c < connectCount && c < candidates.Count; c++)
                {
                    if (!node.nextNodeIds.Contains(candidates[c].id))
                    {
                        node.nextNodeIds.Add(candidates[c].id);
                        connectedNext.Add(candidates[c].id);
                    }
                }
            }

            // 연결 안 된 다음 노드가 있으면 강제 연결
            foreach (RoomNode nextNode in next)
            {
                if (!connectedNext.Contains(nextNode.id))
                {
                    RoomNode randomPrev = current[rng.Next(current.Count)];
                    if (!randomPrev.nextNodeIds.Contains(nextNode.id))
                        randomPrev.nextNodeIds.Add(nextNode.id);
                }
            }
        }

        // 3) 시작 노드 접근 가능 설정
        layers[0][0].isAccessible = true;

        return mapData;
    }

    // 위치가 가까운 노드를 우선 연결 (교차선 최소화)
    private List<RoomNode> GetSortedCandidates(RoomNode from, List<RoomNode> candidates)
    {
        return candidates
            .OrderBy(n => Mathf.Abs(n.position.x - from.position.x))
            .ToList();
    }

    private RoomType DetermineRoomType(int layer)
    {
        if (layer == 0) return RoomType.Combat;
        if (layer == totalLayers - 1) return RoomType.Boss;
        if (layer == totalLayers - 2) return RoomType.Rest;

        // 가중치 기반 랜덤
        int roll = rng.Next(100);
        int cumulative = 0;
        if ((cumulative += combatWeight) > roll) return RoomType.Combat;
        if ((cumulative += eliteWeight) > roll) return RoomType.Elite;
        if ((cumulative += shopWeight) > roll) return RoomType.Shop;
        if ((cumulative += restWeight) > roll) return RoomType.Rest;
        if ((cumulative += eventWeight) > roll) return RoomType.Event;
        return RoomType.Treasure;
    }
}