// ============================================================
// MapGenerator.cs — 맵 생성기 (MVP 고정 일렬)
// ============================================================
//
// [이 파일이 하는 일]
//   기획 §12_맵_노드_설계 (MVP) — 6층 고정 시퀀스, 분기 없음.
//
// [확정 규칙 — 2026-06-09]
//   MVP 맵 = 6노드 일렬 (layer 0~5):
//     layer 0 = 시작   (RoomType.Combat — NodeSystem 이 '현재 위치' 마커로 처리, 클릭 불가)
//     layer 1 = 전투   (Combat)
//     layer 2 = 용병소 (Shop)
//     layer 3 = 전투   (Combat)
//     layer 4 = 화톳불 (Rest)
//     layer 5 = 보스   (Boss)
//   분기/랜덤/이벤트 노드 없음 (정식 버전의 분기 설계는 백로그 — 기획 §12 "전체 설계").
//
// [튜토리얼 맵] 5노드 일렬 (Combat→Shop→Event→Elite→Boss) — 별도 유지.
//
// [호출자]
//   NodeSystem.Awake() → mapGen.GenerateMap() → 결과를 nodeRows 에 매핑
// ============================================================

using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    private MapData mapData;

    // ============================================================
    // 공개 API — 맵 생성
    // ============================================================
    public MapData GenerateMap()
    {
        // 튜토리얼 모드 — 기획 §15 + 2026-05-29 결정: 5노드 일렬 (Combat→Shop→Event→Elite→Boss)
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorial)
            return GenerateTutorialMap();

        return GenerateMvpMap();
    }

    /// <summary>
    /// MVP 맵 — 기획 §12 6노드 고정 일렬 (분기 없음).
    /// 시작 > 전투 > 용병소 > 전투 > 화톳불 > 보스
    /// </summary>
    private MapData GenerateMvpMap()
    {
        // layer 0 시작 / 1 전투 / 2 랜덤노드(용병소/교회/엘리트, 발표용 용병소) / 3 전투 / 4 화톳불 / 5 보스
        var sequence = new[]
        {
            RoomType.Combat, // 0 시작 (NodeSystem 마커 처리)
            RoomType.Combat, // 1 전투
            RoomType.Event,  // 2 랜덤노드 (클릭 시 용병소/교회/엘리트 중 1 — 발표용 100/0/0 용병소)
            RoomType.Combat, // 3 전투
            RoomType.Rest,   // 4 화톳불
            RoomType.Boss,   // 5 보스
        };

        mapData = BuildLinearMap(sequence);
        Debug.Log("[MapGenerator] MVP 맵 생성 — 6노드 일렬 (시작>전투>랜덤노드>전투>화톳불>보스)");
        return mapData;
    }

    /// <summary>
    /// 튜토리얼 전용 맵 — 4개 노드 일렬 (분기 없음). 사용자가 게임 전체 흐름 체험.
    /// 1층 Combat(고블린) → 2층 Shop(용병소) → 3층 Rest(화톳불) → 4층 Boss(즉사 시나리오)
    /// </summary>
    private MapData GenerateTutorialMap()
    {
        var sequence = new[]
        {
            RoomType.Combat, RoomType.Shop, RoomType.Rest, RoomType.Boss,
        };

        mapData = BuildLinearMap(sequence);
        Debug.Log("[MapGenerator] 튜토리얼 맵 생성 — 4노드 일렬 (Combat→Shop→Rest→Boss)");
        return mapData;
    }

    // ============================================================
    // 고정 일렬 맵 빌더 — 시퀀스 길이만큼 노드 1개씩, 직선 연결.
    // ============================================================
    private MapData BuildLinearMap(RoomType[] sequence)
    {
        int layers = sequence.Length;
        var data = new MapData
        {
            totalLayers = layers,
            maxNodesPerLayer = 1,
        };

        var nodes = new List<RoomNode>();
        for (int layer = 0; layer < layers; layer++)
        {
            var node = new RoomNode
            {
                id          = layer,
                layer       = layer,
                nextNodeIds = new List<int>(),
                roomType    = sequence[layer],
                position    = new Vector2(0.5f, layers > 1 ? (float)layer / (layers - 1) : 0.5f),
            };
            nodes.Add(node);
            data.nodes.Add(node);
        }

        // 일렬 연결 (각 노드 → 다음 노드 하나)
        for (int i = 0; i < nodes.Count - 1; i++)
            nodes[i].nextNodeIds.Add(nodes[i + 1].id);

        if (nodes.Count > 0)
            nodes[0].isAccessible = true;

        return data;
    }
}
