using UnityEngine;
using UnityEngine.UI;
public enum StackType 
{ 
    Dealer, // 딜 (딜러)
    Tank,   // 탱 (탱커)
    Support // 힐 (서포터)
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("에셋 설정 (Assets)")]
    // Inspector에서 숫자 1~10 이미지 에셋을 순서대로 넣을 배열
    public Sprite[] numberSprites; 
    public Sprite emptyCardSprite; // 사용 후 보여줄 빈 카드 이미지

    [Header("UI 연결 (UI References)")]
    public StackCardController[] myCards; // CardArea 아래의 4개 카드
    public GameObject checkButtonBox; // Hierarchy의 CheckButtonBox 오브젝트
    public Button btnConfirm; // CheckButtonBox 하위의 확인 버튼
    public Button btnCancel;  // CheckButtonBox 하위의 취소 버튼
    
    [Header("플레이어 스택 상태 (Player Stacks)")]
    // 2. 각 직업별 스택을 누적해서 저장할 변수
    public int dealerStack = 0;
    public int tankStack = 0;
    public int supportStack = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 필요시 씬 전환 후에도 유지하려면 주석 해제
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 게임 시작 시 내 턴 시작
        StartMyTurn();
    }

    // 내 턴 시작 (카드 랜덤 지정)
    public void StartMyTurn()
    {
        Debug.Log("내 턴 시작! 카드를 세팅합니다.");

        foreach (StackCardController card in myCards)
        {
            // 1부터 등록된 이미지 개수 사이에서 랜덤 숫자 
            // 성향별 if 문 구현
            //ToDo
            /*
            // 1. 상태를 enum으로 정의합니다. (플레이어는 이 중 딱 하나의 성향만 가질 수 있습니다)
            public enum PlayerPersonality { Gambler, Safety, Opportunist, Optimist }
            public PlayerPersonality personality; // 인스펙터 창에서 선택 가능

            // 2. 난수 생성 함수 내부
            switch (personality)
            {
                case PlayerPersonality.Gambler: // 도박사
                    randomNum = Random.Range(0, 2) == 0 ? -5 : 5;
                    break;

                case PlayerPersonality.Safety:        min = -1; max = 3; break; // 안전주의자
                case PlayerPersonality.Opportunist: min = -3; max = 4; break; // 기회주의자
                case PlayerPersonality.Optimist:    min = -5; max = 5; break; // 낙천가
            }

            // 도박사가 아닐 때만 0을 제외한 랜덤값을 뽑습니다.
            if (personality != PlayerPersonality.Gambler)
            {
                do {
                    randomNum = Random.Range(min, max);
                } while (randomNum == 0);
            }
             */
            int randomNum;
            do
            {
                randomNum = Random.Range(-5, 6);
            } while (randomNum==0);
            Debug.Log("랜덤숫자: " + randomNum);
            // Sprite 배열은 0부터 시작하므로 randomNum - 1 을 해줍니다.
            // if (randomNum > 0) card.SetupCard(randomNum, numberSprites[randomNum+4], emptyCardSprite);
            // else card.SetupCard(randomNum, numberSprites[randomNum+5], emptyCardSprite);
            
            // 3항 연산자로 코드 길이 줄임.
            card.SetupCard(randomNum, numberSprites[randomNum+(randomNum > 0 ? 4 : 5)], emptyCardSprite);
        }
    }

    // 카드를 최종 선택(확인) 했을 때
    public void OnCardUsed(StackCardController usedCard)
    {
        Debug.Log("선택한 카드 숫자: " + usedCard.currentNumber);
        // 카드의 stackType에 따라 알맞은 스택 변수에 stackDelta를 더해줍니다.
        switch (usedCard.stackType)
        {
            case StackType.Dealer:
                dealerStack += usedCard.stackDelta;
                Debug.Log($"딜러 스택 갱신! 현재 딜러 스택: {dealerStack}");
                break;
                
            case StackType.Tank:
                tankStack += usedCard.stackDelta;
                Debug.Log($"탱커 스택 갱신! 현재 탱커 스택: {tankStack}");
                break;
                
            case StackType.Support:
                supportStack += usedCard.stackDelta;
                Debug.Log($"서포터 스택 갱신! 현재 서포터 스택: {supportStack}");
                break;
        }
    }
    
}