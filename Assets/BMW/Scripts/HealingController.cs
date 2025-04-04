using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class HealingController : MonoBehaviour
{

    public GameObject heallingEffectPrefab;
    public int HealingAmount = 5;
    public Vector3 heallingEffectScale = new Vector3( 0.7f, 0.7f, 0.7f);
    public float heallingEffectDuration = 4.0f;

    public float delayTime = 0f;
    private GameObject heallingEffectCircle;
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

        heallingEffectCircle = Instantiate(heallingEffectPrefab, transform.position, Quaternion.identity);

        heallingEffectCircle.transform.localScale = heallingEffectScale;

        heallingEffectCircle.transform.parent = transform;

        Destroy(heallingEffectCircle, heallingEffectDuration);
    }
}
