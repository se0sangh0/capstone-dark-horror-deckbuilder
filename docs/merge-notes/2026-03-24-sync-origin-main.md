# Sync origin/main → feat/docs-chocochip (2026-03-24)

요약
- origin/main의 최근 변경(5 커밋)을 feat/docs-chocochip 브랜치에 병합(FF, fast-forward)하였습니다.
- 병합은 fast-forward로 진행되어 별도 병합 커밋 없이 반영되었습니다.

병합된 커밋(간단)
- 0867e26 — node 병합 및 파일 위치 변경
- e2cbb39 — Merge remote-tracking branch origin/feat/Node into feat/UI
- 0920e80 — feat 시작화면 및 전투UI 구성
- f2214e9 — feat: 전투 화면 <-> 노드 화면 전환 기능 구현
- eaa122d — feat: 노드 이동 구현

주요 변경 하이라이트
- 새로 추가된 씬/프리팹
  - Assets/Scenes/GamePlayScene.unity (신규)
  - Assets/Prefab/NodeLevel.prefab (신규)
- 스크립트 추가/수정
  - Assets/Scripts/Node/NodeSystem.cs 등 노드 관련 스크립트 추가
  - Assets/Scripts/GameManager.cs 등 일부 게임 로직 보완
- ProjectSettings 일부 변경(에디터 빌드 설정, 태그 등)

테스트 권장 사항
- Unity 에디터에서 GamePlayScene을 열어 씬/프리팹 연결 상태 확인
- 빌드 전에 PlayerSettings 및 TagManager 확인

백업 및 복구
- 병합 전 상태 백업: backup/before-merge-20260324-212010 브랜치 생성됨
  필요 시 해당 브랜치로 체크아웃하여 복구 가능

작성자 노트
- 병합은 안전 모드(merge)로 진행하였으며 충돌은 발생하지 않았습니다.
- 이후 추가 설명이나 테스트 결과가 있으면 이 파일에 업데이트해 주세요.
