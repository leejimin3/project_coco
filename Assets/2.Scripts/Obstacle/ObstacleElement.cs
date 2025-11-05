using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObstacleElement : MonoBehaviour
{
    public GameObject KeyRoot;
    public TextMeshProUGUI keyTMP;
    
    public Image spriteImage;

    public void ShowKey(string key)
    {
        KeyRoot.SetActive(true);
        keyTMP.text = key;
    }
    
    public void ShowKey(Sprite sprite)
    {
        KeyRoot.SetActive(true);
        spriteImage.sprite = sprite;
        spriteImage.SetNativeSize();
    }

    public void HideKey()
    {
        KeyRoot.SetActive(false);
    }
}
