using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Vector2 moveInput;

    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float rotationSpeed = 720f;
    public float jumpHeight = 2f;
    public float gravity = -30f; // Adjust gravity as needed if floaty

    private float verticalVelocity;
    private bool jumpRequested;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        // request to true here. handle the logic in Update
        if (value.isPressed)
        {
            jumpRequested = true;
        }
    }

    private void Update()
    {
        Vector3 finalMovement = CalculateHorizontalMovement() + CalculateVerticalMovement();

        // calculates movement all at once: ensures isGrounded updates correctly for the next frame
        controller.Move(finalMovement * Time.deltaTime);
    }

    private Vector3 CalculateHorizontalMovement()
    {
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 relativeDirection = (camForward * moveInput.y) + (camRight * moveInput.x);

        if (relativeDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(relativeDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        return relativeDirection * moveSpeed;
    }

    private Vector3 CalculateVerticalMovement()
    {
        if (controller.isGrounded)
        {
            // Reset velocity but keep a small downward force to stay grounded
            if (verticalVelocity < 0) verticalVelocity = -2f;

            if (jumpRequested)
            {
                // Physics Formula: v = sqrt(h * -2 * g)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        else
        {
            // Apply gravity over time
            verticalVelocity += gravity * Time.deltaTime;
        }

        // IMPORTANT: Reset the jump request so we don't "double jump" 
        // if the grounded check stays true for two frames
        jumpRequested = false;

        return new Vector3(0, verticalVelocity, 0);
    }
}