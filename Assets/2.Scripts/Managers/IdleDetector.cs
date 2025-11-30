using UnityEngine;
using UnityEngine.SceneManagement;

public class IdleDetector : BaseSingleton<IdleDetector>
{
    public float idleLimit = 30f;
    private float lastInputTime;
    [HideInInspector] public bool isIdle = false;

    void Start()
    {
        lastInputTime = Time.realtimeSinceStartup;
    }

    void Update()
    {
        Debug.Log(Time.realtimeSinceStartup - lastInputTime);
        // 입력 감지
        if (CheckAnyInput())
        {
            if (isIdle)
            {
                // idle 상태였다가 입력 들어옴
                OnBackFromIdle();
                isIdle = false;
            }

            lastInputTime = Time.realtimeSinceStartup;
        }

        // 5분 이상 입력 없으면 Idle 처리
        if (!isIdle && Time.realtimeSinceStartup - lastInputTime >= idleLimit)
        {
            OnIdle();
            isIdle = true;
        }
    }

    // 키보드 또는 마우스 입력 감지
    private bool CheckAnyInput()
    {
        if (Input.anyKeyDown) return true;
        if (Input.GetMouseButtonDown(0)) return true;
        if (Input.GetMouseButtonDown(1)) return true;
        if (Input.GetMouseButtonDown(2)) return true;
        
        return false;
    }

    /// <summary>
    /// 5분 동안 입력이 없을 때 호출
    /// </summary>
    private void OnIdle()
    {
        Time.timeScale = 1;
        AudioManager.Instance.StopSfxLoop();
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            SceneManager.LoadScene(0);
        }
    }

    /// <summary>
    /// Idle 상태에서 입력이 들어오면 호출
    /// </summary>
    private void OnBackFromIdle()
    {
        AudioManager.Instance.PlayBgm(true);
        Time.timeScale = 1;

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            AudioManager.Instance.PlaySfxLoop(Sfx.bgm_cutScene);
        }
    }
}