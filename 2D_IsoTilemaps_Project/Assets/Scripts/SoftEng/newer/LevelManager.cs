using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public Tilemap tilemap;
    public PlayerController player;
    public ProgramManager programManager;
    
    [Header("Tiles")]
    public TileBase floorTile;
    public TileBase wallTile;
    public TileBase goalTile;
    public TileBase startTile;
    
    [Header("Obstacle Prefabs")]
    public GameObject bugPrefab;
    public GameObject serverPrefab;
    public GameObject oneWayGatePrefab;
    public GameObject teleporterPrefab;
    
    [Header("Levels")]
    public LevelData[] levels;
    private int currentLevelIndex = 0;
    
    private LevelData currentLevel;
    private List<GameObject> spawnedObstacles = new List<GameObject>();
    private Vector2Int goalPosition;
    private bool levelComplete = false;
    
    void Start()
    {
        LoadLevel(currentLevelIndex);
    }
    
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex >= levels.Length) return;
        
        currentLevelIndex = levelIndex;
        currentLevel = levels[levelIndex];
        levelComplete = false;
        
        ClearLevel();
        GenerateGrid();
        SpawnObstacles();
        SetPlayerStart();
        
        Debug.Log($"Loaded Level {currentLevel.levelNumber}: {currentLevel.levelName}");
    }
    
    void ClearLevel()
    {
        tilemap.ClearAllTiles();
        
        foreach (GameObject obj in spawnedObstacles)
        {
            Destroy(obj);
        }
        spawnedObstacles.Clear();
    }
    
    void GenerateGrid()
    {
        // Create floor tiles
        for (int x = 0; x < currentLevel.gridSize.x; x++)
        {
            for (int y = 0; y < currentLevel.gridSize.y; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                tilemap.SetTile(pos, floorTile);
            }
        }
        
        // Set start tile
        Vector3Int startPos = new Vector3Int(currentLevel.startPosition.x, currentLevel.startPosition.y, 0);
        tilemap.SetTile(startPos, startTile);
        
        // Set goal tile
        goalPosition = currentLevel.goalPosition;
        Vector3Int goalPos = new Vector3Int(goalPosition.x, goalPosition.y, 0);
        tilemap.SetTile(goalPos, goalTile);
    }
    
    void SpawnObstacles()
    {
        foreach (ObstacleData obstacle in currentLevel.obstacles)
        {
            switch (obstacle.type)
            {
                case ObstacleType.Wall:
                    SpawnWalls(obstacle.positions);
                    break;
                    
                case ObstacleType.Bug:
                    SpawnBugs(obstacle.positions, obstacle.patrolPath, obstacle.moveSpeed);
                    break;
                    
                case ObstacleType.Server:
                    SpawnServers(obstacle.positions);
                    break;
                    
                case ObstacleType.OneWayGate:
                    SpawnOneWayGates(obstacle.positions);
                    break;
                    
                case ObstacleType.Teleporter:
                    SpawnTeleporters(obstacle.positions);
                    break;
            }
        }
    }
    
    void SpawnWalls(Vector2Int[] positions)
    {
        foreach (Vector2Int pos in positions)
        {
            Vector3Int tilePos = new Vector3Int(pos.x, pos.y, 0);
            tilemap.SetTile(tilePos, wallTile);
        }
    }
    
    void SpawnBugs(Vector2Int[] positions, Vector2Int[] patrolPath, float moveSpeed)
    {
        foreach (Vector2Int pos in positions)
        {
            Vector3 worldPos = tilemap.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));
            GameObject bug = Instantiate(bugPrefab, worldPos, Quaternion.identity);
            
            if (patrolPath != null && patrolPath.Length > 0)
            {
                BugObstacle bugScript = bug.GetComponent<BugObstacle>();
                if (bugScript != null)
                {
                    bugScript.SetPatrolPath(patrolPath, tilemap, moveSpeed);
                }
            }
            
            spawnedObstacles.Add(bug);
        }
    }
    
    void SpawnServers(Vector2Int[] positions)
    {
        foreach (Vector2Int pos in positions)
        {
            Vector3 worldPos = tilemap.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));
            GameObject server = Instantiate(serverPrefab, worldPos, Quaternion.identity);
            spawnedObstacles.Add(server);
        }
    }
    
    void SpawnOneWayGates(Vector2Int[] positions)
    {
        foreach (Vector2Int pos in positions)
        {
            Vector3 worldPos = tilemap.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));
            GameObject gate = Instantiate(oneWayGatePrefab, worldPos, Quaternion.identity);
            spawnedObstacles.Add(gate);
        }
    }
    
    void SpawnTeleporters(Vector2Int[] positions)
    {
        // Teleporters work in pairs
        for (int i = 0; i < positions.Length - 1; i += 2)
        {
            Vector3 worldPos1 = tilemap.GetCellCenterWorld(new Vector3Int(positions[i].x, positions[i].y, 0));
            Vector3 worldPos2 = tilemap.GetCellCenterWorld(new Vector3Int(positions[i + 1].x, positions[i + 1].y, 0));
            
            GameObject teleporter1 = Instantiate(teleporterPrefab, worldPos1, Quaternion.identity);
            GameObject teleporter2 = Instantiate(teleporterPrefab, worldPos2, Quaternion.identity);
            
            // Link teleporters
            TeleporterObstacle tele1 = teleporter1.GetComponent<TeleporterObstacle>();
            TeleporterObstacle tele2 = teleporter2.GetComponent<TeleporterObstacle>();
            
            if (tele1 != null && tele2 != null)
            {
                tele1.linkedTeleporter = tele2;
                tele2.linkedTeleporter = tele1;
            }
            
            spawnedObstacles.Add(teleporter1);
            spawnedObstacles.Add(teleporter2);
        }
    }
    
    void SetPlayerStart()
    {
        Vector3Int startPos = new Vector3Int(currentLevel.startPosition.x, currentLevel.startPosition.y, 0);
        player.SetGridPosition(startPos);
    }
    
    public void CheckGoalReached()
    {
        Vector3Int playerPos = player.GetGridPosition();
        
        if (playerPos.x == goalPosition.x && playerPos.y == goalPosition.y && !levelComplete)
        {
            levelComplete = true;
            Debug.Log("Level Complete!");
            
            // Load next level or show victory
            if (currentLevelIndex < levels.Length - 1)
            {
                Invoke("LoadNextLevel", 2f);
            }
            else
            {
                Debug.Log("All levels complete!");
            }
        }
    }
    
    void LoadNextLevel()
    {
        LoadLevel(currentLevelIndex + 1);
        programManager.ClearProgram();
    }
    
    public void RestartLevel()
    {
        LoadLevel(currentLevelIndex);
        programManager.ClearProgram();
    }
}
