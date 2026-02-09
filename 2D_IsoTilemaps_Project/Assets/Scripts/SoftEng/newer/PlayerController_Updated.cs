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

    // Animation system
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Animation parameter hashes (more efficient than strings)
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int IsMovingParam = Animator.StringToHash("IsMoving");

    // Reference to level manager
    private LevelManager levelManager;

    void Start()
    {
        // Get components
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        levelManager = FindFirstObjectByType<LevelManager>();

        if (animator == null)
        {
            Debug.LogError("PlayerController: No Animator component found!");
        }

        if (spriteRenderer == null)
        {
            Debug.LogError("PlayerController: No SpriteRenderer component found!");
        }

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

        // Set initial idle animation facing NE
        UpdateAnimator(Vector2.zero);

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

        // Update animation to idle in current facing direction
        UpdateAnimator(Vector2.zero);
    }

    public void TeleportTo(Vector3Int targetCell)
    {
        gridPosition = targetCell;

        // Apply offset consistently
        Vector3 worldPos = tilemap.GetCellCenterWorld(targetCell);
        worldPos.y += yOffset;
        transform.position = worldPos;

        UpdateAnimator(Vector2.zero);
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
        // Update to idle animation in new direction
        UpdateAnimator(Vector2.zero);
    }

    public void TurnRight()
    {
        facingIndex = (facingIndex + 7) % 8; // wrap backwards
        // Update to idle animation in new direction
        UpdateAnimator(Vector2.zero);
    }

    /// <summary>
    /// Update animator parameters based on movement state
    /// </summary>
    void UpdateAnimator(Vector2 moveDirection)
    {
        if (animator == null) return;

        // Get the direction to face (either current movement or last facing direction)
        Vector2 animDirection = moveDirection.magnitude > 0.01f ? moveDirection : DirectionFromIndex();

        // Set animator parameters
        animator.SetFloat(MoveX, animDirection.x);
        animator.SetFloat(MoveY, animDirection.y);
        animator.SetBool(IsMovingParam, moveDirection.magnitude > 0.01f);

        // Handle sprite flipping for left-facing directions
        FlipSprite(animDirection);

        Debug.Log($"Animator updated - MoveX: {animDirection.x}, MoveY: {animDirection.y}, IsMoving: {moveDirection.magnitude > 0.01f}");
    }

    /// <summary>
    /// Flip sprite for right-facing directions
    /// Since sprites are LEFT-facing by default, we flip them for right
    /// </summary>
    private void FlipSprite(Vector2 direction)
    {
        if (spriteRenderer == null) return;

        // Only flip based on horizontal direction
        if (direction.x > 0.01f)
        {
            // Moving right - flip sprite (since default is left)
            spriteRenderer.flipX = true;

            // Also flip the animator's X parameter so blend tree uses left-side animations
            animator.SetFloat(MoveX, -direction.x);
        }
        else if (direction.x < -0.01f)
        {
            // Moving left - normal orientation (default sprite faces left)
            spriteRenderer.flipX = false;
        }
        // If only moving vertically, maintain current flip state
    }

    Vector2Int GridDirectionFromIndex()
    {
        switch (facingIndex)
        {
            case 0: return new Vector2Int(1, 1);   // N
            case 1: return new Vector2Int(0, 1);   // NW
            case 2: return new Vector2Int(-1, 1);  // W
            case 3: return new Vector2Int(-1, 0);  // SW
            case 4: return new Vector2Int(-1, -1); // S
            case 5: return new Vector2Int(0, -1);  // SE
            case 6: return new Vector2Int(1, -1);  // E
            case 7: return new Vector2Int(1, 0);   // NE
        }
        return Vector2Int.zero;
    }

    Vector2 DirectionFromIndex()
    {
        switch (facingIndex)
        {
            case 0: return Vector2.up;                  // N
            case 1: return new Vector2(-1, 1).normalized;  // NW
            case 2: return Vector2.left;                // W
            case 3: return new Vector2(-1, -1).normalized; // SW
            case 4: return Vector2.down;                // S
            case 5: return new Vector2(1, -1).normalized;  // SE
            case 6: return Vector2.right;               // E
            case 7: return new Vector2(1, 1).normalized;   // NE
        }
        return Vector2.zero;
    }

    private System.Collections.IEnumerator MoveToCell(Vector3Int targetCell)
    {
        isMoving = true;

        // Set walking animation direction at the START of movement
        Vector2 movementDirection = DirectionFromIndex();
        UpdateAnimator(movementDirection);
        Debug.Log("START MOVEMENT - Setting walking animation");

        Vector3 start = transform.position;

        // Apply offset consistently
        Vector3 end = tilemap.GetCellCenterWorld(targetCell);
        end.y += yOffset;

        // Calculate the duration based on moveSpeed (moveSpeed of 4 = 0.25 seconds per tile)
        float duration = 1f / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        transform.position = end;
        gridPosition = targetCell;

        Debug.Log("MOVEMENT COMPLETE - Setting to idle");

        // Set to idle animation (maintains facing direction)
        UpdateAnimator(Vector2.zero);

        // Set isMoving to false AFTER setting idle animation
        isMoving = false;

        if (levelManager != null)
        {
            levelManager.CheckGoalReached();
        }
    }
}
