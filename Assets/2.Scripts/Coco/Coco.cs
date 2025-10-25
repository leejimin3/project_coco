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
        runState = state;
        anim.SetInteger("State", runState);
    }
}
