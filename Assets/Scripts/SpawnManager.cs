using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Manages player spawn positions for multiplayer games.
/// Assigns spawn points to players as they join (up to 4 players max).
/// </summary>
public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [SerializeField] private GameObject spawnPoint1;
    [SerializeField] private GameObject spawnPoint2;
    [SerializeField] private GameObject spawnPoint3;
    [SerializeField] private GameObject spawnPoint4;

    private GameObject[] spawnPoints;
    private int nextSpawnIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Build array of assigned spawn points
        spawnPoints = new[] { spawnPoint1, spawnPoint2, spawnPoint3, spawnPoint4 };

        int validCount = 0;
        foreach (var point in spawnPoints)
        {
            if (point != null) validCount++;
        }

        if (validCount == 0)
        {
            Debug.LogWarning("SpawnManager: No spawn points assigned! Players will spawn at (0,0,0)");
        }
        else
        {
            Debug.Log($"SpawnManager: Found {validCount} spawn points assigned");
        }
    }

    /// <summary>
    /// Get the next available spawn point for a new player.
    /// </summary>
    public Vector3 GetNextSpawnPosition()
    {
        // Find next non-null spawn point
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[nextSpawnIndex] != null)
            {
                Vector3 spawnPos = spawnPoints[nextSpawnIndex].transform.position;
                nextSpawnIndex = (nextSpawnIndex + 1) % spawnPoints.Length;
                return spawnPos;
            }

            nextSpawnIndex = (nextSpawnIndex + 1) % spawnPoints.Length;
        }

        Debug.LogWarning("SpawnManager: No valid spawn points found!");
        return Vector3.zero;
    }

    /// <summary>
    /// Get spawn point rotation (direction player should face)
    /// </summary>
    public Quaternion GetNextSpawnRotation()
    {
        int currentIndex = (nextSpawnIndex - 1 + spawnPoints.Length) % spawnPoints.Length;

        if (spawnPoints[currentIndex] != null)
            return spawnPoints[currentIndex].transform.rotation;

        return Quaternion.identity;
    }
}
