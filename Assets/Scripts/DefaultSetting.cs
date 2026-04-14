using UnityEngine;
public enum FactionType { Ally, Enemy }
public class DefaultSetting : MonoBehaviour
{
    [Header("소속 설정")]
    [Tooltip("이 생성기가 아군을 만들지, 적군을 만들지 선택하세요.")]
    public FactionType factionType = FactionType.Ally;
    
    [Header("생성 설정")]
    public GameObject cardPrefab;     // 생성할 카드 프리팹 (인스펙터에서 할당)
    public int cardCount = 4;         // 생성할 카드 개수
    
    [Header("위치 설정")]
    public float startX = -0.3f;      // 첫 카드가 생성될 X 좌표 시작점
    public float spacingX = 0.15f;    // 카드와 카드 사이의 X축 간격

    void Start()
    {
        // 자신의 카드만 1번 반복해서 생성하도록 수정
        SpawnCard();
    }

    void SpawnCard()
    {
        for (int i = 0; i < cardCount; i++)
        {
            var allies = BattleManager.Instance.allies;
            // 지정한 startX부터 시작해서 spacingX만큼 X값을 더해줌
            float currentX = startX + (spacingX * i);
            
            // 현재 오브젝트의 위치를 기준으로 계산된 X값을 더해 새로운 위치 지정
            Vector3 newPosition = transform.position + new Vector3(currentX, 0f, 0f);
            
            // 오브젝트 복제 (Instantiate)
            GameObject newObj = Instantiate(cardPrefab, newPosition, Quaternion.identity);
            
            // 계층 구조 정리
            newObj.transform.parent = this.transform;
            newObj.name = cardPrefab.name + "_" + i;
            
            // --- 추가된 로직: 생성된 카드에 에셋 이미지 씌우기 ---
            // 3D 오브젝트 메테리얼 텍스처 적용 로직
            MeshRenderer renderer = newObj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                ApplyCardImage(i, renderer, newObj.name);
            }
        }
    }
    
    // 소속에 따라 데이터를 다르게 가져와서 이미지를 입히는 함수
    void ApplyCardImage(int index, MeshRenderer renderer, string objName)
    {
        if (factionType == FactionType.Ally)
        {
            var allies = BattleManager.Instance.allies;
            if (index < allies.Count && allies[index] != null && allies[index].fellowSprite != null)
            {
                renderer.material.mainTexture = allies[index].fellowSprite.texture;
                Debug.Log($"[아군 세팅 성공] {objName}에 이미지 할당됨.");
            }
        }
        else if (factionType == FactionType.Enemy)
        {
            var enemies = BattleManager.Instance.enemies;
            if (index < enemies.Count && enemies[index].baseData != null && enemies[index].baseData.cardArt != null)
            {
                renderer.material.mainTexture = enemies[index].baseData.cardArt.texture;
                Debug.Log($"[적군 세팅 성공] {objName}에 이미지 할당됨.");
            }
        }
    }
}