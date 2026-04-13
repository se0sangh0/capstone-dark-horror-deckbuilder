import { Map, Swords, Users, Church, Flame } from 'lucide-react';
import { Button } from '../ui/button';
import { ExplorePanel } from './ExplorePanel';
import { BattlePanel } from './BattlePanel';
import { MercenaryOfficePanel } from './MercenaryOfficePanel';
import { AllyData } from './RecruitPopup';

/**
 * 중앙 콘텐츠 영역 - 교체 가능한 패널
 * Unity 좌표: X: 320px, Y: 0, Width: 1600px, Height: 1080px
 *
 * 기획서 기준 뷰 타입:
 * - exploration: 맵 탐험
 * - battle: 전투
 * - campfire: 화툿불 (체력/스트레스 회복)
 * - mercenary: 용병소 (동료 모집)
 * - church: 교회 (백로그 - 치유/부활)
 */

type ViewType = 'exploration' | 'battle' | 'campfire' | 'mercenary' | 'church';

interface CenterContentProps {
  currentView: ViewType;
  onViewChange: (view: ViewType) => void;
  soulStone: number;
  setSoulStone: (value: number | ((prev: number) => number)) => void;
  party: (AllyData | null)[];
  setParty: (value: (AllyData | null)[] | ((prev: (AllyData | null)[]) => (AllyData | null)[])) => void;
  reserves: AllyData[];
  setReserves: (value: AllyData[] | ((prev: AllyData[]) => AllyData[])) => void;
}

export function CenterContent({ 
  currentView, 
  onViewChange,
  soulStone,
  setSoulStone,
  party,
  setParty,
  reserves,
  setReserves
}: CenterContentProps) {
  return (
    <div className="size-full flex flex-col">
      {/* 
        상단 네비게이션 바
        Unity 좌표: X: 320px, Y: 0, Width: 1600px, Height: 64px
      */}
      

      {/* 
        콘텐츠 영역
        Unity 좌표: X: 320px, Y: 64px, Width: 1600px, Height: 1016px
      */}
      <div className="flex-1 overflow-auto">
        {currentView === 'exploration' && (
          <ExplorePanel
            onBattleStart={() => onViewChange('battle')}
            onMercenaryOffice={() => onViewChange('mercenary')}
            onCampfire={() => onViewChange('campfire')}
          />
        )}
        {currentView === 'battle' && (
          <BattlePanel
            party={party}
            onBattleEnd={() => onViewChange('exploration')}
          />
        )}
        {currentView === 'campfire' && (
          <CampfireView
            party={party}
            setParty={setParty}
            onClose={() => onViewChange('exploration')}
          />
        )}
        {currentView === 'mercenary' && (
          <MercenaryOfficePanel
            soulStone={soulStone}
            setSoulStone={setSoulStone}
            party={party}
            setParty={setParty}
            reserves={reserves}
            setReserves={setReserves}
            onClose={() => onViewChange('exploration')}
          />
        )}
        {currentView === 'church' && <ChurchView onClose={() => onViewChange('exploration')} />}
      </div>
    </div>
  );
}

function NavButton({ 
  icon, 
  label, 
  active, 
  onClick 
}: { 
  icon: React.ReactNode; 
  label: string; 
  active: boolean; 
  onClick: () => void;
}) {
  return (
    <Button
      variant={active ? 'default' : 'ghost'}
      className="gap-2"
      onClick={onClick}
      style={active ? {
        backgroundColor: '#C39A52',
        color: '#0F1115',
        borderColor: '#A1783A'
      } : {
        color: '#B5ADA0'
      }}
    >
      {icon}
      {label}
    </Button>
  );
}

// 화툿불 뷰 컴포넌트 (기획서 기준: 체력/스트레스 회복 노드)
function CampfireView({
  party,
  setParty,
  onClose
}: {
  party: (AllyData | null)[];
  setParty: (value: (AllyData | null)[] | ((prev: (AllyData | null)[]) => (AllyData | null)[])) => void;
  onClose: () => void;
}) {
  const handleRest = () => {
    // 기획서 기준: 화툿불 회복량 -15 스트레스 (임시값)
    setParty(prev => prev.map(ally => {
      if (!ally) return null;
      return {
        ...ally,
        stress: Math.max(0, ally.stress - 15),
        hp: Math.min(ally.maxHp, ally.hp + 30) // 체력도 약간 회복
      };
    }));
  };

  return (
    <div className="p-8 h-full flex items-center justify-center">
      <div
        className="max-w-2xl w-full p-8 rounded-lg border-2"
        style={{
          backgroundColor: '#1B1F27',
          borderColor: '#C97A2B'
        }}
      >
        <div className="text-center mb-8">
          <Flame className="w-24 h-24 mx-auto mb-4" style={{ color: '#C97A2B' }} />
          <h2 className="text-3xl font-bold mb-2" style={{ color: '#E6E0D6' }}>
            화툿불
          </h2>
          <p style={{ color: '#B5ADA0' }}>
            짧은 휴식을 취하며 체력과 스트레스를 회복합니다
          </p>
        </div>

        <div className="space-y-4 mb-8">
          <div className="p-4 rounded border" style={{ backgroundColor: '#202631', borderColor: '#3A4452' }}>
            <p className="text-sm mb-2" style={{ color: '#B5ADA0' }}>회복 효과</p>
            <ul className="text-sm space-y-1" style={{ color: '#E6E0D6' }}>
              <li>• 스트레스 -15</li>
              <li>• 체력 +30</li>
            </ul>
          </div>
        </div>

        <div className="flex gap-4">
          <Button
            className="flex-1"
            onClick={() => {
              handleRest();
              onClose();
            }}
            style={{
              backgroundColor: '#C97A2B',
              color: '#0F1115',
              borderColor: '#C97A2B'
            }}
          >
            휴식하기
          </Button>
          <Button
            variant="outline"
            onClick={onClose}
            style={{
              borderColor: '#3A4452',
              color: '#B5ADA0'
            }}
          >
            떠나기
          </Button>
        </div>
      </div>
    </div>
  );
}

// 교회 뷰 컴포넌트 (백로그)
function ChurchView({ onClose }: { onClose: () => void }) {
  return (
    <div className="p-8 h-full flex items-center justify-center">
      <div className="text-center">
        <Church className="w-24 h-24 mx-auto mb-4" style={{ color: '#7B5EA7' }} />
        <h2 className="text-3xl font-bold mb-2" style={{ color: '#E6E0D6' }}>
          교회
        </h2>
        <p className="mb-8" style={{ color: '#B5ADA0' }}>
          스트레스 관리 및 부활 화면 (백로그 예정)
        </p>
        <Button onClick={onClose} style={{ backgroundColor: '#7B5EA7', color: '#E6E0D6' }}>
          돌아가기
        </Button>
      </div>
    </div>
  );
}
