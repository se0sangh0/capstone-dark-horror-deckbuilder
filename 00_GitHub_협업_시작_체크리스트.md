# GitHub 협업 시작 체크리스트

## 0) 현재 상태
- 로컬 git 저장소 존재
- GitHub CLI 로그인 완료(`se0sangh0`)
- 원격(remote) 미설정

## 1) 첫 커밋 정리
- [ ] `.gitignore` 반영 확인
- [ ] 불필요 파일 제외 확인(`.stfolder`, `.stversions`, 임시 probe 파일)
- [ ] 문서 변경사항 스테이징
- [ ] 커밋 메시지 작성

권장 커밋 메시지:
`chore: normalize capstone vault structure and add UI architecture rules`

## 2) GitHub 리포지토리 생성
- [ ] private/public 결정
- [ ] repo 이름 확정 (권장: `capstone-dark-horror-deckbuilder`)
- [ ] remote 연결 및 첫 push

CLI 예시:
```bash
gh repo create capstone-dark-horror-deckbuilder --private --source . --remote origin --push
```

## 3) 팀 협업 세팅
- [ ] 팀원 초대(Write 권한)
- [ ] 브랜치 보호 규칙(선택)
- [ ] PR 템플릿/이슈 템플릿(선택)

## 4) Obsidian 협업 규칙
- [ ] 팀원 모두 같은 Vault 루트 경로를 열기
- [ ] `workspace.json`은 기기별 파일로 간주
- [ ] 구조 변경 시 먼저 `00_프로젝트_허브.md` 갱신
- [ ] 큰 변경은 PR 단위로 리뷰 후 머지

## 5) 권장 운영 방식
- 메인 보호 + 기능 브랜치 사용
- 커밋 단위를 작게 유지(문서 묶음 1개 = 커밋 1개)
- 회의 직후 10분 내 문서 업데이트/푸시
