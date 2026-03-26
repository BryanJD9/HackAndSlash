using System.Collections;
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

    [Header("Damage Settings")]
    public float baseDamage = 20f;
    public SwordHitbox sword; // Drag your Sword object here

    [Header("Finisher Settings")]
    public float finisherCooldown = 2.0f; // Long delay after hit 3
    private bool isFinisherRecovery = false;

    private void Start()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        // Fail-safe: If the combo timer expires, force-unlock movement
        if (isAttacking && Time.time - lastAttackTime > comboResetTime)
        {
            isAttacking = false;
            comboStep = 0;
        }
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
        // 1. If we are still recovering from the big 3rd hit, stop here
        if (isFinisherRecovery) return;

        if (isAttacking && Time.time - lastAttackTime < attackCooldown) return;

        if (Time.time - lastAttackTime > comboResetTime) comboStep = 0;

        lastAttackTime = Time.time;
        comboStep++;

        // 2. Logic for when we hit the end of the combo
        if (comboStep > 3)
        {
            comboStep = 1; // Prepare for next time
        }

        if (sword != null)
        {
            sword.damage = baseDamage + ((comboStep - 1) * 10f);
        }

        animator.SetTrigger("Attack");
        isAttacking = true;

        // 3. If this WAS the 3rd hit, start the "Recovery" cooldown
        if (comboStep == 3)
        {
            StartCoroutine(FinisherCooldownRoutine());
        }
    }

    // Update this to accept the 'int' from the Animation Event
    public void FinishAttack(int attackIndex)
    {
        // Only unlock if the event matches the swing we are currently on!
        // This ignores the 'leftover' events from previous swings.
        if (attackIndex == comboStep)
        {
            isAttacking = false;
            // Debug.Log($"Legit unlock from Attack {attackIndex}");
        }
    }

    private IEnumerator FinisherCooldownRoutine()
    {
        isFinisherRecovery = true;

        // Wait for the duration of the extra cooldown
        yield return new WaitForSeconds(finisherCooldown);

        isFinisherRecovery = false;
        comboStep = 0; // Reset to hit 1 after the long break
    }


}