import { Shield, Sword, Heart } from 'lucide-react';

/**
 * 전투 카드 컴포넌트
 * 손패 영역에 표시되는 카드
 */

export interface BattleCardData {
  id: string;
  role: 'dealer' | 'tank' | 'supporter';
  tendency: 'gambler' | 'safety' | 'opportunist' | 'optimist';
  stackValue: number;
}

interface BattleCardProps {
  card: BattleCardData;
  selected?: boolean;
  onClick?: () => void;
}

export function BattleCard({ card, selected = false, onClick }: BattleCardProps) {
  const roleColors = {
    dealer: '#B3263E',
    tank: '#4C566A',
    supporter: '#4CB3B3'
  };

  const getValueColor = (value: number) => {
    if (value > 0) return '#4F8F63';
    if (value < 0) return '#B3263E';
    return '#8A8F98';
  };

  return (
    <div
      onClick={onClick}
      className="cursor-pointer transition-all duration-300"
      style={{
        transform: selected ? 'translateY(-20px) scale(1.05)' : 'translateY(0)',
        filter: selected ? 'drop-shadow(0 0 20px rgba(195, 154, 82, 0.6))' : 'none'
      }}
    >
      <div
        className="relative rounded-lg p-4 flex flex-col items-center justify-center"
        style={{
          width: '120px',
          height: '160px',
          backgroundColor: '#2A2230',
          border: `3px solid ${selected ? '#C39A52' : roleColors[card.role]}`,
          boxShadow: selected
            ? `0 0 30px ${roleColors[card.role]}80`
            : `0 4px 12px rgba(0, 0, 0, 0.4)`
        }}
      >
        <div
          className="absolute top-2 left-2 w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold"
          style={{
            backgroundColor: roleColors[card.role],
            color: '#E6E0D6'
          }}
        >
          {card.role === 'dealer' ? '딜' : card.role === 'tank' ? '탱' : '힐'}
        </div>

        <div className="flex-1 flex items-center justify-center">
          <div
            className="text-5xl font-bold"
            style={{
              color: getValueColor(card.stackValue),
              textShadow: `0 0 15px ${getValueColor(card.stackValue)}80`,
              fontFamily: 'monospace'
            }}
          >
            {card.stackValue > 0 ? '+' : ''}{card.stackValue}
          </div>
        </div>

        <div
          className="text-xs font-bold mt-2"
          style={{
            color: '#B5ADA0'
          }}
        >
          스택 {card.stackValue > 0 ? '증가' : card.stackValue < 0 ? '감소' : '유지'}
        </div>

        <div
          className="absolute top-0 left-0 right-0 h-1"
          style={{
            background: `linear-gradient(to right, transparent, ${roleColors[card.role]}, transparent)`,
            opacity: 0.5
          }}
        />
        <div
          className="absolute bottom-0 left-0 right-0 h-1"
          style={{
            background: `linear-gradient(to right, transparent, ${roleColors[card.role]}, transparent)`,
            opacity: 0.5
          }}
        />
      </div>
    </div>
  );
}
