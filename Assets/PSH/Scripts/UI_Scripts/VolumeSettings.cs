using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;


// Used only in MainMenuScene (Manages opening music)
public class VolumeSettings : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider musicSlider; // Music volume control slider
    public Slider sfxSlider;  // SFX volume control slider

    private void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager.Instance is null!");
            return;
        }

        // Load saved volume values
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume", 0.75f); 
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);



        // Set slider event listeners
        musicSlider.onValueChanged.AddListener((v) => AudioManager.Instance.SetMusicVolume(v)); // Update music volume when slider value changes
        sfxSlider.onValueChanged.AddListener((v) => AudioManager.Instance.SetSFXVolume(v)); // Update SFX volume when slider value changes
    }
}