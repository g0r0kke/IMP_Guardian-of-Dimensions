using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Implements a virtual joystick for movement control with wall collision detection
/// </summary>
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform lever;
    private RectTransform rectTransform;
    [SerializeField, Range(10f, 150f)] private float leverRange = 50f;
    [SerializeField] private float distancePerFrame = 2.0f; // Movement speed

    // Raycast related variables
    [SerializeField] private float raycastDistance = 0.5f; // Raycast distance
    [SerializeField] private LayerMask wallLayer; // Wall layer mask
    
    private Vector2 inputVector;
    private bool isInput;
    
    // XR Origin reference
    [HideInInspector] public Transform xrOrigin;
    
    // Camera reference (for direction calculation)
    private Camera arCamera;

    // Player capsule collider reference
    private CapsuleCollider playerCollider;
    
    // Canvas scaler reference
    private Canvas parentCanvas;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        arCamera = Camera.main;
        
        // Find parent canvas
        parentCanvas = GetComponentInParent<Canvas>();
        
        // Find collider on object with Player tag
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject)
        {
            playerCollider = playerObject.GetComponent<CapsuleCollider>();
            if (!playerCollider)
            {
                // If no direct collider, check in children
                playerCollider = playerObject.GetComponentInChildren<CapsuleCollider>();
            }
        }
        
        // Set up wall layer
        wallLayer = LayerMask.GetMask("Wall");
    }
    
    void Update()
    {
        if(isInput)
        {
            InputControlVector();
        }
    }
    
    /// <summary>
    /// Called when pointer is pressed on joystick
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        // Reset lever position
        lever.anchoredPosition = Vector2.zero;
        isInput = true;
        // Debug.Log("[Joystick] Touch started");
    }
    
    /// <summary>
    /// Called when pointer is dragged on joystick
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!isInput) return;
        
        // Convert screen coordinates to local coordinates
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            parentCanvas.worldCamera,
            out Vector2 localPoint))
        {
            // Calculate direction in local coordinate system
            Vector2 inputDir = localPoint;
            
            // Limit to maximum range
            Vector2 clampedDir = inputDir.magnitude < leverRange ? inputDir 
                : inputDir.normalized * leverRange;
                
            lever.anchoredPosition = clampedDir;
            inputVector = clampedDir / leverRange;
            
            // Debug.Log("[Joystick] Lever position: " + clampedDir);
        }
    }
    
    /// <summary>
    /// Called when pointer is released from joystick
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        // Reset values
        lever.anchoredPosition = Vector2.zero;
        inputVector = Vector2.zero;
        isInput = false;
        // Debug.Log("[Joystick] Touch ended");
    }
    
    /// <summary>
    /// Processes joystick input for character movement
    /// </summary>
    private void InputControlVector()
    {
        if (xrOrigin && arCamera)
        {
            // Get camera-based directions
            Vector3 forward = arCamera.transform.forward;
            Vector3 right = arCamera.transform.right;
            
            // Remove Y-axis (vertical) component to move only on horizontal plane
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();
            
            // Calculate movement direction based on input vector
            Vector3 moveDirection = forward * inputVector.y + right * inputVector.x;
            
            // Calculate move distance
            float moveDistance = distancePerFrame * Time.deltaTime;
            
            // Calculate target position
            Vector3 targetPosition = xrOrigin.position + moveDirection * moveDistance;
            
            // Check wall collision and move
            if (!CheckWallCollision(moveDirection, moveDistance))
            {
                // Move if no wall collision
                xrOrigin.position = targetPosition;
            }
        }
    }
    
    /// <summary>
    /// Checks for wall collisions in the movement direction
    /// </summary>
    /// <param name="moveDirection">Direction of movement</param>
    /// <param name="moveDistance">Distance to move</param>
    /// <returns>True if wall collision detected, false otherwise</returns>
    private bool CheckWallCollision(Vector3 moveDirection, float moveDistance)
    {
        if (!playerCollider) return false;
        
        // Get player capsule information
        Vector3 colliderCenter = xrOrigin.position + playerCollider.center;
        float colliderRadius = playerCollider.radius;
        float colliderHeight = playerCollider.height;
        
        // Raycast start positions (from capsule collider boundaries)
        Vector3[] raycastOrigins = new Vector3[]
        {
            colliderCenter, // Center
            colliderCenter + Vector3.up * (colliderHeight * 0.5f - colliderRadius), // Top
            colliderCenter + Vector3.down * (colliderHeight * 0.5f - colliderRadius) // Bottom
        };
        
        // Cast rays in multiple directions to check for wall collisions
        foreach (Vector3 origin in raycastOrigins)
        {
            // Show debug ray
            Debug.DrawRay(origin, moveDirection * (raycastDistance + colliderRadius), Color.red, 0.1f);
            
            // Detect walls with raycast
            if (Physics.Raycast(origin, moveDirection, out RaycastHit hit, raycastDistance + colliderRadius, wallLayer))
            {
                // Consider collision if distance is less than collider radius
                if (hit.distance <= colliderRadius + raycastDistance)
                {
                    Debug.Log("Wall detected: " + hit.collider.name + ", distance: " + hit.distance);
                    return true; // Collision occurred
                }
            }
        }
        
        return false; // No collision
    }
}