using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    public float damage = 20f;
    public float lifetime = 0.1f; // How long the hitbox stays active

    void Start()
    {
        // Automatically destroy the hitbox after a fraction of a second
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // For now, generic "Enemy" tag/component
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Hit " + other.name);
            //TODO: logic to damage enemy goes here
        }
    }

}