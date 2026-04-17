using UnityEngine;

public class DisplayChange : MonoBehaviour
{
    public static DisplayChange instance = null;
    [SerializeField] private GameObject[] nodeDisplay;
    [SerializeField] private GameObject[] actionDisplay;

    void Awake()
    {
        if (null == instance)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //(해당 클래스에서 수행할 메서드)
    public void ToggleDisplay()
    {
        if (nodeDisplay != null && actionDisplay != null)
        {
            DisplayChanger(nodeDisplay, actionDisplay);
        }
    }
    //(다른 클래스에서 불러오는 메서드)action <-> node 디스플레이 활성화/비활성화 상태 변경 메서드
    public void DisplayChanger(GameObject[] node, GameObject[] action)
    {
        foreach(GameObject n in node) n.SetActive(!n.activeSelf);
        foreach(GameObject a in action)a.SetActive(!a.activeSelf);
    }
}
