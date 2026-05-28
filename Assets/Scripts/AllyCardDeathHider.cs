// ============================================================
// AllyCardDeathHider.cs
// 아군 카드 사망 시 GameObject 자동 비활성화
// ============================================================
//
// [이 파일이 하는 일]
//   아군 카드 GameObject 에 부착되어, 연결된 FellowData 의 OnDied 이벤트가
//   발동되는 순간 자신을 SetActive(false) 로 즉시 숨긴다.
//
// [왜 별도 컴포넌트?]
//   1. DefaultSetting 책임 분리: "생성"만 하고 "사망 후 처리"는 이 컴포넌트가 담당.
//      → 단일 책임 원칙 + DefaultSetting 비대화 방지.
//   2. OnDestroy 에서 이벤트 자동 해제 → 메모리 누수 방지.
//      (FellowData.OnDied 가 [NonSerialized] 라 SO 인스턴스가 살아있는 한 누적 위험)
//   3. 추후 사망 연출 (페이드아웃 / 효과음 / 파티클) 추가 시 이 컴포넌트만 확장.
//
// [사용 방법]
//   DefaultSetting.SpawnObject() 가 아군 카드 생성 시 AddComponent + Bind 호출.
//
// [생명 주기]
//   카드 GameObject 가 ClearSpawnedObjects() 로 Destroy 되면 OnDestroy 자동 호출 →
//   이벤트 구독 해제 → 다음 전투 진입 시 깨끗한 상태에서 다시 Bind.
// ============================================================

using UnityEngine;

public class AllyCardDeathHider : MonoBehaviour
{
    /// <summary>이 카드와 연결된 동료 데이터 (Bind 시 설정)</summary>
    private FellowData _fellow;

    /// <summary>
    /// 카드와 동료를 연결한다. DefaultSetting.SpawnObject() 에서 호출.
    /// fellow.OnDied 이벤트에 HandleDied 를 구독한다.
    /// </summary>
    public void Bind(FellowData fellow)
    {
        // 이미 다른 fellow 에 바인딩되어 있으면 먼저 해제 (안전장치)
        if (_fellow != null) _fellow.OnDied -= HandleDied;

        _fellow = fellow;
        if (_fellow != null) _fellow.OnDied += HandleDied;
    }

    /// <summary>OnDied 이벤트 핸들러 — 사망 연출(슬라이드+페이드) 재생 후 비활성화</summary>
    private void HandleDied()
    {
        if (this == null || gameObject == null) return;

        var sprites = GetComponent<BattleCardSprites>();
        if (sprites != null)
        {
            sprites.PlayDeathFall(onComplete: () =>
            {
                if (this != null && gameObject != null)
                    gameObject.SetActive(false);
            });
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// GameObject 파괴 시 이벤트 구독 자동 해제 (메모리 누수 방지).
    /// ClearSpawnedObjects() 가 호출되면 자동 실행.
    /// </summary>
    void OnDestroy()
    {
        if (_fellow != null)
            _fellow.OnDied -= HandleDied;
    }
}
