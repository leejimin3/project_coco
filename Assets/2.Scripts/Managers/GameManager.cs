using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = System.Random;

public class GameManager : BaseSingleton<GameManager>
{
    public Slider slider;
    public float duration = 30; // 60초 동안

    private Coroutine fillCoroutine;
    public List<SkillButton> buttonList = new  List<SkillButton>();
    
    List<KeyCode> keyPool = new List<KeyCode>
    {
        KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P,
        KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L,
        KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B, KeyCode.N, KeyCode.M,
    };
    
    public Transform[] btnsPos;
    public List<SkillButton> btns;
    public List<KeyCode> keys;

    public float[] nextFriendValue = new float[] {0f, 0.1f, 0.25f, 0.4f};
    public int currentOpenedFriend = 0;
    public bool isFriendComplete = false;
    public float rank;

    [Header("Game State")]
    private bool isGameOver = false;

    public GameObject[] SpawnManagers;
    
    public GameObject ObstacleTrigger;

    public Transform BoatPosition;
    

    private void Start()
    {
        if (DataManager.Instance.difficulty == 0)
        {
            AudioManager.Instance.PlaySfxLoop(Sfx.bgm_ingame);            
        }
        else if (DataManager.Instance.difficulty == 1)
        {
            AudioManager.Instance.PlaySfxLoop(Sfx.bgm_extream);
            ObstacleTrigger.transform.position += new Vector3(4.36f, 0f, 0f);
            ObstacleTrigger.GetComponent<BoxCollider2D>().size = new Vector3(12f, 4f);
        }

        StartFill();
        Instantiate(SpawnManagers[DataManager.Instance.difficulty]);
    }

    private void Update()
    {
        if (isGameOver) return;

        rank += Time.deltaTime;
        TryInput();
        
        if (Input.anyKeyDown)
        {
            // Input.anyKeyDown이 true일 때, 실제 눌린 KeyCode 확인
            foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(k) && !keys.Contains(k) && keyPool.Contains(k))
                {
                    foreach (var btn in btns)
                    {
                        btn.AllFail();
                    }
                }
            }
        }
    }

    public bool TryAttack(KeyCode key)
    {
        var obstacles = ObstacleManager.Instance.TriggeredObstacle;
        
        var sortedObstacles = obstacles
            .Where(pair => pair.Key.transform.position.x > BoatPosition.position.x) // 오른쪽만 선택
            .OrderBy(pair => pair.Key.transform.position.x - BoatPosition.position.x) // 차이값이 작은 순으로 정렬
            .ToList();
        
        foreach (var obstacle in sortedObstacles)
        {
            foreach (var code in obstacle.Value)
            {
                if (code == key)
                {
                    ObstacleManager.Instance.SuccecsObstacle(obstacle.Key);
                    return true;
                }
            }
        }

        return false;
    }   

    public void TryInput()
    {
        for (int i = 0; i < keys.Count; i++)
        {
            if (Input.GetKeyDown(keys[i]))
            {
                btns[i].TryAttack();
            }
        }
    }

    public void OpenNextFriend()
    {
        if (isFriendComplete) return;
        //if(slider.value < nextFriendValue[currentOpenedFriend]) return;
        
        int btnRan = UnityEngine.Random.Range(0, buttonList.Count);
        SkillButton btn = Instantiate(buttonList[btnRan], Vector3.zero, Quaternion.identity);
        btn.transform.SetParent(btnsPos[currentOpenedFriend]);
        btn.transform.localPosition = Vector3.zero;
        btn.transform.localScale = Vector3.one;
        
        int ran = UnityEngine.Random.Range(0, keyPool.Count);
        KeyCode key = keyPool[ran];
        keys.Add(key);
        
        btn.key = keys[currentOpenedFriend];
        
        btn.keyText.text = KeyCodeInString(keys[currentOpenedFriend]).ToString();
        currentOpenedFriend++;

        btns.Add(btn);
        keyPool.Remove(keyPool[ran]);
        buttonList.Remove(buttonList[btnRan]);
        if (currentOpenedFriend >= btnsPos.Length)
        {
            isFriendComplete = true;
        }
    }

    public int KeyCodeInInt(KeyCode key)
    {
        if (key >= KeyCode.A && key <= KeyCode.Z)
            return (int)'A' + (key - KeyCode.A);
        return -1;
    }

    public string KeyCodeInString(KeyCode key)
    {
        if (key >= KeyCode.A && key <= KeyCode.Z)
            return ((char)('A' + (key - KeyCode.A))).ToString();
        return null;
    }
    
    public void StartFill()
    {
        if (fillCoroutine != null)
            StopCoroutine(fillCoroutine);

        fillCoroutine = StartCoroutine(FillSlider());
    }

    private IEnumerator FillSlider()
    {
        slider.value = 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            slider.value = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        slider.value = 1f; // 정확히 1로 맞추기
        OnGamePass();
    }

    public List<KeyCode> GetRandomFriendsKeys(int count = 1)
    {
        List<KeyCode> keyList = this.keys;
        List<KeyCode> returnKeyList = new List<KeyCode>();

        for (int i = 0; i < count; i++)
        {
            int ran = UnityEngine.Random.Range(0, keyList.Count);
            returnKeyList.Add(keyList[ran]);
        }

        return returnKeyList;
    }

    /// <summary>
    /// 게임 실패 처리
    /// </summary>
    public void OnGameFail()
    {
        if (isGameOver) return;

        isGameOver = true;

        Debug.Log("===========================================");
        Debug.Log("[GameManager] GAME FAILED!");
        Debug.Log($"[GameManager] Time: {slider.value * duration:F2}s / {duration}s");
        Debug.Log($"[GameManager] Friends Opened: {currentOpenedFriend} / {btnsPos.Length}");
        Debug.Log("===========================================");

        UIManager.Instance.OpenLosePanel();
        AudioManager.Instance.StopSfxLoop();
        AudioManager.Instance.PlaySfx(Sfx.bgm_lose);
        // Time.timeScale을 0으로 설정하여 게임 정지
        Time.timeScale = 0f;

        Debug.Log("[GameManager] Game stopped (Time.timeScale = 0)");

        // TODO: Fail UI 표시 또는 씬 전환 로직 추가
    }

    /// <summary>
    /// 게임 성공 처리
    /// </summary>
    public void OnGamePass()
    {
        if (isGameOver) return;

        isGameOver = true;

        Debug.Log("===========================================");
        Debug.Log("[GameManager] GAME PASSED!");
        Debug.Log($"[GameManager] Time: {slider.value * duration:F2}s / {duration}s");
        Debug.Log($"[GameManager] Friends Opened: {currentOpenedFriend} / {btnsPos.Length}");
        Debug.Log("===========================================");

        UIManager.Instance.OpenWinPanel();
        AudioManager.Instance.StopSfxLoop();
        AudioManager.Instance.PlaySfx(Sfx.bgm_win);
        // Time.timeScale을 0으로 설정하여 게임 정지
        Time.timeScale = 0f;

        Debug.Log("[GameManager] Game stopped (Time.timeScale = 0)");

        // TODO: Pass UI 표시 또는 씬 전환 로직 추가
    }
}
