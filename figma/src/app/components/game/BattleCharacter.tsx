import { Shield, Zap, Heart, Skull } from 'lucide-react';
import { JobClass } from '../../utils/allyNameGenerator';

/**
 * 전투 캐릭터 컴포넌트
 * 아군/적 캐릭터 표시
 */

export type CharacterRole = 'dealer' | 'tank' | 'supporter';

export interface CharacterData {
  id: string;
  name: string;
  role?: CharacterRole;
  jobClass?: JobClass;
  displayJobName?: string;
  hp: number;
  maxHp: number;
  stress?: number;
  maxStress?: number;
  position?: 'front' | 'back';
  isEnemy?: boolean;
}

interface BattleCharacterProps {
  character: CharacterData;
  isAlly?: boolean;
  onClick?: () => void;
}

const roleStyles: Record<CharacterRole, {
  icon: React.ReactNode;
  color: string;
  label: string;
}> = {
  dealer: {
    icon: <Zap className="w-6 h-6" />,
    color: '#B3263E',
    label: '딜러'
  },
  tank: {
    icon: <Shield className="w-6 h-6" />,
    color: '#4C566A',
    label: '탱커'
  },
  supporter: {
    icon: <Heart className="w-6 h-6" />,
    color: '#4CB3B3',
    label: '서포터'
  }
};

export function BattleCharacter({ character, isAlly, onClick }: BattleCharacterProps) {
  const hpPercentage = (character.hp / character.maxHp) * 100;
  const stressPercentage = character.stress && character.maxStress 
    ? (character.stress / character.maxStress) * 100 
    : 0;
  const role = character.role ? roleStyles[character.role] : undefined;

  return (
    <div
      onClick={onClick}
      className="relative select-none"
      style={{
        width: '140px',
        cursor: onClick ? 'pointer' : 'default',
        transition: 'all 0.3s ease'
      }}
    >
      <div
        className="relative w-32 h-32 mx-auto mb-2 rounded-lg flex items-center justify-center overflow-hidden"
        style={{
          backgroundColor: character.isEnemy ? '#2A1A1F' : '#1A2A2F',
          border: `2px solid ${character.isEnemy ? '#B3263E' : role?.color || '#4C566A'}`,
          boxShadow: '0 4px 12px rgba(0, 0, 0, 0.5)'
        }}
      >
        <div style={{ color: character.isEnemy ? '#B3263E' : role?.color || '#4CB3B3' }}>
          {character.isEnemy ? (
            <Skull className="w-16 h-16" />
          ) : (
            role?.icon || <Shield className="w-16 h-16" />
          )}
        </div>

        {!character.isEnemy && role && (
          <div
            className="absolute top-1 right-1 w-6 h-6 rounded flex items-center justify-center"
            style={{
              backgroundColor: 'rgba(0, 0, 0, 0.7)',
              border: `1px solid ${role.color}`
            }}
          >
            <div style={{ color: role.color }} className="scale-75">
              {role.icon}
            </div>
          </div>
        )}

        <div
          className="absolute bottom-1 left-1 px-1.5 py-0.5 rounded text-[10px] font-bold"
          style={{
            backgroundColor: 'rgba(0, 0, 0, 0.7)',
            color: character.position === 'front' ? '#C97A2B' : '#4C78A8',
            border: `1px solid ${character.position === 'front' ? '#C97A2B' : '#4C78A8'}`
          }}
        >
          {character.position === 'front' ? '전열' : '후열'}
        </div>
      </div>

      <div
        className="text-center text-sm font-bold mb-1"
        style={{ color: '#E6E0D6' }}
      >
        {character.name}
      </div>

      <div className="relative">
        <div
          className="h-3 rounded-full overflow-hidden"
          style={{
            backgroundColor: '#1B1F27',
            border: '1px solid #3A4452'
          }}
        >
          <div
            className="h-full transition-all duration-500"
            style={{
              width: `${hpPercentage}%`,
              backgroundColor: hpPercentage > 50 ? '#4F8F63' : hpPercentage > 25 ? '#C97A2B' : '#B3263E',
              boxShadow: `0 0 8px ${hpPercentage > 50 ? '#4F8F6380' : hpPercentage > 25 ? '#C97A2B80' : '#B3263E80'}`
            }}
          />
        </div>

        <div
          className="absolute inset-0 flex items-center justify-center text-[10px] font-bold pointer-events-none"
          style={{
            color: '#E6E0D6',
            textShadow: '0 1px 2px rgba(0, 0, 0, 0.8)'
          }}
        >
          {character.hp} / {character.maxHp}
        </div>
      </div>

      {character.stress && character.maxStress && (
        <div className="relative">
          <div
            className="h-3 rounded-full overflow-hidden"
            style={{
              backgroundColor: '#1B1F27',
              border: '1px solid #3A4452'
            }}
          >
            <div
              className="h-full transition-all duration-500"
              style={{
                width: `${stressPercentage}%`,
                backgroundColor: stressPercentage > 50 ? '#FF5733' : '#FFC300',
                boxShadow: `0 0 8px ${stressPercentage > 50 ? '#FF573380' : '#FFC30080'}`
              }}
            />
          </div>

          <div
            className="absolute inset-0 flex items-center justify-center text-[10px] font-bold pointer-events-none"
            style={{
              color: '#E6E0D6',
              textShadow: '0 1px 2px rgba(0, 0, 0, 0.8)'
            }}
          >
            {character.stress} / {character.maxStress}
          </div>
        </div>
      )}

      <div className="mt-1 min-h-4 flex gap-1 justify-center">
      </div>
    </div>
  );
}
