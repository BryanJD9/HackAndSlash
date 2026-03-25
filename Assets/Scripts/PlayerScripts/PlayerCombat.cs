using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public Animator animator;

    [Header("Combo Settings")]
    public float comboResetTime = 1.1f;
    public float attackCooldown = 0.4f;

    private float lastAttackTime;
    private int comboStep = 0;

    private void Start()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    // NEW: This tells the PlayerController if we are busy swinging
    public bool isAttacking { get; private set; }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        if (Time.time - lastAttackTime > comboResetTime) comboStep = 0;

        lastAttackTime = Time.time;
        comboStep++;

        // Start the attack
        animator.SetTrigger("Attack");
        isAttacking = true;

        if (comboStep >= 3) comboStep = 0;
    }

    // NEW: We will call this from the Animator to "unlock" movement
    public void FinishAttack()
    {
        isAttacking = false;
    }
}