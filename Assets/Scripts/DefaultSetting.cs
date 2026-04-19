// ============================================================
// DefaultSetting.cs
// 전투 카드 오브젝트 생성 및 동료 이미지/HP 슬라이더 초기 설정
// ============================================================
//
// [이 파일이 하는 일]
//   씬이 시작될 때 카드 프리팹을 지정된 개수만큼 생성하고,
//   생성된 오브젝트에 동료 이미지와 HP 슬라이더를 연결합니다.
//
//   아군(Ally) 카드면 BattleManager.allies 목록에서
//   동료 스프라이트와 HP 슬라이더를 가져옵니다.
//
// [중요: 동적 참조]
//   이 스크립트는 BattleManager.Instance.allies 를 직접 참조합니다.
//   BattleManager 가 먼저 Start() 에서 allies 를 초기화하므로
//   이 스크립트의 Start() 보다 BattleManager.Start() 가 먼저 실행되어야 합니다.
//
// [어디서 쓰이나요?]
//   - 전투 씬에서 아군/적군 카드 오브젝트를 생성하는 오브젝트에 붙임
//
// [인스펙터 설정]
//   - factionType : Ally(아군) 또는 Enemy(적군) 선택
//   - cardPrefab  : 생성할 카드 프리팹 연결
//   - cardCount   : 생성할 카드 개수
//   - startX      : 첫 카드 X 시작 좌표
//   - spacingX    : 카드 간격
// ============================================================

using UnityEngine;

// ----------------------------------------------------------
// [FactionType 열거형]
// 이 오브젝트가 아군 카드를 생성할지, 적군 카드를 생성할지 선택
// ----------------------------------------------------------
/// <summary>소속 팀 구분 (아군 / 적군)</summary>
public enum FactionType { Ally, Enemy }

/// <summary>
/// 전투 씬 시작 시 카드 오브젝트를 생성하고
/// 동료 이미지/HP 슬라이더를 연결하는 초기 설정 컴포넌트.
/// </summary>
public class DefaultSetting : MonoBehaviour
{
    // ----------------------------------------------------------
    // [소속 설정]
    // ----------------------------------------------------------
    [Header("소속 설정")]
    [Tooltip("이 오브젝트가 아군 카드를 생성할지, 적군 카드를 생성할지 선택하세요.")]
    public FactionType factionType = FactionType.Ally;

    // ----------------------------------------------------------
    // [생성 설정]
    // ----------------------------------------------------------
    [Header("생성 설정")]
    [Tooltip("생성할 카드 프리팹 (Inspector 에서 연결하세요)")]
    public GameObject cardPrefab;

    [Tooltip("생성할 카드 개수")]
    public int cardCount;

    // ----------------------------------------------------------
    // [위치 설정]
    // ----------------------------------------------------------
    [Header("위치 설정")]
    [Tooltip("첫 번째 카드가 생성될 X 좌표 시작점 (이 오브젝트 위치 기준)")]
    public float startX = -0.3f;

    [Tooltip("카드와 카드 사이 X 축 간격")]
    public float spacingX = 0.15f;

    // ----------------------------------------------------------
    // Start — 씬 시작 시 카드 생성
    // ----------------------------------------------------------
    void Start()
    {
        SpawnCard();
    }

    // ----------------------------------------------------------
    // 카드 생성 + 이미지/슬라이더 연결
    // ----------------------------------------------------------

    /// <summary>
    /// cardCount 만큼 카드 프리팹을 생성하고
    /// 각 카드에 동료 이미지와 HP 슬라이더를 연결한다.
    /// </summary>
    void SpawnCard()
    {
        // BattleManager 의 allies 목록을 가져옴 (동적 참조)
        var allies = BattleManager.Instance.allies;
        var enemies = BattleManager.Instance.enemies;

        for (int i = 0; i < cardCount; i++)
        {
            // startX 부터 spacingX 간격으로 X 위치 계산
            // 중앙을 기준으로 아군은 오른쪽에서 왼쪽으로 배치, 적군은 왼쪽에서 오른쪽으로 배치
            float currentX   = startX + (factionType == FactionType.Ally ? -1 : 1) * (spacingX * i);
            Vector3 newPos   = transform.position + new Vector3(currentX, 0f, 0f);

            // 카드 프리팹 생성
            GameObject newObj = Instantiate(cardPrefab, newPos, Quaternion.identity);
            newObj.transform.parent = this.transform;
            newObj.name = cardPrefab.name + "_" + i;

            // 3D 오브젝트라면 MeshRenderer 에 이미지 텍스처 적용
            MeshRenderer renderer = newObj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                ApplyCardImage(i, renderer, newObj.name);
            }

            // 아군 카드라면 HP 슬라이더를 FellowData 에 연결
            if (factionType == FactionType.Ally && i < allies.Count)
            {
                var slider = newObj.GetComponentInChildren<UnityEngine.UI.Slider>();
                if (slider != null)
                {
                    // FellowData.InitHp() 를 호출하면:
                    // - HpSlider 연결
                    // - OnHpChanged 이벤트로 자동 UI 갱신
                    // - 사망 시 OnDied 이벤트 발생
                    allies[i].InitHp(slider);
                    slider.maxValue = allies[i].data != null ? allies[i].data.maxHp : 100;
                    slider.value    = allies[i].CurrentHp;
                }
                else
                {
                    Debug.LogWarning($"[DefaultSetting] {newObj.name} 에서 Slider 를 찾지 못했습니다.");
                }
            }
            // ── 적군 HP 슬라이더 연결 ────────────────────────────
            else if (factionType == FactionType.Enemy && i < enemies.Count)
            {
                var slider = newObj.GetComponentInChildren<UnityEngine.UI.Slider>();
                if (slider != null)
                {
                    // EnemyData.InitHp() 를 호출하면:
                    // - HpSlider 연결
                    // - OnHpChanged 이벤트로 자동 UI 갱신
                    // - 사망 시 OnDied 이벤트 발생
                    enemies[i].InitHp(slider);
                    slider.maxValue = enemies[i].maxHp;
                    slider.value    = enemies[i].CurrentHp;
                    Debug.Log($"[DefaultSetting] {newObj.name} hp 동기화");
                }
                else
                {
                    Debug.LogWarning($"[DefaultSetting] {newObj.name} 에서 Slider 를 찾지 못했습니다.");
                }
            }
        }
    }

    // ----------------------------------------------------------
    // 소속에 따라 카드 이미지 적용
    // ----------------------------------------------------------

    /// <summary>
    /// 소속(factionType)에 따라 적절한 이미지를 카드에 적용한다.
    /// </summary>
    void ApplyCardImage(int index, MeshRenderer renderer, string objName)
    {
        if (factionType == FactionType.Ally)
        {
            var allies = BattleManager.Instance.allies;
            if (index < allies.Count && allies[index] != null && allies[index].fellowSprite != null)
            {
                renderer.material.mainTexture = allies[index].fellowSprite.texture;
                Debug.Log($"[DefaultSetting] 아군 이미지 적용 성공: {objName}");
            }
        }
        else if (factionType == FactionType.Enemy)
        {
            // TODO: 적 이미지 적용 로직 (EnemyEntity 에 스프라이트 추가 후 구현)
            var enemies = BattleManager.Instance.enemies;
            if (index < enemies.Count && enemies[index].enemySprite != null)
                renderer.material.mainTexture = enemies[index].enemySprite.texture;
        }
    }
}
