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
    /// <summary>
    /// InGameScene(전투 씬)으로 이동한다.
    /// 메인 메뉴의 "시작" 버튼 onClick 이벤트에 연결하세요.
    /// </summary>
    public void InGameSceneLoaded()
    {
        Debug.Log("[MoveScene] InGameScene 으로 이동합니다.");
        SceneManager.LoadScene("InGameScene");
    }
}
