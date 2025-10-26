using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : BaseSingleton<UIManager>
{
    [SerializeField] public Sprite[] cocoFaceSprites;
    [SerializeField] public Image cocoImage;

    [SerializeField] public GameObject WinPanel;
    [SerializeField] public GameObject LosePanel;

    public void ReStartPanel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void SetUICocoFace(int cnt)
    {
        cocoImage.sprite = cocoFaceSprites[cnt];
    }

    public void OpenWinPanel()
    {
        WinPanel.SetActive(true);
    }

    public void OpenLosePanel()
    {
        LosePanel.SetActive(true);
    }
}
