using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    public GameObject[] tutorialPages; // 1~4번 튜토리얼 페이지들
    public Button rightButton;
    public Button leftButton;

    private int currentIndex = 0;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        rightButton.onClick.AddListener(NextPage);
        leftButton.onClick.AddListener(PrevPage);

    }

    private void OnEnable()
    {
        currentIndex = 0;
        UpdateTutorialPage();
    }


    public void Close()
    {
        StartCoroutine(CloseAfterDelay());
    }

    private IEnumerator CloseAfterDelay()
    {
        animator.SetTrigger("close");
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
        animator.ResetTrigger("close");
    }

    private void UpdateTutorialPage()
    {
        for (int i = 0; i < tutorialPages.Length; i++)
        {
            tutorialPages[i].SetActive(i == currentIndex);
        }

        // 버튼 상태 업데이트
        leftButton.interactable = currentIndex > 0;
        rightButton.interactable = currentIndex < tutorialPages.Length - 1;
    }

    private void NextPage()
    {
        if (currentIndex < tutorialPages.Length - 1)
        {
            currentIndex++;
            UpdateTutorialPage();
        }
    }

    private void PrevPage()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdateTutorialPage();
        }
    }
}



