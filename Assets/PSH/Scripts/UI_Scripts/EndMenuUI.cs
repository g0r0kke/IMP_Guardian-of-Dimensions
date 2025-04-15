using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndMenuUI : MonoBehaviour
{
    public RectTransform arrowHead; // 화살표 이미지 위치
    public Button[] Buttons; // 0은 MainMenu, 1은 Replay
    public Button okayButton;


    public GameObject victoryUI;
    public GameObject defeatUI;
    public GameObject background;

    private int currentSelection = 0;

    // 이전 씬 저장 변수
    private string previousScene;

    void Start()
    {
        // 초기화: 화살표를 기본 선택 (Main Menu)으로 설정
        ArrowMoveButton(currentSelection);
        okayButton.onClick.AddListener(ExecuteCurrentSelection);

        // 버튼 클릭 이벤트 추가
        Buttons[0].onClick.AddListener(() => SelectMainMenu());
        Buttons[1].onClick.AddListener(() => SelectReplay());
    }

    // 이겼는지 졌는지 bool
    public void ShowEndMenu(bool isVictory)
    {
        // 배경을 항상 활성화
        background.SetActive(true);

        // 승리 UI 또는 패배 UI 활성화
        if (isVictory)
        {
            victoryUI.SetActive(true);  // 승리 UI 활성화
            defeatUI.SetActive(false);  // 패배 UI 비활성화
        }
        else
        {
            victoryUI.SetActive(false); // 승리 UI 비활성화
            defeatUI.SetActive(true);   // 패배 UI 활성화
        }
    }

    public void SelectMainMenu()
    {
        currentSelection = 0;
        ArrowMoveButton(currentSelection);
    }

    public void SelectReplay()
    {
        currentSelection = 1;
        ArrowMoveButton(currentSelection);
    }

    void ArrowMoveButton(int index)
    {
        if (arrowHead != null && Buttons != null && index < Buttons.Length && Buttons[index] != null)
        {
            // 화살표의 위치를 현재 선택된 버튼으로 이동
            arrowHead.position = Buttons[index].transform.position + new Vector3(-200, -5, 0); // 화살표 위치 조정
        }
        else
        {
            Debug.LogWarning("ArrowMoveButton: 유효하지 않은 버튼 인덱스거나 버튼이 누락됨!");
        }
    }

    public void ExecuteCurrentSelection()
    {
        if (currentSelection == 0)
        {
            // Main Menu 선택 시, MainMenu 씬으로 이동
            Debug.Log("Main Menu 선택됨");
            SceneManager.LoadScene("MainMenuScene");
        }
        else if (currentSelection == 1)
        {

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.BossPhase1);
            }
            else
            {
                Debug.LogWarning("GameManager를 찾을 수 없습니다.");
            }

            // Replay 선택 시, 현재 씬을 다시 로드
            Debug.Log("Replay 선택됨");


            SceneManager.LoadScene("ARPlaneScene");
        }
        }
    }
