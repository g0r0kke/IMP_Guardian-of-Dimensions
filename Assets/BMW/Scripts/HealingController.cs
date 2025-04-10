using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class HealingController : MonoBehaviour
{

    public GameObject heallingEffectPrefab;
    public AudioSource heallingSound;
    public int HealingAmount = 5;
    public Vector3 heallingEffectScale = new Vector3( 0.7f, 0.7f, 0.7f);
    public float heallingEffectDuration = 4.0f;

    public float delayTime = 0f;
    private GameObject heallingEffectCircle;
    private PlayerGUI playerGUI;
    private HandGestureController handGestureController;
    private AnimationController animationController;

    void Start()
    {
        playerGUI = GetComponent<PlayerGUI>();
        handGestureController = GetComponent<HandGestureController>();
        animationController = GetComponent<AnimationController>();
    }

    // Update is called once per frame
    void Update()
    {
        bool isPressedHealing = Input.GetKeyDown(KeyCode.V);

        if ((isPressedHealing || handGestureController.isHealingGesture) && delayTime <= 0 && playerGUI.playerHealth < playerGUI.playerHealthLimit)
        {
            animationController.HealingAnimation();
            Invoke("Healing", 1.3f);
            delayTime = playerGUI.healingSkillDelay;
        }
        handGestureController.isHealingGesture = false;

        if (delayTime > 0)
        {
            delayTime -= Time.deltaTime;
            if (delayTime <= 0) delayTime = 0f;
        }

    }

    void Healing()
    {
        heallingSound.Play();

        playerGUI.IncreaseHealth(HealingAmount);

        heallingEffectCircle = Instantiate(heallingEffectPrefab, transform.position, Quaternion.identity);

        heallingEffectCircle.transform.localScale = heallingEffectScale;

        heallingEffectCircle.transform.parent = transform;

        Destroy(heallingEffectCircle, heallingEffectDuration);
    }
}
