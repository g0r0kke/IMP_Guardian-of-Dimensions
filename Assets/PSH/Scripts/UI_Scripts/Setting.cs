using UnityEngine;

public class Setting : MonoBehaviour
{
    public GameObject settingUI; // Setting UI object

    // Function called when the setting button is clicked
    public void OnClickSetting()
    {
        settingUI.SetActive(true); // Activate the setting UI
    }

    // Function called when the close button is clicked
    public void OnClickClose()
    {
        settingUI.SetActive(false); // Deactivate the setting UI
    }
}
