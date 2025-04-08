using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioMixer audioMixer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 씬 전환되어도 바뀌지 않고 살아있음
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 시작할 때 볼륨 불러옴 (종료해도 그 볼륨 그대로)
        LoadVolume();
    }

    public void SetMusicVolume(float volume)
    {
        // 슬라이더 움직였을때 
        Debug.Log("SetMusicVolume: " + volume);
        
        audioMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void LoadVolume()
    {
        float music = PlayerPrefs.GetFloat("musicVolume", 0.75f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        SetMusicVolume(music);
        SetSFXVolume(sfx);
    }

    // 다른씬에서 오디오 음악 바꾸기 위한 함수 
    public void PlayBGM(AudioClip clip)
    {
        AudioSource bgmSource = GetComponent<AudioSource>();
        if (bgmSource.clip != clip)
        {
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }
}
