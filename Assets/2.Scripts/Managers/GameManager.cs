using System;
using System.Collections;
using System.Collections.Generic;
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

    private void Update()
    {
        TryInput();
        OpenNextFriend();
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
        if(slider.value < nextFriendValue[currentOpenedFriend]) return;
        
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
        if (currentOpenedFriend >= btnsPos.Length) isFriendComplete = true;
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
    }
}
