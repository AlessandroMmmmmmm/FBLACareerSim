using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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

    [Header("Isometric Alignment")]
    [Tooltip("Y offset for obstacles (should match player's yOffset)")]
    public float obstacleYOffset = -0.25f;

    [Header("Levels")]
    public LevelData[] levels;

    [Header("Level Selection (Inspector Testing)")]
    [Tooltip("Leave at -1 for random level, or set to 0-3 to force a specific level")]
    [Range(-1, 3)]
    public int forceStartLevel = -1;

    [Tooltip("Second level to play after first (-1 = none, 0-3 = specific level)")]
    [Range(-1, 3)]
    public int forceSecondLevel = -1;

    private int currentLevelIndex = 0;
    private bool isFirstLevelComplete = false;  // Track if we've completed the first level

    private LevelData currentLevel;
    private List<GameObject> spawnedObstacles = new List<GameObject>();
    private Vector2Int goalPosition;
    private bool levelComplete = false;

    void Start()
    {
        // Reset state
        isFirstLevelComplete = false;

        // Use forced level if set, otherwise random
        if (forceStartLevel >= 0 && forceStartLevel < levels.Length)
        {
            currentLevelIndex = forceStartLevel;
            Debug.Log($"Forced start level: {currentLevelIndex}");
        }
        else
        {
            currentLevelIndex = Random.Range(0, levels.Length);
            Debug.Log($"Selected random level: {currentLevelIndex}");
        }

        if (forceSecondLevel >= 0 && forceSecondLevel < levels.Length)
        {
            Debug.Log($"Second level configured: {forceSecondLevel}");
        }

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
            // Apply Y offset to align with tiles
            worldPos.y += obstacleYOffset;
            GameObject server = Instantiate(serverPrefab, worldPos, Quaternion.identity);
            spawnedObstacles.Add(server);
        }
    }

    void SpawnOneWayGates(Vector2Int[] positions)
    {
        foreach (Vector2Int pos in positions)
        {
            Vector3 worldPos = tilemap.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));
            // Apply Y offset to align with tiles
            worldPos.y += obstacleYOffset;
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

            // Apply Y offset to align with tiles
            worldPos1.y += obstacleYOffset;
            worldPos2.y += obstacleYOffset;

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

            // Always show scoring after completing a level
            ShowEndGamePopup();
        }
    }

    void LoadNextLevelForced()
    {
        LoadLevel(forceSecondLevel);
        programManager.ClearProgram();

        // Reset scoring for next level (optional - remove if you want cumulative scoring)
        SoftwareEngScoring scoring = FindFirstObjectByType<SoftwareEngScoring>();
        if (scoring != null)
        {
            // Don't reset - keep cumulative time/attempts across both levels
            // scoring.ResetTracking();
        }
    }

    void ShowEndGamePopup()
    {
        SoftwareEngScoring scoring = FindFirstObjectByType<SoftwareEngScoring>();
        if (scoring != null)
        {
            int commandCount = programManager.GetProgramLength();
            scoring.ShowEndGameReport(commandCount);

        }
        else
        {
            Debug.LogWarning("No SoftwareEngScoring found in scene!");
        }
    }

    public void LoadSecondLevel()
    {
        Debug.Log("=== LoadSecondLevel START ===");
        Debug.Log($"forceStartLevel (Inspector) = {forceStartLevel}");
        Debug.Log($"forceSecondLevel (Inspector) = {forceSecondLevel}");
        Debug.Log($"currentLevelIndex (runtime) = {currentLevelIndex}");
        Debug.Log($"isFirstLevelComplete = {isFirstLevelComplete}");

        if (forceSecondLevel >= 0 && forceSecondLevel < levels.Length && !isFirstLevelComplete)
        {
            Debug.Log($"*** LOADING SECOND LEVEL: {forceSecondLevel} ***");

            isFirstLevelComplete = true;
            programManager.ClearProgram();
            LoadLevel(forceSecondLevel);

            SoftwareEngScoring scoring = FindFirstObjectByType<SoftwareEngScoring>();
            if (scoring != null)
            {
                scoring.ResetTracking();
            }
        }
        else
        {
            Debug.LogError($"FAILED TO LOAD SECOND LEVEL!");
            Debug.LogError($"  forceSecondLevel={forceSecondLevel}, isFirstLevelComplete={isFirstLevelComplete}");
        }
        Debug.Log("=== LoadSecondLevel END ===");
    }

    public bool ShouldLoadSecondLevel()
    {
        // Only load second level if we've completed first level AND second level is configured
        bool should = !isFirstLevelComplete && (forceSecondLevel >= 0 && forceSecondLevel < levels.Length);
        Debug.Log($"ShouldLoadSecondLevel: isFirstLevelComplete={isFirstLevelComplete}, forceSecondLevel={forceSecondLevel}, result={should}");
        return should;
    }

    void LoadNextLevel()
    {
        LoadLevel(currentLevelIndex + 1);
        programManager.ClearProgram();
    }

    public void RestartLevel()
    {
        // Stop the current program execution but keep the commands
        programManager.StopProgram();

        // Reload the same level (resets player position and obstacles)
        LoadLevel(currentLevelIndex);
    }

    public bool IsLevelComplete()
    {
        return levelComplete;
    }

    public void ResetPlayerPosition()
    {
        // Reset player to start position without clearing program
        // Also reset all bugs to their starting positions
        Vector3Int startPos = new Vector3Int(currentLevel.startPosition.x, currentLevel.startPosition.y, 0);
        player.SetGridPosition(startPos);

        // Reset bugs to their starting positions
        BugObstacle[] bugs = FindObjectsByType<BugObstacle>(FindObjectsSortMode.None);
        foreach (BugObstacle bug in bugs)
        {
            // This will reset the bug to its first patrol position
            // We need to respawn bugs to reset them properly
        }

        // Actually, just respawn all obstacles to reset bugs properly
        ClearObstacles();
        SpawnObstacles();
    }

    void ClearObstacles()
    {
        foreach (GameObject obj in spawnedObstacles)
        {
            Destroy(obj);
        }
        spawnedObstacles.Clear();
    }
}