using System;
using UnityEngine;

public class FadeAnimationController : MonoBehaviour
{
    // Animation 컴포넌트 참조
    private Animation animation;
    
    // 애니메이션 클립 참조
    [SerializeField] private AnimationClip fadeInClip;
    [SerializeField] private AnimationClip fadeOutClip;
    [SerializeField] private Canvas canvas;
    
    void Awake()
    {
        // Animation 컴포넌트 가져오기
        animation = GetComponent<Animation>();
        
        if (!animation)
        {
            Debug.LogError("Animation 컴포넌트를 찾을 수 없습니다!");
            return;
        }
        
        // 애니메이션 클립을 Animation 컴포넌트에 추가
        if (fadeInClip)
        {
            animation.AddClip(fadeInClip, "FadeIn");
        }
        
        if (fadeOutClip)
        {
            animation.AddClip(fadeOutClip, "FadeOut");
        }
        
        // 자동 재생 방지
        animation.playAutomatically = false;
    }

    public void PlayFadeAnimation(bool isFadeIn)
    {
        if (!animation) return;

        if (canvas)
        {
            canvas.sortingOrder = 2;
        }
        
        // 이미 재생 중인 애니메이션 중지
        animation.Stop();
        
        // 선택한 애니메이션 재생
        string clipName = isFadeIn ? "FadeIn" : "FadeOut";
        animation.Play(clipName);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
