# 약탈자 FILTERED Idle·도끼 휘두르기 시트 v1

> 상태: **사용자 검토 후보 · Unity 미연결**
> 제작일: 2026-08-30
> 생성 방식: Codex built-in `image_gen` + 밝은 격자 알파 분리 + 프레임 정규화
> 외형 기준: `../../enemy-anchors/Raider_Filtered_IdleAnchor_v1.png`

## 1. 결과 파일

| 동작 | 파일 | 규격 | SHA-256 |
|---|---|---|---|
| 대기 | `Raider_Filtered_Idle_384x512x4_v1.png` | 1536×512 RGBA · 384×512×4 | `2ae22941718eddc23a106e8f3b5198507b42197cdbe2bb186049401f8b77a2dd` |
| 도끼 휘두르기 | `Raider_Filtered_AxeSwing_384x512x4_v1.png` | 1536×512 RGBA · 384×512×4 | `ad65c816ba34e294cc66d8b356839874779b58526a0a5fffabb0a5338f1e9f75` |

두 파일은 실제 알파 채널을 사용한다. 약탈자와 한 손 벌목 도끼만 포함하며, 도끼 궤적·피·타격 연출은 별도 VFX다.

## 2. 프레임 구성

### Idle

1. 낮은 중립 준비 자세
2. 두건과 어깨의 미세 상승
3. 어깨가 가라앉고 무릎을 조금 굽힘
4. 중립 자세로 복귀

### 도끼 휘두르기

1. 한 손 도끼를 낮게 든 준비 자세
2. 체중을 뒤로 옮기며 도끼를 머리 위로 올림
3. 왼쪽 아래로 짧게 내리찍음
4. 낮은 자세로 회수하며 중립 자세로 복귀

## 3. 최종 프롬프트 세트

### Idle

```text
Use case: identity-preserve
Asset type: four-frame horizontal Raider Idle game sprite sheet
Input images: Image 1 is the approved Raider identity anchor and must be preserved exactly as the character reference.
Primary request: create exactly four distinct equal-width frames for a subtle Idle loop, with the Raider facing left in every frame.
Frame 1: approved neutral crouched ready pose, practical one-handed woodcutter axe held low and controlled.
Frame 2: shoulders and patched hood rise only a few pixels with a small breath; axe remains steady.
Frame 3: shoulders settle, knees compress slightly, rope and cloth shift minimally.
Frame 4: return cleanly to the approved neutral pose.
Subject invariants: sturdy adult farmer/woodcutter silhouette from the canyon blockade; rugged patched work clothing; rope; cloth hood; face completely concealed by deep shadow; practical single-bit one-handed woodcutter axe; small utility knife remains sheathed.
Style/medium: strict 16-bit pixel-art game sprite, limited charcoal, dead brown, worn leather and muted bronze palette; crisp hard pixel clusters; no smoothing.
Composition/framing: one clean 3:1 horizontal sheet representing four equal 384×512 cells; one complete full-body character centered in each cell; identical character scale, facing left, foot baseline and padding; no panel borders or frame labels.
Scene/backdrop: genuinely transparent alpha only.
Constraints: exactly four full poses and exactly one Raider per cell; preserve hood, concealed face, body proportions, workwear, rope, axe design and handedness; keep all pixels inside each cell.
Avoid: visible human face, wolf or animal anatomy, monster snout, giant double-headed axe, full plate armor, fantasy bandit ornament, extra limbs, duplicate weapons, thrown axe, scenery, floor, cast shadow, text, UI, blood, attack trail, impact, particles, VFX, checkerboard background, watermark.
```

### 도끼 휘두르기

```text
Use case: identity-preserve
Asset type: four-frame horizontal Raider basic attack game sprite sheet
Input images: Image 1 is the approved Raider identity anchor and must be preserved exactly as the character reference.
Primary request: create exactly four distinct equal-width frames for a compact one-handed Axe Swing attack, with the Raider facing left in every frame.
Frame 1: low ready stance with the practical woodcutter axe held forward and controlled.
Frame 2: short wind-up, body weight shifts back and the one-handed axe rises without crossing the cell boundary.
Frame 3: decisive diagonal downward-left chopping swing; axe remains firmly in the same hand; free arm braces the body.
Frame 4: low follow-through and controlled recovery toward the approved ready pose.
Subject invariants: sturdy adult farmer/woodcutter silhouette from the canyon blockade; rugged patched work clothing; rope; cloth hood; face completely concealed by deep shadow; practical single-bit one-handed woodcutter axe; small utility knife remains sheathed.
Style/medium: strict 16-bit pixel-art game sprite, limited charcoal, dead brown, worn leather and muted bronze palette; crisp hard pixel clusters; no smoothing.
Composition/framing: one clean 3:1 horizontal sheet representing four equal 384×512 cells; one complete full-body character centered in each cell; identical character scale, facing left, foot baseline and padding; no panel borders or frame labels.
Scene/backdrop: genuinely transparent alpha only.
Constraints: exactly four full poses and exactly one Raider per cell; preserve hood, concealed face, body proportions, workwear, rope, axe design and handedness; keep all pixels inside each cell.
Avoid: visible human face, wolf or animal anatomy, monster snout, giant double-headed axe, full plate armor, fantasy bandit ornament, extra limbs, duplicate weapons, thrown axe, scenery, floor, cast shadow, text, UI, blood, slash trail, impact, particles, VFX, checkerboard background, watermark.
```

### 3:1 재배치·투명 배경 보정

```text
Keep the same four poses and approved Raider identity, but remove the unused vertical canvas and re-layout them into one clean 3:1 horizontal sheet with exactly four equal cells. Remove the white checkerboard completely and replace it with genuine transparent alpha.
```

`image_gen`이 투명 추출을 세 차례 시도한 뒤에도 2172×724 RGB 격자 결과를 반환했다. 마지막 결과에서 밝은 무채색 격자와 경계의 회색 잔여 픽셀만 제거하고, 왼쪽부터 네 자세를 분리했다. 두 동작에 공통 0.78 최근접 이웃 배율을 적용하고 384×512 칸의 아래 기준선을 10px로 맞췄다.

## 4. 검증

- 파일 크기: 1536×512
- 색상 형식: RGBA
- 프레임 수: 4
- 프레임 칸: 384×512
- 프레임별 최소 좌우 여백: Idle 41px, 도끼 휘두르기 18px
- 프레임별 최소 위 여백: Idle 123px, 도끼 휘두르기 12px
- 프레임별 아래 여백: 10px
- 프레임 경계의 알파 픽셀: 없음
- 네 프레임의 픽셀 내용: 모두 서로 다름
- 독립 읽기 전용 QA: PASS · 필수 수정 0건
- Unity 임포트·슬라이스·애니메이터 연결: 미실행
