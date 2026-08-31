# INSPECTOR 웹사이트

랜딩 페이지와 GitHub Pages 배포 파일은 이 저장소 안의 `site/`에서 관리합니다.
별도 웹 저장소를 만들지 않습니다.

- `preview/`: GitHub Pages에 공개하는 개발 시안
- `preview/assets/`: 사이트 전용 파생 이미지 · 실제 제작을 시작할 때 생성
- 공개 주소: [https://se0sangh0.github.io/inspector/](https://se0sangh0.github.io/inspector/)
- 배포 방식: GitHub Actions가 `preview/`를 GitHub Pages 아티팩트로 배포
- 워크플로: [`.github/workflows/deploy-pages.yml`](../.github/workflows/deploy-pages.yml)

현재 [`preview/index.html`](preview/index.html)은 한국어·영어·중국어 번체를
한 페이지에서 바꾸는 자체 포함형 공개 개발 시안입니다. 영상과 일부 삽화 자리는
각 언어의 설명용 문구로 남아 있습니다.

브랜드 정본은 `art/brand/`, 그래픽 검토 원본은
`art/production/graphics-remake/`에 유지합니다. 사이트에서는 정본을 직접 수정하지
않고 웹용 파생본을 `site/preview/assets/`에 만듭니다.

`main`의 `site/preview/**` 또는 배포 워크플로가 바뀌면 자동 배포합니다. GitHub Actions의
`Run workflow`로 수동 배포할 수도 있습니다.

현재 페이지는 개발 중 콘셉트 비주얼을 사용하는 공개 시안입니다. 정식 배포본으로
전환하기 전 승인된 리소스, 공유 이미지와 동영상 파일을 다시 확인합니다.
