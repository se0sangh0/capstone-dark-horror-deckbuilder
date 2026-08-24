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
        // [P0 제외 범위] 마석·영구 성장(파워업)은 이번 프로토타입 제외 항목.
        // 16. 프로토타입 개발 사양서 §1: "제외 항목은 잠긴 버튼이나 빈 화면으로 미리 보여 주지 않습니다."
        // → 노드 화면의 [파워업/마석 상점] 진입 버튼을 숨긴다 (마석 시스템 복귀 시 이 가드만 제거).
        gameObject.SetActive(false);
        return;

#pragma warning disable CS0162 // 마석 복귀 시 위 2줄 제거하면 살아나는 원 배선 (의도적 보존)
        GetComponent<Button>().onClick.AddListener(OnClick);
#pragma warning restore CS0162
    }

    private void OnClick()
    {
        if (_panel == null)
            _panel = Object.FindFirstObjectByType<MagicStoneShopPanel>(FindObjectsInactive.Include);

        if (_panel != null) _panel.Open();
        else Debug.LogWarning("[MetaShopButton] 씬에 MagicStoneShopPanel 이 없습니다.");
    }
}
