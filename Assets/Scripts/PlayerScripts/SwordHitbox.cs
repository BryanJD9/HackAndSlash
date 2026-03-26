using System.Collections.Generic;
using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    public float damage = 20f;
    private bool isSensing = false;

    // This list keeps track of who we already hit in ONE swing
    private List<GameObject> alreadyHit = new List<GameObject>();

    // Called by the Animator/Relay to start looking for hits
    public void StartSensing()
    {
        isSensing = true;
        alreadyHit.Clear(); // Clear the list for the new swing
    }

    // Called by the Animator/Relay to stop looking
    public void StopSensing()
    {
        isSensing = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isSensing) return;

        // 1. Check if we hit an enemy and haven't hit them in THIS swing yet
        if (other.CompareTag("Enemy") && !alreadyHit.Contains(other.gameObject))
        {
            // 2. Try to find the Enemy script on the object we hit
            if (other.TryGetComponent(out Enemy enemy))
            {
                alreadyHit.Add(other.gameObject);

                // 3. Invoke the damage function!
                enemy.TakeDamage(damage);
            }
        }
    }



}