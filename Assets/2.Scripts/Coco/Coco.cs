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
    
    private void Update()
    {
        TryTest();
    }

    public void Actions()
    {
        Debug.Log("Action");
    }

    private void TryTest()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            runState++;
            anim.SetInteger("State", runState);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            runState--;
            anim.SetInteger("State", runState);
        }
    }
}
