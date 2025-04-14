using UnityEngine;


// Boss1Scene, Boss2Scene에서 사용할 배경음악 
public class BGMStarter : MonoBehaviour
{
    public AudioClip bgmClip;

    void Start()
    {
        Debug.Log("[Boss1] BGMStarter 실행됨");
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBGM(); 
            AudioManager.Instance.PlayBGMWithFade(bgmClip, 1f);
        }
    }
}