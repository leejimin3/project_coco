using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    public GameObject Cooldown;

    [Header("Cooldown Settings")]
    public float coolDownTime;

    private bool bIsCoolDown = false;
    private Coroutine coolDownCoroutine;
    private Coroutine dimmedCoroutine;
    private Coroutine ReactionCoroutine;
    public KeyCode key;
    
    public TextMeshProUGUI keyText;

    public Sprite DefaultImage;
    public Image BodyImage;
    public Image DimmedImage;
    
    public float DimmedValue = 0.2f;

    public Sprite[] ReactionImage;
    
    public Animator anim;
    public void TryAttack()
    {
        if (bIsCoolDown)
            return;

        bool flag = GameManager.Instance.TryAttack(key);
        if (flag)
        {
            StartReaction(true);
        }
        if (!flag)
        {
            StartReaction(false);
            StartDimmed();
            StartCoolDown();
        }
    }

    public void StartReaction(bool flag)
    {
        if (coolDownCoroutine != null)
            StopCoroutine(coolDownCoroutine);

        coolDownCoroutine = StartCoroutine(Reaction(flag));
    }

    private IEnumerator Reaction(bool flag)
    {
        if (flag)
        {
            BodyImage.sprite = ReactionImage[1];
            DimmedImage.sprite = ReactionImage[1];
        }
        else
        {
            BodyImage.sprite = ReactionImage[0];
            DimmedImage.sprite = ReactionImage[0];
        }
        
        yield return new WaitForSeconds(0.5f);
        BodyImage.sprite = DefaultImage;
        DimmedImage.sprite = DefaultImage;
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
    
    public void StartDimmed()
    {
        if (dimmedCoroutine != null)
            StopCoroutine(dimmedCoroutine);

        dimmedCoroutine = StartCoroutine(Dimmed());
    }

    private IEnumerator Dimmed()
    {
        DimmedImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        DimmedImage.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        DimmedImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        DimmedImage.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        DimmedImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        DimmedImage.gameObject.SetActive(false);
    }
}

