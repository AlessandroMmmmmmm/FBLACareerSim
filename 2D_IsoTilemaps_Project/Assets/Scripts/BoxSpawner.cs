using UnityEngine;

public class BoxSpawner : MonoBehaviour
{
    [Header("Box Variety")]
    // This list will hold Box 1, Box 2, Box 3, and Box 4
    public GameObject[] boxPrefabs;

    [Header("Spawn Settings")]
    public int totalBoxesToSpawn = 10;
    public Vector3 spawnArea = new Vector3(45, 3, 45); // Adjust based on floor size

    void Start()
    {
        // Safety check to make sure you assigned boxes in the Inspector
        if (boxPrefabs.Length == 0)
        {
            Debug.LogError("Please drag your Box prefabs into the Box Prefabs array!");
            return;
        }

        for (int i = 0; i < totalBoxesToSpawn; i++)
        {
            SpawnRandomBox();
        }
    }

    void SpawnRandomBox()
    {
        // 1. Calculate random position (using Y = 3)
        float randomX = Random.Range(-spawnArea.x, spawnArea.x);
        float randomZ = Random.Range(-spawnArea.z, spawnArea.z);
        Vector3 spawnPos = new Vector3(randomX, 3.0f, randomZ);

        // 2. Pick a random box from the array
        int randomIndex = Random.Range(0, boxPrefabs.Length);
        GameObject selectedBox = boxPrefabs[randomIndex];

        // 3. SPAWN the box
        GameObject newBox = Instantiate(selectedBox, spawnPos, Quaternion.identity);

        // 4. THE FIX: Force the Rigidbody to sync to Y=3 immediately
        // This stops it from "snapping" back to 0
        if (newBox.TryGetComponent(out Rigidbody rb))
        {
            rb.position = spawnPos;
            rb.linearVelocity = Vector3.zero; // Ensures it doesn't start with falling speed
        }
    }

}
