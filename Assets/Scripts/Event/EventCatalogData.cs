// ============================================================
// Event/EventCatalogData.cs
// `?` 노드 선택지 이벤트 19종의 코드 정의 (단일 원본)
// ============================================================
//
// [역할]
//   기획_통합/06_이벤트_노드.md §4-B 명세(19종)를 코드로 옮긴 원본 데이터.
//   ① 런타임 폴백 : Resources/Events/ 에 SO 에셋이 없을 때 이 정의로 즉시 동작.
//   ② 에셋 생성   : Editor/EventCatalogGenerator 가 이 정의를 읽어 .asset 을 굽는다.
//
//   따라서 이벤트 내용을 고칠 때는 이 파일을 수정한 뒤
//   Tools ▸ DarkHorror ▸ 이벤트 카탈로그 생성 을 다시 실행하면 에셋이 갱신된다.
//
// [주의]
//   EVT-17(기생하는 빛)의 파티 상태 조건부 분기와 오염도(EVT-18/19)는
//   기획상 미결(§4/§6)이라 대표 결과 1종 + 로그로 축약해 옮겼다.
// ============================================================

using System.Collections.Generic;
using UnityEngine;

using ET  = EventEffectType;
using TG  = EventTarget;
using CT  = EventCostType;

public static class EventCatalogData
{
    // ── 빌더 단축 함수 ──────────────────────────────────────────
    private static EventEffect  Eff(ET t, int v, TG tg)                         => new EventEffect(t, v, tg);
    private static EventOutcome Out(int w, string text, params EventEffect[] e) => new EventOutcome(w, text, e);
    private static EventChoice  Cho(string label, CT ct, int amt, params EventOutcome[] o) => new EventChoice(label, ct, amt, o);

    private static EventDefinition Def(string id, string title, string body, params EventChoice[] choices)
    {
        var so = ScriptableObject.CreateInstance<EventDefinition>();
        so.name     = id;
        so.id       = id;
        so.title    = title;
        so.bodyText = body;
        so.choices  = new List<EventChoice>(choices);
        so.minFloor = 1;
        so.maxFloor = 10;
        return so;
    }

    // 무비용 무효과 선택지 결과 (지나친다/무시한다)
    private static EventOutcome Nothing(string text = "변화 없음.") => Out(100, text, Eff(ET.None, 0, TG.None));

    /// <summary>19종 이벤트 정의를 새 SO 인스턴스로 빌드해 반환. (호출 시마다 새 인스턴스)</summary>
    public static List<EventDefinition> BuildAll()
    {
        var list = new List<EventDefinition>(19);

        // EVT-01 식어 있는 야영지
        list.Add(Def("evt_cold_camp", "식어 있는 야영지",
            "누군가 급히 떠난 흔적. 식은 컵라면에서, 아주 미세하게, 아직 김이 난다.",
            Cho("물자를 챙긴다", CT.None, 0,
                Out(100, "“누군가의 몫이었다. ‘였다’라고 믿기로 했다.”",
                    Eff(ET.SoulStone, 10, TG.None), Eff(ET.Stress, 10, TG.All))),
            Cho("흔적을 조사한다", CT.None, 0,
                Out(50, "“떠난 게 아니라, 데려가진 것이다.”",
                    Eff(ET.Stress, -5, TG.All), Eff(ET.NarrativeHint, 0, TG.None)),
                Out(50, "“라면 위로 김이 멈췄다. 방금까지 누가 보고 있었다는 뜻이다.”",
                    Eff(ET.Stress, 15, TG.All))),
            Cho("지나친다", CT.None, 0, Nothing())));

        // EVT-02 임시 상납소
        list.Add(Def("evt_field_altar", "임시 상납소",
            "피난 안내도의 화살표가 전부 이곳을 가리키고 있었다. 접수구 위에 적힌 글씨 — “상납은 질서다.”",
            Cho("영혼석을 바친다", CT.SoulStone, 10,
                Out(100, "“이상하리만큼 마음이 편해졌다. 그게 제일 이상했다.”",
                    Eff(ET.Stress, -20, TG.All))),
            Cho("접수구를 뒤진다", CT.None, 0,
                Out(50, "“앞사람의 상납분이다. 장부에는 이미 ‘수령 완료’라고 적혀 있었다.”",
                    Eff(ET.SoulStone, 15, TG.None)),
                Out(50, "“접수구 안쪽에서, 손이 아닌 것이 손목을 잡았다.”",
                    Eff(ET.Stress, 15, TG.All), Eff(ET.Hp, -15, TG.RandomOne))),
            Cho("무시한다", CT.None, 0, Nothing())));

        // EVT-03 낯익은 조사관
        list.Add(Def("evt_familiar_face", "낯익은 조사관",
            "벽에 기대앉은 조사관. 장비도, 소속 배지도 당신과 같다. 이름표만 비어 있다.",
            Cho("부축한다", CT.SoulStone, 15,
                Out(100, "“고맙다는 말 대신, 그는 당신의 이름을 정확히 불렀다.”",
                    Eff(ET.RecruitRandom, 1, TG.None))),
            Cho("말을 건다", CT.None, 0,
                Out(50, "“그는 말없이 일어나 당신들 뒤에 섰다.”",
                    Eff(ET.RecruitRandom, 0, TG.None)),
                Out(50, "“빈 이름표가 바닥에 떨어졌다. 안쪽에 당신의 이름이 적혀 있었다.”",
                    Eff(ET.Stress, 20, TG.All))),
            Cho("지나친다", CT.None, 0,
                Out(100, "“뒤에서 발소리가 따라오다, 멈췄다.”", Eff(ET.Stress, 5, TG.All)))));

        // EVT-04 속삭이는 영혼석
        list.Add(Def("evt_whispering_stone", "속삭이는 영혼석",
            "회수 규격을 벗어난 크기의 영혼석. 귀를 대지 않아도 들린다. 당신의 목소리로 말하고 있다.",
            Cho("귀를 기울인다", CT.None, 0,
                Out(100, "“싸우는 법을 알려주었다. 그것이 어떻게 아는지는 말하지 않았다.”",
                    Eff(ET.NextBattleStack, 2, TG.None), Eff(ET.Stress, 10, TG.All))),
            Cho("규격대로 회수한다", CT.None, 0,
                Out(100, "“쪼개는 순간, 목소리는 비명 없이 끝났다.”", Eff(ET.SoulStone, 8, TG.None))),
            Cho("부순다", CT.None, 0,
                Out(100, "“잠깐, 세계가 조용해졌다.”", Eff(ET.Stress, -10, TG.All)))));

        // EVT-05 임시 브리핑 단말
        list.Add(Def("evt_briefing_terminal", "임시 브리핑 단말",
            "“복귀 전 브리핑을 수강하십시오.” 화면 속 진행자는 눈을 깜빡이지 않는다.",
            Cho("수강한다", CT.SoulStone, 5,
                Out(100, "“머리가 맑아졌다. 무언가를 잊은 대가라는 생각도, 곧 잊었다.”",
                    Eff(ET.Stress, -25, TG.All))),
            Cho("단말을 뜯어본다", CT.None, 0,
                Out(50, "“전임자의 미수령 수당이 나왔다. 수령인 서명란은 비어 있었다.”",
                    Eff(ET.SoulStone, 10, TG.None)),
                Out(50, "“‘무단 접근이 기록되었습니다.’ 어디에 기록되는지는 적혀 있지 않았다.”",
                    Eff(ET.Stress, 10, TG.All))),
            Cho("전원을 끈다", CT.None, 0,
                Out(100, "“꺼진 화면에 비친 당신은, 한 박자 늦게 움직였다.”", Eff(ET.None, 0, TG.None)))));

        // EVT-06 거울 복도
        list.Add(Def("evt_mirror_corridor", "거울 복도",
            "양쪽 벽이 거울이다. 거울 속 일행은 숫자가 맞는데, 한 명은 지금과 다른 표정을 하고 있다.",
            Cho("다른 표정을 들여다본다", CT.None, 0,
                Out(100, "“거울 속의 그가 먼저 눈을 돌렸다. 지금의 그는, 조금 다른 사람 같다.”",
                    Eff(ET.RerollAffinity, 0, TG.ChosenOne), Eff(ET.Stress, 15, TG.ChosenOne))),
            Cho("거울을 깬다", CT.None, 0,
                Out(50, "“깨진 조각들은 전부 바닥만 비추었다. 그편이 나았다.”",
                    Eff(ET.Stress, -10, TG.All)),
                Out(50, "“조각이 튀었다. 거울 안쪽에서.”",
                    Eff(ET.Hp, -15, TG.RandomOne))),
            Cho("시선을 내리고 통과한다", CT.None, 0, Nothing())));

        // EVT-07 봉인된 보급 상자
        list.Add(Def("evt_supply_cache", "봉인된 보급 상자",
            "탐사국 보급 상자. 로고가 낯익은데, 볼수록 눈이 미끄러진다. 잠금 장치는 이쪽을 향해 달려 있다.",
            Cho("규정 해제 코드를 시도한다", CT.None, 0,
                Out(50, "“코드가 맞았다. 당신이 이 코드를 어떻게 알고 있었는지는, 넘어가자.”",
                    Eff(ET.SoulStone, 12, TG.None)),
                Out(50, "“‘권한 없음.’ 상자가 조금 더 단단해진 것 같다.”",
                    Eff(ET.None, 0, TG.None))),
            Cho("강제로 연다", CT.None, 0,
                Out(100, "“경첩이 아니라 이빨이 부러지는 소리가 났다.”",
                    Eff(ET.SoulStone, 12, TG.None), Eff(ET.Hp, -20, TG.RandomOne))),
            Cho("놓아둔다", CT.None, 0, Nothing())));

        // EVT-08 지켜보는 까마귀
        list.Add(Def("evt_crow_watcher", "지켜보는 까마귀",
            "까마귀 한 마리가 앉아 있다. 날개 소리를 들은 기억이 없다. 처음부터 거기 있었던 것처럼.",
            Cho("먹이를 준다", CT.SoulStone, 5,
                Out(100, "“까마귀는 먹이를 먹지 않았다. 다만 세는 것을 멈추었다.”",
                    Eff(ET.Stress, -10, TG.All))),
            Cho("쫓아낸다", CT.None, 0,
                Out(100, "“그것은 웃는 것처럼 울었다.”", Eff(ET.Stress, 5, TG.All))),
            Cho("마주 본다", CT.None, 0,
                Out(50, "“까마귀의 눈에 낫이 비쳤다. 이 방에는 낫이 없다.”",
                    Eff(ET.NarrativeHint, 0, TG.None)),
                Out(50, "“먼저 눈을 돌린 쪽은 당신이었다.”",
                    Eff(ET.Stress, 10, TG.All)))));

        // EVT-09 뒤처진 자의 잔재
        list.Add(Def("evt_left_behind", "뒤처진 자의 잔재",
            "바닥에 희미하게 빛나는 잔상이 놓여 있다. 전투에서 행동하지 못하고 사라진 동료의 잔재다. 만지면 아직 따뜻하다.",
            Cho("잔재를 흡수한다", CT.None, 0,
                Out(100, "“잔재가 녹아든 순간, 모두의 표정이 잠시 굳었다. 그걸 본 모두의 표정이.”",
                    Eff(ET.Stress, 5, TG.All), Eff(ET.NextBattleStack, 2, TG.None))),
            Cho("지나친다", CT.None, 0,
                Out(100, "“잔상을 밟고 지나간다. 발밑에서 무언가 가볍게 부서지는 느낌이 들었다.”",
                    Eff(ET.None, 0, TG.None)))));

        // EVT-10 영혼석 공명
        list.Add(Def("evt_stone_resonance", "영혼석 공명",
            "주머니 속 영혼석이 갑자기 진동한다. 공명이 동료들의 형체를 일그러뜨리고 있다. 구현체들이 고통스러운 듯 몸을 움츠린다.",
            Cho("공명을 증폭시킨다", CT.None, 0,
                Out(100, "“동료의 형체가 잠시 흐려졌다가 되돌아온다. 그 사이로 무언가 더 단단한 것이 엿보였다.”",
                    Eff(ET.Hp, -10, TG.RandomOne), Eff(ET.NextBattleStack, 1, TG.None))),
            Cho("동료들에게 분산시킨다", CT.None, 0,
                Out(100, "“공명이 퍼져나간 자리에, 낯선 형체 하나가 어렴풋이 서 있었다. 아는 얼굴 같기도 하다.”",
                    Eff(ET.Hp, -10, TG.All), Eff(ET.RecruitRandom, 1, TG.None))),
            Cho("영혼석을 감춘다", CT.None, 0,
                Out(100, "“돌을 천으로 감싸자 진동이 멎었다. 동료들이 안도한 듯 보인다. 정말일까.”",
                    Eff(ET.None, 0, TG.None)))));

        // EVT-11 벽에 새겨진 눈금
        list.Add(Def("evt_scratch_marks", "벽에 새겨진 눈금",
            "복도 막다른 벽에 손톱으로 긁은 눈금이 빽빽하다. 마흔일곱 개. 마지막 몇 줄은 피가 섞여 있다. 손바닥을 대보자 홈의 간격이 네 손가락과 정확히 일치한다.",
            Cho("마지막 눈금을 긋는다", CT.None, 0,
                Out(50, "“마흔여덟째를 새긴다. 손끝에 전해지는 감촉이 낯설지 않다.”",
                    Eff(ET.SoulStone, 15, TG.None)),
                Out(50, "“마흔여덟째를 긋는다. 동료들이 한 걸음 물러선다.”",
                    Eff(ET.SoulStone, 5, TG.None), Eff(ET.Stress, 10, TG.All))),
            Cho("흔적을 통째로 긁어 지운다", CT.SoulStone, 5,
                Out(100, "“숫자가 사라진다. 모두가 안심한 표정이다. 나만 왠지 손끝이 허전하다.”",
                    Eff(ET.Stress, -10, TG.All))),
            Cho("지나친다", CT.None, 0, Nothing())));

        // EVT-12 격리 해제 승인서
        list.Add(Def("evt_deploy_order", "격리 해제 승인서",
            "바닥에 흩어진 서류 더미. 탐사국 발신 「격리 해제 승인서」다. 피험자 번호, 체액 채취 동의, 재배치 가능 일자… 모든 항목에 도장이 찍혀 있다. 서명란에는 내 이름이 있다. 기억나지 않는다.",
            Cho("서류를 챙긴다", CT.None, 0,
                Out(100, "“주머니에 접어 넣는다. 문구 하나가 눈에 박힌다. ‘피험자는 절차에 동의하였으며, 기억 소거는 표준 규정에 따른 것임.’”",
                    Eff(ET.NarrativeHint, 0, TG.None))),
            Cho("찢어 버린다", CT.None, 0,
                Out(100, "“찢는다. 또 찢는다. 종이 조각이 흩어진다. 동료가 묻는다. ‘뭐였어?’ 대답하지 않는다. 목구멍까지 올라온 말을 삼킨다.”",
                    Eff(ET.Stress, -15, TG.RandomOne))),
            Cho("지나친다", CT.None, 0, Nothing())));

        // EVT-13 두 개의 진입 기록
        list.Add(Def("evt_unfamiliar_log", "두 개의 진입 기록",
            "벽면에 부착된 구형 탐사 일지. 오늘자, 이 구역 진입 기록이 두 건이다. 한 건은 06:12, 내 서명. 다른 한 건은 03:47, 역시 내 서명. 새벽의 나는 혼자 와 있었다.",
            Cho("03:47 기록을 뜯어낸다", CT.None, 0,
                Out(100, "“종이를 뜯어 주머니에 넣는다. 손이 떨린다. 내 서명은 분명한데, 내 기억은 아니다.”",
                    Eff(ET.SoulStone, 10, TG.None), Eff(ET.Stress, 10, TG.RandomOne))),
            Cho("기록을 정독한다", CT.None, 0,
                Out(100, "“기록에 따르면 나는 뭔가를 ‘회수’했다. 회수물 란은 비어 있다. 공란의 잉크 색이 주변보다 진하다. 누군가 지운 흔적이다.”",
                    Eff(ET.NarrativeHint, 0, TG.None))),
            Cho("지나친다", CT.None, 0, Nothing())));

        // EVT-14 그 자리
        list.Add(Def("evt_empty_spot", "그 자리",
            "좁은 통로를 빠져나가던 중, 동료 한 명이 갑자기 멈춰 선다. 허공을 응시한다. 입술이 움직이지만 소리는 없다. 몇 초 후 아무 일 없었다는 듯 걷기 시작한다.",
            Cho("“괜찮아?” 묻는다", CT.None, 0,
                Out(100, "“돌아본다. 미소를 짓는다. 방금 멈춰 섰던 자리를 정확히 피해서 걸어간다. 본인은 눈치채지 못했다.”",
                    Eff(ET.RerollAffinity, 0, TG.ChosenOne), Eff(ET.Stress, 5, TG.ChosenOne))),
            Cho("손목을 잡는다", CT.None, 0,
                Out(100, "“움켜쥐자 동공이 한순간 풀린다. 이내 초점이 돌아온다. ‘미안, 잠깐 멍했어.’ 손목은 차갑다. 오래된 돌을 쥔 것 같다.”",
                    Eff(ET.Hp, -10, TG.ChosenOne), Eff(ET.NextBattleStack, 2, TG.None))),
            Cho("지나친다", CT.None, 0, Nothing())));

        // EVT-15 잊힌 봉납 제단
        list.Add(Def("evt_forgotten_altar", "잊힌 봉납 제단",
            "돌 제단 위, 말라붙은 핏자국이 영혼석 몇 점과 나란히 놓여 있다. 누군가 바치고 갔는지, 바치려다 도망쳤는지는 알 수 없다.",
            Cho("피를 바친다", CT.None, 0,
                Out(100, "“상처가 아물지 않는다. 대신 손안의 돌이 체온을 빨아들이듯 따뜻해진다.”",
                    Eff(ET.Hp, -15, TG.RandomOne), Eff(ET.SoulStone, 12, TG.None))),
            Cho("영혼석을 바친다", CT.SoulStone, 10,
                Out(100, "“돌이 제단에 닿자 바스러진다. 동시에 상처가 거짓말처럼 사라진다. 다만, 기분 좋은 일은 아니다.”",
                    Eff(ET.Hp, 15, TG.All))),
            Cho("지나친다", CT.None, 0,
                Out(100, "“당연한 거래처럼 보였다. 그게 더 신경 쓰인다.”", Eff(ET.None, 0, TG.None)))));

        // EVT-16 갈라진 틈새의 손
        list.Add(Def("evt_hand_in_the_rift", "갈라진 틈새의 손",
            "복도 벽에 난 균열, 그 틈새로 사람의 손이 뻗어 나와 있다. 손바닥을 위로 향한 채, 무언가를 기다리는 자세다.",
            Cho("손을 잡는다", CT.None, 0,
                Out(50, "“손이 부드럽게 돌을 쥐여주고 틈새 너머로 사라진다. 손바닥에 남은 온기. 사람 체온이다.”",
                    Eff(ET.SoulStone, 15, TG.None)),
                Out(50, "“손이 움켜잡는다. 손목을 타고 올라온 한기가 머릿속 어딘가를 만진다. 놓아도, 만져진 기억은 남는다.”",
                    Eff(ET.Stress, 20, TG.All))),
            Cho("지나친다", CT.None, 0,
                Out(100, "“손은 아직 기다리고 있다.”", Eff(ET.None, 0, TG.None)))));

        // EVT-17 기생하는 빛 (조건부 분기 미결 → 대표 결과 ① 로 축약)
        list.Add(Def("evt_parasitic_light", "기생하는 빛",
            "어둠 속에 한 점 빛이 떠 있다. 가까이 다가가자 빛이 파티원들의 그림자를 훑더니, 가장 희미한 그림자 앞에서 멈춘다.",
            Cho("빛을 받아들인다", CT.None, 0,
                Out(100, "“빛이 당신들을 비추더니 이내 만족한 듯 사라진다. 바닥에 영혼석 한 줌이 남았다.”",
                    Eff(ET.SoulStone, 10, TG.None))),
            Cho("밀쳐낸다", CT.None, 0,
                Out(100, "“빛은 한 번 깜빡이더니 소리 없이 꺼졌다. 눈을 깜빡일 때마다 각막 뒤에서 무엇이 남아 움직이는 기분이다.”",
                    Eff(ET.Stress, 5, TG.All))),
            Cho("지나친다", CT.None, 0,
                Out(100, "“빛은 여전히 가장 약한 그림자를 응시하고 있다. 아니, 그냥 빛일 뿐이다.”",
                    Eff(ET.None, 0, TG.None)))));

        // EVT-18 언덕의 보물상자
        list.Add(Def("evt_hill_chest", "언덕의 보물상자",
            "목적지로 향하던 중, 언덕 한 구석에 커다란 보물상자가 놓여 있는 것을 발견한다. 주변에는 누군가 버리고 간 듯한 부서진 방패와 부러진 창대가 널브러져 있다 — 버려진 것치고는, 상자를 지키듯 둘러서 있다.",
            Cho("상자를 열어본다", CT.None, 0,
                Out(100, "“상자 안에는 언제부터 방치됐는지 모를 시체들이 뒤엉켜 있었다. 구역질을 참으며 뒤진 끝에, 당신은 그것을 꺼냈다. 시체들은 전부, 빈손이었다.”",
                    Eff(ET.ObtainObject, 16, TG.None), Eff(ET.Corruption, 1, TG.None))),
            Cho("무시하고 지나간다", CT.None, 0,
                Out(100, "“상자는 언덕에 남았다. 뒤돌아봤을 때, 뚜껑이 조금 더 열려 있었다.”",
                    Eff(ET.None, 0, TG.None)))));

        // EVT-19 발신불명의 메시지
        list.Add(Def("evt_unknown_coordinates", "발신불명의 메시지",
            "조사 노트에 정체불명의 좌표가 스스로 기록되고 있다. 좌표가 가리키는 지점은, 지금 있는 곳에서 그리 멀지 않다.",
            Cho("좌표를 따라간다", CT.None, 0,
                Out(100, "“숲길을 헤치고 나온 당신 앞에, 동료와 똑같이 생긴 존재가 목을 매단 채 죽어 있다. 널브러진 소지품에서 영혼석을 챙겼다. 노트의 좌표는, 다음 줄을 적기 시작했다.”",
                    Eff(ET.SoulStone, 10, TG.None), Eff(ET.Corruption, 1, TG.None))),
            Cho("갈 길을 서두른다", CT.None, 0,
                Out(100, "“궁금증을 눌러두고 발걸음을 재촉했다. 그날 밤, 노트의 좌표는 지워져 있었다. 잉크 자국도 없이.”",
                    Eff(ET.None, 0, TG.None)))));

        return list;
    }
}
