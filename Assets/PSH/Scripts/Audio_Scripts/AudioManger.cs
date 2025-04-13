using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioMixer audioMixer;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup sfxGroup;

    private AudioSource bgmSource;
    private AudioSource sfxSource;

    public AudioClip buttonSFX;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 오디오 소스 생성
            bgmSource = gameObject.AddComponent<AudioSource>();
            sfxSource = gameObject.AddComponent<AudioSource>();

            // 믹서 그룹 연결 (에디터에서 드래그)
            bgmSource.outputAudioMixerGroup = musicGroup;
            sfxSource.outputAudioMixerGroup = sfxGroup;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadVolume();
    }

    public void SetMusicVolume(float volume)
    {
        Debug.Log("SetMusicVolume: " + volume);
        if (volume <= 0.0001f)
        {
            audioMixer.SetFloat("Music", -80f); // 음소거처럼 처리
        }
        else
        {
            audioMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
        }
        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (volume <= 0.0001f)
        {
            audioMixer.SetFloat("SFX", -80f);
        }
        else
        {
            audioMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        }
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void LoadVolume()
    {
        float music = PlayerPrefs.GetFloat("musicVolume", 0.75f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        SetMusicVolume(music);
        SetSFXVolume(sfx);
    }

    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource.clip != clip)
        {
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void StopBGM()
    {
        if (bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayButtonSFX()
    {
        PlaySFX(buttonSFX);
    }

    // 4/13 추가
    public void PlayBGMWithFade(AudioClip clip, float duration = 1f)
    {
        StartCoroutine(FadeInBGM(clip, duration));
    }

    
    private IEnumerator FadeInBGM(AudioClip clip, float duration)
    {
        if (bgmSource.isPlaying && bgmSource.clip == clip)
            yield break;

        float currentVolume;
        audioMixer.GetFloat("Music", out currentVolume);
        float startVolume = Mathf.Pow(10, currentVolume / 20);

        bgmSource.clip = clip;
        bgmSource.volume = 0f;
        bgmSource.loop = true;
        bgmSource.Play();

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float newVolume = Mathf.Lerp(0f, startVolume, t / duration);
            bgmSource.volume = newVolume;
            yield return null;
        }

        bgmSource.volume = startVolume;
    }
}