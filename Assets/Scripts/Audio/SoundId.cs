// SoundId.cs
// 사운드 식별자 enum. SoundDatabase 와 AudioManager 가 공통으로 사용.
//
// ── 신규 사운드 추가 절차 ──────────────────────────────────────
//   1) 여기 BGM / SFX 카테고리에 enum 값 추가
//   2) Resources/Audio/SoundDatabase.asset 인스펙터에서 클립 할당
//   3) 호출 측에서 AudioManager.Instance.PlaySfxById(SoundId.XXX) 사용

public enum BgmId
{
    None = 0,
    Title,         // GameStartScene
    NodeMap,       // 노드맵
    Battle,        // 전투
    Rest,          // 화톳불
    Mercenary,     // 용병 사무소
    Boss,          // 보스 전투 (2026-06-09 신규 에셋)
}

public enum SfxId
{
    None = 0,

    // 카드
    CardDraw,
    CardSelect,
    CardPlay,

    // 전투
    AttackMelee,
    AttackSword,
    HurtAlly,
    HurtEnemy,
    FellowDeath,
    EnemyDeath,
    EnemySkill,

    // UI / 버튼
    ButtonClick,
    Confirm,
    Cancel,

    // 재화 / 거래
    CoinGain,
    CoinSpend,
    Recruit,
    Sell,

    // 결과
    Victory,
    Defeat,

    // 회복
    Heal,

    // 노드 (2026-06-09 — 노드 전용음. 일반 버튼/확인음과 분리)
    NodeMove,    // 노드 클릭(이동)
    NodeEnter,   // 노드 진입(전투 등)

    // 스킬 전용음 (2026-06-09 — 스킬명별 재생. BattleManager.Combat.UseSkill)
    SkillFireball,   // 파이어볼
    SkillStrike,     // 무모한 강타 / 매직 미사일 (공용)
    SkillAxeThrow,   // 도끼 던지기 (약탈자 적 스킬)

    // 씬 전환 (2026-06-09 — 페이드 로딩 연출음)
    SceneTransition, // 씬 전환 페이드
}
