using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioMixer audioMixer;  // Audio mixer
    public AudioMixerGroup musicGroup; // Music mixer group
    public AudioMixerGroup sfxGroup; // SFX mixer group

    private AudioSource bgmSource; // Audio source for background music
    private AudioSource sfxSource; // Audio source for sound effects
    public AudioClip buttonSFX; // Button click sound effect

    // Scene-specific BGM mapping
    [System.Serializable]
    public class SceneBGM
    {
        public string sceneName; // Scene name
        public AudioClip bgmClip; // Background music for the scene
    }

    public List<SceneBGM> sceneBGMs = new List<SceneBGM>(); // List of BGM for each scene

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Don't destroy the audio manager on scene transitions
            // Create audio sources
            bgmSource = gameObject.AddComponent<AudioSource>();
            sfxSource = gameObject.AddComponent<AudioSource>();
            // Connect to mixer groups
            bgmSource.outputAudioMixerGroup = musicGroup;
            sfxSource.outputAudioMixerGroup = sfxGroup;

            LoadVolume(); // Load saved volume settings


            // Add scene change event listener
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject); // Destroy if instance already exists
            return;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Play the corresponding BGM for the scene when it's loaded
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

        // Play the BGM with fade effect if available
        if (clipToPlay != null)
        {
            PlayBGMWithFade(clipToPlay, 0.5f);
        }

        // Find and configure all AudioSources in the current scene
        AudioSource[] allAudioSources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource source in allAudioSources)
        {
            // Exclude the AudioManager itself
            if (source != bgmSource && source != sfxSource)
            {
                // Consider non-looping sources as SFX (sound effects)
                if (!source.loop)
                {
                    source.outputAudioMixerGroup = sfxGroup;
                }
            }
        }
    }

    // Set the music volume
    public void SetMusicVolume(float volume)
    {
        Debug.Log("SetMusicVolume: " + volume);
        if (volume <= 0.0001f)
        {
            audioMixer.SetFloat("Music", -80f);  // Mute the music
        }
        else
        {
            audioMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
        }
        PlayerPrefs.SetFloat("musicVolume", volume); // Save volume setting
    }

    // Set the SFX volume
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
        PlayerPrefs.SetFloat("SFXVolume", volume); // Save volume setting
    }

    // Load saved volume settings
    public void LoadVolume()
    {
        float music = PlayerPrefs.GetFloat("musicVolume", 0.75f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        SetMusicVolume(music);
        SetSFXVolume(sfx);
    }

    // Play background music
    public void PlayBGM(AudioClip clip)
    {
        Debug.Log("PlayBGM called with: " + (clip != null ? clip.name : "NULL"));
        if (bgmSource.clip != clip)
        {
            bgmSource.clip = clip;
            bgmSource.loop = true; // Loop background music
            bgmSource.Play();
        }
    }

    // Stop background music
    public void StopBGM()
    {
        if (bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    // Play sound effect
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            Debug.Log("Playing SFX: " + clip.name);
            sfxSource.PlayOneShot(clip); // Play only once
        }
        else
        {
            Debug.LogWarning("SFX clip is null!");
        }
    }

    // Play button click sound effect
    public void PlayButtonSFX()
    {
        Debug.Log("PlayButtonSFX called");
        PlaySFX(buttonSFX);
    }

    // Play background music with fade-in effect
    public void PlayBGMWithFade(AudioClip clip, float duration = 0.4f)
    {
        StartCoroutine(FadeInBGM(clip, duration));
    }

    // Coroutine for fade-in effect of background music
    private IEnumerator FadeInBGM(AudioClip clip, float duration)
    {
        Debug.Log("FadeInBGM called with: " + (clip != null ? clip.name : "NULL"));
        if (bgmSource.isPlaying && bgmSource.clip == clip)
            yield break;

        // Save the original volume value
        float musicVolume;
        audioMixer.GetFloat("Music", out musicVolume);
        float targetDBVolume = musicVolume;

        // Fade out the current music
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

        // Stop the current music
        bgmSource.Stop();

        // Set the new music
        bgmSource.clip = clip;
        bgmSource.loop = true;
        audioMixer.SetFloat("Music", -80f); // Start with muted state
        bgmSource.Play();

        // Fade in the new music
        float fadeInStart = Time.time;
        float fadeInEnd = fadeInStart + duration;

        while (Time.time < fadeInEnd)
        {
            float t = (Time.time - fadeInStart) / duration;
            audioMixer.SetFloat("Music", Mathf.Lerp(-80f, targetDBVolume, t));
            yield return null;
        }

        // Set final volume
        audioMixer.SetFloat("Music", targetDBVolume);
    }

    // Remove scene loading event listener when object is destroyed
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
