using UnityEngine;

public class AnimationRelay : MonoBehaviour
{
    private PlayerCombat combat;
    public SwordHitbox sword; // Drag sword object here in the Inspector

    void Start()
    {
        // Find the combat script on the parent object
        combat = GetComponentInParent<PlayerCombat>();
    }

    // Inside AnimationRelay.cs
    public void FinishAttack(int attackIndex)
    {
        if (combat != null)
        {
            combat.FinishAttack(attackIndex);
        }
    }

    // New bridge methods for the sword
    public void StartSensing() => sword?.StartSensing();
    public void StopSensing() => sword?.StopSensing();

}