using System;
using UnityEngine;

public class Coco : MonoBehaviour
{
    public Animator anim;
    public int runState;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogError("코코 애니메이션 누락");
        }
    }

    private void Update()
    {
        TryInput(); 
        TryTest();
    }
    
    private void TryInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Q스킬 사용");
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("W스킬 사용");
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E스킬 사용");
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("R스킬 사용");
        }
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
