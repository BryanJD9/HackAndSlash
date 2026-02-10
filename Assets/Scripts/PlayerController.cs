using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Vector2 moveInput;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    public Slider healthSlider; // Drag slider element in inspector

    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float rotationSpeed = 720f;
    public float jumpHeight = 2f;
    public float gravity = -30f; // Adjust gravity as needed if feels floaty

    private float verticalVelocity;
    private bool jumpRequested;

    [Header("Lock-On Settings")]
    public CinemachineTargetGroup targetGroup;
    public float lockOnRange = 15f;
    public LayerMask enemyLayer; // remember to assign in inspector

    private Transform currentTarget;

    [Header("UI Settings")]
    public RectTransform reticleUI; // Drag the 'LockOnReticle' Image here
    public Vector3 reticleOffset = Vector3.up;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Initialize Health
        currentHealth = maxHealth;

        // Initialize Slider
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

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

    public void OnLockOn(InputValue value)
    {
        if (value.isPressed)
        {
            if (currentTarget == null)
            {
                AttemptLockOn();
            }
            else
            {
                ClearLockOn();
            }
        }
    }

    private void Update()
    {
        UpdateHealthUI();

        // Auto-unlock if enemy is too far away
        if (currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.position);
            if (distance > lockOnRange + 2f) // Give a small buffer
            {
                ClearLockOn();
            }
        }

        UpdateReticle(); // Add this call

        Vector3 finalMovement = CalculateHorizontalMovement() + CalculateVerticalMovement();
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

        // Rotate lock on ui element
        if (currentTarget != null)
        {
            // Face the enemy while locked on
            Vector3 dirToEnemy = currentTarget.position - transform.position;
            dirToEnemy.y = 0; // Keep the player upright

            if (dirToEnemy != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(dirToEnemy);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else if (relativeDirection != Vector3.zero)
        {
            // Standard rotation (face movement direction) if NOT locked on
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

        // Reset the jump request so we don't "double jump" 
        // if the grounded check stays true for two frames
        jumpRequested = false;

        return new Vector3(0, verticalVelocity, 0);
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }


    #region LockOn Function

    private void AttemptLockOn()
    {
        // Find all enemies in range
        Collider[] enemies = Physics.OverlapSphere(transform.position, lockOnRange, enemyLayer);

        float closestDistance = Mathf.Infinity;
        Transform bestTarget = null;

        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                bestTarget = enemy.transform;
            }
        }

        if (bestTarget != null)
        {
            currentTarget = bestTarget;
            // Add enemy to Cinemachine Target Group
            targetGroup.AddMember(currentTarget, 1f, 2f);
        }
    }

    private void ClearLockOn()
    {
        if (currentTarget != null)
        {
            targetGroup.RemoveMember(currentTarget);
            currentTarget = null;
        }
    }

    private void UpdateReticle()
    {
        if (currentTarget != null)
        {
            if (!reticleUI.gameObject.activeSelf)
                reticleUI.gameObject.SetActive(true);

            // Convert 3D world position to 2D screen coordinates
            Vector3 worldPos = currentTarget.position + reticleOffset;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            // Check if the enemy is actually in front of the camera
            // Prevents the UI from showing if the enemy is behind you
            if (screenPos.z > 0)
            {
                reticleUI.position = screenPos;
            }
            else
            {
                reticleUI.gameObject.SetActive(false);
            }
        }
        else
        {
            // Hide if no target
            if (reticleUI != null && reticleUI.gameObject.activeSelf)
            {
                reticleUI.gameObject.SetActive(false);
            }
        }
    }

    #endregion


}