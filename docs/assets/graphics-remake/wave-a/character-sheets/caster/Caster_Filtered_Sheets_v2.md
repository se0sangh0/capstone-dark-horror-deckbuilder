# 원소술사 FILTERED Idle·원소탄 시트 v2

> 상태: **2026-08-30 사용자 승인 · Unity 미연결**
> 제작일: 2026-08-30
> 생성 방식: Codex built-in `image_gen` + 최근접 이웃 프레임 정규화
> 외형 기준: `../../character-anchors/Caster_Filtered_IdleAnchor_v2.png`

## 1. 결과 파일

| 동작 | 파일 | 규격 | SHA-256 |
|---|---|---|---|
| 대기 | `Caster_Filtered_Idle_384x512x4_v2.png` | 1536×512 RGBA · 384×512×4 | `807fe87833d7d3219451f0605810a0ca8401c4f3230c1b39729b72b9d29efaf1` |
| 원소탄 | `Caster_Filtered_ElementalBolt_384x512x4_v2.png` | 1536×512 RGBA · 384×512×4 | `a7a7371fb20e8c466ef168f2f7345fe695b30fbd3ce3e9c1f3991bc5538ad098` |

두 파일 모두 실제 알파 채널을 사용하고, 캐릭터·관측봉만 포함한다. 광탄·원소광·표적선은 별도 VFX다.

## 2. 프레임 구성

### Idle

1. 중립 자세
2. 짧은 들숨과 망토의 미세 상승
3. 날숨과 작은 무게중심 이동
4. 중립 자세로 복귀

### 원소탄

1. 중립 준비
2. 몸과 관측봉을 짧게 뒤로 당김
3. 발을 고정하고 관측봉을 오른쪽으로 내미는 핵심 동작
4. 중립 자세로 복귀

## 3. 최종 프롬프트 세트

### Idle

```text
Use case: identity-preserve
Asset type: four-frame horizontal pixel-art Idle sprite sheet
Input images: Image 1 is the approved Elementalist character identity anchor and must be preserved.
Primary request: create exactly four equal animation frames in one horizontal row for the same character's subtle Idle loop.
Frame 1: approved neutral stance, staff upright.
Frame 2: slight inhale; shoulders and woven mantle rise only a few pixels; staff cords shift minimally.
Frame 3: slight exhale and tiny weight shift; staff and feet remain planted.
Frame 4: return cleanly to Frame 1.
Composition: 3:1 sheet corresponding to four equal 384×512 cells; identical scale, proportions, facing right, ground baseline and padding.
Constraints: preserve mask, mantle, staff, bronze rings, cords, tags and teal couplings; genuine transparent alpha; no scenery, floor, shadow, text, UI, particles or VFX; nothing crosses a frame boundary.
```

### 원소탄

```text
Use case: identity-preserve
Asset type: four-frame horizontal pixel-art first-skill action sprite sheet
Input images: Image 1 is the approved Elementalist character identity anchor and must be preserved.
Primary request: create exactly four equal frames for Elemental Bolt body and staff motion only.
Frame 1: neutral ready stance.
Frame 2: short wind-up; torso and staff draw slightly back.
Frame 3: clear casting pose; feet planted and observation staff thrust or angled toward the right; no projectile or magical effect.
Frame 4: follow-through and return toward neutral.
Composition: 3:1 sheet corresponding to four equal 384×512 cells; identical scale, proportions, facing right, baseline and padding.
Constraints: preserve approved identity and equipment; genuine transparent alpha; no scenery, shadow, text, UI, projectile, glow, particles, target line or VFX; nothing crosses a frame boundary.
```

### 투명 배경·프레임 정규화

```text
Remove only the baked background and replace it with genuine transparent alpha. Preserve every frame and pose. Re-layout as one clean 3:1 horizontal sheet with exactly four equal cells and no unused lower half.
```

생성 결과의 프레임 간격이 정확한 384px 경계와 맞지 않아, 고해상도 RGBA 결과를 네 자세 단위로 분리한 뒤 모든 자세에 같은 60% 비율과 최근접 이웃 보간을 적용했다. 최종 캔버스는 1536×512다.

## 4. 검증

- 파일 크기: 1536×512
- 색상 형식: RGBA
- 프레임 수: 4
- 프레임 칸: 384×512
- 프레임별 최소 좌우 여백: Idle 30px, 원소탄 12px
- 프레임별 최소 위 여백: Idle 89px, 원소탄 89px
- 프레임별 최소 아래 여백: Idle 9px, 원소탄 14px
- 프레임 경계 밖 알파 픽셀: 없음
- Unity 임포트·슬라이스·애니메이터 연결: 미실행

## 5. 반려본

v1 시트는 관측봉이 프레임 경계에 닿거나 자세가 이웃 칸을 침범해 `_workspace/graphics-remake/rejected/wave-a-character-sheets/caster/`로 옮겼다.
