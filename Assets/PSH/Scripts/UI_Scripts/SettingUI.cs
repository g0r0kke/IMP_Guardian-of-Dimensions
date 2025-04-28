using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    private Animator animator; // Animator component

    private void Awake()
    {
        animator = GetComponent<Animator>(); // Initialize the Animator component
    }

    private void OnEnable()
    {
        
    }

    public void SetControlMode(int ControlType)
    {
       // Settings.controlType = (EControlType)ControlType;
    }

    public void Close()
    {
        StartCoroutine(CloseAfterDelay()); // Close the UI after a delay
    }

    private IEnumerator CloseAfterDelay()
    {
        animator.SetTrigger("close"); // Trigger close animation
        yield return new WaitForSeconds(0.5f); // Wait for 0.5 seconds
        gameObject.SetActive(false); // Deactivate the game object
        animator.ResetTrigger("close");  // Reset the close animation trigger
    }

}