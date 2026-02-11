using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardZone : MonoBehaviour
{
    public float damageAmount = 10f;
    public float damageInterval = 3f;

    // Track players currently inside the zone
    private Dictionary<Collider, Coroutine> activeDamageRoutines = new Dictionary<Collider, Coroutine>();

    private void OnTriggerEnter(Collider foreignCollider)
    {
        // Check if the object that entered has a PlayerController
        PlayerController player = foreignCollider.GetComponent<PlayerController>();

        if (player != null && !activeDamageRoutines.ContainsKey(foreignCollider))
        {
            // Start the repeating damage "clock"
            Coroutine routine = StartCoroutine(ApplyPeriodicDamage(player));
            activeDamageRoutines.Add(foreignCollider, routine);
        }
    }

    private void OnTriggerExit(Collider foreignCollider)
    {
        if (activeDamageRoutines.ContainsKey(foreignCollider))
        {
            // Stop the damage clock when they leave
            StopCoroutine(activeDamageRoutines[foreignCollider]);
            activeDamageRoutines.Remove(foreignCollider);
        }
    }

    private IEnumerator ApplyPeriodicDamage(PlayerController player)
    {
        while (true)
        {
            player.TakeDamage(damageAmount);
            // Wait for the specified interval before the next tick
            yield return new WaitForSeconds(damageInterval);
        }
    }
}
