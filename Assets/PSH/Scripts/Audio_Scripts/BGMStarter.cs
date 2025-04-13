using UnityEngine;


// Boss1Scene, Boss2Scene에서 사용할 배경음악 
public class BGMStarter : MonoBehaviour
{
    public AudioClip bgmClip;

    void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(bgmClip);
        }
    }
}