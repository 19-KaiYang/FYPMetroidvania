using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public struct SFXTask
{
    public AudioClip clip;
    public float volume;
    public float pitch;
    public SFXTask(AudioClip clip, float volume, float pitch)
    {
        this.clip = clip;
        this.volume = volume;
        this.pitch = pitch;
    }
}

public class AudioManager : MonoBehaviour
{
    public List<SoundEffect> SFXList = new();
    private static Dictionary<SFXTYPE, AudioClip[]> SFXDictionary = new();
    public static AudioManager instance;
    public AudioSource SFXSource;
    public AudioSource BGMSource;
    public List<SFXTask> sfxTasks = new();

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
    }
    private void Update()
    {
        //while (sfxTasks.Count > 0)
        //{
        //    instance.SFXSource.pitch = sfxTasks[0].pitch;
        //    instance.SFXSource.PlayOneShot(sfxTasks[0].clip, sfxTasks[0].volume);
        //    sfxTasks.RemoveAt(0);
        //}
    }
    public static void PlaySFX(SFXTYPE type, float volume = 1f, int variantIndex = -1, float pitch = 1f)
    {
        if (!SFXDictionary.ContainsKey(type) || instance.SFXSource == null) return;

        // Get sfx from dictionary
        AudioClip[] audioClips = SFXDictionary[type];
        AudioClip clipChosen = audioClips[Random.Range(0, audioClips.Length)];
        instance.SFXSource.pitch = pitch;
        instance.SFXSource.PlayOneShot(clipChosen,volume);
        //SFXTask newTask = new SFXTask(clipChosen, volume, pitch);
        //instance.sfxTasks.Add(newTask);
    }
    public void PlayBGM(AudioClip song)
    {
        BGMSource.clip = song;
        //BGMSource.volume = volume;  
        BGMSource.Play();
    }
    public void StopBGM()
    {
        BGMSource.Stop();
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
    PHYSICAL_HIT,
    SWORD_SWING,
    SWORD_LIGHTHIT,
    SWORD_HEAVYHIT,
    DIALOGUE_1
}

