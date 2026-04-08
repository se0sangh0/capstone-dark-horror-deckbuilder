using UnityEngine;
using UnityEngine.UI;

public class StackCardController : MonoBehaviour
{
    private Vector3 originalScale; // 원래 크기 저장
    private Sprite emptySprite;    // 빈 카드(사용한 카드) 이미지 저장
    
    public int currentNumber { get; private set; } // 현재 지정된 숫자
    public bool isUsed { get; private set; }       // 카드 사용 여부

    private Image myImage;
    private Button myButton;
    
    // StackCardController.cs 내부 예시
    public StackType stackType; // Inspector에서 Dealer, Tank, Support 중 선택
    public int stackDelta;      // 카드에 적힌 숫자 (+3, -2 등)

    void Awake()
    {
        myImage = GetComponent<Image>();
        myButton = GetComponent<Button>();
        originalScale = transform.localScale;
    }

    void Start()
    {
        // 버튼 클릭 시 OnCardClicked 함수 실행
        myButton.onClick.AddListener(OnCardClicked);
    }

    // GameManager가 턴 시작 시 호출하여 카드를 세팅하는 함수
    public void SetupCard(int number, Sprite numberSprite, Sprite emptySpr)
    {
        currentNumber = number;
        emptySprite = emptySpr;
        myImage.sprite = numberSprite; // 해당 숫자 이미지로 변경
        
        isUsed = false;
        myButton.interactable = true; // 버튼 클릭 활성화
        myImage.color = Color.white;  // 색상 원래대로
    }

    // 카드 클릭 시 실행
    void OnCardClicked()
    {
        // 이미 사용했거나, CheckButtonBox가 켜져 있으면 작동 안 함
        if (isUsed || GameManager.Instance.checkButtonBox.activeSelf) return; 

        // 1. 카드 크기 살짝 키우기
        transform.localScale = originalScale * 1.15f;

        // 2. CheckButtonBox 켜기 및 위치 조정
        GameManager.Instance.checkButtonBox.SetActive(true);
        // 필요 시 위치를 카드 위로 조정. (Y값 100f는 해상도에 따라 조절하세요)
        GameManager.Instance.checkButtonBox.transform.position = transform.position + new Vector3(0, 100f, 0); 

        // 3. 버튼 이벤트 덮어씌우기 (현재 카드의 확인/취소로 작동하게 만듦)
        GameManager.Instance.btnConfirm.onClick.RemoveAllListeners();
        GameManager.Instance.btnCancel.onClick.RemoveAllListeners();

        GameManager.Instance.btnConfirm.onClick.AddListener(HandleConfirm);
        GameManager.Instance.btnCancel.onClick.AddListener(HandleCancel);
    }

    // 취소 버튼 눌렀을 때
    void HandleCancel()
    {
        transform.localScale = originalScale; // 크기 복구
        GameManager.Instance.checkButtonBox.SetActive(false);      // 팝업 끄기
    }

    // 확인 버튼 눌렀을 때
    void HandleConfirm()
    {
        // 1. 카드 초기화 (빈 이미지로 만들고 클릭 불가 상태로)
        isUsed = true;
        myImage.sprite = emptySprite;
        myButton.interactable = false;
        // 약간 어둡거나 투명하게 하려면 아래 코드 사용 (원치 않으면 삭제 가능)
        myImage.color = new Color(1, 1, 1, 0.5f); 

        // 2. UI 정리
        transform.localScale = originalScale;
        GameManager.Instance.checkButtonBox.SetActive(false);

        // 3. GameManager에 카드 사용했다고 알림
        GameManager.Instance.OnCardUsed(this);
    }
}
