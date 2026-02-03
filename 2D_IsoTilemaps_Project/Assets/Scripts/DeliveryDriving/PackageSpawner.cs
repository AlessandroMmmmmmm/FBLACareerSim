using UnityEngine;

public class PackageSpawner : MonoBehaviour
{
    public GameObject boxPrefab; // Drag your box model/prefab here in the Inspector
    public Transform spawnPoint; // An empty GameObject where boxes should appear
    public int totalPackagesToSpawn = 5;
    public float spawnDelay = 0.5f;

    void Start()
    {
        // Spawns boxes automatically when the career world starts
        InvokeRepeating("SpawnPackage", 1f, spawnDelay);
    }

    void SpawnPackage()
    {
        if (totalPackagesToSpawn > 0)
        {
            // Spawns a box at the designated point with a slight offset so they don't overlap
            Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 2, Random.Range(-0.5f, 0.5f));
            Instantiate(boxPrefab, spawnPoint.position + offset, Quaternion.identity);

            totalPackagesToSpawn--;
        }
        else
        {
            CancelInvoke("SpawnPackage");
        }
    }
}
