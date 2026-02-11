using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardZone : MonoBehaviour
{
    public float damageAmount = 10f;
    public float damageInterval = 3f;

    // Track players currently inside the zone
    private Dictionary<Collider, Coroutine> activeDamageRoutines = new Dictionary<Collider, Coroutine>();

    private void OnTriggerEnter(Collider damageReceiver)
    {
        // Check if the object that entered has a PlayerController
        PlayerController player = damageReceiver.GetComponent<PlayerController>();

        if (player != null && !activeDamageRoutines.ContainsKey(damageReceiver))
        {
            // Start the repeating damage "clock"
            Coroutine routine = StartCoroutine(ApplyPeriodicDamage(player));
            activeDamageRoutines.Add(damageReceiver, routine);
        }
    }

    private void OnTriggerExit(Collider damageReceiver)
    {
        if (activeDamageRoutines.ContainsKey(damageReceiver))
        {
            // Stop the damage clock when they leave
            StopCoroutine(activeDamageRoutines[damageReceiver]);
            activeDamageRoutines.Remove(damageReceiver);
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
