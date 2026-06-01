// ============================================================
// MoveScene.cs
// 씬 전환 버튼 핸들러
// ============================================================
//
// [이 파일이 하는 일]
//   버튼을 누르면 지정된 씬으로 이동합니다.
//   현재는 "InGameScene" (전투 씬) 으로 이동하는 기능만 있습니다.
//
// [어디서 쓰이나요?]
//   - 메인 메뉴 씬의 "게임 시작" 버튼의 onClick 이벤트에 연결
//
// [씬 이름 확인]
//   File → Build Settings 에 씬이 등록되어 있어야 합니다.
//   씬 이름이 다르면 SceneManager.LoadScene() 이 실패합니다.
//
// [인스펙터 설정]
//   - 버튼 오브젝트의 onClick 에 InGameSceneLoaded() 를 연결하세요.
// ============================================================

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 전환 버튼 이벤트 핸들러.
/// </summary>
public class MoveScene : MonoBehaviour
{
    [Header("튜토리얼 재진입 (옵션)")]
    [Tooltip("[처음이신가요?] 버튼 — 튜토리얼 완료 플래그가 true 일 때만 자동으로 활성화. 인스펙터에 메인 메뉴 버튼 연결.")]
    [SerializeField] private GameObject tutorialAgainButton;

    void Start()
    {
        // GameStartScene 의 시작 BGM (제목 화면)
        AudioManager.Instance?.PlayBgmById(BgmId.Title);

        // 튜토리얼 완료 시에만 [처음이신가요?] 버튼 노출 (기획 §15 §2-2)
        if (tutorialAgainButton != null)
            tutorialAgainButton.SetActive(TutorialManager.IsCompleted());
    }

    /// <summary>
    /// InGameScene(전투 씬)으로 이동한다.
    /// 메인 메뉴의 "시작" 버튼 onClick 이벤트에 연결하세요.
    /// 기획 §15: 튜토리얼 완료 플래그(PlayerPrefs)가 없으면 튜토리얼 모드로 자동 진입.
    /// </summary>
    public void InGameSceneLoaded()
    {
        AudioManager.Instance?.PlaySfxById(SfxId.Confirm);

        // 튜토리얼 자동 진입 — 완료 플래그가 false 일 때만 (재실행 시는 일반 진입)
        if (!TutorialManager.IsCompleted())
        {
            EnsureTutorialManager();
            TutorialManager.Instance?.StartTutorial();
            Debug.Log("[MoveScene] 튜토리얼 자동 진입 (완료 플래그 없음)");
        }
        else
        {
            // 이미 완료 — 일반 게임으로. 혹시 이전 세션에서 켜진 IsTutorial 이 남아있으면 해제.
            if (TutorialManager.Instance != null) TutorialManager.Instance.EndTutorial(markComplete: false);
            Debug.Log("[MoveScene] 일반 게임 진입");
        }

        // PartyManager 가 DontDestroyOnLoad — 직전 모드(튜토리얼/일반)의 파티 잔재가 남아있다.
        // 새 모드에 맞게 강제 재초기화 (IsTutorial 보고 튜토리얼/일반 자동 분기).
        PartyManager.Instance?.ForceReinitParty();

        SceneManager.LoadScene("GamePlayScene");
    }

    /// <summary>
    /// 메인 메뉴 [처음이신가요?] 버튼 onClick 핸들러.
    /// 완료 플래그 유지 + 튜토리얼 강제 진입.
    /// </summary>
    public void StartTutorialAgain()
    {
        AudioManager.Instance?.PlaySfxById(SfxId.Confirm);
        EnsureTutorialManager();
        TutorialManager.Instance?.StartTutorial();
        // 일반 게임 파티 잔재 → 튜토리얼 3인 파티로 강제 재초기화
        PartyManager.Instance?.ForceReinitParty();
        Debug.Log("[MoveScene] [처음이신가요?] 클릭 — 튜토리얼 재진입");
        SceneManager.LoadScene("GamePlayScene");
    }

    /// <summary>TutorialManager 가 씬에 없으면 생성한다 (DontDestroyOnLoad).</summary>
    private static void EnsureTutorialManager()
    {
        if (TutorialManager.Instance != null) return;
        var go = new GameObject("TutorialManager");
        go.AddComponent<TutorialManager>();
    }
}
