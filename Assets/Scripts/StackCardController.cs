// ============================================================
// StackCardController.cs
// 스택 카드 UI 컨트롤러
// ============================================================
//
// [이 파일이 하는 일]
//   플레이어가 손패에서 카드를 클릭하면 확인/취소 팝업을 보여주고,
//   확인 시 GameManager 에 "카드 사용됨" 을 알립니다.
//   GameManager → PlayerRoleCost 에 스택이 반영됩니다.
//
// [카드 클릭 흐름]
//   1. 카드 클릭 → 카드 크기 확대 + 확인/취소 팝업 표시
//   2. 확인 버튼 → 카드 비활성화 + GameManager.OnCardUsed() 호출
//   3. 취소 버튼 → 원래 크기로 복구 + 팝업 숨김
//
// [어디서 쓰이나요?]
//   - GameManager.myCards[] 슬롯에 할당되어 있음
//   - GameManager.Instance.OnCardUsed(this) 를 내부에서 호출
//
// [인스펙터 설정]
//   - numberText : 카드 중앙 숫자 텍스트 (+3, -2 등)
//   - roleText   : 카드 역할 텍스트 (딜/탱/힐)
//   - descText   : 카드 설명 텍스트
//   - stackType  : 이 카드의 역할 타입 (Dealer/Tank/Support)
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 개별 스택 카드의 UI 및 클릭 처리 컨트롤러.
/// </summary>
public class StackCardController : MonoBehaviour
{
    // ----------------------------------------------------------
    // [UI 참조] — Inspector 에서 연결하세요.
    // ----------------------------------------------------------
    [Header("UI 참조 (Inspector 에서 연결)")]
    [Tooltip("카드 중앙에 표시될 숫자 텍스트 (+3, -2 등)")]
    public TextMeshProUGUI numberText;

    [Tooltip("카드 상단 역할 뱃지 텍스트 (딜/탱/힐)")]
    public TextMeshProUGUI roleText;

    [Tooltip("카드 하단 설명 텍스트")]
    public TextMeshProUGUI descText;

    // ----------------------------------------------------------
    // [카드 데이터]
    // SetupCard() 에서 설정됩니다.
    // ----------------------------------------------------------
    [Header("카드 데이터 (런타임에 자동 설정됨)")]
    [Tooltip("이 카드가 기여하는 역할 스택 타입")]
    public StackType stackType;

    [Tooltip("스택 기여량 (양수=증가, 음수=감소)")]
    public int stackDelta;

    [Tooltip("이 카드의 소유 동료 (성향 계산용)")]
    public CompanionData owner;

    // ----------------------------------------------------------
    // [내부 상태]
    // ----------------------------------------------------------

    /// <summary>카드 클릭 전 원래 크기 (클릭 시 확대, 취소 시 복구)</summary>
    private Vector3 originalScale;

    /// <summary>사용한 카드(빈 슬롯)에 표시할 스프라이트</summary>
    private Sprite emptySprite;

    /// <summary>현재 카드에 표시된 숫자 값</summary>
    public int currentNumber { get; private set; }

    /// <summary>카드가 이미 사용되었는지 여부</summary>
    public bool isUsed { get; private set; }

    private Image  myImage;
    private Button myButton;

    // ----------------------------------------------------------
    // 초기화
    // ----------------------------------------------------------
    void Awake()
    {
        myImage       = GetComponent<Image>();
        myButton      = GetComponent<Button>();
        originalScale = transform.localScale;
    }

    void Start()
    {
        // 버튼 클릭 시 OnCardClicked 연결
        myButton.onClick.AddListener(OnCardClicked);
    }

    // ----------------------------------------------------------
    // 카드 세팅 (GameManager.StartMyTurn() 에서 호출)
    // ----------------------------------------------------------

    /// <summary>
    /// 턴 시작 시 카드를 초기화한다.
    /// 숫자 값, 소유자, 역할 타입, UI 텍스트를 설정한다.
    /// </summary>
    /// <param name="number">스택 기여량 (+3, -2 등)</param>
    /// <param name="cardOwner">이 카드의 소유 동료</param>
    public void SetupCard(int number, CompanionData cardOwner)
    {
        owner         = cardOwner;
        currentNumber = number;
        stackDelta    = number;

        // 소유자 역할 기반으로 스택 타입 설정
        stackType = (StackType)(int)cardOwner.role;

        // 숫자 텍스트 업데이트
        if (numberText != null)
        {
            numberText.text  = number > 0 ? $"+{number}" : $"{number}";
            // 양수=초록, 음수=빨강
            numberText.color = number > 0
                ? new Color(0.2f, 0.85f, 0.3f)
                : new Color(0.9f, 0.2f, 0.2f);
        }

        // 역할 뱃지 텍스트 업데이트
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

        // 설명 텍스트 업데이트
        if (descText != null)
            descText.text = number > 0 ? "스택 증가" : "스택 감소";

        // 카드 상태 초기화
        isUsed                = false;
        myButton.interactable = true;
        myImage.color = Color.gray1;
        myImage.sprite        = null;
    }

    // ----------------------------------------------------------
    // 카드 클릭 처리
    // ----------------------------------------------------------

    /// <summary>카드를 클릭했을 때 실행된다.</summary>
    void OnCardClicked()
    {
        // 이미 사용했거나 다른 카드의 확인 팝업이 열려있으면 무시
        if (isUsed || GameManager.Instance.checkButtonBox.activeSelf) return;

        // 1. 카드를 약간 크게 만들어 "선택됨" 을 표시
        transform.localScale = originalScale * 1.15f;

        // 2. 확인/취소 팝업 표시 + 위치 이동
        GameManager.Instance.checkButtonBox.SetActive(true);
        GameManager.Instance.checkButtonBox.transform.position =
            transform.position + new Vector3(0, 100f, 0);

        // 3. 이 카드의 확인/취소 버튼 이벤트로 교체
        GameManager.Instance.btnConfirm.onClick.RemoveAllListeners();
        GameManager.Instance.btnCancel.onClick.RemoveAllListeners();

        GameManager.Instance.btnConfirm.onClick.AddListener(HandleConfirm);
        GameManager.Instance.btnCancel.onClick.AddListener(HandleCancel);
    }

    /// <summary>취소 버튼 클릭 시: 카드 크기 복구 + 팝업 닫기</summary>
    void HandleCancel()
    {
        transform.localScale = originalScale;
        GameManager.Instance.checkButtonBox.SetActive(false);
    }

    /// <summary>
    /// 확인 버튼 클릭 시:
    /// 카드를 사용 상태로 만들고 GameManager.OnCardUsed() 를 호출한다.
    /// </summary>
    void HandleConfirm()
    {
        // 카드를 "사용됨" 상태로 전환 (클릭 불가 + 반투명)
        isUsed                = true;
        myImage.sprite        = emptySprite;
        myButton.interactable = false;
        myImage.color         = new Color(1, 1, 1, 0.5f);

        // UI 정리
        transform.localScale = originalScale;
        GameManager.Instance.checkButtonBox.SetActive(false);

        // GameManager 에 카드 사용 알림 → PlayerRoleCost 스택 반영
        GameManager.Instance.OnCardUsed(this);
    }
}
