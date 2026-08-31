# Wave A 최초 파티 4역할 외형 앵커 v1

> 상태: **최초 파티 4역할 외형 승인 완료 · 런타임 스프라이트 아님 · Unity 미연결**
> 제작일: 2026-08-30
> 생성 방식: Codex built-in `image_gen`
> 시각 기준: `docs/assets/graphics-remake/style-anchors/Battle_Forest_StyleAnchor_v1.png`

## 1. 이번 결과의 범위

- P0 본 런 초기 파티의 `FILTERED` 외형을 비교하기 위한 중립 대기 자세 4장이다.
- 대상은 캐스터·오펜더·디펜더·프리스트다.
- 표시 직업명은 원소술사·척살자·봉쇄병·치유사다.
- 캐릭터 공통 프레임 크기와 애니메이션 시트 규격은 아직 정하지 않았다.
- 외형 승인 뒤 같은 이미지를 참조해 첫 번째 스킬용 `Attack` 동작을 만든다.
- 캐릭터 몸·무기와 VFX는 합치지 않는다.

## 2. 선택 후보

| 내부 역할 | 표시명 | 파일 | 크기 | SHA-256 |
|---|---|---|---:|---|
| `Caster` | 원소술사 | `Caster_Filtered_IdleAnchor_v2.png` | 1199×1312 RGBA | `70b6bf41b2b5befea49d5c3a6637383bbe1d1acf0deade18b4ed48dc17dba9f5` |
| `Offender` | 척살자 | `Offender_Filtered_IdleAnchor_v2.png` | 1182×1330 RGBA | `d021d891cd1f0d2505459e7c3747b793b47ace1fd551c86e4a3335c0fca68d60` |
| `Defender` | 봉쇄병 | `Defender_Filtered_IdleAnchor_v1.png` | 1254×1254 RGBA | `df83622e53730e2614b6a93d377ad2c62f636a46c4767b15c2bcb0956ceccf0e` |
| `Priest` | 치유사 | `Priest_Filtered_IdleAnchor_v1.png` | 1177×1337 RGBA | `d7fa058dc42f1d54a71187339c8cb6a89a2450b497a2a5bc838ca269a8e2cd2d` |

네 파일 모두 실제 알파 채널이 있는 투명 PNG다. 생성기가 처음 만든 체크무늬 배경은 `background-extraction` 편집으로 제거했다.

### 승인 기록

- 2026-08-30: 원소술사 `Caster_Filtered_IdleAnchor_v2.png` 사용자 승인.
- 2026-08-30: 척살자 `Offender_Filtered_IdleAnchor_v2.png` 사용자 승인.
- 2026-08-30: 봉쇄병 `Defender_Filtered_IdleAnchor_v1.png` 사용자 승인.
- 2026-08-30: 치유사 `Priest_Filtered_IdleAnchor_v1.png` 사용자 승인.
- 최초 파티 4역할 외형 승인 완료.
- 2026-08-30: 프레임 `384×512`, 한 동작 4프레임 가로 배열 `1536×512`로 사용자 확정.

## 3. 시각 검수 기록

- 원소술사 v1은 가면을 손으로 들고 있어 공격 동작에 방해됐다. v2에서는 같은 점토 인장 가면을 얼굴 전체에 착용하고 빈손을 중립 위치로 내렸다.
- 척살자 v1은 창날이 판타지 무기처럼 과장됐다. v2에서는 좁은 검은 철 사냥창 날과 짧은 멈춤 돌기만 남겼다.
- 봉쇄병은 정상 크기 원형 목제 방패·짧은 창·세로 틈 목제 가면을 유지했다.
- 치유사는 약초·붕대·약포·물그릇·작은 성소 방울을 유지하고 긴 사제 지팡이와 후기 교단복을 사용하지 않았다.
- 네 결과는 고해상도 외형 앵커다. 최대 색 수·1~2px 외곽선·실제 프레임 크기는 런타임 시트 제작 단계에서 다시 맞춘다.

## 4. 최종 프롬프트 세트

### 공통

```text
Use case: stylized-concept
Asset type: game character visual anchor for a future 2D combat sprite
Input images: Image 1 is the approved Battle Forest style anchor; use only its disciplined pixel density, limited palette, darkness, material treatment, and restrained teal accents. Do not copy its forest composition or background.
Primary request: create one single full-body FILTERED companion character in a neutral combat idle-ready pose facing right.
Style/medium: strict handcrafted 16-bit pixel-art game sprite, crisp deliberate pixel clusters, limited charcoal, dead-brown, muted-bronze and dusty-cloth palette, very restrained desaturated teal.
Composition/framing: one character only, full body and equipment fully visible, centered with generous transparent padding, clear side-view/three-quarter combat readability, no sprite sheet and no repeated poses.
Constraints: genuinely transparent background and preserved alpha; no scenery, floor, text, UI, watermark, attack effect or shadow; preserve human anatomy and practical equipment; face censorship is removable; character body and VFX remain separate.
Avoid: late-medieval plate, gothic costume, gears, pipes, steampunk, neon, giant equipment, body fusion, monster anatomy, extra limbs.
```

### 원소술사

```text
Subject: adult seasonal observer; slim rear-line damage-dealer silhouette; faded handwoven mantle; cracked round clay seal mask covering the face, pierced and incised with season, wind, frost and rainfall marks; wooden observation staff with small bronze rings and knotted cords; tiny agency ID tag and small muted-teal soul-stone coupling.
Avoid: pointed wizard hat, late-medieval robe, precision astronomy machinery, ornate gold and giant staff.
Targeted edit: secure the clay seal mask over the entire face and lower the hand touching it into a neutral idle position; keep all other design features unchanged.
```

### 척살자

```text
Subject: adult large-beast hunter; lean fast single-target melee silhouette; repaired leather cloak over one shoulder; detachable stitched leather face mask and small animal-jaw ornament; long practical hunting spear, short heavy hunting knife and trail-marking knot cord; tiny agency ID tag and small muted-teal soul-stone coupling.
Avoid: giant horns, skull helmet, barbarian physique, huge axe, broad armor, trophy clutter, gore and execution sword.
Targeted edit: replace the oversized fantasy spearhead with a smaller practical early-medieval boar-hunting spearhead, using one narrow dark-iron leaf blade and modest short stopping lugs; keep all other design features unchanged.
```

### 봉쇄병

```text
Subject: adult community guard; broad stable defensive silhouette and low center of gravity; believable-size round wooden shield, short spear, layered leather and heavy woven cloth with small chain or scale reinforcement; detachable rough wooden mask with one narrow vertical vision slit; restrained quarantine mark, tiny ID tag and muted-teal fastening on the shield rim.
Avoid: oversized door shield, shield fused to torso, excessive nails or chains, full plate, knight heraldry and large two-handed weapons.
```

### 치유사

```text
Subject: adult sanctuary healer; light practical support silhouette; simple treatment clothing; herb bundle, ointment jar, folded poultice cloth and bandages, small wooden water bowl and small sanctuary bell; short poultice veil and removable herb-stained treatment cloth mask; tiny agency number tag and muted-teal coupling on the bell ring.
Avoid: long funeral veil, late church vestments, full-face bandages, body stitching, mechanical medicine, ornate golden relic and bishop staff.
```

### 투명 배경 보정

```text
Use case: background-extraction
Primary request: remove only the baked white-and-light-gray checkerboard background and replace it with genuine transparent alpha.
Constraints: preserve every character and equipment pixel, pose, palette, canvas framing and resolution; do not redraw, restyle, crop, resize, sharpen, soften or add anything; no floor or shadow.
```

## 5. 다음 승인 뒤 작업

1. 첫 번째 스킬 동작을 만든다.
   - 원소술사: `원소탄`
   - 척살자: `표적 관통`
   - 봉쇄병: `방벽 전개`
   - 치유사: `응급 처치`
4. 광탄·표적선·봉쇄선·치료 프레임은 별도 VFX로 남긴다.

원소술사 시트 후보는 `../character-sheets/caster/Caster_Filtered_Sheets_v2.md`에 기록한다.
