using UnityEngine;
using System;

[RequireComponent(typeof(CharacterController))]
public class Enemy : MonoBehaviour
{
    // This event notifies the spawner when the enemy dies
    public event Action OnEnemyDeath;

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

    // Add this to your Enemy.cs
    private bool isBeingPulled = false;

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

    //public void TakeDamage(float amount/*, Vector3 hitDirection, float knockbackForce*/)
    //{
    //    currentHealth -= amount;

    //    // Flatten direction so they don't get knocked into the air/floor
    //    //hitDirection.y = 0;
    //    //impactVelocity = hitDirection.normalized * knockbackForce;

    //    if (currentHealth <= 0)
    //    {
    //        Die();
    //    }
    //}

    // Simplified TakeDamage - only requires the amount
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage! HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }

    }

    private void Die()
    {
        OnEnemyDeath?.Invoke();
        
        // Remembers to remove this enemy from Target Group before destroying
        var targetGroup = FindFirstObjectByType<Unity.Cinemachine.CinemachineTargetGroup>();
        if (targetGroup != null)
        {
            targetGroup.RemoveMember(transform);
        }

        Destroy(gameObject);
    }


    public void StartPull()
    {
        isBeingPulled = true;
        verticalVelocity = 0;
        //if (animator != null) animator.SetTrigger("HitStagger"); // Or "Airborne"
    }

    public void EndPull()
    {
        isBeingPulled = false;
    }

    // Update your Move logic in Update() to allow external forces
    public void ExternalMove(Vector3 velocity)
    {
        controller.Move(velocity * Time.deltaTime);
    }



}