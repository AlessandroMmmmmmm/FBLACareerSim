using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates random road networks connecting fixed delivery points
/// Uses a simple pathfinding approach to create varied but drivable routes
/// </summary>
public class RoadGenerator : MonoBehaviour
{
    [Header("Road Prefabs")]
    [Tooltip("Straight roads - will be randomly selected")]
    public GameObject[] straightRoadPrefabs = new GameObject[2];
    [Tooltip("Corner/turn roads - will be randomly selected")]
    public GameObject[] cornerRoadPrefabs = new GameObject[2];
    [Tooltip("4-way intersections - will be randomly selected")]
    public GameObject[] intersectionRoadPrefabs = new GameObject[2];
    [Tooltip("T-junction: straight road with open side")]
    public GameObject tJunctionRoadPrefab;
    [Tooltip("Dead end: road that terminates (no connection on one side)")]
    public GameObject deadEndRoadPrefab;

    [Header("Environment Prefabs")]
    [Tooltip("Grass tile (20x20) - fills empty spaces")]
    public GameObject grassTilePrefab;
    [Tooltip("Buildings (10x10) - placed along roads, facing street")]
    public GameObject[] buildingPrefabs = new GameObject[7];

    [Header("Generation Settings")]
    public float gridSize = 10f; // Size of each road tile
    public int gridWidth = 20; // Grid dimensions
    public int gridHeight = 20;
    public float maxZPosition = 50f; // Don't generate anything past this Z value

    [Header("Warehouse Connection")]
    public bool connectToWarehouse = true;
    public Vector3 warehouseEntrance1 = new Vector3(5f, 0f, 50f);
    public Vector3 warehouseEntrance2 = new Vector3(-10f, 0f, 50f);

    [Header("Fixed Points")]
    public Transform loadingZone; // Starting point
    public Transform[] dropOffPoints; // Your 3 delivery locations (fixed positions)

    [Header("Road Style")]
    public bool allowIntersections = true;
    public int maxIntersections = 3; // Limit number of 4-way intersections
    public float branchingChance = 0.3f; // Chance to create side roads
    public int extraRoadSegments = 10; // Additional roads beyond main paths

    [Header("Building Placement")]
    public bool generateBuildings = true;
    public float buildingPlacementChance = 0.7f; // Chance to place building next to road
    public float minDistanceFromIntersection = 1; // Grid cells away from intersections

    [Header("Debug")]
    public bool showDebugGizmos = true;

    private Grid2D roadGrid;
    private Transform roadParent;
    private int intersectionCount = 0; // Track how many intersections have been placed

    [System.Serializable]
    private class Grid2D
    {
        public int width;
        public int height;
        public bool[,] occupied; // Is there a road here?
        public Vector2Int[,] roadDirection; // Which way does the road go?
        public bool[,] hasBuilding; // Is there a building here?

        public Grid2D(int w, int h)
        {
            width = w;
            height = h;
            occupied = new bool[w, h];
            roadDirection = new Vector2Int[w, h];
            hasBuilding = new bool[w, h];
        }

        public bool IsValid(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }
    }

    private void Start()
    {
        // Optional: Generate on start
        // GenerateRoadNetwork();
    }

    /// <summary>
    /// Main generation function - call this to create your road network
    /// </summary>
    public void GenerateRoadNetwork()
    {
        // Clear existing roads
        ClearOldRoads();

        // Initialize grid
        roadGrid = new Grid2D(gridWidth, gridHeight);
        intersectionCount = 0; // Reset intersection counter

        // Validate setup
        if (!ValidateSetup())
        {
            Debug.LogError("Road generation failed: Invalid setup!");
            return;
        }

        // Generate main paths from loading zone to each drop-off
        GenerateMainPaths();

        // Generate warehouse connection roads
        if (connectToWarehouse)
        {
            GenerateWarehouseRoads();
        }

        // Add extra connecting roads and branches
        GenerateExtraRoads();

        // Actually place the road prefabs
        PlaceRoadMeshes();

        // Place buildings along roads
        if (generateBuildings && buildingPrefabs != null && buildingPrefabs.Length > 0)
        {
            PlaceBuildings();
        }

        // Fill empty spaces with grass
        if (grassTilePrefab != null)
        {
            PlaceGrassTiles();
        }

        Debug.Log("Road network generated successfully!");
    }

    private bool ValidateSetup()
    {
        if (loadingZone == null)
        {
            Debug.LogError("Loading Zone not assigned!");
            return false;
        }

        if (dropOffPoints == null || dropOffPoints.Length != 3)
        {
            Debug.LogError("Must assign exactly 3 drop-off points!");
            return false;
        }

        if (straightRoadPrefabs == null || straightRoadPrefabs.Length == 0 || straightRoadPrefabs[0] == null)
        {
            Debug.LogError("At least one straight road prefab must be assigned!");
            return false;
        }

        return true;
    }

    private void ClearOldRoads()
    {
        // Find or create road parent
        GameObject roadParentObj = GameObject.Find("Roads_Generated");
        if (roadParentObj != null)
        {
            DestroyImmediate(roadParentObj);
        }

        roadParentObj = new GameObject("Roads_Generated");
        roadParent = roadParentObj.transform;
    }

    private void GenerateMainPaths()
    {
        Vector2Int startGrid = WorldToGrid(loadingZone.position);

        // Create path to each drop-off point
        foreach (Transform dropOff in dropOffPoints)
        {
            Vector2Int endGrid = WorldToGrid(dropOff.position);
            List<Vector2Int> path = GeneratePath(startGrid, endGrid);

            // Mark these grid cells as occupied
            foreach (Vector2Int cell in path)
            {
                if (roadGrid.IsValid(cell.x, cell.y))
                {
                    roadGrid.occupied[cell.x, cell.y] = true;
                }
            }
        }
    }

    private List<Vector2Int> GeneratePath(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = start;

        // Simple pathfinding - move toward target with some randomness
        while (current != end)
        {
            path.Add(current);

            Vector2Int direction = new Vector2Int(
                end.x > current.x ? 1 : (end.x < current.x ? -1 : 0),
                end.y > current.y ? 1 : (end.y < current.y ? -1 : 0)
            );

            // Add some randomness to path (30% chance to take a detour)
            if (Random.value < 0.3f)
            {
                // Randomly choose to move in x or y direction
                if (Random.value < 0.5f && direction.x != 0)
                {
                    direction.y = 0;
                }
                else if (direction.y != 0)
                {
                    direction.x = 0;
                }
            }

            // Move in preferred direction
            if (direction.x != 0 && direction.y != 0)
            {
                // Diagonal - pick one
                if (Random.value < 0.5f)
                {
                    direction.y = 0;
                }
                else
                {
                    direction.x = 0;
                }
            }

            Vector2Int next = current + direction;

            // Make sure we don't go out of bounds
            if (roadGrid.IsValid(next.x, next.y))
            {
                current = next;
            }
            else
            {
                break; // Hit edge, stop
            }

            // Safety: prevent infinite loops
            if (path.Count > gridWidth * gridHeight)
            {
                Debug.LogWarning("Path generation took too long, stopping");
                break;
            }
        }

        path.Add(end);
        return path;
    }

    private void GenerateWarehouseRoads()
    {
        // Generate straight roads from warehouse entrances toward the city
        Vector2Int entrance1Grid = WorldToGrid(warehouseEntrance1);
        Vector2Int entrance2Grid = WorldToGrid(warehouseEntrance2);

        // Create straight roads going south (negative Z) from each entrance
        // Continue until we hit an existing road or reach a reasonable distance
        int maxRoadLength = 10; // Maximum cells to extend

        // Entrance 1 road
        for (int i = 0; i < maxRoadLength; i++)
        {
            Vector2Int currentCell = new Vector2Int(entrance1Grid.x, entrance1Grid.y - i);

            if (!roadGrid.IsValid(currentCell.x, currentCell.y)) break;

            // Stop if we hit an existing road
            if (roadGrid.occupied[currentCell.x, currentCell.y] && i > 0) break;

            roadGrid.occupied[currentCell.x, currentCell.y] = true;
        }

        // Entrance 2 road
        for (int i = 0; i < maxRoadLength; i++)
        {
            Vector2Int currentCell = new Vector2Int(entrance2Grid.x, entrance2Grid.y - i);

            if (!roadGrid.IsValid(currentCell.x, currentCell.y)) break;

            // Stop if we hit an existing road
            if (roadGrid.occupied[currentCell.x, currentCell.y] && i > 0) break;

            roadGrid.occupied[currentCell.x, currentCell.y] = true;
        }
    }

    private void GenerateExtraRoads()
    {
        int added = 0;
        int attempts = 0;
        int maxAttempts = extraRoadSegments * 5;

        while (added < extraRoadSegments && attempts < maxAttempts)
        {
            attempts++;

            // Pick a random occupied cell
            int x = Random.Range(0, gridWidth);
            int y = Random.Range(0, gridHeight);

            if (!roadGrid.occupied[x, y]) continue;

            // Try to extend in a random direction
            Vector2Int[] directions = {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
            };

            Vector2Int dir = directions[Random.Range(0, directions.Length)];
            Vector2Int newCell = new Vector2Int(x, y) + dir;

            // Check if valid and not occupied
            if (roadGrid.IsValid(newCell.x, newCell.y) && !roadGrid.occupied[newCell.x, newCell.y])
            {
                // Check if this would connect to another road (avoid isolated segments)
                int adjacentRoads = CountAdjacentRoads(newCell.x, newCell.y);

                if (adjacentRoads >= 1 || Random.value < branchingChance)
                {
                    roadGrid.occupied[newCell.x, newCell.y] = true;
                    added++;
                }
            }
        }
    }

    private int CountAdjacentRoads(int x, int y)
    {
        int count = 0;
        Vector2Int[] directions = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        foreach (Vector2Int dir in directions)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;

            if (roadGrid.IsValid(nx, ny) && roadGrid.occupied[nx, ny])
            {
                count++;
            }
        }

        return count;
    }

    private void PlaceRoadMeshes()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (roadGrid.occupied[x, y])
                {
                    Vector3 worldPos = GridToWorld(x, y);

                    // Don't place roads past maxZPosition
                    if (worldPos.z > maxZPosition) continue;

                    GameObject roadPrefab = DetermineRoadType(x, y);
                    Quaternion rotation = DetermineRoadRotation(x, y);

                    if (roadPrefab != null)
                    {
                        GameObject road = Instantiate(roadPrefab, worldPos, rotation, roadParent);
                        road.name = $"Road_{x}_{y}";
                    }
                }
            }
        }
    }

    private GameObject DetermineRoadType(int x, int y)
    {
        int connections = CountAdjacentRoads(x, y);

        bool hasUp = roadGrid.IsValid(x, y + 1) && roadGrid.occupied[x, y + 1];
        bool hasDown = roadGrid.IsValid(x, y - 1) && roadGrid.occupied[x, y - 1];
        bool hasLeft = roadGrid.IsValid(x - 1, y) && roadGrid.occupied[x - 1, y];
        bool hasRight = roadGrid.IsValid(x + 1, y) && roadGrid.occupied[x + 1, y];

        // 4-way intersection
        if (connections >= 4 && allowIntersections && intersectionCount < maxIntersections)
        {
            GameObject prefab = GetRandomPrefab(intersectionRoadPrefabs);
            if (prefab != null)
            {
                intersectionCount++;
                return prefab;
            }
        }

        // T-junction (3 connections)
        if (connections == 3)
        {
            if (tJunctionRoadPrefab != null)
            {
                return tJunctionRoadPrefab;
            }
            // Fallback to intersection if available
            GameObject prefab = GetRandomPrefab(intersectionRoadPrefabs);
            if (prefab != null)
            {
                return prefab;
            }
        }

        // 2 connections - could be straight or corner
        if (connections == 2)
        {
            bool isCorner = (hasUp && hasRight) || (hasUp && hasLeft) ||
                           (hasDown && hasRight) || (hasDown && hasLeft);

            if (isCorner)
            {
                GameObject prefab = GetRandomPrefab(cornerRoadPrefabs);
                if (prefab != null)
                {
                    return prefab;
                }
            }
        }

        // 1 connection - dead end
        if (connections == 1)
        {
            if (deadEndRoadPrefab != null)
            {
                return deadEndRoadPrefab;
            }
        }

        // Default to straight road (for 2 connections that aren't corners, 0 connections, etc.)
        return GetRandomPrefab(straightRoadPrefabs);
    }

    private GameObject GetRandomPrefab(GameObject[] prefabs)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            return null;
        }

        // Filter out null entries
        List<GameObject> validPrefabs = new List<GameObject>();
        foreach (GameObject prefab in prefabs)
        {
            if (prefab != null)
            {
                validPrefabs.Add(prefab);
            }
        }

        if (validPrefabs.Count == 0)
        {
            return null;
        }

        // Return random valid prefab
        return validPrefabs[Random.Range(0, validPrefabs.Count)];
    }

    private Quaternion DetermineRoadRotation(int x, int y)
    {
        bool hasUp = roadGrid.IsValid(x, y + 1) && roadGrid.occupied[x, y + 1];
        bool hasDown = roadGrid.IsValid(x, y - 1) && roadGrid.occupied[x, y - 1];
        bool hasLeft = roadGrid.IsValid(x - 1, y) && roadGrid.occupied[x - 1, y];
        bool hasRight = roadGrid.IsValid(x + 1, y) && roadGrid.occupied[x + 1, y];

        int connections = (hasUp ? 1 : 0) + (hasDown ? 1 : 0) +
                         (hasLeft ? 1 : 0) + (hasRight ? 1 : 0);

        if (connections >= 4)
        {
            // 4-way intersection - default rotation
            return Quaternion.identity;
        }
        else if (connections == 3)
        {
            // T-junction - rotate so the open side faces the correct direction
            // (+90 degree offset for SimplePoly assets)
            if (!hasUp) // Open on top, road connects from bottom, left, right
            {
                return Quaternion.Euler(0, 270, 0);
            }
            else if (!hasRight) // Open on right, road connects from left, up, down
            {
                return Quaternion.Euler(0, 0, 0);
            }
            else if (!hasDown) // Open on bottom, road connects from top, left, right
            {
                return Quaternion.Euler(0, 90, 0);
            }
            else // !hasLeft - Open on left, road connects from right, up, down
            {
                return Quaternion.Euler(0, 180, 0);
            }
        }
        else if (connections == 2)
        {
            // Straight or corner
            if ((hasUp && hasDown) || (hasLeft && hasRight))
            {
                // Straight road
                if (hasUp && hasDown)
                {
                    return Quaternion.Euler(0, 0, 0); // Vertical
                }
                else
                {
                    return Quaternion.Euler(0, 90, 0); // Horizontal
                }
            }
            else
            {
                // Corner (with +90 degree offset for SimplePoly assets)
                if (hasUp && hasRight)
                {
                    return Quaternion.Euler(0, 90, 0);
                }
                else if (hasRight && hasDown)
                {
                    return Quaternion.Euler(0, 180, 0);
                }
                else if (hasDown && hasLeft)
                {
                    return Quaternion.Euler(0, 270, 0);
                }
                else // hasLeft && hasUp
                {
                    return Quaternion.Euler(0, 0, 0);
                }
            }
        }
        else if (connections == 1)
        {
            // Dead end - road opens toward the connected direction (+180 degree rotation)
            if (hasUp) return Quaternion.Euler(0, 180, 0);   // Opens up
            if (hasRight) return Quaternion.Euler(0, 270, 0); // Opens right
            if (hasDown) return Quaternion.Euler(0, 0, 0);    // Opens down
            if (hasLeft) return Quaternion.Euler(0, 90, 0);   // Opens left
        }

        // No connections or default
        return Quaternion.identity;
    }

    private Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / gridSize) + gridWidth / 2;
        int y = Mathf.RoundToInt(worldPos.z / gridSize) + gridHeight / 2;
        return new Vector2Int(Mathf.Clamp(x, 0, gridWidth - 1), Mathf.Clamp(y, 0, gridHeight - 1));
    }

    private void PlaceBuildings()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Skip if there's already a road here
                if (roadGrid.occupied[x, y]) continue;

                // Check if adjacent to a road
                bool adjacentToRoad = false;
                Vector2Int roadDirection = Vector2Int.zero;

                // Check all four directions
                if (roadGrid.IsValid(x, y + 1) && roadGrid.occupied[x, y + 1])
                {
                    adjacentToRoad = true;
                    roadDirection = Vector2Int.down; // Building faces south (toward road on north)
                }
                else if (roadGrid.IsValid(x, y - 1) && roadGrid.occupied[x, y - 1])
                {
                    adjacentToRoad = true;
                    roadDirection = Vector2Int.up; // Building faces north (toward road on south)
                }
                else if (roadGrid.IsValid(x + 1, y) && roadGrid.occupied[x + 1, y])
                {
                    adjacentToRoad = true;
                    roadDirection = Vector2Int.left; // Building faces west (toward road on east)
                }
                else if (roadGrid.IsValid(x - 1, y) && roadGrid.occupied[x - 1, y])
                {
                    adjacentToRoad = true;
                    roadDirection = Vector2Int.right; // Building faces east (toward road on west)
                }

                if (!adjacentToRoad) continue;

                // Check if too close to intersection
                if (IsTooCloseToIntersection(x, y)) continue;

                // Random chance to place building
                if (Random.value > buildingPlacementChance) continue;

                // Get world position and check Z limit
                Vector3 worldPos = GridToWorld(x, y);
                if (worldPos.z > maxZPosition) continue;

                // Place the building
                GameObject buildingPrefab = GetRandomPrefab(buildingPrefabs);

                if (buildingPrefab != null)
                {
                    // Calculate rotation based on which direction the road is
                    Quaternion rotation = GetBuildingRotation(roadDirection);

                    GameObject building = Instantiate(buildingPrefab, worldPos, rotation, roadParent);
                    building.name = $"Building_{x}_{y}";

                    // Mark this cell as having a building
                    roadGrid.hasBuilding[x, y] = true;
                }
            }
        }
    }

    private bool IsTooCloseToIntersection(int x, int y)
    {
        int checkRadius = Mathf.RoundToInt(minDistanceFromIntersection);

        for (int dx = -checkRadius; dx <= checkRadius; dx++)
        {
            for (int dy = -checkRadius; dy <= checkRadius; dy++)
            {
                int checkX = x + dx;
                int checkY = y + dy;

                if (!roadGrid.IsValid(checkX, checkY)) continue;
                if (!roadGrid.occupied[checkX, checkY]) continue;

                // Check if this road cell is an intersection (3+ connections)
                int connections = CountAdjacentRoads(checkX, checkY);
                if (connections >= 3)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private Quaternion GetBuildingRotation(Vector2Int roadDirection)
    {
        // Buildings face +Z by default, rotate to face the road
        if (roadDirection == Vector2Int.up) // Road to the north, building faces north
        {
            return Quaternion.Euler(0, 180, 0);
        }
        else if (roadDirection == Vector2Int.right) // Road to the east, building faces east
        {
            return Quaternion.Euler(0, 270, 0);
        }
        else if (roadDirection == Vector2Int.down) // Road to the south, building faces south
        {
            return Quaternion.Euler(0, 0, 0);
        }
        else // Road to the west, building faces west
        {
            return Quaternion.Euler(0, 90, 0);
        }
    }

    private void PlaceGrassTiles()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Only place grass where there's no road and no building
                if (!roadGrid.occupied[x, y] && !roadGrid.hasBuilding[x, y])
                {
                    Vector3 worldPos = GridToWorld(x, y);

                    // Don't place grass past maxZPosition
                    if (worldPos.z > maxZPosition) continue;

                    GameObject grass = Instantiate(grassTilePrefab, worldPos, Quaternion.identity, roadParent);
                    grass.name = $"Grass_{x}_{y}";
                }
            }
        }
    }

    private Vector3 GridToWorld(int x, int y)
    {
        float worldX = (x - gridWidth / 2) * gridSize;
        float worldZ = (y - gridHeight / 2) * gridSize;
        return new Vector3(worldX, 0.509f, worldZ);
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Draw grid
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 pos = GridToWorld(x, y);
                Gizmos.DrawWireCube(pos, Vector3.one * gridSize * 0.9f);
            }
        }

        // Draw occupied cells
        if (roadGrid != null)
        {
            Gizmos.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (roadGrid.occupied[x, y])
                    {
                        Vector3 pos = GridToWorld(x, y);
                        Gizmos.DrawCube(pos, Vector3.one * gridSize * 0.8f);
                    }
                }
            }
        }

        // Draw fixed points
        if (loadingZone != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(loadingZone.position, 3f);
        }

        if (dropOffPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform dropOff in dropOffPoints)
            {
                if (dropOff != null)
                {
                    Gizmos.DrawWireSphere(dropOff.position, 3f);
                }
            }
        }
    }
}
