using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class LockOnSystem : MonoBehaviour
{
    [Header("Settings")]
    public float detectionRadius = 15f;
    public LayerMask enemyLayer; // WE MUST SET THIS IN INSPECTOR!

    [Header("Setup")]
    public CinemachineTargetGroup targetGroup;

    public Transform currentTarget;
    private CinemachineCamera cam; // Automatically found

    private void Awake()
    {
        // Auto-find the camera if it's active in the scene
        cam = FindAnyObjectByType<CinemachineCamera>();
    }

    // THIS FUNCTION MUST BE LINKED TO YOUR INPUT
    public void OnLockOn(InputValue value)
    {
        if (value.isPressed)
        {
            if (currentTarget == null)
            {
                FindTarget();
            }
            else
            {
                ClearTarget();
            }
        }
    }

    void FindTarget()
    {
        // 1. Find enemies
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);

        // Debugging: verify we see them
        Debug.Log($"Found {enemies.Length} enemies on the Enemy layer.");

        if (enemies.Length == 0) return;

        // 2. Simple logic: just pick the first one found for now
        currentTarget = enemies[0].transform;

        // 3. Update Cinemachine Target Group
        // We look for the second slot (index 1) and assign the enemy
        if (targetGroup.Targets.Count > 1)
        {
            var targets = targetGroup.Targets;
            targets[1].Object = currentTarget;
            targets[1].Weight = 1f;
        }

        Debug.Log("LOCKED ON: " + currentTarget.name);
    }

    void ClearTarget()
    {
        currentTarget = null;

        // Clear the enemy from the group
        if (targetGroup.Targets.Count > 1)
        {
            var targets = targetGroup.Targets;
            targets[1].Object = null;
            targets[1].Weight = 0f;
        }

        Debug.Log("LOCK CLEARED");
    }

    // Visualize the range in the Scene View
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}