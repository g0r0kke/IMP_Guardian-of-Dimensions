using UnityEngine;
using System.Collections;

public class DefenseController : MonoBehaviour
{

    public GameObject ShieldPrefab;
    public Vector3 shieldScale = new Vector3(1.0f, 1.0f, 1.0f);
    public float shieldDuration = 4.0f;
    public float shieldDistance = 1.0f;

    private GameObject shieldSphere;
    public float delayTime = 0f;
    private PlayerGUI playerGUI;
    private DamageController damageController;
    private HandGestureController handGestureController;
    private AnimationController animationController;

    void Start()
    {
        playerGUI = GetComponent<PlayerGUI>();
        damageController = GetComponent<DamageController>();
        handGestureController = GetComponent<HandGestureController>();
        animationController = GetComponent<AnimationController>();
    }

    // Update is called once per frame
    void Update()
    {
        bool isPressedDefense = Input.GetKeyDown(KeyCode.C);

        if ((isPressedDefense || handGestureController.isDefenseGesture) && delayTime <= 0)
        {
            animationController.DefenseAnimation();
            StartCoroutine(Defense());
            delayTime = playerGUI.defenseSkillDelay;
            handGestureController.isDefenseGesture = false;
        }
        else
        {
            handGestureController.isDefenseGesture = false;
        }

        if (delayTime > 0)
        {
            delayTime -= Time.deltaTime;
            if (delayTime <= 0) delayTime = 0f;
        }
    }

    IEnumerator Defense()
    {
        yield return new WaitForSeconds(1.1f);

        if (shieldSphere != null)
        {
            Destroy(shieldSphere);
        }

        damageController.isDefense = true;

        //Vector3 shieldPosition = transform.position + transform.forward * shieldDistance;

        shieldSphere = Instantiate(ShieldPrefab, transform.position, transform.rotation);

        shieldSphere.transform.localScale = shieldScale;

        shieldSphere.transform.parent = transform;

        yield return new WaitForSeconds(shieldDuration);

        Destroy(shieldSphere);

        damageController.isDefense = false;
    }
}
