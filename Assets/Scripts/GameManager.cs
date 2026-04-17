// ============================================================
// GameManager.cs
// UI 입력 수신 및 드로우 페이즈 카드 세팅 매니저
// ============================================================
//
// [이 파일이 하는 일]
//   - 플레이어가 사용할 카드 슬롯을 화면에 세팅합니다.
//   - 카드가 사용되면 BattleManager 에 스택 반영을 요청합니다.
//   - 게임 내 덱(카드 뭉치)을 보관하고 관리합니다.
//
// [스택 관리 권한]
//   스택 값 자체는 BattleManager / PlayerRoleCost 가 관리합니다.
//   GameManager 는 카드 선택 이벤트를 중계하는 역할만 합니다.
//
// [어디서 쓰이나요?]
//   - BattleManager.cs : InjectDeck(), RemoveCardsOfCompanion() 호출
//   - StackCardController.cs : GameManager.Instance 참조
//
// [연결된 파일]
//   - Core/Singleton.cs : 싱글톤 기반 클래스
//   - StackCardController.cs : 카드 UI 컨트롤러
//   - BattleManager.cs : 전투 흐름 관리
// ============================================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// ----------------------------------------------------------
// [StackType 열거형]
// 역할별 스택 종류를 나타냅니다.
// 이 값은 게임 전체에서 공통으로 사용됩니다.
// ----------------------------------------------------------
/// <summary>
/// 역할별 스택 종류.
/// Dealer=딜러, Tank=탱커, Support=서포터
/// </summary>
public enum StackType
{
    Dealer  = 0,  // 딜 (딜러)
    Tank    = 1,  // 탱 (탱커)
    Support = 2   // 힐 (서포터)
}

/// <summary>
/// UI 입력 수신 및 드로우 페이즈 카드 세팅 싱글톤 매니저.
/// GameManager.Instance 로 전역 접근 가능.
/// </summary>
public class GameManager : Singleton<GameManager>
{
    // ----------------------------------------------------------
    // [에셋 설정]
    // ----------------------------------------------------------
    [Header("에셋 설정 (Assets)")]
    [Tooltip("숫자 스프라이트 배열. 인덱스 순서: -5~-1, +1~+5 (총 10장)")]
    public Sprite[] numberSprites;

    [Tooltip("사용한(빈) 카드 슬롯에 표시될 스프라이트")]
    public Sprite emptyCardSprite;

    // ----------------------------------------------------------
    // [UI 연결]
    // myCards : 화면에 표시될 카드 슬롯들 (Inspector 에서 연결)
    // checkButtonBox : 카드 확인/취소 팝업 박스
    // btnConfirm / btnCancel : 확인/취소 버튼
    // ----------------------------------------------------------
    [Header("UI 연결 (UI References)")]
    [Tooltip("화면에 표시될 카드 슬롯 배열. Inspector 에서 연결하세요.")]
    public StackCardController[] myCards;

    [Tooltip("카드 클릭 시 나타나는 확인/취소 팝업 오브젝트")]
    public GameObject checkButtonBox;

    [Tooltip("카드 사용 확인 버튼")]
    public Button btnConfirm;

    [Tooltip("카드 사용 취소 버튼")]
    public Button btnCancel;

    // ----------------------------------------------------------
    // [드로우 덱]
    // 전투 진입 시 BattleManager → DeckBuilder 가 생성하여 주입합니다.
    // 각 카드는 (CardData, 소유자 CompanionData) 쌍으로 저장됩니다.
    // ----------------------------------------------------------
    private List<(CardData card, CompanionData owner)> drawDeck = new();

    /// <summary>현재 드로우 위치 (몇 번째 카드까지 뽑았는지)</summary>
    private int currentDrawIndex = 0;

    // ----------------------------------------------------------
    // 드로우 페이즈 — 카드 슬롯에 카드 세팅
    // drawDeck 에서 카드를 뽑아 myCards 슬롯에 배치합니다.
    // ----------------------------------------------------------

    /// <summary>
    /// 드로우 페이즈: 덱에서 카드를 뽑아 화면 슬롯에 세팅한다.
    /// BattleManager.HandleDrawPhase() 에서 호출됩니다.
    /// </summary>
    public void StartMyTurn()
    {
        Debug.Log("[GameManager] 내 턴 시작! 카드 슬롯을 세팅합니다.");

        for (int i = 0; i < myCards.Length; i++)
        {
            // 이미 카드가 세팅되어 있고 아직 사용하지 않은 슬롯 → 그대로 유지
            if (myCards[i].owner != null && !myCards[i].isUsed)
            {
                Debug.Log($"[GameManager] myCards[{i}] 유지 (미사용 카드)");
                continue;
            }

            if (currentDrawIndex >= drawDeck.Count)
            {
                myCards[i].gameObject.SetActive(false);
                continue;
            }

            // 사용됐거나 초기화되지 않은 슬롯에만 새 카드 드로우
            var (cardData, owner) = drawDeck[currentDrawIndex];
            currentDrawIndex++;

            int stackValue = GenerateStackValue(owner.affinity);
            myCards[i].SetupCard(stackValue, owner);
            myCards[i].gameObject.SetActive(true);
            Debug.Log($"[GameManager] myCards[{i}] 새 카드 드로우 → {stackValue:+#;-#;0} ({owner.displayName})");
        }
    }

    // ----------------------------------------------------------
    // 공개 API
    // ----------------------------------------------------------

    /// <summary>
    /// 전투 진입 시 BattleManager 가 호출하여 덱을 주입한다.
    /// </summary>
    public void InjectDeck(List<(CardData card, CompanionData owner)> deck)
    {
        drawDeck = deck;
        currentDrawIndex = 0;
        Debug.Log($"[GameManager] 덱 주입 완료: {drawDeck.Count}장");
    }

    /// <summary>
    /// 사망한 동료의 카드를 drawDeck 에서 전부 제거한다.
    /// BattleManager.ProcessDeathAndStress() 에서 호출됩니다.
    /// </summary>
    public void RemoveCardsOfCompanion(CompanionData deadCompanion)
    {
        // 이미 뽑힌 카드 중 해당 동료 카드 수 (인덱스 보정용)
        int removedBeforeIndex = drawDeck
            .Take(currentDrawIndex)
            .Count(entry => entry.owner == deadCompanion);

        // drawDeck 에서 해당 동료 카드 전부 제거
        int removedCount = drawDeck.RemoveAll(entry => entry.owner == deadCompanion);

        // 이미 지나간 인덱스도 당겨지므로 보정
        currentDrawIndex = Mathf.Max(0, currentDrawIndex - removedBeforeIndex);

        Debug.Log($"[GameManager] {deadCompanion.displayName} 카드 {removedCount}장 제거 | 잔여 덱: {drawDeck.Count}장");
    }

    /// <summary>
    /// 카드 확정 시 호출. BattleManager(PlayerRoleCost) 에 스택을 반영한다.
    /// StackCardController.HandleConfirm() 에서 호출됩니다.
    /// </summary>
    public void OnCardUsed(StackCardController usedCard)
    {
        Debug.Log($"[GameManager] 카드 사용됨 | 역할={usedCard.stackType} 스택={usedCard.stackDelta:+#;-#;0}");

        // 스택 관리 권한: PlayerRoleCost
        if (PlayerRoleCost.Instance != null)
            PlayerRoleCost.Instance.Add(usedCard.stackType, usedCard.stackDelta);
    }

    /// <summary>
    /// 내 턴 종료. BattleManager.FinishPlayerTurn() 을 호출한다.
    /// </summary>
    public void EndMyTurn()
    {
        Debug.Log("[GameManager] 내 턴 종료 신호 전송.");
        BattleManager.Instance?.FinishPlayerTurn();
    }

    // ----------------------------------------------------------
    // 내부 유틸리티
    // ----------------------------------------------------------

    /// <summary>
    /// 성향 규칙에 따라 0을 제외한 스택 값을 생성한다.
    /// AffinityHelper.GenerateStack() 이 0을 반환하면 최대 20회 재시도.
    /// </summary>
    private int GenerateStackValue(CardAffinity affinity)
    {
        // None 성향은 낙천가(Optimist)와 동일하게 처리
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
    /// stackValue (-5~-1, +1~+5) 에 대응하는 스프라이트를 반환한다.
    /// 배열 인덱스: 0=-5, 1=-4, 2=-3, 3=-2, 4=-1, 5=+1, 6=+2, 7=+3, 8=+4, 9=+5
    /// </summary>
    private Sprite GetSpriteForValue(int value)
    {
        if (numberSprites == null || numberSprites.Length == 0) return null;

        int index = value > 0 ? value + 4 : value + 5;
        index = Mathf.Clamp(index, 0, numberSprites.Length - 1);
        return numberSprites[index];
    }

    // ----------------------------------------------------------
    // [ContextMenu] 무결성 테스트
    // ----------------------------------------------------------

    /// <summary>[에디터 테스트] 현재 덱 상태를 콘솔에 출력한다.</summary>
    [ContextMenu("TEST / 덱 상태 출력")]
    private void TestPrintDeck()
    {
        Debug.Log($"[GameManager] 덱 상태: 전체={drawDeck.Count}장 | 현재 인덱스={currentDrawIndex}");
        for (int i = currentDrawIndex; i < drawDeck.Count; i++)
            Debug.Log($"  [{i}] {drawDeck[i].owner?.displayName ?? "없음"} — {drawDeck[i].card?.id ?? "없음"}");
    }

    /// <summary>[에디터 테스트] 카드 슬롯 연결 상태를 확인한다.</summary>
    [ContextMenu("TEST / 카드 슬롯 연결 확인")]
    private void TestCheckCardSlots()
    {
        Debug.Log($"[GameManager] 카드 슬롯 확인: {myCards?.Length ?? 0}개");
        if (myCards != null)
            for (int i = 0; i < myCards.Length; i++)
                Debug.Log($"  슬롯[{i}]: {(myCards[i] == null ? "NULL ← 연결 필요!" : myCards[i].name)}");

        Debug.Log($"  checkButtonBox: {(checkButtonBox == null ? "NULL ← 연결 필요!" : checkButtonBox.name)}");
        Debug.Log($"  btnConfirm: {(btnConfirm == null ? "NULL ← 연결 필요!" : btnConfirm.name)}");
        Debug.Log($"  btnCancel: {(btnCancel == null ? "NULL ← 연결 필요!" : btnCancel.name)}");
    }
}
