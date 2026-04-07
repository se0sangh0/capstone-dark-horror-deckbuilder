using UnityEngine;

public class GameManager : MonoBehaviour
{
    public void GameExit()
    {
        Debug.Log("게임 종료"); // 에디터 확인용
        Application.Quit(); // 실제 게임 종료
    }
     

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
