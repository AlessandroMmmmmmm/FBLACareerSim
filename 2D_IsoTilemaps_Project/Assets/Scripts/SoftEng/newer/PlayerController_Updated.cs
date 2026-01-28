using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    public Tilemap tilemap;
    public float moveSpeed = 4f;

    private Vector3Int gridPosition;
    private bool isMoving = false;

    // Facing direction (grid-based)
    private int facingIndex = 7; // NE to start (matches your sprite)

    public SpriteRenderer spriteRenderer;
    public IsometricCharacterRenderer characterRenderer;
    
    // Reference to level manager
    private LevelManager levelManager;
    
    void Start()
    {
        characterRenderer = GetComponentInChildren<IsometricCharacterRenderer>();
        levelManager = FindFirstObjectByType<LevelManager>();

        gridPosition = tilemap.WorldToCell(transform.position);
        gridPosition.z = tilemap.origin.z;
        transform.position = tilemap.GetCellCenterWorld(gridPosition);

        UpdateAnimationDirection(Vector2.zero);
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public Vector3Int GetGridPosition()
    {
        return gridPosition;
    }
    
    // public void SetGridPosition(Vector3Int newPosition)
    // {
    //     gridPosition = newPosition;
    //     gridPosition.z = tilemap.origin.z;
    //     transform.position = tilemap.GetCellCenterWorld(gridPosition);
    //     UpdateAnimationDirection(Vector2.zero);
    // }

    public void SetGridPosition(Vector3Int newPosition)
    {
        gridPosition = newPosition;
        gridPosition.z = tilemap.origin.z;
        Vector3 worldPos = tilemap.GetCellCenterWorld(gridPosition);
        worldPos.y += 0.5f; // Add offset here - adjust value as needed
        transform.position = worldPos;
        UpdateAnimationDirection(Vector2.zero);
    }
    
    public void TeleportTo(Vector3Int targetCell)
    {
        gridPosition = targetCell;
        transform.position = tilemap.GetCellCenterWorld(targetCell);
        UpdateAnimationDirection(Vector2.zero);
    }

    public void MoveForward()
    {
        if (isMoving) return;

        Vector2Int dir = GridDirectionFromIndex();

        Vector3Int targetCell = new Vector3Int(
            gridPosition.x + dir.x,
            gridPosition.y + dir.y,
            gridPosition.z
        );

        if (!tilemap.HasTile(targetCell)) return;
        
        // Check if target is a wall
        if (IsWall(targetCell)) return;

        UpdateAnimationDirection(DirectionFromIndex());
        StartCoroutine(MoveToCell(targetCell));
    }
    
    bool IsWall(Vector3Int position)
    {
        // Check if the tile at this position is a wall tile
        // This assumes you have a way to identify wall tiles
        // You might need to adjust this based on your tile setup
        TileBase tile = tilemap.GetTile(position);
        
        // You'll need to compare against your wall tile
        // For now, return false - implement this based on your tile setup
        return false;
    }

    public void TurnLeft()
    {
        facingIndex = (facingIndex + 1) % 8;
        UpdateAnimationDirection(DirectionFromIndex());
    }
    
    public void TurnRight()
    {
        facingIndex = (facingIndex + 7) % 8; // wrap backwards
        UpdateAnimationDirection(DirectionFromIndex());
    }

    void UpdateAnimationDirection(Vector2 dir)
    {
        Debug.Log("Ayty: " + dir);
        if (characterRenderer != null)
            characterRenderer.SetDirection(dir);
    }

    Vector2Int GridDirectionFromIndex()
    {
        switch (facingIndex)
        {
            case 0: return new Vector2Int(1, 1);  // N
            case 1: return new Vector2Int(0, 1);  // NW
            case 2: return new Vector2Int(-1, 1); // W
            case 3: return new Vector2Int(-1, 0);  // SW
            case 4: return new Vector2Int(-1, -1);  // S
            case 5: return new Vector2Int(0, -1);   // SE
            case 6: return new Vector2Int(1, -1);   // E
            case 7: return new Vector2Int(1, 0);   // NE
        }
        return Vector2Int.zero;
    }

    Vector2 DirectionFromIndex()
    {
        switch (facingIndex)
        {
            case 0: return Vector2.up;
            case 1: return new Vector2(-1, 1);
            case 2: return Vector2.left;
            case 3: return new Vector2(-1, -1);
            case 4: return Vector2.down;
            case 5: return new Vector2(1, -1);
            case 6: return Vector2.right;
            case 7: return new Vector2(1, 1);
        }
        return Vector2.zero;
    }

    // private System.Collections.IEnumerator MoveToCell(Vector3Int targetCell)
    // {
    //     isMoving = true;

    //     Vector3 start = transform.position;
    //     Vector3 end = tilemap.GetCellCenterWorld(targetCell);

    //     float t = 0;
    //     while (t < 1)
    //     {
    //         t += Time.deltaTime * moveSpeed;
    //         transform.position = Vector3.Lerp(start, end, t);
    //         yield return null;
    //     }

    //     transform.position = end;
    //     gridPosition = targetCell;
    //     isMoving = false;

    //     // Switch to static facing direction
    //     UpdateAnimationDirection(Vector2.zero);
        
    //     // Check if player reached the goal
    //     if (levelManager != null)
    //     {
    //         levelManager.CheckGoalReached();
    //     }
    // }

    private System.Collections.IEnumerator MoveToCell(Vector3Int targetCell)
    {
        isMoving = true;

        Vector3 start = transform.position;
        Vector3 end = tilemap.GetCellCenterWorld(targetCell);
        end.y += 0.5f; // Add offset here - adjust value as needed

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        transform.position = end;
        gridPosition = targetCell;
        isMoving = false;

        UpdateAnimationDirection(Vector2.zero);
        
        if (levelManager != null)
        {
            levelManager.CheckGoalReached();
        }
    }
}


// using UnityEngine;
// using UnityEngine.Tilemaps;

// public class PlayerController : MonoBehaviour
// {
//     public Tilemap tilemap;
//     public float moveSpeed = 4f;
    
//     [Header("Isometric Alignment")]
//     [Tooltip("Adjust this to fix vertical alignment issues")]
//     public Vector3 positionOffset = new Vector3(0, -0.5f, 0);

//     private Vector3Int gridPosition;
//     private bool isMoving = false;

//     // Facing direction (grid-based)
//     private int facingIndex = 7; // NE to start (matches your sprite)

//     public SpriteRenderer spriteRenderer;
//     public IsometricCharacterRenderer characterRenderer;
    
//     // Reference to level manager
//     private LevelManager levelManager;
    
//     void Start()
//     {
//         characterRenderer = GetComponentInChildren<IsometricCharacterRenderer>();
//         levelManager = FindFirstObjectByType<LevelManager>();

//         gridPosition = tilemap.WorldToCell(transform.position);
//         gridPosition.z = tilemap.origin.z;
//         transform.position = tilemap.GetCellCenterWorld(gridPosition) + positionOffset;

//         UpdateAnimationDirection(Vector2.zero);
//     }

//     public bool IsMoving()
//     {
//         return isMoving;
//     }

//     public Vector3Int GetGridPosition()
//     {
//         return gridPosition;
//     }
    
//     public void SetGridPosition(Vector3Int newPosition)
//     {
//         gridPosition = newPosition;
//         gridPosition.z = tilemap.origin.z;
//         transform.position = tilemap.GetCellCenterWorld(gridPosition) + positionOffset;
//         UpdateAnimationDirection(Vector2.zero);
//     }
    
//     public void TeleportTo(Vector3Int targetCell)
//     {
//         gridPosition = targetCell;
//         transform.position = tilemap.GetCellCenterWorld(targetCell) + positionOffset;
//         UpdateAnimationDirection(Vector2.zero);
//     }

//     public void MoveForward()
//     {
//         if (isMoving) return;

//         Vector2Int dir = GridDirectionFromIndex();

//         Vector3Int targetCell = new Vector3Int(
//             gridPosition.x + dir.x,
//             gridPosition.y + dir.y,
//             gridPosition.z
//         );

//         if (!tilemap.HasTile(targetCell)) 
//         {
//             Debug.Log($"No tile at target position: {targetCell}");
//             return;
//         }
        
//         // Check if target is a wall
//         if (IsWall(targetCell)) return;

//         UpdateAnimationDirection(DirectionFromIndex());
//         StartCoroutine(MoveToCell(targetCell));
//     }
    
//     bool IsWall(Vector3Int position)
//     {
//         // Check if the tile at this position is a wall tile
//         // This assumes you have a way to identify wall tiles
//         // You might need to adjust this based on your tile setup
//         TileBase tile = tilemap.GetTile(position);
        
//         // You'll need to compare against your wall tile
//         // For now, return false - implement this based on your tile setup
//         return false;
//     }

//     public void TurnLeft()
//     {
//         facingIndex = (facingIndex + 1) % 8;
//         UpdateAnimationDirection(DirectionFromIndex());
//     }
    
//     public void TurnRight()
//     {
//         facingIndex = (facingIndex + 7) % 8; // wrap backwards
//         UpdateAnimationDirection(DirectionFromIndex());
//     }

//     void UpdateAnimationDirection(Vector2 dir)
//     {
//         Debug.Log("Ayty: " + dir);
//         if (characterRenderer != null)
//             characterRenderer.SetDirection(dir);
//     }

//     Vector2Int GridDirectionFromIndex()
//     {
//         switch (facingIndex)
//         {
//             case 0: return new Vector2Int(1, 1);  // N
//             case 1: return new Vector2Int(0, 1);  // NW
//             case 2: return new Vector2Int(-1, 1); // W
//             case 3: return new Vector2Int(-1, 0);  // SW
//             case 4: return new Vector2Int(-1, -1);  // S
//             case 5: return new Vector2Int(0, -1);   // SE
//             case 6: return new Vector2Int(1, -1);   // E
//             case 7: return new Vector2Int(1, 0);   // NE
//         }
//         return Vector2Int.zero;
//     }

//     Vector2 DirectionFromIndex()
//     {
//         switch (facingIndex)
//         {
//             case 0: return Vector2.up;
//             case 1: return new Vector2(-1, 1);
//             case 2: return Vector2.left;
//             case 3: return new Vector2(-1, -1);
//             case 4: return Vector2.down;
//             case 5: return new Vector2(1, -1);
//             case 6: return Vector2.right;
//             case 7: return new Vector2(1, 1);
//         }
//         return Vector2.zero;
//     }

//     private System.Collections.IEnumerator MoveToCell(Vector3Int targetCell)
//     {
//         isMoving = true;

//         Vector3 start = transform.position;
//         Vector3 end = tilemap.GetCellCenterWorld(targetCell) + positionOffset;

//         float t = 0;
//         while (t < 1)
//         {
//             t += Time.deltaTime * moveSpeed;
//             transform.position = Vector3.Lerp(start, end, t);
//             yield return null;
//         }

//         transform.position = end;
//         gridPosition = targetCell;
//         isMoving = false;

//         // Switch to static facing direction
//         UpdateAnimationDirection(Vector2.zero);
        
//         // Check if player reached the goal
//         if (levelManager != null)
//         {
//             levelManager.CheckGoalReached();
//         }
//     }
// }