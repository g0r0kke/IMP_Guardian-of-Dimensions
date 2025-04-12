using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start()
    {
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume", 0.75f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        musicSlider.onValueChanged.AddListener((v) => AudioManager.Instance.SetMusicVolume(v));
        sfxSlider.onValueChanged.AddListener((v) => AudioManager.Instance.SetSFXVolume(v));
    
    
    }
}