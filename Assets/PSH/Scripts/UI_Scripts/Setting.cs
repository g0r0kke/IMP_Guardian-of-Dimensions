using UnityEngine;

public class Setting : MonoBehaviour
{
    public GameObject settingUI;

    public void OnClickSetting()
    {
        settingUI.SetActive(true);
    }

    public void OnClickClose()
    {
        settingUI.SetActive(false);
    }
}