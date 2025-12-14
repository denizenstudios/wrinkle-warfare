using UnityEngine;

/// <summary>
/// Simple marker script for player spawn locations.
/// Just attach this to empty GameObjects positioned where you want players to spawn.
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    private void OnDrawGizmosSelected()
    {
        // Draw a red sphere in the editor to visualize spawn point
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.5f);
        
        // Draw a forward direction indicator
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
    }
}
