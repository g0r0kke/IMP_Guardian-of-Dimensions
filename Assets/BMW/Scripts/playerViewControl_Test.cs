using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class playerViewControl : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public float mouseSencivity = 600f;
    public Transform playerBody;
    public Texture2D crosshair;
    public int crosshairSize = 50;
    private float xRotation = 0f;


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSencivity *Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSencivity * Time.deltaTime;

        xRotation -= mouseY;

        //
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        //
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        //
        playerBody.Rotate(Vector3.up * mouseX);

    }
    private void OnGUI()
    {
        float xMin = (Screen.width / 2) - (crosshairSize / 2);
        float yMin = (Screen.height / 2) - (crosshairSize / 2);
        GUI.DrawTexture(new Rect(xMin, yMin, crosshairSize, crosshairSize), crosshair);
    }
}
