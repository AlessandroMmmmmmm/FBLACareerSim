using UnityEngine;

public class BoxSpawner : MonoBehaviour
{
    public GameObject boxPrefab; // Drag your box prefab here
    public int boxCount = 3;
    public Vector3 spawnArea = new Vector3(20, 0, 20); // The size of your floor

    void Start()
    {
        for (int i = 0; i < boxCount; i++)
        {
            SpawnBox();
        }
    }

    void SpawnBox()
    {
        // 1. Generate a random position
        float randomX = Random.Range(-spawnArea.x, spawnArea.x);
        float randomZ = Random.Range(-spawnArea.z, spawnArea.z);
        Vector3 randomPos = new Vector3(randomX, 1f, randomZ);

        // 2. Spawn the box
        Instantiate(boxPrefab, randomPos, Quaternion.identity);
    }
}
