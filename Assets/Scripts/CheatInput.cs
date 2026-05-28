// ============================================================
// CheatInput.cs
// 치트 입력 처리 (개발/디버깅 전용)
// ============================================================
//
// [동작]
//   F1 — BattleManager 활성 여부로 분기:
//     ✦ 배틀 노드 진행 중 → 모든 역할 스택을 999 로 설정
//     ✦ 그 외 (노드맵 등)  → 소울스톤 +10000, 마나스톤 +10000
//   F2 — 노드 1단계 전진 (현재 층에서 다음 층으로 점프, RoomType 패널 없이)
//
// [씬 배치 — ⭐ 사용자 직접]
//   영구 살아있는 GameObject(DontDestroyOnLoad 적용) 에 이 컴포넌트를 부착하세요.
//   GameManager, PartyManager, FellowDatabase 등 이미 DontDestroyOnLoad 인 GameObject 에
//   같이 부착해도 OK. 또는 별도의 GameObject 만들어 부착하고 DontDestroyOnLoad 처리.
//
// [⚠️ 출시 전 제거]
//   배포 빌드에서는 이 컴포넌트를 빼거나 #if UNITY_EDITOR 로 감싸세요.
// ============================================================

using UnityEngine;
using UnityEngine.InputSystem;

public class CheatInput : MonoBehaviour
{
    [Header("치트 수치 (Inspector 에서 조정 가능)")]
    [Tooltip("F1 (배틀 중) — 모든 역할 스택을 이 값으로 설정")]
    [SerializeField] private int cheatStackValue = 999;

    [Tooltip("F1 (배틀 외) — 소울스톤/마나스톤에 더할 값")]
    [SerializeField] private int cheatCurrencyValue = 10000;

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            if (IsInBattle()) ApplyStackCheat();
            else              ApplyCurrencyCheat();
        }

        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            ApplyAdvanceFloorCheat();
        }
    }

    /// <summary>BattleManager 가 씬에서 활성 상태이면 "배틀 노드 진행 중" 으로 본다.</summary>
    private bool IsInBattle()
    {
        return BattleManager.Instance != null
            && BattleManager.Instance.isActiveAndEnabled;
    }

    private void ApplyStackCheat()
    {
        if (PlayerRoleCost.Instance == null)
        {
            Debug.LogWarning("[CheatInput] PlayerRoleCost 없음 — 스택 치트 무시.");
            return;
        }
        foreach (StackType role in System.Enum.GetValues(typeof(StackType)))
            PlayerRoleCost.Instance.SetAmount(role, cheatStackValue);

        Debug.Log($"[CheatInput] 🎮 F1(배틀 중) — 모든 스택 {cheatStackValue} 로 설정");
    }

    private void ApplyCurrencyCheat()
    {
        bool soulOk = SoulstoneManager.Instance != null;
        bool manaOk = ManastoneManager.Instance != null;

        if (soulOk) SoulstoneManager.Instance.Add(cheatCurrencyValue);
        if (manaOk) ManastoneManager.Instance.Add(cheatCurrencyValue);

        Debug.Log($"[CheatInput] 🎮 F1(배틀 외) — 소울스톤+{(soulOk ? cheatCurrencyValue.ToString() : "스킵")}, 마나스톤+{(manaOk ? cheatCurrencyValue.ToString() : "스킵")}");
    }

    private void ApplyAdvanceFloorCheat()
    {
        if (NodeSystem.Current == null)
        {
            Debug.LogWarning("[CheatInput] 🎮 F2 — NodeSystem 미초기화, 무시");
            return;
        }
        NodeSystem.Current.CheatAdvanceFloor();
    }
}
