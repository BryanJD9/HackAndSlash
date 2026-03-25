using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public Animator animator;

    [Header("Combo Settings")]
    public float comboResetTime = 1.5f; // How long before the combo resets to Attack 1
    private float lastAttackTime;
    private int comboStep = 0;

    private void Start()
    {
        // Auto-grab the animator from the child model
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    // This catches the Broadcast Message from the Input System
    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        // If too much time has passed since the last swing, reset the combo
        if (Time.time - lastAttackTime > comboResetTime)
        {
            comboStep = 0;
        }

        // Record the time we clicked
        lastAttackTime = Time.time;

        // Increase the combo step
        comboStep++;

        // Trigger the attack in the Animator
        animator.SetTrigger("Attack");

        // Cap the combo at 3 hits, so it loops back to the start if we keep mashing
        if (comboStep >= 3)
        {
            comboStep = 0;
        }
    }


}