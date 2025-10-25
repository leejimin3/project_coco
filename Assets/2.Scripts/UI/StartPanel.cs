using UnityEngine;

public class StartPanel : MonoBehaviour
{
    public void TabStartButton()
    {
        GameManager.Instance?.StartFill();
        gameObject.SetActive(false);
    }
}
