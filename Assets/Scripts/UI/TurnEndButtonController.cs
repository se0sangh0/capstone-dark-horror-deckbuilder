// TurnEndButtonController.cs
// 턴 종료 버튼의 활성 상태를 BattleManager.currentPhase 에 따라 자동 갱신.
//
// ── 동작 ────────────────────────────────────────────────────────
//   PlayerCardPlay 페이즈 (= 카드 선택 + 턴 종료 대기) 에만 interactable=true.
//   그 외 페이즈 (드로우/행동/결과 처리/턴 종료 후 대기) 에는 interactable=false.
//   Unity Button.interactable=false 가 자동으로 disabled color/sprite 적용.
//
// ── 사용 ────────────────────────────────────────────────────────
//   Turn_Button GameObject 에 부착. Button 컴포넌트는 같은 객체에서 자동 검색.

using UnityEngine;
using UnityEngine.UI;

public class TurnEndButtonController : MonoBehaviour
{
    [SerializeField] private Button button;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
    }

    private void Update()
    {
        if (button == null) return;

        bool canEnd = BattleManager.Instance != null
                   && BattleManager.Instance.currentPhase == BattlePhase.PlayerCardPlay;

        if (button.interactable != canEnd) button.interactable = canEnd;
    }
}
