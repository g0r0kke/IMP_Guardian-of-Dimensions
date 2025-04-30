using UnityEngine;

/// <summary>
/// Controls hit/damage animation effects
/// </summary>
public class HitAnimationController : MonoBehaviour
{
    [SerializeField] private Animation imageAnimation; // Animation component
    [SerializeField] private AnimationClip hitAnimClip; // Hit animation clip
    [SerializeField] private string hitAnimName = "Hit"; // Animation name
    
    private void Awake()
    {
        // Find Animation component if not assigned
        if (imageAnimation == null)
        {
            imageAnimation = GetComponent<Animation>();
            if (imageAnimation == null)
            {
                Debug.LogError("HitAnimationController: No Animation component found");
                return;
            }
        }
        
        // Add animation clip if not already added
        if (hitAnimClip != null && !imageAnimation.GetClip(hitAnimName))
        {
            imageAnimation.AddClip(hitAnimClip, hitAnimName);
        }
    }
    
    /// <summary>
    /// Plays the hit animation effect
    /// </summary>
    public void PlayHitAnimation()
    {
        if (!imageAnimation)
        {
            Debug.LogError("HitAnimationController: Animation component is null");
            return;
        }
        
        if (!imageAnimation.GetClip(hitAnimName))
        {
            Debug.LogError("HitAnimationController: Hit animation clip is not assigned");
            return;
        }
        
        // Stop any currently playing animation
        imageAnimation.Stop();
        
        // Play the hit animation
        imageAnimation.Play(hitAnimName);
    }
}