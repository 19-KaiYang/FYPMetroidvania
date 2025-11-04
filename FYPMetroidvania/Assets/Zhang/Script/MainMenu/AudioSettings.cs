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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void setMasterVolume()
    {
        audioMixer.SetFloat("Master", MasterSlider.value);
    }
    public void setBGMVolume()
    {
        audioMixer.SetFloat("Music", BGMSlider.value);
    }
    public void setSFXVolume()
    {
        audioMixer.SetFloat("SFX", SFXSlider.value);
    }
}
