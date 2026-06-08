// ============================================================
// Currency/MetaShopButton.cs
// 노드 화면 우측 상단 [마석 상점] 버튼 (2026-06-07 노드 시퀀스 재설계)
// ============================================================
//
// 기존: 마석 상점(MagicStoneShopPanel)은 새 런 시작 시 자동으로만 열렸다.
// 변경: 초록 시작 노드는 순수 출발점이 되고, 마석 해금은 게임 중 언제든
//       이 버튼으로 열 수 있도록 분리한다.
//
// [사용]
//   노드 화면(NodeDisplay) 우측 상단에 Button 을 두고 이 컴포넌트를 부착한다.
//   onClick 은 런타임에 자가 배선하므로 인스펙터 연결이 필요 없다.
// ============================================================

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MetaShopButton : MonoBehaviour
{
    private MagicStoneShopPanel _panel;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (_panel == null)
            _panel = Object.FindFirstObjectByType<MagicStoneShopPanel>(FindObjectsInactive.Include);

        if (_panel != null) _panel.Open();
        else Debug.LogWarning("[MetaShopButton] 씬에 MagicStoneShopPanel 이 없습니다.");
    }
}
