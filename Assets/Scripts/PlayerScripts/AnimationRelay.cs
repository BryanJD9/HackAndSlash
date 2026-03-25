using UnityEngine;

public class AnimationRelay : MonoBehaviour
{
    private PlayerCombat combat;

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
}