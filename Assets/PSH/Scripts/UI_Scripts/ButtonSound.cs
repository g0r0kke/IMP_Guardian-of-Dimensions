using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    public AudioClip clickSound;

    public void PlayClickSound()
    {
        if (clickSound != null)
            AudioManager.Instance.PlaySFX(clickSound);
    }
}
