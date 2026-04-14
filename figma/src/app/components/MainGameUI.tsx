import { useState } from 'react';
import { PersistentUI } from './game/PersistentUI';
import { CenterContent } from './game/CenterContent';
import { AllyData } from './game/RecruitPopup';
import { generateAllyName, generateAllyId, getRandomGender } from '../utils/allyNameGenerator';
import { JOB_CLASS_DATA, getRandomJobClass } from '../utils/jobClassData';

type ViewType = 'exploration' | 'battle' | 'campfire' | 'mercenary' | 'church';

export function MainGameUI() {
  // 초기 파티 (예시)
  const initialParty: (AllyData | null)[] = [
    {
      id: generateAllyId('caster'),
      name: generateAllyName('male'),
      role: 'dealer',
      jobClass: 'caster',
      displayJobName: '캐스터',
      tendency: 'gambler',
      star: 1,
      maxHp: 100,
      hp: 100,
      stress: 0,
      maxStress: 100
    },
    {
      id: generateAllyId('defender'),
      name: generateAllyName('female'),
      role: 'tank',
      jobClass: 'defender',
      displayJobName: '디펜더',
      tendency: 'safety',
      star: 1,
      maxHp: 200,
      hp: 200,
      stress: 0,
      maxStress: 100
    },
    null,
    null
  ];
  
  const [party, setParty] = useState<(AllyData | null)[]>(initialParty);
  const [reserves, setReserves] = useState<AllyData[]>([]); // 예비대
  const [currentView, setCurrentView] = useState<ViewType>('exploration');
  const [soulStone, setSoulStone] = useState(100);
  const [manaStone, setManaStone] = useState(50);

  return (
    // 전체 화면 배경 - 다크 호러 느낌
    <div 
      className="size-full flex relative overflow-hidden"
      style={{ background: 'linear-gradient(to bottom right, #0F1115, #151821)' }}
    >
      {/* 배경 잔광 효과 */}
      <div className="absolute inset-0 pointer-events-none">
        {/* 청록색 유물 에너지 */}
        <div 
          className="absolute top-1/4 left-1/4 w-96 h-96 rounded-full mix-blend-screen filter blur-3xl opacity-5 animate-pulse" 
          style={{ backgroundColor: '#2E8B8B' }}
        />
        {/* 황동 반사광 */}
        <div 
          className="absolute bottom-1/3 right-1/3 w-80 h-80 rounded-full mix-blend-screen filter blur-3xl opacity-5 animate-pulse" 
          style={{ backgroundColor: '#A1783A', animationDelay: '1.5s' }}
        />
      </div>

      {/* 
        고정 UI 영역 (왼쪽)
        Unity 좌표: X: 0, Y: 0, Width: 320px, Height: 1080px
      */}
      <PersistentUI 
        soulStone={soulStone}
        manaStone={manaStone}
        party={party}
      />

      {/* 
        중앙 콘텐츠 영역
        Unity 좌표: X: 320px, Y: 0, Width: 1600px, Height: 1080px
      */}
      <div className="flex-1 relative">
        <CenterContent 
          currentView={currentView}
          onViewChange={setCurrentView}
          soulStone={soulStone}
          setSoulStone={setSoulStone}
          party={party}
          setParty={setParty}
          reserves={reserves}
          setReserves={setReserves}
        />
      </div>
    </div>
  );
}
