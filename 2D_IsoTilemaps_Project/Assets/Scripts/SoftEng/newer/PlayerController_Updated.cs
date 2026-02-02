// using UnityEngine;
// using UnityEngine.Tilemaps;

// public class PlayerController : MonoBehaviour
// {
//     public Tilemap tilemap;
//     public float moveSpeed = 4f;
    
//     [Header("Tile References")]
//     public TileBase wallTile; // Assign your wall tile here
    
//     [Header("Isometric Alignment")]
//     [Tooltip("Adjust this Y value to fix vertical alignment (try -0.25 to -0.5)")]
//     public float yOffset = -0.25f;

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

//         // Convert current world position to grid cell position
//         Vector3Int cellPos = tilemap.WorldToCell(transform.position);
        
//         // Get the actual world center of that cell
//         Vector3 cellCenter = tilemap.GetCellCenterWorld(cellPos);
        
//         // Set grid position
//         gridPosition = cellPos;
//         gridPosition.z = 0;
        
//         // Apply offset and set position
//         cellCenter.y += yOffset;
//         transform.position = cellCenter;

//         // Set initial facing direction
//         if (characterRenderer != null)
//         {
//             characterRenderer.SetDirection(DirectionFromIndex());
//         }
        
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
//         gridPosition.z = 0;
        
//         // Get world center from tilemap (accounts for tilemap offset)
//         Vector3 worldPos = tilemap.GetCellCenterWorld(gridPosition);
//         worldPos.y += yOffset;
//         transform.position = worldPos;
        
//         // Reset facing direction to default (NE - matches starting direction)
//         facingIndex = 7;
        
//         // Update animation to show correct facing direction
//         if (characterRenderer != null)
//         {
//             characterRenderer.SetDirection(DirectionFromIndex());
//         }
        
//         UpdateAnimationDirection(Vector2.zero);
//     }
    
//     public void TeleportTo(Vector3Int targetCell)
//     {
//         gridPosition = targetCell;
        
//         // Apply offset consistently
//         Vector3 worldPos = tilemap.GetCellCenterWorld(targetCell);
//         worldPos.y += yOffset;
//         transform.position = worldPos;
        
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
//             return;
//         }
        
//         // Check if target is a wall
//         if (IsWall(targetCell)) return;

//         UpdateAnimationDirection(DirectionFromIndex());
//         StartCoroutine(MoveToCell(targetCell));
//     }
    
//     bool IsWall(Vector3Int position)
//     {
//         TileBase tile = tilemap.GetTile(position);
        
//         // If wallTile is assigned, compare directly
//         if (wallTile != null && tile == wallTile)
//         {
//             return true;
//         }
        
//         // Fallback: check if tile name contains "Wall"
//         if (tile != null && tile.name.Contains("Wall"))
//         {
//             return true;
//         }
        
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
//         Debug.Log("Animation Direction: " + dir);
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
        
//         // Apply offset consistently
//         Vector3 end = tilemap.GetCellCenterWorld(targetCell);
//         end.y += yOffset;

//         // Keep the walking animation direction active during movement
//         // (it was already set in MoveForward before this coroutine started)
        
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

//         // Small delay before switching to idle to ensure animation plays
//         yield return new WaitForSeconds(0.1f);

//         // NOW switch to idle/static animation (after movement is done)
//         UpdateAnimationDirection(Vector2.zero);
        
//         if (levelManager != null)
//         {
//             levelManager.CheckGoalReached();
//         }
//     }
// }

using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    public Tilemap tilemap;
    public float moveSpeed = 4f;
    
    [Header("Tile References")]
    public TileBase wallTile; // Assign your wall tile here
    
    [Header("Isometric Alignment")]
    [Tooltip("Adjust this Y value to fix vertical alignment (try -0.25 to -0.5)")]
    public float yOffset = -0.25f;

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

        // Convert current world position to grid cell position
        Vector3Int cellPos = tilemap.WorldToCell(transform.position);
        
        // Get the actual world center of that cell
        Vector3 cellCenter = tilemap.GetCellCenterWorld(cellPos);
        
        // Set grid position
        gridPosition = cellPos;
        gridPosition.z = 0;
        
        // Apply offset and set position
        cellCenter.y += yOffset;
        transform.position = cellCenter;

        // Set initial facing direction
        if (characterRenderer != null)
        {
            characterRenderer.SetDirection(DirectionFromIndex());
        }
        
        UpdateAnimationDirection(Vector2.zero);
        
        Debug.Log($"Player starting at grid position: {gridPosition}, world pos: {transform.position}");
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public Vector3Int GetGridPosition()
    {
        return gridPosition;
    }
    
    public void SetGridPosition(Vector3Int newPosition)
    {
        gridPosition = newPosition;
        gridPosition.z = 0;
        
        // Get world center from tilemap (accounts for tilemap offset)
        Vector3 worldPos = tilemap.GetCellCenterWorld(gridPosition);
        worldPos.y += yOffset;
        transform.position = worldPos;
        
        // Reset facing direction to default (NE - matches starting direction)
        facingIndex = 7;
        
        // Update animation to show correct facing direction
        if (characterRenderer != null)
        {
            characterRenderer.SetDirection(DirectionFromIndex());
        }
        
        UpdateAnimationDirection(Vector2.zero);
    }
    
    public void TeleportTo(Vector3Int targetCell)
    {
        gridPosition = targetCell;
        
        // Apply offset consistently
        Vector3 worldPos = tilemap.GetCellCenterWorld(targetCell);
        worldPos.y += yOffset;
        transform.position = worldPos;
        
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

        Debug.Log($"Moving from {gridPosition} to {targetCell}, HasTile: {tilemap.HasTile(targetCell)}");

        if (!tilemap.HasTile(targetCell))
        {
            Debug.Log($"Cannot move - no tile at {targetCell}");
            return;
        }
        
        // Check if target is a wall
        if (IsWall(targetCell)) return;

        // Don't update animation here - let the coroutine handle it
        StartCoroutine(MoveToCell(targetCell));
    }
    
    bool IsWall(Vector3Int position)
    {
        TileBase tile = tilemap.GetTile(position);
        
        // If wallTile is assigned, compare directly
        if (wallTile != null && tile == wallTile)
        {
            Debug.Log($"Wall detected at {position}");
            return true;
        }
        
        // Fallback: check if tile name contains "Wall"
        if (tile != null && tile.name.Contains("Wall"))
        {
            Debug.Log($"Wall detected at {position} by name");
            return true;
        }
        
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
        Debug.Log("Animation Direction: " + dir);
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

    private System.Collections.IEnumerator MoveToCell(Vector3Int targetCell)
    {
        isMoving = true;
        
        // Set walking animation direction at the START of movement
        UpdateAnimationDirection(DirectionFromIndex());

        Vector3 start = transform.position;
        
        // Apply offset consistently
        Vector3 end = tilemap.GetCellCenterWorld(targetCell);
        end.y += yOffset;
        
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

        // Switch to idle/static animation AFTER movement is complete
        UpdateAnimationDirection(Vector2.zero);
        
        if (levelManager != null)
        {
            levelManager.CheckGoalReached();
        }
    }
}