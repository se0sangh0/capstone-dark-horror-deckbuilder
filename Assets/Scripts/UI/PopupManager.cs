// PopupManager.cs
// 팝업(Setting/Log) 열고 닫는 싱글톤. PanelBase 의 Open()/Close() 를 호출.
//
// ── 사용 예 ─────────────────────────────────────────────────────
//   PopupManager.Instance.OpenSetting();
//   PopupManager.Instance.OpenLog();
//   PopupManager.Instance.CloseAll();
//
// ── 인스펙터 ───────────────────────────────────────────────────
//   씬 안의 SettingPopup / LogPopup PanelBase 인스턴스를 연결.
//   PanelBase.Awake 에서 자동으로 SetActive(false) + alpha=0 초기화됨.

using UnityEngine;

public class PopupManager : Singleton<PopupManager>
{
    [SerializeField] private PanelBase settingPopup;
    [SerializeField] private PanelBase logPopup;

    public void OpenSetting()
    {
        CloseAll();
        if (settingPopup != null) settingPopup.Open();
    }

    public void OpenLog()
    {
        CloseAll();
        if (logPopup != null) logPopup.Open();
    }

    public void CloseAll()
    {
        if (settingPopup != null && settingPopup.gameObject.activeSelf) settingPopup.Close();
        if (logPopup     != null && logPopup.gameObject.activeSelf)     logPopup.Close();
    }
}
