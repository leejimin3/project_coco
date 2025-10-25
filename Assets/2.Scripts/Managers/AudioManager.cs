using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Sfx
{
    coco_sound1,
    coco_sound2,
    coco_sound3,
    
    mell_success,
    miew_success,
    adwd_success,
    toto_success,
    
    mell_failed,
    miew_failed,
    adwd_failed,
    toto_failed,
    
    key_success,
    key_failed,
    
    bgm_ingame,
    bgm_win,
    bgm_lose,
    
    cutScene1,
    cutScene2,
    cutScene3,
    cutScene4,
    cutScene5,
    cutScene6,
    bgm_cutScene,
}

[Serializable]
public class ClipData
{
    public AudioClip clip;
    public float volume = 1f;
}

public enum OptionType
{
    BGM_OnOff,
    SFX_OnOff,
    Vibrate_OnOff,
}

public enum SfxPlayMode
{
    Single,
    Random,
    Sequence
}

public class AudioManager : BaseSingleton<AudioManager>
{
    [Header("# SFX Data")]
    public SerializedDictionary<Sfx, ClipData[]> data;

    [Header("# BGM")]
    [SerializeField] private AudioClip[] bgms;
    [SerializeField] private float[] bgmVolumes;
    private AudioSource bgmPlayer;

    [Header("# SFX")]
    private AudioSource sfxPlayer;
    private AudioSource loopSfxPlayer;

    private bool isBgmOn;
    private bool isSfxOn;
    private bool isVibOn;

    protected override void Awake()
    {
        base.Awake();

        SceneManager.sceneLoaded += OnSceneLoaded;
        Init();
    }

    private void Init()
    {
        InitializeBGM();
        InitializeSFX();

        LoadOptions();
    }

    private void InitializeBGM()
    {
        var bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
    }

    private void InitializeSFX()
    {
        var sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayer = sfxObject.AddComponent<AudioSource>();
        sfxPlayer.playOnAwake = false;

        loopSfxPlayer = sfxObject.AddComponent<AudioSource>();
        loopSfxPlayer.loop = true;
    }

    public void PlaySfx(Sfx sfx, SfxPlayMode mode = SfxPlayMode.Single, int idx = 0)
    {
        if (!isSfxOn) return;
        if (!data.TryGetValue(sfx, out var clips) || clips.Length <= 0) return;

        if (mode == SfxPlayMode.Random)
        {
            int randIndex = clips.Length > 1 ? UnityEngine.Random.Range(0, clips.Length) : 0;
            sfxPlayer.PlayOneShot(clips[randIndex].clip, clips[randIndex].volume);
        }
        else if (mode == SfxPlayMode.Sequence)
        {
            int Index = clips.Length > 1 ? Mathf.Min(idx, clips.Length-1) : 0;
            sfxPlayer.PlayOneShot(clips[Index].clip, clips[Index].volume);
        }
        else
        {
            sfxPlayer.PlayOneShot(clips[0].clip, clips[0].volume);
        }
    }

    public void PlaySfxLoop(Sfx sfx)
    {
        if (loopSfxPlayer.isPlaying) return;

        if (data.TryGetValue(sfx, out var clips) && clips.Length > 0)
        {
            loopSfxPlayer.clip = clips[0].clip;
            loopSfxPlayer.volume = clips[0].volume;
            loopSfxPlayer.Play();
        }
    }

    public void StopSfxLoop()
    {
        if (loopSfxPlayer.isPlaying)
        {
            loopSfxPlayer.Stop();
        }
    }

    public void PlayBgm(bool isPlay)
    {
        if (isPlay)
        {
            bgmPlayer.Play();
        }
        else if (bgmPlayer.isPlaying)
        {
            bgmPlayer.Stop();
        }
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        int sceneIndex = scene.buildIndex;
        if (sceneIndex < bgms.Length && bgms[sceneIndex] != null)
        {
            bgmPlayer.clip = bgms[sceneIndex];
            bgmPlayer.volume = bgmVolumes[sceneIndex];
            PlayBgm(isBgmOn);
        }
    }

    public void SetOptionState(OptionType type, bool isOn)
    {
        switch (type)
        {
            case OptionType.BGM_OnOff:
                isBgmOn = isOn;
                PlayBgm(isBgmOn);
                break;
            case OptionType.SFX_OnOff:
                isSfxOn = isOn;
                break;
            case OptionType.Vibrate_OnOff:
                isVibOn = isOn;
                break;
        }

        PlayerPrefs.SetInt(type.ToString(), isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadOptions()
    {
        isBgmOn = PlayerPrefs.GetInt(OptionType.BGM_OnOff.ToString(), 1) == 1;
        isSfxOn = PlayerPrefs.GetInt(OptionType.SFX_OnOff.ToString(), 1) == 1;
        isVibOn = PlayerPrefs.GetInt(OptionType.Vibrate_OnOff.ToString(), 1) == 1;

        SetOptionState(OptionType.BGM_OnOff, isBgmOn);
        SetOptionState(OptionType.SFX_OnOff, isSfxOn);
        SetOptionState(OptionType.Vibrate_OnOff, isVibOn);
    }
}
