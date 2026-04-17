// ============================================================
// GameExit.cs
// 게임 종료 버튼 핸들러
// ============================================================
//
// [이 파일이 하는 일]
//   게임 종료 버튼을 누르면 게임을 종료합니다.
//   유니티 에디터에서는 에디터 플레이 모드를 종료합니다.
//   실제 빌드된 게임에서는 Application.Quit() 으로 종료됩니다.
//
// [어디서 쓰이나요?]
//   - 메인 메뉴 씬의 "게임 종료" 버튼의 onClick 이벤트에 연결
//
// [인스펙터 설정]
//   - 버튼 오브젝트의 onClick 에 Exit() 를 연결하세요.
// ============================================================

using UnityEngine;

/// <summary>
/// 게임 종료 버튼 이벤트 핸들러.
/// </summary>
public class GameExit : MonoBehaviour
{
    /// <summary>
    /// 게임을 종료한다.
    /// 에디터에서는 플레이 모드를 종료하고, 빌드에서는 프로그램을 닫는다.
    /// </summary>
    public void Exit()
    {
        Debug.Log("[GameExit] 게임 종료 요청됨.");

#if UNITY_EDITOR
        // 에디터 에서는 플레이 모드 종료
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 실제 빌드에서는 애플리케이션 종료
        Application.Quit();
#endif
    }
}
