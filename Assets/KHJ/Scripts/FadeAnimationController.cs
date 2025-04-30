using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Controls screen fade animations for scene transitions
/// </summary>
public class FadeAnimationController : MonoBehaviour
{
    // Animation component reference
    private Animation imageAnimation;
    
    // Animation clip references
    [SerializeField] private AnimationClip fadeInClip; // Fade-in (screen darkens)
    [SerializeField] private AnimationClip fadeOutClip; // Fade-out (screen brightens)
    
    // Canvas reference
    private Canvas canvas;
    
    // Image that the fade effect will be applied to
    [SerializeField] private Image fadeImage;
    
    // Callback to be invoked after animation completion
    public delegate void FadeAnimationComplete();
    private FadeAnimationComplete onComplete;
    
    void Awake()
    {
        // Get Canvas component
        canvas = GetComponent<Canvas>();
        
        if (!canvas)
        {
            Debug.LogError("Cannot find Canvas component!");
        }
        
        // Check fade image
        if (!fadeImage)
        {
            Debug.LogError("Fade Image is not assigned!");
        }
        else
        {
            // Get Animation component from Image
            imageAnimation = fadeImage.GetComponent<Animation>();
            
            if (!imageAnimation)
            {
                Debug.LogError("Cannot find Animation component on Image!");
                return;
            }
            
            // Add animation clips to Animation component
            if (fadeInClip)
            {
                imageAnimation.AddClip(fadeInClip, "FadeIn");
            }
            else
            {
                Debug.LogError("FadeIn animation clip is not assigned!");
            }
            
            if (fadeOutClip)
            {
                imageAnimation.AddClip(fadeOutClip, "FadeOut");
            }
            else
            {
                Debug.LogError("FadeOut animation clip is not assigned!");
            }
            
            // Prevent automatic playback
            imageAnimation.playAutomatically = false;
        }
    }

    void Start()
    {
        // Check if Canvas is a root GameObject
        if (transform.parent)
        {
            Debug.LogWarning("Canvas is not a root GameObject. It may not persist between scene transitions.");
        }
        
        // Make this object persist between scene loads
        DontDestroyOnLoad(gameObject);
    }
    
    /// <summary>
    /// Plays fade animation (fadeIn=true for fade-in, false for fade-out)
    /// </summary>
    /// <param name="fadeIn">Direction of fade animation</param>
    /// <param name="callback">Optional callback after animation completes</param>
    public void PlayFadeAnimation(bool fadeIn, FadeAnimationComplete callback = null)
    {
        if (!imageAnimation || !fadeImage) return;

        // Store callback
        onComplete = callback;
        
        // Set Canvas sorting order
        if (canvas)
        {
            canvas.sortingOrder = 2;
        }
        
        // Select and play animation
        string clipName = fadeIn ? "FadeIn" : "FadeOut";
        imageAnimation.Play(clipName);
        
        // Start coroutine to wait for animation completion
        AnimationClip currentClip = fadeIn ? fadeInClip : fadeOutClip;
        StartCoroutine(WaitForAnimationComplete(currentClip));
    }
    
    /// <summary>
    /// Waits for animation to complete and then invokes callback
    /// </summary>
    private IEnumerator WaitForAnimationComplete(AnimationClip clip)
    {
        // Wait for the duration of the animation
        if (clip)
        {
            yield return new WaitForSeconds(clip.length);
        }
        else
        {
            yield return new WaitForSeconds(1.0f); // Default 1 second wait
        }
        
        // Execute callback
        if (onComplete != null)
        {
            onComplete();
            onComplete = null; // Reset callback
        }
    }
}