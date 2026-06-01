// ============================================================
// DefaultSetting.cs
// 전투 카드 오브젝트 생성 및 동료 이미지/HP 슬라이더 초기 설정
// ============================================================
//
// [이 파일이 하는 일]
//   씬이 시작될 때 오브젝트 프리팹을 지정된 개수만큼 생성하고,
//   생성된 오브젝트에 동료 이미지와 HP 슬라이더를 연결합니다.
//
//   아군(Ally) 오브젝트면 BattleManager.allies 목록에서
//   동료 스프라이트와 HP 슬라이더를 가져옵니다.
//
// [중요: 동적 참조]
//   이 스크립트는 BattleManager.Instance.allies 를 직접 참조합니다.
//   BattleManager 가 먼저 Start() 에서 allies 를 초기화하므로
//   이 스크립트의 Start() 보다 BattleManager.Start() 가 먼저 실행되어야 합니다.
//
// [어디서 쓰이나요?]
//   - 전투 씬에서 아군/적군 오브젝트를 생성하는 오브젝트에 붙임
//
// [인스펙터 설정]
//   - factionType : Ally(아군) 또는 Enemy(적군) 선택
//   - ObjectPrefab  : 생성할 카드 프리팹 연결
//   - ObjectCount   : 생성할 카드 개수
//   - startX      : 첫 카드 X 시작 좌표
//   - spacingX    : 카드 간격
// ============================================================

using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

// ----------------------------------------------------------
// [FactionType 열거형]
// 이 오브젝트가 아군 오브젝트를 생성할지, 적군 오브젝트를 생성할지 선택
// ----------------------------------------------------------
/// <summary>소속 팀 구분 (아군 / 적군)</summary>
public enum FactionType { Ally, Enemy }

/// <summary>
/// 전투 씬 시작 시 오브젝트를 생성하고
/// 동료 이미지/HP 슬라이더를 연결하는 초기 설정 컴포넌트.
/// </summary>
public class DefaultSetting : MonoBehaviour
{
    // ----------------------------------------------------------
    // [소속 설정]
    // ----------------------------------------------------------
    [Header("소속 설정")]
    [Tooltip("이 오브젝트가 아군 오브젝트를 생성할지, 적군 오브젝트를 생성할지 선택하세요.")]
    public FactionType factionType = FactionType.Ally;

    // ----------------------------------------------------------
    // [생성 설정]
    // ----------------------------------------------------------
    [Header("생성 설정")]
    [Tooltip("생성할 오브젝트 프리팹 (Inspector 에서 연결하세요)")]
    public GameObject ObjectPrefab;

    [Tooltip("생성할 오브젝트 개수")]
    public int ObjectCount;

    // ----------------------------------------------------------
    // [위치 설정]
    // ----------------------------------------------------------
    [Header("위치 설정")]
    [Tooltip("첫 번째 오브젝트가 생성될 X 좌표 시작점 (이 오브젝트 위치 기준)")]
    public float startX = -0.3f;

    [Tooltip("카드와 카드 사이 X 축 간격")]
    public float spacingX = 0.15f;

    [Tooltip("Y 축 오프셋 — 이름/HP 텍스트와 캐릭터 겹침 방지용 (음수면 카드 아래로)")]
    public float spawnOffsetY = 0f;

    // 사망 → 재정렬 이벤트 구독 추적 — 카드 정리 시 안전하게 해제
    private readonly List<System.Action> _diedUnsubscribers = new();

    // ----------------------------------------------------------
    // OnEnable — 화면이 다시 켜질 때마다 카드 오브젝트 재생성
    // ----------------------------------------------------------
    void OnEnable()
    {
        if (BattleManager.Instance == null)
        {
            // 로그 주석 처리 (사용자 요청)
            // Debug.LogWarning("[DefaultSetting] BattleManager.Instance 가 없어 스폰을 건너뜁니다.");
            return;
        }

        ClearSpawnedObjects();
        SpawnObject();

        // 적 측만 — 전투 중 새로 소환된 적 카드를 자동 append
        if (factionType == FactionType.Enemy)
            BattleManager.Instance.OnEnemySpawned += HandleEnemySpawned;
    }

    void OnDisable()
    {
        if (factionType == FactionType.Enemy && BattleManager.Instance != null)
            BattleManager.Instance.OnEnemySpawned -= HandleEnemySpawned;

        foreach (var unsub in _diedUnsubscribers) unsub?.Invoke();
        _diedUnsubscribers.Clear();
    }

    // 신규 소환된 적 1마리만 카드 추가 (기존 살아있는 적 카드는 그대로 유지)
    void HandleEnemySpawned(EnemyData spawned)
    {
        if (factionType != FactionType.Enemy) return;
        var enemies = BattleManager.Instance.enemies;
        int idx = enemies.IndexOf(spawned);
        if (idx < 0) return;
        SpawnOneCard(idx);
        RelayoutCards(); // 소환 후 보스 후열 보장 + 센터 압축
    }

    /// <summary>
    /// 기존에 생성해둔 카드 오브젝트를 정리한다.
    /// (전투 재진입 시 중복 생성 방지)
    /// </summary>
    void ClearSpawnedObjects()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i).gameObject;
            // Destroy 는 프레임 끝에 실행 — 같은 프레임의 RelayoutCards 가 자식을 훑을 때 잡히면
            // 이전 카드 + 새 카드 둘 다 위치 재배정 받아 누적되는 현상 발생.
            // → SetParent(null) 로 즉시 분리 후 Destroy 예약.
            child.transform.SetParent(null, false);
            Destroy(child);
        }
    }

    // ----------------------------------------------------------
    // 카드 생성 + 이미지/슬라이더 연결
    // ----------------------------------------------------------

    /// <summary>
    /// ObjectCount 만큼 카드 프리팹을 생성하고
    /// 각 오브젝트에 동료 이미지와 HP 슬라이더를 연결한다.
    /// </summary>
    void SpawnObject()
    {
        var allies  = BattleManager.Instance.allies;
        var enemies = BattleManager.Instance.enemies;

        for (int i = 0; i < ObjectCount; i++)
        {
            // ✨ 실제 엔티티 수보다 많은 슬롯은 빈 카드 생성 안 함 (흰 배경 방지)
            if (factionType == FactionType.Enemy && i >= enemies.Count) continue;
            if (factionType == FactionType.Ally  && i >= allies.Count)  continue;
            SpawnOneCard(i);
        }

        // 시작 시점부터 센터 압축(아군 좌측·적 우측) + 보스 후열 보장 적용 — 즉시 스냅
        RelayoutCards(instant: true);
    }

    // 카드 1장 생성 — SpawnObject 의 for 본문 + 이벤트 기반 신규 소환 양쪽에서 공용.
    void SpawnOneCard(int i)
    {
        var allies  = BattleManager.Instance.allies;
        var enemies = BattleManager.Instance.enemies;

        // startX 부터 spacingX 간격으로 X 위치 계산
        // 중앙을 기준으로 아군은 오른쪽에서 왼쪽으로 배치, 적군은 왼쪽에서 오른쪽으로 배치
        float currentX = startX + (factionType == FactionType.Ally ? -1 : 1) * (spacingX * i);
        Vector3 newPos = transform.position + new Vector3(currentX, spawnOffsetY, 0f);

        GameObject newObj = Instantiate(ObjectPrefab, newPos, Quaternion.identity);
        newObj.transform.parent = this.transform;
        newObj.name = ObjectPrefab.name + "_" + i;

        // 카드 sprite 적용 — root SpriteRenderer 에 정적 초기 sprite 할당.
        // Animator + .anim 가 있으면 즉시 덮어쓰지만 (아군), 적은 Animator controller 없이 정적 표시 유지.
        var rootSr = newObj.GetComponent<SpriteRenderer>();
        if (rootSr != null) ApplyObjectSprite(i, rootSr, newObj.name);

        if (factionType == FactionType.Ally && i < allies.Count)
        {
            var slider = newObj.GetComponentInChildren<UnityEngine.UI.Slider>();
            if (slider != null)
            {
                allies[i].InitHp(slider);
                slider.maxValue = allies[i].maxHp > 0 ? allies[i].maxHp : 100;
                slider.value    = allies[i].CurrentHp;

                var shieldBarUI = newObj.GetComponentInChildren<ShieldBarUI>();
                if (shieldBarUI != null) shieldBarUI.Init(allies[i], slider);

                var deathHider = newObj.AddComponent<AllyCardDeathHider>();
                deathHider.Bind(allies[i]);
            }

            var battleCard = newObj.GetComponent<BattleCardView>();
            if (battleCard != null) battleCard.BindFellow(allies[i]);

            SubscribeRelayoutOnDeath(allies[i]);
        }
        else if (factionType == FactionType.Enemy && i < enemies.Count)
        {
            var slider = newObj.GetComponentInChildren<UnityEngine.UI.Slider>();
            if (slider != null)
            {
                enemies[i].InitHp(slider);
                slider.maxValue = enemies[i].maxHp;
                slider.value    = enemies[i].CurrentHp;
            }

            var deathHider = newObj.AddComponent<EnemyCardDeathHider>();
            deathHider.Bind(enemies[i]);

            var battleCard = newObj.GetComponent<BattleCardView>();
            if (battleCard != null) battleCard.BindEnemy(enemies[i]);

            // 적별 visualScale 적용 — prefab 기본 scale 에 곱셈. enemies.json 의 visualScale 필드.
            float vs = enemies[i].visualScale > 0 ? enemies[i].visualScale : 1.0f;
            newObj.transform.localScale = ObjectPrefab.transform.localScale * vs;

            SubscribeRelayoutOnDeath(enemies[i]);
        }

        // ── Facing: 아군은 오른쪽(적 방향), 적은 왼쪽(아군 방향) 보도록 비주얼만 flip ──
        // ConfigureFaction: Melee dash 방향(아군 +X / 적군 -X) 결정용
        var sprites = newObj.GetComponent<BattleCardSprites>();
        if (sprites != null)
        {
            bool isEnemy = factionType == FactionType.Enemy;
            sprites.SetFacing(faceLeft: isEnemy);
            sprites.ConfigureFaction(isEnemy);
        }

        // ── Animator 주입 ────────────────────────────────────────────
        //   데이터의 animatorPath 가 채워져 있으면 Resources 에서 controller 로드,
        //   prefab 의 Animator 컴포넌트에 끼우고 BattleCardSprites 에 전달.
        //   비어 있거나 로드 실패 시 sprite 교체 방식(idleSprite/attackSprite) 으로 fallback.
        string animatorPath = null;
        string attack1Name = null, attack2Name = null;
        if (factionType == FactionType.Ally && i < allies.Count)
        {
            animatorPath = allies[i].animatorPath;
            attack1Name  = allies[i].attack1Anim;
            attack2Name  = allies[i].attack2Anim;
        }
        else if (factionType == FactionType.Enemy && i < enemies.Count)
        {
            animatorPath = enemies[i].animatorPath;
            attack1Name  = enemies[i].attack1Anim;
            attack2Name  = enemies[i].attack2Anim;
        }

        if (!string.IsNullOrEmpty(animatorPath))
        {
            var ctrl = Resources.Load<RuntimeAnimatorController>(animatorPath);
            if (ctrl != null)
            {
                var animator = newObj.GetComponentInChildren<Animator>();
                if (animator != null)
                {
                    animator.runtimeAnimatorController = ctrl;
                    if (sprites != null) sprites.AttachAnimator(animator, attack1Name, attack2Name);
                }
                else Debug.LogWarning($"[DefaultSetting] {newObj.name}: Animator 컴포넌트 없음 — prefab 에 추가 필요");
            }
            else Debug.LogWarning($"[DefaultSetting] AnimatorController 로드 실패: {animatorPath}");
        }
    }

    // ============================================================
    // 사망 시 카드 재정렬
    //   기획: 사망 시 빈 슬롯은 중앙(센터) 쪽에 모이고, 생존 카드는 바깥쪽으로 압축.
    //   예: 아군 4명 [3 2 1 0] 배치에서 ally[3](가장 왼쪽) 사망 → 생존 3명이 한 칸씩 바깥(왼쪽)으로 밀려
    //        [A2 A1 A0 _] 로 표시. gap 은 항상 i=0 (센터에 가장 가까운 인덱스).
    // ============================================================
    // 사망 연출(BattleCardSprites.PlayDeathFall) 시간과 동일 — 연출 끝난 후 재배치.
    private const float DeathRelayoutDelay = 0.5f;

    private void SubscribeRelayoutOnDeath(FellowData fellow)
    {
        if (fellow == null) return;
        System.Action handler = () => DOVirtual.DelayedCall(DeathRelayoutDelay, () => RelayoutCards());
        fellow.OnDied += handler;
        _diedUnsubscribers.Add(() => { if (fellow != null) fellow.OnDied -= handler; });
    }

    private void SubscribeRelayoutOnDeath(EnemyData enemy)
    {
        if (enemy == null) return;
        System.Action handler = () => DOVirtual.DelayedCall(DeathRelayoutDelay, () => RelayoutCards());
        enemy.OnDied += handler;
        _diedUnsubscribers.Add(() => { if (enemy != null) enemy.OnDied -= handler; });
    }

    private const float RelayoutDuration = 0.25f;

    /// <summary>
    /// 생존 카드만 새 위치로 이동.
    ///   - 아군: 원본 인덱스 오름차순 → 바깥쪽(왼쪽) 부터 채움, gap 은 센터.
    ///   - 적군: 비-보스 먼저 (앞열) → 보스(EnemyTier.Boss) 가장 뒤(오른쪽).
    /// instant=true 면 트윈 없이 즉시 스냅 (전투 시작 시).
    /// </summary>
    private void RelayoutCards(bool instant = false)
    {
        if (BattleManager.Instance == null) return;

        int dir = factionType == FactionType.Ally ? -1 : 1;
        var cards = GetComponentsInChildren<BattleCardView>(true);

        List<(BattleCardView card, int sortKey)> alive = new();

        if (factionType == FactionType.Ally)
        {
            // sortKey = fellow.battleSlotIndex — 사망/노드 진행 후에도 슬롯 위치 영구 보존.
            //   PartyManager.GetActiveFellows 가 _activeFellows 의 인덱스를 fellow.battleSlotIndex 에 stamp.
            //   ally 배열 인덱스 (allies.IndexOf) 를 쓰면 사망 후 인덱스 압축되어 전열/후열 분류가 깨짐.
            foreach (var c in cards)
            {
                if (c.Fellow == null || c.Fellow.isDead) continue;
                int slotIdx = c.Fellow.battleSlotIndex;
                if (slotIdx < 0) continue;
                alive.Add((c, slotIdx));
            }
            alive = alive.OrderBy(t => t.sortKey).ToList();
        }
        else
        {
            var enemies = BattleManager.Instance.enemies;
            // 비-보스 먼저(앞열) → 보스 뒤(후열). 비-보스끼리는 원본 인덱스 순.
            var temp = new List<(BattleCardView card, int origIdx, bool isBoss)>();
            foreach (var c in cards)
            {
                if (c.Enemy == null || c.Enemy.isDead) continue;
                int idx = enemies.IndexOf(c.Enemy);
                if (idx < 0) continue;
                temp.Add((c, idx, c.Enemy.tier == EnemyTier.Boss));
            }
            temp = temp.OrderBy(t => t.isBoss ? 1 : 0).ThenBy(t => t.origIdx).ToList();
            foreach (var t in temp) alive.Add((t.card, t.origIdx));
        }

        int total = ObjectCount > 0 ? ObjectCount : alive.Count;

        // 기획 §02 §피격 순서 = 배치 순서 — 아군/적군 모두 0~3 단일 순번, 행 구분 없음.
        //   alive 는 이미 정렬됨 (아군: sortKey=battleSlotIndex 오름차순 / 적군: 비-보스 먼저).
        //   사망 자리는 뒤에서 앞으로 압축 → 빈 자리는 항상 뒷쪽(3 → 2 → 1).
        //   EnemySkillExecutor FrontFirst(alive[0]) / BackLast(alive.Last) 일관성 유지.
        int newStart = total - alive.Count;
        for (int k = 0; k < alive.Count; k++)
            PlaceCardAt(alive[k].card, newStart + k, dir, instant);
    }

    private void PlaceCardAt(BattleCardView card, int newIndex, int dir, bool instant)
    {
        float currentX  = startX + dir * (spacingX * newIndex);
        Vector3 targetPos = transform.position + new Vector3(currentX, spawnOffsetY, 0f);
        var go = card.gameObject;
        go.transform.DOKill();
        if (instant) go.transform.position = targetPos;
        else         go.transform.DOMove(targetPos, RelayoutDuration).SetEase(Ease.OutQuad);
    }

    // ----------------------------------------------------------
    // 소속에 따라 오브젝트 이미지 적용
    // ----------------------------------------------------------

    /// <summary>
    /// 카드의 root SpriteRenderer 에 sprite 를 적용한다.
    /// 아군: fellowSprite 가 있으면 정적 표시. Animator + .anim 가 있으면 Idle 첫 프레임이 곧 덮어씀.
    /// 적: enemySprite 정적 표시 (현재 적은 Animator controller 없음).
    /// </summary>
    void ApplyObjectSprite(int index, SpriteRenderer sr, string objName)
    {
        if (factionType == FactionType.Ally)
        {
            var allies = BattleManager.Instance.allies;
            if (index < allies.Count && allies[index] != null && allies[index].fellowSprite != null)
                sr.sprite = allies[index].fellowSprite;
        }
        else if (factionType == FactionType.Enemy)
        {
            var enemies = BattleManager.Instance.enemies;
            if (index < enemies.Count && enemies[index].enemySprite != null)
                sr.sprite = enemies[index].enemySprite;
        }
    }
}