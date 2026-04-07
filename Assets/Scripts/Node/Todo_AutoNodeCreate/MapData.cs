// MapData.cs
using System.Collections.Generic;

[System.Serializable]
public class MapData
{
    public List<RoomNode> nodes = new List<RoomNode>();
    public int totalLayers = 15;   // 세로 층 수
    public int maxNodesPerLayer = 3; // 층당 최대 노드 수
}