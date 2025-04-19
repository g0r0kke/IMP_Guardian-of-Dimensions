using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    public AudioClip clickSound;


    void Start()
    {
        // 시작할 때 clickSound가 할당되어 있는지 확인
        if (clickSound == null)
        {
            Debug.LogWarning(gameObject.name + "의 ButtonSound에 clickSound가 할당되지 않았습니다!");
        }
    }

    public void PlayClickSound()
    {
        Debug.Log(gameObject.name + "의 PlayClickSound 호출됨");

        if (clickSound != null)
        {
            if (AudioManager.Instance != null)
            {
                Debug.Log("AudioManager.Instance 존재함, 소리 재생 시도");
                AudioManager.Instance.PlaySFX(clickSound);
            }
            else
            {
                Debug.LogError("AudioManager.Instance가 null입니다!");
            }
        }
        else
        {
            Debug.LogWarning("clickSound가 null입니다!");
        }
    }
    }
