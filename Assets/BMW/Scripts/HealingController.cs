using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class HealingController : MonoBehaviour
{

    [Header("Healing Skill Initial Element Connection Settings")]
    [SerializeField] private GameObject heallingEffectPrefab;
    [SerializeField] private AudioSource heallingSound;
                     private GameObject heallingEffectCircle;

    [Header("Healing Skill Initial Settings")]
    [SerializeField] private int HealingAmount = 30;
    [SerializeField] private Vector3 heallingEffectScale = new Vector3( 0.7f, 0.7f, 0.7f);
    [SerializeField] private float heallingEffectDuration = 4.0f;
                     public float delayTime = 0f;

    // Setting up an external script connection
    private PlayerDataManager playerDataManager;
    private PlayerGUI playerGUI;
    private HandGestureController handGestureController;
    private AnimationController animationController;

    void Start()
    {
        playerDataManager = PlayerDataManager.Instance;

        playerGUI = GetComponent<PlayerGUI>();
        handGestureController = GetComponent<HandGestureController>();
        animationController = GetComponent<AnimationController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerDataManager.isControlPlayer) return;

        bool isPressedHealing = Keyboard.current.vKey.wasPressedThisFrame;

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
