using UnityEngine;
using UnityEngine.InputSystem;
public class SamplePlayer : MonoBehaviour
{
    public float distancePerFrame = 2.0f;
    private Vector2 movement;

    void Update()
    {
        if (Keyboard.current != null)
        {
            movement = new Vector2(
                Keyboard.current.aKey.isPressed ? -1 : Keyboard.current.dKey.isPressed ? 1 : 0,
                Keyboard.current.wKey.isPressed ? 1 : Keyboard.current.sKey.isPressed ? -1 : 0
            );

            transform.Translate(new Vector3(movement.x, 0, movement.y) * distancePerFrame * Time.deltaTime);
        }
    }
}