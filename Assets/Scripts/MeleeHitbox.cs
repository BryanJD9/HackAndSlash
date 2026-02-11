using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    public float damage = 20f;
    public float knockbackStrength = 10f;
    public float lifetime = 0.1f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Try to find the Enemy component
        Enemy enemy = other.GetComponent<Enemy>();

        if (enemy != null)
        {
            // Calculate direction from the player to the enemy
            Vector3 knockbackDir = other.transform.position - transform.position;

            enemy.TakeDamage(damage, knockbackDir, knockbackStrength);
        }
    }
}