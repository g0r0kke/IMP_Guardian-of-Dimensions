using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform lever;
    private RectTransform rectTransform;
    [SerializeField, Range(10f, 150f)] private float leverRange = 50f;
    [SerializeField] private float distancePerFrame = 2.0f; // 이동 속도

    // 레이캐스트 관련 변수
    [SerializeField] private float raycastDistance = 0.5f; // 레이캐스트 거리
    [SerializeField] private LayerMask wallLayer; // Wall 레이어 마스크
    
    private Vector2 inputVector;
    private bool isInput;
    
    // XR Origin 참조
    [HideInInspector] public Transform xrOrigin;
    
    // 카메라 참조 (방향 계산용)
    private Camera arCamera;

    // 플레이어 캡슐 콜라이더 참조
    private CapsuleCollider playerCollider;
    
    // 조이스틱 visible 제어를 위한 이미지 컴포넌트
    private Image joystickImage;
    private Image leverImage;
    
    // 캔버스 스케일러 참조
    private Canvas parentCanvas;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        arCamera = Camera.main;
        
        // 조이스틱 배경과 레버의 이미지 컴포넌트 가져오기
        joystickImage = GetComponent<Image>();
        if (lever != null)
        {
            leverImage = lever.GetComponent<Image>();
        }
        
        // 부모 캔버스 찾기
        parentCanvas = GetComponentInParent<Canvas>();
        
        // Player 태그를 가진 오브젝트에서 콜라이더 찾기
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerCollider = playerObject.GetComponent<CapsuleCollider>();
            if (playerCollider == null)
            {
                // 직접적으로 연결된 콜라이더가 없으면 자식 오브젝트에서 찾기
                playerCollider = playerObject.GetComponentInChildren<CapsuleCollider>();
            }
        }
        
        // Wall 레이어 설정
        wallLayer = LayerMask.GetMask("Wall");
    }
    
    void Update()
    {
        if(isInput)
        {
            InputControlVector();
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        // 레버 초기화
        lever.anchoredPosition = Vector2.zero;
        isInput = true;
        // Debug.Log("[조이스틱] 터치 시작");
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!isInput) return;
        
        // 스크린 좌표를 로컬 좌표로 변환
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            parentCanvas.worldCamera,
            out Vector2 localPoint))
        {
            // 로컬 좌표계에서의 방향 계산
            Vector2 inputDir = localPoint;
            
            // 최대 범위 제한
            Vector2 clampedDir = inputDir.magnitude < leverRange ? inputDir 
                : inputDir.normalized * leverRange;
                
            lever.anchoredPosition = clampedDir;
            inputVector = clampedDir / leverRange;
            
            // Debug.Log("[조이스틱] 레버 위치: " + clampedDir);
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        // 값 초기화
        lever.anchoredPosition = Vector2.zero;
        inputVector = Vector2.zero;
        isInput = false;
        // Debug.Log("[조이스틱] 터치 종료");
    }
    
    private void InputControlVector()
    {
        if (xrOrigin != null && arCamera != null)
        {
            // 카메라 기준 방향 가져오기
            Vector3 forward = arCamera.transform.forward;
            Vector3 right = arCamera.transform.right;
            
            // Y축(수직) 성분 제거하여 수평면에서만 이동하도록 설정
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();
            
            // 입력벡터에 따른 이동 방향 계산
            Vector3 moveDirection = forward * inputVector.y + right * inputVector.x;
            
            // 이동 거리 계산
            float moveDistance = distancePerFrame * Time.deltaTime;
            
            // 이동할 위치 계산
            Vector3 targetPosition = xrOrigin.position + moveDirection * moveDistance;
            
            // 벽 충돌 체크 및 이동
            if (!CheckWallCollision(moveDirection, moveDistance))
            {
                // 벽과 충돌하지 않으면 이동
                xrOrigin.position = targetPosition;
            }
        }
    }
    
    private bool CheckWallCollision(Vector3 moveDirection, float moveDistance)
    {
        if (playerCollider == null) return false;
        
        // 플레이어 캡슐 정보 가져오기
        Vector3 colliderCenter = xrOrigin.position + playerCollider.center;
        float colliderRadius = playerCollider.radius;
        float colliderHeight = playerCollider.height;
        
        // 레이캐스트 시작 위치들 (캡슐 콜라이더의 경계 부분에서 시작)
        Vector3[] raycastOrigins = new Vector3[]
        {
            colliderCenter, // 중앙
            colliderCenter + Vector3.up * (colliderHeight * 0.5f - colliderRadius), // 상단
            colliderCenter + Vector3.down * (colliderHeight * 0.5f - colliderRadius) // 하단
        };
        
        // 여러 방향으로 레이캐스트를 발사하여 벽 충돌 확인
        foreach (Vector3 origin in raycastOrigins)
        {
            // 디버그 레이 표시
            Debug.DrawRay(origin, moveDirection * (raycastDistance + colliderRadius), Color.red, 0.1f);
            
            // 레이캐스트로 벽 감지
            if (Physics.Raycast(origin, moveDirection, out RaycastHit hit, raycastDistance + colliderRadius, wallLayer))
            {
                // 벽과의 거리가 콜라이더 반경보다 작으면 충돌로 간주
                if (hit.distance <= colliderRadius + raycastDistance)
                {
                    Debug.Log("벽 감지: " + hit.collider.name + ", 거리: " + hit.distance);
                    return true; // 충돌 발생
                }
            }
        }
        
        return false; // 충돌 없음
    }
}