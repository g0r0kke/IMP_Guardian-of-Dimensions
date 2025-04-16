using UnityEngine;
using UnityEngine.UI;

public class BossDirectionIndicator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float borderOffset = 20f;      // 화면 가장자리로부터의 오프셋
    [SerializeField] private Transform playerTransform;     // 플레이어 트랜스폼 레퍼런스 (없으면 Player 태그로 자동 찾음)
    
    private Camera mainCamera;                              // 메인 카메라 레퍼런스
    private Canvas parentCanvas;
    private RectTransform canvasRect;
    private Image indicatorImage;
    private RectTransform indicatorRect;
    private Transform targetBoss;
    
    private void Start()
    {
        // 필요한 컴포넌트 참조 가져오기
        indicatorImage = GetComponent<Image>();
        indicatorRect = GetComponent<RectTransform>();
        
        // 메인 카메라 사용
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("BossDirectionIndicator: 메인 카메라를 찾을 수 없습니다!");
        }
        
        // 플레이어 트랜스폼이 설정되지 않은 경우 Player 태그로 찾기
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("BossDirectionIndicator: Player 태그를 가진 게임 오브젝트를 찾을 수 없습니다!");
            }
        }
        
        // 부모 캔버스 찾기
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            canvasRect = parentCanvas.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogError("BossDirectionIndicator: 부모 캔버스를 찾을 수 없습니다!");
        }
        
        // Enemy 태그를 가진 게임 오브젝트 중 Boss 컴포넌트가 있는 것만 찾기
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            // Boss 컴포넌트가 있는지 확인 (어떤 Boss 파생 클래스든 상관없음)
            Boss bossComponent = enemy.GetComponent<Boss>();
            if (bossComponent != null)
            {
                targetBoss = enemy.transform;
                break; // 첫 번째 보스를 찾으면 루프 종료
            }
        }
    
        if (targetBoss == null)
        {
            Debug.LogWarning("BossDirectionIndicator: Boss 컴포넌트를 가진 적을 찾을 수 없습니다!");
            indicatorImage.enabled = false;
        }
        
        // 권장 앵커 설정 (코드로 설정하려면 아래 주석 해제)
        // indicatorRect.anchorMin = new Vector2(0.5f, 0.5f);
        // indicatorRect.anchorMax = new Vector2(0.5f, 0.5f);
        // indicatorRect.pivot = new Vector2(0.5f, 0.5f);
    }
    
    private void Update()
    {
        // 타겟 보스나 카메라가 없으면 인디케이터 숨기기
        if (targetBoss == null || mainCamera == null)
        {
            indicatorImage.enabled = false;
            return;
        }
    
        // 보스의 월드 위치를 뷰포트 좌표로 변환 (0~1 범위)
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(targetBoss.position);
    
        // 보스가 카메라 뒤에 있는지 확인 (z < 0)
        bool isBossBehind = viewportPosition.z < 0;
    
        // 보스가 화면 밖에 있는지 확인
        bool isOffScreen = viewportPosition.x < 0 || viewportPosition.x > 1 || 
                           viewportPosition.y < 0 || viewportPosition.y > 1;
    
        // 보스가 화면 밖에 있거나 카메라 뒤에 있을 때만 인디케이터 표시
        indicatorImage.enabled = isOffScreen || isBossBehind;
    
        // 인디케이터가 비활성화된 경우 더 이상 계산하지 않음
        if (!indicatorImage.enabled)
        {
            return;
        }

        // 캔버스 크기 얻기
        Vector2 canvasSize = canvasRect.sizeDelta;
    
        // 화면 테두리를 따라 인디케이터 위치 계산
        Vector2 indicatorPos = CalculateIndicatorPosition(viewportPosition, canvasSize);
        
        // 보스의 수직 중앙 위치 계산
        Renderer bossRenderer = targetBoss.GetComponent<Renderer>();
        Vector3 bossCenterPosition;
        
        if (bossRenderer != null)
        {
            // 렌더러가 있는 경우 바운드의 수직 중앙점 사용
            bossCenterPosition = targetBoss.position;
            bossCenterPosition.y = bossRenderer.bounds.center.y; // 수직 중앙만 사용
        }
        else
        {
            // 렌더러가 없는 경우 그냥 트랜스폼 위치 사용
            bossCenterPosition = targetBoss.position;
        }
        
        // 보스 위치를 스크린 좌표로 변환
        Vector3 bossViewportPos = mainCamera.WorldToViewportPoint(bossCenterPosition);
        
        // 플레이어 위치를 스크린 좌표로 변환
        Vector2 playerScreenPos;
        if (playerTransform != null)
        {
            playerScreenPos = mainCamera.WorldToScreenPoint(playerTransform.position);
            playerScreenPos.y = Screen.height - playerScreenPos.y; // UI 좌표계로 변환
        }
        else
        {
            // 플레이어 참조가 없으면 화면 중앙을 사용
            playerScreenPos = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }
        
        // 보스 위치를 스크린 좌표로 변환
        Vector2 bossScreenPos = mainCamera.WorldToScreenPoint(bossCenterPosition);
        bossScreenPos.y = Screen.height - bossScreenPos.y; // UI 좌표계로 변환
        
        Vector2 directionToIndicator;
        if (isBossBehind)
        {
            // 보스가 뒤에 있는 경우, 방향을 반대로
            directionToIndicator = (playerScreenPos - bossScreenPos).normalized;
        }
        else
        {
            directionToIndicator = (bossScreenPos - playerScreenPos).normalized;
        }
        
        float angle = Mathf.Atan2(directionToIndicator.y, directionToIndicator.x) * Mathf.Rad2Deg;
        
        // 인디케이터 위치와 회전 설정
        indicatorRect.anchoredPosition = indicatorPos;
        indicatorRect.localRotation = Quaternion.Euler(0, 0, angle - 90); // -90도는 화살표가 위쪽을 가리키도록 조정
    }
    
    private Vector2 CalculateIndicatorPosition(Vector3 viewportPos, Vector2 canvasSize)
    {
        // 화면 크기의 절반
        float halfWidth = canvasSize.x * 0.5f;
        float halfHeight = canvasSize.y * 0.5f;
        
        // 화면 테두리 경계값 (오프셋 적용)
        float edgeX = halfWidth - borderOffset;
        float edgeY = halfHeight - borderOffset;
        
        // 뷰포트 좌표를 -0.5 ~ 0.5 범위로 변환 (화면 중앙이 원점)
        float viewX = viewportPos.x - 0.5f;
        float viewY = viewportPos.y - 0.5f;
        
        // 화면 밖에 있는지 여부 확인
        bool isOffScreen = viewportPos.x < 0 || viewportPos.x > 1 || 
                          viewportPos.y < 0 || viewportPos.y > 1;
        
        // 카메라 뒤에 있는지 여부 확인
        bool isBehind = viewportPos.z < 0;
        
        // 방향 벡터 계산 (정규화)
        Vector2 direction;
        
        if (isBehind)
        {
            // 보스가 카메라 뒤에 있으면 아래쪽을 가리키되, 좌우는 보스 위치 반영
            // x 좌표는 유지하고 y는 항상 아래쪽(-1)으로 설정
            direction = new Vector2(viewX, -1).normalized;
        }
        else
        {
            direction = new Vector2(viewX, viewY).normalized;
        }
        
        // 화면 테두리 위치 계산
        Vector2 indicatorPos;
        
        if (isOffScreen || isBehind)
        {
            // 보스가 화면 밖에 있거나 카메라 뒤에 있는 경우
            // 화면 테두리를 따라 표시
            
            if (isBehind)
            {
                // 카메라 뒤에 있을 때, 아래쪽으로 가리키되 좌우는 보스 위치 기준
                float xPos = Mathf.Clamp(direction.x * edgeX, -edgeX, edgeX);
                indicatorPos = new Vector2(xPos, -edgeY);
            }
            else if (Mathf.Abs(direction.x) * edgeY > Mathf.Abs(direction.y) * edgeX)
            {
                // x방향 테두리와 교차
                float sign = Mathf.Sign(direction.x);
                indicatorPos = new Vector2(
                    sign * edgeX,
                    (direction.y / direction.x) * sign * edgeX
                );
            }
            else
            {
                // y방향 테두리와 교차
                float sign = Mathf.Sign(direction.y);
                indicatorPos = new Vector2(
                    (direction.x / direction.y) * sign * edgeY,
                    sign * edgeY
                );
            }
        }
        else
        {
            // 보스가 화면 안에 있는 경우
            // 보스의 위치를 뷰포트에서 캔버스 좌표로 변환
            indicatorPos = new Vector2(
                viewX * canvasSize.x,
                viewY * canvasSize.y
            );
            
            // 인디케이터가 화면 테두리를 벗어나지 않도록 조정
            float maxMagnitude = Mathf.Min(edgeX, edgeY);
            if (indicatorPos.magnitude > maxMagnitude)
            {
                indicatorPos = indicatorPos.normalized * maxMagnitude;
            }
        }
        
        return indicatorPos;
    }
}