using UnityEngine;
using UnityEngine.EventSystems; // 키보드, 마우스, 터치를 이벤트로 오브젝트에 보낼 수 있는 기능을 지원

public class VirtualJoystick : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform lever;
    private RectTransform rectTransform;
    [SerializeField, Range(10f, 150f)] private float leverRange;
    [SerializeField] private float distancePerFrame = 2.0f; // 이동 속도

    private Vector2 inputVector;
    private bool isInput;
    
    // XR Origin 참조
    public Transform xrOrigin;
    
    // 카메라 참조 (방향 계산용)
    private Camera arCamera;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        arCamera = Camera.main;
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
            
            // 직접 XR Origin 위치 이동
            xrOrigin.Translate(moveDirection * distancePerFrame * Time.deltaTime, Space.World);
            
            Debug.Log("Input Vector: " + inputVector.x + " / " + inputVector.y);
        }
    }
}