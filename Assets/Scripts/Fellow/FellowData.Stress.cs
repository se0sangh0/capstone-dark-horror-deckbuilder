// FellowData.Stress.cs
// FellowData (partial) — 스트레스 시스템 담당.
//
// ── 역할 ────────────────────────────────────────────────────────
//   FellowData.cs 에서 스트레스 관련 코드를 분리한 파일입니다.
//   partial class 이므로 FellowData 와 동일한 타입으로 컴파일됩니다.
//
// ── 스트레스 시스템 ────────────────────────────────────────────
//   currentStress 프로퍼티로 스트레스 변경 시:
//   1. 0~100 자동 Clamp
//   2. OnStressChanged 이벤트 → UI 자동 업데이트 (LeftPanelView 등)
//
// ── 호출 위치 ──────────────────────────────────────────────────
//   BattleManager.Combat.cs (피격/탈진/사망 패널티/패닉 진입)
//   BattleManager.Phases.cs (GrantStressRecovery — 전투 승리 회복)

using UnityEngine;

public partial class FellowData
{
    // ── 스트레스 필드 ────────────────────────────────────────────
    [Header("스트레스")]
    [SerializeField] private int _currentStress = 0;

    // ── currentStress 프로퍼티 ───────────────────────────────────
    public int currentStress
    {
        get => _currentStress;
        set
        {
            int clamped = Mathf.Clamp(value, 0, 100);
            if (_currentStress == clamped) return;
            _currentStress = clamped;
            OnStressChanged?.Invoke(_currentStress);
        }
    }

    // ── 런타임 전용 (NonSerialized) ──────────────────────────────
    [System.NonSerialized] public System.Action<int> OnStressChanged;
}
