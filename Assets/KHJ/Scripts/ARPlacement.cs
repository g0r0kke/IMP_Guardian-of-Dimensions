using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// Handles AR object placement on detected planes
/// </summary>
public class ARPlacement : MonoBehaviour
{
    [SerializeField] private GameObject markerPrefab; // Object to place in AR
    [SerializeField] private ARRaycastManager raycastManager; // AR raycast manager reference
    [SerializeField] private ARPlaneManager planeManager; // AR plane manager reference
    [SerializeField] private AudioSource placementAudioSource; // Audio source for placement sound
    
    private GameObject spawnedObject; // Reference to the currently placed object
    private List<ARRaycastHit> hits = new List<ARRaycastHit>(); // List to store raycast hits

    private void Awake()
    {
        // Enable Enhanced Touch Support for New Input System
        EnhancedTouchSupport.Enable();
    }

    private void OnEnable()
    {
        // Register touch events
        TouchSimulation.Enable();
        Touch.onFingerDown += OnFingerDown;
    }

    private void OnDisable()
    {
        // Unregister touch events
        TouchSimulation.Disable();
        Touch.onFingerDown -= OnFingerDown;
    }

    void Start()
    {
        // Check raycast manager reference
        if (!raycastManager)
        {
            raycastManager = FindFirstObjectByType<ARRaycastManager>();
            if (!raycastManager)
            {
                Debug.LogError("Cannot find AR Raycast Manager!");
                enabled = false;
                return;
            }
        }

        // Check plane manager reference
        if (!planeManager)
        {
            planeManager = FindFirstObjectByType<ARPlaneManager>();
            if (!planeManager)
            {
                Debug.LogError("Cannot find AR Plane Manager!");
                enabled = false;
                return; 
            }
        }

        // Check marker prefab
        if (!markerPrefab)
        {
            Debug.LogError("Marker prefab is not assigned!");
            enabled = false;
            return;
        }
        
        // Check audio source
        if (!placementAudioSource)
        {
            placementAudioSource = GetComponent<AudioSource>();
            if (!placementAudioSource)
            {
                Debug.LogWarning("Audio source is not assigned. Sound may not play.");
            }
        }
        
        // Set initial game state
        if (GameManager.Instance)
        {
            GameManager.Instance.SetState(GameState.Intro);
        }
        else
        {
            Debug.LogWarning("GameManager not found.");
        }
    }
    
    /// <summary>
    /// Handles finger touch input for AR placement
    /// </summary>
    private void OnFingerDown(Finger finger)
    {
        // Check if at least one AR plane has been detected
        if (planeManager.trackables.count == 0)
        {
            // No planes detected yet
            return;
        }
        
        // Check if touch is over UI elements - ignore if so
        if (IsPointerOverUI(finger.currentTouch.screenPosition))
        {
            return; // Ignore touches on UI elements
        }
        
        // Create ray from touch position
        Ray ray = Camera.main.ScreenPointToRay(finger.currentTouch.screenPosition);
        
        // Check for collision with AR planes
        if (raycastManager && raycastManager.Raycast(finger.currentTouch.screenPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
        {
            if (hits.Count > 0)
            {
                // Get hit position and pose
                Pose hitPose = hits[0].pose;
                
                // Calculate rotation to face camera
                Vector3 directionToCamera = Camera.main.transform.position - hitPose.position;
                directionToCamera.y = 0; // Only apply y-axis rotation
                
                Quaternion lookAtCamera = Quaternion.LookRotation(directionToCamera);
                
                // Create or update object position
                if (!spawnedObject)
                {
                    spawnedObject = Instantiate(markerPrefab, hitPose.position, lookAtCamera);
                    PlayPlacementAudio();
                }
                else
                {
                    spawnedObject.transform.position = hitPose.position;
                    spawnedObject.transform.rotation = lookAtCamera;
                    PlayPlacementAudio();
                }
            }
        }
    }
    
    /// <summary>
    /// Checks if pointer is over a UI element
    /// </summary>
    private bool IsPointerOverUI(Vector2 position)
    {
        // Check if the position is over any UI elements using the event system
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = position;
        
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        return results.Count > 0; // Return true if results exist (UI element was hit)
    }
    
    /// <summary>
    /// Returns the position of the placed object
    /// </summary>
    public Vector3 GetSpawnedObjectPosition()
    {
        if (spawnedObject)
        {
            return spawnedObject.transform.position;
        }
        return Vector3.zero;
    }
    
    /// <summary>
    /// Plays sound effect when object is placed
    /// </summary>
    private void PlayPlacementAudio()
    {
        if (placementAudioSource && placementAudioSource.clip)
        {
            // Restart if already playing
            if (placementAudioSource.isPlaying)
            {
                placementAudioSource.Stop();
            }
            
            placementAudioSource.Play();
        }
    }
}