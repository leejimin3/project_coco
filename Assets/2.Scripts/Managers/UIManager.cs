using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : BaseSingleton<UIManager>
{
    [SerializeField] public Sprite[] cocoFaceSprites;
    [SerializeField] public Image cocoImage;

    [SerializeField] public GameObject WinPanel;
    [SerializeField] public GameObject LosePanel;
    [SerializeField] public GameObject SettingPanel;
    
    [SerializeField] public TextMeshProUGUI cocoText;

    private bool bisOpenSettingPanel = false;
    
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
        cocoText.text = $"{(int)GameManager.Instance.rank} 코코미터";
    }

    public void GotoTitle()
    {
        Time.timeScale = 1f;
        AudioManager.Instance.StopSfxLoop();
        SceneManager.LoadScene(0);
    }

    public void OpenSettingPanel()
    {
        if (bisOpenSettingPanel) return;
        
        bisOpenSettingPanel = true;
        SettingPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        bisOpenSettingPanel = false;
        SettingPanel.SetActive(false);
        Time.timeScale = 1f;   
    }
}
