# 봉쇄병 FILTERED Idle·방벽 전개 시트 v2

> 상태: **2026-08-30 사용자 승인 · Unity 미연결**
> 제작일: 2026-08-30
> 생성 방식: Codex built-in `image_gen` + 알파 연결 성분 분리 + 최근접 이웃 정규화
> 외형 기준: `../../character-anchors/Defender_Filtered_IdleAnchor_v1.png`

## 1. 결과 파일

| 동작 | 파일 | 규격 | SHA-256 |
|---|---|---|---|
| 대기 | `Defender_Filtered_Idle_384x512x4_v1.png` | 1536×512 RGBA · 384×512×4 | `da8f9e4db43f4e7963c8664af4696e44fd861a0d7674fb35667fa2eddb1f1e37` |
| 방벽 전개 | `Defender_Filtered_DeployBarrier_384x512x4_v2.png` | 1536×512 RGBA · 384×512×4 | `fa5c175473365b12cb732e5cb450f40b59f827ac18843ce5997b9bd6715bf9a3` |

두 파일 모두 실제 알파 채널을 사용하고 캐릭터·원형 목제 방패·짧은 창만 포함한다. 봉쇄선·격리 인장 발광·충격파는 별도 VFX다.

## 2. 프레임 구성

### Idle

1. 중립 방어 자세 A
2. 중립 방어 자세 B
3. 중립 방어 자세 A
4. 중립 방어 자세 B

Idle 배경 추출이 반복 실패해 RGB 결과를 사용하지 않았다. 투명 배경이 정상인 방벽 전개 원본의 중립 자세 1·4번을 교차 배치한 정적 루프다.

### 방벽 전개

1. 중립 보호 자세
2. 무게중심을 낮추고 방패를 중앙으로 당김
3. 방패를 앞에 세워 지지
4. 중립 보호 자세로 복귀

## 3. 최종 프롬프트 세트

### Idle

```text
Use case: identity-preserve
Asset type: four-frame horizontal pixel-art Idle sprite sheet
Input images: Image 1 is the approved Blockade Guard character identity anchor and must be preserved.
Primary request: create exactly four equal frames for a subtle defensive Idle loop.
Frame 1: approved low stable stance, round shield forward and short spear controlled at the side.
Frame 2: slight inhale; shoulders and shield rise only a few pixels.
Frame 3: slight exhale and tiny weight shift behind the shield; spear angle changes minimally.
Frame 4: return cleanly to Frame 1.
Composition: 3:1 sheet corresponding to four equal 384×512 cells; identical scale, shield size, facing right, baseline and padding.
Constraints: preserve wooden mask, round shield, short spear, clothing, reinforcement, markings, tag and teal fastening; genuine transparent alpha; no scenery, shadow, text, UI, energy barrier, glow or VFX.
```

### 방벽 전개

```text
Use case: identity-preserve
Asset type: four-frame horizontal pixel-art first-skill action sprite sheet
Input images: Image 1 is the approved Blockade Guard character identity anchor and must be preserved.
Primary request: create exactly four equal frames for Deploy Barrier defensive body, shield and short-spear motion only.
Frame 1: neutral protective stance.
Frame 2: lower the center of gravity and draw the shield toward the center line.
Frame 3: plant and brace the shield forward as a full-party protection pose; no energy line or magical effect.
Frame 4: settle and return toward the protective stance.
Composition: 3:1 sheet corresponding to four equal 384×512 cells; identical identity, shield size, facing right and baseline.
Constraints: genuine transparent alpha; no shield bash, attack, barrier line, seal flash, shockwave, glow, scenery, shadow, text or UI.
```

### 투명 배경·프레임 정규화

```text
Remove only the baked checkerboard background and replace it with genuine transparent alpha. Preserve the four guards, shields and spears, poses, proportions, palette, 3:1 canvas and resolution.
```

투명 배경이 정상인 방벽 전개 결과에서 알파 128 이상 픽셀의 가장 큰 네 연결 성분을 왼쪽부터 분리했다. 60% 최근접 이웃 배율을 적용한 뒤 2프레임은 1.177배, 3프레임은 1.118배로 보정해 방패·체격을 1·4프레임과 맞췄다.

## 4. 검증

- 파일 크기: 1536×512
- 색상 형식: RGBA
- 프레임 수: 4
- 프레임 칸: 384×512
- 프레임별 최소 좌우 여백: Idle 53px, 방벽 전개 53px
- 프레임별 위 여백: 189px
- 프레임별 아래 여백: 10px
- 네 프레임의 알파 높이: 313px로 통일
- 프레임 경계 밖 알파 픽셀: 없음
- Unity 임포트·슬라이스·애니메이터 연결: 미실행

## 5. 반려본

방벽 전개 v1은 2·3프레임의 방패와 체격이 작아 `_workspace/graphics-remake/rejected/wave-a-character-sheets/defender/`로 옮겼다. 배경 추출에 실패한 RGB Idle 결과는 프로젝트 후보로 보존하지 않았다.
