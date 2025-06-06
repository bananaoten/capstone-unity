using UnityEngine;
using UnityEngine.InputSystem;

public class VRInputMovement : MonoBehaviour
{
    public Transform playerRig; // Assign your PlayerRig (VR root)
    public float moveSpeed = 2f;

    void Update()
    {
        Vector2 moveInput = Vector2.zero;

        // ✅ Editor / PC WASD
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveInput.y += 1;
            if (Keyboard.current.sKey.isPressed) moveInput.y -= 1;
            if (Keyboard.current.aKey.isPressed) moveInput.x -= 1;
            if (Keyboard.current.dKey.isPressed) moveInput.x += 1;
        }

        // ✅ Gamepad (PS4 controller)
        if (Gamepad.current != null)
        {
            Vector2 gamepadInput = Gamepad.current.leftStick.ReadValue();
            if (gamepadInput != Vector2.zero) moveInput = gamepadInput;
        }

        // Move relative to where the camera is looking
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        forward.y = 0; right.y = 0;
        forward.Normalize(); right.Normalize();

        Vector3 move = forward * moveInput.y + right * moveInput.x;
        playerRig.position += move * moveSpeed * Time.deltaTime;
    }
}
