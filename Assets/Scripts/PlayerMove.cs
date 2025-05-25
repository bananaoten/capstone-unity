using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    public FixedJoystick joystick;
    public float SpeedMove = 2f;
    public float gravityMultiplier = 3.0f;

    private CharacterController controller;
    private float gravity = -9.81f;
    private float velocityY; // vertical velocity for gravity
    private Vector3 moveDirection;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        ApplyMovement();
        ApplyGravity();
    }

    void ApplyMovement()
    {
        Vector3 inputDirection = transform.right * joystick.Horizontal + transform.forward * joystick.Vertical;
        moveDirection = new Vector3(inputDirection.x, moveDirection.y, inputDirection.z);
    }

    void ApplyGravity()
    {
        if (controller.isGrounded && velocityY < 0)
        {
            velocityY = -1f; // slight downward force to keep grounded
        }
        else
        {
            velocityY += gravity * gravityMultiplier * Time.deltaTime;
        }

        moveDirection.y = velocityY;

        controller.Move(moveDirection * SpeedMove * Time.deltaTime);
    }
}
