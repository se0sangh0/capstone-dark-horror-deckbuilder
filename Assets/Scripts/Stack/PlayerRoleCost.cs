// ============================================================
// Stack/PlayerRoleCost.cs
// 아군(플레이어) 역할별 스택 코스트 매니저
// ============================================================
//
// [이 파일이 하는 일]
//   플레이어(아군) 팀의 딜러/탱커/서포터 스택을 관리하고
//   화면의 스택 숫자 UI 를 업데이트합니다.
//
// [어디서 쓰이나요?]
//   - BattleManager.cs : PlayerRoleCost.Instance.GetAmount()/Use()/SetAmount()
//   - GameManager.cs : OnCardUsed → PlayerRoleCost.Instance.Add()
//   - StackCardController.cs : 카드 사용 시 스택 반영
//
// [연결된 파일]
//   - Stack/RoleCostBase.cs : 공통 스택 로직 베이스 클래스
//   - Stack/EnemyRoleCost.cs : 적군 스택 매니저 (이것과 쌍을 이룸)
//
// [인스펙터 설정]
//   - _costTexts : [딜러 텍스트, 탱커 텍스트, 서포터 텍스트] 3개 연결
// ============================================================

using TMPro;

/// <summary>
/// 아군 역할별 스택 코스트 싱글톤 매니저.
/// PlayerRoleCost.Instance 로 전역 접근 가능.
/// </summary>
public class PlayerRoleCost : RoleCostBase<PlayerRoleCost>
{
    // ----------------------------------------------------------
    // [OwnerPrefix] — PlayerPrefs 저장 키 접두사
    // 저장 키 예: "PlayerDealer", "PlayerTank", "PlayerSupport"
    // ----------------------------------------------------------
    protected override string OwnerPrefix => "Player";

    // ----------------------------------------------------------
    // [_costTexts] — 화면에 스택 숫자를 보여주는 TMP 텍스트 배열
    // Inspector 에서 [0]=딜러, [1]=탱커, [2]=서포터 순서로 연결하세요.
    // ----------------------------------------------------------
    [UnityEngine.SerializeField]
    [UnityEngine.Tooltip("딜러/탱커/서포터 스택 표시 TMP 텍스트. 인덱스 0=딜, 1=탱, 2=힐 순서로 연결하세요.")]
    private TMP_Text[] _costTexts;

    // ----------------------------------------------------------
    // UpdateUI — 스택 값이 바뀔 때마다 화면 숫자를 갱신
    // OnCostChanged 이벤트에 자동 구독됨 (RoleCostBase.Awake 에서 처리)
    // ----------------------------------------------------------
    protected override void UpdateUI()
    {
        if (_costTexts == null) return;

        // StackType 순서: Dealer=0, Tank=1, Support=2
        for (int i = 0; i < 3 && i < _costTexts.Length; i++)
        {
            if (_costTexts[i] != null)
                _costTexts[i].text = GetAmount((StackType)i).ToString();
        }
    }
}
