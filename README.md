# 괴이탐사국

다크 호러 로그라이크 탐험과 공용 카드 스택 기반 자동 전투를 결합한 Unity 캡스톤 프로젝트입니다.

> 현재 기획의 기준은 **Notion**입니다. 이 저장소의 과거 기획 문서는 2026-08-01 이전 기록을 보존하는 아카이브이며, 최신 기획으로 사용하지 않습니다.

## 처음 확인할 곳

1. [16. 프로토타입 개발 사양서](https://app.notion.com/p/3b96aef5baac81209a2dcf56163e0ea5) — 구현 범위·작업 순서·합격 기준
2. [괴이탐사국 팀 기획서](https://app.notion.com/p/3af6aef5baac800298b6c2154b9b2699) — 최신 기획 진입점
3. [기획서 안내 · 문서 지도](https://app.notion.com/p/3af6aef5baac818e95d2c2a39782a267) — 역할별 읽는 순서
4. [15. 미결 안건표](https://app.notion.com/p/3af6aef5baac8185866ff29253c2e69f) — 지금 결정할 항목
5. [HANDOFF.md](HANDOFF.md) — 현재 구현 상태와 기획 차이
6. [AGENTS.md](AGENTS.md) — 팀 공통 작업·검증 규칙
7. Unity Hub — Unity **6000.3.9f1**로 이 폴더 열기

## 정보가 관리되는 위치

| 정보 | 기준 위치 |
|---|---|
| 최신 게임 기획·미결 상태 | Notion `기획 문서`와 `15. 미결 안건표` |
| 전체 기획 문서 목록 | Notion [기획 문서 데이터베이스](https://app.notion.com/p/48e78f58eca144708c66425d28b88b10) |
| 용어·디자인·생성 이력 | Notion [용어 사전](https://app.notion.com/p/229e9a1f30df495d996a0eb52abc1525) · [디자인 가이드](https://app.notion.com/p/e749c2ad0b9a417f91ec85ce1641c73a) · [AI 디자인 생성 기록](https://app.notion.com/p/5c53f3ac5a5d47359d6c8e8c419254a1) |
| 실제 구현 | `Assets/`, `Packages/`, `ProjectSettings/`와 직접 확인한 테스트 결과 |
| 현재 구현 인수인계 | `HANDOFF.md` |
| 과거 기획·디자인 기록 | `기획/`, `기획_통합/`, `디자인_UI/`, `archive/` |
| 제출 패키지 | `제출용/` |

## 저장소 구조

- `Assets/` — Unity 코드·씬·프리팹·게임 에셋
- `Packages/`, `ProjectSettings/` — Unity 패키지와 프로젝트 설정
- `운영/` — Git, 회의, 코드 작성 등 저장소 협업 규칙
- `회의록/`, `주간_체크리스트/` — 팀 운영 기록
- `docs/` — 구현 인수인계 이력과 병합 기록
- `기획/`, `기획_통합/`, `디자인_UI/` — 읽기 전용 과거 자료
- `archive/` — 병합·제출·체크리스트 보관본

## 협업 원칙

- 기획을 바꾸기 전에 Notion 원문과 미결 상태를 확인합니다.
- 코드·씬·프리팹 변경은 현재 체크아웃과 직접 검증 결과로 판단합니다.
- 구조나 파일 이름을 바꾸면 README, [로컬 프로젝트 허브](00_프로젝트_허브.md), 관련 링크를 함께 점검합니다.
- 상세 규칙은 [협업 운영 규칙](운영/00_협업_운영_규칙.md)과 [Unity C# 코드 작성 규칙](운영/01_코드_작성_규칙.md)을 따릅니다.
- 사람과 자동화 작업자는 공통으로 [AGENTS.md](AGENTS.md)의 기준 문서·변경 안전·검증 원칙을 따릅니다.

## Git 추적 범위

- 추적: `Assets/`, `Packages/`, `ProjectSettings/`, 팀 공통 문서와 규칙
- 비추적: `Library/`, `Temp/`, `Obj/`, `Build/`, `Logs/`, `UserSettings/`, IDE 생성 파일과 개인 도구 상태
