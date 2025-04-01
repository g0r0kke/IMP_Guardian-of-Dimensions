using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class HealingController : MonoBehaviour
{

    public int HealingAmount = 5;

    public float delayTime = 0f;
    private PlayerGUI playerGUI;

    void Start()
    {
        playerGUI = GetComponent<PlayerGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        bool healingPressed = Input.GetKeyDown(KeyCode.V);

        if (healingPressed && delayTime <= 0 && playerGUI.playerHealth < playerGUI.playerHealthLimit)
        {
            Healing();
            delayTime = playerGUI.healingSkillDelay;
        }

        if (delayTime > 0)
        {
            delayTime -= Time.deltaTime;
            if (delayTime <= 0) delayTime = 0f;
        }

    }

    void Healing()
    {
        playerGUI.IncreaseHealth(HealingAmount);
    }
}
