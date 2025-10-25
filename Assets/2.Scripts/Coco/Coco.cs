using System;
using UnityEngine;

public class Coco : MonoBehaviour
{
    public Animator anim;
    public int runState;

    public float moveSpeed = 1.66f;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogError("코코 애니메이터 누락");
        }
    }

    public void Actions()
    {
        Debug.Log("Action");
    }

    public void SetState(int state)
    {
        if (runState == state) return; // 같은 상태면 무시

        runState = state;
        anim.SetInteger("State", runState);

        // 직접 애니메이션 재생 (Animator Controller 설정 없이도 작동)
        string animName = "run" + state;
        if (anim.HasState(0, Animator.StringToHash(animName)))
        {
            anim.Play(animName, 0);
        }

        Debug.Log($"Coco State Changed: run{state}");
    }
}
