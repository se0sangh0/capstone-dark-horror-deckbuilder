// ============================================================
// Core/Singleton.cs
// 제네릭 싱글톤 베이스 클래스 (Generic Singleton Base Class)
// ============================================================
//
// [이 파일이 하는 일] ← 초등학생도 이해할 수 있는 설명
//   게임 안에는 "딱 하나만 있어야 하는 매니저(관리자)" 들이 있습니다.
//   예를 들어 GameManager, BattleManager, PartyManager 같은 것들이요.
//   이 파일은 그 "딱 하나만" 규칙을 자동으로 적용해주는 마법 틀입니다.
//
// [어떻게 쓰나요?]
//   내 클래스가 싱글톤이 되고 싶으면 이렇게 씁니다:
//
//     public class GameManager : Singleton<GameManager> { ... }
//
//   그러면 어디서든 GameManager.Instance 로 접근할 수 있습니다!
//
// [씬이 바뀌어도 살아남기]
//   Inspector 에서 "씬이 바뀌어도 유지" 체크박스를 켜면
//   씬이 바뀌어도 이 오브젝트가 사라지지 않습니다.
//
// [주의사항]
//   - 자식 클래스에서 Awake() 를 사용하려면 반드시
//     protected override void Awake() { base.Awake(); ... } 형식으로 작성하세요.
//   - MonoBehaviour 를 상속하므로 반드시 GameObject 에 붙여서 사용하세요.
// ============================================================

using UnityEngine;

/// <summary>
/// 제네릭 싱글톤 패턴 기반 클래스.
/// T 에 자신의 타입을 지정하면 Instance 프로퍼티와 중복 제거 로직이
/// 자동으로 제공된다.
///
/// 사용 예:  public class GameManager : Singleton&lt;GameManager&gt; { }
/// </summary>
/// <typeparam name="T">싱글톤이 될 자기 자신의 클래스 타입</typeparam>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // ----------------------------------------------------------
    // [Instance]
    // "나(T) 의 유일한 대표자" 를 가리키는 전역 참조입니다.
    // 게임 어디서든 GameManager.Instance 처럼 바로 접근할 수 있어요.
    // ----------------------------------------------------------
    public static T Instance { get; private set; }

    // ----------------------------------------------------------
    // [persistAcrossScenes]
    // true 로 설정하면 씬이 전환되어도 이 오브젝트가 파괴되지 않습니다.
    // Inspector 의 체크박스로 켜고 끌 수 있습니다.
    // ----------------------------------------------------------
    [SerializeField]
    [Tooltip("체크하면 씬이 바뀌어도 이 오브젝트가 사라지지 않습니다 (DontDestroyOnLoad)")]
    private bool persistAcrossScenes = false;

    // ----------------------------------------------------------
    // Awake — 유니티가 오브젝트를 처음 만들 때 자동 호출
    // "이미 나와 같은 게 있으면 내가 사라지고, 없으면 내가 유일한 존재가 된다!"
    // ----------------------------------------------------------
    protected virtual void Awake()
    {
        if (Instance == null)
        {
            // 처음 생성된 인스턴스: 나를 Instance 로 등록
            Instance = this as T;

            // persistAcrossScenes 가 true 이면 씬 전환 시 파괴되지 않음
            if (persistAcrossScenes)
                DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 이미 Instance 가 존재함: 중복이므로 이 오브젝트를 즉시 제거
            Destroy(gameObject);
        }
    }
}
