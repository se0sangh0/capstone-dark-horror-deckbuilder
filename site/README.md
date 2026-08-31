# INSPECTOR 웹사이트

랜딩 페이지와 GitHub Pages 배포 파일은 이 저장소 안의 `site/`에서 관리합니다.
별도 웹 저장소를 만들지 않습니다.

- `preview/`: 승인 전 독립 HTML 시안
- `assets/`: 사이트 전용 파생 이미지 · 실제 제작을 시작할 때 생성
- 배포 방식: GitHub Actions에서 `site/` 결과물을 GitHub Pages로 배포할 예정

현재 [`preview/index.html`](preview/index.html)은 한국어·영어·중국어 번체를
한 페이지에서 바꾸는 자체 포함형 시안입니다. 영상과 일부 삽화 자리는 설명용
문구로 남아 있습니다.

브랜드 정본은 `docs/assets/brand/`, 그래픽 검토 원본은
`docs/assets/graphics-remake/`에 유지합니다. 사이트에서는 정본을 직접 수정하지
않고 웹용 파생본을 `site/assets/`에 만듭니다.

실제 배포 전에는 승인된 리소스, `/inspector/` 기본 경로, 공유 이미지,
파비콘과 동영상 파일을 다시 확인합니다.
