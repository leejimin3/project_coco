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
            Debug.LogError("코코 애니메이터 누락");
        }
    }

    public void Actions()
    {
        Debug.Log("Action");
    }

    public void SetState(int state)
    {
        int lastState = runState;
        if (runState == state) return; // 같은 상태면 무시

        runState = state;
        anim.SetInteger("State", runState);


        if (runState > lastState)
        {
            switch (state)
            {
                case 0:
                    break;
                case 1:
                    AudioManager.Instance.PlaySfx(Sfx.coco_sound1);
                    break;
                case 2:
                    AudioManager.Instance.PlaySfx(Sfx.coco_sound2);
                    break;
                case 3:
                    AudioManager.Instance.PlaySfx(Sfx.coco_sound3);
                    break;
            }
        }


        // 직접 애니메이션 재생 (Animator Controller 설정 없이도 작동)
        string animName = "run" + state;
        if (anim.HasState(0, Animator.StringToHash(animName)))
        {
            anim.Play(animName, 0);
        }
        
        UIManager.Instance.SetUICocoFace(state);

        Debug.Log($"Coco State Changed: run{state}");
    }
}
