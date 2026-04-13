import { useState } from 'react';
import { Settings, Play, Sparkles } from 'lucide-react';
import { Button } from './components/ui/button';
import { SettingsDialog } from './components/SettingsDialog';
import { MainGameUI } from './components/MainGameUI';

export default function App() {
  const [isSettingsOpen, setIsSettingsOpen] = useState(false);
  const [gameStarted, setGameStarted] = useState(false);

  const handleStartGame = () => {
    setGameStarted(true);
  };

  // 게임이 시작되면 메인 UI 표시
  if (gameStarted) {
    return <MainGameUI />;
  }

  return (
    // 전체 화면 배경 (1920x1080 기준) - 어두운 심연 느낌
    <div className="size-full flex items-center justify-center relative overflow-hidden" style={{ background: 'linear-gradient(to bottom right, #0F1115, #151821, #1B1F27)' }}>
      {/* 배경 애니메이션 효과 - 유물/에너지 잔광 */}
      <div className="absolute inset-0">
        {/* 청록색 유물 에너지 */}
        <div className="absolute top-20 left-20 w-96 h-96 rounded-full mix-blend-screen filter blur-3xl opacity-10 animate-pulse" style={{ backgroundColor: '#2E8B8B' }}></div>
        {/* 황동 금속 반사광 */}
        <div className="absolute top-40 right-20 w-80 h-80 rounded-full mix-blend-screen filter blur-3xl opacity-10 animate-pulse" style={{ backgroundColor: '#A1783A', animationDelay: '1s' }}></div>
        {/* 위험 신호 붉은빛 */}
        <div className="absolute bottom-20 left-1/2 w-72 h-72 rounded-full mix-blend-screen filter blur-3xl opacity-10 animate-pulse" style={{ backgroundColor: '#B3263E', animationDelay: '2s' }}></div>
        {/* 룬 보라빛 흔적 */}
        <div className="absolute top-1/2 left-1/3 w-64 h-64 rounded-full mix-blend-screen filter blur-3xl opacity-8 animate-pulse" style={{ backgroundColor: '#5A3D7A', animationDelay: '1.5s' }}></div>
      </div>

      {/* 
        설정 버튼
        Unity 좌표 참고: 우측 상단 (X: screen.width - 64px, Y: 32px)
        크기: 48x48px
      */}
      <Button
        variant="ghost"
        size="icon"
        className="absolute top-8 right-8 hover:bg-white/10 transition-all"
        style={{ color: '#B5ADA0' }}
        onClick={() => setIsSettingsOpen(true)}
      >
        <Settings className="w-6 h-6" />
      </Button>

      {/* 
        메인 컨텐츠 영역
        Unity 좌표 참고: 화면 중앙 (X: screen.width/2, Y: screen.height/2)
      */}
      <div className="relative z-10 flex flex-col items-center gap-12">
        {/* 
          게임 타이틀 영역
          Unity 좌표 참고: 중앙 상단 기준
          타이틀 Y 위치: 중앙에서 -150px 정도
        */}
        <div className="text-center space-y-4">
          <div className="flex items-center justify-center gap-3 mb-2">
            <h1 className="text-7xl font-bold tracking-wider drop-shadow-2xl" style={{ color: '#E6E0D6' }}>
              괴이탐사국
            </h1>
          </div>
        </div>

        {/* 
          시작 버튼
          Unity 좌표 참고: 화면 중앙 (X: screen.width/2, Y: screen.height/2 + 100px)
          크기: 약 240x72px
        */}
        <Button
          size="lg"
          onClick={handleStartGame}
          className="group relative px-12 py-6 text-xl font-bold shadow-2xl transition-all duration-300 hover:scale-105 border border-opacity-50"
          style={{ 
            background: 'linear-gradient(to right, #A1783A, #C39A52)',
            color: '#0F1115',
            borderColor: '#C39A52',
            boxShadow: '0 0 30px rgba(195, 154, 82, 0.3)'
          }}
        >
          <Play className="w-6 h-6 mr-2 group-hover:translate-x-1 transition-transform" />
          게임 시작
        </Button>

        {/* 
          장식 요소 (옵션)
          Unity 좌표 참고: 시작 버튼 아래 약 80px
        */}
        <div className="mt-8 flex gap-2">
          {[...Array(5)].map((_, i) => (
            <div
              key={i}
              className="w-2 h-2 rounded-full"
              style={{
                backgroundColor: '#4C566A',
                opacity: 0.3,
                animation: `pulse 2s ease-in-out infinite`,
                animationDelay: `${i * 0.2}s`,
              }}
            ></div>
          ))}
        </div>
      </div>

      {/* 설정 다이얼로그 */}
      <SettingsDialog open={isSettingsOpen} onOpenChange={setIsSettingsOpen} />
    </div>
  );
}
