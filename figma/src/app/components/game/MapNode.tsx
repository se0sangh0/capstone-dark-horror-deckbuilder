/**
 * 맵 노드 컴포넌트
 * 각 노드는 시작, 전투, 화툿불, 용병소, 교회, 엘리트 전투, 보스 등의 타입을 가짐
 * 기획서 기준: MVP 우선 구현 - 전투, 화툿불, 보스, 용병소
 */

import { Swords, Flame, Users, Church, Skull, Play, Zap } from 'lucide-react';

// MVP 노드 타입 (우선 구현: combat, campfire, boss, mercenary)
// 백로그: church, elite
export type NodeType = 'start' | 'combat' | 'campfire' | 'mercenary' | 'church' | 'elite' | 'boss';

export interface MapNodeData {
  id: string;
  type: NodeType;
  x: number;
  y: number;
  visited?: boolean;
  available?: boolean;
  current?: boolean;
}

interface MapNodeProps {
  node: MapNodeData;
  onClick?: (node: MapNodeData) => void;
}

const nodeConfig: Record<NodeType, {
  icon: React.ComponentType<{ className?: string }>;
  bgColor: string;
  borderColor: string;
  iconColor: string;
  label: string;
}> = {
  start: {
    icon: Play,
    bgColor: '#2A2230',
    borderColor: '#C39A52',
    iconColor: '#C39A52',
    label: '시작'
  },
  combat: {
    icon: Swords,
    bgColor: '#2A2230',
    borderColor: '#D16474',
    iconColor: '#D16474',
    label: '전투'
  },
  campfire: {
    icon: Flame,
    bgColor: '#2A2230',
    borderColor: '#C97A2B',
    iconColor: '#C97A2B',
    label: '화툿불'
  },
  mercenary: {
    icon: Users,
    bgColor: '#2A2230',
    borderColor: '#C39A52',
    iconColor: '#C39A52',
    label: '용병소'
  },
  church: {
    icon: Church,
    bgColor: '#2A2230',
    borderColor: '#7B5EA7',
    iconColor: '#7B5EA7',
    label: '교회'
  },
  elite: {
    icon: Zap,
    bgColor: '#2A2230',
    borderColor: '#B3263E',
    iconColor: '#B3263E',
    label: '엘리트 전투'
  },
  boss: {
    icon: Skull,
    bgColor: '#1B1F27',
    borderColor: '#B3263E',
    iconColor: '#B3263E',
    label: '보스'
  }
};

export function MapNode({ node, onClick }: MapNodeProps) {
  const config = nodeConfig[node.type];
  const Icon = config.icon;

  const handleClick = () => {
    if (node.available && onClick) {
      onClick(node);
    }
  };

  const size = node.type === 'boss' ? 48 : 36;
  const opacity = node.visited ? 0.5 : node.available ? 1 : 0.6;
  const cursor = node.available ? 'pointer' : 'default';

  return (
    <div
      className="absolute flex items-center justify-center transition-all duration-200"
      style={{
        left: `${node.x}px`,
        top: `${node.y}px`,
        width: `${size}px`,
        height: `${size}px`,
        transform: 'translate(-50%, -50%)',
        opacity,
        cursor
      }}
      onClick={handleClick}
    >
      {node.current && (
        <div
          className="absolute inset-0 rounded-full animate-pulse"
          style={{
            border: '2px solid #C39A52',
            boxShadow: '0 0 20px #C39A52'
          }}
        />
      )}

      <div
        className="relative w-full h-full rounded-full flex items-center justify-center border-2 transition-all hover:brightness-125"
        style={{
          backgroundColor: config.bgColor,
          borderColor: node.current ? '#C39A52' : config.borderColor,
          boxShadow: node.available ? `0 0 10px ${config.borderColor}50` : 'none'
        }}
      >
        <Icon
          className="w-1/2 h-1/2"
          style={{ color: config.iconColor }}
        />
      </div>

      {node.visited && (
        <div
          className="absolute inset-0 flex items-center justify-center text-xl"
          style={{ color: '#4F8F63' }}
        >
          ✓
        </div>
      )}
    </div>
  );
}
