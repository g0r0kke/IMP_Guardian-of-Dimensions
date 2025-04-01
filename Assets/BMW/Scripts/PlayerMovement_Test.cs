using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    private CharacterController controller;
    public float speed = 12.0f;
    public float gravity = -9.81f;
    public float jumpHeight = 2.0f;
    public float pushPower = 5.0f;

    private Vector3 velocity;
    private bool isGrounded;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = 0f;
        }

        // WASD movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 movement = transform.right * x + transform.forward * z;

        controller.Move(movement * speed * Time.deltaTime);

        
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Get the jump velocity needed to jump a certain height: sqrt(h * -2*g)
            velocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);


    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Enemy"))
        {
            Rigidbody enemyRb = hit.collider.attachedRigidbody;

            if (enemyRb == null)
            {
                return;
            }

            if(hit.moveDirection.y < -0.3f)
            {
                return;
            }

            Debug.Log(hit.gameObject.name);

            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

            enemyRb.AddForce(pushDir * pushPower, ForceMode.Force);
        }

    } 
}
