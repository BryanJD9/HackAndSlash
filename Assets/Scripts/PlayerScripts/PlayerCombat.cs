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
        if (Time.time - lastAttackTime < attackCooldown) return;

        if (Time.time - lastAttackTime > comboResetTime) comboStep = 0;

        lastAttackTime = Time.time;
        comboStep++; // If this becomes 1, we are doing Attack 1

        // CALCULATE DAMAGE: 
        // Step 1 = 20, Step 2 = 30, Step 3 = 40
        if (sword != null)
        {
            sword.damage = baseDamage + ((comboStep - 1) * 10f);
        }

        animator.SetTrigger("Attack");
        isAttacking = true;

        if (comboStep > 3) comboStep = 1;
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


}