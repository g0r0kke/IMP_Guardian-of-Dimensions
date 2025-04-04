using UnityEngine;

public class DefenseController : MonoBehaviour
{

    public GameObject ShieldPrefab;
    public Vector3 shieldScale = new Vector3(1.0f, 1.0f, 1.0f);
    public float shieldDuration = 4.0f;
    public float shieldDistance = 1.0f;

    private GameObject shieldSphere;
    public float delayTime = 0f;
    private PlayerGUI playerGUI;

    void Start()
    {
        playerGUI = GetComponent<PlayerGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        bool defensePressed = Input.GetKeyDown(KeyCode.C);

        if (defensePressed && delayTime <= 0)
        {
            Defense();
            delayTime = playerGUI.defenseSkillDelay;
        }

        if (delayTime > 0)
        {
            delayTime -= Time.deltaTime;
            if (delayTime <= 0) delayTime = 0f;
        }
    }

    void Defense()
    {
        if (shieldSphere != null)
        {
            Destroy(shieldSphere);
        }
        
        //Vector3 shieldPosition = transform.position + transform.forward * shieldDistance;

        shieldSphere = Instantiate(ShieldPrefab, transform.position, transform.rotation);

        shieldSphere.transform.localScale = shieldScale;

        shieldSphere.transform.parent = transform;

        Destroy(shieldSphere, shieldDuration);
    }
}
