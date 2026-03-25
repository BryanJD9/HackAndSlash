using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public Animator animator;

    [Header("Combo Settings")]
    public float comboResetTime = 1.5f;
    public float attackCooldown = 0.4f; // ADD THIS: Minimum time between clicks

    private float lastAttackTime;
    private int comboStep = 0;

    private void Start()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        // ADD THIS: Ignore the click if we pressed it too soon after the last swing
        if (Time.time - lastAttackTime < attackCooldown)
        {
            return; // Exit the function completely
        }

        // If too much time has passed, reset the combo
        if (Time.time - lastAttackTime > comboResetTime)
        {
            comboStep = 0;
        }

        lastAttackTime = Time.time;
        comboStep++;
        animator.SetTrigger("Attack");

        if (comboStep >= 3)
        {
            comboStep = 0;
        }
    }
}