using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public List<SoundEffect> SFXList = new();
    private static Dictionary<SFXTYPE, AudioClip[]> SFXDictionary = new();
    private static AudioManager instance;
    public AudioSource SFXSource;

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
    public static void PlaySFX(SFXTYPE type, float volume = 1f, int variantIndex = -1)
    {
        if (!SFXDictionary.ContainsKey(type) || instance.SFXSource == null) return;

        // Get sfx from dictionary
        AudioClip[] audioClips = SFXDictionary[type];
        AudioClip clipChosen = audioClips[Random.Range(0, audioClips.Length)];
        instance.SFXSource.PlayOneShot(clipChosen, volume);
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
    PLAYER_HURT
}

