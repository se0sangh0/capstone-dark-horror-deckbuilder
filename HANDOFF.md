# HANDOFF — 다음 세션 인수인계

> 마지막 갱신: **2026-07-29** (기획 문서 통합 세션 종료 — 6개 동료와 주술사 전투 개념 명세 완료, 세부 수치·UI는 추후 설정. 재개 시 질문 48에서 다음 기획 범위 선택)
> 마지막 갱신: **2026-06-12** (14차 — **디코 QA 6건 + 추가 1건 처리**: ①튜토 팝업 버튼(씬 Canvas 3종 복원+overrideSorting=100) · ②좌패널 동기화(ClearShield+아코디언 RefreshHeight) · ④메인메뉴 버튼 280×70/폰트36 · ⑤파티편집 표시 반전(B안) · ⑧게임오버→타이틀+런리셋 · ➕튜토 노드맵 안내 미표시(Start 레이스 — 초기화 Awake 이동) **수정·검증 완료** + ⑥까마귀 허수아비 애니 **재현 불가 판정**(현 코드 정상 실측 — 구버전 빌드 의심). 남은 것: ③·⑥ QA 재확인 요청 / ⑦ 툴팁("위력 진하게" 의미 미확인). **§14차 에디터 함정 5건 필독**)
> 마지막 갱신(이전): 2026-06-12 (13차 — 디코 QA 중간점검 접수·항목 정리 + ① 튜토리얼 팝업 버튼 정적 진단(파일상 정상 — 원인 미특정). 코드/씬 수정 0건, MCP 미연결 세션. §13차 — QA 원문 포함)
> 마지막 갱신(이전): **2026-06-11** (12차 — 로딩/결과 화면 + Panel_1 프레임 가시화 + **색 유실 사고 복구·씬 저장 함정 3가지**(§🚨 필독) + **UI 오버사이즈 일괄 개편**(#15~#20: PanelBase z-순서·좌패널·파티편집 세로 5:5·나가기 정사각 통일·용병소 아이콘·명단보기/성장 수정))
> 마지막 갱신(이전): **2026-06-10** (11차 — 상태이상 호버 툴팁 전투카드 수정 확인 + **턴종료 버튼 재배치**(8차 #7))
> 11차 세션 요약: (1) 상태 호버 툴팁이 전투 카드에서 안 뜨던 것 → 월드캔버스 호버를 뷰포트 수동검사로 해결, 사용자 확인 완료. (2) **턴종료 버튼 재배치(최종 B안)** — 상단 바 제거, **턴카운터=스택 헤더 우측 / 턴종료 버튼=검·방패·하트 3컬럼 줄 끝(4번째)** 가로정렬, 스택창 위로(+상하 마진). (3) **스킬 이펙트** — Effect/ 10개 mp4를 ffmpeg(흰배경 키잉)로 스프라이트화→`SkillEffectFx`로 스킬 시전 시 대상/시전자에 재생. 컴파일/렌더 검증(실전 훅은 사용자 확인). 상세 §11차.
> 마지막 갱신(이전): 2026-06-09 (10차 세션 — 노드 아이콘 검증 + 옛 아이콘 삭제 + **상태이상 표시 UI 아이콘화**(8차 #6))
> 10차 세션 요약: (1) 노드 아이콘 9차분 검증 완료(빈 원 아님, 재임포트 불필요) + 옛 Green/Random/Fire 삭제. (2) **상태이상 표시** — 공포경직/과호흡/중증디버프 등 표기 0이던 것을 아이콘+턴수로 표시(전투카드 HP아래 다중칩 + 좌패널 카드 우상단). FellowData 프로퍼티화+OnStatusChanged, StatusVisual 아이콘로더, status_*.png 7종. 컴파일 0·캡처 검증. 상세 §10차 세션.
> 9차 세션 요약: #5 스택카드 모양 통일(딜 sprite_sheet_15→8 민무늬), #4 스택창 헤더(공격/방어/지원)→아이콘(검24/방패31/하트32), 카드 숫자 96→54·정중앙·아이콘/설명 카드 안으로, 노드 마커→원형 컬러버튼+1-bit 안쪽 아이콘. ✅ **노드 아이콘은 10차 세션에 검증 완료**. 상세 §9차/§10차 세션.
> 8차 세션 요약: 스택카드 숫자 Bold·검은색·확대 + descText 축소, LeftPanelToggle hover 노출(180px), 하단 버튼 3개 stretch 복원·확대, Panel_1.png 9-slice border 0→60. ⚠️ LeftPanel ornament 노출 시도(VLG padding·자식 alpha·color tint)는 UI 깨져서 **전부 원복**. 상세 §8차 세션.
> 직전 큰 작업(2026-06-09 7차): 동료·적 스킬 스프라이트 Loader, 노드 타입별 마석 차등(10·20·30), DamagePopup AOE cascade, 보스 텔레포트 연출, 영혼석 드롭 Pool/Fx 코드. 상세 §7차 세션 완료.
> 직전 큰 작업(2026-06-05): 기획자 피드백 15항목 전부 완료 — 상세는 아래 §6차 세션 완료.
> 직전 큰 작업(2026-06-02~06-05): UI 전반 다크 서사톤 통일, 좌측 패널 접기 기능, 파티편집 양방향 스왑, 용병소/교회 통일 등 — 상세는 아래 §직전 세션 완료(2026-06-02~05)
> (이전) 2026-06-01: 튜토리얼 풀 플로우 + 모달 다이얼로그 + DoT + 노드/적 보강. 모달 박스 원본(900×400) 유지 롤백.

---

## 기획 문서 통합 세션 (2026-07-28~29)

### 작업 원칙

- **현재 구현 코드는 최신 기획과 차이가 크므로 코드보다 `기획_통합/` 문서를 기준으로 작업한다.**
- 기획자와 결정할 때는 한 번에 질문 하나만 제시하고, 2~3개 구체 선택지와 권장안을 제공한다.
- 결정된 답변은 관련 통합 문서·스토리보드·디자인 프롬프트에 즉시 반영하고 [15_미결_안건표](기획_통합/15_미결_안건표.md)의 완료 항목으로 남긴다.
- 현재 문서의 단일 기준은 [기획_통합/README](기획_통합/README.md), 미결의 단일 기준은 [15_미결_안건표](기획_통합/15_미결_안건표.md)다.

### 전역 확정 사항

- 파티는 1~4명, 최대 4인이다. 동료당 시작 덱 10장, 동료당 손패 슬롯 1칸이다.
- 영혼석은 런 내 재화다. 마석은 반복 런 보상으로 얻고 런 사이에 소비하는 계정 공용 영구 스탯 강화 재화이며, 《Vampire Survivors》 PowerUp형 구조를 따른다.
- 샤먼과 확장 적은 현재 범위에 정식 포함한다. 샤먼 스킬·능력치와 확장 적 밸런스 수치만 미정이다.
- 교회와 화톳불은 현재 범위에 포함하지만 상세 비용·회복량은 미정이다.
- 비선택 서사 문구는 노드 사이 막간 텍스트, 선택지는 `?` 노드 이벤트로 구분한다.
- 승리·패배 후에는 단순 타이틀 복귀가 아니라 런 종료 리포트를 먼저 표시한다.
- 오프닝은 타자기로 실패 요원 보고서를 작성한 뒤 현 플레이어의 후속 투입을 통보하는 방향이다. 전문·실패 원인·튜토리얼 순서는 아직 미결이다.

### 인식 레이어

- §3B-5의 문화·사회·수호자 설정은 해당 세계의 실제 기준인 `TRUE_BASELINE`이다.
- 기본 플레이 화면은 탐사국이 왜곡한 `FILTERED`다. 오염·암시는 `LEAK`, 재인식 거부·엔딩 장면은 `TRUE_VIEW`다.
- `FILTERED`는 생활 공간을 폐허·적대 거점으로, 주민과 수호자를 괴이로, 방어·피난 행동을 공격·감금·제물 의식으로 오독시킨다.
- 컨셉 아트는 별도 지정이 없으면 `FILTERED`로 생성한다. 전체 환경을 두 벌 만들지 않고 보스·동료 얼굴·핵심 표식부터 `TRUE_VIEW` 대체 에셋을 만든다.

### 차원 테마

- 최초 런 테마는 고대 말기~중세 초기의 가상 혼합 문화인 **초기 농경 신앙 세계**로 고정한다.
- 이후 구현된 테마는 런마다 무작위 배정하는 방향이나 실제 제작 수·가중치·중복 방지는 미결이다.
- 후보는 초기 농경 신앙 / 무협 변경 항전 / 일본 신사 토착신 / 현대 경계도시다.
- 초기 농경 세계와 무협 세계 모두 실존 국가·민족·왕조를 직접 대응시키지 않는다.
- 무협 세계의 외세 침략군은 별도 국가가 아니라 균열을 넘어온 플레이어와 탐사국 원정대다.

### 최초 테마 공간 제작 명세

- 탐사국은 어린 토착신을 `선발 조사대를 전멸시키고 차원 봉합을 방해하는 최상위 괴이`로 규정하고 좌표를 지정한다.
- 이동 순서는 `야생림 → 협곡 → 경작지 → 성소`다.
- 층 배경은 1~2 야생림 / 3 숲·협곡 혼합 / 4~5 협곡 / 6 협곡·경작지 혼합 / 7~8 경작지 / 9 큰 나무 쉼터 / 10 개방형 목석 성소다.
- 야생림 첫 전투는 고블린만 등장한다. 실제로는 채집민이며, 일부가 도망가고 남은 주민이 채집칼과 짐승 퇴치용 자극성 침으로 방어한다. 전투 후 바구니에서 약초·열매·땔감이 쏟아진다.
- 협곡에서는 채집민 고블린과 농민·나무꾼 약탈자가 섞인다. 실제 방어선은 숲 방향을 향하고 수레에는 곡식·담요·붕대가 있다.
- 경작지는 주민이 대피한 빈 농지다. 테마 전용 `?` 이벤트에서 `우회한다 / 조사한다 / 제거한다`를 선택한다. 조사는 환경 단서와 짧은 `LEAK`, 제거는 소형 수확 수호자 1체와 까마귀 권속 전투다.
- 9층은 실제로 깨끗하게 관리된 큰 나무 아래 돌 쉼터지만 `FILTERED`에서는 낡은 폐허로 보인다. 보이지 않는 권속이 꺼지지 않는 불을 관리하며, 방문 시 자동 회복 후 떠날 때 불이 꺼진다. 추가 그림자는 무작위 ‘잘못된 그림자’ 이벤트에서만 발생한다.
- 10층은 경작지 중앙의 개방형 목석 성소다. 주민들은 저장고 뒤편과 지하 피난실에 숨고, `FILTERED` 성소는 거두는 자의 생체 둥지처럼 보인다.
- 실제 토착신은 해진 의례복·짚 망토·작은 수확 낫을 지닌 농촌 아이 형상이다. 지치고 절박한 말투로 돌아갈 마지막 기회를 주지만, `FILTERED`에서는 거대한 허수아비형 거두는 자의 포효로 들린다.
- HP 50% `수확`에서 어린 신의 실루엣이 한 프레임 누출된다. 일반 승리에서는 영혼 전체가 영혼석으로 추출되고, 실제 주민은 나와 애도하지만 `FILTERED`에서는 빈 둥지와 `핵심 개체 제거 완료`만 보인다.

### 현재 Q&A 위치

- 공간 제작 명세는 완료되어 [13 §3B-5 A-1~3](기획_통합/13_서사_설정_확정.md), [03 §1-1A](기획_통합/03_노드_용병소_보상_메타.md), [06 §3-2](기획_통합/06_이벤트_노드.md)에 반영했다.
- **동료 6종 테마화 질문 41 답변 확정**: 캐스터·오펜더·디펜더·어택커·프리스트·샤먼은 내부 직업 ID로만 유지한다. 인게임 표시 직업명·외형·스킬명은 세계별로 변경하고, 최초 구현에서는 효과·수치를 공통으로 유지하는 L1 변형을 사용한다.
- **질문 42-1 답변 확정**: 초기 농경 신앙 세계 캐스터의 `TRUE_BASELINE` 실제 공동체 역할은 **절기 관측자**다. 별·바람·구름을 읽어 파종일·수확일을 정하고 서리·폭풍을 경고한다.
- **질문 42-2 답변 확정(전역 원칙에 맞춰 보정)**: 캐스터는 실제 목제 관측봉·청동 고리·매듭끈과 복식 실루엣을 유지한다. 얼굴에는 탈착식 점토 절기 가면을 씌우고 작은 탐사국 식별표·청록 결합부와 제한적인 원소 VFX로 원소 무장처럼 오독시킨다. 별자리는 보조 문양으로만 사용한다.
- **질문 42-3 답변 확정**: 캐스터의 인게임 표시 직업명은 **원소술사**다. 별자리보다 불·냉기·바람을 다루는 공격형 캐스터 가독성을 우선한다.
- **질문 42-4 답변 확정**: 원소술사의 초기 농경 신앙 L1 스킬 표시명은 `원소탄 / 화염구 / 바람 장막 / 빙결 폭풍`이다. 내부 ID·효과·비용·위력은 유지한다.
- **질문 42-5 답변 확정**: 원소술사는 30~50대 남성형·여성형으로 구성한다. `FILTERED` 전투 스프라이트는 공용 1종, `TRUE_VIEW` 얼굴 초상은 남성형·여성형 2종으로 제작한다.
- **캐스터 명세 완료**: 이후 캐스터 관련 추가 결정은 제작 검증에서만 재개한다.
- **질문 43-1 답변 확정**: 오펜더의 `TRUE_BASELINE` 실제 공동체 역할은 **큰짐승 사냥꾼**이다. 멧돼지·늑대 같은 위험 대상을 추적해 주민·가축·경작지를 지키고, 긴 사냥창과 무거운 사냥칼로 한 대상에 결정타를 가한다.
- **질문 43-2 답변 확정(전역 원칙에 맞춰 보정)**: 오펜더의 `FILTERED` 외형은 **가죽 봉인 추적자**다. 실제 수선한 가죽 망토·사냥창·사냥칼·매듭끈은 유지하고, 탈착식 꿰맨 가죽 가면과 작은 턱뼈 장식·소형 탐사국 부착물·표적선 VFX만 덧씌운다. 신체 융합은 사용하지 않는다.
- **질문 43-3 답변 확정**: 오펜더의 인게임 표시 직업명은 **척살자**다. 공동체를 지키던 사냥꾼을 한 대상을 집요하게 추적·제거하는 살육 전문 전투원으로 오독한 명칭이다.
- **질문 43-4 답변 확정**: 척살자의 초기 농경 신앙 L1 스킬 표시명은 `표적 관통 / 급소 절단 / 표적 말소 / 척살 집행`이다. 사냥 기술이 아니라 관리국의 제거 명령처럼 쓰며 내부 ID·효과·비용·위력은 유지한다.
- **질문 43-5 답변 확정**: 척살자는 20~40대 남성형·여성형으로 구성한다. `FILTERED` 전투 스프라이트는 공용 1종, `TRUE_VIEW` 얼굴 초상은 남성형·여성형 2종으로 제작한다.
- **오펜더 명세 완료**: 이후 오펜더 관련 추가 결정은 제작 검증에서만 재개한다.
- **질문 44-1 답변 확정**: 디펜더의 `TRUE_BASELINE` 실제 공동체 역할은 특수 직책이 아닌 **공동체 경비병**이다. 취락·곡창·농지 경계를 순찰하고 위기에는 원형 방패와 짧은 창으로 주민 대피를 돕는다.
- **질문 44-2 답변 확정(전역 원칙에 맞춰 보정)**: 디펜더의 `FILTERED` 외형은 **봉쇄 방패병**이다. 실제 원형 목제 방패의 크기·재질과 짧은 창을 유지하고, 탈착식 목제 가면·어두운 색보정·격리 인장 오버레이·소형 탐사국 부착물만 덧씌운다. 방패 확대와 신체 융합은 사용하지 않는다.
- **질문 44-3 답변 확정**: 디펜더의 인게임 표시 직업명은 **봉쇄병**이다. 공동체 경비 행동을 관리국의 구역 차단·격리 임무로 재분류한 명칭이다.
- **질문 44-4 답변 확정**: 봉쇄병의 초기 농경 신앙 L1 스킬 표시명은 `방벽 전개 / 접근 차단 / 봉쇄선 고정 / 강제 격리`다. 관리국의 차단·격리 명령처럼 쓰며 내부 ID·효과·비용·위력은 유지한다.
- **질문 44-5 답변 확정**: 봉쇄병은 20~50대 남성형·여성형으로 구성한다. `FILTERED` 전투 스프라이트는 공용 1종, `TRUE_VIEW` 얼굴 초상은 남성형·여성형 2종으로 제작한다.
- **디펜더 명세 완료**: 이후 디펜더 관련 추가 결정은 제작 검증에서만 재개한다.
- **질문 45-1 답변 확정**: 어택커의 `TRUE_BASELINE` 실제 공동체 역할은 **공동체 운반꾼**이다. 곡식·목재·물자를 나르고 위기에는 부상자를 운반하며 피난길을 여는 노동·구조 인력이다.
- **전역 외형 원칙 재확정**: 초기 농경 신앙의 모든 동료 `FILTERED`는 실제 복식·체형·장비 실루엣을 유지한다. 얼굴 검열·관리국 번호표·소형 청록 영혼석 결합부·UI/VFX만 덧씌우며, 신체 융합과 괴물화는 적·보스·고오염 단계에만 사용한다.
- **질문 45-2 답변 확정**: 어택커의 `FILTERED`는 실제 운반꾼의 작업복·체형·짐틀·운반 도구를 유지하고, 빛바랜 짐보자기 두건으로 눈과 이마만 가린다. 가슴 운반 끈에는 작은 탐사국 번호표를, 등짐틀에는 소형 청록 영혼석 결합부를 부착하며 장비·신체 변형은 사용하지 않는다.
- **질문 45-3 답변 확정**: 어택커의 인게임 표시 직업명은 **돌격병**이다. 운반꾼이 짐과 부상자를 나르며 피난길을 열던 행동을 관리국이 적진 돌입과 진입로 확보 임무로 재분류한 이름이다.
- **질문 45-4 답변 확정**: 돌격병의 초기 농경 신앙 L1 스킬 표시명은 `진로 개척 / 전열 보급 / 강행 돌파 / 경로 제압`이다. 관리국의 진입 작전 명령처럼 쓰며 내부 ID·효과·비용·위력은 유지한다.
- **질문 45-5 답변 확정**: 돌격병은 20~50대 남성형·여성형으로 구성한다. `FILTERED` 전투 스프라이트는 중성적 공용 1종, `TRUE_VIEW` 얼굴 초상은 남성형·여성형 2종으로 제작한다.
- **어택커 명세 완료**: 이후 어택커 관련 추가 결정은 제작 검증에서만 재개한다.
- **질문 46-1 답변 확정**: 프리스트의 `TRUE_BASELINE` 실제 공동체 역할은 **성소 치유사**다. 약초·붕대·따뜻한 물과 간단한 회복 의례로 환자와 부상자를 돌보며, 영혼과 교섭하는 샤먼이나 절기를 관측하는 캐스터와 구분한다.
- **질문 46-2 답변 확정**: 프리스트의 `FILTERED`는 실제 치료복·도구를 유지하고 **짧은 약포 베일 + 치료용 천 마스크**로 머리카락·이마·코·입을 가린다. 약가방 끈에는 작은 번호표를, 성소 방울에는 소형 청록 결합부를 부착하며 장비·신체 변형은 사용하지 않는다.
- **질문 46-3 답변 확정**: 프리스트의 인게임 표시 직업명은 **치유사**다. 실제 성소 치유사의 역할을 짧게 줄인 이름이며, 관리국 왜곡은 스킬명·설명문·치료 대상 프레임·안정화 UI에서 드러낸다.
- **질문 46-4 답변 우선 확정**: 치유사의 초기 농경 신앙 L1 스킬 표시명은 `응급 처치 / 위협 제거 / 집중 치료 / 일괄 안정화`다. 관리국의 의료·안정화 절차처럼 쓰며 내부 ID·효과·비용·위력은 유지한다. 제작 검증에서 표현만 재조정할 수 있다.
- **데이터 정정**: 네 번째 내부 스킬의 기존명은 `치유의 파동`이 아니라 `기원(skill_prayer)`이며, 선택한 표시명 `일괄 안정화`는 이 스킬에 연결했다.
- **질문 46-5 답변 확정**: 치유사는 20~60대 남성형·여성형으로 구성한다. `FILTERED` 전투 스프라이트는 베일·마스크를 공유하는 공용 1종, `TRUE_VIEW` 얼굴 초상은 남성형·여성형 2종으로 제작한다.
- **프리스트 명세 완료**: 이후 프리스트 관련 추가 결정은 제작 검증에서만 재개한다.
- **질문 47-1 답변 확정**: 샤먼의 `TRUE_BASELINE` 실제 공동체 역할은 **영혼 중재자**다. 조상·들판·가정의 작은 영들과 공동체 사이를 중재하고 금기 위반·경계의 불안정을 진정시키며, 몸을 치료하는 치유사나 절기를 관측하는 캐스터와 구분한다.
- **질문 47-2 답변 확정**: 샤먼의 `FILTERED`는 실제 복식·의례 도구를 유지하고 **잿빛 두건 + 매듭 장막**으로 머리카락·머리 윤곽·눈·코를 가린다. 재 주머니에는 작은 번호표를, 연기 그릇에는 소형 청록 결합부를 부착하며 장비·신체 변형은 사용하지 않는다.
- **질문 47-3 답변 확정**: 샤먼의 인게임 표시 직업명은 **주술사**다. 실제 영혼 중재자의 매듭·재·연기 의례를 재인식 필터가 적대적 약화·지속 피해 주술로 오독한 이름이다.
- **질문 47-4 답변 확정**: 주술사는 20~60대 남성형·여성형으로 구성한다. `FILTERED` 전투 스프라이트는 두건·매듭 장막을 공유하는 공용 1종, `TRUE_VIEW` 얼굴 초상은 남성형·여성형 2종으로 제작한다.
- **6개 동료 시각 명세 완료**: 캐스터·오펜더·디펜더·어택커·프리스트는 역할·외형·표시명·L1 스킬명·변형 범위를 확정했다. 샤먼은 역할·외형·표시명·변형 범위를 확정했으며 상세 전투 스킬·능력치 수치만 미결이다.
- **질문 47-5 답변 확정**: 주술사의 전투 기능은 `지속 피해 / 적 디버프 / 아군 버프` 3축 혼합형으로 구성한다. 세 축을 모두 포함하되 축별 스킬 개수·상세 효과·비용·수치는 아직 배분하지 않는다.
- **질문 47-6 답변 확정**: 아군 버프 축의 핵심 기능은 대상 동료의 **다음 행동 위력 강화**다. 다음 공격·실드·회복에 모두 적용하되 강화율·대상 범위·정확한 소모 시점은 아직 정하지 않는다.
- **질문 47-7 답변 확정**: 적 디버프 축의 핵심 기능은 대상 적의 **다음 공격 위력 감소**다. 감소율·대상 범위·공격 판정·정확한 소모 시점은 아직 정하지 않는다.
- **질문 47-8 답변 철회**: 같은 종류의 지속 피해를 비중첩·지속시간 갱신으로 처리하던 결정은 질문 47-15에서 철회했다.
- **질문 47-9 답변 확정**: 지속 피해는 아군과 적의 행동이 모두 끝난 **전투 턴 종료 시점**에 일괄 적용한다. 현행 적 독침 DoT 처리 흐름과 같은 페이즈를 사용한다.
- **질문 47-10 답변 확정**: 새로 부여된 지속 피해도 **부여된 같은 전투 턴의 종료 시점부터 첫 틱**이 발생한다. 별도의 적용 턴 유예나 즉시 절반 피해 예외는 두지 않는다.
- **질문 47-11 답변 확정**: **서로 다른 지속 피해 종류는 한 대상에게 함께 공존**하며 턴 종료에 종류별 피해를 모두 처리한다. 같은 종류의 비중첩 전제는 질문 47-15에서 철회했다.
- **질문 47-12 답변 확정**: 지속 피해의 틱 피해량은 대상 최대 체력이나 주술사 능력치에 비례시키지 않고 **스킬별 고정값**을 사용한다. 정확한 피해 수치는 밸런스 단계까지 비워 둔다.
- **질문 47-13 답변 확정**: 지속 피해의 지속시간은 모든 효과에 공통값을 적용하지 않고 **스킬별 개별값**으로 설정한다. 정확한 지속 턴 수는 밸런스 단계까지 비워 둔다.
- **질문 47-14 답변 확정**: 카드·상태 툴팁의 `지속 N턴`은 적용 턴 종료의 첫 틱을 포함한 **총 N회 피해**를 뜻한다. 남은 턴은 남은 틱 횟수와 동일하게 표시한다.
- **질문 47-15 답변 확정**: 이전의 비중첩 결정을 뒤집고 **같은 종류의 지속 피해도 중첩 가능**하게 한다. 따라서 갱신 시 피해값 선택 질문은 폐기하며, 정확한 중첩 단위·상한·지속시간 관리는 후속 확정한다.
- **질문 47-16 답변 확정**: 같은 종류의 지속 피해도 **적용마다 독립 인스턴스**로 생성한다. 각 인스턴스는 해당 스킬의 고정 틱 피해값과 남은 틱 횟수를 따로 보유하고 개별 만료하며 피해나 지속시간을 공유하지 않는다.
- **질문 47-17 답변 확정**: 한 대상에게 유지할 수 있는 지속 피해 인스턴스에는 종류별·대상별 **별도 중첩 상한을 두지 않는다**. 카드 비용·사용 기회·각 인스턴스의 개별 만료로 자연스럽게 제한한다.
- **질문 47-18 답변 확정**: 같은 종류의 지속 피해는 **종류별 아이콘 하나로 묶어** 인스턴스 수와 다음 틱 총피해를 표시한다. 툴팁에서는 각 인스턴스의 고정 피해와 남은 틱을 확인하며, 세부 배치·표기 형식은 추후 설정한다.
- **주술사 전투 개념 명세 완료**: 지속 피해·적 디버프·아군 버프 3축과 핵심 지속 피해 규칙까지만 확정한다. 축별 스킬 개수·정확한 수치·상세 스킬·버프/디버프 세부 소모 규칙·UI 세부는 추후 설계로 보류한다.
- **세션 종료 지점**: 사용자 요청으로 이번 작업을 여기서 종료한다. 재개 시 질문 48에서 다음 기획 범위를 **초기 테마 적군 설계 / 동료 개별 서사 / 6개 동료 명세 통합 점검** 중 선택한다.

---

## ✅ 14차 세션 — 디코 QA 6건 처리 (①②④⑤⑥⑧) (2026-06-12)

> MCP 연결 세션. 13차 §E 미답 질문 전부 답 받음: ①재현환경=**에디터·빌드 둘 다** / ②의미=**좌패널 아코디언 동기화 버그** / ⑤=**B안** / ⑧=**타이틀행+패배 직후 리셋**. 건마다 진단→수정안 제시→OK→백업→수정→검증 워크플로우로 진행.

### A. 항목별 결과 요약
| # | 항목 | 결과 | 변경 | 백업(~/Documents/backup/) |
|---|---|---|---|---|
| ① | 튜토 팝업 버튼 안 눌림 (P2) | ✅ 수정·검증 | GamePlayScene.unity (코드 0) | `2026-06-12_140523_session14_tutorial_canvas_restore` |
| ② | 좌패널 동기화 (P2) | ✅ 수정·검증 | FellowData.Hp.cs / BattleManager.Phases.cs / AccordionController.cs / LeftPanelView.cs | `2026-06-12_143826_session14_leftpanel_sync` |
| ④ | 튜토 메인메뉴 버튼 (P3) | ✅ 수정·검증 | GamePlayScene.unity (씬 오버라이드) | `2026-06-12_153746_session14_skipbtn_resize` |
| ⑤ | 파티 순서 역순 (P3) | ✅ 수정·검증 | PartyEditPanel.prefab (코드 0) | `2026-06-12_145742_session14_partyedit_reverse` |
| ⑥ | 까마귀 허수아비 애니 (P3) | 🔎 **재현 불가 — 코드 정상** | 변경 없음 | — |
| ⑧ | 게임오버→타이틀 (개선) | ✅ 수정·검증 | BattleManager.Phases.cs | `2026-06-12_150636_session14_gameover_to_title` |
| ➕ | 튜토 노드맵 안내 미표시 (사용자 추가 요청) | ✅ 수정·검증 | TutorialGuidePanel.cs | `2026-06-12_160838_session14_nodemap_intro_race` |
| ➕ | 튜토 승리팝업·랜덤노드 통일 (사용자 추가 요청) | ✅ 수정·검증 | MapGenerator.cs / NodeSystem.cs / BattleManager.Phases.cs / TutorialManager.cs | `2026-06-12_161957_session14_tutorial_victory_randomnode` |

### B. ① 튜토 팝업 버튼 — 진짜 원인과 수정 (13차 정적 진단의 빈틈)
- **원인**: 씬의 TutorialCanvas 프리팹 인스턴스에 **m_RemovedComponents 3종**(Canvas·CanvasScaler·GraphicRaycaster — 13차가 m_Modifications만 보고 제거 오버라이드를 놓침) → 팝업이 루트 Canvas 일반 자식[6]으로 강등 → 전투 진입 시 활성화되는 `GamePlayScene_RightMainArea`[13] 루트의 **풀스크린 투명 Image(sprite=Background, α=0, raycastTarget=true)**가 클릭 전부 흡수. RaycastAll 실측으로 확정 (최상단=RightMainArea). 노드맵에선 RMA 비활성이라 클릭 정상 — QA 증상·에디터/빌드 양쪽 재현과 전부 부합.
- **수정**: `PrefabUtility.GetRemovedComponents().Revert()` 3종 복원 + **overrideSorting=true** (중첩 캔버스라 이것 없이는 sortingOrder=100이 무시됨 — 직렬화값 100은 살아 있었음). 씬 저장+디스크 검증.
- **검증**: RaycastAll 최상단=NextButton + 클릭 시뮬→Hide() 실행(CanvasGroup α=0) + 씬 리로드 후 removedComponents=0. BossIntro 등 같은 캔버스 팝업도 함께 해결됨.

### C. ② 좌패널 동기화 — 두 증상 두 원인
- **쉴드 잔존**: 실드 리셋이 전투 **시작** 시에만 있고(BattleManager.cs InitBattle "일시 상태만 리셋") **종료 시 없음** + `shield`는 public 필드라 직접 할당은 OnShieldChanged 미발행 → `FellowData.ClearShield()` 신설(0+이벤트) + `HandleBattleEnd()` 진입 직후(승패/튜토 분기 전 공통) 아군 전원 호출. 실측: 실드30→전투종료 즉시 0.
- **사망 빈 공간**: Refresh 체인(RemoveFellow→OnPartyChanged→Refresh)은 정상이나, **AccordionController가 펼침 때 잰 높이로 contentPanel.sizeDelta를 고정** → 슬롯이 꺼져도 높이 안 줄어 빈 공간(접었다 펴면 재측정돼 사라짐 — QA 표현과 일치) → `RefreshHeight()` 신설(열림 상태면 SetOpen(true,instant) 재호출 — 기존 instant 경로가 형제 아코디언 재배치까지 처리) + `LeftPanelView.Refresh()` 끝에서 하위 아코디언 전부 호출(GetComponentsInChildren — 배선 불필요). 실측: 4명 630→사망 직후 자동 470.

### D. ⑤·⑧·④ 요점
- **⑤**: PartySlots(파티편집)의 슬롯 위치는 **HorizontalLayoutGroup이 런타임 배치**(직렬화 x=0 — 라이브 416/692/968/1244는 HLG 결과) → 프리팹에서 **reverseArrangement=true** 한 속성으로 반전. `slot[0]`, 즉 1번=오른쪽 끝 1244 실측 + 캡처 + SwapFellows 회귀 OK. 코드/스왑 로직 무변경.
- **⑧**: 패배 콜백 `ShowDefeat(() => StartNextRunLoop())` → `ResetRunState() + SceneTransition.Go("GameStartScene")`. 리셋 3종(예비대·파티·영혼석, 마석 유지)을 `ResetRunState()`로 추출(보스 클리어 새 런과 공유). **패배 직후 리셋이 필수인 이유**: 타이틀의 `[시작하기]` 버튼(`MoveScene`)은 ForceReinitParty만 호출 — 예비대/영혼석 안 비움(§D 우려가 실제 빈틈이었음). 실측: 영혼석77→20(PlayerPrefs 영속)+새 파티+타이틀 캡처+`[시작하기]` 재진입 루프 정상. 보스 클리어 엔딩→새 런 직행은 범위 밖 유지.
- **④**: SkipButton(메인 메뉴로) 180×50/폰트54 → **280×70/폰트36+NoWrap** (텍스트가 버튼 밖 두 줄로 깨지던 것 → 한 줄). 우상단 위치 유지(A안). 씬 오버라이드로 저장, 라이브 튜닝→씬 적용→리로드 검증→최종 캡처.

### E. ⑥ 까마귀 허수아비 애니 — 재현 불가 (13차 가설 기각)
- 13차 가설("Summon 분기가 시전 모션 안 탐")의 근거였던 EnemyAction.cs 101행 주석("Summon/Teleport는 즉시 효과 — 모션 없음")이 **낡은 것** — 실제 모션 트리거(OnSkillCast)는 분기 **전**(86행)에서 전 스킬 공통 발행.
- 실측(보스전 강제 진입 + `ExecuteEnemySkillCast(boss, summon)` 직접 실행): 보스 _hasAttack=[T,T,T,T] / 까마귀 2마리 소환 / **Attack2 애니 상태 진입·재생 확인** / Animator 리바인드 없음. **현 코드에서 정상.**
- 판단: 보스 스킬 애니는 12차(6/11) 작업 — QA가 그 이전 빌드로 테스트했을 가능성 유력. **③(순간이동)과 함께 최신 빌드로 QA 재확인 요청 권장.** (선택) 101행 낡은 주석 현행화는 미진행 — 다음 세션 1줄 작업.

### E-2. ➕ 튜토 노드맵 안내("노드를 클릭하세요") 미표시 — Start 레이스
- 사용자 요청 "튜토 진입 시 노드 클릭 안내 추가" → 조사 결과 **이미 구현돼 있었음**: `NodeMapIntro`(DialogueId 0, "환영합니다… 다음 노드를 클릭해 진행해보세요") + NodeSystem.Start() 호출. 안 보인 원인 = **Start 실행 순서 레이스** — NodeSystem.Start()의 Show(SetVisible true) 후에 TutorialGuidePanel.Start()의 초기화 `SetVisible(false)`가 실행되면 방금 띄운 팝업이 덮임. `_shownDialogues`에 이미 기록돼 재시도도 없음. 씬 로드 직후에 뜨는 NodeMapIntro만 해당 (CombatIntro 등은 한참 뒤라 무사).
- **수정**: TutorialGuidePanel 초기화(숨김+리스너+하이라이트 숨김)를 Start→**Awake로 이동** (Awake는 모든 Start보다 먼저 — 호출측 무수정).
- **검증**: 실경로(GameStartScene Play→[처음이신가요?] 클릭→GamePlayScene) — 팝업 alpha=1·메시지 정확·캡처 확인 + [확인] 클릭 닫힘. 컴파일 0.

### E-3. ➕ 튜토 승리팝업 + 용병소 랜덤노드 통일 (사용자 요청)
- **승리 팝업**: 튜토 일반 전투 승리가 옛 Win 팝업(ToggleResultDisplay 1.5s×2)이던 것 → **본편과 동일 `BattleResultScreen.ShowVictory(영혼석, 콜백)`** (콜백=노드맵 복귀+BGM+CombatVictory 모달). **패배 계열(일반 패배 자동 재시작 / 보스 전멸 완료 종료)은 기존 Lose 팝업 연출 유지** — 본편 ShowDefeat(타이틀행)와 흐름이 달라 통일 불가. 튜토 승리에 스트레스 회복(GrantStressRecovery)은 기존대로 미적용(고정 시나리오 보존).
- **랜덤노드**: 튜토 맵 `[Combat, Shop(고정), Rest, Boss]` → **`[Combat, Event, Rest, Boss]`** — 본편과 동일한 물음표 랜덤노드(EventOutcomeWeights 100/0/0 → 용병소 확정). ShopIntro 트리거를 모달 스위치(case Shop — 제거)에서 **Event 랜덤 결과=용병소 분기 안으로 이동**. Dispatch의 `case RoomType.Shop`(고정 용병소, 사용 0)도 제거.
- **대화 수정**: CombatVictory에 "다음 물음표 노드는 들어가기 전까지 무엇이 나올지 모르는 랜덤 노드예요" 추가 / ShopIntro 첫 줄 "용병소입니다..." → "랜덤 노드에서 용병소가 나왔습니다!".
- **검증(실경로 풀 플로우)**: [처음이신가요?] → 맵 4노드(Combat/Event/Rest/Boss) 확인 → 1층 승리 → **새 승리팝업(영혼석 +8) 캡처** → [다음으로] → CombatVictory 새 문구 → 2층 Event 클릭 → **용병소 열림+ShopIntro 새 문구 캡처**. 컴파일·스크립트 에러 0.

### F. ⚠️ 14차 발견 — 에디터/MCP 함정 5건 (다음 세션 필독)
1. **비포커스 시 stop도 펜딩** — manage_editor stop이 success를 반환해도 `isPlayingOrWillChangePlaymode=True`로 남음. 그 상태의 "에디트 모드 진단"은 전부 플레이 모드에서 실행된 것. 해결: `osascript -e 'tell application "System Events" to set frontmost of first application process whose name is "Unity" to true'` 후 stop 재요청.
2. **플레이 모드에선 프리팹 API 무력** — IsPartOfPrefabInstance=False/NotAPrefab/handle=null 반환. 프리팹 진단·수정은 반드시 에디트 모드 확인(isPlaying=False) 후.
3. **EditorApplication.Step()** — 비포커스 프레임 정지를 뚫고 프레임 강제 진행(코루틴/WaitUntil/애니 검증에 유효). 단 `EditorApplication.update` 펌프 등록은 비포커스 idle에서 안 불림(무효).
4. **LoadingScreen은 실시간 기반** — 한 execute 안의 Step 폴링으론 안 걷힘(벽시계 시간 필요). 다음 execute에서 재폴링하면 통과(호출 간 간격이 시간을 채움). 첫 폴링 타임아웃은 정상 현상.
5. **FinishPlayerTurn 레이스** — currentPhase=PlayerCardPlay 폴링 직후 호출하면 HandlePlayerCardPlay 진입(59행 `isPlayerTurnFinishing=false`)이 플래그를 덮음 → 다음 execute에서 재호출하면 통과. (실플레이는 버튼 활성 타이밍상 발생 불가 — 게임 버그 아님)
- 비고: Screen.width/height는 비포커스 시 게임뷰가 아닌 값(3024×72 등) 반환 — RaycastAll 좌표 검증 전 `GetWindow(GameView).Focus()` + Screen 값 확인 필수.

### G. 다음 세션
1. **⑦ 툴팁 상세설명** — 스킬 이름 밑으로 + 폰트 색 조정 + 약간 키움 (`UI/SkillTooltip.cs` 구조화 엔트리). **"위력 부분 폰트 진하게" 의미는 사용자/QA에 확인 필요**(위력 줄은 삭제된 상태).
2. **③·⑥ QA 재확인 요청** — 최신 빌드(12차 보스 애니+14차 수정 포함)로. ②와 ①도 수정 빌드로 함께 재검증 받으면 좋음.
3. (1줄) EnemyAction.cs 101행 낡은 주석 현행화.

### H. 14차 백업 목록
```
2026-06-12_140523_session14_tutorial_canvas_restore  (GamePlayScene.unity — ① 수정 전)
2026-06-12_143826_session14_leftpanel_sync           (FellowData.Hp/Phases/Accordion/LeftPanelView.cs — ② 수정 전)
2026-06-12_145742_session14_partyedit_reverse        (PartyEditPanel.prefab — ⑤ 수정 전)
2026-06-12_150636_session14_gameover_to_title        (BattleManager.Phases.cs — ⑧ 수정 전, ② 수정 후 버전)
2026-06-12_153746_session14_skipbtn_resize           (GamePlayScene.unity — ④ 수정 전)
2026-06-12_154314_session14_handoff                  (HANDOFF.md — 14차 갱신 전)
2026-06-12_160838_session14_nodemap_intro_race       (TutorialGuidePanel.cs — ➕ 수정 전)
2026-06-12_161957_session14_tutorial_victory_randomnode (MapGenerator/NodeSystem/Phases/TutorialManager.cs — ➕➕ 수정 전)
```

---

## 🔍 13차 세션 — 디코 QA 중간점검 접수 + ① 튜토리얼 팝업 버튼 정적 진단 (2026-06-12)

> **수정 0건 세션** — QA 항목 정리와 ① 정적 진단까지만 진행. **Unity MCP 미연결**(ToolSearch 0건)이라 런타임 진단을 못 하고 인수인계. 12차 작업 상세는 하단 §2026-06-11 섹션들 참조.
> ⚠️ **작업 방식(사용자 재확인 2026-06-12)**: "진행하고 할때마다 질문해줘 임의로 하지말고" — **건마다** 진단 결과·수정안 제시 → OK 받고 → 백업 → 수정 → 검증. 여러 건 묶음 일괄 진행 금지. (메모리 feedback_workflow §7에도 반영됨)

### A. 디코 QA 원문 (2026-06-12 중간 점검) — 보존용
- **우선순위 1 (진행에 심각)**: 없음
- **우선순위 2 (영향 O, 진행 가능)**
  - ① 타이틀 "처음이신가요?" 클릭 → 전투 노드 클릭 → 전투 화면 **튜토리얼 팝업 내 버튼 안 눌림**
  - ② **좌측 UI와 현재 상태 상이함**
  - ③ 순간이동 로직 잘못된 듯? (**QA 작성자가 재확인 예정**)
- **우선순위 3 (영향 X)**
  - ④ 튜토리얼 팝업 내 **메인메뉴 버튼 크기·위치** 수정
  - ⑤ **파티 편집 배치 순서 역순** 적용 (파티편집: 왼쪽이 1번 / 전투: 오른쪽이 1번)
  - ⑥ **까마귀 소환 시 허수아비 애니메이션 안 나옴**
- **개선사항**
  - 스킬 툴팁 — (수정 완료) 코스트를 이름 옆·같은 폰트 / (수정 완료) 위력 삭제 / (수정 완료) [공격|광역 적|사거리] 형식. **남은 1건**: ⑦ **상세 설명 폰트 색 조정 + 스킬 이름 밑으로 위치 + 폰트 약간 키움** (+"위력 부분 폰트 진하게 가능하면 좋을 듯" — 위력 줄은 삭제된 상태라 어디를 말하는지 확인 필요)
  - (수정 완료) 용병소 닫기 버튼 빨간 계열
  - ⑧ **게임 오버 시 타이틀 화면으로** (현재: 노드맵 새 런으로 감)

### B. 항목별 상태·다음 행동
| # | 항목 | 상태 | 다음 행동 |
|---|---|---|---|
| ① | 튜토 팝업 버튼 안 눌림 (P2) | **정적 진단 완료 — 파일상 정상, 원인 미특정** | MCP 연결 후 런타임 진단 (§C 절차 그대로) |
| ② | 좌측 UI 상이 (P2) | 의미 불명확 | **사용자에게 구체 상황 재질문** (미답) |
| ③ | 순간이동 의심 (P2) | 보류 | QA측 재확인 대기. 맥락: 12차 §텔레포트 순서(모션 후 배치)·§보스 스킬 애니(Reverse+슬롯재할당+잔상) 개편 직후임 |
| ④ | 튜토 메인메뉴 버튼 크기/위치 (P3) | 구조 파악 완료 | ①과 같은 화면 — 묶어 처리. SkipButton은 씬 오버라이드로 우상단 배치됨 (§C 구조 참조) |
| ⑤ | 파티 순서 역순 (P3) | **방향 결정 대기** | A안=전투 배치 반전(왼쪽=1번, 직관 일치) vs B안=파티편집 표시 반전(오른쪽=1번, 1번=선봉 의미 유지). 전투 아군 x=2,0,-2,-4 — 1번(allies[0])이 x=2로 적측(오른쪽) (12차 검증 기준). 적은 x≈7.75 오른쪽 |
| ⑥ | 까마귀 소환 허수아비 애니 (P3) | 가설 수립 | 12차 보스 검증이 `ExecuteSummonSkill` **직접 호출**이라 시전 모션 경로를 안 탔을 가능성 → 실전 경로 `ExecuteEnemySkillCast`(BattleManager.EnemyAction.cs)에서 Summon 타입일 때 Attack2 트리거(까마귀 부름=Attack2 매핑, 12차 §보스 스킬별 애니) 호출 여부 코드 점검부터 |
| ⑦ | 툴팁 상세설명 색/위치/크기 | 대상 파악 완료 | `UI/SkillTooltip.cs` 구조화 엔트리(12차 §#11 재설계) 수정. 위력 스탯줄은 189행 주석 처리로 이미 삭제 확인. "위력 진하게" 의미는 사용자에게 확인 |
| ⑧ | 게임오버→타이틀 | 경로 파악 완료 + 사전 분석 §D | **사용자 확정 대기** (미답) |

### C. ① 튜토리얼 팝업 버튼 — 정적 진단 상세 (이번 세션 결과, 재조사 불필요)
**증상**: 타이틀 [처음이신가요?] → 노드맵(클릭 **정상**) → 전투 노드 클릭 → CombatIntro 팝업 표시됨 → [다음]/[스킵] 버튼 무반응. P2(진행 가능)인 걸로 보아 모달이 게임 전체를 막지는 않는 듯.

**확인된 구조** (파일 직접 확인 — 그대로 신뢰 가능):
- 팝업 = `Assets/Prefab/UI/TutorialCanvas.prefab` 인스턴스. 씬 부모 = 루트 `Canvas`(fileID 2001003227) — **부모에 CanvasGroup 없음**(간섭 불가)
- 프리팹 루트 TutorialCanvas: Canvas **Overlay sortingOrder=100** + CanvasScaler(**ConstantPixelSize** scale=1) + GraphicRaycaster + TutorialGuidePanel + CanvasGroup(기본 alpha1/inter1/blocks1)
  - └ GuidePanel(Image) — 씬 오버라이드 중앙 900×400. └ MessageText(TMP) / NextButton(1000×100, y=-200, Label "다음")
  - └ SkipButton(루트 직속, Label "스킵") — 씬 오버라이드 pivot(1,1)·anchor(1,1)계 우상단 (씬 GamePlayScene.unity ≈1480~1600행 m_Modifications)
- 배선: `TutorialGuidePanel.cs` Start() 65-66행 리스너(확인=Hide / 메인메뉴=EndTutorial+`SceneTransition.Go("GameStartScene")`). 프리팹 내 nextButton/skipButton 참조 정상
- 표시 시퀀스: `NodeSystem.DispatchByRoomType()` 735행 `TryShowDialogue(CombatIntro)` → popup.Show() → **그 다음** DisplayChanger(SetActive 토글) → `BattleManager.OnEnable()` 218행 `LoadingScreen.Cover()` → BattleLoop [yield→재배치→hold 0.6s→UncoverRoutine(0.4s)]
- 씬 캔버스 정렬: 0×2 / 50 / 80 / **100(튜토 = 씬 내 최상단)**. 런타임 자가생성 오버레이(전부 DDOL): LoadingScreen 10000 / SceneTransition 9999 / BattleResultScreen 9990

**배제된 원인**:
| 가설 | 배제 근거 |
|---|---|
| 버튼 배선 누락 | 프리팹 참조 + Start() 리스너 확인 |
| 다른 캔버스가 위(z) | 튜토=100 씬 최상단. 노드맵 클릭이 됐으므로 SceneTransition 잔존 차단도 아님 |
| 부모 CanvasGroup 간섭 | 부모(루트 Canvas)에 CanvasGroup 없음 |
| LoadingScreen 잔존 차단 | FadeOut 종료 시 blocksRaycasts=false + 자식 이미지 전부 raycastTarget=false(TMP Label 900×90 중앙 띠만 true) |
| DisplayChanger 간섭 | SetActive 반전 토글만, 팝업 캔버스 안 건드림 |
| PanelBase z-개편(#16) 영향 | 팝업은 PanelBase 미상속 + 전투 진입 시 열리는 PanelBase 없음 |

**남은 가설(우선순위순)**:
1. **런타임 가로채기/상태 꼬임** — 정적으로 안 보이는 것. 라이브 RaycastAll 로 특정해야 함
2. **에디터 고DPI 좌표 불일치**(10차 §전투 카드 호버 함정 계열) — 단 튜토 캔버스는 Overlay+ConstantPixelSize라 이론상 무관. **QA 재현 환경(에디터/빌드) 미확인 — 질문해 둔 상태(미답)**
3. Show()가 Start() 전 호출되는 타이밍 류 — CombatIntro는 씬 로드 한참 뒤라 가능성 낮음

**다음 진단 절차(MCP 연결 후 그대로 실행)**:
1. Play → 튜토리얼 재현([처음이신가요?] 경로 또는 TutorialManager.StartTutorial 강제) → 전투 노드 클릭
2. 팝업 표시 상태에서 execute_code: NextButton 스크린좌표에 `PointerEventData` 만들어 `EventSystem.current.RaycastAll()` 덤프 → **최상단 히트가 무엇인지 특정**
3. 동시 점검: LoadingScreen._group.blocksRaycasts 값 / TutorialCanvas CanvasGroup 상태 / EventSystem 활성 InputModule / TutorialGuidePanel.Instance 중복 여부
4. ⚠️ 에디터 비포커스 프레임 정지·`refresh_unity(compile=request)` 함정 — §12차(2026-06-11 까마귀 카운트다운) 참조

### D. ⑧ 게임오버→타이틀 — 사전 분석 메모
- 현재 비튜토 패배: `BattleManager.Phases.cs` 284행 `BattleResultScreen.ShowDefeat(() => StartNextRunLoop())` — 노드맵에서 새 런(마석 유지)
- 참고 패턴: **튜토리얼 보스 전멸 경로**(같은 파일 230~236행)가 이미 `SceneTransition.Go("GameStartScene")` 사용 — 콜백 교체 모델로 쓸 수 있음
- ⚠️ 단순 교체 금지: `StartNextRunLoop()`가 하던 **런 리셋**(파티/덱/노드 진행, 마석 유지)이 타이틀 경유 시 어디서 일어나는지 추적 필요 — `MoveScene.InGameSceneLoaded`는 `ForceReinitParty()`만 호출. 노드 진행(층)/영혼석 등 리셋 누락 가능성 → 코드 추적 후 수정안 제시할 것
- 사용자 확정 필요: 타이틀행 확정 여부 + 리셋 시점(패배 직후 vs 다음 [시작하기] 시)

### E. 사용자 미답 질문 5개 — **다음 세션 첫 질문으로 그대로 사용**
1. Unity 에디터 실행 + **MCP for Unity 연결** 부탁 (이번 세션 내내 미연결)
2. QA ①의 재현 환경: **에디터 플레이였는지 빌드였는지?**
3. ② "좌측 UI와 현재 상태 상이함" — 구체적으로 어떤 화면/어떤 차이?
4. ⑤ 파티 순서 통일 방향: **A안**(전투 반전 — 왼쪽=1번) vs **B안**(파티편집 표시 반전 — 오른쪽=1번)
5. ⑧ 게임오버 → 타이틀 확정 + 런 리셋 시점

### F. 13차 백업
```
~/Documents/backup/2026-06-12_134133_session13_handoff/   (HANDOFF.md — 13차 갱신 전 원본)
```

---

## ✅ 11차 세션 — 상태 호버 수정 + 턴종료 버튼 재배치 (2026-06-10)

### ✅ 상태이상 호버 툴팁 — 전투 카드 수정 (10차 후속)
> 좌패널은 됐지만 전투 카드 호버가 안 됐음. 원인: 신 InputSystem + 에디터 고DPI 에서 **포인터 좌표(≈데스크톱 2868·3381) ≠ 카메라 pixelWidth(1920)** → GraphicRaycaster 카메라 투영이 빗나감(좌패널은 오버레이+CanvasScaler라 정규화돼 정상). (※reversed/`ignoreReversedGraphics=false`·depth 도 짚었으나 진짜 원인은 좌표계 불일치)
> **해결**: `BattleCardView.Update()` 에서 **뷰포트(0~1) 수동 호버** — `Input.mousePosition/Screen`(마우스 VP) vs 칩 `WorldToViewportPoint` 사각형 비교(해상도 불일치 상쇄). `s_hoverOwner` static 으로 카드 간 충돌 방지. activeInputHandler=Both 라 `Input.mousePosition` 사용 가능. **사용자 호버 확인 완료(전투·좌패널 둘 다)**.

### ✅ 턴종료 버튼 재배치 (8차 #7)
> 프리팹 `Assets/Prefab/UI/GamePlayScene_RightMainArea.prefab` 편집(`PrefabUtility.LoadPrefabContents`→`SaveAsPrefabAsset`).
- 상단 바(`Top`) **비활성**. 그 안의 `Turn_Button`(턴종료)+`show_turn`(턴카운터)을 스택 패널(`StatusArea/MyStatus`)로 **reparent**.
- MyStatus엔 **VerticalLayoutGroup**(자식 풀폭·세로배치) → 헤더를 가로로 하려면 **`HeaderRow`(HorizontalLayoutGroup) 신설**(MyStatus SiblingIndex 0, StackBar 1). **최종 B안**: HeaderRow=[스택 Text · Spacer(flexW=1) · show_turn(턴N, 우측·Right정렬)]. **Turn_Button은 `StackBar/Row_3_Boxes` 끝(4번째)** 으로 reparent + LayoutElement(preferredWidth=240, flexW=0, minWidth=200) → 검/방패/하트 3박스(flexW=1)는 남는 폭 균등, 턴종료는 우측 고정. (A안=턴종료도 헤더 우측이었으나 사용자가 B로 변경)
- StatusArea 위로(`pos.y -250→-110`) — 상단 바 있던 자리로 올려 공간 절약.
- **정렬 보정(사용자 요청)**: ① "스택" → 3박스 그룹 위 **중앙**(HeaderRow padding L30/R30·spacing10 으로 박스줄 인셋과 맞춤 + Text flexW=1·중앙정렬), ② "턴N" → 턴종료 버튼 위 **중앙**(show_turn preferredWidth=240=버튼폭·중앙정렬, Spacer 제거), ③ 검/방패/하트 3박스 **1:1:1**(각 LayoutElement preferredWidth=0·minWidth=0·flexW=1 — 콘텐츠 preferred 차이로 270/390/270 이던 것 강제 균등 310/310/310). 검증: 스택중앙=박스그룹중앙(1135), 턴중앙=버튼중앙(1740).
- **박스 내부 아이콘/숫자 분리(사용자 요청)**: 박스가 넓고 낮아(310×58) 아이콘(job 36)+숫자(job_score)가 간격0으로 붙어 있던 것 → `Row_3_Boxes` LE preferredHeight 60→95 + HLG childForceExpandHeight=true(박스가 행 높이 채움), 각 박스 VLG spacing 0→18·padding T6B6 → **아이콘 위 / 숫자 아래 간격 18**. (※턴종료 버튼도 같은 행이라 같은 높이로 커짐 — 행 일관)
- **패널 컴팩트화(후속)**: 박스를 키웠더니 `StatusArea` HLG가 MyStatus를 250-60=190으로 강제 + StackBar **상하 padding 30** + Row minH → StackBar가 부풀어 헤더 눌리고 라벨이 박스 위 ~30px 떠보임. 수정: **StackBar padding T30/B30→T8/B8** + StatusArea 높이로 패널 높이 조절(StatusArea HLG가 MyStatus 높이를 250-60으로 강제하므로 패널 높이는 StatusArea 높이로만 조절). 라벨이 박스 바로 위에 붙음.
- **헤더 텍스트 상하 마진(사용자 요청, 최종)**: 사용자 의도는 "위 스택/턴 텍스트의 **탑 마진**(상단 여백) 부족" 이었음(박스 크기가 아니라). 박스를 키운 건 오해 → **박스 원복**(Row preferredHeight/minHeight=95, 박스 VLG spacing=18, 아이콘 job LE 40×36) + **`MyStatus` VLG padding top=22, bottom=20** 로 라벨 위·박스 아래 여백 확보. **`StatusArea` 높이=260·posY=-115**(여백분 패널 키움, 상단 고정). 결과: 탑마진 ~26 / 하단마진 ~28 / 라벨~박스 ~12 / 박스 95.
  - ⚠️ **스택 패널 높이/마진 조절법(요약)**: StatusArea HLG가 MyStatus 높이를 `StatusArea높이-60`으로 강제 → 패널 키우려면 `StatusArea.sizeDelta.y`↑ + `posY`로 상단 고정(top=posY+높이/2 일정). 내부 상하 여백은 `MyStatus VLG padding top/bottom`. 박스 높이는 `Row_3_Boxes LE preferredHeight`. 박스↔헤더 간격은 `StackBar HLG padding.top`.
- ⚠️ **교훈**: 이 UI들은 Layout Group 제어라 `anchoredPosition`/`sizeDelta` 직접 지정은 무시됨 → `LayoutElement`(preferredWidth/Height)·flexible + 컨테이너(HLG/VLG) 구조로 다뤄야 함. (처음에 직접 좌표로 시도→풀폭 세로로 깨짐)
- 검증: 헤더 가로배치(스택[좌]·턴N·…·턴종료[우]) + 3컬럼 아래, 캡처 확인(가로비 1280×720). 버튼 Button/onClick/`TurnEndButtonController` + `BattleManager.turnDisplayText→show_turn` 참조 유지 확인.
- 백업: `~/Documents/backup/2026-06-10_000922_session11_turnbutton_layout` (prefab+scene).

### ✅ 스킬 이펙트(mp4→스프라이트 애니메이션) 적용 (8차 #3)
> `Assets/Effect/` 10개 mp4를 ffmpeg로 변환해 스킬 시전 시 재생. ffmpeg는 `brew install ffmpeg`(8.1.1)로 설치.
- **변환**: mp4가 H.264(알파 없음, **흰 배경**) → ffmpeg `colorkey=0xFFFFFF:0.14:0.10` 로 흰배경 제거 → 알파 프레임 PNG → `Assets/Resources/SkillFX/{key}/f_##.png` (총 183프레임, Sprite/알파/Bilinear). 명령: `ffmpeg -i "Effect/{이름}.mp4" -vf "colorkey=0xFFFFFF:0.14:0.10" "Resources/SkillFX/{key}/f_%02d.png"`.
- **매핑**(skillId→폴더키): fireball=skill_fireball · magic_missile=skill_magic_missile · reckless_strike=skill_reckless · iaido=skill_draw · flash_slash=skill_flash · defense_ready=skill_guard · battle_stance=skill_battle_stance · indomitable=skill_indomitable · prayer=skill_prayer · star_call=skill_starlight.
- **코드**: 신규 `UI/SkillEffectPlayer.cs`(SpriteRenderer 프레임 순차 재생 + 투사체면 from→to 이동, 후 자동 파괴) + `UI/SkillEffectFx.cs`(skillId→key/fps/높이/**종류** 레지스트리 + 캐시 + Play(casterPos,targetPos)). `BattleManager.Combat.UseSkill` 의 impactDelay 후 훅: `SkillEffectFx.Play(skill.id, GetUnitFxPos(user), GetPrimaryEnemyFxPos(user))`. 위치 헬퍼는 BattleCardView `.Fellow`/`.Enemy` 매칭.
- **이펙트 종류 분류(FxKind, 이펙트 모양 보고 판단)**: **Projectile(시전자→적 좌→우 비행)**=파이어볼·매직미사일 / **AtTarget(적 위치 고정)**=발도·일섬·무모한강타·별부름(검호/일격/강하) / **AtCaster(시전자 위치)**=방어준비·전투태세·불굴·기원(원형 오라/광휘 버프·힐).
- **검증**: 컴파일 0. Play+전투 직접 호출 — 투사체(파이어볼)=시전자(-2.5)→적(7.75) 비행(_travel=true, 중간위치 렌더 확인 `Temp/fx_projectile.png`) / 타격(발도)=적 위치 / 버프(전투태세)=시전자 위치, 알파 투명 렌더 확인. ⚠️ 실제 스킬 시전 훅은 MCP 프레임정지로 코루틴(WaitForSeconds) 타이밍 검증 불가 — 컴포넌트·매핑·라우팅·스폰은 검증됨, **실전 전투에서 사용자 확인 필요**.
- 튜닝 여지(`SkillEffectFx` 레지스트리): 이펙트별 fps·worldHeight, 스폰 위치 오프셋(현재 캐릭터 transform 그대로 — 몸 중앙 +Y 오프셋 가능), AOE는 현재 첫 적에만(전체/중앙 확장 가능). 매핑 없는 스킬(전장의방패·워크라이·매직실드·아이스스톰 등)은 이펙트 없음.
- 백업: `~/Documents/backup/2026-06-10_015115_session11_skillfx` (Combat.cs 변경 전). 원본 mp4는 `Assets/Effect/` 유지.

---

## ✅ 10차 세션 — 노드 아이콘 검증 + 옛 아이콘 정리 + 상태이상 표시 UI (2026-06-09)

> (A) 9차의 미검증 노드 아이콘 검증 완료 + 옛 아이콘 삭제. (B) 8차 미처리 #6 **상태이상 표시 UI** 아이콘화 완성.

### ✅ 노드 아이콘 검증 통과 (4단계)
- **컴파일**: `NodeSystem.cs` 에러 0 (555행 `enableWordWrapping` obsolete 경고만 — 노드 아이콘과 무관, 기존부터 있던 TMP API 경고).
- **임포트/로드**: `Resources/Icons/node_{start,combat,event,rest,boss}.png` 5종 전부 `textureType=Sprite, spriteImportMode=Single, filterMode=Point` + `Resources.Load<Sprite>` 성공(14×14·14×14·13×13·14×16·15×16). → **9차에 손수 작성한 Single .meta 정상 작동, 재임포트 불필요**.
- **런타임(Play)**: MVP맵 6노드(시작/전투/Event/전투/화톳불/보스) 전부 `img.sprite=흰원(런타임 생성)`+타입색 틴트(시작 파랑/전투 흰/Event 노랑/화톳불 빨강/보스 보라) + `TypeIcon` 자식에 올바른 `node_*` 스프라이트 적용·enabled 확인(데이터 레벨).
- **시각(캡처)**: 캔버스 임시 ScreenSpaceCamera→RenderTexture 기법(HANDOFF §262)으로 노드맵 + 5아이콘 몽타주 캡처. 깃발/십자검/물음표(원안)/모닥불/해골 전부 정상 렌더 — 흰색+검은 외곽선 1-bit이라 밝은 원·컬러 원 모두에서 식별됨. (`Temp/nodemap_verify.png`, `Temp/node_icons_montage.png`)
- (선택) `node_event`는 "원 안 물음표"라 노드 원과 원-속-원이 되지만 식별 문제없음 → **유지**. 더 깔끔히 원하면 상자(`Tools_Crafting_Chest_Locked_Loot`) 등으로 교체 가능.

### ✅ 옛 아이콘 정리 — 삭제 완료
- `Green_icon.png` / `Random_node_icon.png` / `Fire_camp_icon.png` (Resources/Icons): **코드 문자열 참조 0 + 씬/프리팹/에셋/머티리얼 GUID 참조 0** = 완전 데드 확인 → `manage_asset delete`로 삭제(.meta 동반). 삭제 후 node_* 5종 로드 정상, 콘솔 스크립트/에셋 에러 0.
- 백업: `~/Documents/backup/2026-06-09_213819_session10_old_node_icons_DELETE` (삭제 직전 9차수정본 png+meta). 9차이전 원본은 `..._194954_session9_node_icon_pngs`.

### ✅ 상태이상 표시 UI — 아이콘화 완성 (8차 미처리 #6)
> 기획 §04 상태이상. **로직은 기존 구현 완료**(BattleManager.Combat: 패닉/압박/중증디버프). 표시만 빠졌던 것.

**진단**: 기존 칩은 `dotTurnsLeft`/`FromStress(stress)` 만 봐서 **공포경직(isFrozen)·과호흡(isOverBreathing)·중증디버프(hasSevereDebuff) 표기 0**. 패닉도 stress 100→즉시 50 드롭이라 칩 실질 미표시. (※중증디버프는 기획 §04엔 백로그지만 코드는 활성 — 딜러 받피+30%/탱커 실드-50%/서폿 광역→단일)

**구현** (사용자 결정: 전부 표시 / 여러 아이콘 동시 / 텍스트→아이콘+턴수 / 전투카드+좌패널 둘 다):
- `FellowData.cs`: isFrozen·isOverBreathing·hasSevereDebuff **bool→프로퍼티**화 + `[NonSerialized] OnStatusChanged` 이벤트(값 변경 시 발행). 로직·호출부(`ally.isFrozen=true` 등) 불변 — UI 갱신 신호만. (패닉 시 currentStress=50 이벤트가 플래그 set보다 먼저 발생→칩 갱신 트리거 없던 문제 해결)
- `StatusVisual.cs`: enum +Frozen/OverBreathing/SevereDebuff. `IconOf(kind)` 아이콘 로더(`Resources/Icons/status_*`, 캐시) + 색/라벨.
- `BattleCardView.cs`: 단일 텍스트칩 → **다중 아이콘 칩 행**(HP 아래, HorizontalLayoutGroup). 겹친 상태 모두 + 칩 우하단 턴수. 표시순: 경직·과호흡·중독·압박/패닉·중증. OnStatusChanged 구독 추가.
  - ⚠️ **중첩 스케일 함정**(다음 세션 주의): 전투 카드 HP텍스트는 캐릭터별 **월드 캔버스에서 localScale 0.01**. 그래서 StatusRow를 **HP텍스트의 자식**으로 붙여 0.01 스케일 상속시키고, 크기/오프셋을 HP `fontSize`(24) 기준으로 잡음(`_chipSize=fs*0.85`, anchoredPos.y=`-fs*0.75`). 처음에 캔버스 자식+캔버스단위 오프셋(-50)으로 화면 밖으로 날아갔던 버그를 이렇게 수정.
- `CardSlotView.cs`: 좌패널 카드 **우상단**에 동일 아이콘 행(36px). 기존 HP/스트레스의 "중독"/"압박" 텍스트 라벨 제거(아이콘 대체), HP·스트레스 숫자는 유지. OnStatusChanged 구독.
- **호버 툴팁(후속 요청)**: `StatusVisual.DescOf/TooltipText` + `SkillTooltipController.ShowText`(범용 텍스트) + `StatusTooltipTrigger`(SkillTooltip.cs). 칩 호버 시 "**라벨 — 설명 (N턴 남음)**"(턴 0이면 이름+설명만) 공용 툴팁 표시. 칩 bg `raycastTarget=true`, 전투 월드 캔버스엔 `GraphicRaycaster` 자동 추가(worldCamera=Main) + **`ignoreReversedGraphics=false`**. 아이콘 크기 ↑(전투 `fs×0.85→1.0` / 좌패널 `30→36`). 검증: 4상태 칩 OnPointerEnter 시뮬→패널 활성+본문 정확(경직"1턴 남음"/압박 턴없음 등). ⚠️툴팁 패널 시각 캡처는 오프스크린 renderMode 전환이 스크린좌표 패널을 깨서 미확인 — 실게임 호버에선 정상.
  - ⚠️ **전투 카드 호버 함정(중요)**: 처음엔 좌패널만 호버 되고 전투는 안 됐음.
    - 1차 원인: 캐릭터 월드 캔버스가 카메라(전방 +Z)와 같은 +Z를 향해 **reversed**로 판정 → GraphicRaycaster 기본값(`ignoreReversedGraphics=true`)이 무시. → `false`로 설정(EnsureStatusRow). (depth=-1은 MCP 프레임정지 아티팩트, 실프레임/`cam.Render()` 후 정상)
    - 2차(진짜) 원인: 신 InputSystem + 에디터 고DPI 에서 **포인터 좌표계(≈데스크톱 2868·3381)와 카메라 pixelWidth(1920)가 불일치** → GraphicRaycaster의 카메라 투영이 어긋나 보이는 칩을 못 맞힘(픽셀 비교 실패). 좌패널은 오버레이+CanvasScaler라 자동 정규화돼 정상.
    - **최종 해결**: `BattleCardView.Update()` 에서 **뷰포트(0~1) 공간 수동 호버** — `mvp = Input.mousePosition/Screen` vs 칩 `WorldToViewportPoint` 사각형 비교(해상도 불일치 상쇄). `s_hoverOwner` static 으로 카드 간 충돌 방지. 좌패널은 기존 EventSystem 트리거 유지. (GraphicRaycaster/트리거는 잔존하나 전투에선 수동 호버가 실동작). 칩 VP 사각형·중앙적중 검증 완료 + **사용자 실제 호버 확인 완료(전투·좌패널 둘 다 정상, 2026-06-09)**.

**아이콘** (1-bit 팩 `Sprites_Cropped` → `Resources/Icons/status_*.png`, Single/Point, 7종):
| 상태 | status_*.png | 1-bit 원본 |
|---|---|---|
| 중독 | status_poison | Misc_Poison_Venom_Skull_Drop_Death |
| 압박 | status_pressure | Emoji_Face_Sad_Frown |
| 공포경직 | status_frozen | RPG_Debuff_Stunned_Disabled_CC |
| 과호흡 | status_overbreath | Misc_Organ_Lungs_Breathing_Breath |
| 중증디버프 | status_severe | RPG_Spell_Curse_Pentagram |
| 도발(적) | status_taunt | RPG_Buff_Enraged_..._Taunt |
| 패닉(찰나) | status_panic | Emoji_Face_Surprised_Shocked |

**검증**(Play→`OnNodeClicked(1,0)` 전투 강제→아군 allies[0]에 중독2/압박/경직/중증 강제→캡처): 전투 카드 HP 아래 4아이콘+턴수("2") 정상, 좌패널 카드 우상단 아이콘 정상. 컴파일 0에러. (`Temp/final_battle.png`, `Temp/final_leftpanel.png`)
> ⚠️ 좌패널 파티 카드는 ScrollRect의 RectMask2D+Mask 안이라 **오프스크린 RenderTexture 캡처엔 콘텐츠가 안 잡힘**(스텐실 한계, 버그 아님) — 실게임/에디터 게임뷰에선 정상. 캡처 검증 시 마스크 임시 비활성으로 확인함.
> 미세조정 여지: 칩 크기/오프셋(코드 상수), 공포경직/과호흡 turns=1 표기 여부, 압박/중증 아이콘 교체(교체 후보 §대화 참조).

### 백업 (~/Documents/backup/, 10차)
```
2026-06-09_213819_session10_old_node_icons_DELETE  (Green/Random/Fire png+meta — 삭제 직전본)
2026-06-09_214310_session10_handoff                (HANDOFF.md.bak, A단계)
2026-06-09_221347_session10_status_display         (FellowData/StatusVisual/BattleCardView/CardSlotView 변경 전 .cs)
2026-06-09_225302_session10_status_handoff         (HANDOFF.md.bak, B단계)
2026-06-09_230706_session10_status_tooltip         (StatusVisual/SkillTooltip/BattleCardView/CardSlotView — 호버 툴팁·크기 작업 전)
```

---

## 🟡 9차 세션 — 스택/카드/노드 아이콘 (2026-06-09)

> 앞 3건(스택카드·스택헤더·카드숫자)은 검증 완료(컴파일 0 + 몽타주). 노드 아이콘 교체는 **10차 세션에 검증 완료**(§10차 세션).

### ✅ 완료·검증됨
| 항목 | 변경 |
|---|---|
| #5 스택카드 모양 통일 | `StackCardController.cs` `RoleCardSprite` 딜 `sprite_sheet_15`(빨강+다이아 문양)→**`sprite_sheet_8`**(빨강 민무늬). 탱9/힐14 유지 → 3장 동일 모양·색만 다름 |
| #4 스택창 헤더→아이콘 | `GamePlayScene_RightMainArea.prefab` `StatusArea/MyStatus/StackBar/Row_3_Boxes/Box_{Attack,Tank,Support}/job`: TMP 텍스트 비움 + LayoutElement(40×36) + **RoleIcon Image 자식**(공격=검24/방어=방패31/지원=하트32). job_score(숫자)는 유지 |
| 카드 디자인 보정 | `StackCardController.cs:203` numberText `fontSize 96→54`. 프리팹 4장: CardStack(숫자) `y -122→-140`(정중앙), RoleType(아이콘) `y -30→-52`(아래로), SubText(설명) `y +30→+52`(위로) — 요소를 카드 테두리 안으로 |

### ✅ 노드 아이콘 교체 — 코드/파일 적용 + **10차 검증 완료** (§10차)
`NodeSystem.cs` 구조 변경 (런타임 only, 프리팹/씬 안 건드림 → 코드 되돌리면 원복):
- **겉 = 원형 컬러 버튼**: `CircleFrameSprite()`가 런타임에 흰 원 텍스처 생성(빌트인 Knob이 Unity6에서 null → 직접 생성). `UpdateNodeStates`: `btn.Image.sprite=흰원` + 타입색 틴트(`baseColor`, 시작은 `StartFrameColor` 파랑) + `preserveAspect`.
- **안 = 타입 아이콘**: `NodeInnerIconFor()` → `IconSprite("node_*")` (Resources/Icons 단독 PNG 로더, 캐시).
- 옛 `NodeMarkerFor`(도형 마커) + `MarkerSprite`(sprite_sheet 로더) **제거**(데드코드). `EnsureNodeTypeIcon`(TypeIcon 자식)은 안쪽 슬롯으로 재사용.

**1-bit 아이콘** (`Assets/1-bit_Pixel_Icons/Sprites_Cropped/` → `Assets/Resources/Icons/`로 복사 + **손수 작성한 Single .meta**, Point 필터, 새 guid):
| 노드 | Resources 파일 | 1-bit 팩 원본 |
|---|---|---|
| 시작 | `node_start.png` (14×14) | Map_Markers_Flagpole (깃발) |
| 전투 | `node_combat.png` (14×14) | RPG_Crossed_Swords_..._Combat_Battle_War (십자검) |
| Event(랜덤) | `node_event.png` (13×13) | Software_..._Question_Mark_Help (물음표) |
| 화톳불 | `node_rest.png` (14×16) | Weather_Campfire_Camping_Site_Rest (모닥불) |
| 보스 | `node_boss.png` (15×16) | RPG_Skull_Death_Dead_Bones_Pirates (해골) |

> **설계 검증됨(중요)**: 엘리트·용병소·교회는 **별도 노드가 아님** — `RoomType.Event` 1개 노드의 랜덤 결과(`NodeSystem.RollEventOutcome`, `EventOutcomeWeights={100,0,0}`=현재 용병소만). 그래서 Event엔 특정 결과 대신 **물음표**. MVP맵(`MapGenerator.cs:49-58`): 시작/전투/Event/전투/화톳불/보스 6노드 고정.

### ✅ 노드 아이콘 검증 — 10차 완료 (상세 §10차 세션)
1. ✅ NodeSystem.cs 컴파일 에러 0.
2. ✅ Play 캡처 — MVP 6노드 색 원 + 1-bit 아이콘(깃발/십자검/물음표/모닥불/해골) 정상.
3. ✅ 재임포트 불필요 — 5종 이미 Single 임포트 + `Resources.Load<Sprite>` 성공(빈 원 아님).
4. (선택) `node_event` 원-속-원 — 식별 문제없어 유지. 더 깔끔히 원하면 상자(`Tools_Crafting_Chest_Locked_Loot`) 등으로 교체.

### ✅ 옛 아이콘 정리 — 10차 삭제 완료
- 옛 `Green_icon`/`Random_node_icon`/`Fire_camp_icon`(Resources/Icons): 참조 0 데드 확인 → **삭제 완료**(§10차). 백업 `213819` + 9차이전 원본 `194954`.

### 백업 (~/Documents/backup/, 9차)
```
2026-06-09_183712_session9_stackcard_header   (StackCardController.cs + prefab)
2026-06-09_185315_session9_card_layout
2026-06-09_185611_session9_card_number_center
2026-06-09_192455_session9_node_icons         (NodeSystem.cs 노드아이콘 변경 전)
2026-06-09_194954_session9_node_icon_pngs      (Green/Random/Fire 원본)
2026-06-09_212018_session9_handoff
```

### 남은 8차 미처리 (1건) — #6 상태이상(10차), 턴종료 재배치·스킬 mp4 이펙트(11차) 완료
Foozle 전면 적용 (에셋 세트로 패널/버튼 스킨 교체 — 적용 범위 협의 필요).

---

## ✅ 8차 세션 — UI 소작업 (2026-06-09)

> 컴파일 0에러. ⚠️ 교훈: UI 변경은 한 번에 하나씩 + 매번 캡쳐 검증해야 함(이번에 가설 다중변경 후 사용자가 깨진 화면 발견). 캡쳐: `ScreenCapture.CaptureScreenshot(Temp/xxx.png)` 를 execute_code 로.

### 살아남은 변경 (4건)
| 항목 | 파일 / 변경 |
|---|---|
| 스택카드 숫자 강조 | `StackCardController.SetupCard` — numberText `fontStyle=Bold`·`color=black`·`fontSize=96`·`enableAutoSizing=false` (기존 양수초록/음수빨강 분기 제거). descText `fontSize=18`·autoSize off. |
| 좌패널 토글 hover | `LeftPanelToggle` — `hoverRevealRadius=180`(px, 0이면 항상 노출)·`hoverFadeDuration=0.15`. Update 에서 차단게이트 우선 → 비차단 시 마우스가 탭 중심 180px 내일 때만 alpha 1 fade. `IsMouseNearTab()` 신규. |
| 하단 버튼 3개 복원·확대 | LeftPanel.prefab(씬 인스턴스) PartyEdit/Setting/Log RectTransform: anchor (0,0)~(1,0) stretch, pivot (0.5,0), sizeDelta (0,55), anchoredPosition y=165/105/45. 좌우 마진 0(흰 배경 최소화)·높이 55. |
| Panel_1.png 9-slice | `.meta` per-sprite border + importer spriteBorder `{0,0,0,0}`→`{60,60,60,60}`. force reimport. ⚠️ **Panel_1.png 를 쓰는 다른 UI(있다면) 에도 영향** — 모니터링 필요. |

### ⛔ 원복된 시도 (LeftPanel ornament 노출 — 실패)
- LeftPanel VerticalLayoutGroup `padding` 추가 → 하단버튼이 패널 밖으로 삐져나옴 → **원복(0,0,0,0)**
- Scroll View / 하단 Image 의 Image `color.a=0` → 가운데 검은 아코디언 박스 노출 → **원복(a=1)**
- LeftPanel Image `color` 0.14/0.16/0.20 → **원복(0.05/0.06/0.07, 8차 이전 다크 톤)**
- **결론: LeftPanel 은 손대지 말 것(사용자 지시). ornament 노출은 별도 디자인 협의 후 sprite 교체 등으로 재접근.**

### 백업
```
~/Documents/backup/2026-06-09_173527_session8_smallfixes/      (StackCard/LeftPanelToggle/LeftPanel.prefab)
~/Documents/backup/2026-06-09_174654_panel1_border/            (Panel_1.png.meta 원본 border 24)
~/Documents/backup/2026-06-09_175813_leftpanel_image_visibility/ (Panel_1.meta + GamePlayScene — 원복 기준점)
~/Documents/backup/2026-06-09_180903_handoff_session8/         (HANDOFF.md.bak)
```

### 8차 미처리 — 사용자 요청 11항목 중 7건 (다음 세션)
> 사용자 원본 요청(전투/노드 UI 대개편). 이번엔 "주은 작은 건들" 4개만 처리. 나머지:
1. **노드 마커 아이콘 제거** — 노드맵 다이아몬드/원 마커. NodeSystem 직접코드 없음 → prefab/씬 NodeButton child sprite 추정. 위치 재확인 필요.
2. **Foozle UI 전면 적용** — `Assets/Foozle_UI_0001_RPG_Set_1/`(~35 PNG: Panel_1/2·Button·Main_Button·Accept/Decline 등). 노드마커용 다이아몬드는 없음. 적용 범위 사용자 협의 필요.
3. **스킬 이펙트 (mp4)** — `Assets/Effect/` 에 10개 .mp4(기원/매직미사일/무모한강타/발도/방어준비/별부름/불굴/일섬/전투태세/파이어볼). **ffmpeg 로 스프라이트시트 변환 후** SpriteAnimation 적용 결정(별도 세션). VideoPlayer 직접은 보류.
4. **공격/방어/지원 텍스트 → 아이콘** — 스택창. (스택카드 roleText 는 7차에 이미 아이콘화 — sprite_sheet_24검/31방패/32하트. 스택창 헤더는 미적용)
5. **스택카드 모양 통일** — 딜=sprite_sheet_15 / 탱=9 / 힐=14 로 **서로 다른 인덱스**. "색만 다른 똑같은 모양" 으로 통일 요청 → 같은 프레임 + 색만 변경하게 수정 필요.
6. ✅ **상태이상 표시 UI** — **10차 세션 완료**(§10차). 공포경직/과호흡/중증디버프 + 패닉칩을 아이콘+턴수로 표시(전투카드 HP아래 다중칩 + 좌패널 카드 우상단). status_*.png 7종.
7. **턴 종료 버튼 위치** — 현재 배틀 진입 시 최상단. 스택 표기 표 안으로 넣어 가로정렬 + 스택창을 위로 올려 공간 절약 요청. 배틀 UI 재배치 작업.

---

## ✅ 7차 세션 완료 — 작은+중간 작업 묶음 (2026-06-09)

> 백업 6건, 모두 컴파일 0에러. Play 검증은 다음 세션 또는 사용자 직접.

| # | 작업 | 변경 파일 / 핵심 |
|---|---|---|
| 1 | 동료 스킬 스프라이트 Loader | `Fellow_Skill/SkillData.cs` `[NonSerialized] Sprite sprite` 추가 + `SkillDatabase.LoadSpritesForSkills()` 부팅 시 1회 Resources.Load. JSON `spritePath` 비면 skip, 누락 시 경고. |
| 2 | 적 스킬 스프라이트 Loader | 1번과 동일 패턴 — `Enemy_Skill/EnemySkillData.cs` + `EnemySkillDatabase.LoadSpritesForSkills()`. Fellow/Enemy 일관 패턴. |
| 3 | 노드 타입별 마석 차등 보상 | `BattleManager.Phases.GrantBattleReward` 재작성. `RoomType` switch: **Combat 10 / Elite 20 / Boss 30**(폴백 10). 영혼석은 적 처치 즉시(기존 동작 유지). |
| 4 | 마석 UI 노출 결정 | 코드 변경 X — **현행 LeftPanel 표시 유지** 결정. 기획 백로그 §53 "MVP 숨김/노출" 미결을 "노출 유지" 로 사용자 확정. 문서 갱신은 보류. |
| 5 | DamagePopup AOE cascade | `DamagePopup.Show(..., float startDelay)` 추가 (지연 동안 alpha=0). `BattleCardView.SpawnPopup` 에 정적 stagger 트래커 — 0.15s 윈도우 내 연속 스폰은 인덱스 누적, 0.05s 씩 지연, 최대 0.3s 캡. AOE 시 좌→우 cascade. |
| 7 | 보스 K·Teleport 비가시 연출 | `BattleCardSprites.PlayTeleport(fadeDur=0.3, waitDur=0.2)` 추가(기존 `BuildFadeTween/CacheOriginalColors` 재활용, OnKill 색 복원). `BattleManager.EnemyAction.ExecuteTeleportSkill` TODO 제거 → `FindCardSprites(caster)` 헬퍼로 보스 카드 찾아 호출. allies.Reverse 는 기존대로 유지(타게팅 우선순위만 반전). |
| 8 | 영혼석 드롭 Pool/Fx — **코드만** | 신규 `Currency/SoulstoneDropFx.cs`(pop→dwell 0.5s→tween 0.4s→onArrive 콜백→SetActive false) + `Currency/SoulstoneDropPool.cs`(싱글톤·prewarm·lazy 확장). `BattleManager.Combat.ProcessDeath` 에서 `SoulstoneDropPool.Instance?.SpawnAt(worldPos, drop)` 호출, **Pool null·prefab null·target null 모두 즉시 Add 폴백**. `LeftPanelView.soulstoneIconTransform` 인스펙터 슬롯 추가(이미 Item_SoulStone 연결). prefab/씬 Pool GameObject 는 **생성 안 함** — 스프라이트 자산 준비되면 사용자가 prefab 1개 만들고 Pool 연결 시 자동 활성화. |

### 7차 세션 발견·이슈
- ⚠️ **사고 회복**: 8번 진행 중 임시 Sphere prefab + 씬 Pool GameObject 까지 만들었다가 **씬 정중앙에 흰 Sphere 가 노출**되는 placeholder-in-production 사고 발생. 사용자가 Play 시 발견 → 즉시 모두 삭제(Sphere prefab `Assets/Prefab/UI/SoulstoneDropFx.prefab`, 씬 `SoulstoneDropPool` GameObject, 잔여 `SoulstoneDropFx_TEMP` 인스턴스). 교훈: 그래픽 자산 미정 작업은 **코드만 + null 폴백** 으로 마무리. prefab/씬 인스턴스 까지 만들지 않는다.
- ⚠️ MCP `manage_gameobject delete` 가 instanceID 가 변경된 시점에 호출되면 silent 실패 → 씬에 잔재 → `find_gameobjects by_component MeshFilter` 로 재탐색해서 발견하는 패턴 학습.

### 백업
```
~/Documents/backup/2026-06-09_000949_skill_sprite_loader/         (1번 — Fellow_Skill/*)
~/Documents/backup/2026-06-09_001256_enemy_skill_sprite_loader/   (2번 — Enemy_Skill/*)
~/Documents/backup/2026-06-09_001802_battle_reward_tiering/       (3번 — Phases.cs)
~/Documents/backup/2026-06-09_002500_damage_popup_aoe_stagger/    (5번 — DamagePopup/BattleCardView)
~/Documents/backup/2026-06-09_003229_boss_teleport_fx/             (7번 — BattleCardSprites/EnemyAction)
~/Documents/backup/2026-06-09_003653_soulstone_drop_fx/            (8번 — Combat/LeftPanelView)
~/Documents/backup/2026-06-09_172111_handoff_session7/             (HANDOFF.md.bak)
```

### Play 검증 체크리스트 (7차)
- [ ] 노드 클리어 마석: 일반 +10 / 엘리트 +20 / 보스 +30
- [ ] AOE(파이어볼 등) 시 적 카드들 데미지 팝업 좌→우 cascade (0.05s 간격, 최대 0.3s 캡)
- [ ] 보스 K·Teleport: 까마귀 만료 다음 보스 턴 — 보스 카드 fade out → 0.2s 비가시 → fade in (총 0.8s)
- [ ] 영혼석 드롭: 적 처치 시 숫자 즉시 +`soulstoneDrop` (시각 연출 없음, 폴백 동작)
- [ ] 마석 LeftPanel 표시 정상 (현행 유지)
- [ ] 화면 정중앙에 흰 Sphere 등 placeholder 잔재 0개

### 7차 미진행
- 6번 튜토리얼 시스템 코드 — **기획 §15 매우 상세하나 코드 0**. 반나절+ 작업. 별도 세션 권장
- 9번 합성 UI 검증 — `TrySynthesize`/`GrowthPanel`/`FellowSourcePickerPopup` 코드 완비, Play 모드에서 직접 검증 필요
- 10번 교회 노드 / 11번 마석 사용처 메타 / 12번 DoT 사후 도트뎀 / 13번 디버프 — 각 큼 작업
- 영혼석 스프라이트 자산 준비 후 prefab + Pool 연결 (그래픽 작업 동반)

---

## 🎯 NEXT — 2차 피드백 12항목 (2026-06-05) ★현재 진행

> 사용자 2차 버그/개선 리스트. **#7만 이번에 수정 완료**, 나머지는 백로그.

| # | 항목 | 상태 / 메모 |
|---|---|---|
| 1 | 파티 편집 오타 | ⬜ 오타 위치 찾아 수정 (어느 텍스트인지 확인 필요) |
| 2 | 좌패널 스킬 팝업이 "지점"이 아니라 "영역"으로 떠야 함 | ✅ **수정 완료** — CardSlotView가 트리거를 스킬 항목 컨테이너(Skill1/Skill2)에 부착, SkillTooltipTrigger.Ensure가 투명 raycast Image를 보장 → 칸 전체에서 호버. |
| 3 | 노드 화면 포커싱이 현재 노드 단계(라인)로 자동 스크롤돼야 함 | ✅ **수정 완료** — NodeSystem.UpdateNodeStates 끝에 FocusCurrentRow() 추가. NodeDisplay ScrollRect를 현재 행이 viewport 중앙에 오도록 스크롤(끝단 클램프). 스크롤뷰 아니면 무시. |
| 4 | 파티편집 후 전투 진입 시 초기 정렬 안 됨 → 턴 종료 눌러야 재정렬 | ✅ **수정 완료** — DefaultSetting.SpawnObject가 스폰 직후 아군 battleSlotIndex를 allies 순서로 확정한 뒤 RelayoutCards(instant). GetActiveFellows 스탬프 타이밍/빈슬롯으로 초기 relayout이 스킵되던 문제 해소. |
| 5 | 이벤트 노드 색상 제거(랜덤이라) + **시퀀스 고정**: 시작>전투>용병소>전투>화톳불>보스 | ⬜ MapGenerator 일반 맵 재구성(고정 시퀀스) + 노드 색상 제거 |
| 6 | 전투 중 아군 사망했는데 손패 안 줄어듦 | 🟡 **부분 수정** — 사망 1회 처리(deathHandled)로 RemoveCardsOfFellow 중복 호출 정리, ProcessPendingDiscard에 인게임 로그 추가. 체인은 정상이나 "랜덤 파괴 아예 안 됨"은 런타임 repro 필요(드로우 리필/marked 비어있음 가능성). 사양: 사망 동료 카드가 손패에 없으면 다음 턴 랜덤 1장 파괴. |
| 7 | 미행동 보상 로그/애니메이션 없음 + 스택 인원보다 적게 들어감 | ✅ **수정 완료** (아래 §2차-#7) |
| 8 | 게임 내부 텍스트 특수문자 금지 | ✅ **수정 완료** — enemy_skills.json 설명 8개 재작성(개발메모 (기획 §…)·TryGetForcedSkill·weight 0 제거 + §/≤/·/→/— 치환), MetaPassive 해금설명 "— 스킬 풀에 추가"→"(스킬 풀에 추가)", 튜토리얼 대사 "[X] — Y"→"[X]: Y"·"직업·성급"→"직업, 성급". 데이터/씬/프리팹 표시텍스트 장식 특수문자 0 확인. (정상 부호 +·-·/·%·()·[]·: 및 CollapseTab ◀▶ 화살표는 유지) |
| 9 | 아군 사망 시 스트레스 반복 상승 → 사망당 1회만 | ✅ **수정 완료** — FellowData.deathHandled 플래그 추가, ProcessDeathAndStress가 새 사망만 처리(매 턴 재적용 방지), InitBattle 리셋. |
| 10 | 프리스트 스킬 시 옷 색상 바뀜 | ⬜ BattleCardSprites 색 복원/틴트 점검 — 다만 Priest_Attack 스프라이트 아트가 다른 색일 가능성(아트 이슈). repro 필요. |
| 11 | 패시브 해금이 마석상점 아니라 **영혼석 기반**이어야 함 | ✅ **수정 완료** — MetaPassiveManager.TryUnlock이 SoulstoneManager.Amount/Use 사용(영혼석). ※스킬 해금도 동일하게 영혼석. 마석은 현재 미사용 — 의도면 OK, 아니면 알려주세요. |
| 12 | 좌패널 파티창 텍스트 밑 잘림 + 특수문자 제거 | ✅ **수정 완료** — 원인: 스킬 이름 폰트 24 > 스킬 칸 30px(밑 잘림). CardSlotView가 스킬이름/이름/성향 텍스트에 오토사이즈(ApplyAutoFit) 적용해 칸에 맞춰 축소. 특수문자는 #8에서 정리됨(카드 표시 텍스트 깨끗). |

### §2차-#7 미행동 보상 (2026-06-05 수정 완료)
- **원인 1(로그 없음)**: 미행동 보상이 `Debug.Log`(에디터 콘솔)만 하고 `GameLog.Event`(인게임 로그)가 없었음 → 로그 팝업·연출에 안 뜸.
- **원인 2(스택 인원보다 적음)**: `skills.Count==0`(보유 스킬 0)인 동료는 carryover 없이 조용히 `continue` → 미행동 3명인데 스택 +2처럼 1명 누락. (스택 상한 cap은 없음 — `RoleCostBase.Add` clamp만.)
- **수정**(`BattleManager.Combat.cs` ExecuteAction): 스킬 부족/스킬 없음 **모두 미행동 보상(+1)으로 통일** + 미행동 시 `GameLog.Event("…행동하지 않아 다음 턴 스택 +1")` 추가. carryover는 여전히 역할(StackType)별 누적(기획 "해당 스택 +1").
- ※후속: "보유 스킬 없음" 경고가 실제로 찍히면 스킬 배정(PickSkillsFromPool) 쪽 별도 점검 필요. 미행동 "애니메이션"은 재배치 카드 이동으로 일부 표현되나 전용 연출은 백로그.
- 백업: `~/Documents/backup/2026-06-05_noaction_log_fix/`.

---

## ✅ 6차 세션 완료 — 기획자 피드백 15항목 (2026-06-05)

> 백업: `~/Documents/backup/2026-06-05_planner_feedback_15/` (Scripts 전체 + 두 씬 + 프리팹 + Data).
> 컴파일 0에러, Play 런타임 0에러 검증. 좌측패널 한글·팔레트 스크린샷 육안 확인.

| # | 결과 |
|---|---|
| 4 | ✅ 로그 스크롤 — `Canvas/LogPopup/.../Content` VerticalLayoutGroup `childControlHeight=false`가 원인. `true`+`childForceExpandHeight=false`로 수정(텍스트만큼 Content 성장). 씬 인스턴스 오버라이드. |
| 10 | ✅ 노드 클릭 가드 — `NodeSystem.OnNodeClicked` 진입에 `IsAnyBlockingPanelOpen()`(PanelBase alpha>0.5) 가드. 열린 팝업 위 클릭 무시. |
| 12 | ✅ 사망 즉시 파티 재빌드 — `LeftPanelView`가 멤버별 `OnDied` 구독→`Refresh()`, 그리고 Refresh가 `!isDead` 필터로 즉시 압축. |
| 13 | ✅ 패배 엔딩 텍스트 — `BattleManager.Phases.ShowEndingPanel(string)`로 변경. 승리="보스 처치\n\n엔딩" / 패배="전원 전멸…\n\n패배"(`EndingPanel/EndingText`). |
| 11 | ✅ 역할별 스킬 우선순위 — `BattleManager.Combat.SelectSkillByRole()`. 서포터=아군 HP<60% 시 Heal / 탱커=Shield / 딜러=Damage 우선, 없으면 코스트 최고 폴백. effectType.Contains 판정. |
| 14 | ✅ 해금 직업당 1개 — `MetaPassiveManager`에서 fireball/moonlight_slash/battle_stance/indomitable/starlight 5종 게이트 제거(상수·All·_skillUnlockKey). 기본3/해금1. 랜덤 다양성 복원. |
| 1 | ✅ 좌측 메뉴 한글 — `LeftPanel.prefab`: 재화/파티/덱/스트레스(헤더), 설정/로그(버튼). 파티편집은 기존 한글. |
| 3 | ✅ 카드 스트레스 바 — `CardSlotView.stressSlider` 추가(OnStressChanged 구독, 안정/압박/패닉 색). 4개 카드 `Right_Area`의 HP `Bar_Line` 복제→`Stress_Line`(HP 아래)·점수 제거·maxValue 100·각 CardSlotView 연결. ※좌패널 별도 Stress 섹션(stressEntries)은 잔존(중복) — 추후 제거 검토. |
| 5 | ✅ 파티편집 dim — `PartyEditPanel.prefab`에 이미 `BackgroundDim`(풀스크린 활성) 존재. α 0.51→0.6. (옛 `Background`(비활성)는 잔재.) |
| 6 | ✅ 카드 초상화 — `FellowCardView`가 roleBadgeImage에 `fellow.portrait??fellowSprite` 표시(white+preserveAspect), 없으면 역할색 폴백. |
| 2·7 | ✅ 스킬 호버 툴팁(공용) — 신규 `UI/SkillTooltip.cs`(`SkillTooltipController`+`SkillTooltipTrigger`). 씬 `Canvas/SkillTooltip`(Canvas overrideSorting 100, TooltipBox+Body). `CardSlotView`(스킬1·2 라벨, #2)·`FellowCardView`(skillsLabel 전체, #7)가 런타임 `Ensure().SetSkills()`로 부착. 명/코스트/효과/설명 표시. |
| 8 | ✅ 한기 강화 팔레트(사용자 선택) — 두 씬 배경 0.05·0.067·0.075→0.04·0.06·0.085 / GameStartScene 버튼 0.16·0.19·0.22→0.15·0.19·0.26 / `LeftPanel.prefab` 슬레이트 0.0588·0.0706·0.0941→0.043·0.062·0.105(17곳). ※메르세나리/교회 등 패널은 미적용 — 추가 통일은 백로그. |
| 9 | ✅ 모집비용 통일 30 — 기획 §14 "1성 모집비용 30" 확정값. `fellow.json` 전원 30(디펜더·어택커 40→30, 프리스트 35→30). ※README/09 표는 옛 30/40/35 잔존 — 문서 동기화 백로그. |
| 15 | ✅ 확인 완료(이전 세션) — 튜토리얼 `tutorial_completed`==0 시 자동. |

**남은 백로그(이번 세션 발견)**: ③ README/09_캐릭터시트 모집비용 표를 30으로 문서 동기화.

### 6차 세션 후속(2026-06-05, 동일 세션 추가 처리)
- ✅ **좌패널 별도 Stress 섹션 제거** — `LeftPanel.prefab`의 `Stress_Accordion` 비활성(m_IsActive 0). `LeftPanelView.cs`에서 stressEntries 필드·StressEntry 클래스·RefreshStressRows/GetStressColor/GetStressLabel·OnStressChanged 구독 전부 제거(데드코드). 스트레스는 카드 바(#3)로 일원화. 검증: Play 시 좌패널에 재화/파티/덱만 표시(스트레스 섹션 없음).
- ✅ **팔레트 메르세나리/교회 확장** — 두 패널 배경은 프리팹이 아닌 **씬 인스턴스 오버라이드**로 다크 슬레이트(0.05·0.06·0.07, α0.96)였음(grep이 α0.96을 놓쳐 초기엔 흰색으로 오판). 4개 `Background`(MercenaryOffice/Recruit/Growth + Church)를 한기 슬레이트 0.04·0.06·0.085(α0.96)로 통일(GamePlayScene 저장). ※패널 내 버튼은 슬레이트 색이 검출 안 됨(스프라이트 기반 추정) — 미적용.
- ✅ **고블린 애니메이션 연결** — 자산(`Resources/Animators/Enemies/Goblin/`: Goblin.controller + Idle/Attack/Attack2.anim, 각 0.52s, 키프레임 채워짐)은 있었으나 `enemies.json` 고블린 `animatorPath`가 **빈 문자열**이라 미연결이었음. `"Animators/Enemies/Goblin/Goblin"`로 설정. `DefaultSetting.cs`가 컨트롤러 로드→EnemyObject(Animator 보유) 주입. **런타임 검증**: Combat 진입 시 스폰 고블린 2마리 모두 `runtimeAnimatorController=Goblin` + Idle 재생(playing=True), 로드 실패 경고 0. ※raider/boss/crow는 컨트롤러 없어 animatorPath 빈 채 유지.
- 백업: `~/Documents/backup/2026-06-05_stress_palette_goblin/`.

### 6차 세션 후속 2 (2026-06-05, 사용자 추가 요청)
- ✅ **고블린 좌우 반전** — 고블린 애니메이션 아트가 기본 적 페이싱(좌향, `SetFacing` localScale.x −)과 반대로 그려져 있어 보정. 데이터 필드 `flipSprite`(EnemyDef/EnemyData/EnemyDatabase) 신설, `enemies.json` 고블린 `"flipSprite": true`. `DefaultSetting`이 `SetFacing(faceLeft: isEnemy ^ flipSprite)`. 검증: 스폰 고블린 렌더러 localScale.x=+0.85(일반 적은 −). raider/boss/crow는 flipSprite 미설정.
- ✅ **스킬 해금 2디폴트+2잠금 복원(#14 되돌림)** — 직전 #14에서 제거했던 fireball/moonlight_slash/battle_stance/indomitable/starlight 5종 해금을 `MetaPassiveManager`(상수·All·_skillUnlockKey)에 재추가. 검증: All 내 스킬해금 10개, 캐스터 풀 잠금=[fireball, ice_storm] → 직업당 기본2/해금2(마석 해금). ⚠️ 이전 #14 표 기록은 무효(사용자가 2잠금 유지를 원함).
- ✅ **공격 로직 복원(미행동→진형 변경)** — 2026-05-29의 actionOrder 분리(allies 불변) 방식을 사용자 요청으로 원복. `BattleManager.Combat.ExecuteAction`: allies 진형 순서대로 행동, 미행동자는 턴 종료 시 `allies` 맨 앞으로 이동(상대순서 유지) + `battleSlotIndex` 재할당 + `DefaultSetting.AllyLayout.RelayoutNow()`로 즉시 시각 재배치. `_carryoverOrderList`(Phases·BattleManager.cs) 제거. `DefaultSetting`에 `static AllyLayout` + `RelayoutNow()` 신설. 검증: [Support,Tank,Tank,Dealer]에서 Dealer 미행동→[Dealer,Support,Tank,Tank], slot 재할당, RelayoutNow 정상.
  - ※부수효과: 미행동자가 allies 맨 앞 → 적 FrontFirst 타겟도 그 앞열을 향함(원래 동작, 의도됨).
  - 🔧 정렬 순서 사양 보정(2026-06-05): 코어루프 명세 §동료 행동 — 복수 미행동 시 **감지(배치) 순서대로 각자 맨 앞 삽입** → 나중 미행동자가 더 앞. 예 `1-2-3-4`에서 3,4 미행동 → **`4-3-1-2`**(역순 삽입 버그를 spec대로 수정). 단일 3 미행동 → `3-1-2-4`. 검증 완료.

### 6차 세션 후속 3 (2026-06-05) — 프리스트 스케일 / 모션 검토 / 파티편집 UI
- ✅ **프리스트 스케일** — 6/5 추가 Idle 시트 `Priest_Idle_2~5.png`가 기본 PPU 100이라 renderH 2.66~2.74로 큼(원본 `Priest_Idle` PPU 129.5=2.0). 새 4시트 PPU를 2→133/3→135/4→137/5→137로 보정 → 전 프레임 renderH=2.0(다른 동료와 동일, 프레임별 편차 해소). 백업 `~/Documents/backup/2026-06-05_priest_scale/`.
- ✅ **스킬 모션 검토** — `MotionCategoryResolver`(effectType+isRanged): 검 근접→Melee(대시), 마법/신성 원거리→Ranged(제자리), 힐/실드/혼합→Stationary. 20개 스킬 전수 검토 결과 **전부 적합** — 데이터 변경 없음. (한계: 힐/실드도 Attack 애니 재생 — 전용 캐스트 에셋 필요, AoE 멜리(역류)는 단일 타겟 대시 — 백로그.)
- ✅ **파티편집 UI 개선** — ⚠️핵심: 패널이 LeftPanel·노드맵 뒤에 렌더돼 모달이 화면을 못 덮고 좌패널/노드가 비쳐 겹쳤음. 수정: `Canvas/PartyEditPanel`에 **Canvas(overrideSorting, order 80) + GraphicRaycaster** 추가 → LeftPanel(0)·노드 위로. `BackgroundDim`을 불투명 쿨 슬레이트(0.04·0.055·0.08, α0.98)로 → 배경 완전 차단. 잔재 `Background`(비활성) 삭제. `ReserveScrollView` 밝은 회색→다크 슬레이트(0.075·0.09·0.12). 풀해상도 캡처로 클린 확인(타이틀/인원/4카드/예비대/나가기/안내 정렬, 비침 0). ※저해상도 MCP 오버레이 캡처는 비침 아티팩트 있음(실제 렌더는 정상). 백업 `~/Documents/backup/2026-06-05_partyedit_ui/`.
- 백업: `~/Documents/backup/2026-06-05_flip_unlock_actionorder/`.

---

## 🗂️ (이전) 기획자 피드백 15항목 원본 표 — 참고용

> 작업 전 위치·원인 메모. 위 §6차 완료 표가 최신.
> **권장 순서**: 버그(4,10,12,13) → 밸런스(11,14) → UI(1,3,5,6,2·7,8) → 대기(9). 15는 확인 완료.

| # | 분류 | 요구 / 원인·위치 / 수정 방향 |
|---|---|---|
| 1 | UI | 좌측 메뉴 한/영 혼용 → **한글 통일**(재화/파티/덱/스트레스, 파티편집/설정/로그). 위치: `LeftPanel/.../{Money,Party,Deck,Stress}_Accordion/Button_Header/Text (TMP)`, `LeftPanel/Image/{PartyEdit,Setting,Log}/Text (TMP)`. 정적 TMP. ※일부 런타임 세팅 여부(LeftPanelView) 확인 |
| 2·7 | UI신규 | **스킬 호버 툴팁 공용 시스템**(명/코스트/효과/설명). `CardSlotView`(2) + `FellowCardView`(7) 공용. 데이터 `SkillData.description` + `SkillDatabase.GetSkill(id)`. IPointerEnter/Exit + 툴팁 패널 |
| 3 | UI | **스트레스 바를 파티 카드 HP 아래로**. `CardSlotView.cs`에 스트레스 Slider 추가 + `FellowData.OnStressChanged` 구독. 별도 `Stress_Accordion` 제거 검토 |
| 4 | 버그 | **로그 스크롤 안 됨**. `Log/LogPopup.cs`(scrollRect line 21,107). ScrollView Viewport/Content·ContentSizeFitter(Vertical)·ScrollRect.content 연결·Vertical 점검. 위치: `Canvas/LogPopup` |
| 5 | UI | **파티편집 배경 dim**. `Canvas/PartyEditPanel/Background`(현재 비활성) 활성화 + 반투명 검정 `(0,0,0,0.6)` 풀스크린 |
| 6 | UI | **파티편집 카드 아이콘 → 초상화/스프라이트**. `FellowCardView.cs`(roleBadgeImage 색상배지) → `fellow.portrait`/`fellowSprite`. (정면 초상화 에셋 없으면 스프라이트 우선) |
| 8 | UI협의 | **전체 색상톤 추가 변경 희망**(주관적). 다음 세션에 팔레트 2~3안 제시 후 협의. 현재: 배경 0.05,0.067,0.075 / 버튼 0.17,0.20,0.24 / 텍스트 0.86~0.9 |
| 9 | 기획대기 | **모집비용 통일**(현 fellow.json 30~40). **기획자 기획서 수정 예정** → 확정값 후 fellow.json+§14 반영 |
| 10 | 버그 | **팝업 열린 채 노드 클릭 시 노드 실행+팝업 잔존**. `Node/NodeSystem.cs OnNodeClicked`(line 371) 가드 없음 → 패널 열림(PanelBase alpha>0.5) 중 클릭 무시 또는 열린 패널 강제 Close. (LeftPanelToggle 게이트 로직 재사용 가능) |
| 11 | 로직 | **스킬 우선순위**. `BattleManager.Combat.cs:107~110` 무조건 `OrderByDescending(costAmount)`. → Support는 아군 HP낮으면 Heal, Tank는 Shield 우선, 딜러는 딜. `SkillData.effectType` + 아군 HP비율 활용 |
| 12 | 버그 | **파티 아코디언 열린 채 아군 사망 → 빈칸, 재오픈해야 정렬**. `LeftPanelView.cs`/`CardSlotView.cs` — `FellowData.OnDied` 구독해 사망 즉시 파티 리스트 재빌드 |
| 13 | 버그 | **패배 시 "보스 처치 엔딩" 팝업**. `BattleManager.Phases.cs:286` 패배도 `ShowEndingPanel()`(정적 "보스 처치\n\n엔딩" 텍스트, GamePlayScene). → 승리/패배 텍스트 분기(ShowEndingPanel(text)) 또는 패배 전용 패널 |
| 14 | 밸런스 | **해금 2개→축소**. 직전 세션 직업당 2개 잠금(fireball/moonlight/battle_stance/indomitable/starlight, `MetaPassiveManager`). 사용자 의도(랜덤 다양성)와 충돌 → **직업당 1개만 잠금(기본3/해금1)** 또는 잠금 제거. `_skillUnlockKey`·`All` 조정 |
| 15 | 확인✅ | **튜토리얼 조건**: PlayerPrefs `tutorial_completed`==0(미완료/키없음)일 때 [게임시작] 시 **최초 1회 자동**. `MoveScene.cs:53` `if(!IsCompleted())`. 안 뜬 건 이전 완료/스킵으로 1 저장됨 → `ResetCompletedFlag()` 또는 PlayerPrefs 삭제, 또는 [처음이신가요?] 버튼(`MoveScene.cs:40`, 완료시 노출) |

**다음 세션 시작용 복사 글**
```
[이어서 작업 — 기획자 피드백 15항목]
HANDOFF.md 의 §NEXT(기획자 피드백 15항목) 대로 작업한다.
작업 전 MEMORY.md·기획 폴더·백업 규칙 확인, 대상 파일/씬은 ~/Documents/backup/ 에 백업.
순서: 버그(4,10,12,13) → 밸런스(11,14) → UI(1,3,5,6,2·7,8) → 대기(9). 15는 확인완료.
각 항목 '위치/원인/수정' 기준으로 UnityMCP 직접 수정·검증(플레이→스크린샷). 8·9는 진행 전 사용자에게 방향/확정값 확인.
```

---

## 🟢 직전 세션 완료 (2026-06-02 ~ 06-05, 4·5차)

> 전반 **UI 서사톤(어둡고 차가움) 통일** + 좌측 패널 접기 기능군. 모두 저장·검증됨.

- **테마 통일(다크)**: 메인 메뉴(GameStartScene — 한글 라벨·임시 타이틀 "괴이탐사대"·Exit 버튼 수정), 노드맵(엘리트 호박색 구분·동선 대비), 전투 프레임(배경/좌패널/턴버튼/덱), HP 바 트랙, 스택 3컬럼(역할색 외곽선+다크박스), 손패 카드(`StackCardController` 다크), 좌측 패널(아코디언/버튼 슬레이트+밝은텍스트), 용병소 3패널(버튼16·텍스트47 일괄), 교회.
- **좌측 패널 접기**: `Assets/Scripts/UI/LeftPanelToggle.cs`(신규) + `Canvas/LeftPanel/CollapseTab`(▶ 탭, override Canvas sortingOrder 50, CanvasGroup). 접으면 ① 패널 슬라이드 숨김 ② 오른쪽 콘텐츠 offsetMin 600→0 화면채움(rightContents=NodeDisplay/RightMainArea/MercenaryRoot/RestPanel) ③ 전투 월드(InGameObjects) 좌측 이동(battleWorldShiftX). **게이트**: 용병소(Office/Recruit/Growth)·노드맵·전투에서만 가능, 그 외 PanelBase 패널 열리면 탭 숨김. `MagicStoneShopPanel.IsOpen` 추가.
- **파티편집 양방향 스왑**: `PartyEditPanel.cs` — `_selectedReserveIndex` 추가, 예비대/파티 어느쪽 먼저 선택해도 교체(`ClearSelection`/하이라이트). 동작변화: 예비대 클릭=즉시합류 → 선택후 대상클릭.
- **스킬 배정**: `SkillDatabase.PickSkillsFromPool`(풀에서 해금된 것 중 2개) + 직업당 2개 해금(`MetaPassiveManager`) ← **#14에서 축소 요청**.
- **튜토리얼 영혼석**: 진입 시 400 지급/종료 시 원복(`TutorialManager` + `BaseCurrency.SetAmount`). 백업·검증 워크플로우 확립: 플레이 진입→`NodeSystem.OnNodeClicked(0,0)`로 전투 강제→**메인카메라 직접 렌더**(월드 스프라이트)·**Scene뷰**(오버레이)·**Canvas를 임시 ScreenSpaceCamera로 바꿔 합성** 캡처.
- ⚠️ **MCP 주의**: 플레이 모드에서 프레임이 스크린샷 시점에만 진행됨 → 매프레임 Update 검증은 스크린샷으로 강제 틱 필요. / execute_code의 raw 색변경은 프리팹 인스턴스 오버라이드 직렬화 안 됨 → `EditorUtility.SetDirty`+`PrefabUtility.RecordPrefabInstancePropertyModifications` 필요.
- **백업**: `~/Documents/backup/20260602_startscene_ui/`, `~/Documents/backup/20260602_tutorial_currency_exitfix/`.

---

## 🚀 다음 세션 진입 시 첫 행동 (필독)

### 1) Unity Editor 살아있는지 확인
```
mcp__UnityMCP__manage_editor action=telemetry_status
```
- `no_unity_session` 응답 시: 사용자에게 Unity Editor 재실행 요청
- 정상 시: 다음 단계 진행

### 2) 튜토리얼 모달 박스 사이즈 — 원본 유지 (롤백됨)
- 2026-06-01 에 1100×560 으로 키워봤으나, **사용자가 기존 위치/크기가 더 낫다고 판단해 원본으로 롤백**.
- **현재 최종 상태 (원본):** GuidePanel **900×400** (pos 0,0 중앙) / MessageText fontSize **30** · **Center** 정렬 · anchor(0.05~0.95, 0.25~0.95)
- 롤백 방식: `~/Documents/backup/2026-06-01_tutorial_modal_size/GamePlayScene.unity.bak` 파일 복원 → refresh.
  1100×560 버전은 같은 폴더의 `GamePlayScene.unity.modal1100x560.bak` 에 보관 (되살리려면 이 파일 복원).
- ⚠️ MCP 팁: `save` 가 path 인자를 무시하고 활성 씬을 저장하므로, 특정 씬 저장 전 `set_active_scene` 으로 대상 활성화 필요.

### 3) Play 검증 권장 (선택)
- 튜토리얼 한 사이클 (자동 진입 → 5노드 → 보스 즉사 → 메뉴)
- W1 전투 시각 흐름 (dash → 모션 → impact → back dash)
- W6 Idle 5종 자동 재생
- DoT 초록 tint + 도발 + 전장의 방패

---

## 🟢 2026-05-29 세션 완료 작업 (시간순 정리)

### A. 원거리 적 스킬 dash 제거 (5종)
- `enemy_skills.json` 5개 스킬에 `isRanged: true` 추가
- 대상: 고블린 독침, 약탈자 도끼던지기, 보스 까마귀 부름/수확/순간이동
- `MotionCategoryResolver.Resolve(..., bool isRanged)` → 최우선 분기로 Ranged 반환
- `OnSkillCast` 시그니처: `Action<string, int>` → **`Action<string, int, bool>`** (EnemyData.Hp / FellowData.Hp 둘 다)
- 동료 호출부 `false` 전달, 적 호출부 `skill.isRanged` 전달

### B. 까마귀 자폭 카운트다운 UI
- `EnemyData.OnLifeTurnsChanged` 이벤트 추가
- `ProcessSummonExpiration` 에서 `currentLifeTurns--` 후 Invoke
- `BattleCardView` 가 까마귀(`summonLifeTurns > 0`) 일 때 hpScoreText 복제로 카운트다운 텍스트 동적 생성 (HP 라인 아래 주황 "자폭까지 N턴")

### C. 노드 분포 동적 가중치 (엘리트 후반, 일반 초반)
- `MapGenerator.GetCombatEliteWeights(layer)` — layer 1~7 표 lookup
  - layer 1: Combat 70 / Elite 0 / Shop 20 / Event 10
  - layer 7: Combat 30 / Elite 40 / Shop 20 / Event 10
- 인스펙터 `combatWeight/eliteWeight` 폐기 (shopWeight/eventWeight 만 유지)

### D. 노드별 적 수 랜덤화 (후반 가중)
- `FloorTierResolver.GetEnemyPool(floor)` + `RollCount(floor)` 분리
- 일반 전투 풀: **고블린만** (약탈자 제거)
- 마릿수: 1~2층=2 / 3~4층=2~3 / 5~6층=3~4 / 7~8층=3~4 (4가중 70%)
- `EnemySpawner` 가 Combat/Elite 모두 풀+RollCount 방식

### E. 행동/피격 순서 분리 (allies 진형 영구)
- 기존 버그: 미행동자가 다음 턴 `allies` 맨 앞으로 이동 → 적 FrontFirst 타겟이 매 턴 바뀜
- 해결: `BattleManager.Phases.cs:122` 의 `allies.Remove/InsertRange` 코드 제거
- `BattleManager.Combat.cs:55` `ExecuteAction(true)` 진입 시 별도 `actionOrder` 임시 리스트 빌드
  - priority (이전 미행동자) → 나머지 allies 순서
- `allies` 자체는 진형 순서 영구 유지 — FrontFirst/BackLast 일관성

### F. D1 — 어택커.role=Tanker 데이터 검증
- fellow.json / 기획서 §08 모두 일치: 어택커 = Tank role (탱커형 공격 + 전체 힐)
- 데이터 오류 아님. 기획 의도 유지. 변경 없음.

### G. W4 — 적 카드 visualScale (적별 개별)
- `EnemyDef`/`EnemyData` 에 `float visualScale` 필드
- `enemies.json`: 고블린 0.85 / 약탈자 1.0 / 보스 1.25 / 까마귀 0.6
- `DefaultSetting.cs` 적 spawn 후 `newObj.transform.localScale = ObjectPrefab.transform.localScale * vs`
- `EnemyObject.prefab` 자체 localScale 1.0 정상화 (MCP)

### H. W6 — Idle 자동 재생 (5종 키프레임)
- 5종 동료 Idle.anim 의 키프레임을 sprite 시트에서 자동 수집해 균등 배치 + loop
- Attacker/Defender/Offender/Priest: 4프레임 ~0.77s 사이클
- Caster: 처음 13프레임 → **0~4 영어 텍스트 sprite 발견 → 5~12 만 사용 (8프레임)**
  - 추가로 _8 (width 150 비정상) 제외 → 최종 7프레임
- Priest: 사용자 요청으로 1프레임(Priest_Idle_0)만 loop (정적)

### I. WS — skill_war_shield (Damage + Shield 복합) ✅ 완성 (2026-06-01 확인)
- 기획 §10: Tank 6 / AllEnemies / Damage 20 + Shield 30 (전체 실드)
- skills.json 정의 ✅ (`MixedDamageShield`, power 20 / shieldPower 30) / 디펜더 skillIds 할당 ✅
- 코드 `BattleManager.Combat.cs:461` → `ApplySkillDamage` + `ApplyMixedShield`(AllAllies 실드) ✅, `FellowData.Hp.AddShield` 구현 ✅
- 남은 것: **Play 검증만**
- 사용자 메모: "스킬 전부 늘어났다 — 1직업당 4스킬, 2개 랜덤 지정" 요청 → fellow.json 의 skillIds 가 직업별 4개로 확장 + PartyManager 가 2개 랜덤 선택 (BattleManager.cs:283 `skillPool` 변수)

### J. WC — skill_war_cry (Damage + Taunt 도발) ✅ 완성 (2026-06-01 확인)
- skills.json 정의 ✅ (`MixedDamageTaunt`, power 20 / tauntTurns 1) / 어택커 skillIds 할당 ✅
- 도발 폐루프 전부 연결: 부착 `Combat.cs:504` / 적 타겟 우선 `EnemySkillExecutor.cs:42` / 매 턴 -1·해제 `Phases.cs:152`
- 남은 것: **Play 검증만**

### K. 동료 4스킬 풀 + 2개 랜덤 선택
- `BattleManager.cs:280` 동료 스킬 배정 시 fellow.skillIds 4개 풀에서 2개 비복원 추출
- 변수명 충돌 fix: `pool` → `skillPool` (외부 스코프 line 337의 `pool` 과 충돌)

### L. 튜토리얼 시스템 (대규모)

#### L-1. 기본 골격
- `TutorialManager.cs` 신규 — 싱글톤, PlayerPrefs (`tutorial_completed` 0/1), IsTutorial 플래그
- `TutorialGuidePanel.cs` 신규 → 이후 **모달 다이얼로그 컨트롤러**로 재작성 (L-5)
- `MoveScene.cs` — `InGameSceneLoaded()`: 완료 플래그 분기. `StartTutorialAgain()` 신규 핸들러
- 메인 메뉴 `TutorialAgainButton` (MCP) — [처음이신가요?] 버튼. 완료 플래그 true 일 때만 노출

#### L-2. 파티/적 분기
- `PartyManager.InitDefaultParty` — IsTutorial 면 강제 클리어 + `GenerateTutorialParty()` (캐스터/프리스트/디펜더 3인 + null slot3)
- `EnemySpawner.SpawnIntoBattleManager` — IsTutorial 진입 시 **RoomType별 단일 적 고정** (2026-06-01 분기 완료): Boss→까마귀 보스 / Elite→약탈자 / 그 외→고블린. (이전엔 전 노드 고블린 고정 — 엘리트·보스도 고블린 뜨던 문제 해소)

#### L-3. Canvas 통합 + CanvasGroup 전환
- 기존 root level `TutorialCanvas` (별도 Canvas + CanvasScaler + GraphicRaycaster) → 기존 Canvas 자식으로 reparent
- 자식 Canvas 컴포넌트 제거 + CanvasGroup 부착 (alpha/interactable/blocksRaycasts 일괄)
- TutorialGuidePanel.cs 슬롯 `rootCanvas (GameObject)` → `canvasGroup (CanvasGroup)`

#### L-4. 재진입 버그 fix (3명 파티)
- 원인: PartyManager 가 `DontDestroyOnLoad` → InitDefaultParty 가 Start 에서 단 1회만 호출. 재진입 시 튜토리얼 파티 잔재
- 해결: `PartyManager.ForceReinitParty()` 공개 메소드 추가 → MoveScene 의 `InGameSceneLoaded()` 와 `StartTutorialAgain()` 둘 다 호출

#### L-5. TUT-EXIT / TUT-FAIL
- `BattleManager.Phases.HandleBattleEnd` 진입에 IsTutorial 분기
  - **승리 (일반 노드)**: 다음 노드로 진행 (일반 흐름과 동일)
  - **승리 (보스 노드 — 일어나지 않음, 즉사 시나리오로 보스는 못 죽임)**
  - **패배 (일반 노드)**: ForceReinitParty + 같은 씬 리로드 (재시작)
  - **패배 (보스 노드)**: `EndTutorial(true)` + 메뉴 복귀 = 튜토리얼 완료

#### L-6. 5노드 시퀀스
- `MapGenerator.GenerateTutorialMap()` — IsTutorial 면 5층 일렬 맵
- 시퀀스: `Combat → Shop → Event → Elite → Boss`
- 각 층 1노드 (분기 없음)

#### L-7. 보스 즉사 시나리오 (1턴 후 전멸)
- `BattleManager.Combat.ApplyDamageToAlly` 진입 시 IsTutorial + RoomType.Boss → damage 를 target.maxHp 로 갈음
- 사용자가 카드 1번 쓰고 보스 1턴 행동으로 어떤 보스 스킬이든 즉사

#### L-8. 모달 다이얼로그 전환 (진행형) ★ 가장 큰 변경
**기존 (제거됨)**: 하단 고정 패널 + 자동 단계 진행 (CurrentStep 0~3)
**현재**: 중앙 모달 + 9개 진행형 다이얼로그 (1회 표시 보장)

- `TutorialManager.DialogueId` enum (9개) + `_shownDialogues HashSet`
- `TutorialManager.TryShowDialogue(id)` — 1회만 표시
- `TutorialGuidePanel.Show(message)` / `Hide()` + 정적 `Instance`
- 호출 위치:
  - **0 NodeMapIntro**: `NodeSystem.Start()` 첫 노드맵
  - **1 CombatIntro**: `DispatchByRoomType` Combat case
  - **2 EnemyTurnIntro**: `BattleManager.Phases.HandleActionPhase` 적군 행동 진입
  - **3 ResultIntro**: `HandleResultProcessing` 진입
  - **4 CombatVictory**: `HandleBattleEnd` 튜토리얼 승리 분기
  - **5 ShopIntro**: `DispatchByRoomType` Shop case
  - **6 ChurchIntro**: Event case
  - **7 EliteIntro**: Elite case
  - **8 BossIntro**: Boss case
- 메시지 강화 (Shop/Church/Elite/Boss 각 버튼 안내 포함)
- 라벨: [다음] → [확인], [스킵] → [메인 메뉴로]
- UI 재배치 (MCP): 풀스크린 → 중앙 900×400 모달 (BackgroundDim 풀스크린 자식 추가) — **추후 1100×560 으로 키우는 작업 미완** (위 §다음 세션 진입 시 첫 행동 §2 참조)

### M. C3 — DoT 시스템 + 초록 tint 시각화
- `EnemySkillData.dotPower/dotTurns` 필드. 독침: 10/2
- `FellowData.Hp` 에 `dotTurnsLeft/dotPerTurn` + `OnDotChanged` 이벤트
- `EnemyAction.cs` DoT 부착 (덮어쓰기) + 사망 대상 제외
- `BattleManager.Phases.HandleResultProcessing` 에 매 턴 끝 누적 적용 + 카운터 -1
- `BattleCardSprites` 에 `SetPersistentTint(Color)` / `ClearPersistentTint()` public 메소드
- `BattleCardView` 가 `OnDotChanged` 구독해서 카드 sprite 초록 tint (DotTintColor = 0.55, 1, 0.55)

---

## ⚠️ 미완 / 검증 미수행

| # | 항목 | 처리 방법 |
|---|---|---|
| — UI | 튜토리얼 모달 박스 크기 | 1100×560 시도 → **원본(900×400) 유지로 롤백** (추가 작업 없음) |
| W1 | 전투 시각 흐름 (dash → 모션 → impact → back dash) | Play 모드 1회 전투 |
| ✅ W2/W3 | ~~Portrait (LeftPanel)~~ | **2026-06-10 완료** — 초상화는 Idle 첫 프레임 자동 추출로 정상 표시됨. 파티 아코디언이 기본 접힘이라 안 보였던 것 → 기본 펼침으로 변경 |
| W5 | Attack2 모션 (캐스터/어택커) | 2번 스킬 발동 |
| W6 | Idle 5종 자동 재생 | 카드 spawn 직후 |
| **신규** | **튜토리얼 한 사이클** | 자동진입 / 5노드 / 보스 즉사 / 메뉴 / [처음이신가요?] |
| **신규** | DoT 초록 tint + 도발 + 전장의 방패 | 각 스킬 발동 후 |
| ✅ WS | ~~skill_war_shield JSON/코드 effectType 처리~~ | **완성 — Play 검증만 남음** (2026-06-01 확인) |
| ✅ WC | ~~skill_war_cry 도발 어택커 할당~~ | **완성 — Play 검증만 남음** (2026-06-01 확인) |

---

## 🚧 남은 작업 — 우선순위 표

### 🟥 작음 (즉시 가능, 30분 내)
| # | 작업 | 위치 / 메모 |
|---|---|---|
| ~~TUT-HL~~ | ✅ 튜토리얼 하이라이트 오버레이 (기획 §4-2) | **2026-06-01 완료** — 외곽선 박스. `TutorialGuidePanel.highlightBox`+`handTarget=LeftPanel`. CombatIntro·ResultIntro→손패 강조. enemyTarget 미배선(확장 가능) |
| ~~WS-ASSIGN~~ | ✅ 디펜더 skill_war_shield 할당 + MixedDamageShield 처리 | **완료** (코드·데이터 모두 구현, Play 검증만) |
| ~~WC-ASSIGN~~ | ✅ 어택커 skill_war_cry 할당 + 도발 폐루프 | **완료** (부착·타겟·소모 전부 연결, Play 검증만) |

### 🟧 중간 (1~2시간)
| # | 작업 | 위치 / 메모 |
|---|---|---|
| 적 신규 컨트롤러 | 적도 동료 패턴으로 .controller + Idle.anim/Attack.anim | 작업자 자산 대기 |
| 까마귀 자폭 모션 | Resources/Animators/Crow/ 자산 후 dash + 폭발 | 자산 대기 |

### 🟦 큼 (반나절~)
| # | 작업 | 위치 / 메모 |
|---|---|---|
| C1 | 엘리트 적 1~2종 신규 (현재 약탈자 1종) | 적 정의 + 스킬 + sprite |
| ~~11~~ | ✅ 마석 메타 시스템 (재설계) | **2026-06-02 재설계** — 보상형 3종 폐기 → **전투 관여형 시그니처 패시브 5종 + 시그니처 스킬 해금 5종**. 마석 영구 누적. 진입: 노드맵 초록 시작노드 + 새 런 첫 노드 전 자동. 기획 §16 |
| **로그라이크 메타 루프** | ✅ 2026-06-02 — 보스클리어/전멸 공통: 예비대·파티·영혼석 초기화(마석 유지) → 패시브 해금 화면 → GamePlayScene 재로드. `BattleManager.Phases.StartNextRunLoop` (Play 검증 대기) |
| **동료 패시브 15종 (풀+랜덤)** | ✅ 2026-06-02 — 동료당 3개(총 15). 해금된 풀에서 **런 시작 시 무작위 1개** 배정(`FellowData.activePassiveId`, InitBattle). 전투 훅 15종 BattleManager.Combat. 카드(CardSlotView)에 활성 패시브 표시. 상점 스크롤(20항목). (Play 검증 대기) |
| **시그니처 스킬 마석 해금** | ✅ 2026-06-02 — 아이스스톰/하늘가르기/전장의방패/워크라이/기원. 미해금 시 스킬풀 제외 (BattleManager.cs) |
| ~~13~~ | ✅ 역할별 중증 디버프 | **2026-06-01 완료** — 첫 패닉 시 부착(전투종료까지): 딜러 받는피해+30%/탱커 실드-50%/서포터 광역힐→단일. FellowData.hasSevereDebuff |

### 🟪 기획자 추가 작업 대기 중
| # | 항목 | 메모 |
|---|---|---|
| ~~B1~~ | ✅ 스트레스 51~99 압박 디버프 | **2026-06-02 완료** — 스킬 −10%(`pressureSkillPenaltyPercent`) + 피격 스트레스 +10%(`pressureStressGainPercent`). 기획 §04 확정 |
| B2 | 패닉 후유증 (영구 stressResist 감소) |
| B4 | 시작 영혼석 증액 |
| 보스 패턴 추가 |
| 신규 동료 (샤먼 외) |

---

## 📂 핵심 위치 메모

| 종류 | 경로 |
|---|---|
| 프로젝트 루트 | `/Users/kosungmo/Desktop/Project/` |
| 기획 문서 | `/Users/kosungmo/Desktop/Project/기획/` |
| 데이터 JSON | `Assets/Resources/Data/` — `enemies.json` / `enemy_skills.json` / `skills.json` / `fellow.json` |
| 동료 애니메이션 | `Assets/Resources/Animators/Fellows/<Role>/` — Attacker / Caster / Defender / Offender / Priest |
| 적 sprite | `Assets/Resources/Characters/test_enemy_goblin.png` (적 컨트롤러 미생성) |
| 백업 | `~/Documents/backup/YYYY-MM-DD_<작업명>/` |
| Unity 씬 | `Assets/Scenes/GamePlayScene.unity`, `GameStartScene.unity` |
| 전투 카드 prefab | `Assets/Prefab/MyObject.prefab`, `Assets/Prefab/EnemyObject.prefab` |
| **튜토리얼 코드** | `Assets/Scripts/Tutorial/TutorialManager.cs`, `TutorialGuidePanel.cs` |
| **튜토리얼 UI** | GamePlayScene 의 `Canvas/TutorialCanvas` (CanvasGroup + 중앙 모달) |
| **튜토리얼 메뉴 버튼** | GameStartScene 의 `Canvas/Panel/TutorialAgainButton` |
| 핵심 코드 | `Assets/Scripts/BattleManager*.cs`, `Church/*`, `Log/*`, `Mercenary/*`, `Node/NodeSystem.cs`, `PartyManager.cs`, `Currency/*`, `Fellow/*`, `Enemy/*`, `UI/BattleCardSprites.cs`, `UI/BattleCardView.cs` |
| 메모리 | `/Users/kosungmo/.claude/projects/-Users-kosungmo-Desktop-Project/memory/MEMORY.md` |

---

## 💾 2026-05-29 세션 백업 폴더 (시간순, 2차 세션)

```
~/Documents/backup/2026-05-29_ranged_and_crow_countdown/  (원거리 dash + 까마귀 카운트다운)
~/Documents/backup/2026-05-29_node_distribution_and_count/ (노드 분포 + 적 수)
~/Documents/backup/2026-05-29_action_order_split/        (행동/피격 분리)
~/Documents/backup/2026-05-29_enemy_visual_scale/        (적 visualScale)
~/Documents/backup/2026-05-29_idle_animation/            (Fellows Idle.anim 전체)
~/Documents/backup/2026-05-29_tutorial/                  (튜토리얼 1차)
~/Documents/backup/2026-05-29_tutorial_exit_fail/        (TUT-EXIT/FAIL)
~/Documents/backup/2026-05-29_dot_system/                (DoT + 초록 tint)
~/Documents/backup/2026-05-29_tutorial_full_flow/        (5노드 시퀀스 + 보스 즉사 + 모달 전환)
~/Documents/backup/2026-05-29_handoff_update_session2/   (2차 세션 HANDOFF 갱신 직전)
~/Documents/backup/2026-06-01_tutorial_modal_size/       (3차 세션 — 모달 롤백 + EnemySpawner/NodeSystem deprecated fix)
~/Documents/backup/2026-06-01_severe_debuff/             (#13 역할별 중증 디버프 — FellowData/BattleManager*)
~/Documents/backup/2026-06-01_tut_highlight/             (TUT-HL — TutorialGuidePanel/TutorialManager)
~/Documents/backup/2026-06-01_meta_passive/              (#11 마석 메타 — Phases/Combat/SettingPopup)
```

---

## 🛠️ 새 세션 시작 시 행동 가이드

1. **MEMORY.md 전수 확인** (~/.claude/.../memory/)
2. **이 HANDOFF.md 정독**
3. **MCP 연결 확인** — `mcp__UnityMCP__manage_editor action=telemetry_status`
4. **컴파일 + 콘솔 점검** — `read_console` 으로 에러/워닝 0건 확인
5. ~~미완 작업 자동 적용~~ — ✅ 모달 박스 사이즈 조정은 2026-06-01 완료 (남은 미완 없음)
6. **Play 모드 검증 권장** — 튜토리얼 한 사이클 + W1~W6 + 모달 박스 육안 확인
7. **사용자 첫 메시지 대기**

---

## ⚠️ 진행 중 / 미해결 이슈

### 튜토리얼 시스템 (2026-05-29 풀 구현)
- **모달 박스 사이즈** — 1100×560 시도 후 사용자 판단으로 **원본(900×400, fontSize 30, Center) 롤백**. 최종 원본 유지
- **사용자 의도 변경 흔적**: 처음엔 자동 진행 (CurrentStep 0~3), 후반에 진행형 모달로 재설계. 기존 OnStepAdvanced/TryAdvanceTo 코드는 `TutorialManager.cs` 에 호환용으로 남아있지만 사용처는 없음 (정리 가능)
- **하이라이트 오버레이** 백로그 (모달 메시지로만 안내 중)

### DoT 시스템 (2026-05-29 신규)
- 적 → 아군만 — 동료 → 적 DoT 는 백로그
- 스택 X (덮어쓰기) — 추후 정책 결정 시 변경 가능
- 사망 대상 제외 (기획 §02-2 명세)

### 모션 시스템
- Idle.anim: 5종 모두 sprite 시트 슬라이싱 적용 완료. Caster 만 7프레임 (영어 텍스트 + 비정상 wide 프레임 제외)
- Priest Idle = 1프레임 정적 (사용자 결정)
- Attack2 자산: Defender/Offender/Priest 미생성. Attack 폴백 정상 동작
- 적 컨트롤러 미생성

### 정책 의문점
- ~~마석↔영혼석 변환~~ — ✅ 2026-06-01 해소: 변환 폐기, 마석은 영구 메타 재화로 전환 (기획 §16)
- `skill_war_shield` / `skill_war_cry` — ✅ 2026-06-01 코드·데이터 완성 확인 (Play 검증만 남음)

---

## 🔗 기획서 ↔ 코드 동기화 상태

| 기획서 항목 | 코드 상태 |
|---|---|
| 보스 maxHp 700 / 약탈자 250 / 약탈자 스킬 2종 | ✅ |
| 보스 수확 스킬 (HP≤50% 1회) | ✅ |
| ~~마석↔영혼석 10:1 (런 종료 자동 변환)~~ | ⛔ 폐기 → 마석 영구 메타 재화 (기획 §16, 2026-06-01) |
| **마석 메타 성장** (영구 누적 + 패시브 3종 해금 + 마석상점) | ✅ 2026-06-01 (Play 검증 대기) |
| **역할별 중증 디버프** (패닉 시 부착) | ✅ 2026-06-01 (Play 검증 대기) |
| **튜토리얼 하이라이트** (외곽선 박스) | ✅ 2026-06-01 (Play 검증 대기) |
| 교회 부활 100% | ✅ |
| 동료 스킬 추가 8종 → 4종 풀 × 5직업 = 20종 | ✅ (war_shield/war_cry 코드 처리도 완성) |
| 동료 5종 애니메이션 (Attacker/Caster/Defender/Offender/Priest) | ✅ Idle 자동 재생 적용 |
| 모션·데미지 동기화 (impactDelay 1.25s) | ✅ |
| **원거리 스킬 dash 제거** (isRanged 플래그) | ✅ |
| **동료 스킬별 원/근거리 포지션** | ✅ 2026-06-02 — skills.json `isRanged` per-skill. 원거리: 매직미사일/파이어볼/아이스스톰/심판. 나머지 Damage 근거리. `UseSkill`이 skill.isRanged 전달, Resolver 캐스터 자동규칙 제거(데이터 기반) |
| **해상도 대응 UI** | ✅ 2026-06-02 — 두 Canvas ScaleWithScreenSize 1920×1080 **match 0.5 통일**(GamePlayScene 0→0.5). 최상위 패널 앵커 감사: LeftPanel 좌측edge·우측영역 stretch·팝업 풀스크린 모두 양호 |
| **압박 디버프 / 투혼 이중적용 / 패시브명 자동축소** | ✅ 2026-06-02 — 압박 스킬-10%·피격+10% / 투혼 분담분 재적용 제거 / 카드 Job 폰트 autoSize(20~11) |
| **노드 분포 동적 가중치** (엘리트 후반) | ✅ |
| **노드별 적 수 랜덤화** | ✅ |
| **행동/피격 순서 분리** (allies 진형 영구) | ✅ |
| **적 visualScale 개별** | ✅ |
| **DoT 시스템** (고블린 독침 10×2턴) | ✅ + 초록 tint |
| **튜토리얼 시스템** (모달 + 5노드 + 보스 즉사) | ✅ (UI 사이즈 원본 900×400 유지) |
| 까마귀 자폭 카운트다운 UI | ✅ |
| 시작 영혼석 / 보스 추가 패턴 / 신규 동료 | ⏳ 기획자 |
| 적 애니메이션 | ⏳ 작업자 |

---

## 📌 마지막 변경 (분 단위 시간 순, 2026-05-29 2차 세션)

1. 원거리 적 스킬 5종 dash 제거 (`isRanged` 플래그)
2. 까마귀 자폭 카운트다운 UI (`OnLifeTurnsChanged`)
3. 노드 분포 동적 가중치 (`GetCombatEliteWeights`)
4. 노드별 적 수 랜덤화 (`FloorTierResolver.RollCount`)
5. 행동/피격 순서 분리 (`_carryoverOrderList` priority 큐)
6. 적 visualScale 개별 (4종 enemies.json 값 다름)
7. Idle 5종 자동 재생 (sprite 시트 자동 키프레임)
8. Caster Idle 슬라이싱 (영어 텍스트 sprite 5개 제외 → 7프레임)
9. Priest Idle 1프레임 고정 (사용자 결정)
10. 동료 4스킬 풀 + 2개 랜덤 (`skillPool` 변수)
11. 튜토리얼 시스템 골격 (TutorialManager + GuidePanel + PartyManager/EnemySpawner 분기 + MoveScene 진입 + 메뉴 버튼)
12. Canvas 통합 + CanvasGroup 전환 (root → Canvas 자식)
13. 파티 3명 재진입 버그 fix (`ForceReinitParty`)
14. TUT-EXIT (보스 패배 = 완료) / TUT-FAIL (일반 노드 패배 = 재시작)
15. 5노드 시퀀스 (Combat→Shop→Event→Elite→Boss)
16. 보스 즉사 시나리오 (ApplyDamageToAlly 분기)
17. **모달 다이얼로그 전환** (9개 진행형, hashSet 1회 표시)
18. 다이얼로그 메시지 강화 (Shop/Church/Elite/Boss 각 버튼 안내)
19. DoT 시스템 + 초록 tint (`OnDotChanged` + `SetPersistentTint`)
20. (미완) 모달 박스 1100×560 사이즈 조정 — 다음 세션
21. HANDOFF.md 갱신 (현재 문서)

---

## 🎆 2026-06-10 세션 — 스킬 이펙트/모션/데미지 타이밍 정리

**백업**: `~/Documents/backup/skillfx_timing_20260610_125503/` (SkillEffectFx, SkillEffectPlayer, BattleCardSprites, BattleManager.cs, BattleManager.Combat.cs)

### 1) 어택커 근접 데미지 모션 수정 — `BattleCardSprites.PlayAttack`
- 확인: 어택커 애니 `Attack`=검 휘두르기(strike), `Attack2`=방패 들기(shield).
- 문제: 역류(counter_flow, Damage, skillIndex 3)가 `Attack2`(방패)를 타서 "데미지인데 방패만 드는" 모션.
- 수정: **Melee 카테고리는 항상 `Attack`(0)** 사용 (`PlayMeleeAttackSequence(0, ...)`). Attack2(방패)는 비-근접(불굴 Heal/워크라이 Taunt) 전용으로 남김.

### 2) 이펙트 좌우 반전 — `SkillEffectFx` + `SkillEffectPlayer`
- `Fx`에 `nativeRight`(원본이 오른쪽 향함) 추가. 공격 방향(시전자→타겟)과 다르면 `flipX`.
- `SkillEffectPlayer.Play(..., bool flipX)` → `SpriteRenderer.flipX`.
- nativeRight: fireball=true(폭발 오른쪽), magic_missile/iaido/flash/reckless=false(→반전), 버프/힐=true(대칭).

### 3) 캐스터 투사체 지팡이 발사 + 확대 — `SkillEffectFx.Play`
- Projectile from = `casterPos + StaffUp(0,1.15) + (StaffFwd 0.55 × 방향)` (지팡이 끝 근사), to = `targetPos + EnemyBody(0,0.7)` (적 상체).
- 크기: fireball 2.6→2.8, magic_missile 1.8→2.4.

### 4) 카테고리별 데미지 타이밍 — `BattleManager.Combat.cs UseSkill` + `BattleManager.cs`
- 기존 단일 `impactDelay`(1.25) 분기 → 카테고리별로 분리 (impactDelay는 **적 행동 전용**으로 유지).
- **Ranged**: `rangedCastWindup`(0.4) → 지팡이서 투사체 발사 → 비행(이펙트 길이 `SkillEffectFx.GetDuration`)이 끝난 뒤 데미지. (이펙트 끝나고 데미지)
- **Melee**: `meleeImpactDelay`(0.55, 전진 dash 후 휘두름 순간) 에 이펙트+데미지 **동시**. (발도·일섬 등 휘두를 때 발동)
- **Stationary(힐/실드/버프)**: `stationaryImpactDelay`(0.7) 후 이펙트+효과.
- 4개 타이밍 필드 모두 인스펙터 노출(BattleManager) — 플레이 후 미세조정 가능.

### 5) 별부름(star_call) 위치 — AtTarget→AtCaster (힐이므로 시전자 측)

### 검증
- 컴파일 0 에러. `GetDuration` fireball 1.23s / magic_missile 0.59s.
- 정적 캡처: 5개 이펙트 flip 적용 후 모두 오른쪽(적) 향함 확인.
- 실씬 캡처: fireball 지팡이서 발사·확대·고블린行 / iaido 적 위치 반전 확인 (`Temp/v_fire_staff.png`, `v_fire_fly.png`, `v_iaido.png`).
- 실제 플레이 타이밍 체감은 사용자 플레이로 미세조정 권장(필드 인스펙터 조정).

### 역할별 스킬 이펙트/모션 표 → 본 세션 응답 참조 (20스킬: 이펙트 10개 등록 / 10개 미등록)

---

## 🧑‍🤝‍🧑 2026-06-10 세션 — 좌패널 파티 초상화 기본 표시 (W2/W3)

**백업**: `~/Documents/backup/leftpanel_party_expand_20260610_134226/` (AccordionController, LeftPanelView)

- 진단: 좌패널 초상화 시스템은 이미 정상(초상화 = 역할 애니메이터 Idle 첫 프레임 자동 추출, CardSlotView 표시). 단지 **4개 아코디언(재화/파티/덱/스트레스)이 기본 접힘**이라 파티가 안 보였음.
- 수정 ①: `AccordionController` — `SetOpen(bool, instant)` + `IsOpened` 추가. `ToggleAccordion`은 `SetOpen` 호출로 리팩터. instant 펼침 시 **스크롤 콘텐츠(ScrollRect.content) 전체 재계산** → 형제 아코디언 재배치(겹침 방지).
- 수정 ②: `LeftPanelView.Refresh` — 카드 바인딩 직후 **파티 아코디언만 1회 자동 펼침**(카드 슬롯의 부모 `AccordionController`를 찾아 `SetOpen(true, instant:true)`). `_partyAutoExpanded` 플래그로 1회만 → 사용자가 수동으로 접으면 존중.
- 검증: 깨끗한 플레이에서 파티 4명 초상화 기본 노출 + 덱이 파티 아래로 정상 배치 확인(`Temp/leftpanel_fixed.png`). 씬/프리팹 수정 없음(순수 코드).

---

## 🎴 2026-06-10 세션 — 재화 아이콘 / 파티 박스 / 카드 초상화

**백업**: `~/Documents/backup/ui_cards_icons_20260610_231636/` (CardPrefab, LeftPanel, MercenaryRoot, GamePlayScene)

### 1) 재화 아이콘 적용 — `LeftPanel.prefab`
- `Item_SoulStone/Icon`·`Magic_SoulStone/Icon` 비어있던 슬롯에 sprite_sheet 보석 할당.
- 영혼석 = `sprite_sheet_52`(금 마름모), 마석 = `sprite_sheet_65`(청 마름모).

### 2) 파티 카드 박스 크기 — `LeftPanel.prefab`
- 좌패널 `Card_Base_1~4` LayoutElement.preferredHeight **130 → 160** (여백 확보).

### 3) 카드 초상화 자연스럽게 — `CardPrefab.prefab` (1곳 수정 → 전 패널 반영)
- 모집/성장(MercenaryRoot)/파티편집(PartyEditPanel) 카드가 **전부 CardPrefab 중첩(nested) 인스턴스** → CardPrefab 하나만 고치면 자동 전파.
- RoleBadge(초상화) **80×80 → 150×165**, 상단(top-anchor) 배치 + preserveAspect. 전신 Idle 스프라이트가 작게 떠 보이던 문제 해결(약 2배).
- 카드 320×330 유지, NameLabel/HpLabel/AffinityLabel/SkillsLabel/ActionButton 위치 재정돈(겹침 없음).
- 검증: 4역할 카드 캡처(`Temp/cards_final.png`) — 초상화 크게·일관. 재화/파티 캡처(`Temp/leftpanel_currency.png`).
- 비고: 카드 배경 프레임(`Panel_2`) 하단의 붉은 띠는 프레임 스프라이트 디자인 요소(요소 아님).

---

## 🖼️ 2026-06-11 세션 — UI 개편 (Panel_1 / 1-bit 아이콘 / 좌패널 / 스킬 아이콘)

**백업**: `~/Documents/backup/ui_overhaul_20260610_235541/` (CardPrefab, LeftPanel, skills.json, CardSlotView, FellowCardView, SkillData)

### A. 카드 배경 Panel_2 → Panel_1 — `CardPrefab.prefab`
- Panel_2(가로 배너, 보더24)는 세로 카드에 늘리면 붉은 줄이 어색 → **Panel_1**(장식 프레임, 9-slice 보더60)로 교체. Sliced 유지.

### B. 재화 아이콘 1-bit 재지정 — `LeftPanel.prefab`
- 영혼석 = `1-bit Sprites/RPG_Coin_Gold_Currency_Money_GP`(금 틴트), 마석 = `RPG_Magic_Mana_Hearth_Stone`(청 틴트). Icon 박스 40×40.

### C. 좌패널 파티 레이아웃 — `LeftPanel.prefab` + `CardSlotView.cs`
- **카드 높이 130→150**: VLG childControlHeight=false라 LayoutElement.preferredHeight 무시됨 → Card_Base RectTransform 높이 직접 150 설정(이전 세션 prefHeight 변경이 무효였던 원인).
- **성향 태그 잘림 수정**: affinityTagBg 57→**98폭**, 코드 autofit 18→15. "안전주의자"(5자) 들어감.
- 기본 상태(재화 접힘)에서 4카드 모두 뷰포트(800)에 표시 확인.

### D. 스킬 아이콘 20종 (1-bit) — Resources + skills.json + 표시 코드
- `Assets/Resources/Sprites/Skills/{skillId}.png` 20개 복사 + Sprite(Point) 임포트. skills.json `spritePath`="Sprites/Skills/{id}" 20개 추가. (※ json 외부 편집 후 **AssetDatabase.ImportAsset 강제 재임포트** 필요 — 안 하면 TextAsset 캐시가 옛 버전이라 spritePath 빈 값으로 로드됨.)
- `SkillData.spritePath`→`SkillDatabase.LoadSpritesForSkills`가 Resources.Load → `skill.sprite`.
- **표시**: `CardSlotView`(좌패널 Skill1/2 박스) + `FellowCardView`(용병/성장/파티편집 카드 skillsLabel) 둘 다 **왼쪽 아이콘 + 종류별 색 틴트**(공격=적/마법=청/힐=초록/방어=하늘/버프·도발=금). 1-bit 단색을 Image.color 틴트로 채색.
- 아이콘 매핑: 발도/일섬=검, 파이어볼=혜성, 아이스스톰=얼음, 매직미사일=비전, 무모한강타=해골, 워크라이=분노, 역류=발톱, 축성=태양, 심판=혜성, 별부름=별, 기원=하트, 불굴=∞, 방어/전장/방밀/전태=방패 등.
- 검증: 좌패널(`Temp/lp_skillicon2.png`) + 용병카드(`Temp/recruit_final.png`).
- 남은 미세조정: 용병카드(FellowCardView) 스킬이 Panel_1 두꺼운 보더(60)에 약간 붙음 — 콘텐츠 inset 여지.

---

## 🐛 2026-06-11 세션 — 배틀 카드 배치 타이밍 레이스 수정

**백업**: `~/Documents/backup/battle_race_20260611_010124/` (BattleManager, DefaultSetting)

- 증상: 첫 로딩 시 아군 카드가 한 곳에 뭉치거나(또는 2장 겹침) 배치가 꼬임. 데이터/HP바는 정상. 리그룹 시도 겹침.
- 진짜 원인: **간격 문제 아님**(spacingX 1.5 정상). `BattleManager.OnEnable`(InitBattle→BattleLoop)과 `DefaultSetting.OnEnable`(SpawnObject→RelayoutCards)이 **독립 실행**되어 순서 미보장 → 스폰/재배치 전에 전투 로직 진행 시 레이스. MCP는 프레임 정지라 재현 안 되지만 실기기(풀 프레임)에서 발생.
- 수정 ①: `BattleManager.BattleLoop` 시작에 가드 — `yield null`(스폰 완료 대기) → `allies[i].battleSlotIndex=i` 재스탬프 → `DefaultSetting.AllyLayout.RelayoutNow(instant)` 강제 스냅 → `yield null` → 턴 시작. (사용자 요청 "전부 옮긴 후 전투 시작")
- 수정 ②: `DefaultSetting.RelayoutCards` 아군 분기 — `battleSlotIndex<0` 카드를 **스킵하지 않고** allies 순서 폴백으로 포함(스킵 시 스폰 위치에 잔존→겹침).
- 검증: 깨끗한 전투 4명 고유위치 4/4(2,0,-2,-4) + 리그룹 시뮬 겹침 없음. spacingX는 1.5/1.25 원복(사용자가 "배치는 정상"이라 함).
- 리그룹 트윈(0.25s) 중 카드 교차는 애니메이션 특성(Combat.cs가 0.25s 대기 후 진행) — 최종 위치 정상.

### 전투 진입 로딩 화면 (시간 벌기 + 스폰 가림) — `UI/LoadingScreen.cs` 신규
- 자체 생성형(씬 배치 불필요, DontDestroyOnLoad, Canvas sortingOrder 10000). 다크 풀스크린 + **회전 스피너**(금색 원형 화살표 `Resources/UI/loading_spinner`, 240°/s) + 금색 "전투 준비 중…"(점 애니) + 금색 진행바(자동 채움). 폰트=`TMP_Settings.defaultFontAsset`(NanumGothic).
- API: `LoadingScreen.Cover(msg)` / `UncoverRoutine(d)` / `UncoverInstant()`.
- 연결: `BattleManager.OnEnable`에서 `Cover("전투 준비 중")` → `BattleLoop`이 [yield→슬롯재스탬프→RelayoutNow(instant)→yield→`battleEntryLoadingHold`(0.6s) 대기→`UncoverRoutine(0.4)`] → 턴 시작. 노드맵→전투는 같은 씬 패널전환이라 SceneTransition(씬로드 페이드) 미적용 → 이 커버가 스폰/정렬 찰나를 가림.
- 검증: 진입 시 커버 alpha=1 + 그 아래 카드 4명 정상배치 확인. (MCP는 프레임 정지라 타이머 자동 언커버는 실기기에서 동작; UncoverInstant로 메커니즘 확인.)
- 인스펙터 `battleEntryLoadingHold`(BattleManager)로 커버 유지시간 조정 가능.

### 중복/잔존 카드 제거 가드 + 파티 아코디언 기본 접힘 (2026-06-11)
- 증상: 실기기 전투 진입 시 캐릭 스프라이트가 뭉쳐 보임(4명인데 5개처럼) = 중복/잔존 카드. MCP에선 항상 4개라 재현 안 됨 → **원인 무관 제거 가드**로 해결.
- `BattleManager.BattleLoop` 가드: 스폰 완료 후 **씬 전체** BattleCardView(적 제외) 중 **Fellow 없음(유령) / allies 에 없음(잔존) / 같은 Fellow 중복**을 제거. (이전엔 AllyLayout 하위만 봐서 **컨테이너 밖으로 분리된 유령 카드를 놓침** → 씬 전체로 확장.) 이후 슬롯 재스탬프 + RelayoutNow(instant).
- `DefaultSetting.ClearSpawnedObjects`: 컨테이너 자식을 **DestroyImmediate**(지연 Destroy로 인한 잔존/중복 방지) + 진영 프리팹명(MyObject/EnemyObject)으로 시작하는 **분리된 잔존 카드까지 씬 전체에서 제거**.
- 검증: 유령/중복 카드 2장 주입(4→6) → 프루닝이 2장 제거 → 4 복구 확인. 정상 4카드는 유지.

### ⚠️ 2026-06-11 — 위 "배틀 중복/배치 버그"는 **유니티 렉(렌더 글리치)** 으로 판명, 코드 수정 전부 폐기
- 사용자 확인: 실제 코드 버그가 아니라 **에디터 렉**으로 캐릭이 겹쳐 보인 것(MCP에선 항상 4명 정상이었음).
- 폐기(원복): `DefaultSetting.cs` → 원본 복원(ClearSpawnedObjects 지연 Destroy, RelayoutCards `slotIdx<0 continue`). `BattleManager.BattleLoop` → 가드/프루닝/진단 제거. 씬 spacingX 1.5(원본).
- **유지(폐기 안 함)**: 로딩창(`LoadingScreen.cs` + `BattleManager.OnEnable` Cover + BattleLoop 로딩 hold/uncover + `battleEntryLoadingHold` 필드) — 사용자가 "나쁘지 않다"며 유지 요청.
- 별개 요청으로 유지: 파티 아코디언 기본 접힘(LeftPanelView), 재화 아이콘(영혼석=소울오브/마석), 마석상점→파워업, 카드 초상화·스킬아이콘.

### 노드맵 잔여 초록 노드 제거 (2026-06-11)
- `GamePlayScene` `Canvas/NodeDisplay/Viewport/Content` 의 번호 없는 첫 `NodeLevel`(초록 placeholder Image+Button, sprite 없음) 삭제. nodeRows(NodeLevel 1~10)에 미포함된 잔여물(옛 시작 노드)이라 노드맵 영향 없음. 백업 `remove_greennode_*`.

### 모든 팝업 Panel_1 통일 (2026-06-11)
- 점검 결과 설정/로그/교회/용병소(office)/모집/성장은 **이미 Panel_1**(9-slice) 사용 중이었음.
- **파워업(MagicStoneShopPanel)** 만 단색 다크였음 → `Resources/UI/panel_1`(Panel_1 복사본, border 60) 로드해 `panelImg`에 Sliced 적용. (코드빌드 패널이라 Resources 경유.)
- 파티편집은 Dim+카드(각 Panel_1) 구조 — 중앙 프레임 없이 유지(성장 패널과 동일, 정상).
- 검증: 파워업/설정 캡처로 Panel_1 9-slice 정상 렌더(공백/늘어남 없음) 확인.

### 승리/패배 결과 화면 (2026-06-11) — `UI/BattleResultScreen.cs` 신규
- 기존 `DisplayChange.ToggleResultDisplay`("Win/Lose" 텍스트)를 **비-튜토리얼 경로에서 새 화면으로 교체**(튜토리얼은 기존 유지).
- **승리**: Panel_1 팝업 — "전투 승리"(금) + "획득 재화" + "영혼석 +N  마석 +M" + **"다음으로"** 버튼(→`DisplayChange.ToggleDisplay()` 노드맵 + NodeMap BGM). 자체 생성형(DontDestroyOnLoad, sortingOrder 9990).
- **패배**: 전체 어둡게(alpha 0.9) + 중앙 **빨강 볼드 "게임오버"** + "클릭하여 계속"(클릭 시 `StartNextRunLoop()` 로그라이크 루프).
- `BattleManager.HandleBattleEnd` 비-튜토리얼 분기에서 `BattleResultScreen.ShowVictory(soul,mana,onNext)` / `ShowDefeat(onContinue)` 호출. 보상: 영혼석=`enemies.Sum(soulstoneDrop)`, 마석=`GrantBattleReward()`(int 반환으로 변경). 보스 클리어 엔딩은 기존 유지.
- 검증: 승리/패배 화면 캡처 확인(`Temp/result_victory.png`, `result_defeat.png`). 실제 전투 승/패 플로우는 빌드/플레이 확인 권장.

### ⚠️ 작업 지침 (사용자 요청) — 렉/렌더 글리치 의심 시
- 캐릭/오브젝트가 겹쳐 보이거나 이상 배치인데 **코드상 정상(MCP에서 정상 확인)**이면, 사용자에게 **유니티 종료 후 재부팅**을 권장할 것. (2026-06-11 배틀 중복 현상이 에디터 렉으로 판명된 사례)
- `LeftPanelView.Refresh`: 파티 아코디언 **자동 펼침 제거**(사용자 요청 — 불편). 모든 아코디언 기본 접힘으로 시작(프리팹 Image_Content 높이 0). `_partyAutoExpanded` 필드 삭제(데드코드).

### Panel_1 프레임 가시화 — 용병소·좌패널·파티편집 (2026-06-11)
> 위 "모든 팝업 Panel_1 통일"에서 "office/모집/성장 이미 Panel_1"이라 했으나 **어두운 틴트(0.04,0.06,0.085,0.96)로 덮여 프레임이 안 보였음** → 사용자 지적("설정/로그는 잘 했으면서 왜 빠져먹음")으로 수정.
- **용병소(office)/모집/성장/교회 Background** 색 → **흰색**(씬 인스턴스, 이미지별 SetDirty) — 설정/로그와 동일한 오너먼트 프레임 노출.
- **좌패널**: 루트 Panel_1 어두움(0.05,0.06,0.07)→흰색, Scroll View 흰 회색 박스→반투명(0.06,0.07,0.09,0.5). **LeftPanel.prefab에도 베이크**.
- **파티편집**: Panel_1 **WindowFrame(1840×980) 신규 추가**(BackgroundDim 다음 sibling), PartyArea(1000×760)@x600·ReserveArea(680×760)@우-440 재배치(카드 4장 프레임 안 수용), 예비대 ReserveScrollView 불투명 다크→반투명(0.06,0.07,0.10,0.45), PartySlots HLG spacing 20→16, Title (320,-130). **PartyEditPanel.prefab에 색·spacing 베이크**.
- **CardPrefab RoleBadge 150×165→108×132**(캐릭 너무 큼) + anchoredPos(0,-8).
- 캡처 검증: `~/Documents/_cap_recruit.png`·`_cap_leftpanel.png`·`_cap_partyedit.png`. 카드 뒤 흰 박스는 SelectionOutline(선택 강조)로 런타임엔 숨김(`FellowCardView` 바인드 시 SetActive(false)).
- 백업: `~/Documents/backup/panel1_fix_20260611_025551`(프리팹4), `partyedit_frame_20260611`(씬), `panel1_restore_20260611`(씬+좌패널/파티편집 프리팹).

### 🚨 색 유실 사고 + 복구 (2026-06-11) — **씬 저장 함정 3가지(중요!)**
> 사용자가 플레이 돌린 뒤 좌패널 루트색/스크롤뷰/예비대/spacing 4개가 옛값으로 복귀. 조사로 진짜 원인 규명:
1. **프리팹 인스턴스의 컴포넌트 값 변경은 `EditorUtility.SetDirty(컴포넌트)` 필수.** GameObject 단위 SetDirty + MarkSceneDirty + SaveScene(true 반환)만으로는 m_Modifications에 직렬화 안 됨 → 메모리/캡처는 정상인데 디스크는 옛값(리로드·플레이로 노출).
2. **`Resources.FindObjectsOfTypeAll` 탐색은 죽은 복제본을 잡을 수 있음**(씬 리로드 후). 수정용 탐색은 `scene.GetRootGameObjects()`→transform.Find.
3. **확실한 저장 검증 = `EditorSceneManager.OpenScene(scene.path)` 디스크 재로드 후 값 재확인.** SaveScene 반환 true는 보증 아님.
- 복구 완료(디스크 재로드 검증): 좌패널 루트 흰색·스크롤뷰 반투명·예비대 반투명·spacing16 + **프리팹 베이크로 이중 안전**. WindowFrame/영역재배치/용병소 흰색화는 유실 안 됐었음(신규 GO·RectTransform·이미지별 SetDirty는 저장됨).

### UI 오버사이즈 일괄 개편 (2026-06-11, #15~#20)
> 사용자 요청: "작게 보이는 데 오버사이즈" + 버튼 정리 + z-순서 버그.
- **팝업 z-순서 (#16)**: `PanelBase.Open()`에 `transform.SetAsLastSibling()` 1줄 추가 — 설정/로그/명단보기/판매픽커/파티편집 등 PanelBase 파생 전부 열릴 때 최상단. **플레이 모드 실증**: 설정→14/14, 로그→14(설정13), 좌패널 10. 팝업 열림 중 좌패널 접기버튼이 dim에 가리는 건 의도 동작(사용자 OK).
- **좌패널 오버사이즈 (#15)**: 아코디언(재화/파티/덱) 헤더 50→64·fs30·골드볼드·Button 흰색, 하단 버튼(파티편집/설정/로그) fs30·골드볼드, 하단 흰 'Image' 컨테이너 → **투명**(패널 일체화)+높이 210+VLG 패딩(20,20,8,14). Scroll View 750h로 축소(-495). LeftPanel.prefab + 씬 양쪽.
- **파티편집 세로 5:5 (#17)**: 파티(위 1700×440@-400)/예비대(아래 1700×380@-830). 슬롯 RowLabel(슬롯1~4) 삭제 + 슬롯 220×335·**localScale 1.10**(HLG childScale on, spacing20). 예비대 ScrollRect **가로 전환**(vertical 스크롤바 off) + Grid FixedRowCount=1·**cell(224,330)**(기존 180×240 찌그러짐 해소)·CSF horizontal. ToastLabel 프레임 아래(960,-1052). PartyEditPanel.prefab+씬.
- **카드 오버사이즈 (#18)**: 파티 슬롯 1.10배 + 예비대/픽커 그리드 셀 정상화(180×240→224×330)로 카드 원사이즈 표시.
- **나가기 버튼 통일 (#19)**: Exit/Close 8개 전부 **80×80 정사각 + 텍스트 라벨 제거**(Exit_Button_Normal 스프라이트에 X 내장). 패널 4종(-90,-90 anchor(1,1)): 용병소/모집/성장(MercenaryRoot.prefab)+파티편집(prefab). 팝업 4종(설정/로그/명단/판매)은 씬에서 라벨 SetActive(false).
- **용병소 메뉴 아이콘 (#20)**: 동료모집=`Hats_Knight_Helmet_Armor`(투구), 동료성장=`Boardgames_Card_Star`(카드+별) — 120×120 골드, 라벨 하단 정렬 (MercenaryRoot.prefab).
- **명단보기(FellowSourcePickerPopup) 표시 버그 (#20)**: 'Background'가 **흰색 솔리드 풀스크린**이라 가려 보였음 → 투명(레이캐스트 차단 유지). 그리드 2개 cell 224×330. PartyScrollView/ReserveScrollView 흰 반투명 → 다크 글래스(0.06,0.07,0.10,0.45). 판매픽커도 동일 처리.
- **성장 카드 Panel_1 (#20)**: SynthSlot1~3/ResultPreview의 Background(Panel_1)가 어두운 틴트(0.05,0.06,0.07)로 가려져 있던 것 → 흰색화(씬 오버라이드, SetDirty 컴포넌트). ※GrowthPanel은 fellowCardPrefab 없음(미리 배치된 FellowCardView 4개 구조).
- 검증 캡처: `~/Documents/_cap_leftpanel.png`·`_cap_partyedit.png`(더미6장)·`_cap_office.png`·`_cap_growth.png`·`_cap_picker.png`. 백업: `~/Documents/backup/oversize_ui_20260611/`(씬+프리팹4+UI/Mercenary 스크립트).

### 좌패널 Panel_1 HD 교체 (2026-06-11)
- 좌패널 중앙이 검게 보이는 문제: ① 스크롤뷰 어두운 덮개 → 투명화(raycast 유지), ② 사용자가 누끼 딴 고해상도 패널(`~/Downloads/panel1.png`, 1024×715 = 기존 624×436의 1.64배) 을 **`Assets/Foozle_UI_0001_RPG_Set_1/Panel_1_HD.png`** 로 임포트(Sprite, border 98=60×1.64, alphaIsTransparency).
- ⚠️ 사용자 파일 2차본에 **체커보드(투명표시 무늬)가 픽셀로 구워져** 있었음 → Unity에서 콘텐츠 박스 측정(x[18,1006] y[30,685]) 후 **989×656로 크롭**해 덮어씀(여백 제거). 크롭은 isReadable=true → GetPixels → EncodeToPNG 방식.

### Panel_1_HD 일괄 적용 + 버튼 통합 관리 (2026-06-11)
- **Panel_1 → Panel_1_HD 전면 교체**: 프리팹(LeftPanel 1·CardPrefab 1·MercenaryRoot 7·PartyEditPanel 5) + 씬 18곳. **spritePixelsPerUnit=164.06**(=100×1.6406)으로 임포트해 sliced 테두리가 자동으로 기존 60px급으로 렌더 — Image.pixelsPerUnitMultiplier는 전부 1로 통일(좌패널의 1.641 제거).
- **Resources/UI/panel_1.png 파일 자체를 크롭 HD로 교체**(GUID 유지) → 코드 로드처(파워업·BattleResultScreen·LoadingScreen·파티편집 WindowFrame) 자동 HD화. 임포트: border 98, PPU 164.06.
- **버튼 통합**: 사용자가 만든 `Assets/Resources/Button/`(default_button.png 200×52 스틸+블루라인, Exit_Button_Dark.png)에 **Foozle Button.png 복사**해 버튼 에셋 일원화. default_button 임포트: Sprite, border 14.
- **default_button 적용처**: 씬 12개(교회 NextNode/Hp/Stress·용병소 RecruitMenu/GrowthMenu·모집 Reroll/명단보기·성장 Synthesize+ActionButton×4) — 판별식: sprite==Button && 낡은 회색(0.17,0.2,0.24). + MetaShopButton(파워업). + **코드 2곳**: `BattleResultScreen.NewButton`·`MagicStoneShopPanel.NewButton` — `Resources.Load<Sprite>("Button/default_button")` Sliced 흰색, 실패 시 기존 단색 폴백.
- 🚨 **오적용 사고+원복**: sprite==null 버튼 일괄 적용 1차 스캔이 **노드맵 NodeLevel 클릭존 24개 + 전투 CardArea 이미지 4개**(GamePlayScene_RightMainArea.prefab)까지 물들임 → 즉시 sprite=null·흰색 원복(원래가 흰 사각 placeholder). 노드맵 캡처로 복구 확인. **교훈: 버튼 일괄 스타일링 시 NodeLevel/CardArea/투명 클릭존 제외 필수.**
- 검증: `_cap_nodemap.png`(원복)·`_cap_office.png`·`_cap_growth.png`(HD 슬롯+스틸 버튼)·`_cap_partyedit.png`(HD 카드). 컴파일 0. 백업: `~/Documents/backup/panel_hd_rollout_20260611/`.

### 재화 규칙 수정 — 전투 마석 지급 폐지 (2026-06-11, 기획 §15 준수)
> 사용자 지적: 노드(전투) 종료 시 영혼석·마석 둘 다 상승. 기획 `시스템/15_보상_시스템_명세.md` = **전투 보상은 영혼석만**(처치 즉시 드롭·자동수거), 마석은 "런 종료 후 영혼석→마석 치환(비율 미결)" **백로그**.
- `BattleManager.Phases.cs`: **`GrantBattleReward()` 삭제**(전투마다 마석 10/20/30 지급하던 7차 코드 — 데드코드 정책으로 메소드째 제거). 승리 분기에서 호출 제거.
- `BattleResultScreen.ShowVictory(int soul, Action)` 으로 시그니처 변경 — 팝업 표시 "영혼석 +N" 만. (영혼석 자체는 전투 중 처치 시 즉시 지급되는 기존 구조 유지 — 팝업은 합계 표시용.)
- ※ 현재 마석 수입 경로 없음(의도) — 백로그의 "런 종료 영혼석→마석 치환" 구현 시 추가(치환 비율 기획 확정 필요).

### 파워업 팝업 안 보이던 버그 수정 (2026-06-11)
- 원인: **NodeDisplay(노드맵)가 런타임에 SetAsLastSibling 되어 위로 올라옴** → 상점(MagicStoneShop, sibling 8 고정·PanelBase 미상속이라 z-수정 누락분)이 노드맵 **뒤에서** 열려 안 보였음. Open() 자체는 정상(IsOpen=true 확인).
- 수정: `MagicStoneShopPanel.Open()` 에 `transform.SetAsLastSibling()` 추가. **플레이 실증**: Open 후 sibling 14/14 + 캡처(`_cap_powerup.png`)로 노드맵 위 표시 확인.

### 예비대 보기 개편 + 카드 프리팹 240×380 확대 + 버튼 텍스트 가독성 (2026-06-11)
> 사용자: 예비대 보기 카드 크기 맞춤·스크롤바 숨김·카드 배경이 구성 대비 작음·버튼 텍스트 안 보임.
- **CardPrefab 220×330 → 240×380**: Background/SelectionOutline(260×400) 동반 확대, RoleBadge (0,-14), 라벨 상단앵커 재배치(Name fs22@-152 / HP·성향 fs16@-188 / 스킬 fs16@-218).
- **버튼 텍스트 원인**: ActionButtonLabel이 **진회색(0.196)** — 다크 스틸 default_button 위에서 안 보였음 → **fs26 골드 볼드** (FellowCardView는 text만 세팅하므로 프리팹 스타일 안전). ActionButton 200×46 + default_button.
- **RemoveButton 정체**: 'DECLINE' 가로바 스프라이트를 28×28로 찌그러뜨려 쓰고 있었음 → **Resources/Button/Exit_Button_Dark(X 사각형) 32×32** + 라벨 비활성. 성장 SynthSlot1~3/ResultPreview도 동일(라벨 골드 포함, MR 프리팹+씬).
- **파티편집 재조정**(새 카드 맞춤): PartyArea(1700,440)@-390·슬롯 240×385·**스케일 1.10 제거**(카드 자체 확대로 대체), ReserveArea(1700,418)@-821·뷰포트 384·그리드 셀(244,380). PartyEditPanel.prefab+씬.
- **픽커 2종(동료 명단/판매)**: 그리드 셀(244,384)·**3열**·spacing(16,12), 스크롤바 GO off+ScrollRect 참조 null(드래그/휠 스크롤은 유지). 모집 CandidatesParent HLG spacing 24.
- ⚠️ **활성 씬 함정**: 사용자가 타이틀(GameStartScene)에서 플레이 중이라 활성 씬이 바뀌어 있었음 → 씬 작업 전 `GetActiveScene().name` 확인, GamePlayScene 열고 작업 후 원래 씬 복귀. (이번 NRE 원인)
- 검증 캡처: `_cap_partyedit.png`·`_cap_picker.png`(선택 골드 가독 ✓)·`_cap_growth.png`. 백업: `~/Documents/backup/cardprefab_resize_20260611/`.

### 버튼 비트음 전부 제거 (2026-06-11)
- **UIButtonSfxInstaller 삭제** — 씬의 모든 Button.onClick에 ButtonClick 비트음을 자동 장착하던 스크립트. 양 씬(AudioManager GO)에서 컴포넌트 제거 후 .cs 삭제 (외부 Rescan 호출 0 확인).
- 명시 호출 제거: LeftPanelToggle(접기 클릭음)·MoveScene×2·TutorialGuidePanel×2·MagicStoneShopPanel×3 — 전부 Confirm/ButtonClick 계열.
- **유지**: 전투(타격/스킬/아군사망/패배음), 노드 이동/진입, 씬 전환, 카드 뽑기(CardDraw), 모집/판매/코인(Recruit/Sell/CoinSpend), **스택 카드 선택/사용/해제(CardSelect/CardPlay/Cancel — 버튼 아닌 카드 조작음. 원하면 제거 가능)**. SfxId enum의 ButtonClick/Confirm 값은 매핑 호환 위해 유지(호출 0).
- **추가 제거(사용자 요청 2차)**: **적 처치음(EnemyDeath, EnemyData.Hp.cs)** + **승리음(Victory ×2, BattleManager.Phases.cs — 튜토리얼/일반 경로)**. 호출 0 확인.
- **추가 제거(3차)**: **아군 사망음(FellowDeath)** + **패배음(Defeat ×3)**. 전투 사운드 중 남은 것: 타격(AttackSword/HurtAlly/HurtEnemy)·스킬·힐·스택카드 조작음·노드/씬 전환·재화.

### 승패 결과 표시 단축 (2026-06-11)
- HandleBattleEnd 가 결과 표시 전 `gameOverDelay(1.5s)×2 = 3초` 대기하던 것 → **비튜토리얼은 신규 `resultPopupDelay = 0.6s`** 한 번만 (마지막 처치 연출 호흡). 튜토리얼 Win/Lose 팝업 흐름(1.5→토글→1.5)은 유지. 인스펙터에서 조정 가능.

### 보스 처치(엔딩) 팝업 UI 적용 (2026-06-11)
- 원인: `PopUp`(RightMainArea 프리팹) 루트가 **흰색 풀스크린** + 자식 `Result` TMP 가 파랑 'New Text' 방치 — 엔딩은 그 위에 글자만 얹혀 회색+겹침으로 보였음.
- 수정: PopUp 루트 → 다크 dim(0,0,0,0.8) / Result → 빈 문자열+골드 볼드 96(튜토리얼 Win·Lose 용 스타일 정리) [프리팹+씬]. **EndingPanel(씬 전용 오브젝트) 재구성**: dim 0.92 + **Panel_1 HD 프레임(760×440)** + EndingText(골드 볼드 56, 코드가 메시지 set — 프레임 첫 자식 유지) + 하단 힌트 "잠시 후 새로운 탐사가 시작됩니다…". 캡처 `_cap_ending.png`.

### 텔레포트 순서·투사체 시작점·스킬 전용음 타이밍 (2026-06-11)
- **순간이동 = 모션 후 배치**: `ExecuteTeleportSkill` 코루틴화 — Attack3 모션+후방 잔상(≈2.1s) **완료 후** allies.Reverse + 슬롯 재할당 + 순차 재배치(+재배치 대기). 시전부(ExecuteEnemySkillCast)가 yield 로 대기.
- **캐스터 투사체 시작점**: SkillEffectFx 발사 오프셋 — 지팡이 끝(높이 1.15+전방 0.55) → **몸통(높이 0.7, 전방 0)** 으로 변경.
- **스킬 전용음 = 타격 순간**: 시전 직후 재생하던 파이어볼/무모한강타/매직미사일 전용음을 제거하고, `_castImpactSfx` 로 **DealDamageToEnemy 의 디폴트 타격음(AttackSword) 자리에서 1회 대체 재생**(AOE 도 첫 타격만, 피격음 HurtEnemy 는 유지). UseSkill 종료 시 플래그 리셋.

### 팝업 버튼 라벨 골드+볼드 통일 (2026-06-11)
- 텍스트 있는 모든 버튼 라벨에 좌패널 스타일(골드 1,0.84,0.4 + Bold) 적용 — 폰트 크기는 각자 유지. 프리팹 5종 19개(MercenaryRoot 9·PartyEdit 4·CardPrefab 1·RightMainArea 5) + 양 씬 31개 + 코드 생성 1곳(MagicStoneShopPanel.NewButton — 해금/닫기). 멱등 가드(이미 골드+볼드면 스킵). 캡처: `_cap_setting.png` (게임 종료 골드 확인).

### 설정창 '재화 초기화' 제거 (2026-06-11)
- SettingPopup.cs: `resetPrefsButton` 필드·구독·`OnResetPrefs()` 삭제(데드코드 정책). 양 씬(GamePlay/GameStart)의 **ResetPrefsButton GO 삭제**. 재화/메타패시브 리셋이 필요하면 **디버그 툴(F1)** 의 재화 0 버튼 사용. ※MetaPassiveManager.ResetAll 호출처가 사라짐 — 디버그 툴에는 메타패시브 리셋 버튼 없음(필요 시 추가).

### 미사용 에셋 대청소 — 약 1,900파일 삭제 (2026-06-11)
> 방식: 전 직렬화 파일(.unity/.prefab/.asset/.controller/.anim/.mat)에서 참조 GUID 1패스 수집 → 후보별 대조. **Resources 는 문자열 로드 가능성 때문에 코드 grep 교차 확인** (node_*.png 가 GUID 무참조인데 `IconSprite("node_combat")` 로 사용 중이던 것을 이걸로 잡음 — 보존!).
- 삭제: **Free Pack 통째**(wav 44 전부 미참조 — 실사용 클립은 `오디오추가/` 10개 + 400팩 18개), **1-bit 아이콘 1,473 + 시트 19**(보존 4: 마나스톤/크리스탈볼/투구/카드별), **Foozle 30**(보존: Button/Coin/Exit 3종/Panel_1/Panel_1_HD/Readme), **400 Sounds Pack 384**(보존 18 = SoundDatabase 참조분), **빈 폴더 5**(Test_Image/free horror ambience 2/Door팩/Material/Editor).
- 검증: 리프레시 후 **에러 0**. 폰트 경고 발견 → 가운뎃점(·)이 NanumGothic 에 없어 □ 깨짐: 디버그 라벨·툴팁 구분자를 ASCII(|, /, +, 괄호)로 교체.
- 백업: **`~/Documents/backup/cleanup_20260611/`** (트리 보존 3,900여 파일 — 복구 시 그대로 덮어쓰면 됨).
- ⚠️ 잔존 추적: "referenced script (Unknown) missing" 경고 1건이 간헐 출현(전 프리팹+양 씬 스캔은 0건) — 플레이 세션 잔재로 추정, 재발 시 전 프리팹 스캔 재실행.

### 보스 행동 패턴 기획 §11 §3 원안 복귀 (2026-06-11, 사용자 결정)
> 운용 규칙 대조에서 기획과 다른 2건 발견 → 사용자 확인 후 기획대로 수정.
- **① 확정 소환 규칙 [J] 폐기**: "필드에 까마귀 없으면 무조건 소환"(과거 구두 결정, 60/40 룰렛을 사문화시킴) 제거 → **기본 상태 매 턴 60% 휘두르기 / 40% 까마귀 부름 룰렛** 복귀. 생존 중/쿨다운 중 제외는 기존 룰렛 가드 유지.
- **② 재소환 쿨다운 시점**: 소환 시점 → **까마귀 사망(처치·자폭 공통) 시점부터 3턴** (기획 "처치 시 재사용 대기 3턴"). 구현: 소환 시 Summon 타입은 캐스트 쿨다운 제외 + `ExecuteSummonSkill` 에서 각 까마귀 `OnDied += owner.StartSkillCooldown(summon, 3)` (마지막 사망 기준 갱신, owner.isDead 가드).
- 효과: 까마귀를 빨리 잡을수록 다음 소환이 늦어지는 보상 구조 + 소환 타이밍 랜덤화. 순간이동(만료→예약) 흐름은 무변경.

### 보스 스킬별 애니메이션(Attack1~4) + 순간이동 후방 잔상 + 까마귀 표기 (2026-06-11)
- **트리거 시스템 4슬롯 일반화**: `BattleCardSprites` — `_hashAttack1/2` 쌍 → `_attackHashes[4]/_hasAttack[4]` 배열. `AttachAnimator(animator, string[] attackNames)` 신설(기존 2-파라미터는 호환 위임). `TriggerAttackImmediate(skillIndex)` — 해당 인덱스 트리거 없으면 아래로 폴백. 기본명 Attack/Attack2/**Attack3/Attack4**.
- **데이터**: EnemyDef/EnemyData/EnemyDatabase 에 `attack3Anim/attack4Anim` 추가. enemies.json 보스: **skillIds 에 teleport 추가**(인덱스3 — weight 0 이라 룰렛 영향 없음, OnSkillCast 인덱스용) + attack3/4Anim 명시. 매핑(사용자 확인): 휘두르기=Attack1(근접 dash) · 까마귀 부름=Attack2 · **수확=Attack4 · 순간이동=Attack3** (attack3Anim="Attack4"/attack4Anim="Attack3" — 필드명은 스킬 인덱스, 값은 트리거명).
- **순간이동 보강**: ① `allies.Reverse()` 후 **battleSlotIndex 재할당 + RelayoutNow** (기존엔 리스트만 뒤집혀 카드 위치 안 바뀌던 잠재버그). ② 연출 — `PlayTeleportGhost(후방pos)`: 0.45s(Attack4 모션) → 페이드아웃 → **아군 최후방(-1.6) 잔상(α0.45) 0.5s** → 원위치 페이드인 (기획 §11 §3 연출 가이드 충족).
- **까마귀 카운트다운 표기**: 내부 +1 보정 제외하고 표시 — 소환 직후 "자폭까지 3턴"(기획 일치), 0 되면 "이번 턴 자폭!".

### 적 플립·보스 검증·까마귀 카운트다운·보스 BGM 1회 (2026-06-11)
- **플립**: 새 아트 3종(Wolf/Scarecrow/Crow) 모두 **좌향으로 그려짐** — `SetFacing(isEnemy ^ flipSprite)` 구조상 고블린처럼 `flipSprite: true` 필요 → enemies.json 3종 추가. **보스전 캡처로 좌향 확인**.
- **보스 BGM 1회 재생**: `AudioManager.PlayBgm/PlayBgmById 에 loop 파라미터(기본 true)` 추가, NodeSystem 보스 진입만 `loop:false`. 플레이 검증: clip=보스 노드, loop=False ✓.
- **보스 스킬 검증(플레이)**: 보스 노드 강제 진입(OnNodeClicked) → Scarecrow Idle 6프레임 순환 ✓ / SetTrigger Attack1→Scarecrow_Attack_1~3 ✓ / Attack2→Attack2_1~3 ✓ / 까마귀 부름(ExecuteSummonSkill 직접 호출)→까마귀 2마리 소환·수명 4턴 ✓. 수확(Harvest)은 정적 검증(HP≤50% 1회 강제 발동 + 실드 제외 드레인 코드) — 코루틴이라 라이브 생략.
- **까마귀 카운트다운**: 이미 구현돼 있었음(BattleCardView가 summonLifeTurns>0 시 HP 아래 'CrowCountdownText' 동적 생성 + OnLifeTurnsChanged 구독). 소환 직후 **"자폭까지 4턴" 표시 확인** — 매 턴 끝 감소 갱신.
- ⚠️ **검증 함정**: 에디터 비포커스 시 프레임 정지 → 애니메이터 normTime 0·LoadingScreen 안 걷힘 — `Animator.Update(dt)` 수동 펌핑으로 우회 검증. 실플레이(포커스)에선 무관.

### 화톳불 BGM 제거 + 적 종류별 애니메이터 적용 (2026-06-11)
- **Rest BGM 제거**: NodeSystem 의 `PlayBgmById(BgmId.Rest)` 2곳(화톳불·교회 — 같은 트랙) 삭제. 진입해도 노드맵 BGM 유지(이탈 후 Rest 브금이 계속 남던 문제 해소). BgmId.Rest enum/매핑은 유지(호출 0).
- **적 애니메이터** (enemies.json — 사용자가 만든 컨트롤러 연결): 약탈자=`Animators/Enemies/Wolf/Wolf`(트리거 기본 Attack/Attack2), 거두는 자=`Scarecrow/Scarecrow`(**attack1Anim=Attack1, attack2Anim=Attack2 명시** — 트리거명이 기본값과 다름. Attack3/4는 파이프라인 미사용), 까마귀=`Crow/Crow`(Idle 전용, 파라미터 없음 → 트리거 안 씀). 고블린은 기존 연결 유지.
- 검증: 컨트롤러 4종 Resources.Load OK + JSON 경로/트리거 포함 확인. 실제 모션은 전투에서 확인 필요.

### 덱 더미 UI + 남은 장수 표시 (2026-06-11)
- **기존 `GamePlayScene_RightMainArea/Deck`(172×280, 우하단 빈 흰 박스 placeholder) 활용** (사용자 지시 — 신규 DeckPile 은 만들었다가 폐기): 루트 Image 비활성(흰 박스 제거) + 자식으로 `sprite_sheet_9`(파랑 카드 프레임) **3장 겹침**(Back2/1/0 깊이감) + **CountText**(흰 볼드 56) + "덱" 골드 라벨 + `DeckPileView` 부착.
- **`UI/DeckPileView.cs`** 신규: `GameManager.RemainingDeckCount`(신규 public 프로퍼티 = drawDeck.Count - currentDrawIndex) 폴링해 숫자 갱신.
- **씬 GameManager.cardStackAnchor = Deck 연결** → 기존에 null 이라 생략되던 **드로우 애니메이션(덱→손패 날아가기)이 활성화됨**. 어색하면 cardStackAnchor 만 비우면 원복.
- 디버그툴 즉시승리/패배: HP 0 + **DebugForceBattleEnd()**(StopAllCoroutines→BattleEnd 직행 — 입력 대기 없이 결과 화면). 디버그창 **드래그 이동**(DragMove, 빈 영역 잡고 끌기).

### #14 디버그 툴 — F1 팝업 (2026-06-11, 에디터/개발빌드 전용)
- **신규 `Debugging/DebugToolPanel.cs`**: `#if UNITY_EDITOR || DEVELOPMENT_BUILD` 전체 가드, RuntimeInitializeOnLoad 자가생성(DDOL, 씬 배치 불필요), **F1 토글** 우측 팝업(Panel_1 HD + default_button).
  - 재화: 영혼석 +100/-100/**0** · 마석 +50/-50/**0** (감소·초기화 — 사용자 요구)
  - 전투(전투 중 가드): 즉시 승리(적 전멸)/즉시 패배/풀힐·스트레스0/스택 999
  - 진행: 층 전진(구 F2=`NodeSystem.CheatAdvanceFloor`)/새 런 시작(StartNextRunLoop 동일 절차 독립 구현 — 노드맵에서도 동작)
  - 스킬/연출 테스트(전투 중 단축키, 3차 개편): **1·2=아군 스킬1·2, 3·4=적 전원 스킬1·2, 5·6=적(보스) 스킬3·4 — 전부 모션+효과 실제 적용**. 2차의 '모션 전용'은 사용자 피드백(데미지 안 들어감·까마귀 안 생김)으로 폐기 — `ExecuteEnemyTurn` 에서 시전부를 **`ExecuteEnemySkillCast(enemy, skill)` 로 분리**해 본전투와 디버그가 동일 경로 공유(사운드/쿨다운/모션/데미지/소환/순간이동/수확). passive(까마귀) 제외. 패널 버튼: + 적 랜덤 행동/피격 모션. 박스 높이 880.
- **신규 `BattleManager.Debug.cs`**(파셜, 동일 가드): DebugKillAllEnemies/Allies·DebugFullHeal·DebugCastAllAllySkills(내부 UseSkill 순차)·DebugEnemyTurnOnce(ExecuteEnemyTurn)·DebugHitMotionAll(OnDamaged(0,1)).
- **구 `CheatInput.cs` 삭제**(F1 즉발치트/F2 — 팝업으로 대체. 씬 컴포넌트 제거 후 파일 삭제, 백업 보관).
- 검증(플레이): 자가생성 ✓ F1 토글 ✓ 영혼석 10134→+100→-100→0 ✓ 마석 ±50 ✓ 비전투 시 전투버튼 가드 ✓ 캡처 `_cap_debugtool.png`.

### #11 스킬 호버 툴팁 재설계 — 이미지1 사양 (2026-06-11)
- `SkillTooltipController` 내부 재작성: 단일 리치텍스트 → **구조화 엔트리 2개(코드 생성)**. 엔트리 = [아이콘 52(타입색 틴트) | 이름+`[타입 · 대상]`칩 / `위력 N · 사거리 X · 코스트 M` 스탯줄] + 설명(줄바꿈) + 2번째 엔트리 위 구분선. VLG/CSF 자동 높이.
- 타입 라벨/색: 공격=빨강·회복=초록·실드=파랑·강화=금·약화=보라·공격+실드/도발=주황. **사거리**: Damage계=isRanged(원거리/근접), 그 외=자신/아군 지원.
- `ShowText`(상태이상 칩 툴팁)는 엔트리 숨기고 기존 Body 텍스트 모드로 동작 — 호환 유지. 트리거(CardSlotView/FellowCardView)는 무변경(Show API 동일).
- 씬: TooltipBox → Panel_1_HD 프레임 + 폭 440 + VLG 패딩(20,20,16,16)·childControlWidth.
- **검증(플레이)**: 발도 → 아이콘 skill_draw 빨강, "[공격 · 단일 적]", "위력 25 · 사거리 근접 · 코스트 2", 설명 ✓ / 매직 실드 → "[실드 · 단일 아군]", "사거리 아군 지원" ✓. (RT 캡처는 SkillTooltip 로컬 z=-100 탓에 번번이 컬링 — 내용 덤프로 검증, 실호버 확인은 사용자)
- ⚠️ 캡처 메모: SkillTooltip GO는 **로컬 z=-100** — 임시카메라 RT 캡처 시 근평면에 잘림. 백업: `~/Documents/backup/skilltooltip_20260611/`.

### 승리/패배 자동 진행 + Card.prefab missing script 정리 (2026-06-11)
- **승리/패배 화면 클릭 필수 → 자동 진행**: `BattleResultScreen` 에 `AutoAdvanceDelay=2.5f` — 표시 후 2.5초면 자동으로 다음(승리=노드맵 / 패배=새 런). **클릭/다음으로 버튼은 즉시 스킵**으로 유지. 버튼·자동이 같은 Proceed 경로 공유 + activeSelf 가드로 중복 발화 차단, Hide 시 코루틴 정지. 패배 힌트 문구 "잠시 후 계속됩니다 (클릭 시 즉시)".
- **콘솔 missing script 경고 근원**: `Assets/Prefab/Card.prefab` 루트에 깨진 스크립트 참조 1건 → 제거. 프리팹 전수(Prefab/Resources) + 양 씬 재스캔 0건, 콘솔 클린.

### 파티편집 토스트 제거 + 헤더 정밀 배치 (2026-06-11)
- **토스트/안내문 제거(사용자 요청)**: PartyEditPanel.cs 에서 statusLabel·toastDuration·DefaultGuide·UnsupportedMessage·_toastRoutine·ShowGuide/ShowToast/RestoreGuideAfter **전부 삭제**(데드코드 정책, using System.Collections 도 제거). ToastLabel GO 프리팹에서 삭제(씬 전파).
- **헤더 겹침 수정**: '파티'(인원 N/4)/'예비대' 라벨이 카드 상단과 수px 겹쳐 있었음 → 전용 밴드 확보: PartyHeaderLabel (0,-18) 500×32 fs28 / PartySlots 인셋 T36 B2 / ReserveHeaderLabel (0,-14) 300×28 fs26 / ReserveScrollView T30 B0 → **뷰포트 400 = 셀 400 정확**(패딩 0). 프리팹+씬.
- 양 씬 missing script 스캔: 0건 (콘솔의 1회성 경고는 플레이 세션 잔재로 판단).

### 보스 진입 문소리 + 적 데미지 타이밍 수정 (2026-06-11)
- **문 열림음 계속 울림**: 전투 노드 진입음(NodeEnter)이 원본이 길어 보스 진입 후 계속 재생 → `PlaySfxByIdClipped(SfxId.NodeEnter, 1.5f)` 로 1.5초 컷 (NodeMove 와 동일 기법, NodeSystem.cs).
- **적 공격 딜 지연**: 아군 근접은 `meleeImpactDelay=0.55s`(휘두르는 순간) 인데 적은 `impactDelay=1.25s` — **모션이 다 끝난 뒤 데미지**가 들어갔음(사용자 체감 "1턴 늦게"). EnemyAction 3곳(Fallback 직타/스킬/수확) 전부 `meleeImpactDelay` 로 교체 → 아군과 동일하게 타격 순간 적용. **`impactDelay` 필드 삭제**(사용처 0, 데드코드 정책). ⚠️ 타이밍 체감은 실플레이 확인 필요 — 빠르/늦으면 meleeImpactDelay 또는 적 전용 상수로 미세조정.

### 용병소 기획(§14) 대조 + 명세 위반 2건 수정 (2026-06-11)
> 일치 확인: 후보 3인 / 리롤 2→+1 누적·노드 진입 리셋·무한 / 예비대 9칸 / 빈 슬롯 우선 합류 / 중복 직업 허용 / 1성 30 (승급 추가비용 없음 = 명세 "현재는" 조항 그대로) / 부족 시 실행 차단.
- **수정① §7-1 실패 사유 메시지**: Debug.Log뿐이었음 → `RecruitPanel.statusLabel`(빨강 fs28, MR프리팹에 StatusLabel GO 생성+SerializedObject 연결) + ShowToast 2초: 고용/리롤 실패 시 "영혼석이 부족합니다 (필요 N)" / "파티와 예비대가 가득 찼습니다".
- **수정② §8-3 비용 부족 빨간 경고**: 버튼 비활성뿐이었음 → `FellowCardView.SetInteractable`에서 costLabel 색 흰↔빨강(0.9,0.25,0.25) 토글.
- **판단 유보(보고)**: ⓐ §7-2 "만석 시 교체 모드" vs 현행 "만석 고용→예비대 직행" — §5-3 "파티 **또는 후보 대기열**에 반영"과는 부합, 교체는 파티편집 담당. 강제 교체 선택 UI 원하면 별도 작업. ⓑ §3 "동료 성장 MVP 배제" vs 성장(합성) 구현됨 — **문서가 뒤처진 것**(성급 비용표가 성장 전제). 코드 유지, 기획 문서 갱신 권장.
- 백업: `~/Documents/backup/sfx_mercspec_20260611/`.

### 미행동 리그룹 겹침 — 딜레이+순차 이동 (2026-06-11)
> 미행동 재정렬(예: 1-2-3-4→3-1-2-4) 시 카드들이 **동시에 직선 이동하며 교차** → 몸 관통 겹침. 딜레이만으론 교차 자체가 안 사라져 **스태거(순차 출발)** 조합으로 해결 (사용자 선택).
- `DefaultSetting.cs`: `RelayoutStagger=0.1f` 신설. `RelayoutCards`→`PlaceCardAt(..., delay)` — instant 아닐 때 **왼쪽 자리부터 k×0.1초 시차** 출발(`DOMove().SetDelay`). 사망/적 재정렬도 동일 적용(연출 일관).
- `BattleManager.Combat.cs` 미행동 블록: 재정렬 **전 0.25초 사전 호흡** + 대기시간을 `RelayoutDuration + Stagger×(생존아군-1)` 로 보정(적 턴이 이동 중 시작 안 되게).
- ⚠️ 트윈 체감은 MCP 프레임 정지로 검증 불가 — **실플레이 확인 필요**. 튜닝 노브: `RelayoutStagger`(0.08~0.12), 사전 호흡(0.25f). 백업: `~/Documents/backup/regroup_stagger_20260611/`.
- LeftPanel 루트 Image: sprite=Panel_1_HD, Sliced, **pixelsPerUnitMultiplier=1.641**(1024/624 — 화면상 테두리 두께를 기존 60px급으로 유지). 프리팹+씬.
- 다른 Panel_1 사용처(팝업/카드/파티편집 프레임)는 아직 원본 — 필요 시 같은 방식(sprite 교체 + ppum 1.641)으로 일괄 교체 가능. 백업: `~/Documents/backup/panel1_hd_20260611/`.

### 까마귀 자폭 카운트다운 가시성 수정 (2026-06-11)
> 사용자: "까마귀 자폭하는거 어디서 뜨니? 안 보이던데" — 텍스트는 생성됐지만 **화면 밖 y=-3102px**에 있었음.
- **근본 원인**: `EnsureCountdownText`가 HP 텍스트(rectTransform)를 복제 후 `rect.height × 1.1` 만큼 아래로 내렸는데, HP 텍스트가 **스트레치 앵커**라 rect.height = 카드 전체(~2800 캔버스 단위) → 오프셋이 화면 밖 3천px. 추가로 HP rect 폭이 "2/2" 기준이라 긴 문구는 truncate.
- **수정** (BattleCardView.cs `EnsureCountdownText`/`UpdateCountdownText`):
  - 배치 기준을 rect → **렌더된 글리프 경계(`hpScoreText.textBounds`)**로: `ForceMeshUpdate()` 후 글리프 하단 - fontSize×0.4 지점을 `TransformPoint`로 월드 변환, 비스트레치 앵커(0.5,0.5)+pivot(0.5,1)로 고정 배치.
  - sizeDelta = fontSize×(14, 2.2) + `Overflow`+`NoWrap`+Center — truncate 차단.
  - 스타일: fontSize **HP×1.25 볼드 주황**(1,0.55,0.25), 마지막 턴 **빨강**(1,0.2,0.2).
  - 문구: "자폭까지 N턴" → **"자폭 N턴" / "자폭!"** — 까마귀 2마리 나란히 설 때 옆 텍스트와 겹치던 것 해소 (×1.5 폰트도 같은 이유로 ×1.25).
- **검증(플레이)**: 보스 노드 강제 진입(currentRowIndex=5 → OnNodeClicked(5,2) — row==currentRowIndex 여야 통과!) + ExecuteSummonSkill reflection → 까마귀 2마리 각각 HP "2/2" 바로 아래 "자폭 3턴" 주황 볼드, 픽셀 캡처 확인 ✓.
- ⚠️ 에디터 비포커스 시 stop→play 만으론 **컴파일이 안 돔** — `refresh_unity(compile=request)` 후 진행할 것 (이번에 구코드로 2회 헛검증).
