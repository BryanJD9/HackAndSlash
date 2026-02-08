using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 moveDirection;

    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float rotationSpeed = 720f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    // This method is called by the Player Input component
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void Update()
    {
        ApplyMovement();
    }

    private void ApplyMovement()
    {
        // 1. Get the camera's forward and right vectors
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        // 2. Flatten them so the player doesn't move up/down
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // 3. Create the movement direction relative to the camera
        // moveInput.y is 'Vertical' (W/S), moveInput.x is 'Horizontal' (A/D)
        Vector3 relativeDirection = (camForward * moveInput.y) + (camRight * moveInput.x);

        // 4. Apply movement
        controller.Move(relativeDirection * moveSpeed * Time.deltaTime);

        // 5. Rotate to face that relative direction
        if (relativeDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(relativeDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }


}