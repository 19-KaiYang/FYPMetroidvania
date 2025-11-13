using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public List<SoundEffect> SFXList = new();
    private static Dictionary<SFXTYPE, AudioClip[]> SFXDictionary = new();
    public List<BGM> BGMList = new();
    private static Dictionary<BGMType, BGM> BGMDictionary = new(); 
    public static AudioManager instance;
    public AudioSource SFXSource;
    public AudioSource BGMSource;
    public bool isPlayingBGM;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        foreach (var sfx in SFXList)
        {
            if (!SFXDictionary.ContainsKey(sfx.key))
            {
                SFXDictionary.Add(sfx.key, sfx.clips);
            }
        }
        foreach (var bgm in BGMList)
        {
            if (!BGMDictionary.ContainsKey(bgm.key))
            {
                BGMDictionary.Add(bgm.key, bgm);
            }
        }
        isPlayingBGM = false;

        if (SFXSource != null)
        {
            SFXSource.ignoreListenerPause = true;
        }

    }
    private void Update()
    {
    }
    public static void PlaySFX(SFXTYPE type, float volume = 1f, int variantIndex = -1, float pitch = 1f)
    {
        if (!SFXDictionary.ContainsKey(type) || instance.SFXSource == null) return;

        // Get sfx from dictionary
        AudioClip[] audioClips = SFXDictionary[type];
        AudioClip clipChosen = audioClips[Random.Range(0, audioClips.Length)];
        instance.SFXSource.pitch = pitch;
        instance.SFXSource.PlayOneShot(clipChosen,volume);
    }
    public void PlayBGM(AudioClip song)
    {
        BGMSource.clip = song;
        //BGMSource.volume = volume;  
        BGMSource.Play();
    }
    public void PlayBGM(BGMType type)
    {
        if (!BGMDictionary.ContainsKey(type) || instance.BGMSource == null) return;
        StopBGM();
        BGM bgm = BGMDictionary[type];
        BGMSource.clip = bgm.audio;
        BGMSource.volume = bgm.volume;
        BGMSource.Play();
        isPlayingBGM = true;
    }
    public void PlayBGM(string keyname)
    {
        BGMType type = (BGMType)Enum.Parse(typeof(BGMType), keyname);
        if (!BGMDictionary.ContainsKey(type) || instance.BGMSource == null) return;

        BGM bgm = BGMDictionary[type];
        BGMSource.clip = bgm.audio;
        BGMSource.volume = bgm.volume;
        BGMSource.Play();
        isPlayingBGM = true;
    }
    public void StopBGM()
    {
        BGMSource.Stop();
        isPlayingBGM = false;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        switch (scene.name)
        {
            case "Goblin Camp":
                PlayBGM(BGMType.OPENING_CUTSCENE);
                break;

            case "Room1":
                PlayBGM(BGMType.TOWN_COMBAT);
                break;

            case "Room2":
                PlayBGM(BGMType.SECOND_ROOM);
                break;

            case "Room3":
                PlayBGM(BGMType.THIRD_ROOM);
                break;

            case "BossRoom": 
                PlayBGM(BGMType.BOSS_ROOM);
                break;

            case "MainMenu":
                StopBGM();
                break;

            default:
                StopBGM();
                break;
        }
    }

    public IEnumerator FadeToBGM(BGMType newBGM, float fadeTime = 1f)
    {
        float startVol = BGMSource.volume;
        while (BGMSource.volume > 0f)
        {
            BGMSource.volume -= startVol * Time.deltaTime / fadeTime;
            yield return null;
        }

        PlayBGM(newBGM);

        BGMSource.volume = 0f;
        while (BGMSource.volume < startVol)
        {
            BGMSource.volume += startVol * Time.deltaTime / fadeTime;
            yield return null;
        }
    }


}

[Serializable]
public class SoundEffect
{
    public SFXTYPE key;
    public AudioClip[] clips;
}
public enum SFXTYPE
{
    NONE,
    PLAYER_JUMP,
    PLAYER_DASH,
    PLAYER_LAND,
    PLAYER_FOOTSTEP,
    PHYSICAL_HIT,
    SWORD_SWING,
    SWORD_LIGHTHIT,
    SWORD_HEAVYHIT,
    SWORD_DASH,
    SWORD_UPPERCUT,
    SWORD_PROJECTILE,
    UPGRADE_POPUP,
    PLAYER_HURT,
    DIALOGUE_1,
    BRAWLER_ATTACK,
    BRALWER_CHARGE,
    SPEARMAN_ATTACK,
    SPEARMAN_CHARGE,
    SPEARMAN_THROW,
    ENEMY_ATTACKFLASH,
    HAWK_ATTACK,
    TRUCK_HORN,
    IMPACT,
    BARREL_BREAK,
    HEALING,
    SPIRIT_POTIONSFX,
    SPIRIT_CUTINSFX,
    REVVING
}
[Serializable]
public class BGM
{ 
    public BGMType key;
    public AudioClip audio;
    public float volume = 1f;
}
public enum BGMType
{
    OPENING_CUTSCENE,
    TOWN_COMBAT,
    BOSS_PHASE1,
    BOSS_PHASE2,
    SECOND_ROOM,
    THIRD_ROOM,
    BOSS_ROOM   
}


