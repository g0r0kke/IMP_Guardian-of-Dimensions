using UnityEngine;
using UnityEngine.EventSystems; // 키보드, 마우스, 터치를 이벤트로 오브젝트에 보낼 수 있는 기능을 지원

public class VirtualJoystick : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform lever;
    private RectTransform rectTransform;
    [SerializeField, Range(10f, 150f)] private float leverRange;
    [SerializeField] private float distancePerFrame = 2.0f; // 이동 속도

    // 레이캐스트 관련 변수
    [SerializeField] private float raycastDistance = 0.5f; // 레이캐스트 거리
    [SerializeField] private LayerMask wallLayer; // Wall 레이어 마스크
    
    private Vector2 inputVector;
    private bool isInput;
    
    // XR Origin 참조
    public Transform xrOrigin;
    
    // 카메라 참조 (방향 계산용)
    private Camera arCamera;

    // 플레이어 캡슐 콜라이더 참조
    private CapsuleCollider playerCollider;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        arCamera = Camera.main;
        
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
            
            if (playerCollider == null)
            {
                Debug.LogWarning("Player 태그를 가진 오브젝트에서 캡슐 콜라이더를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogError("Player 태그를 가진 오브젝트를 찾을 수 없습니다.");
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
    
    public void OnBeginDrag(PointerEventData eventData)
    {  
        // Debug.Log("Begin");
        // var inputDir = eventData.position - rectTransform.anchoredPosition;
        // var clampedDir = inputDir.magnitude < leverRange ? inputDir 
        //     : inputDir.normalized * leverRange;
        // lever.anchoredPosition = clampedDir;
        
        ControlJoystickLever(eventData);
        isInput = true;
    }
    
    // 오브젝트를 클릭해서 드래그 하는 도중에 들어오는 이벤트
    // 하지만 클릭을 유지한 상태로 마우스를 멈추면 이벤트가 들어오지 않음    
    public void OnDrag(PointerEventData eventData)
    {
        // Debug.Log("Drag");
        // var inputDir = eventData.position - rectTransform.anchoredPosition;
        // // lever.anchoredPosition = inputDir;
        // var clampedDir = inputDir.magnitude < leverRange ? 
        //     inputDir : inputDir.normalized * leverRange;
        // lever.anchoredPosition = clampedDir;
        
        ControlJoystickLever(eventData);
    }
    
    public void ControlJoystickLever(PointerEventData eventData)
    {
        var inputDir = eventData.position - rectTransform.anchoredPosition;
        var clampedDir = inputDir.magnitude < leverRange ? inputDir 
            : inputDir.normalized * leverRange;
        lever.anchoredPosition = clampedDir;
        inputVector = clampedDir / leverRange;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        // Debug.Log("End");
        lever.anchoredPosition = Vector2.zero;
        inputVector = Vector2.zero; // 입력 벡터 초기화
        isInput = false;
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