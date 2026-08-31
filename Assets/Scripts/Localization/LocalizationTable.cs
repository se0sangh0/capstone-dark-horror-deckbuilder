// ============================================================
// Localization/LocalizationTable.cs
// 한국어 → 영어 번역표 (해외 전시용)
// ============================================================
//
// [키] 한국어 원문 (코드/씬의 실제 문자열과 정확히 일치해야 함).
//   - 정적 라벨: 화면/코드의 문자열을 그대로 키로 넣으면 자동 스캔이 교체.
//   - 동적 서식: {0} 등이 든 서식 문자열은 코드에서 Loc.Tr(키, args) 로 호출.
//
// [추가 방법] 아래 Add(ko, en) 에 한 줄 추가. 새 한국어 문자열이 화면에 남아
//   번역이 안 되면, 그 문자열을 키로 여기에 넣으면 된다.
// ============================================================

using System.Collections.Generic;

public static class LocalizationTable
{
    public static readonly Dictionary<string, string> Ko2En = new();
    public static readonly Dictionary<string, string> En2Ko = new();

    private static void Add(string ko, string en)
    {
        if (string.IsNullOrEmpty(ko)) return;
        Ko2En[ko] = en;
        if (!En2Ko.ContainsKey(en)) En2Ko[en] = ko; // 역방향(영→한) — 왕복 전환용
    }

    static LocalizationTable()
    {
        // ── 타이틀 / 노드 화면 / 좌측 패널 ──
        Add("괴이탐사대", "Anomaly Investigation Corps");
        Add("괴이탐사국", "Anomaly Investigation Bureau");
        Add("재화", "Currency");
        Add("파티", "Party");
        Add("덱", "Deck");
        Add("파티편집", "Edit Party");
        Add("파티 편집", "Edit Party");
        Add("설정", "Settings");
        Add("로그", "Log");
        Add("파워업", "Power-Up");
        Add("현재 위치는 여기입니다", "You are here");

        // ── 공용 버튼 ──
        Add("계속", "Continue");
        Add("확인", "Confirm");
        Add("다음", "Next");
        Add("이전", "Previous");
        Add("닫기", "Close");
        Add("취소", "Cancel");
        Add("나가기", "Leave");
        Add("다음 층", "Next Floor");
        Add("다음 층으로", "To Next Floor");
        Add("다음 탐사", "Next Expedition");
        Add("타이틀", "Title");
        Add("시작하기", "Start");
        Add("오프닝 다시보기", "Replay Opening");
        Add("처음이신가요?", "First time?");
        Add("게임 시작", "Start Game");
        Add("게임 종료", "Quit Game");

        // ── 설정 ──
        Add("전체화면", "Fullscreen");
        Add("전체화면 (창모드)", "Fullscreen (Windowed)");
        Add("창모드", "Windowed");
        Add("메인화면으로", "To Main Menu");
        Add("음량", "Volume");
        Add("배경음", "Music");
        Add("배경음악", "Music");
        Add("효과음", "SFX");
        Add("화면 모드", "Screen Mode");
        Add("해상도", "Resolution");
        Add("언어", "Language");

        // ── 오프닝 (OpeningFlowController) ──
        Add("이전 탐사 결과", "Previous Expedition Report");
        Add("이전 탐사대와의 연락이 두절되었다.\n해당 임무는 실패로 종결한다.",
            "Contact with the previous expedition team has been lost.\nThe mission is closed as a failure.");
        Add("후속 투입 통보", "Redeployment Notice");
        Add("후속 투입 절차가 승인되었다.\n교전 수칙 숙지 후 즉시 현장으로 이동하라.",
            "Follow-up deployment has been approved.\nReview the rules of engagement and proceed to the site at once.");
        Add("계속하려면 아무 키나 누르십시오", "Press any key to continue");
        Add("아무 키나 누르면 현장으로 이동합니다", "Press any key to depart for the site");
        Add("아무 키나 누르면 타이틀로 돌아갑니다", "Press any key to return to the title");

        // ── 첫 전투 가이드 (CombatGuideController) ──
        Add("전투 안내\n① 손패의 카드를 클릭해 공용 스택을 쌓으세요.\n② [턴 종료]로 이번 턴을 확정합니다.\n③ 동료가 쌓인 스택에 맞춰 자동으로 행동합니다.",
            "Combat Guide\n(1) Click cards in your hand to build the shared stack.\n(2) Press [End Turn] to confirm this turn.\n(3) Companions act automatically based on the stack.");
        Add("턴 종료", "End Turn");

        // ── 전투 결과 팝업 (BattleResultScreen) ──
        Add("전투 승리", "Victory");
        Add("획득 재화", "Rewards");
        Add("아무 키나 누르면 계속됩니다", "Press any key to continue");
        Add("게임오버", "GAME OVER");
        Add("영혼석 +{0}", "Soulstone +{0}");            // 동적 (Loc.Tr)

        // ── 현장 관찰 (FieldObservationCatalog — 정적) ──
        Add("찢어진 바구니", "Torn Basket");
        Add("전투가 끝난 자리에서 찢어진 바구니가 발견된다. 안쪽에서는 평범한 약초와 열매, 땔감이 쏟아져 있다.",
            "A torn basket is found where the battle ended. Inside, ordinary herbs, berries, and firewood have spilled out.");
        Add("숲을 향한 목책", "Palisade Facing the Woods");
        Add("전투가 끝난 자리에서 숲을 향해 세워진 목책이 확인된다. 목책 위쪽 끝에는 흰 천 조각이 묶여 있다.",
            "Where the battle ended stands a palisade facing the woods. A scrap of white cloth is tied to its upper edge.");

        // ── EVT-01 식어 있는 야영지 (EventCatalogData — 정적) ──
        Add("식어 있는 야영지", "A Cold Campsite");
        Add("협곡 입구에 버려진 야영지가 있다. 꺼진 화덕의 재 아래쪽이 아직 따뜻하고, 말리다 만 약초 다발과 흙을 털지 않은 도구가 흩어져 있다.",
            "An abandoned campsite lies at the canyon's mouth. The ash beneath the dead hearth is still warm, and half-dried herb bundles and undusted tools lie scattered about.");
        Add("휴식하기", "Rest");
        Add("일행이 화덕에 새 불을 지피고 짧게 쉰 뒤 진행한다.",
            "The party rekindles the hearth, rests briefly, and moves on.");
        Add("흩어져서 살펴보기", "Split up and search");
        Add("일행이 흩어져 물건을 조사하던 중 짐 뒤의 괴생물체를 쫓아낸다. 회수한 물건에서 영혼석을 확보한다.",
            "While searching, the party drives off a creature lurking behind the packs and recovers soulstones from the salvaged goods.");
        Add("지나치기", "Move on");
        Add("일행은 기록에 없는 휴식처를 그대로 지나친다.",
            "The party passes by the unrecorded resting place.");

        // ── 탐사 보고서 (RunReportPanel — 정적 + 동적) ──
        Add("제 {0}차 정기 탐사 보고", "Expedition Report No. {0}");
        Add("결과: 탐사 완료, 성소 도달", "Result: Expedition complete - Sanctum reached");
        Add("결과: 탐사 실패, 파티 전멸", "Result: Expedition failed - party wiped out");
        Add("탐사 구역: 야생림—협곡—성소", "Survey zone: Wildwood - Canyon - Sanctum");
        Add("도달 구역: {0}", "Reached: {0}");
        Add("성소", "Sanctum");
        Add("{0}층", "Floor {0}");
        Add("미상", "Unknown");
        Add("기록 요약: 전투 {0}건, 사건 {1}건, 정비/회복 {2}건, 현장 관찰 {3}건 확인.",
            "Summary: {0} battles, {1} events, {2} recoveries, {3} field observations recorded.");
        Add("          동료 편성 변동 {0}건.", "          Party changes: {0}.");
        Add("획득 영혼석: {0}개", "Soulstone gained: {0}");
        Add("다음 행동을 선택하십시오", "Choose your next action");
        Add("그들은 무엇을 지키고 있었나.", "What were they protecting?");

        // ── 조사관 수첩 (InvestigatorNotebookController) ──
        Add("조사관 수첩", "Investigator's Notebook");
        Add("조사관 수첩 — 제 {0}차 탐사   ({1}/{2})", "Investigator's Notebook - Expedition No. {0}   ({1}/{2})");
        Add("아직 기록된 조사 내용이 없다.", "No records yet.");
        Add("[{0}층 | 제 {1}구역]", "[Floor {0} | Zone {1}]");
        Add("[{0}층 | 현장 기록]", "[Floor {0} | Field Record]");
        // 사건 기록 항목 (EventService — 동적)
        Add("확인 장소: {0}", "Location: {0}");
        Add("조치: {0}", "Action: {0}");
        Add("결과: {0}", "Result: {0}");

        // ── 전투 사건 기록 (RunSessionManager — 동적) ──
        Add("{0} 사살", "Slew {0}");
        Add("탐사대 소실", "Expedition team lost");
        Add("{0}와 교전 중 탐사대 소실", "The expedition team was lost while engaging {0}");
        Add("영혼석 {0}개 획득", "Recovered {0} soulstones");
        Add("괴이", "creature");
        Add("클리어", "Cleared");
        Add("전멸", "Wiped out");
        Add("최종 도달: {0}층", "Final depth: Floor {0}");

        // 조우 대상 요약 (BuildEnemySummary — 동적): "{name} {count}체"
        Add("{0} {1}체", "{1}x {0}");

        // 적 이름
        Add("고블린", "Goblin");
        Add("약탈자", "Raider");
        Add("거두는 자", "Reaper");
        Add("괴생물체", "Creature");

        // 동료 직업명
        Add("캐스터", "Caster");
        Add("오펜더", "Offender");
        Add("디펜더", "Defender");
        Add("프리스트", "Priest");
        Add("어택커", "Attacker");
        Add("샤먼", "Shaman");

        // ── 이벤트 결과 효과 요약 (EventService — 동적, 결과 창) ──
        Add("생존 동료", "Living companions");
        Add("동료 {0}명", "{0} companion(s)");
        Add("{0} 스트레스 {1}", "{0} Stress {1}");
        Add("{0} 스트레스 +{1}", "{0} Stress +{1}");
        Add("{0} HP {1}", "{0} HP {1}");
        Add("{0} HP -{1} (사망 없음)", "{0} HP -{1} (no deaths)");
        Add("영혼석 {0}", "Soulstone {0}");

        // ── 용병소 / 교회 / 화톳불 (서비스 라벨) ──
        Add("용병소", "Mercenary Post");
        Add("교회", "Church");
        Add("화톳불", "Campfire");
        Add("화툿불", "Campfire");
        Add("고용", "Hire");
        Add("편집", "Edit");
        Add("모집", "Recruit");
        Add("성장", "Growth");
        Add("리롤", "Reroll");
        Add("HP 회복", "Heal HP");
        Add("스트레스 회복", "Relieve Stress");
        Add("부활", "Revive");
        Add("판매", "Sell");
        Add("성소로 향한다", "Head to the Sanctum");
        Add("정비 뒤 성소로 향한다", "Head to the Sanctum after resting");

        // ── 카드 (스택 증가/감소) ──
        Add("스택 증가", "Stack Up");
        Add("스택 감소", "Stack Down");

        // ── 교회 UI (동적 라벨) ──
        Add("HP +{0} (영혼석 {1})", "Heal HP +{0} ({1} Soulstone)");
        Add("스트레스 -{0} (영혼석 {1})", "Stress -{0} ({1} Soulstone)");
        Add("부활할 동료가 없습니다", "No companions to revive");
        Add("사망한 동료가 없습니다", "No fallen companions");

        // ── 동료 카드 (FellowCardView — 역할/성향/액션) ──
        Add("딜러", "Dealer");
        Add("탱커", "Tank");
        Add("서포터", "Support");
        Add("도박사", "Gambler");
        Add("안전주의자", "Safety");
        Add("기회주의자", "Opportunist");
        Add("낙천가", "Optimist");
        Add("고용 ({0})", "Hire ({0})");
        Add("판매 (+{0})", "Sell (+{0})");
        Add("부활 ({0})", "Revive ({0})");
        Add("선택", "Select");
        Add("+ 동료 선택", "+ Select Companion");
    }
}
