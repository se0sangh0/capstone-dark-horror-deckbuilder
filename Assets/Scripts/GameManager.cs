// GameManager.cs
// UI 입력 수신 및 드로우 페이즈 카드 세팅.
// 스택 관리 권한은 BattleManager 에 있음.

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
    [Tooltip("숫자 스프라이트 배열. 인덱스 순서: -5~-1, +1~+5 (총 10장)")]
    public Sprite[] numberSprites;
    public Sprite   emptyCardSprite;

    [Header("UI 연결 (UI References)")]
    public StackCardController[] myCards;
    public GameObject checkButtonBox;
    public Button     btnConfirm;
    public Button     btnCancel;

    // -------------------------------------------------------
    // 스택 상태 → BattleManager 가 관리. 여기선 제거.
    // (구 필드: dealerStack / tankStack / supportStack)
    // -------------------------------------------------------

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    void Start()
    {
        StartMyTurn();
    }

    // -------------------------------------------------------
    // 드로우 페이즈 — 카드 세팅
    // 성향(CardAffinity) 기반 stackDelta 생성.
    // AffinityHelper.GenerateStack() 이용 (CardAffinity.cs 참조)
    // -------------------------------------------------------
    public void StartMyTurn()
    {
        Debug.Log("내 턴 시작! 카드를 세팅합니다.");

        foreach (StackCardController card in myCards)
        {
            // 성향 기반 스택값 생성 (성향이 None이면 -5~+5 균등, 0 제외)
            int stackValue = GenerateStackValue(card.affinity);

            Sprite sprite = GetSpriteForValue(stackValue);
            card.SetupCard(stackValue, sprite, emptyCardSprite);
        }
    }

    // -------------------------------------------------------
    // 카드 확정 시 → BattleManager 에 스택 반영
    // -------------------------------------------------------
    public void OnCardUsed(StackCardController usedCard)
    {
        Debug.Log($"[카드 사용] type={usedCard.stackType} delta={usedCard.stackDelta:+#;-#;0}");

        // 스택 관리 권한: BattleManager
        if (BattleManager.Instance != null)
            BattleManager.Instance.AddStack(usedCard.stackType, usedCard.stackDelta);
    }

    // -------------------------------------------------------
    // 내 턴 종료 (레거시 — BattleManager.FinishPlayerTurn 권장)
    // -------------------------------------------------------
    public void EndMyTurn()
    {
        Debug.Log("내 턴 종료.");
        BattleManager.Instance?.FinishPlayerTurn();
    }

    // -------------------------------------------------------
    // 내부 유틸리티
    // -------------------------------------------------------

    /// <summary>
    /// 성향 규칙에 따라 0을 제외한 stackDelta 값을 생성한다.
    /// AffinityHelper.GenerateStack() 이 0을 반환할 경우(None 성향 등) 재시도.
    /// </summary>
    private int GenerateStackValue(CardAffinity affinity)
    {
        // None 성향이면 낙천가 범위(-5~+5)와 동일하게 취급
        if (affinity == CardAffinity.None)
            affinity = CardAffinity.Optimist;

        int value;
        int maxRetry = 20;
        do
        {
            value = AffinityHelper.GenerateStack(affinity);
            maxRetry--;
        }
        while (value == 0 && maxRetry > 0);

        return value;
    }

    /// <summary>
    /// stackValue (-5~-1, +1~+5) 에 대응하는 스프라이트 반환.
    /// 배열 순서: index 0=-5, 1=-4, 2=-3, 3=-2, 4=-1, 5=+1, 6=+2, 7=+3, 8=+4, 9=+5
    /// </summary>
    private Sprite GetSpriteForValue(int value)
    {
        if (numberSprites == null || numberSprites.Length == 0) return null;

        // value: -5→0, -4→1, -3→2, -2→3, -1→4, +1→5, +2→6, +3→7, +4→8, +5→9
        int index = value > 0 ? value + 4 : value + 5;
        index = Mathf.Clamp(index, 0, numberSprites.Length - 1);
        return numberSprites[index];
    }
}
