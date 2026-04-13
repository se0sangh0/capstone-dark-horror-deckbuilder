# 괴이탐사국 (Dark Horror Deckbuilder) - Figma Make UI Prototype

Figma Make으로 제작한 다크 호러 덱빌딩 오토배틀러 게임 UI 프로토타입입니다.

## 📋 프로젝트 개요

이 프로토타입은 Slay the Spire 스타일의 덱빌딩과 Darkest Dungeon 스타일의 스트레스 시스템을 결합한 다크 판타지 로그라이크 게임의 UI/UX를 구현합니다.

## ✨ 구현된 주요 기능

### 1. 노드 시스템 (ExploreMap.tsx)
- **10층 구조**: 기획서 기준 10층 맵 시스템
- **층당 3갈래**: 2~8층은 각각 3개의 분기점
- **고정 노드**:
  - 1층: 시작 노드 (start)
  - 9층: 화툿불 (campfire) - 보스 직전 휴게소
  - 10층: 보스 (boss)
- **랜덤 노드** (2~8층):
  - 전투 노드 (combat): 60% 확률
  - 용병소 노드 (mercenary): 40% 확률
  - 연속 3회 같은 노드 타입 방지 로직
- **가중치 기반 랜덤 생성**: Fisher-Yates 셔플 알고리즘

### 2. 전투 시스템 (BattlePanel.tsx, BattleField.tsx)
- **턴 페이즈 구조**:
  1. 카드 플레이 단계 (draw/card_play)
  2. 선공 판정 단계 (initiative)
  3. 아군 행동 단계 (ally_action)
  4. 적 행동 단계 (enemy_action)
  5. 결과 처리 단계 (result)
- **선공 판정**: 스킬 코스트 기반
  - 아군 점수 = Σ(각 동료의 스킬 코스트 합)
  - 적 점수 = 고정 스택 (일반: 3, 보스: 8)
  - 동점 시 코인 토스
- **스킬 코스트 데이터** (기획서 10_동료_스킬_데이터.md 기준):
  - Caster: 4 (매직 미사일 1 + 파이어볼 3)
  - Offender: 7 (발도 2 + 일섬 5)
  - Defender: 7 (방어 준비 2 + 전투 태세 5)
  - Attacker: 5 (무모한 강타 1 + 불굴 4)
  - Priest: 8 (별부름 3 + 기원 5)
  - Shaman: 0 (추후 구현)
- **공용 스택**: 역할별 (dealer, tank, supporter) 독립 스택

### 3. 카드 시스템 (BattleCard.tsx, HandCards.tsx)
- **덱 구성**: 파티원 4명 × 10장/명 = 40장
- **초기 손패**: 파티원 수만큼 드로우 (4장)
- **드로우 시스템**: 턴 종료 시 사용한 카드 수만큼 재드로우
- **덱 고갈**: 덱이 비면 탈진 상태 (추후 구현 예정)
- **성향별 스택 값 생성** (0 제외 - 스택 유지 카드 없음):
  - `gambler`: -5 또는 +5 (50:50)
  - `safety`: [-1, 1, 2, 3] 중 랜덤
  - `opportunist`: [-3, -2, -1, 1, 2, 3, 4] 중 랜덤
  - `optimist`: [-5, -4, -3, -2, -1, 1, 2, 3, 4, 5] 중 랜덤

### 4. 동료 시스템 (RecruitPopup.tsx, PartySlot.tsx)
- **6종 직업** (MVP):
  - 딜러: Caster, Offender
  - 탱커: Defender, Attacker
  - 서포터: Priest, Shaman
- **동료 데이터**:
  - 이름 (랜덤 생성 - 판타지 이름 풀)
  - 직업 (jobClass)
  - 역할 (role: dealer/tank/supporter)
  - 성향 (tendency: gambler/safety/opportunist/optimist)
  - 체력 (hp/maxHp)
  - 스트레스 (stress/maxStress)
  - 성급 (star: 1~3)
- **파티 구성**: 최대 4명 (0~3번 슬롯)
- **예비대**: 무제한 (추후 제한 구현 예정)

### 5. 화툿불 시스템 (CenterContent.tsx)
- **위치**: 9층 고정 (보스 직전)
- **기능**:
  - 휴식: 체력 +30, 스트레스 -15
  - 명상: 스트레스 -25
  - 훈련: 파티원 경험치 획득 (추후 구현)

### 6. 용병소 시스템 (MercenaryOfficePanel.tsx)
- **기능**: 신규 동료 모집
- **모집 풀**: 3명 랜덤 생성
- **비용**: 직업별 소울스톤
  - Caster: 30
  - Offender: 30
  - Defender: 40
  - Attacker: 40
  - Priest: 35
  - Shaman: 35

## 🏗️ 프로젝트 구조

```
figma/
├── README.md                    # 이 파일
├── package.json                 # 의존성 정보
└── src/
    └── app/
        ├── App.tsx              # 메인 엔트리 (타이틀 화면)
        ├── components/
        │   ├── MainGameUI.tsx   # 게임 메인 UI 컨테이너
        │   └── game/
        │       ├── BattleCard.tsx           # 전투 카드 컴포넌트
        │       ├── BattleCharacter.tsx      # 전투 캐릭터 표시
        │       ├── BattleField.tsx          # 전투 필드 (아군/적 배치)
        │       ├── BattlePanel.tsx          # 전투 메인 패널
        │       ├── CenterContent.tsx        # 중앙 콘텐츠 영역 라우터
        │       ├── DeckStatus.tsx           # 덱 상태 표시
        │       ├── ExploreMap.tsx           # 탐험 맵 생성 로직
        │       ├── ExplorePanel.tsx         # 탐험 화면 패널
        │       ├── GrowthPopup.tsx          # 성장 팝업 (추후 구현)
        │       ├── HandCards.tsx            # 손패 UI
        │       ├── HelpPopup.tsx            # 도움말 팝업
        │       ├── LogPopup.tsx             # 전투 로그 팝업
        │       ├── MapNode.tsx              # 맵 노드 컴포넌트
        │       ├── MercenaryOfficePanel.tsx # 용병소 화면
        │       ├── PartyEditPopup.tsx       # 파티 편집 팝업
        │       ├── PartySlot.tsx            # 파티 슬롯 컴포넌트
        │       ├── PersistentUI.tsx         # 좌측 고정 UI
        │       ├── RecruitPopup.tsx         # 모집 팝업
        │       ├── ResourceBar.tsx          # 자원 표시 바
        │       ├── SettingsPopup.tsx        # 설정 팝업
        │       └── StressGauge.tsx          # 스트레스 게이지
        └── utils/
            ├── allyNameGenerator.ts  # 동료 이름 생성기
            └── jobClassData.ts       # 직업 정보 데이터
```

## 🎨 Unity 좌표 참조

모든 컴포넌트는 Unity 마이그레이션을 위한 좌표 주석을 포함합니다 (1920×1080 기준):

```typescript
/**
 * Unity 좌표:
 * X: 320px (좌측 패널 이후)
 * Y: 64px (상단 바 이후)
 * Width: 1600px
 * Height: 816px (880 - 64, 손패 영역 제외)
 */
```

### 주요 영역 좌표

- **고정 UI (좌측)**: X: 0, Y: 0, Width: 320px, Height: 1080px
- **중앙 콘텐츠**: X: 320px, Y: 0, Width: 1600px, Height: 1080px
  - 상단 바: Y: 0, Height: 64px
  - 메인 영역: Y: 64px, Height: 816px
  - 손패 영역: Y: 880px, Height: 200px

## 🛠️ 기술 스택

- **React**: 18.3.1
- **TypeScript**: 최신 버전
- **Tailwind CSS**: 4.1.12
- **Vite**: 6.3.5
- **Lucide React**: 0.487.0 (아이콘)
- **Radix UI**: 다양한 컴포넌트 (Dialog, Popover 등)

## 📝 기획서 반영 사항

이 프로토타입은 다음 기획 문서를 기반으로 구현되었습니다:

1. **노드 시스템**: `기획/시스템/01_노드_시스템.md`
   - 10층 구조, 3갈래 분기
   - 화툿불 9층 고정, 보스 10층 고정
   - 전투 60%, 용병소 40% 가중치

2. **전투 시스템**: `기획/시스템/02_전투_시스템.md`
   - 턴 페이즈 구조
   - 스킬 코스트 기반 선공 판정
   - 공용 스택 시스템

3. **카드 시스템**: `기획/시스템/03_카드_시스템.md`
   - 40장 덱 (파티원 4명 × 10장)
   - 성향별 스택 값 생성
   - 덱 고갈 시 탈진

4. **동료 데이터**: `기획/시스템/10_동료_스킬_데이터.md`
   - 6종 직업 MVP
   - 스킬 코스트 데이터
   - 역할별 분류

5. **캐릭터 시트**: `기획/시스템/04_캐릭터_시트_프로토타입.md`
   - 동료 속성 (hp, stress, star 등)
   - 성향 시스템 (gambler, safety, opportunist, optimist)

## 🔄 기획서 수정 제안

프로토타입 구현 중 다음 사항을 기획서에 추가하면 좋을 것 같습니다:

1. **선공 판정 상세 로직**:
   - 현재: "아군 스킬 코스트 합 > 적 고정 스택" 비교
   - 제안: 동점 시 코인 토스 (50:50) 명시

2. **덱 고갈 시 패널티**:
   - 현재: "탈진 상태" 언급만
   - 제안: 구체적 패널티 (예: 매 턴 전체 스트레스 +10)

3. **화툿불 옵션 제한**:
   - 현재: 무제한 사용 가능
   - 제안: 턴당 1회 제한 또는 비용 추가

4. **예비대 제한**:
   - 현재: 무제한
   - 제안: 최대 인원 제한 (예: 8명)

5. **스트레스 패닉 시스템**:
   - 현재: UI만 존재
   - 제안: 100 도달 시 구체적 효과 (예: 랜덤 행동, 파티 이탈 등)

## 🚀 Unity 마이그레이션 가이드

### 1. 레이아웃 변환
- 모든 컴포넌트의 Unity 좌표 주석 참조
- Canvas 해상도: 1920×1080 (16:9)
- UI 요소는 RectTransform 사용

### 2. 색상 팔레트
```csharp
// 주요 색상 (Hex)
public static class ColorPalette
{
    public static Color Background = HexToColor("#0F1115");
    public static Color Gold = HexToColor("#C39A52");
    public static Color DarkGold = HexToColor("#A1783A");
    public static Color Teal = HexToColor("#4CB3B3");
    public static Color Red = HexToColor("#B3263E");
    public static Color DarkRed = HexToColor("#D16474");
    public static Color Gray = HexToColor("#B5ADA0");
    public static Color DarkGray = HexToColor("#3A4452");
    public static Color Background2 = HexToColor("#1B1F27");
}
```

### 3. 데이터 구조
```csharp
// 동료 데이터 구조 예시
[Serializable]
public class AllyData
{
    public string id;
    public string name;
    public string jobClass;  // caster, offender, defender, attacker, priest, shaman
    public string role;      // dealer, tank, supporter
    public string tendency;  // gambler, safety, opportunist, optimist
    public int star;         // 1~3
    public int hp;
    public int maxHp;
    public int stress;
    public int maxStress;
}

// 카드 데이터 구조 예시
[Serializable]
public class BattleCardData
{
    public string id;
    public string role;      // dealer, tank, supporter
    public string tendency;  // gambler, safety, opportunist, optimist
    public int stackValue;   // 사용 시점에 생성
}
```

### 4. 주요 시스템 로직
- **맵 생성**: `ExploreMap.tsx`의 `generateMap()` 함수 참조
- **덱 셔플**: `BattlePanel.tsx`의 `shuffleDeck()` 함수 (Fisher-Yates 알고리즘)
- **스택 생성**: `BattlePanel.tsx`의 `generateStackValue()` 함수
- **선공 판정**: `BattlePanel.tsx`의 `performInitiativeCheck()` 함수

## 📦 의존성 설치 (개발 환경)

이 프로토타입을 로컬에서 실행하려면:

```bash
pnpm install
pnpm dev
```

주요 의존성:
- `react` ^18.3.1
- `react-dom` ^18.3.1
- `tailwindcss` ^4.1.12
- `lucide-react` ^0.487.0
- `@radix-ui/*` (다양한 UI 컴포넌트)

## 📄 라이선스

이 프로토타입은 `괴이탐사국` 프로젝트의 일부이며, 해당 프로젝트의 라이선스를 따릅니다.

## 🙋‍♂️ 문의

기획 문서와 프로토타입 간 차이점이나 구현 관련 질문은 프로젝트 이슈 트래커에 등록해주세요.

---

**제작**: Figma Make (Claude Code)  
**버전**: 1.0.0  
**최종 업데이트**: 2026-04-13