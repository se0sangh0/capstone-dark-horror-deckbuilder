# GPT 컨셉 아트 프롬프트 가이드

> **문서 역할**: 컨셉 아트의 그래픽 풍을 고정하는 비주얼 마스터
> **기준일**: 2026-07-29
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
- [탐험 컨셉 아트](컨셉%20아트/탐험%20컨셉아트.png): 심연형 여백, 작은 조사관, 청록 랜턴, 다세계 건축 파편
- [잘못된 그림자 화톳불 이벤트](컨셉%20아트/잘못된%20그림자%20화툿불%20이벤트%20컨셉아트.png): 높은 시점, 황동색 화광, 파티 실루엣, 검은 외곽부
- [잘못 붙여진 복도](컨셉%20아트/잘못%20붙여진%20복도%20컨셉아트.png): 서로 다른 건축 재질의 봉합 방식
- [거두는 자](컨셉%20아트/거두는자%20컨셉아트.png): 초기 농경 신앙 토착신의 크기와 위계
- [우상숭배 이벤트](컨셉%20아트/우상숭배%20이벤트%20컨셉아트.png): 단일 이상 현상과 의식 공간의 연출

> 기존 이미지에 남아 있는 후기 중세 예배당·판금 갑옷·기사풍 요소는 **그래픽 풍과 구도만 참고**한다. 최초 테마의 현행 시대 기준은 [13 §3B-5](../기획_통합/13_서사_설정_확정.md)의 고대 말기~중세 초기이며, 새 이미지에서는 목석 성소·원형 방패·겹가죽·사슬·비늘 갑주로 교체한다.

---

## 1. 그래픽 풍 한 줄

> **행정적 통제가 타 세계의 일상과 방어를 적대적인 폐허와 괴이로 보이게 만든 공간을, 작은 조사관이 단 하나의 빛에 의지해 걷는 고밀도 시네마틱 픽셀 호러.**

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

> 테마별 실제 세계 원형·기본 인게임 왜곡·진실 시야 의미는 [기획_통합/13 §3B-0·§3B-5](../기획_통합/13_서사_설정_확정.md)를 기준으로 한다.

- 픽셀 밀도
- 암부 비율
- 조명 낙차
- 재질의 마모도
- 작은 인물과 거대한 공간의 대비
- 영혼석 청록과 경고 적색의 의미

테마별로 바뀌는 것은 동료 복식, 토착 건축, 보스의 신앙 도상과 이벤트 괴담 문법이다. 아래 실제 원형은 디자인 근거이며, 일반 컨셉 아트는 반드시 FILTERED 열처럼 왜곡해 출력한다.

| 테마 | 실제 세계 원형 `TRUE_BASELINE` | 기본 인게임 왜곡 `FILTERED` | 피해야 할 과장 |
|---|---|---|---|
| 초기 농경 신앙 | 흙성벽, 목석 성소, 원형 방패, 토착 농경 의례 | 곡창은 시체 저장고, 경계 기둥은 제물 말뚝, 주민은 수확 괴이로 보임 | 고딕 성당·완성형 판금·왕궁·MMORPG 장식 |
| 무협 변경 항전 | 볏짚 우의, 죽창·도검, 낡은 군기와 격문, 민초·임협 | 피난처는 약탈자 소굴, 군기와 봉화는 저주 의식, 항전자는 전쟁 귀신으로 보임 | 지맥·경락 도상, 선협식 비행, 귀족 문파, 과도한 금빛 신기 |
| 일본 신사 토착신 | 목조 신사, 도리이, 금줄, 등롱, 토착신과 요괴·귀신의 규칙 | 도리이는 감옥문, 금줄은 제물 봉인, 수호자는 통행을 막는 괴이로 보임 | 관광 엽서 같은 선명한 풍경, 닌자 판타지, 요괴 도감식 나열 |
| 현대 경계도시 | 학교, 병원, 지하철, 통제선, 재난대응 설비 | 대피소는 실험시설, 통제선은 감금 구역, 구조대원은 얼굴 없는 통제 괴이로 보임 | 사이버펑크 네온·미래 도시·좀비 아포칼립스 |

### 인식 레이어 사용 규칙

| 레이어 | 이미지 용도 | 표현 |
|---|---|---|
| `FILTERED` | 일반 컨셉 아트·기본 인게임 화면 | 실제 문화 실루엣은 유지하되 모든 방어·생활 행위를 적대 의식과 괴이로 오독 |
| `LEAK` | 오염·회차 암시 이미지 | FILTERED 장면에 실제 얼굴·표식·생활 흔적을 한두 곳만 겹침 |
| `TRUE_VIEW` | 내부 스포일러·엔딩 장면 | 괴물화를 제거하고 현지 수호자와 탐사국이 남긴 피해를 드러냄 |

별도 지정이 없는 모든 장면 모듈은 `FILTERED`다. `TRUE_BASELINE`은 일반 플레이용 완성 이미지가 아니라 설정 원본이며, `CA-07`만 `TRUE_VIEW`를 요청한다.

**가상 역사 원칙**

- 초기 농경 신앙 세계는 특정 고대·중세 국가나 종교를 재현하지 않고 여러 생활 재질·도구를 혼합한다.
- 무협 변경 항전 세계는 가상 왕조를 사용하며, 침략군은 별도 국가가 아니라 균열을 넘어온 **탐사국 원정대와 플레이어**다.
- 실제 국기·군기·왕조 문장·민족 복식·종교 상징을 그대로 복제하지 않는다.
- 별도의 침략 민족이나 적국 복식을 만들지 않는다. 현지 민초·임협은 생활 도구를 개조한 무기와 가상의 항전 표식으로 구분한다.

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
cracked school tiles becoming the rough stone of an early harvest sanctuary, a shrine
pillar growing through a ruined office wall, dry roots sewing masonry together, or
old road signs embedded in sacred architecture.

PERCEPTION LAYER: Unless the scene module explicitly requests TRUE_VIEW, render the
FILTERED in-game perception imposed by the Inspector institution. Preserve each
world's historical materials and silhouettes, but make inhabited places appear
abandoned and hostile, defensive boundaries appear ritualistic or imprisoning, local
protectors appear monstrous, and damage caused by the expedition appear to be the
world's own corruption. Do not show the normal world at face value.

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

For the early harvest and wuxia border-resistance dimensions, use fictional composite
cultures only. Do not reproduce a recognizable real-world flag, dynastic emblem,
military insignia, ethnic caricature, sacred symbol, or exact historical uniform.
Differentiate fictional groups through practical equipment, material wear, formation,
and invented abstract markings.

In the wuxia border-resistance dimension, the invading force is the player's own
Inspector expedition crossing the rift. Do not invent a separate foreign nation,
ethnic army, or off-screen invader faction. Show local farmers and wandering heroes
defending homes, granaries, graves, and evacuation routes from the player.

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
| 4 | 거두는 자의 왜곡된 성소 | 보스·토착 신앙·전투 절정 |

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
and its tiled wall gradually becomes the rough stone-and-timber frame of an early
harvest sanctuary. On the right, a weathered shrine gate and a broken roadside barrier
are half-swallowed by black roots that stitch them into the same cliff. The structures
continue downward at impossible angles but remain physically believable. Far below,
one tiny red inspection light glows with no visible device attached to it.

Camera: very wide establishing shot, slightly elevated behind the investigator.
Focal order: investigator's teal lantern, impossible architectural seam, distant red
point. The center and upper middle must retain large oppressive negative space.
Mood: entering a place that has been officially declared safe but was never repaired.
No party, no visible monster, no machinery, no fantasy castle, no readable signs.
```

### CA-02. 다섯 번째 그림자의 화톳불

```text
SCENE: ONE SHADOW TOO MANY

Four small companions rest around an undying campfire beneath an enormous old tree.
The FILTERED perception makes the well-maintained communal stone shelter look
abandoned and neglected: cracked benches, false moss stains, dead-looking branches,
and soot that appears decades old. Use the current early-harvest-dimension set: a
round-shield defender in layered leather and scale armor, a lightly equipped
hunter-attacker, a ritual caster, and a modest healer carrying herbs and a small
brazier. Their faces must remain unreadable; identify them by silhouette, equipment,
and posture only.

The fire projects exactly five human shadows across a broad standing stone beneath
the tree. Four shadows correspond clearly to the seated companions. The fifth is
taller, standing upright slightly apart, with no body casting it. This extra shadow
belongs to a random event and must be the single anomaly of the scene. None of the
companions is looking at it.

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

The room combines an ordinary records office with the remains of an early
stone-and-timber harvest sanctuary: dented filing cabinets sink into rough standing
stones, frosted observation glass cuts through a charred timber frame, and black
root-like seams hold the incompatible wall materials together. Behind the glass are
only vague human-shaped silhouettes, too soft to count. An inactive terminal reflects
one small warning-red indicator even though no device appears powered.

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
toward a soul-stone suspended at each chest. The forms belong to the current early
harvest dimension: one round-shield defender in layered leather and scale, one
weathered ritual caster, and one herb-bearing healer, but their faces remain absent
or obscured as if identity is the last part to load.

An investigator stands before the platform holding a closed registry folder. The hall
looks like a worn civic registry office built inside an older timber-and-stone harvest
sanctuary: wooden queue rails terminate inside packed-earth ritual steps, a service
counter grows into a communal offering table, and paper storage drawers are fused
with clay votive niches. One old brass lamp provides warm light; the materialization
provides the restrained teal accent.

Camera: wide frontal three-quarter view with the investigator small in the foreground
and the three forms clearly separated. Mood: a transaction presented as recruitment,
with something human reduced to inventory. Do not show a laboratory, cloning tubes,
robot parts, holographic UI, readable forms, or friendly shop decoration.
```

### CA-05. 거두는 자의 왜곡된 성소

```text
SCENE: THE REAPER'S FILTERED NEST

At the far end of what appears to be a hostile nest stands the Reaper, a towering
hollow scarecrow figure wrapped in a tattered off-white hooded robe. Its face is a
deep faceless shadow with two restrained dark-red points. It holds a weathered scythe
as tall as its body. Two black crows perch nearby rather than flying dramatically.

The FILTERED battlefield distorts an open timber-and-stone agricultural sanctuary.
Black roots, bone-like shapes, and dead straw seem to bind its low pillars, communal
altar, grain storage, and sealed refuge entrance into one organic den. Preserve enough
practical architecture to suggest that this hostile structure was once a place of
worship, food storage, and livelihood. Do not show the hidden residents.

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
PERCEPTION LAYER: TRUE_VIEW. This explicitly overrides the default FILTERED layer.

Show the same damaged open timber-and-stone harvest sanctuary after the investigator
refuses the re-identification process. Do not restore it into a pristine place:
retain damage caused by the expedition, but remove the false roots, bones, and
monstrous nest cues. Reveal ordinary grain storage, a communal altar, a sealed refuge
entrance, defensive boundary markers, family offerings, and signs of evacuation.

The supposed Reaper is now perceived as an exhausted child-shaped guardian deity of
a rural world. The deity resembles an ordinary village child wearing a worn ritual
robe and straw mantle, holding a small harvest sickle rather than a giant scythe.
Ancient, tired eyes and posture convey a life far older than the youthful form. The
deity stands protectively between the investigator and the refuge entrance while
offering one final chance to leave. No wounds, no gore, no sexualization, and no
helpless victim framing.

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
rough harvest-sanctuary stone and timber, shrine pillars, roadside signs, worn office
walls, and practical field equipment. The horror must feel institutional and familiar,
not epic.
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
