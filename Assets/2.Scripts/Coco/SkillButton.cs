using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum Friends
{
    mell,
    miew,
    adwd,
    toto,
}
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
    
    public Friends EFriend;
    public void TryAttack()
    {
        if (bIsCoolDown)
            return;

        bool flag = GameManager.Instance.TryAttack(key);
        if (flag)
        {
            AudioManager.Instance.PlaySfx(Sfx.key_success);
            StartReaction(true);
        }
        if (!flag)
        {
            AudioManager.Instance.PlaySfx(Sfx.key_failed);
            StartReaction(false);
            StartDimmed();
            StartCoolDown();
        }
    }

    public void AllFail()
    {
        if (bIsCoolDown)
            return;
        
        AudioManager.Instance.PlaySfx(Sfx.key_failed);
        StartReaction(false);
        StartDimmed();
        StartCoolDown();
    }

    public void StartReaction(bool flag)
    {
        if (ReactionCoroutine != null)
        {
            StopCoroutine(ReactionCoroutine);
            BodyImage.sprite = DefaultImage;
            DimmedImage.sprite = DefaultImage;
        }
            

        ReactionCoroutine = StartCoroutine(Reaction(flag));
    }

    private IEnumerator Reaction(bool flag)
    {
        if (flag)
        {
            BodyImage.sprite = ReactionImage[1];
            DimmedImage.sprite = ReactionImage[1];

            switch (EFriend)
            {
                case Friends.mell:
                    AudioManager.Instance.PlaySfx(Sfx.mell_success);
                    break;
                case Friends.miew:
                    AudioManager.Instance.PlaySfx(Sfx.miew_success);
                    break;
                case Friends.adwd:
                    AudioManager.Instance.PlaySfx(Sfx.adwd_success);
                    break;
                case Friends.toto:
                    AudioManager.Instance.PlaySfx(Sfx.toto_success);
                    break;
                default:
                    break;
            }
        }
        else
        {
            BodyImage.sprite = ReactionImage[0];
            DimmedImage.sprite = ReactionImage[0];
            
            switch (EFriend)
            {
                case Friends.mell:
                    AudioManager.Instance.PlaySfx(Sfx.mell_failed);
                    break;
                case Friends.miew:
                    AudioManager.Instance.PlaySfx(Sfx.miew_failed);
                    break;
                case Friends.adwd:
                    AudioManager.Instance.PlaySfx(Sfx.adwd_failed);
                    break;
                case Friends.toto:
                    AudioManager.Instance.PlaySfx(Sfx.toto_failed);
                    break;
                default:
                    break;
            }
        }
        
        yield return new WaitForSeconds(coolDownTime);
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

