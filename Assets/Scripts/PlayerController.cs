using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Vector2 moveInput;

    // We need to reference the LockOnSystem to know if we are fighting
    private LockOnSystem lockOnSystem;

    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float rotationSpeed = 720f;

    private float verticalVelocity;
    private float gravity = -20f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        lockOnSystem = GetComponent<LockOnSystem>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void Update()
    {
        ApplyGravity();
        ApplyMovement();

        // This handles facing the enemy when locked on
        ApplyLockOnRotation();
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        controller.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
    }

    private void ApplyMovement()
    {
        // 1. Get camera vectors
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        // 2. Flatten them
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // 3. Calculate direction relative to camera
        Vector3 relativeDirection = (camForward * moveInput.y) + (camRight * moveInput.x);

        // 4. Move the character
        controller.Move(relativeDirection * moveSpeed * Time.deltaTime);

        // 5. FREE MOVEMENT ROTATION
        // Only rotate towards movement direction if we are NOT locked on
        // (If we ARE locked on, the ApplyLockOnRotation function handles rotation)
        if (relativeDirection != Vector3.zero && lockOnSystem.currentTarget == null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(relativeDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void ApplyLockOnRotation()
    {
        // Check if we have a target (and make sure the enemy still exists)
        if (lockOnSystem.currentTarget != null)
        {
            // Calculate direction to enemy
            Vector3 dirToEnemy = lockOnSystem.currentTarget.position - transform.position;
            dirToEnemy.y = 0; // Keep the rotation flat on the ground

            if (dirToEnemy != Vector3.zero)
            {
                // Smoothly rotate the character to face the enemy
                Quaternion targetRotation = Quaternion.LookRotation(dirToEnemy);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
    }

}