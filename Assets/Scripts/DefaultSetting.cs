using UnityEngine;

public class DefaultSetting : MonoBehaviour
{
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
            // 지정한 startX부터 시작해서 spacingX만큼 X값을 더해줌
            float currentX = startX + (spacingX * i);
            
            // 현재 오브젝트의 위치를 기준으로 계산된 X값을 더해 새로운 위치 지정
            Vector3 newPosition = transform.position + new Vector3(currentX, 0f, 0f);
            
            // 오브젝트 복제 (Instantiate)
            GameObject newObj = Instantiate(cardPrefab, newPosition, Quaternion.identity);
            
            // 계층 구조 정리
            newObj.transform.parent = this.transform;
            newObj.name = cardPrefab.name + "_" + i;
        }
    }
}