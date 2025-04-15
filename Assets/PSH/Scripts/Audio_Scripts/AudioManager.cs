using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioMixer audioMixer;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup sfxGroup;

    private AudioSource bgmSource;
    private AudioSource sfxSource;
    public AudioClip buttonSFX;

    // 씬별 BGM 매핑
    [System.Serializable]
    public class SceneBGM
    {
        public string sceneName;
        public AudioClip bgmClip;
    }

    public List<SceneBGM> sceneBGMs = new List<SceneBGM>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // 오디오 소스 생성
            bgmSource = gameObject.AddComponent<AudioSource>();
            sfxSource = gameObject.AddComponent<AudioSource>();
            // 믹서 그룹 연결
            bgmSource.outputAudioMixerGroup = musicGroup;
            sfxSource.outputAudioMixerGroup = sfxGroup;

            LoadVolume();


            // 씬 변경 이벤트 리스너 추가
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
        // 씬 변경 시 해당 씬에 맞는 BGM 재생
        string sceneName = scene.name;
        AudioClip clipToPlay = null;

        foreach (SceneBGM item in sceneBGMs)
        {
            if (item.sceneName == sceneName && item.bgmClip != null)
            {
                clipToPlay = item.bgmClip;
                break;
            }
        }

        if (clipToPlay != null)
        {
            PlayBGMWithFade(clipToPlay, 0.5f);
        }

        // 현재 씬의 모든 AudioSource를 찾아서 설정
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in allAudioSources)
        {
            // AudioManager 자신의 소스는 제외
            if (source != bgmSource && source != sfxSource)
            {
            // 루프되지 않는 소스는 SFX로 간주 (효과음)
            if (!source.loop)
            {
                source.outputAudioMixerGroup = sfxGroup;
            }
        }
    }

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
        Debug.Log("PlayBGM called with: " + (clip != null ? clip.name : "NULL"));
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
            Debug.Log("Playing SFX: " + clip.name);
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("SFX clip is null!");
        }
    }

    public void PlayButtonSFX()
    {
        Debug.Log("PlayButtonSFX called");
        PlaySFX(buttonSFX);
    }

   


    public void PlayBGMWithFade(AudioClip clip, float duration = 0.4f)
    {
        StartCoroutine(FadeInBGM(clip, duration));
    }

    private IEnumerator FadeInBGM(AudioClip clip, float duration)
    {
        Debug.Log("FadeInBGM called with: " + (clip != null ? clip.name : "NULL"));
        if (bgmSource.isPlaying && bgmSource.clip == clip)
            yield break;

        // 원래 볼륨 값을 저장
        float musicVolume;
        audioMixer.GetFloat("Music", out musicVolume);
        float targetDBVolume = musicVolume;

        // 현재 음악 페이드아웃
        if (bgmSource.isPlaying)
        {
            float startTime = Time.time;
            float endTime = startTime + duration;

            while (Time.time < endTime)
            {
                float t = (Time.time - startTime) / duration;
                float volumeFactor = 1 - t;
                audioMixer.SetFloat("Music", Mathf.Lerp(targetDBVolume, -80f, t));
                yield return null;
            }
        }

        // 기존 음악 정지
        bgmSource.Stop();

        // 새로운 음악 설정
        bgmSource.clip = clip;
        bgmSource.loop = true;
        audioMixer.SetFloat("Music", -80f); // 완전 음소거 상태에서 시작
        bgmSource.Play();

        // 새로운 음악 페이드인
        float fadeInStart = Time.time;
        float fadeInEnd = fadeInStart + duration;

        while (Time.time < fadeInEnd)
        {
            float t = (Time.time - fadeInStart) / duration;
            audioMixer.SetFloat("Music", Mathf.Lerp(-80f, targetDBVolume, t));
            yield return null;
        }

        // 최종 볼륨 설정
        audioMixer.SetFloat("Music", targetDBVolume);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }




}