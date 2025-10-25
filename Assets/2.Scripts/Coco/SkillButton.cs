using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    public GameObject Cooldown;

    [Header("Cooldown Settings")]
    public float coolDownTime;   // 쿨타임 시간 (초)

    private bool bIsCoolDown = false;
    private Coroutine coolDownCoroutine;

    private event Action SkillAction;
    
    public KeyCode key;
    
    public TextMeshProUGUI keyText;
    
    public void SetSkill(Action callback)
    {
        SkillAction += callback;
    }

    public void TryAttack()
    {
        if (bIsCoolDown)
            return;
        
        SkillAction?.Invoke();
        StartCoolDown();
    }

    public void StartCoolDown()
    {
        if (coolDownCoroutine != null)
            StopCoroutine(coolDownCoroutine);

        coolDownCoroutine = StartCoroutine(CoolDown());
    }

    private IEnumerator CoolDown()
    {
        bIsCoolDown = true;
        Cooldown.SetActive(true);
        yield return new WaitForSeconds(coolDownTime);
        Cooldown.SetActive(false);
        bIsCoolDown = false;
    }
}
