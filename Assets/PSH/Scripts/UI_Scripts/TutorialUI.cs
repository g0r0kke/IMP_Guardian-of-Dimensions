using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    public GameObject[] tutorialPages; // Tutorial pages 1~4
    public Button rightButton;  // Right button (to go to next page)
    public Button leftButton; // Left button (to go to previous page)

    private int currentIndex = 0; // Current page index

    private Animator animator; // Animator component

    private void Awake()
    {
        animator = GetComponent<Animator>(); // Initialize the Animator component

        rightButton.onClick.AddListener(NextPage);  // When the right button is clicked, go to the next page
        leftButton.onClick.AddListener(PrevPage); // When the left button is clicked, go to the previous page
    }

    private void OnEnable()
    {
        currentIndex = 0; // Initialize to the first page when the tutorial starts
        UpdateTutorialPage(); // Update the tutorial page
    }

    // Close the tutorial UI
    public void Close()
    {
        StartCoroutine(CloseAfterDelay()); // Close the UI after a delay
    }

    // Coroutine to close the tutorial UI after a delay
    private IEnumerator CloseAfterDelay()
    {
        animator.SetTrigger("close"); // Trigger the close animation
        yield return new WaitForSeconds(0.5f); // Wait for 0.5 seconds
        gameObject.SetActive(false); // Deactivate the game object
        animator.ResetTrigger("close"); // Reset the close animation trigger
    }

    // Update the tutorial page
    private void UpdateTutorialPage()
    {
        for (int i = 0; i < tutorialPages.Length; i++)
        {
            tutorialPages[i].SetActive(i == currentIndex); // Only activate the current page
        }

        // Update button states
        leftButton.interactable = currentIndex > 0; // Enable the left button if it's not the first page
        rightButton.interactable = currentIndex < tutorialPages.Length - 1;  // Enable the right button if it's not the last page
    }

    // Go to the next page
    private void NextPage()
    {
        if (currentIndex < tutorialPages.Length - 1)
        {
            currentIndex++; // Increment the page index
            UpdateTutorialPage(); // Update the page
        }
    }

    // Go to the previous page
    private void PrevPage()
    {
        if (currentIndex > 0)
        {
            currentIndex--; // Decrement the page index
            UpdateTutorialPage(); // Update the page
        }
    }
}


