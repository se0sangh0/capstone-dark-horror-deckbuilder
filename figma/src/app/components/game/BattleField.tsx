import { BattleCharacter, CharacterData } from './BattleCharacter';

/**
 * 전투 필드 - 중앙 전투 메인 영역
 * 아군은 좌측, 적군은 우측에 배치 (1행)
 * Unity 좌표: X: 320px, Y: 64px, Width: 1600px, Height: 816px (880 - 64)
 */

// 기획서 기준: 직업별 스킬 코스트 합계
function getJobClassSkillCostSum(jobClass?: string): number {
  if (!jobClass) return 0;

  const skillCosts: Record<string, number> = {
    'caster': 4,      // 1 + 3
    'offender': 7,    // 2 + 5
    'defender': 7,    // 2 + 5
    'attacker': 5,    // 1 + 4
    'priest': 8,      // 3 + 5
    'shaman': 0       // 추후 구현
  };

  return skillCosts[jobClass.toLowerCase()] || 0;
}

interface BattleFieldProps {
  allies: CharacterData[];
  enemies: CharacterData[];
  onCharacterClick?: (character: CharacterData) => void;
  currentStack?: {
    dealer: number;
    tank: number;
    supporter: number;
  };
  turnStackSum?: number; // 이번 턴 스택 합계
}

export function BattleField({
  allies,
  enemies,
  onCharacterClick,
  currentStack = { dealer: 5, tank: 8, supporter: 3 },
  turnStackSum = 0
}: BattleFieldProps) {
  // 안전한 스택 값 보장
  const safeStack = {
    dealer: currentStack?.dealer ?? 5,
    tank: currentStack?.tank ?? 8,
    supporter: currentStack?.supporter ?? 3
  };

  // 기획서 기준: 아군 스킬 코스트 합 (선공 판정용)
  const allySkillCostSum = allies.reduce((sum, ally) => {
    const cost = getJobClassSkillCostSum(ally.jobClass);
    return sum + cost;
  }, 0);

  return (
    <div
      className="relative w-full h-full"
      style={{
        background: 'linear-gradient(to bottom, rgba(27, 31, 39, 0.3), rgba(15, 17, 21, 0.5))',
        /* Unity 좌표:
         * X: 320px (좌측 패널 이후)
         * Y: 64px (상단 바 이후)
         * Width: 1600px
         * Height: 816px (880 - 64, 손패 영역 제외)
         */
      }}
    >
      {/* 배경 전투 효과선 */}
      <div className="absolute inset-0 pointer-events-none overflow-hidden">
        {/* 중앙 VS 라인 */}
        <div
          className="absolute left-1/2 top-0 bottom-0 w-px opacity-20"
          style={{
            background: 'linear-gradient(to bottom, transparent, #C39A52, transparent)',
            boxShadow: '0 0 20px #C39A52'
          }}
        />

        {/* 전투 구역 표시선 */}
        <div
          className="absolute left-1/4 top-1/4 bottom-1/4 w-px opacity-10"
          style={{ backgroundColor: '#4CB3B3' }}
        />
        <div
          className="absolute right-1/4 top-1/4 bottom-1/4 w-px opacity-10"
          style={{ backgroundColor: '#B3263E' }}
        />
      </div>

      {/* VS 중앙 표시 */}
      <div className="absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 pointer-events-none">
        <div
          className="px-6 py-3 rounded-lg font-bold text-2xl"
          style={{
            backgroundColor: 'rgba(15, 17, 21, 0.8)',
            border: '2px solid #C39A52',
            color: '#C39A52',
            textShadow: '0 0 10px #C39A5280',
            boxShadow: '0 0 30px rgba(195, 154, 82, 0.3)'
          }}
        >
          VS
        </div>
      </div>

      {/* 현재 스택 표시 - 좌측 상단 */}
      <div
        className="absolute top-4 left-4 rounded-lg p-4"
        style={{
          backgroundColor: 'rgba(27, 31, 39, 0.95)',
          border: '3px solid #C39A52',
          boxShadow: '0 8px 32px rgba(195, 154, 82, 0.4)',
          minWidth: '360px',
          /* Unity 좌표:
           * X: 336px (320 + 16)
           * Y: 80px (64 + 16)
           * Width: 360px (min)
           * Height: auto
           */
        }}
      >
        <div className="space-y-2.5">
          <div
            className="text-xs font-bold tracking-wider text-center pb-2 border-b"
            style={{
              color: '#C39A52',
              letterSpacing: '0.1em',
              borderColor: '#3A4452'
            }}
          >
            현재 스택
          </div>
          
          {/* 역할별 스택 - 가로 배치 */}
          <div className="flex gap-3">
            {/* 딜러 스택 */}
            <div className="flex-1">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-1.5">
                  <div
                    className="w-5 h-5 rounded-full flex items-center justify-center text-xs font-bold"
                    style={{
                      backgroundColor: '#B3263E',
                      color: '#E6E0D6'
                    }}
                  >
                    딜
                  </div>
                  <span className="text-xs font-bold" style={{ color: '#B5ADA0' }}>딜러</span>
                </div>
                <div
                  className="text-2xl font-bold"
                  style={{
                    color: '#B3263E',
                    textShadow: '0 0 15px rgba(179, 38, 62, 0.8)',
                    fontFamily: 'monospace'
                  }}
                >
                  {safeStack.dealer}
                </div>
              </div>
            </div>

            {/* 탱커 스택 */}
            <div className="flex-1">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-1.5">
                  <div
                    className="w-5 h-5 rounded-full flex items-center justify-center text-xs font-bold"
                    style={{
                      backgroundColor: '#4C566A',
                      color: '#E6E0D6'
                    }}
                  >
                    탱
                  </div>
                  <span className="text-xs font-bold" style={{ color: '#B5ADA0' }}>탱커</span>
                </div>
                <div
                  className="text-2xl font-bold"
                  style={{
                    color: '#4C566A',
                    textShadow: '0 0 15px rgba(76, 86, 106, 0.8)',
                    fontFamily: 'monospace'
                  }}
                >
                  {safeStack.tank}
                </div>
              </div>
            </div>

            {/* 서포터 스택 */}
            <div className="flex-1">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-1.5">
                  <div
                    className="w-5 h-5 rounded-full flex items-center justify-center text-xs font-bold"
                    style={{
                      backgroundColor: '#4CB3B3',
                      color: '#E6E0D6'
                    }}
                  >
                    힐
                  </div>
                  <span className="text-xs font-bold" style={{ color: '#B5ADA0' }}>힐러</span>
                </div>
                <div
                  className="text-2xl font-bold"
                  style={{
                    color: '#4CB3B3',
                    textShadow: '0 0 15px rgba(76, 179, 179, 0.8)',
                    fontFamily: 'monospace'
                  }}
                >
                  {safeStack.supporter}
                </div>
              </div>
            </div>
          </div>

          {/* 기획서 기준: 선공 판정 정보 (스킬 코스트 기반) */}
          <div
            className="pt-2.5 mt-1 border-t"
            style={{ borderColor: '#3A4452' }}
          >
            <div className="flex items-center justify-between text-xs">
              <span style={{ color: '#B5ADA0' }}>아군 스킬 코스트</span>
              <span
                className="font-bold text-xl"
                style={{
                  color: '#C39A52',
                  textShadow: '0 0 10px rgba(195, 154, 82, 0.6)',
                  fontFamily: 'monospace'
                }}
              >
                {allySkillCostSum}
              </span>
            </div>
            <div className="flex items-center justify-between text-xs mt-1.5">
              <span style={{ color: '#B5ADA0' }}>적 고정 스택</span>
              <span
                className="font-bold text-xl"
                style={{
                  color: '#B3263E',
                  textShadow: '0 0 10px rgba(179, 38, 62, 0.6)',
                  fontFamily: 'monospace'
                }}
              >
                3
              </span>
            </div>
            <div className="text-center text-xs mt-2 pt-2 border-t" style={{ borderColor: '#3A4452', color: '#8A8F98' }}>
              선공 판정: {allySkillCostSum > 3 ? '아군 우세' : allySkillCostSum < 3 ? '적 우세' : '동점'}
            </div>
          </div>
        </div>
      </div>

      {/* 아군 영역 (좌측) */}
      <div
        className="absolute left-0 top-0 bottom-0 flex items-center justify-center"
        style={{
          width: '50%',
          /* Unity 좌표:
           * 아군 영역: X: 320px, Y: 64px, Width: 800px, Height: 816px
           */
        }}
      >
        <div className="w-full px-8 flex flex-col gap-6 items-center">
          {/* 아군 라벨 */}
          <div
            className="px-4 py-2 rounded-lg font-bold"
            style={{
              backgroundColor: 'rgba(76, 179, 179, 0.15)',
              border: '1px solid #4CB3B3',
              color: '#4CB3B3',
            }}
          >
            아군 파티
          </div>

          {/* 아군 캐릭터들 - 1행 배치 */}
          <div className="flex gap-6 justify-center">
            {allies.map(character => (
              <BattleCharacter
                key={character.id}
                character={character}
                onClick={() => onCharacterClick?.(character)}
              />
            ))}
          </div>
        </div>
      </div>

      {/* 적군 영역 (우측) */}
      <div
        className="absolute right-0 top-0 bottom-0 flex items-center justify-center"
        style={{
          width: '50%',
          /* Unity 좌표:
           * 적군 영역: X: 1120px, Y: 64px, Width: 800px, Height: 816px
           */
        }}
      >
        <div className="w-full px-8 flex flex-col gap-6 items-center">
          {/* 적군 라벨 */}
          <div
            className="px-4 py-2 rounded-lg font-bold"
            style={{
              backgroundColor: 'rgba(179, 38, 62, 0.15)',
              border: '1px solid #B3263E',
              color: '#B3263E',
            }}
          >
            적 세력
          </div>

          {/* 적군 캐릭터들 - 1행 배치 */}
          <div className="flex gap-6 justify-center">
            {enemies.map(character => (
              <BattleCharacter
                key={character.id}
                character={character}
                onClick={() => onCharacterClick?.(character)}
              />
            ))}
          </div>
        </div>
      </div>

      {/* 전투 로그 영역 (우측 상단) */}
      <div
        className="absolute top-4 right-4 w-64 max-h-32 overflow-y-auto rounded-lg p-3"
        style={{
          backgroundColor: 'rgba(27, 31, 39, 0.9)',
          border: '1px solid #3A4452',
          /* Unity 좌표:
           * X: 1640px (1920 - 280)
           * Y: 68px
           * Width: 256px
           * Height: 128px (max)
           */
        }}
      >
        <div className="text-xs space-y-1">
          <div style={{ color: '#C39A52' }}>전투 시작!</div>
          <div style={{ color: '#8A8F98' }}>• 턴 1 시작</div>
        </div>
      </div>
    </div>
  );
}
