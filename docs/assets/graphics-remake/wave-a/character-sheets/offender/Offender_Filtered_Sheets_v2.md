# 척살자 FILTERED Idle·표적 관통 시트 v2

> 상태: **2026-08-30 사용자 승인 · Unity 미연결**
> 제작일: 2026-08-30
> 생성 방식: Codex built-in `image_gen` + 알파 연결 성분 분리 + 최근접 이웃 정규화
> 외형 기준: `../../character-anchors/Offender_Filtered_IdleAnchor_v2.png`

## 1. 결과 파일

| 동작 | 파일 | 규격 | SHA-256 |
|---|---|---|---|
| 대기 | `Offender_Filtered_Idle_384x512x4_v2.png` | 1536×512 RGBA · 384×512×4 | `63fda332fec813b6d83eb052e9f885edcee1d34ee2d6951c2f7137b2cb71cffa` |
| 표적 관통 | `Offender_Filtered_TargetPierce_384x512x4_v2.png` | 1536×512 RGBA · 384×512×4 | `a752ec4de25d7afdb7c58f31dc03ab346831f2513a19939ade65387b0ded9ba3` |

두 파일 모두 실제 알파 채널을 사용하고, 캐릭터·사냥창·사냥칼만 포함한다. 표적선·타격·유혈 연출은 넣지 않았다.

## 2. 프레임 구성

### Idle

1. 낮은 중립 자세
2. 짧은 들숨과 어깨·망토의 미세 상승
3. 날숨과 작은 무게중심 이동
4. 중립 자세로 복귀

### 표적 관통

1. 낮은 중립 준비
2. 앞발을 고정하고 사냥창을 뒤로 당김
3. 오른쪽으로 직선 관통
4. 중립 자세로 복귀

## 3. 최종 프롬프트 세트

### Idle

```text
Use case: identity-preserve
Asset type: four-frame horizontal pixel-art Idle sprite sheet
Input images: Image 1 is the approved Executioner character identity anchor and must be preserved.
Primary request: create exactly four equal frames for a subtle Idle loop.
Frame 1: approved low ready stance, practical hunting spear held diagonally.
Frame 2: slight inhale and tiny shoulder rise; repaired leather cloak and knot cord shift only a few pixels.
Frame 3: slight exhale and small weight shift; spear tip and both feet remain controlled.
Frame 4: return cleanly to Frame 1.
Composition: 3:1 sheet corresponding to four equal 384×512 cells; identical scale, proportions, facing right, ground baseline and padding.
Constraints: preserve stitched leather mask, jaw ornament, cloak, practical boar-hunting spear, hunting knife, cords, tag and teal coupling; genuine transparent alpha; no scenery, floor, shadow, text, UI, blood, target line or VFX.
```

### 표적 관통

```text
Use case: identity-preserve
Asset type: four-frame horizontal pixel-art first-skill action sprite sheet
Input images: Image 1 is the approved Executioner character identity anchor and must be preserved.
Primary request: create exactly four equal frames for Target Pierce body and hunting-spear motion only.
Frame 1: low neutral ready stance.
Frame 2: short wind-up; front foot braces and the spear draws back.
Frame 3: decisive straight thrust toward the right with the narrow boar-spear point leading.
Frame 4: follow-through and recovery toward ready stance.
Composition: 3:1 sheet corresponding to four equal 384×512 cells; identical character identity, camera angle, facing right and baseline.
Constraints: practical spearhead only; genuine transparent alpha; no blood, severing, target line, trail, impact or VFX; no scenery, shadow, text or UI.
```

### 투명 배경·프레임 정규화

```text
Remove only the baked checkerboard background and replace it with genuine transparent alpha. Preserve all four frames, every character and weapon pixel, poses, proportions, palette, 3:1 canvas and resolution.
```

긴 창이 인접 프레임의 가로 범위와 겹쳐 단순 균등 자르기를 사용하지 않았다. 알파 128 이상 픽셀의 연결 성분 중 가장 큰 네 자세를 왼쪽부터 분리한 뒤 60% 최근접 이웃 배율을 적용했다. 관통 자세만 칸 너비를 넘지 않도록 최대 374px로 제한했다.

## 4. 검증

- 파일 크기: 1536×512
- 색상 형식: RGBA
- 프레임 수: 4
- 프레임 칸: 384×512
- 프레임별 최소 좌우 여백: Idle 24px, 표적 관통 5px
- 프레임별 최소 위 여백: Idle 128px, 표적 관통 201px
- 프레임별 아래 여백: 10px
- 프레임 경계 밖 알파 픽셀: 없음
- Unity 임포트·슬라이스·애니메이터 연결: 미실행

## 5. 반려본

v1 시트는 사냥창 조각이 인접 칸에 섞여 `_workspace/graphics-remake/rejected/wave-a-character-sheets/offender/`로 옮겼다.
