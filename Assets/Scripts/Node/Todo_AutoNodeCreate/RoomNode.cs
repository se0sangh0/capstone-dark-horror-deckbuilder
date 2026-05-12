using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomNode
{
    public int id;
    public RoomType roomType;      // 기획 §11_맵_노드 §노드 분류 — 오픈 3종(Combat/Rest/Boss) + 이벤트 3종(Shop/Event/Elite)
    public int layer;              // 깊이 (층) — 0-base, layer 0 = 1층
    public List<int> nextNodeIds;  // 연결된 다음 노드들
    public Vector2 position;       // UI 상 위치 (0~1 정규화)
    public bool isVisited;
    public bool isAccessible;
}

// 기획 §11_맵_노드 §노드 분류 (MVP 6종)
//   오픈 노드(진입 전 공개)   : Combat / Rest(화툿불) / Boss
//   이벤트 노드(진입 전 비공개): Shop(용병소) / Event(교회·백로그) / Elite(엘리트 전투·백로그)
public enum RoomType { Combat, Elite, Shop, Rest, Event, Boss }