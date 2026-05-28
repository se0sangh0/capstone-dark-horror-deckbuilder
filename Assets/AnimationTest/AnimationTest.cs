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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            anim.SetTrigger("doAttack2");
        }
    }
}
