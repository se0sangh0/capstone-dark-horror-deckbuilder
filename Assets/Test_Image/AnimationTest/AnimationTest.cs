using UnityEngine;

public class AnimationTest : MonoBehaviour
{
    private Animator anim;

    void Start() {
        anim = GetComponent<Animator>();
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) { // 좌클릭 시 공격
            anim.SetTrigger("doAttack");
        }
        if (Input.GetKeyDown(KeyCode.H)) { // H키 누르면 피격
            anim.SetTrigger("doHurt");
        }
    }
}
