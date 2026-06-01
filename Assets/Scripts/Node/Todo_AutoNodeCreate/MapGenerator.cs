// ============================================================
// MapGenerator.cs — 자동 맵 생성기
// ============================================================
//
// [이 파일이 하는 일]
//   기획 §11_맵_노드_설계 / §MVP_02_노드_설계 를 따라
//   10층 × 층당 3갈래 분기 맵 데이터를 자동 생성합니다.
//
// [확정 규칙]
//   - 총 10층 (layer 0~9 = 1~10층)
//   - 층당 3갈래 분기 (1·9·10층은 항상 단일 노드)
//   - 1층(layer 0) = Combat 고정 (시작)
//   - 9층(layer 8) = Rest(화툿불) 고정
//   - 10층(layer 9) = Boss 고정
//   - 노드 타입 비율 (가중치): 전투 70% / 이벤트 30%
//     · 이벤트 30 = 용병소(Shop) 10 + 교회(Event) 10 + 엘리트(Elite) 10
//   - 연속 같은 타입 3회 이상 방지 (재시도)
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
    [Tooltip("총 층 수 — 기획 §맵 노드 설계: 10층 고정")]
    public int totalLayers = 10;

    [Tooltip("층당 분기 수 — 기획: 3갈래 (1·9·10층은 강제로 1개)")]
    public int branchPerLayer = 3;

    [Tooltip("0 이면 매 실행마다 새 맵, 그 외는 시드 고정")]
    public int fixedSeed = 0;

    // ── 룸 타입 가중치 (층별 동적 계산) ───────────────────────────
    // 2026-05-29 정책: 엘리트는 후반에 몰리고, 일반 전투는 초반에 몰리도록 layer 기반 가중치.
    // Shop/Event 는 20/10 고정, Combat + Elite = 70 을 layer 따라 분배.
    //
    // | layer | 층 | Combat | Elite | Shop | Event |
    // |   1   | 2 |   70   |   0   |  20  |  10   |
    // |   2   | 3 |   65   |   5   |  20  |  10   |
    // |   3   | 4 |   55   |  15   |  20  |  10   |
    // |   4   | 5 |   45   |  25   |  20  |  10   |
    // |   5   | 6 |   40   |  30   |  20  |  10   |
    // |   6   | 7 |   35   |  35   |  20  |  10   |
    // |   7   | 8 |   30   |  40   |  20  |  10   |
    [Header("룸 타입 확률 — 층별 동적 (기획 §11 + 2026-05-29 분포 조정)")]
    [Tooltip("Shop 가중치 (전 층 동일)")]
    public int shopWeight = 20;

    [Tooltip("Event(교회) 가중치 (전 층 동일)")]
    public int eventWeight = 10;

    [Header("연속 방지")]
    [Tooltip("같은 타입이 연속으로 이만큼 나오면 재롤. 기획 §11 §연속 같은 타입 3회 이상 방지")]
    public int maxConsecutiveSameType = 2;

    [Tooltip("재롤 시도 최대 횟수 (무한 루프 방지)")]
    public int maxRerollAttempts = 10;

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

        // 같은 층 내 직전 타입 추적 — 연속 방지용
        // (층마다 갈래가 3개라 같은 층 내 3개가 모두 같은 타입이 되는 것도 막음)
        for (int layer = 0; layer < totalLayers; layer++)
        {
            var layerNodes = new List<RoomNode>();

            int nodeCount = IsFixedSingleNodeLayer(layer) ? 1 : branchPerLayer;

            // 직전 연속 카운트 (이 층의 후보들끼리)
            RoomType? lastType = null;
            int consecutiveCount = 0;

            for (int i = 0; i < nodeCount; i++)
            {
                RoomType type = ResolveRoomType(layer, lastType, consecutiveCount);
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

                // 연속 카운트 업데이트
                if (lastType.HasValue && lastType.Value == type) consecutiveCount++;
                else consecutiveCount = 1;
                lastType = type;
            }

            layers.Add(layerNodes);
        }

        return layers;
    }

    /// <summary>1·9·10층은 단일 노드 고정 (기획 §맵 노드 설계).</summary>
    private bool IsFixedSingleNodeLayer(int layer)
    {
        // layer 0 = 1층(시작), layer = totalLayers-2 = 9층(화툿불), layer = totalLayers-1 = 10층(보스)
        return layer == 0
            || layer == totalLayers - 2
            || layer == totalLayers - 1;
    }

    // ============================================================
    // 룸 타입 결정 (고정 층 우선, 그 외는 가중치 + 연속 방지)
    // ============================================================
    private RoomType ResolveRoomType(int layer, RoomType? lastType, int consecutiveCount)
    {
        // 1) 층 고정 — 기획 §맵 노드 §1·9·10층 고정
        if (layer == 0)                  return RoomType.Combat;
        if (layer == totalLayers - 2)    return RoomType.Rest;
        if (layer == totalLayers - 1)    return RoomType.Boss;

        // 2) 가중치 랜덤 + 연속 방지 재롤
        for (int attempt = 0; attempt < maxRerollAttempts; attempt++)
        {
            RoomType picked = WeightedRollRoomType(layer);

            // 연속 같은 타입 maxConsecutiveSameType 회 도달 시 재롤
            bool wouldExceedConsecutive =
                lastType.HasValue
                && lastType.Value == picked
                && consecutiveCount >= maxConsecutiveSameType;

            if (!wouldExceedConsecutive) return picked;
        }

        // 재롤 다 실패하면 일단 반환 (안전장치)
        return WeightedRollRoomType(layer);
    }

    /// <summary>layer 1~7 (2~8층) 의 Combat/Elite 가중치를 표 기반으로 반환. 초반은 Combat, 후반은 Elite 우세.</summary>
    private (int combat, int elite) GetCombatEliteWeights(int layer)
    {
        // 표 lookup — layer=1 → Combat 70/Elite 0, layer=7 → Combat 30/Elite 40
        switch (layer)
        {
            case 1: return (70, 0);
            case 2: return (65, 5);
            case 3: return (55, 15);
            case 4: return (45, 25);
            case 5: return (40, 30);
            case 6: return (35, 35);
            case 7: return (30, 40);
            default: return (55, 15); // 안전 폴백 (호출되지 않아야 함 — 1·9·10층은 ResolveRoomType 앞단에서 고정)
        }
    }

    private RoomType WeightedRollRoomType(int layer)
    {
        var (combatWeight, eliteWeight) = GetCombatEliteWeights(layer);

        int total = combatWeight + eliteWeight + shopWeight + eventWeight;
        if (total <= 0) return RoomType.Combat; // 가중치 다 0이면 안전 폴백

        int roll = rng.Next(total);
        int cumulative = 0;

        cumulative += combatWeight; if (roll < cumulative) return RoomType.Combat;
        cumulative += eliteWeight;  if (roll < cumulative) return RoomType.Elite;
        cumulative += shopWeight;   if (roll < cumulative) return RoomType.Shop;
        cumulative += eventWeight;  if (roll < cumulative) return RoomType.Event;
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
