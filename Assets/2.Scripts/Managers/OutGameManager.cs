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
        DataManager.Instance.difficulty = diff;
        SceneManager.LoadScene(1);
    }
}
