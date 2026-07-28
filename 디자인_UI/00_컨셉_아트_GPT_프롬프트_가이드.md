# GPT 컨셉 아트 프롬프트 가이드

> **문서 역할**: 컨셉 아트의 그래픽 풍을 고정하는 비주얼 마스터
> **기준일**: 2026-07-28
> **적용 대상**: GPT 이미지 생성, 키 아트, 환경 컨셉, 장면 삽화
> **비적용 대상**: 실제 게임용 초상·스프라이트·애니메이션 시트

## 0. 먼저 구분할 것

현재 참고 이미지의 그래픽 풍은 엄격한 저해상도 16비트 스프라이트가 아니라, **고밀도 시네마틱 픽셀 아트 컨셉 일러스트**다.

| 구분 | 컨셉 아트 | 게임 제작용 에셋 |
|---|---|---|
| 목적 | 세계관·조명·공간·감정 확정 | 실제 UI·전투에서 사용 |
| 픽셀 표현 | 고밀도 픽셀 클러스터, 풍부한 재질 | 큰 픽셀, 제한 팔레트, 작은 실루엣 |
| 구도 | 16:9 시네마틱 장면 | 정면 초상, 측면 유닛, 스프라이트 시트 |
| 기준 문서 | **이 문서** | [캐릭터 프롬프트](00_캐릭터_디자인_프롬프트.md) · [적 프롬프트](00_적_캐릭터_디자인_프롬프트.md) |

**기존 스타일 레퍼런스**
- [탐험 컨셉 아트](컨셉%20아트/탐험%20컨셉%20아트.jpeg): 심연형 여백, 작은 조사관, 청록 랜턴, 다세계 건축 파편
- [화톳불 이벤트 컨셉 아트](컨셉%20아트/화툿불%20이벤트%20컨셉아트.jpeg): 높은 시점, 황동색 화광, 파티 실루엣, 검은 외곽부

---

## 1. 그래픽 풍 한 줄

> **행정적 통제로 정상처럼 보이게 만든 다세계의 폐허를, 작은 조사관이 단 하나의 빛에 의지해 걷는 고밀도 시네마틱 픽셀 호러.**

### 반드시 반복되는 시각 문법

| 축 | 고정 규칙 |
|---|---|
| 공간 | 서로 다른 세계의 **평범한 장소**가 물리적으로 잘못 봉합되어 있다 |
| 인물 | 인물은 작고 취약하다. 얼굴보다 자세와 실루엣을 읽게 한다 |
| 암부 | 화면의 65~75%는 차콜 블랙에 가까운 암부로 남긴다 |
| 현실광 | 화톳불·랜턴·낡은 전등처럼 이해 가능한 따뜻한 광원 1개 |
| 비현실광 | 영혼석 청록 또는 경고 적색 중 1개만 보조 광원으로 사용 |
| 재질 | 젖은 돌, 금 간 회벽, 오래된 목재, 마른 흙·짚, 낡은 천, 종이 기록물 |
| 공포 | 피와 고어보다 **기록 오류·반복 흔적·하나 더 많은 그림자·잘못 이어진 공간** |
| 구성 | 주된 이상 현상은 한 장면에 1개만 둔다 |

### 이 게임만의 공간 봉합 방식

- 건물을 콜라주처럼 나열하지 않는다.
- 교실 문틀이 예배당 아치로 이어지거나, 사당 기둥이 현대 복도 타일에서 자라는 식으로 **재료와 구조가 중간에서 변형**된다.
- 봉합선은 검은 뿌리, 금 간 모르타르, 마른 짚, 먹선 같은 물질로 표현한다.
- 스팀펑크 금속과 기계는 **UI 언어**다. 환경 컨셉에는 톱니·파이프·황동 기계를 반복하지 않는다.
- 탐사국 장면에서만 낡은 캐비닛, 종이 서류, 아날로그 단말기, 산업용 조명을 제한적으로 허용한다.

### 차원 테마 운용

런의 차원 테마가 달라져도 아래 항목은 바뀌지 않는다.

- 픽셀 밀도
- 암부 비율
- 조명 낙차
- 재질의 마모도
- 작은 인물과 거대한 공간의 대비
- 영혼석 청록과 경고 적색의 의미

테마별로 바뀌는 것은 동료 복식, 토착 건축, 보스의 신앙 도상이다.

| 테마 | 주된 외형 언어 | 피해야 할 과장 |
|---|---|---|
| 중세 판타지 | 석조 예배당, 성소, 갑옷, 로브, 농경 신앙 | 화려한 왕궁·MMORPG 장식 |
| 무협 | 산문, 사당, 먹선, 부적, 협객 복식 | 선협식 비행·과도한 금빛 신기 |
| 일본식 | 목조 신사, 도리이, 종이 부적, 순례 복식 | 관광 엽서 같은 선명한 풍경 |
| 현대 | 학교, 휴게실, 도로, 산업 표지, 사무 설비 | 사이버펑크 네온·미래 도시 |

---

## 2. GPT 대화 시작용 스타일 잠금

새 대화에서 기존 컨셉 이미지 2장을 첨부한 뒤 아래 문장을 먼저 입력한다.

```text
Use the two attached images only as visual style references for the INSPECTOR game
concept art. Lock their pixel density, near-black shadow ratio, restrained palette,
steep light falloff, worn material rendering, and the scale contrast between a tiny
human figure and a monumental environment. Do not copy their exact composition,
architecture, characters, or object placement. Future images must feel as if they
were painted by the same pixel artist for the same game world.
```

그 다음부터 아래 **공통 스타일 앵커**를 장면 프롬프트 앞에 그대로 붙인다.

---

## 3. 공통 스타일 앵커

```text
INSPECTOR CONCEPT ART STYLE LOCK

Create one single cinematic 16:9 dark-horror pixel-art concept illustration. This is
high-density handcrafted pixel art for game key art, not a tiny gameplay sprite and
not a smooth painting passed through a pixel filter. Use deliberate square pixel
clusters, hard pixel edges, selective dithering, restrained texture, and no
anti-aliasing. Keep forms readable at thumbnail size while preserving rich material
detail at full size.

The visual identity is bureaucratic liminal horror: ordinary physical places from
incompatible worlds are fused together at structurally wrong angles, as if reality
was repaired by an institution that cared more about hiding the seam than restoring
the world. Show believable transitions between materials instead of a random collage:
cracked school tiles becoming chapel stone, a shrine pillar growing through a ruined
office wall, dry roots sewing masonry together, or old road signs embedded in sacred
architecture.

Composition: one dominant focal anomaly, monumental environment, tiny vulnerable
human figures placed in the lower third, strong negative space, controlled asymmetry,
and clear depth. Keep 65-75% of the frame in near-black charcoal shadow. Lighting must
come from one understandable warm source such as a lantern, campfire, or old lamp,
plus at most one unnatural accent source.

Strict palette: charcoal black #0F1115 dominant; oxidized brass and dry earth
#C39A52; dusty cold gray #667085; relic soul-light teal #4CB3B3; one restrained
warning red #B3263E only when the scene needs danger. Purple is reserved for explicit
ritual or re-identification scenes and must not appear by default.

Materials: damp stone, cracked plaster, aged timber, dry soil and straw, worn cloth,
corroded signage, fog, dust, and paper records. Horror comes from absence, repetition,
incorrect records, impossible shadows, and architecture that almost makes sense.
Avoid graphic gore and avoid a visible attacking monster unless the scene explicitly
requires one.

No readable text, no letters, no numbers, no logo, no UI, no border, no watermark,
no contact sheet, and no multiple panels. No glossy 3D render, no anime illustration,
no smooth painterly brushwork, no photorealism, no bright fantasy spectacle, no
cyberpunk neon, no holograms, no ornate steampunk machinery, no decorative gear
networks, and no generic medieval castle vista.
```

---

## 4. 장면 프롬프트 사용법

1. GPT 대화 첫 메시지에 기존 컨셉 이미지 2장과 §2 스타일 잠금을 전달한다.
2. 이미지를 만들 때마다 **§3 공통 스타일 앵커 + 아래 장면 모듈 1개**를 함께 입력한다.
3. 한 번에 한 장만 생성한다. 4분할 시안이나 콘택트 시트를 요청하지 않는다.
4. 가장 방향이 맞는 결과를 다음 생성 때 다시 첨부하고 `keep the visual language unchanged`라고 명시한다.
5. 변형은 카메라·시간대·이상 현상 중 한 축만 바꾼다.
6. 공개용 이미지는 [13 서사 설정](../기획_통합/13_서사_설정_확정.md)의 진실을 직접 노출하지 않는다.

### 1차 생성 권장 순서

| 순서 | 이미지 | 확인할 시각 기준 |
|---|---|---|
| 1 | 균열 탐험 키 아트 | 세계 봉합·인물 크기·암부 |
| 2 | 다섯 번째 그림자의 화톳불 | 조명·은근한 공포 |
| 3 | 복귀 브리핑실 | 행정적 호러·탐사국 정체성 |
| 4 | 거두는 자의 버려진 성소 | 보스·토착 신앙·전투 절정 |

---

## 5. 장면 모듈

아래 코드 블록 하나를 §3 공통 스타일 앵커 뒤에 붙여 사용한다.

### CA-01. 균열 탐험 키 아트

```text
SCENE: THE SEAMED DESCENT

A lone field investigator stands at the lower edge of a colossal descending rift,
seen from behind, holding an old hand lantern containing a dim teal soul-stone.
The investigator wears a practical dark survey coat, a small field satchel, gloves,
and no heroic ornament. The figure must occupy less than 8% of the frame.

The rift is not a natural cave. On the left, a ruined school corridor bends downward
and its tiled wall gradually becomes the stone arch of a chapel. On the right, a
weathered shrine gate and a broken roadside barrier are half-swallowed by black roots
that stitch them into the same cliff. The structures continue downward at impossible
angles but remain physically believable. Far below, one tiny red inspection light
glows with no visible device attached to it.

Camera: very wide establishing shot, slightly elevated behind the investigator.
Focal order: investigator's teal lantern, impossible architectural seam, distant red
point. The center and upper middle must retain large oppressive negative space.
Mood: entering a place that has been officially declared safe but was never repaired.
No party, no visible monster, no machinery, no fantasy castle, no readable signs.
```

### CA-02. 다섯 번째 그림자의 화톳불

```text
SCENE: ONE SHADOW TOO MANY

Four small companions rest around a modest campfire on a broken stone floor suspended
at the edge of darkness. Use the current medieval-dimension set: a shield-bearing
defender, a lightly armored attacker, a robed caster, and a modest priest. Their faces
must remain unreadable; identify them by silhouette, equipment, and posture only.

Behind them stands a cracked wall where old chapel masonry changes halfway into
painted school brick. The campfire projects exactly five human shadows across that
wall. Four shadows correspond clearly to the seated companions. The fifth is taller,
standing upright slightly apart, with no body casting it. None of the companions is
looking at the extra shadow.

Camera: elevated three-quarter view, wide enough to show the complete group and the
entire shadow wall. Warm brass firelight is the only strong light; one closed satchel
near the group leaks a very faint teal glow. Keep the outer half of the frame almost
black. Mood: temporary safety maintained by everyone agreeing not to notice the
evidence. No creature, no dramatic reaction, no extra person, no UI, no text.
```

### CA-03. 복귀 브리핑실

```text
SCENE: RETURN BRIEFING

A lone investigator sits at a heavy desk in a dim institutional briefing room after
a mission. The composition is viewed from the back corner, making the investigator
look small and observed. On the desk: a stack of nearly identical field reports, an
ink stamp, a sealed soul-stone container emitting weak teal light, and an old task
lamp casting a narrow warm cone. All paper content must be abstract and unreadable.

The room combines an ordinary records office with the remains of a chapel: dented
filing cabinets sink into old stone columns, frosted observation glass cuts through
a bricked arch, and black root-like seams hold the incompatible wall materials
together. Behind the glass are only vague human-shaped silhouettes, too soft to count.
An inactive analog terminal reflects one small warning-red indicator even though no
device appears powered.

Camera: wide, slightly high, with the desk in the lower third and a large dark ceiling
above. Mood: routine paperwork that quietly resembles an interrogation and memory
correction. Use sparse analog office equipment only. No futuristic laboratory, no
holograms, no cyberpunk screens, no readable labels, no explicit torture device.
```

### CA-04. 영혼석 구현체 모집소

```text
SCENE: RECONSTRUCTED COMPANIONS

Inside a cold recruitment hall, three incomplete humanoid companion forms stand on a
low stone platform. They are being assembled from translucent teal fragments drawn
toward a soul-stone suspended at each chest. The forms belong to the current medieval
dimension: one armored defender, one weathered caster, and one priest, but their faces
remain absent or obscured as if identity is the last part to load.

An investigator stands before the platform holding a closed registry folder. The hall
looks like a worn civic registry office built inside an older sanctuary: wooden queue
rails terminate inside stone prayer steps, a service counter grows into an altar, and
paper storage drawers are fused with devotional niches. One old brass lamp provides
warm light; the materialization provides the restrained teal accent.

Camera: wide frontal three-quarter view with the investigator small in the foreground
and the three forms clearly separated. Mood: a transaction presented as recruitment,
with something human reduced to inventory. Do not show a laboratory, cloning tubes,
robot parts, holographic UI, readable forms, or friendly shop decoration.
```

### CA-05. 거두는 자의 버려진 성소

```text
SCENE: THE ABANDONED HARVEST SANCTUARY

At the far end of a ruined agricultural sanctuary stands the Reaper, a tall hollow
scarecrow figure wrapped in a tattered off-white hooded robe. Its face is a deep
faceless shadow with two restrained dark-red points. It holds a weathered scythe as
tall as its body. Two black crows perch nearby rather than flying dramatically.

The battlefield is a dry harvest field growing through the floor of a collapsed
chapel. Broken pews are half-buried in soil; sheaves of dead grain are arranged like
old offerings; a cracked circular harvest emblem is visible behind the figure. The
space must suggest that this was once a place of worship and livelihood before it was
classified as a hostile zone.

In the lower left, four tiny party silhouettes face the Reaper in a restrained battle
formation. A faint teal rim light separates them from the dark. The Reaper is backlit
by one low warning-red glow, not a magical explosion.

Camera: very wide side-facing confrontation, monumental boss scale, strong central
aisle leading toward the Reaper. Mood: dread mixed with the suspicion that the party
is trespassing in someone else's sacred place. No attack pose, no slash trail, no VFX
burst, no gore, no giant moon, no ornate cathedral spectacle, no UI.
```

### CA-06. 잘못 봉합된 일상 복도

```text
SCENE: THE CORRIDOR THAT WAS REPAIRED WRONG

An empty school corridor at night extends into the distance under several dead ceiling
lights. Halfway down the corridor, the tiled floor and painted walls gradually become
the timber floor and stone threshold of a small shrine, but the perspective remains
continuous as if the building considers them the same place. A faded evacuation map
has every arrow physically scratched toward the seam, but no symbol or text is
readable.

Near the foreground sits a paper cup that still releases a thin thread of steam even
though thick dust covers the floor around it. A hairline crack beneath the cup emits
the faintest teal light. No person is visible.

Camera: one-point perspective, eye level, strong depth, the seam slightly off-center.
One weak warm emergency lamp lights the foreground; the far corridor disappears into
charcoal darkness. Mood: an ordinary place preserving evidence of a routine that
should have ended long ago. No ghost, no monster, no blood, no dramatic portal, no
machinery, no readable text.
```

### CA-07. 내부 스포일러용 진실 시야

> 🔴 팀 내부 전용. 발표·공개 컨셉에는 사용하지 않는다.

```text
SCENE: WHEN RE-IDENTIFICATION FAILS

Show the same abandoned harvest sanctuary after the investigator refuses the
re-identification process. The supposed Reaper is now perceived as the exhausted
guardian deity of a rural world: still tall and robed, still carrying the same
harvest scythe, but its posture is protective rather than predatory. It kneels beside
an extinguished offering fire, one open hand asking for mercy. The red eye-points are
gone. Its hood reveals no human face, only a soft darkness framed by woven straw and
weathered ritual cloth.

The party's companion forms are partially transparent around their teal soul-stones.
Within the fragments are brief silhouettes of ordinary local people, implying that
the companions were reconstructed from stolen souls. Keep this subtle and symbolic,
not anatomical or graphic.

Camera: wide from behind the investigator, who stands between the translucent party
and the kneeling guardian. Warm light comes from a dawn that reaches only the far
field; the foreground retains the established near-black palette. Mood: moral horror,
recognition, and the terrible stillness before choosing whether to continue the
attack. No combat action, no villain pose, no triumphant hero framing, no text, no UI.
```

---

## 6. 결과 수정용 GPT 명령

스타일이 흔들릴 때 새 프롬프트를 다시 쓰지 말고, 승인된 이미지와 함께 아래처럼 한 축만 수정한다.

### 픽셀 아트가 매끈한 일러스트로 나온 경우

```text
Keep the composition and scene content unchanged. Re-render only the surface language
as high-density handcrafted pixel art: visible square pixel clusters, hard stepped
edges, selective dithering, no anti-aliasing, and no smooth painterly gradients.
Do not reduce the image into a tiny sprite or apply a uniform pixelation filter.
```

### 화면이 너무 밝은 경우

```text
Keep every object and camera position unchanged. Reduce ambient fill light so that
70% of the frame falls into near-black charcoal shadow. Preserve only the existing
warm practical light and the single restrained teal or red accent light.
```

### 일반적인 판타지 성처럼 나온 경우

```text
Keep the mood and camera unchanged. Replace generic castle architecture and ornate
fantasy decoration with ordinary lived-in structures fused incorrectly: school tile,
chapel masonry, shrine timber, roadside signs, worn office walls, and practical field
equipment. The horror must feel institutional and familiar, not epic.
```

### 스팀펑크·사이버펑크로 치우친 경우

```text
Remove decorative gears, pipe networks, glowing circuitry, holograms, neon signs,
and futuristic machinery. Keep only sparse analog institutional objects where the
scene requires them. Shift brass color onto old wood, dry straw, and worn stone rather
than machines.
```

### 장면이 산만한 경우

```text
Keep only one focal anomaly. Remove secondary monsters, extra props, decorative
lights, floating particles, and unnecessary symbols. Restore large negative space
and make the human figures smaller.
```

### 세계 파편이 콜라주처럼 보이는 경우

```text
Do not place separate themed buildings side by side. Make the materials transform
continuously across one believable structure: tile becomes stone, timber grows
through plaster, roots stitch masonry, and perspective remains physically coherent.
```

### 캐릭터가 주인공처럼 과장된 경우

```text
Keep the environment unchanged. Make every character smaller, less detailed, and
identified by silhouette and equipment only. Remove heroic posing, facial close-ups,
ornate armor, capes in motion, and spotlight framing.
```

---

## 7. 최종 선별 체크리스트

- [ ] 두 기존 컨셉 이미지와 같은 픽셀 밀도와 조명 낙차인가?
- [ ] 화면의 65~75%가 어두운 중성색인가?
- [ ] 따뜻한 현실광 1개와 비현실 강조광 최대 1개만 있는가?
- [ ] 인물이 환경보다 작고 취약하게 보이는가?
- [ ] 서로 다른 공간이 나열되지 않고 물질적으로 봉합되어 있는가?
- [ ] 한 장면의 이상 현상이 하나로 읽히는가?
- [ ] 공포가 고어보다 기록·반복·부재·오류에서 오는가?
- [ ] 스팀펑크 기계·사이버펑크 네온·화려한 판타지 장식이 억제됐는가?
- [ ] 텍스트·로고·UI·워터마크가 없는가?
- [ ] 축소해서 보아도 실루엣과 초점이 읽히는가?

## 한 줄 결론

**컨셉 아트의 핵심은 "어두운 판타지"가 아니라, 정상으로 관리되고 있다고 주장하는 기관 아래에서 서로 다른 현실이 잘못 봉합된 흔적을 발견하는 순간이다.**
