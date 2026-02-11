using System.Collections;
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

    [Header("Invulnerability")]
    public float invulnerabilityDuration = 1.0f; // 1 second of safety
    private bool isInvulnerable = false;
    private float invulnerabilityTimer;
    public float flickerSpeed = .05f;

    [Header("Visuals")]
    public GameObject playerModel; // assign character mesh/model here

    [SerializeField] private float healthSmoothTime = 0.2f; // How fast the bar catches up
    private float healthVelocity; // Required for SmoothDamp
    private float visualHealth;   // The "fake" health that the slider actually shows

    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float rotationSpeed = 720f;
    public float jumpHeight = 2f;
    public float gravity = -30f; // Adjust gravity as needed if feels floaty

    private float verticalVelocity;
    private bool jumpRequested;

    [Header("Attack Settings")]
    public GameObject hitboxPrefab;
    public Transform attackPoint; // Create an empty child on the player as the spawn point
    public float attackCooldown = 0.5f;
    private float lastAttackTime;
    public float stepForwardDistance = .5f;

    [Header("Lock-On Settings")]
    public CinemachineTargetGroup targetGroup;
    public float lockOnRange = 15f;
    public LayerMask enemyLayer; // remember to assign in inspector
    private Transform currentTarget;

    [Header("Lock-On Swapping")]
    public float swapThreshold = 0.5f; // How far you must flick to swap
    private bool canSwap = true; // Prevent rapid flickering between targets
    private Transform lastTarget; // Track who we were looking at last frame

    [Header("UI Settings")]
    public RectTransform reticleUI; // Drag the 'LockOnReticle' Image here
    public Vector3 reticleOffset = Vector3.up;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Initialize Health
        currentHealth = maxHealth;
        visualHealth = maxHealth; // Start the visual health at full too

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }

    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    #region InputSystem
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

    public void OnSwitchTarget(InputValue value)
    {
        if (currentTarget == null) return;

        Vector2 input = value.Get<Vector2>();

        // Only swap if the input is strong enough (a "flick")
        if (canSwap && input.magnitude > swapThreshold)
        {
            // Determine if swapping Left or Right
            bool lookRight = input.x > 0;
            AttemptTargetSwap(lookRight);

            // Start cooldown so we don't swap 60 times per second
            StartCoroutine(SwapCooldown());
        }
    }

    #endregion

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

    #region PlayerMovement
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

    #endregion

    #region HealthFunctions
    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            // SmoothDamp moves visualHealth towards currentHealth over healthSmoothTime
            visualHealth = Mathf.SmoothDamp(visualHealth, currentHealth, ref healthVelocity, healthSmoothTime);

            healthSlider.value = visualHealth;
        }
    }

    public void TakeDamage(float amount)
    {
        // Flag: if already invulnerable, ignore dmg
        if (isInvulnerable) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        StartCoroutine(TriggerInvulnerability());

        Debug.Log($"Player took {amount} damage. Current Health: {currentHealth}");
    }

    private IEnumerator TriggerInvulnerability()
    {
        isInvulnerable = true;

        float elapsed = 0;
        while (elapsed < invulnerabilityDuration)
        {
            // Toggle model visibility to create a flicker effect
            if (playerModel != null)
                playerModel.SetActive(!playerModel.activeSelf);

            yield return new WaitForSeconds(flickerSpeed);
            elapsed += 0.1f;
        }

        // make sure model is visible when finished
        if (playerModel != null) playerModel.SetActive(true);

        isInvulnerable = false;
    }

    #endregion

    #region AttackingLogic
    public void OnAttack(InputValue value)
    {
        if (value.isPressed && Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        lastAttackTime = Time.time;

        // Spawn the hitbox at the attackPoint's position and rotation
        // remember to make an empty child that has a point in front of player
        GameObject hitbox = Instantiate(hitboxPrefab, attackPoint.position, attackPoint.rotation);

        // Parent it to the player so hitbox moves with the player
        hitbox.transform.SetParent(transform);

        if (currentTarget != null)
        {
            // Turn to face the enemy instantly when swinging
            Vector3 dir = currentTarget.position - transform.position;
            dir.y = 0;
            transform.rotation = Quaternion.LookRotation(dir);

            // Add a "step forwards" so the player chases the enemy as they attack
            controller.Move(transform.forward * stepForwardDistance);
        }

        //Debug.Log("Attacking.");
    }

    #endregion


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

            Vector3 worldPos = currentTarget.position + reticleOffset;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            if (screenPos.z > 0)
            {
                // --- THE JITTER FIX ---
                // If this is a brand new target, don't Lerp. Just snap there instantly.
                if (currentTarget != lastTarget)
                {
                    reticleUI.position = screenPos;
                    lastTarget = currentTarget; // Update the reference
                }
                else
                {
                    // Smoothly follow the target if it's the same one
                    reticleUI.position = Vector3.Lerp(reticleUI.position, screenPos, Time.deltaTime * 20f);
                }
            }
            else
            {
                reticleUI.gameObject.SetActive(false);
            }
        }
        else
        {
            if (reticleUI != null && reticleUI.gameObject.activeSelf)
            {
                reticleUI.gameObject.SetActive(false);
                lastTarget = null; // Reset when lock-on is cleared
            }
        }
    }

    private void AttemptTargetSwap(bool lookRight)
    {
        // 1. Find all potential enemies
        Collider[] enemies = Physics.OverlapSphere(transform.position, lockOnRange, enemyLayer);

        Transform bestTarget = null;
        float closestAngle = Mathf.Infinity;

        foreach (var enemy in enemies)
        {
            if (enemy.transform == currentTarget) continue;

            // 2. Calculate direction to this potential enemy relative to the camera
            Vector3 camRight = Camera.main.transform.right;
            Vector3 dirToEnemy = (enemy.transform.position - transform.position).normalized;

            // 3. Check if the enemy is to the left or right of our current target
            Vector3 dirToCurrent = (currentTarget.position - transform.position).normalized;
            float angle = Vector3.SignedAngle(dirToCurrent, dirToEnemy, Vector3.up);

            // If we want to look right, angle must be positive. If left, negative.
            if ((lookRight && angle > 10) || (!lookRight && angle < -10))
            {
                if (Mathf.Abs(angle) < closestAngle)
                {
                    closestAngle = Mathf.Abs(angle);
                    bestTarget = enemy.transform;
                }
            }
        }

        // 4. Switch if we found a valid candidate
        if (bestTarget != null)
        {
            ClearLockOn();
            currentTarget = bestTarget;
            targetGroup.AddMember(currentTarget, 1f, 2f);
        }
    }

    private IEnumerator SwapCooldown()
    {
        canSwap = false;
        yield return new WaitForSeconds(0.25f); // Short delay between swaps
        canSwap = true;
    }

    #endregion





}