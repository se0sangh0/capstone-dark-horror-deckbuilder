import { useState, useEffect } from 'react';
import { BattleField } from './BattleField';
import { HandCards } from './HandCards';
import { BattleCardData } from './BattleCard';
import { CharacterData } from './BattleCharacter';
import { Button } from '../ui/button';
import { Swords, SkipForward, LogOut } from 'lucide-react';
import { AllyData } from './RecruitPopup';

/**
 * 전투 패널 - 메인 전투 화면
 * Guidelines에 따른 전투 UI 구조
 * Unity 좌표: X: 320px, Y: 0, Width: 1600px, Height: 1080px
 */

interface BattlePanelProps {
  party: (AllyData | null)[];
  onBattleEnd?: () => void;
}

// AllyData를 CharacterData로 변환
function allyToCharacter(ally: AllyData, index: number): CharacterData {
  // 0,1번은 전열, 2,3번은 후열
  const position = index < 2 ? 'front' : 'back';
  
  return {
    id: ally.id,
    name: ally.name,
    jobClass: ally.jobClass,
    displayJobName: ally.displayJobName,
    role: ally.role,
    hp: ally.hp,
    maxHp: ally.maxHp,
    stress: ally.stress,
    maxStress: ally.maxStress,
    position,
    isEnemy: false
  };
}

// 기획서 기준: 파티원 1명당 10장씩 덱 생성 (4인 = 40장)
function generateDeck(party: (AllyData | null)[]): BattleCardData[] {
  const deck: BattleCardData[] = [];

  party.forEach((ally) => {
    if (ally) {
      // 각 파티원당 10장의 카드 생성
      for (let i = 0; i < 10; i++) {
        deck.push({
          id: `card-${ally.id}-${i}-${Date.now()}-${Math.random()}`,
          role: ally.role,
          tendency: ally.tendency,
          stackValue: 0 // 사용 시점에 성향에 따라 결정됨
        });
      }
    }
  });

  // 덱 셔플
  return shuffleDeck(deck);
}

// 덱 셔플 (Fisher-Yates)
function shuffleDeck(deck: BattleCardData[]): BattleCardData[] {
  const shuffled = [...deck];
  for (let i = shuffled.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [shuffled[i], shuffled[j]] = [shuffled[j], shuffled[i]];
  }
  return shuffled;
}

// 덱에서 N장 드로우
function drawCards(deck: BattleCardData[], count: number): { drawn: BattleCardData[], remainingDeck: BattleCardData[] } {
  const drawn = deck.slice(0, count);
  const remainingDeck = deck.slice(count);
  return { drawn, remainingDeck };
}

// 기획서 기준: 직업별 스킬 코스트 합계 (선공 판정용)
function getJobClassSkillCostSum(jobClass: string): number {
  // 기획서 10_동료_스킬_데이터 기준
  const skillCosts: Record<string, number> = {
    'caster': 1 + 3,      // 매직 미사일(1) + 파이어볼(3) = 4
    'offender': 2 + 5,    // 발도(2) + 일섬(5) = 7
    'defender': 2 + 5,    // 방어 준비(2) + 전투 태세(5) = 7
    'attacker': 1 + 4,    // 무모한 강타(1) + 불굴(4) = 5
    'priest': 3 + 5,      // 별부름(3) + 기원(5) = 8
    'shaman': 0           // 샤먼은 추후 구현 예정
  };

  return skillCosts[jobClass.toLowerCase()] || 0;
}

// 기획서 기준: 성향별 스택 값 생성 (0 제외 - 스택 유지 카드 없음)
function generateStackValue(tendency: 'gambler' | 'safety' | 'opportunist' | 'optimist'): number {
  switch (tendency) {
    case 'gambler': // -5 또는 +5 (50:50)
      return Math.random() < 0.5 ? -5 : 5;

    case 'safety': { // -1 ~ +3, 0 제외 → [-1, 1, 2, 3]
      const values = [-1, 1, 2, 3];
      return values[Math.floor(Math.random() * values.length)];
    }

    case 'opportunist': { // -3 ~ +4, 0 제외 → [-3, -2, -1, 1, 2, 3, 4]
      const values = [-3, -2, -1, 1, 2, 3, 4];
      return values[Math.floor(Math.random() * values.length)];
    }

    case 'optimist': { // -5 ~ +5, 0 제외 → [-5, -4, -3, -2, -1, 1, 2, 3, 4, 5]
      const values = [-5, -4, -3, -2, -1, 1, 2, 3, 4, 5];
      return values[Math.floor(Math.random() * values.length)];
    }

    default:
      return 1;
  }
}

// 기획서 기준: 턴 페이즈 (선공 판정은 전투 시작 시 1회만)
type TurnPhase = 'draw' | 'card_play' | 'ally_action' | 'enemy_action' | 'result';

export function BattlePanel({ party, onBattleEnd }: BattlePanelProps) {
  const [currentTurn, setCurrentTurn] = useState(1);
  const [turnPhase, setTurnPhase] = useState<TurnPhase>('card_play'); // 시작은 카드 플레이 단계

  // 공용 스택 (역할별)
  const [currentStack, setCurrentStack] = useState({
    dealer: 0,
    tank: 0,
    supporter: 0
  });

  // 이번 턴에 사용한 카드들의 스택 합계 (선공 판정용)
  const [turnStackSum, setTurnStackSum] = useState(0);

  // 기획서 기준: 덱 40장 (파티원 4명 × 10장/명) + 초기 손패 드로우
  const [deck, setDeck] = useState<BattleCardData[]>(() => {
    const newDeck = generateDeck(party);
    const allyCount = party.filter(ally => ally !== null).length;
    const { remainingDeck } = drawCards(newDeck, allyCount);
    return remainingDeck;
  });

  // 기획서 기준: 손패는 배치 동료 수 (4장) - 초기값 바로 설정
  const [handCards, setHandCards] = useState<BattleCardData[]>(() => {
    const newDeck = generateDeck(party);
    const allyCount = party.filter(ally => ally !== null).length;
    const { drawn } = drawCards(newDeck, allyCount);
    console.log(`초기 손패 드로우: ${drawn.length}장`);
    return drawn;
  });

  // 선공 판정 결과
  const [allyFirst, setAllyFirst] = useState<boolean | null>(null);

  // 파티 변경 시 덱/손패 재생성
  useEffect(() => {
    const newDeck = generateDeck(party);
    const allyCount = party.filter(ally => ally !== null).length;
    const { drawn, remainingDeck } = drawCards(newDeck, allyCount);

    setDeck(remainingDeck);
    setHandCards(drawn);
    console.log(`파티 변경: 손패 ${drawn.length}장, 덱 ${remainingDeck.length}장`);
  }, [party]);

  // 파티를 CharacterData로 변환
  const allies: CharacterData[] = party
    .map((ally, index) => ally ? allyToCharacter(ally, index) : null)
    .filter((char): char is CharacterData => char !== null);

  const enemies: CharacterData[] = [
    {
      id: 'enemy-1',
      name: '어둠의 정찰병',
      hp: 60,
      maxHp: 80,
      isEnemy: true,
      position: 'front'
    },
    {
      id: 'enemy-2',
      name: '그림자 궁수',
      hp: 45,
      maxHp: 60,
      isEnemy: true,
      position: 'back'
    },
    {
      id: 'enemy-3',
      name: '타락한 전사',
      hp: 100,
      maxHp: 100,
      isEnemy: true,
      position: 'front'
    }
  ];

  const handleCardSelect = (card: BattleCardData) => {
    // 기획서 기준: 카드는 카드 플레이 단계에만 사용 가능
    if (turnPhase !== 'card_play') return;

    // 기획서 기준: 카드 사용 시점에 성향에 따라 스택 값 생성
    const stackValue = generateStackValue(card.tendency);
    console.log(`카드 사용: ${card.role} / 성향: ${card.tendency} / 스택: ${stackValue > 0 ? '+' : ''}${stackValue}`);

    // 스택 업데이트
    setCurrentStack(prev => ({
      ...prev,
      [card.role]: Math.max(0, prev[card.role] + stackValue)
    }));

    // 이번 턴 스택 합계 누적
    setTurnStackSum(prev => prev + stackValue);

    // 손패에서 카드 제거
    setHandCards(prev => prev.filter(c => c.id !== card.id));
  };

  const handleCharacterClick = (character: CharacterData) => {
    console.log('클릭된 캐릭터:', character);
  };

  // 기획서 기준: 선공 판정 (스킬 코스트 기반)
  const performInitiativeCheck = () => {
    // 아군 점수 = Σ(각 동료의 스킬 코스트 합)
    const allyScore = party
      .filter(ally => ally !== null)
      .reduce((sum, ally) => {
        if (!ally) return sum;
        const skillCost = getJobClassSkillCostSum(ally.jobClass);
        return sum + skillCost;
      }, 0);

    // 적 점수 = 적의 기본 고정 스택 (일반: 3, 보스: 8)
    const enemyScore = 3; // TODO: 보스 판정 추가

    console.log(`선공 판정 (스킬 코스트 기반):`);
    console.log(`  아군 스킬 코스트 합: ${allyScore}`);
    party.filter(ally => ally !== null).forEach(ally => {
      if (ally) {
        const cost = getJobClassSkillCostSum(ally.jobClass);
        console.log(`    ${ally.name} (${ally.displayJobName}): ${cost}`);
      }
    });
    console.log(`  적 고정 스택: ${enemyScore}`);

    if (allyScore > enemyScore) {
      setAllyFirst(true);
      console.log('→ 아군 선공');
    } else if (allyScore < enemyScore) {
      setAllyFirst(false);
      console.log('→ 적 선공');
    } else {
      // 동점: 랜덤 (코인 토스)
      const coinFlip = Math.random() < 0.5;
      setAllyFirst(coinFlip);
      console.log(`→ 동점! 코인 토스 결과: ${coinFlip ? '아군' : '적'} 선공`);
    }
  };

  // 기획서 기준: 선공 판정은 전투 시작 시 1회만 실행
  useEffect(() => {
    performInitiativeCheck();
  }, []); // 컴포넌트 마운트 시 1회만 실행

  const handleEndTurn = () => {
    console.log('턴 종료 버튼 클릭');

    // 기획서 기준: 턴 구조 진행 (선공 판정은 전투 시작 시 1회만)
    if (turnPhase === 'card_play') {
      // 1. 카드 플레이 단계 종료 → 전투 페이즈로 이동
      // 선공 판정 결과에 따라 아군/적 행동 순서 결정
      setTurnPhase(allyFirst ? 'ally_action' : 'enemy_action');
    } else if (turnPhase === 'ally_action' || turnPhase === 'enemy_action') {
      // 2. 전투 종료 → 결과 처리 → 다음 턴 시작
      setTurnPhase('result');

      // 다음 턴 준비
      setTimeout(() => {
        // 기획서 기준: 드로우 페이즈 - 사용한 손패만큼 재드로우
        const allyCount = party.filter(ally => ally !== null).length;
        const usedCardCount = allyCount - handCards.length;

        if (usedCardCount > 0 && deck.length > 0) {
          const drawCount = Math.min(usedCardCount, deck.length);
          const { drawn, remainingDeck } = drawCards(deck, drawCount);

          setHandCards(prev => [...prev, ...drawn]);
          setDeck(remainingDeck);

          console.log(`드로우 페이즈: ${drawCount}장 드로우, 남은 덱: ${remainingDeck.length}장`);
        } else if (deck.length === 0) {
          console.log('덱 고갈 - 탈진 상태!');
        }

        // 턴 초기화 (선공 판정 결과는 유지)
        setCurrentTurn(prev => prev + 1);
        setTurnStackSum(0);
        setTurnPhase('card_play');
      }, 1000);
    }
  };

  return (
    <div className="w-full h-full flex flex-col relative">
      {/* 
        상단 전투 정보 바
        Unity 좌표: X: 320px, Y: 0, Width: 1600px, Height: 64px
      */}
      <div
        className="px-6 py-3 border-b flex items-center justify-between"
        style={{
          backgroundColor: '#1B1F27',
          borderColor: '#3A4452',
          height: '64px'
        }}
      >
        <div className="flex items-center gap-4">
          <Swords className="w-5 h-5" style={{ color: '#B3263E' }} />
          <div>
            <h2 className="text-lg font-bold" style={{ color: '#C39A52' }}>
              전투 진행 중
            </h2>
            <p className="text-xs" style={{ color: '#B5ADA0' }}>
              심연의 탐사 - 1층
            </p>
          </div>
        </div>

        <div className="flex items-center gap-6">
          {/* 턴 페이즈 표시 (기획서 기준) */}
          <div className="text-center">
            <div className="text-xs" style={{ color: '#B5ADA0' }}>페이즈</div>
            <div
              className="text-sm font-bold"
              style={{
                color: turnPhase === 'card_play' ? '#C39A52' : '#4CB3B3'
              }}
            >
              {turnPhase === 'card_play' && '카드 플레이'}
              {turnPhase === 'ally_action' && '아군 행동'}
              {turnPhase === 'enemy_action' && '적 행동'}
              {turnPhase === 'result' && '결과 처리'}
            </div>
          </div>

          {/* 선공 판정 결과 */}
          {allyFirst !== null && (
            <div className="text-center">
              <div className="text-xs" style={{ color: '#B5ADA0' }}>선공</div>
              <div
                className="text-sm font-bold"
                style={{
                  color: allyFirst ? '#4F8F63' : '#B3263E'
                }}
              >
                {allyFirst ? '아군' : '적'}
              </div>
            </div>
          )}

          {/* 덱 상태 표시 (기획서 기준: 40장 덱) */}
          <div className="text-center">
            <div className="text-xs" style={{ color: '#B5ADA0' }}>남은 덱</div>
            <div
              className="text-xl font-bold"
              style={{
                color: deck.length > 10 ? '#4F8F63' : deck.length > 0 ? '#C97A2B' : '#B3263E',
                textShadow: deck.length > 0 ? '0 0 10px currentColor50' : 'none'
              }}
            >
              {deck.length}장
            </div>
          </div>

          {/* 턴 표시 */}
          <div className="text-center">
            <div className="text-xs" style={{ color: '#B5ADA0' }}>현재 턴</div>
            <div
              className="text-xl font-bold"
              style={{
                color: '#4CB3B3',
                textShadow: '0 0 10px #4CB3B380'
              }}
            >
              {currentTurn}
            </div>
          </div>

          {/* 턴 종료 버튼 (페이즈별 텍스트) */}
          <Button
            onClick={handleEndTurn}
            className="gap-2"
            disabled={turnPhase === 'result'}
            style={{
              backgroundColor: turnPhase === 'card_play' ? '#C39A52' : '#4CB3B3',
              color: '#0F1115',
              borderColor: '#A1783A',
              opacity: turnPhase === 'result' ? 0.5 : 1
            }}
          >
            <SkipForward className="w-4 h-4" />
            {turnPhase === 'card_play' && '카드 플레이 종료'}
            {(turnPhase === 'ally_action' || turnPhase === 'enemy_action') && '전투 종료'}
            {turnPhase === 'result' && '결과 처리 중...'}
          </Button>

          {/* 전투 종료 버튼 (임시 - 테스트용) */}
          {onBattleEnd && (
            <Button
              onClick={onBattleEnd}
              variant="ghost"
              className="gap-2"
              style={{
                color: '#B5ADA0',
                borderColor: '#3A4452'
              }}
            >
              <LogOut className="w-4 h-4" />
              전투 종료
            </Button>
          )}
        </div>
      </div>

      {/* 
        전투 필드 영역
        Unity 좌표: X: 320px, Y: 64px, Width: 1600px, Height: 816px
      */}
      <div className="flex-1 relative">
        <BattleField
          allies={allies}
          enemies={enemies}
          onCharacterClick={handleCharacterClick}
          currentStack={currentStack}
          turnStackSum={turnStackSum}
        />
      </div>

      {/* 
        손패 영역
        Unity 좌표: X: 320px, Y: 880px, Width: 1600px, Height: 200px
      */}
      <HandCards cards={handCards} onCardSelect={handleCardSelect} />

      {/* 공용 스택 정보 (향후 확장) */}
      
    </div>
  );
}
