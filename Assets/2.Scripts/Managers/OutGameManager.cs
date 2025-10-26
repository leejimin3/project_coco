using UnityEngine;
using UnityEngine.SceneManagement;

public class OutGameManager : MonoBehaviour
{
    void Start()
    {
        AudioManager.Instance.PlaySfxLoop(Sfx.bgm_cutScene);
    }

    public void MoveScene(int diff)
    {
        AudioManager.Instance.StopSfxLoop();
        DataManager.Instance.difficulty = diff;
        SceneManager.LoadScene(1);
    }

    public void ReIntro()
    {
        AudioManager.Instance.StopSfxLoop();
        SceneManager.LoadScene(0);
    }
}
