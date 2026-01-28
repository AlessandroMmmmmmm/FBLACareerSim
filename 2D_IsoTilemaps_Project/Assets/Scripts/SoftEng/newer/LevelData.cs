using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "SoftwareEng/Level Data")]
public class LevelData : ScriptableObject
{
    public int levelNumber;
    public string levelName;
    
    [TextArea(3, 5)]
    public string description;
    
    public Vector2Int gridSize = new Vector2Int(10, 10);
    public Vector2Int startPosition;
    public Vector2Int goalPosition;
    
    // Obstacles
    public ObstacleData[] obstacles;
    
    // Optional: Limit number of moves
    public int maxMoves = -1; // -1 = unlimited
    
    // Optional: Required moves to use
    public bool restrictMoveTypes = false;
    public MoveType[] allowedMoves;
}

[System.Serializable]
public class ObstacleData
{
    public ObstacleType type;
    public Vector2Int[] positions;
    
    // For moving obstacles
    public Vector2Int[] patrolPath;
    public float moveSpeed = 1f;
}

public enum ObstacleType
{
    Wall,           // Cannot pass through
    Bug,            // Moving obstacle that resets player
    Server,         // Must interact with before reaching goal
    OneWayGate,     // Can only pass in one direction
    Teleporter      // Teleports to another location
}
