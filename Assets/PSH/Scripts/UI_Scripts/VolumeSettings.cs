using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;


// MainMenuScene 에서만 사용 (오프닝 음악 관리)
public class VolumeSettings : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager.Instance is null!");
            return;
        }

        // 저장된 볼륨 값 불러오기
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume", 0.75f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);



        // 슬라이더 이벤트 리스너 설정
        musicSlider.onValueChanged.AddListener((v) => AudioManager.Instance.SetMusicVolume(v));
        sfxSlider.onValueChanged.AddListener((v) => AudioManager.Instance.SetSFXVolume(v));
    }
}