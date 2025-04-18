using UnityEngine;

public class HitAnimationController : MonoBehaviour
{
    [SerializeField] private Animation imageAnimation; // Animation 컴포넌트
    [SerializeField] private AnimationClip hitAnimClip; // 피격 애니메이션 클립
    [SerializeField] private string hitAnimName = "Hit"; // 애니메이션 이름
    
    private void Awake()
    {
        // Animation 컴포넌트가 없다면 자동으로 찾기
        if (imageAnimation == null)
        {
            imageAnimation = GetComponent<Animation>();
            if (imageAnimation == null)
            {
                Debug.LogError("HitAnimationController: No Animation component found");
                return;
            }
        }
        
        // 애니메이션 클립 추가
        if (hitAnimClip != null && !imageAnimation.GetClip(hitAnimName))
        {
            imageAnimation.AddClip(hitAnimClip, hitAnimName);
        }
    }
    
    // 피격 애니메이션 재생
    public void PlayHitAnimation()
    {
        if (imageAnimation == null)
        {
            Debug.LogError("HitAnimationController: Animation component is null");
            return;
        }
        
        if (!imageAnimation.GetClip(hitAnimName))
        {
            Debug.LogError("HitAnimationController: Hit animation clip is not assigned");
            return;
        }
        
        // 이미 재생 중인 애니메이션 중지
        imageAnimation.Stop();
        
        // 새 애니메이션 재생
        imageAnimation.Play(hitAnimName);
    }
}