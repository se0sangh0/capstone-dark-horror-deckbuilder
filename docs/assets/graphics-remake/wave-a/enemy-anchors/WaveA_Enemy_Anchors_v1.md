# Wave A 고블린·약탈자 외형 앵커 v1

> 상태: **고블린 v2·약탈자 v1 외형 승인 완료 · 런타임 스프라이트 아님 · Unity 미연결**
> 제작일: 2026-08-30
> 생성 방식: Codex built-in `image_gen`
> 시각 기준: `../../style-anchors/Battle_Forest_StyleAnchor_v1.png`

## 1. 결과 파일

| 적 ID | 표시명 | 파일 | 크기 | SHA-256 |
|---|---|---|---:|---|
| `enemy_goblin_01` | 고블린 | `Goblin_Filtered_IdleAnchor_v2.png` | 1290×1219 RGBA | `1f3beba1a5741406e985bfcfebc94efdb79bd702352884bd42fef6429195c319` |
| `enemy_raider_01` | 약탈자 | `Raider_Filtered_IdleAnchor_v1.png` | 1200×1310 RGBA | `286a5877110ee63281873647b9ba6a64de5fb89df37d4c3c2c82e552c7fb420b` |

두 파일은 왼쪽을 보는 중립 대기 외형 앵커다. 아직 `Idle·Attack` 시트가 아니며 Unity에 연결하지 않았다.

### 승인 기록

- 2026-08-30: 고블린 `Goblin_Filtered_IdleAnchor_v2.png` 사용자 승인.
- 2026-08-30: 약탈자 `Raider_Filtered_IdleAnchor_v1.png` 사용자 승인.
- 일반 적 2종 외형 승인 완료.

## 2. 시각 해석

### 고블린

- 실제 원형은 야생림의 성인 채집민이다.
- `FILTERED`에서는 인간 팔다리가 희미하게 남은 낮은 채집 괴이로 보인다.
- 얼굴은 지워진 어둠과 거친 직조 덮개로 가린다.
- 채집 바구니는 등·옆구리에 제한적으로 융합하고 가시 얽힘을 더한다.
- 채집칼은 작은 녹슨 단검으로 과장한다.
- 초록 피부·뾰족귀·아동 체형의 판타지 고블린은 사용하지 않는다.
- v1은 인간형 자세·손·복식이 너무 선명해 반려했다. v2는 목을 없애고 머리·등바구니·어깨를 하나의 가시 덩어리로 합쳤으며, 팔·다리를 덩굴과 바구니 섬유로 감아 첫인상을 채집 괴이로 바꿨다.

### 약탈자

- 실제 원형은 협곡 봉쇄에 참여한 성인 농민·나무꾼이다.
- `FILTERED`에서는 고블린보다 크고 단단한 길목 점거자로 보인다.
- 얼굴은 깊은 그림자와 거친 탈착식 두건으로 가린다.
- 실제 노동복·가죽 보강·밧줄을 유지한다.
- 무기는 한 손 나무꾼 도끼를 약탈자 도끼로 제한적으로 과장한다.
- 늑대·동물 해부학·거대한 양손 도끼·판금 갑옷은 사용하지 않는다.

## 3. 최종 프롬프트 세트

### 고블린

```text
Use case: stylized-concept
Asset type: game enemy visual anchor for a future 2D combat sprite
Input images: Image 1 is the approved Battle Forest style anchor; use only its pixel density, palette, darkness and material treatment. Do not copy its background.
Primary request: create one full-body FILTERED enemy called Goblin in a neutral combat idle-ready pose facing left.
Subject: distorted adult human forest gatherer; short hunched silhouette; identity erased beneath rough woven covering; plain early-medieval gathering clothes; worn wicker basket partially fused into back and torso with restrained thorny growth; practical gathering knife exaggerated into a small rusty dagger; irritant plant spines misread as poison thorns.
Style: strict handcrafted 16-bit pixel-art enemy, limited charcoal, wet-brown wicker, dead vegetation, rusted iron and dirty cloth palette.
Constraints: genuine transparent alpha; one enemy only; no scenery, shadow, text, UI, projectile, glow or VFX; adult human-rooted silhouette with restrained enemy-only fusion; no gore.
Avoid: green fantasy goblin, pointed ears, child proportions, comic monster, orc, giant weapon, steampunk, exposed normal face and extra limbs.
```

### 약탈자

```text
Use case: stylized-concept
Asset type: game enemy visual anchor for a future 2D combat sprite
Input images: Image 1 is the approved Battle Forest style anchor; use only its pixel density, palette, darkness and material treatment. Do not copy its background.
Primary request: create one full-body FILTERED enemy called Raider in a neutral combat idle-ready pose facing left.
Subject: adult farmer or woodcutter from a hurried canyon blockade, misread as a dangerous raider; sturdy human silhouette; face obscured by deep shadow and rough removable hood; patched work clothes, worn leather, rope and boundary-warning scraps; practical one-handed woodcutter axe slightly exaggerated into a raider hand axe; utility knife sheathed.
Style: strict handcrafted 16-bit pixel-art enemy, limited charcoal, timber brown, dirty cloth, dark leather and rusted iron palette.
Constraints: genuine transparent alpha; one enemy only; no scenery, cart, barricade, shadow, text, UI, thrown axe, blood or VFX; human work gear remains readable while identity stays obscured.
Avoid: wolf anatomy, fantasy bandit costume, giant double axe, horned helmet, full plate, skulls, steampunk, exposed normal face and extra limbs.
```

### 고블린 투명 배경 보정

```text
Remove only the baked checkerboard background and replace it with genuine transparent alpha. Preserve the enemy, fused basket, wicker, thorns, clothing and rusty dagger pixel-for-pixel.
```

### 고블린 v2 괴이화 보정

```text
Use case: precise-object-edit
Primary request: make the enemy read clearly as a nonhuman gathering horror at first glance, while preserving only faint second-read traces of its adult human gatherer origin.
Change the silhouette: lower compressed crouch; shoulders, spine and wicker basket fused into one asymmetrical shell; neck disappears into the woven hood; face becomes a featureless woven cavity; forearms lengthen beneath wicker tendrils; gnarled wicker-bound hands retain the same small rusty dagger; knees stay sharply bent and lower legs are bound with roots and basket fibers.
Strengthen basket fusion so wicker ribs and thorn branches grow across back, flank and shoulder instead of reading as a worn backpack.
Constraints: preserve facing left, dagger identity, palette, pixel-art rendering and transparent alpha; exactly two arms and two legs; no gore, text, UI, floor, shadow, VFX or scenery.
Avoid: normal human posture, ordinary hands, visible face, backpack straps, green fantasy goblin, pointed ears, child proportions, orc, quadruped, extra limbs and giant weapon.
```

## 4. 검증

- 고블린: 1290×1219 RGBA
- 약탈자: 1200×1310 RGBA
- 실제 알파 채널: 확인
- 방향: 왼쪽
- 배경·그림자·텍스트·UI·VFX: 없음
- Unity 임포트·슬라이스·애니메이터 연결: 미실행

## 5. 외형 승인 뒤 시트 작업

1. [x] 고블린 `Idle`·`단검 휘두르기` v1을 제작하고 승인했다.
2. [ ] 약탈자 `Idle`·`도끼 휘두르기` v1을 제작했으며 사용자 검토를 기다린다.
3. 승인된 시트만 Unity 임포트·슬라이스·애니메이터 연결 대상으로 넘긴다.

## 6. 반려본

고블린 v1은 인간 느낌이 강하다는 사용자 검토로 반려했고 `_workspace/graphics-remake/rejected/wave-a-enemy-anchors/Goblin_Filtered_IdleAnchor_v1_too-human.png`에 보존했다.
