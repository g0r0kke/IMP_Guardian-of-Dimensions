using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Manages touch input on panel and dynamically positions virtual joystick
/// </summary>
public class TouchPanelController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Tooltip("Joystick game object")]
    [SerializeField] private GameObject joystickObject;
    
    [Tooltip("XR Origin reference")]
    [SerializeField] private Transform xrOrigin;
    
    private VirtualJoystick joystickScript;
    private Canvas parentCanvas;
    private RectTransform joystickRectTransform;
    
    private void Awake()
    {
        // Get canvas reference
        parentCanvas = GetComponentInParent<Canvas>();
        if (!parentCanvas)
        {
            Debug.LogError("캔버스를 찾을 수 없습니다!");
        }
        
        // Get joystick script and RectTransform references
        if (joystickObject)
        {
            joystickScript = joystickObject.GetComponent<VirtualJoystick>();
            joystickRectTransform = joystickObject.GetComponent<RectTransform>();
            
            // Set XR Origin reference
            if (xrOrigin && joystickScript)
            {
                joystickScript.xrOrigin = xrOrigin;
            }
            
            // Initially hide joystick
            joystickObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Joystick object not assigned!");
        }
    }
    
    /// <summary>
    /// Called when pointer is pressed on the panel
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (joystickObject == null || joystickRectTransform == null) return;
        
        // Activate joystick
        joystickObject.SetActive(true);
        
        // Move joystick to touch position
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            eventData.position,
            parentCanvas.worldCamera,
            out localPoint))
        {
            joystickRectTransform.localPosition = localPoint;
            // Debug.Log("Joystick position set: " + localPoint);
        }
        
        // Forward pointer event to joystick script
        if (joystickScript)
        {
            joystickScript.OnPointerDown(eventData);
        }
    }
    
    /// <summary>
    /// Called when pointer is dragged on the panel
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        // Forward drag event to joystick
        if (joystickScript && joystickObject.activeSelf)
        {
            joystickScript.OnDrag(eventData);
        }
    }
    
    /// <summary>
    /// Called when pointer is released from the panel
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        // Forward pointer up event to joystick
        if (joystickScript)
        {
            joystickScript.OnPointerUp(eventData);
            
            // Hide joystick (after joystick script's OnPointerUp is executed)
            joystickObject.SetActive(false);
        }
    }
}