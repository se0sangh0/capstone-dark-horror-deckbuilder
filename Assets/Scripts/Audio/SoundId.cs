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
}
