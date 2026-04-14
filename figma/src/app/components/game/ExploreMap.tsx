/**
 * 탐험 맵 컴포넌트
 * Slay the Spire 스타일의 노드 맵 시스템
 */

import { MapNode, MapNodeData, NodeType } from './MapNode';

interface ExploreMapProps {
  onNodeClick?: (node: MapNodeData) => void;
}

const NODE_WEIGHTS = {
  combat: 60,
  mercenary: 40,
  church: 0,
  elite: 0
};

function getRandomNodeType(excludeRecent: NodeType[] = []): NodeType {
  const availableTypes: NodeType[] = ['combat', 'mercenary'];
  const filteredTypes = availableTypes.filter(
    type => !excludeRecent.slice(-2).every(recent => recent === type)
  );
  const types = filteredTypes.length > 0 ? filteredTypes : availableTypes;
  const totalWeight = types.reduce((sum, type) => sum + NODE_WEIGHTS[type], 0);
  let random = Math.random() * totalWeight;
  for (const type of types) {
    random -= NODE_WEIGHTS[type];
    if (random <= 0) return type;
  }
  return types[0];
}

function generateMap(): { nodes: MapNodeData[], connections: [string, string][] } {
  const nodes: MapNodeData[] = [];
  const connections: [string, string][] = [];
  const TOTAL_FLOORS = 10;
  const NODES_PER_FLOOR = 3;
  const mapWidth = 800;
  const mapHeight = 600;
  const layerHeight = mapHeight / (TOTAL_FLOORS + 1);
  let nodeId = 0;
  const layerNodes: MapNodeData[][] = [];
  const recentTypes: NodeType[] = [];

  for (let floor = 0; floor < TOTAL_FLOORS; floor++) {
    const y = (floor + 1) * layerHeight;
    const layerNodesArray: MapNodeData[] = [];

    if (floor === 0) {
      const node: MapNodeData = {
        id: `node-${nodeId++}`,
        type: 'start',
        x: mapWidth / 2,
        y,
        visited: true,
        available: true,
        current: true
      };
      nodes.push(node);
      layerNodesArray.push(node);
    }
    else if (floor === TOTAL_FLOORS - 2) {
      const node: MapNodeData = {
        id: `node-${nodeId++}`,
        type: 'campfire',
        x: mapWidth / 2,
        y,
        visited: false,
        available: false,
        current: false
      };
      nodes.push(node);
      layerNodesArray.push(node);
    }
    else if (floor === TOTAL_FLOORS - 1) {
      const node: MapNodeData = {
        id: `node-${nodeId++}`,
        type: 'boss',
        x: mapWidth / 2,
        y,
        visited: false,
        available: false,
        current: false
      };
      nodes.push(node);
      layerNodesArray.push(node);
    }
    else {
      for (let i = 0; i < NODES_PER_FLOOR; i++) {
        const spacing = mapWidth / (NODES_PER_FLOOR + 1);
        const x = spacing * (i + 1);
        const type = getRandomNodeType(recentTypes);
        recentTypes.push(type);
        if (recentTypes.length > 3) recentTypes.shift();
        const node: MapNodeData = {
          id: `node-${nodeId++}`,
          type,
          x,
          y,
          visited: false,
          available: floor === 1,
          current: false
        };
        nodes.push(node);
        layerNodesArray.push(node);
      }
    }
    layerNodes.push(layerNodesArray);
  }

  for (let layerIndex = 0; layerIndex < layerNodes.length - 1; layerIndex++) {
    const currentLayer = layerNodes[layerIndex];
    const nextLayer = layerNodes[layerIndex + 1];
    currentLayer.forEach((node) => {
      nextLayer.forEach((nextNode) => {
        connections.push([node.id, nextNode.id]);
      });
    });
  }

  return { nodes, connections };
}

export function ExploreMap({ onNodeClick }: ExploreMapProps) {
  const { nodes, connections } = generateMap();

  return (
    <div className="w-full h-full flex items-center justify-center p-8">
      <div 
        className="relative rounded-lg border-2 overflow-hidden"
        style={{
          width: '800px',
          height: '600px',
          backgroundColor: '#151821',
          borderColor: '#3A4452',
          backgroundImage: 'radial-gradient(circle at 50% 50%, #1B1F2720 0%, transparent 100%)'
        }}
      >
        <svg 
          className="absolute inset-0 w-full h-full pointer-events-none"
          style={{ zIndex: 0 }}
        >
          {connections.map(([fromId, toId], index) => {
            const fromNode = nodes.find(n => n.id === fromId);
            const toNode = nodes.find(n => n.id === toId);
            if (!fromNode || !toNode) return null;
            const isAvailable = fromNode.visited || fromNode.available;
            return (
              <line
                key={`${fromId}-${toId}-${index}`}
                x1={fromNode.x}
                y1={fromNode.y}
                x2={toNode.x}
                y2={toNode.y}
                stroke={isAvailable ? '#667085' : '#3A4452'}
                strokeWidth="2"
                strokeDasharray={isAvailable ? '0' : '4 4'}
                opacity={isAvailable ? 0.6 : 0.3}
              />
            );
          })}
        </svg>

        <div className="relative w-full h-full" style={{ zIndex: 1 }}>
          {nodes.map(node => (
            <MapNode
              key={node.id}
              node={node}
              onClick={onNodeClick}
            />
          ))}
        </div>

        <div
          className="absolute top-4 left-4 p-3 rounded border"
          style={{
            backgroundColor: '#1B1F27E0',
            borderColor: '#3A4452'
          }}
        >
          <div className="text-xs font-bold mb-2" style={{ color: '#C39A52' }}>노드 종류</div>
          <div className="space-y-1.5">
            <LegendItem color="#D16474" label="전투" />
            <LegendItem color="#C39A52" label="용병소" />
            <LegendItem color="#C97A2B" label="화툿불 (9층)" />
            <LegendItem color="#B3263E" label="보스 (10층)" />
          </div>
        </div>

        <div 
          className="absolute top-4 right-4 p-2 rounded border"
          style={{
            backgroundColor: '#1B1F27E0',
            borderColor: '#3A4452'
          }}
        >
          <div className="text-xs" style={{ color: '#B5ADA0' }}>현재 층</div>
          <div className="text-xl font-bold" style={{ color: '#C39A52' }}>1F</div>
        </div>
      </div>
    </div>
  );
}

function LegendItem({ color, label }: { color: string; label: string }) {
  return (
    <div className="flex items-center gap-2">
      <div 
        className="w-3 h-3 rounded-full border"
        style={{ 
          backgroundColor: '#2A2230',
          borderColor: color
        }}
      />
      <span className="text-xs" style={{ color: '#B5ADA0' }}>{label}</span>
    </div>
  );
}
