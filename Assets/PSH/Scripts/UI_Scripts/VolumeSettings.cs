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

        // 4/13 추가 , 메인 메뉴 배경음악 재생 (부드럽게 페이드인)
        AudioClip mainMenuBGM = Resources.Load<AudioClip>("Audio/MainMenuTheme");
        AudioManager.Instance.PlayBGMWithFade(mainMenuBGM, 1.5f);


    }
}