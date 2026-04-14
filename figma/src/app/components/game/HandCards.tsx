import { BattleCard, BattleCardData } from './BattleCard';

/**
 * 손패 영역 - 가로 일렬 배치
 * Unity 좌표: X: 320px, Y: 880px, Width: 1600px, Height: 200px
 */

interface HandCardsProps {
  cards: BattleCardData[];
  onCardSelect?: (card: BattleCardData) => void;
}

export function HandCards({ cards, onCardSelect }: HandCardsProps) {
  const handleCardClick = (card: BattleCardData) => {
    onCardSelect?.(card);
  };

  return (
    <div
      className="absolute bottom-0 left-0 right-0 flex items-end justify-center pointer-events-none"
      style={{
        height: '200px'
      }}
    >
      <div
        className="absolute inset-0"
        style={{
          background: 'linear-gradient(to top, rgba(15, 17, 21, 0.95) 0%, transparent 100%)',
          pointerEvents: 'none'
        }}
      />

      <div
        className="relative flex items-center justify-center gap-3 pointer-events-auto pb-2"
        style={{
          width: '100%',
          height: '100%',
        }}
      >
        {cards.map((card) => (
          <div
            key={card.id}
            style={{
              transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
            }}
          >
            <BattleCard
              card={card}
              onClick={() => handleCardClick(card)}
            />
          </div>
        ))}
      </div>

      <div
        className="absolute left-1/2 bottom-2 -translate-x-1/2 px-4 py-1 rounded-full text-xs font-bold pointer-events-none"
        style={{
          backgroundColor: 'rgba(26, 29, 39, 0.9)',
          border: '1px solid #3A4452',
          color: '#B5ADA0'
        }}
      >
        손패: {cards.length}장
      </div>
    </div>
  );
}
