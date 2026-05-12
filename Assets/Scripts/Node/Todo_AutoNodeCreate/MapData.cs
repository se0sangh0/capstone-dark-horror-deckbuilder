// MapData.cs — 기획 §11_맵_노드_설계 / §MVP_02_노드_설계
//   총 10층, 층당 3갈래 중 1개 선택, 총 방문 10개
using System.Collections.Generic;

[System.Serializable]
public class MapData
{
    public List<RoomNode> nodes = new List<RoomNode>();
    public int totalLayers = 10;     // 기획 §총 층 수
    public int maxNodesPerLayer = 3; // 기획 §층당 3갈래 분기
}