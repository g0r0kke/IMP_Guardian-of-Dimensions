using UnityEngine;
using UnityEngine.EventSystems;

public class TouchPanelController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Tooltip("조이스틱 오브젝트")]
    [SerializeField] private GameObject joystickObject;
    
    [Tooltip("XR Origin 참조")]
    [SerializeField] private Transform xrOrigin;
    
    private VirtualJoystick joystickScript;
    private Canvas parentCanvas;
    private RectTransform joystickRectTransform;
    
    private void Awake()
    {
        // 캔버스 참조 가져오기
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogError("캔버스를 찾을 수 없습니다!");
        }
        
        // 조이스틱 스크립트 및 RectTransform 참조 가져오기
        if (joystickObject != null)
        {
            joystickScript = joystickObject.GetComponent<VirtualJoystick>();
            joystickRectTransform = joystickObject.GetComponent<RectTransform>();
            
            // XR Origin 참조 설정
            if (xrOrigin != null && joystickScript != null)
            {
                joystickScript.xrOrigin = xrOrigin;
            }
            
            // 처음에는 조이스틱 비활성화
            joystickObject.SetActive(false);
        }
        else
        {
            Debug.LogError("조이스틱 오브젝트가 설정되지 않았습니다!");
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (joystickObject == null || joystickRectTransform == null) return;
        
        // 조이스틱 활성화
        joystickObject.SetActive(true);
        
        // 터치 위치로 조이스틱 이동
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            eventData.position,
            parentCanvas.worldCamera,
            out localPoint))
        {
            joystickRectTransform.localPosition = localPoint;
            // Debug.Log("조이스틱 위치 설정: " + localPoint);
        }
        
        // 조이스틱 스크립트에 포인터 이벤트 전달
        if (joystickScript != null)
        {
            joystickScript.OnPointerDown(eventData);
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        // 드래그 이벤트를 조이스틱에 전달
        if (joystickScript != null && joystickObject.activeSelf)
        {
            joystickScript.OnDrag(eventData);
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        // 포인터 업 이벤트를 조이스틱에 전달
        if (joystickScript != null)
        {
            joystickScript.OnPointerUp(eventData);
            
            // 조이스틱 비활성화 (조이스틱 스크립트의 OnPointerUp 함수가 실행된 후)
            joystickObject.SetActive(false);
        }
    }
}