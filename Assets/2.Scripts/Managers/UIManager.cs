using UnityEngine;
using UnityEngine.UI;

public class UIManager : BaseSingleton<UIManager>
{
    [SerializeField] public Sprite[] cocoFaceSprites;
    [SerializeField] public Image cocoImage;


    public void SetUICocoFace(int cnt)
    {
        cocoImage.sprite = cocoFaceSprites[cnt];
    }
}
