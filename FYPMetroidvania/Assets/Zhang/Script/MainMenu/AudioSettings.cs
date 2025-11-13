using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Slider MasterSlider;
    [SerializeField] Slider SFXSlider;
    [SerializeField] Slider BGMSlider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MasterSlider.value = PlayerPrefs.GetFloat("Master", 10f);
        SFXSlider.value = PlayerPrefs.GetFloat("BGM", 10f);
        BGMSlider.value = PlayerPrefs.GetFloat("SFX", 10f);

        SetMasterVolume();
        SetBGMVolume();
        SetSFXVolume();
    }

    private void OnDisable()
    {
        MasterSlider.value = PlayerPrefs.GetFloat("Master", 10f);
        SFXSlider.value = PlayerPrefs.GetFloat("BGM", 10f);
        BGMSlider.value = PlayerPrefs.GetFloat("SFX", 10f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMasterVolume()
    {
        float volume = (MasterSlider.value * 4 - 40);

        if (volume <= -40)
        {
            volume = -80;
        }

        audioMixer.SetFloat("Master", volume);
        PlayerPrefs.SetFloat("Master", MasterSlider.value);
    }

    public void SetBGMVolume()
    {
        float volume = (BGMSlider.value * 4 - 40);

        if (volume <= -40)
        {
            volume = -80;
        }

        audioMixer.SetFloat("BGM", volume);
        PlayerPrefs.SetFloat("BGM", BGMSlider.value);
    }

    public void SetSFXVolume()
    {
        float volume = (SFXSlider.value * 4 - 40);

        if (volume <= -40)
        {
            volume = -80;
        }

        audioMixer.SetFloat("SFX", volume);
        PlayerPrefs.SetFloat("SFX", SFXSlider.value);
    }
}
