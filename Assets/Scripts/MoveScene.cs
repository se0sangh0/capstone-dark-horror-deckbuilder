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

        // 튜토리얼 자동 진입 — 완료 플래그가 false 일 때만 (재실행 시는 일반 진입)
        if (!TutorialManager.IsCompleted())
        {
            EnsureTutorialManager();
            TutorialManager.Instance?.StartTutorial();
            Debug.Log("[MoveScene] 튜토리얼 자동 진입 (완료 플래그 없음)");

            // 튜토리얼은 런 세션 밖 — 파티만 튜토리얼 구성으로 강제 재초기화.
            PartyManager.Instance?.ForceReinitParty();
            SceneTransition.Go("GamePlayScene");
            return;
        }

        // 일반 게임 — 혹시 이전 세션에서 켜진 IsTutorial 이 남아있으면 해제.
        if (TutorialManager.Instance != null) TutorialManager.Instance.EndTutorial(markComplete: false);

        // 새 런 초기화는 RunSessionManager.StartNewRun 단일 창구 (16-B §4).
        // 예비대·영혼석·파티가 계약된 순서로 함께 초기화된다 (기존 파티만 재생성하던 누수 수정).
        // 전체 초기화 성공 후에만 1층(GamePlayScene) 입력을 연다.
        if (RunSessionManager.Instance == null || !RunSessionManager.Instance.StartNewRun())
        {
            Debug.LogError("[MoveScene] 새 런 초기화 실패 — 타이틀에 머무름 (1층 입력 미개방)");
            return;
        }

        Debug.Log("[MoveScene] 일반 게임 진입 — 새 런 시작");
        SceneTransition.Go("GamePlayScene");
    }

    /// <summary>
    /// 메인 메뉴 [처음이신가요?] 버튼 onClick 핸들러.
    /// 완료 플래그 유지 + 튜토리얼 강제 진입.
    /// </summary>
    public void StartTutorialAgain()
    {
        EnsureTutorialManager();
        TutorialManager.Instance?.StartTutorial();
        // 일반 게임 파티 잔재 → 튜토리얼 3인 파티로 강제 재초기화
        PartyManager.Instance?.ForceReinitParty();
        Debug.Log("[MoveScene] [처음이신가요?] 클릭 — 튜토리얼 재진입");
        SceneTransition.Go("GamePlayScene");
    }

    /// <summary>TutorialManager 가 씬에 없으면 생성한다 (DontDestroyOnLoad).</summary>
    private static void EnsureTutorialManager()
    {
        if (TutorialManager.Instance != null) return;
        var go = new GameObject("TutorialManager");
        go.AddComponent<TutorialManager>();
    }
}
