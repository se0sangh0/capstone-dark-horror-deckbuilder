// ============================================================
// Event/EventDefinition.cs
// `?` 노드 선택지 이벤트 1종의 ScriptableObject 정의
// ============================================================
//
// [에셋 생성]
//   ① 자동:   Tools ▸ DarkHorror ▸ 이벤트 카탈로그 생성 (19종 일괄) — 권장
//   ② 수동:   Assets 우클릭 ▸ Create ▸ DarkHorror ▸ EventDefinition
//
// [로드 경로]
//   런타임은 Resources/Events/ 아래의 EventDefinition 에셋을 모두 로드해
//   무작위로 1개를 뽑는다 (EventCatalog).
//   에셋이 하나도 없으면 EventCatalogData 의 코드 정의로 폴백한다.
//
// [기획 참조] 06_이벤트_노드.md §5 데이터 구조
// ============================================================

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DarkHorror/EventDefinition", fileName = "evt_new")]
public class EventDefinition : ScriptableObject
{
    [Header("식별")]
    [Tooltip("로직용 고유 ID. 예: evt_cold_camp")]
    public string id;

    [Tooltip("이벤트 제목. 팝업 상단에 표시.")]
    public string title;

    [Header("본문")]
    [TextArea(2, 6)]
    [Tooltip("상황 텍스트. 팝업 본문에 표시.")]
    public string bodyText;

    [Header("선택지 (2~3)")]
    public List<EventChoice> choices = new();

    [Header("발생 층 제한 (미결 — §4/§15 B2)")]
    [Tooltip("이 이벤트가 등장 가능한 최소 층 (1-base).")]
    public int minFloor = 1;

    [Tooltip("이 이벤트가 등장 가능한 최대 층 (1-base).")]
    public int maxFloor = 10;

    /// <summary>이번 층 조건에 부합하는지. (floor 는 1-base)</summary>
    public bool MatchesFloor(int floor) => floor >= minFloor && floor <= maxFloor;
}
