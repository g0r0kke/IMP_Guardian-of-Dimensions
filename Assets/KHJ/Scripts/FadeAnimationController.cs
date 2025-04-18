using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeAnimationController : MonoBehaviour
{
    // Animation 컴포넌트 참조
    private Animation imageAnimation;
    
    // 애니메이션 클립 참조
    [SerializeField] private AnimationClip fadeInClip;  // 페이드인(화면이 어두워지는) 애니메이션
    [SerializeField] private AnimationClip fadeOutClip; // 페이드아웃(화면이 밝아지는) 애니메이션
    
    // 캔버스
    private Canvas canvas;
    
    // 페이드 효과가 적용될 이미지
    [SerializeField] private Image fadeImage;
    
    // 애니메이션 완료 후 호출될 콜백
    public delegate void FadeAnimationComplete();
    private FadeAnimationComplete onComplete;
    
    void Awake()
    {
        // Canvas 컴포넌트 가져오기
        canvas = GetComponent<Canvas>();
        
        if (!canvas)
        {
            Debug.LogError("Canvas 컴포넌트를 찾을 수 없습니다!");
        }
        
        // 페이드 이미지 확인
        if (!fadeImage)
        {
            Debug.LogError("Fade Image가 할당되지 않았습니다!");
        }
        else
        {
            // Image의 Animation 컴포넌트 가져오기
            imageAnimation = fadeImage.GetComponent<Animation>();
            
            if (!imageAnimation)
            {
                Debug.LogError("Image에 Animation 컴포넌트를 찾을 수 없습니다!");
                return;
            }
            
            // 애니메이션 클립을 Animation 컴포넌트에 추가
            if (fadeInClip)
            {
                imageAnimation.AddClip(fadeInClip, "FadeIn");
            }
            else
            {
                Debug.LogError("FadeIn 애니메이션 클립이 할당되지 않았습니다!");
            }
            
            if (fadeOutClip)
            {
                imageAnimation.AddClip(fadeOutClip, "FadeOut");
            }
            else
            {
                Debug.LogError("FadeOut 애니메이션 클립이 할당되지 않았습니다!");
            }
            
            // 자동 재생 방지
            imageAnimation.playAutomatically = false;
        }
    }

    void Start()
    {
        // Canvas가 루트 게임오브젝트인지 확인
        if (transform.parent)
        {
            Debug.LogWarning("Canvas가 루트 게임오브젝트가 아닙니다. 씬 전환 시 유지되지 않을 수 있습니다.");
        }
        
        DontDestroyOnLoad(gameObject);
    }
    
    // 페이드 애니메이션 재생 (fadeIn이 true면 페이드인, false면 페이드아웃)
    public void PlayFadeAnimation(bool fadeIn, FadeAnimationComplete callback = null)
    {
        if (!imageAnimation || !fadeImage) return;

        // 콜백 저장
        onComplete = callback;
        
        // Canvas의 sortingOrder 설정
        if (canvas)
        {
            canvas.sortingOrder = 2;
        }
        
        // 이미 재생 중인 애니메이션 중지
        imageAnimation.Stop();
        
        // 선택한 애니메이션 재생
        string clipName = fadeIn ? "FadeIn" : "FadeOut";
        imageAnimation.Play(clipName);
        
        // 애니메이션 완료 대기 코루틴 시작
        AnimationClip currentClip = fadeIn ? fadeInClip : fadeOutClip;
        StartCoroutine(WaitForAnimationComplete(currentClip));
    }
    
    private IEnumerator WaitForAnimationComplete(AnimationClip clip)
    {
        // 애니메이션 지속 시간만큼 대기
        if (clip)
        {
            yield return new WaitForSeconds(clip.length);
        }
        else
        {
            yield return new WaitForSeconds(1.0f); // 기본 1초 대기
        }
        
        // 콜백 실행
        if (onComplete != null)
        {
            onComplete();
            onComplete = null; // 콜백 초기화
        }
    }
}