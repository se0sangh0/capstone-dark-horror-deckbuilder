# 치유사 FILTERED Idle·응급 처치 시트 v1

> 상태: **2026-08-30 사용자 승인 · Unity 미연결**
> 제작일: 2026-08-30
> 생성 방식: Codex built-in `image_gen` + 알파 연결 성분 분리 + 최근접 이웃 정규화
> 외형 기준: `../../character-anchors/Priest_Filtered_IdleAnchor_v1.png`

## 1. 결과 파일

| 동작 | 파일 | 규격 | SHA-256 |
|---|---|---|---|
| 대기 | `Priest_Filtered_Idle_384x512x4_v1.png` | 1536×512 RGBA · 384×512×4 | `42bcbba98c870d90b8dcf3480a73b1d0767b4912c1721a23cca0806e6e83cfe9` |
| 응급 처치 | `Priest_Filtered_EmergencyTreatment_384x512x4_v1.png` | 1536×512 RGBA · 384×512×4 | `563f986f9d3737cc5580eb7e9db7713cf37989d9a11e243259ca602f3e88f1d2` |

두 파일 모두 실제 알파 채널을 사용하고 치유사와 실제 치료 도구만 포함한다. 치료 대상 프레임·안정화 수치·회복광은 별도 UI/VFX다.

## 2. 프레임 구성

### Idle

1. 중립 치료 준비
2. 짧은 들숨과 베일·가방끈의 미세 이동
3. 날숨과 작은 무게중심 이동
4. 중립 자세로 복귀

### 응급 처치

1. 중립 치료 준비
2. 가방에서 약포·붕대를 꺼냄
3. 오른쪽 아군에게 약포·붕대를 내밂
4. 도구를 정리하고 중립 자세로 복귀

## 3. 최종 프롬프트 세트

### Idle

```text
Use case: identity-preserve
Asset type: four-frame horizontal pixel-art Idle sprite sheet
Input images: Image 1 is the approved Healer character identity anchor and must be preserved.
Primary request: create exactly four equal frames for a subtle rear-line Idle loop.
Frame 1: approved calm ready stance with wooden water bowl and small sanctuary bell.
Frame 2: slight inhale; short poultice veil, treatment cloth and bag straps shift only a few pixels.
Frame 3: slight exhale and tiny weight shift; bowl, bell, herbs and feet remain controlled.
Frame 4: return cleanly to Frame 1.
Composition: 3:1 sheet corresponding to four equal 384×512 cells; identical scale, facing right, baseline and padding.
Constraints: preserve veil, treatment mask, clothes, herbs, ointment, poultice, bandages, bowl, bell, number tag and teal coupling; genuine transparent alpha; no scenery, shadow, text, UI, healing glow or VFX.
```

### 응급 처치

```text
Use case: identity-preserve
Asset type: four-frame horizontal pixel-art first-skill action sprite sheet
Input images: Image 1 is the approved Healer character identity anchor and must be preserved.
Primary request: create exactly four equal frames for Emergency Treatment practical poultice and bandage motion only.
Frame 1: calm ready stance with tools secured.
Frame 2: secure bowl and bell, then reach into the medicine bag for one folded poultice cloth and bandage.
Frame 3: extend and apply the poultice and bandage toward an off-screen ally on the right.
Frame 4: finish and return toward the ready stance.
Composition: 3:1 sheet corresponding to four equal 384×512 cells; identical identity, facing right and baseline.
Constraints: genuine transparent alpha; no weapon attack, patient, healing frame, glow, particles, scenery, shadow, text or UI.
```

### 투명 배경·프레임 정규화

```text
Keep the same four poses and approved Healer identity, but re-layout them into one clean 3:1 horizontal sheet with exactly four equal frame cells. Remove only the baked checkerboard background and replace it with genuine transparent alpha.
```

RGBA 고해상도 결과에서 알파 128 이상 픽셀의 가장 큰 네 연결 성분을 왼쪽부터 분리했다. 각 자세에 최대 60% 최근접 이웃 배율을 적용하고 384×512 칸의 아래 기준선을 10px로 맞췄다.

## 4. 검증

- 파일 크기: 1536×512
- 색상 형식: RGBA
- 프레임 수: 4
- 프레임 칸: 384×512
- 프레임별 최소 좌우 여백: Idle 80px, 응급 처치 59px
- 프레임별 최소 위 여백: Idle 146px, 응급 처치 129px
- 프레임별 아래 여백: 10px
- 프레임 경계 밖 알파 픽셀: 없음
- Unity 임포트·슬라이스·애니메이터 연결: 미실행
