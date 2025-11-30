using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendTrigger : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Image textBackImage;
    [SerializeField] private TextMeshProUGUI text;

    private int btnRan;
    private int textRan;
    
    public void SetNextTrigger()
    {
         btnRan = UnityEngine.Random.Range(0, GameManager.Instance.buttonList.Count);
         textRan = UnityEngine.Random.Range(0, GameManager.Instance.keyPool.Count);

         if (image != null && text != null)
         {
             SetUI();    
         }
    }

    private void SetUI()
    {
        image.sprite = GameManager.Instance.buttonIconList[btnRan];
        text.text = GameManager.Instance.KeyCodeInString(GameManager.Instance.keyPool[textRan]);
        
        image.SetNativeSize();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Coco"))
        {
            GameManager.Instance.OpenNextFriend(btnRan, textRan);
            image.enabled = false;
            textBackImage.enabled = false;
            text.enabled = false;
        }
    }
}
    