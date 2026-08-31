# Battle Forest Style Anchor v1

- 상태: 2026-08-30 사용자 승인 · Wave A 시각 기준 · 현재 Unity 미연결
- 생성 방식: Codex built-in `image_gen`
- 생성일: 2026-08-30
- 이미지: `Battle_Forest_StyleAnchor_v1.png`
- Notion 미리보기: `Battle_Forest_StyleAnchor_v1_preview.svg`
- Git 기준 폴더: `docs/assets/graphics-remake/style-anchors/`
- 크기: 1672×941 · RGB PNG · 알파 없음
- SHA-256: `6dee84c2d0f0f4b0943454b02a5a85a351396daaf0ec42ece830a37ff7109361`

## 참조 이미지 역할

- `01_탐사_진입_키아트.png`: 픽셀 밀도·암부·재질·청록 보조광 참고
- `03_거두는자_대치_키아트.png`: 팔레트·명암 낙차·초기 농경 신앙 재질 참고
- 두 이미지는 스타일 참고일 뿐 구도·인물·장소·오브젝트를 복사하지 않음

## 최종 프롬프트

```text
Use case: stylized-concept
Asset type: new 2D game battle background style-anchor for the first forest combat area, 16:9 landscape
Input images: Image 1 and Image 2 are style, pixel density, darkness, material, and lighting references only; do not copy their composition, characters, gate, boss, shrine, or objects
Primary request: generate a brand-new dark-horror battle background for a side-view deckbuilder combat scene in an early-medieval mountain forest corridor
Scene/backdrop: damp narrow forest trail between dark rock walls, low brush, cut vines, tangled footprints, weathered wood and rough stone traces; no recognizable settlement and no revealed shrine
Style/medium: high-density cinematic pixel art, deliberate crisp pixel clusters, richly textured but readable at game scale; same disciplined darkness and bronze-brown/charcoal mood as the references
Composition/framing: exact wide 16:9 composition; high horizon; lower 45 percent forms a clear, mostly level combat plane; left and right combat slots remain readable; restrained center detail; no central hero prop; safe outer margins for cropping
Lighting/mood: overcast dusk, heavy shadow, faint cold teal reflected light and very sparse warm amber accents; ominous, quiet, administrative-horror mood; characters must remain readable when overlaid
Color palette: charcoal black, wet gray stone, dead brown vegetation, muted bronze, very limited desaturated teal
Materials/textures: wet rock, rough timber, matted leaves, cut vine fibers, sparse mud
Constraints: environment only; no people, allies, enemies, creatures, corpses, weapons, wagons, tents, UI, text, logo, health bars, watermark, in-image frame, attack VFX, glowing portals; no bright blue sky
Avoid: photorealism, smooth painterly illustration, 3D render, gothic arches, stained glass, castle, full plate armor, giant moon, machinery, gears, pipes, neon, sci-fi technology, child figure, villagers, hidden residents, explicit story revelation
```

## 확인할 점

- 현재 `BattleBackground.backgroundBrightness=0.3`을 그대로 적용하면 지나치게 어두워질 수 있음
- 실제 캐릭터를 올린 1280×720·1920×1080 캡처에서 명도·좌우 실루엣 가독성을 먼저 확인
- 승인 전에는 `Assets/Resources/BackGround/Battle_Normal.png`를 덮어쓰지 않음
