# 고블린 FILTERED Idle·단검 휘두르기 시트 v1

> 상태: **2026-08-30 사용자 승인 · Unity 미연결**
> 제작일: 2026-08-30
> 생성 방식: Codex built-in `image_gen` + 알파 연결 성분 분리 + 최근접 이웃 정규화
> 외형 기준: `../../enemy-anchors/Goblin_Filtered_IdleAnchor_v2.png`

## 1. 결과 파일

| 동작 | 파일 | 규격 | SHA-256 |
|---|---|---|---|
| 대기 | `Goblin_Filtered_Idle_384x512x4_v1.png` | 1536×512 RGBA · 384×512×4 | `912c840b5492919c2a81080d49c742516859ea02bf6be7278c8de5c0b7495981` |
| 단검 휘두르기 | `Goblin_Filtered_DaggerSwing_384x512x4_v1.png` | 1536×512 RGBA · 384×512×4 | `d7d897715bbec7584299962426074f83900fc337532b54f526a4419193fe75b5` |

두 파일은 실제 알파 채널을 사용하고 고블린·융합 바구니·녹슨 단검만 포함한다. 독침·독·타격 연출은 별도 VFX다.

## 2. 프레임 구성

### Idle

1. 낮은 중립 자세
2. 직조 껍질과 어깨의 미세 상승
3. 껍질이 가라앉고 무릎을 조금 굽힘
4. 중립 자세로 복귀

### 단검 휘두르기

1. 낮은 중립 준비
2. 몸을 압축하고 단검 팔을 당김
3. 왼쪽으로 짧은 단검 공격
4. 중립 자세로 복귀

## 3. 최종 프롬프트 세트

### Idle

```text
Use case: identity-preserve
Asset type: four-frame horizontal Goblin Idle sprite sheet
Input images: Image 1 is the approved Goblin v2 identity anchor and must be preserved.
Primary request: create exactly four equal frames for a subtle Idle loop facing left.
Frame 1: approved low compressed crouch with rusty dagger controlled.
Frame 2: wicker shell and shoulders rise only a few pixels; thorns sway minimally.
Frame 3: shell settles, knees compress slightly and the long free hand shifts a little.
Frame 4: return cleanly to Frame 1.
Composition: 3:1 sheet corresponding to four equal 384×512 cells; identical scale, shell size, facing left, baseline and padding.
Constraints: preserve woven cavity, fused thorn shell, wicker limbs, two arms, two legs and small dagger; genuine transparent alpha; no scenery, shadow, text, UI, poison or VFX.
```

### 단검 휘두르기

```text
Use case: identity-preserve
Asset type: four-frame horizontal Goblin basic attack sprite sheet
Input images: Image 1 is the approved Goblin v2 identity anchor and must be preserved.
Primary request: create exactly four equal frames for Dagger Swing body and small rusty dagger motion only, facing left.
Frame 1: low ready crouch.
Frame 2: compress the shell and draw the dagger arm slightly back.
Frame 3: one short decisive dagger attack toward the left; free hand braces the body.
Frame 4: follow-through and recover toward the approved crouch.
Composition: 3:1 sheet corresponding to four equal 384×512 cells; identical scale, shell size, facing left and baseline.
Constraints: genuine transparent alpha; no poison, blood, slash trail, impact, scenery, shadow, text, UI or VFX.
```

### 투명 배경·프레임 정규화

```text
Keep the same four poses and approved Goblin v2 identity, but re-layout them into one clean 3:1 horizontal sheet with exactly four equal cells. Remove the baked background and use genuine transparent alpha.
```

RGBA 고해상도 결과에서 알파 128 이상 픽셀의 가장 큰 네 연결 성분을 왼쪽부터 분리했다. 각 자세에 최대 60% 최근접 이웃 배율을 적용하고 384×512 칸의 아래 기준선을 10px로 맞췄다.

## 4. 검증

- 파일 크기: 1536×512
- 색상 형식: RGBA
- 프레임 수: 4
- 프레임 칸: 384×512
- 프레임별 최소 좌우 여백: Idle 17px, 단검 휘두르기 13px
- 프레임별 최소 위 여백: Idle 146px, 단검 휘두르기 164px
- 프레임별 아래 여백: 10px 이상
- 프레임 경계 밖 알파 픽셀: 없음
- Unity 임포트·슬라이스·애니메이터 연결: 미실행
