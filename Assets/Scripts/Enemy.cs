using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 50f;
    private float currentHealth;

    [Header("Movement & Gravity")]
    private CharacterController controller;
    private float verticalVelocity;
    public float gravity = -30f; // Matches your Player's gravity

    [Header("Knockback")]
    private Vector3 impactVelocity;
    public float drag = 5f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        currentHealth = maxHealth;
    }

    private void Update()
    {
        ApplyGravity();

        // Calculate the final movement vector
        // impactVelocity handles the horizontal knockback
        // Vector3.up * verticalVelocity handles the falling
        Vector3 finalMove = impactVelocity + (Vector3.up * verticalVelocity);

        controller.Move(finalMove * Time.deltaTime);

        // Gradually reduce knockback over time
        impactVelocity = Vector3.Lerp(impactVelocity, Vector3.zero, drag * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0)
        {
            // Keep the enemy snapped to the ground
            verticalVelocity = -2f;
        }
        else
        {
            // Apply gravity over time
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    public void TakeDamage(float amount, Vector3 hitDirection, float knockbackForce)
    {
        currentHealth -= amount;

        // Flatten direction so they don't get knocked into the air/floor
        hitDirection.y = 0;
        impactVelocity = hitDirection.normalized * knockbackForce;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}