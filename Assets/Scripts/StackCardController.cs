using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용 시

public class StackCardController : MonoBehaviour
{
    // ✅ 추가 — Inspector에서 연결
    [Header("UI 참조")]
    public TextMeshProUGUI numberText;   // 카드 중앙 숫자 (+3, -2 등)
    public TextMeshProUGUI roleText;     // 상단 역할 뱃지 (딜/탱/힐)
    public TextMeshProUGUI descText;     // 하단 설명 텍스트
    
    private Vector3 originalScale; // 원래 크기 저장
    private Sprite emptySprite;    // 빈 카드(사용한 카드) 이미지 저장
    
    public int currentNumber { get; private set; } // 현재 지정된 숫자
    public bool isUsed { get; private set; }       // 카드 사용 여부

    private Image myImage;
    private Button myButton;
    
    public StackType   stackType; // Inspector에서 Dealer, Tank, Support 중 선택
    public int         stackDelta; // 카드에 적힌 숫자 (+3, -2 등)
    public CompanionData owner;    // 이 카드의 소유 동료 (성향 계산용)

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

    public void SetupCard(int number, CompanionData cardOwner)
    {
        owner = cardOwner;
        currentNumber = number;
        stackDelta = number;
        stackType = (StackType)(int)cardOwner.role;

        // ✅ 런타임 텍스트 렌더링
        if (numberText != null)
        {
            numberText.text = number > 0 ? $"+{number}" : $"{number}";
            // 양수 초록, 음수 빨강
            numberText.color = number > 0
                ? new Color(0.2f, 0.85f, 0.3f)
                : new Color(0.9f, 0.2f, 0.2f);
        }

        if (roleText != null)
        {
            roleText.text = stackType switch
            {
                StackType.Dealer  => "딜",
                StackType.Tank    => "탱",
                StackType.Support => "힐",
                _ => "?"
            };
        }

        if (descText != null)
            descText.text = number > 0 ? "스택 증가" : "스택 감소";

        isUsed = false;
        myButton.interactable = true;
        myImage.color = Color.white;
        myImage.sprite = null; // 스프라이트 비워도 됨
    }
    
    // GameManager가 턴 시작 시 호출하여 카드를 세팅하는 함수
    /*public void SetupCard(int number, Sprite numberSprite, Sprite emptySpr, CompanionData cardOwner)
    {
        owner = cardOwner;
        currentNumber = number;
        stackDelta = number;
        emptySprite = emptySpr;
        myImage.sprite = numberSprite;

        // ✅ 이 줄 추가 — owner 역할 기반으로 stackType 세팅
        stackType = (StackType)(int)cardOwner.role;

        isUsed = false;
        myButton.interactable = true;
        myImage.color = Color.white;
    }*/

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
