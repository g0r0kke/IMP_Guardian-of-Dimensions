using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class ARPlacement : MonoBehaviour
{
    [SerializeField] private GameObject markerPrefab;
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private AudioSource placementAudioSource;
    
    private GameObject spawnedObject;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {
        // 컴포넌트 참조 확인
        if (raycastManager == null)
        {
            raycastManager = FindObjectOfType<ARRaycastManager>();
            if (raycastManager == null)
            {
                Debug.LogError("AR Raycast Manager를 찾을 수 없습니다!");
                enabled = false;
                return;
            }
        }

        if (planeManager == null)
        {
            planeManager = FindObjectOfType<ARPlaneManager>();
            if (planeManager == null)
            {
                Debug.LogError("AR Plane Manager를 찾을 수 없습니다!");
                enabled = false;
                return; 
            }
        }

        if (markerPrefab == null)
        {
            Debug.LogError("마커 프리팹이 할당되지 않았습니다!");
            enabled = false;
            return;
        }
        
        if (placementAudioSource == null)
        {
            placementAudioSource = GetComponent<AudioSource>();
            if (placementAudioSource == null)
            {
                Debug.LogWarning("오디오 소스가 할당되지 않았습니다. 소리가 재생되지 않을 수 있습니다.");
            }
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetState(GameState.Intro);
        }
        else
        {
            Debug.LogWarning("GameManager를 찾을 수 없습니다.");
        }
    }
    
    void Update()
    {
        // AR 플레인이 하나 이상 감지되었는지 확인
        if (planeManager.trackables.count == 0)
        {
            // 아직 플레인이 감지되지 않음
            return;
        }
        
        // 터치 입력 확인
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // UI 요소 터치 확인 - UI 요소를 터치한 경우 처리하지 않음
            if (IsPointerOverUI(Input.GetTouch(0).position))
            {
                return;  // UI 요소를 터치했으면 레이캐스트 처리하지 않음
            }
            
            // 터치한 화면 위치로부터 Ray 생성
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            
            // AR 플레인과의 충돌 확인
            if (raycastManager != null && raycastManager.Raycast(Input.GetTouch(0).position, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
            {
                if (hits.Count > 0)
                {
                    // Ray가 AR 플레인과 충돌한 위치
                    Pose hitPose = hits[0].pose;
                    
                    // Y축 회전에 180도 추가
                    // Quaternion rotationWithYFlip = hitPose.rotation * Quaternion.Euler(0, 180, 0);
                    
                    Quaternion zeroRotation = Quaternion.Euler(0, 180, 0);
                    
                    // 큐브가 아직 없으면 생성, 있으면 위치 업데이트
                    if (spawnedObject == null)
                    {
                        // spawnedObject = Instantiate(markerPrefab, hitPose.position, rotationWithYFlip);
                        spawnedObject = Instantiate(markerPrefab, hitPose.position, zeroRotation);
                        PlayPlacementAudio();
                    }
                    else
                    {
                        spawnedObject.transform.position = hitPose.position;
                        spawnedObject.transform.rotation = zeroRotation;
                        // spawnedObject.transform.rotation = rotationWithYFlip;
                        PlayPlacementAudio();
                    }
                }
            }
        }
    }
    
    // UI 요소 위에 터치했는지 확인하는 함수
    private bool IsPointerOverUI(Vector2 position)
    {
        // 이벤트 시스템을 통해 해당 위치에 UI 요소가 있는지 확인
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = position;
        
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        return results.Count > 0;  // 결과가 있으면 UI 요소 위에 있는 것
    }
    
    public Vector3 GetSpawnedObjectPosition()
    {
        if (spawnedObject != null)
        {
            return spawnedObject.transform.position;
        }
        return Vector3.zero;
    }
    
    private void PlayPlacementAudio()
    {
        if (placementAudioSource != null && placementAudioSource.clip != null)
        {
            // 이미 재생 중이면 재시작
            if (placementAudioSource.isPlaying)
            {
                placementAudioSource.Stop();
            }
            
            placementAudioSource.Play();
        }
    }
}