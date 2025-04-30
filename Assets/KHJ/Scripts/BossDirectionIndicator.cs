using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI element that displays an indicator arrow pointing toward the boss when it's off-screen or behind the camera.
/// </summary>
public class BossDirectionIndicator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float borderOffset = 20f;      // Offset from the screen edge
    [SerializeField] private Transform playerTransform;     // Player transform reference (if null, finds object with Player tag)
    
    private Camera mainCamera;                              // Main camera reference
    private Canvas parentCanvas;
    private RectTransform canvasRect;
    private Image indicatorImage;
    private RectTransform indicatorRect;
    private Transform targetBoss;
    
    private void Start()
    {
        // Get required component references
        indicatorImage = GetComponent<Image>();
        indicatorRect = GetComponent<RectTransform>();
        
        // Use main camera
        mainCamera = Camera.main;
        if (!mainCamera)
        {
            Debug.LogError("BossDirectionIndicator: Cannot find main camera!");
        }
        
        // If player transform is not set, find object with Player tag
        if (!playerTransform)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("BossDirectionIndicator: Cannot find game object with Player tag!");
            }
        }
        
        // Find parent canvas
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas)
        {
            canvasRect = parentCanvas.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogError("BossDirectionIndicator: Cannot find parent canvas!");
        }
        
        // Find game object with Enemy tag that has Boss component
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            // Check if it has any Boss component (any derived class is fine)
            Boss bossComponent = enemy.GetComponent<Boss>();
            if (bossComponent)
            {
                targetBoss = enemy.transform;
                break; // End loop after finding the first boss
            }
        }
    
        if (!targetBoss)
        {
            Debug.LogWarning("BossDirectionIndicator: Cannot find enemy with Boss component!");
            indicatorImage.enabled = false;
        }
    }
    
    private void Update()
    {
        // Hide indicator if target boss or camera is missing
        if (!targetBoss || !mainCamera)
        {
            indicatorImage.enabled = false;
            return;
        }
    
        // Convert boss world position to viewport coordinates (0-1 range)
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(targetBoss.position);
    
        // Check if boss is behind camera (z < 0)
        bool isBossBehind = viewportPosition.z < 0;
    
        // Check if boss is off-screen
        bool isOffScreen = viewportPosition.x < 0 || viewportPosition.x > 1 || 
                           viewportPosition.y < 0 || viewportPosition.y > 1;
    
        // Show indicator only when boss is off-screen or behind camera
        indicatorImage.enabled = isOffScreen || isBossBehind;
    
        // Stop calculations if indicator is disabled
        if (!indicatorImage.enabled)
        {
            return;
        }

        // Get canvas size
        Vector2 canvasSize = canvasRect.sizeDelta;
    
        // Calculate indicator position along screen border
        Vector2 indicatorPos = CalculateIndicatorPosition(viewportPosition, canvasSize);
        
        // Calculate boss vertical center position
        Renderer bossRenderer = targetBoss.GetComponent<Renderer>();
        Vector3 bossCenterPosition;
        
        if (bossRenderer)
        {
            // Use bounds center point for vertical middle if renderer exists
            bossCenterPosition = targetBoss.position;
            bossCenterPosition.y = bossRenderer.bounds.center.y; // Use only vertical center
        }
        else
        {
            // Use transform position if no renderer
            bossCenterPosition = targetBoss.position;
        }
        
        // Convert player position to screen coordinates
        Vector2 playerScreenPos;
        if (playerTransform)
        {
            playerScreenPos = mainCamera.WorldToScreenPoint(playerTransform.position);
            playerScreenPos.y = Screen.height - playerScreenPos.y; // Convert to UI coordinate system
        }
        else
        {
            // Use screen center if no player reference
            playerScreenPos = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }
        
        // Convert boss position to screen coordinates
        Vector2 bossScreenPos = mainCamera.WorldToScreenPoint(bossCenterPosition);
        bossScreenPos.y = Screen.height - bossScreenPos.y; // Convert to UI coordinate system
        
        Vector2 directionToIndicator;
        if (isBossBehind)
        {
            // If boss is behind, reverse direction
            directionToIndicator = (playerScreenPos - bossScreenPos).normalized;
        }
        else
        {
            directionToIndicator = (bossScreenPos - playerScreenPos).normalized;
        }
        
        float angle = Mathf.Atan2(directionToIndicator.y, directionToIndicator.x) * Mathf.Rad2Deg;
        
        // Set indicator position and rotation
        indicatorRect.anchoredPosition = indicatorPos;
        indicatorRect.localRotation = Quaternion.Euler(0, 0, angle - 90); // -90 degrees to make arrow point upward
    }
    
    private Vector2 CalculateIndicatorPosition(Vector3 viewportPos, Vector2 canvasSize)
    {
        // Half of screen size
        float halfWidth = canvasSize.x * 0.5f;
        float halfHeight = canvasSize.y * 0.5f;
        
        // Screen border boundaries (with offset)
        float edgeX = halfWidth - borderOffset;
        float edgeY = halfHeight - borderOffset;
        
        // Convert viewport coordinates to -0.5 ~ 0.5 range (screen center is origin)
        float viewX = viewportPos.x - 0.5f;
        float viewY = viewportPos.y - 0.5f;
        
        // Check if boss is off-screen
        bool isOffScreen = viewportPos.x < 0 || viewportPos.x > 1 || 
                          viewportPos.y < 0 || viewportPos.y > 1;
        
        // Check if boss is behind camera
        bool isBehind = viewportPos.z < 0;
        
        // Calculate direction vector (normalized)
        Vector2 direction;
        
        if (isBehind)
        {
            // If boss is behind camera, point downward but reflect horizontal position
            // Keep x coordinate but set y to always point down (-1)
            direction = new Vector2(viewX, -1).normalized;
        }
        else
        {
            direction = new Vector2(viewX, viewY).normalized;
        }
        
        // Calculate position on screen border
        Vector2 indicatorPos;
        
        if (isOffScreen || isBehind)
        {
            // If boss is off-screen or behind camera
            // Display along screen border
            
            if (isBehind)
            {
                // When behind camera, point downward but horizontal position follows boss
                float xPos = Mathf.Clamp(direction.x * edgeX, -edgeX, edgeX);
                indicatorPos = new Vector2(xPos, -edgeY);
            }
            else if (Mathf.Abs(direction.x) * edgeY > Mathf.Abs(direction.y) * edgeX)
            {
                // Intersect with x-axis border
                float sign = Mathf.Sign(direction.x);
                indicatorPos = new Vector2(
                    sign * edgeX,
                    (direction.y / direction.x) * sign * edgeX
                );
            }
            else
            {
                // Intersect with y-axis border
                float sign = Mathf.Sign(direction.y);
                indicatorPos = new Vector2(
                    (direction.x / direction.y) * sign * edgeY,
                    sign * edgeY
                );
            }
        }
        else
        {
            // If boss is on-screen
            // Convert boss position from viewport to canvas coordinates
            indicatorPos = new Vector2(
                viewX * canvasSize.x,
                viewY * canvasSize.y
            );
            
            // Ensure indicator stays within screen border
            float maxMagnitude = Mathf.Min(edgeX, edgeY);
            if (indicatorPos.magnitude > maxMagnitude)
            {
                indicatorPos = indicatorPos.normalized * maxMagnitude;
            }
        }
        
        return indicatorPos;
    }
}