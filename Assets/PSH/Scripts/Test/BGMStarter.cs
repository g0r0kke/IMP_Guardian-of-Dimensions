using UnityEngine;


// Background music to be used in Boss1Scene and Boss2Scene 
public class BGMStarter : MonoBehaviour
{
    public AudioClip bgmClip;

    void Start()
    {
        Debug.Log("[Boss1] BGMStarter 실행됨");
        if (AudioManager.Instance != null)
        {
            // Stop any currently playing background music
            AudioManager.Instance.StopBGM(); 

            // Play the new background music with a fade-in effect
            AudioManager.Instance.PlayBGMWithFade(bgmClip, 1f); // 새로운 배경음악을 페이드 인 효과와 함께 재생
        }
    }
}