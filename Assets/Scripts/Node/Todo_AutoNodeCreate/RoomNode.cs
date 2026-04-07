using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomNode
{
    public int id;
    public RoomType roomType;      // 전투, 상점, 보스, 휴식, 이벤트 등
    public int layer;              // 깊이 (층)
    public List<int> nextNodeIds;  // 연결된 다음 노드들
    public Vector2 position;       // UI 상 위치
    public bool isVisited;
    public bool isAccessible;
}

public enum RoomType { Combat, Elite, Shop, Rest, Event, Boss, Treasure }