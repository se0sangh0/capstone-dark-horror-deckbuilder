// ============================================================
// Currency/ManastoneManager.cs
// 마나석(Manastone) 재화 싱글톤 매니저
// ============================================================
//
// [이 파일이 하는 일]
//   마나석 재화를 관리하고, 화면의 마나석 숫자 UI 를 업데이트합니다.
//   ManastoneManager.Instance.Add(5) 로 어디서나 마나석을 추가할 수 있습니다.
//
// [어디서 쓰이나요?]
//   - 스킬 사용, 아이템 구매 등 마나석이 필요한 모든 곳
//
// [연결된 파일]
//   - Currency/BaseCurrency.cs : 재화 관리 공통 베이스 클래스
//   - Currency/SoulstoneManager.cs : 영혼석 매니저 (이것과 쌍을 이룸)
//
// [인스펙터 설정]
//   - amountText : 마나석 개수를 표시할 TMP 텍스트를 연결하세요.
//   - 시작 마나석: 10개 (StartingAmount = 10)
// ============================================================

using UnityEngine;
using TMPro;

/// <summary>
/// 마나석 재화 싱글톤 매니저.
/// ManastoneManager.Instance 로 전역 접근 가능.
/// </summary>
public class ManastoneManager : BaseCurrency<ManastoneManager>
{
    // ----------------------------------------------------------
    // [amountText] — 화면에 마나석 개수를 보여주는 텍스트
    // Inspector 에서 TMP 텍스트 오브젝트를 연결하세요.
    // ----------------------------------------------------------
    [SerializeField]
    [Tooltip("마나석 개수를 표시할 TMP_Text 오브젝트를 연결하세요.")]
    private TMP_Text amountText;

    // ----------------------------------------------------------
    // [SaveKey] — PlayerPrefs 저장 키
    // ----------------------------------------------------------
    protected override string SaveKey => "ManaStone";

    // ----------------------------------------------------------
    // [StartingAmount] — 게임 시작 시 기본 마나석 개수
    // ----------------------------------------------------------
    protected override int StartingAmount => 10;

    // ----------------------------------------------------------
    // UpdateText — 마나석 값이 바뀔 때마다 화면 텍스트 갱신
    // OnCurrencyChanged 이벤트에 자동 구독됨 (BaseCurrency.Awake 에서 처리)
    // ----------------------------------------------------------
    protected override void UpdateText(int amount)
    {
        if (amountText == null) return;

        // N0 형식: 천 단위 쉼표 (예: 1,000)
        amountText.text = $"{amount:N0}";
    }
}
