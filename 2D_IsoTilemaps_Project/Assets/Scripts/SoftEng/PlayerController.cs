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
    void Start()
    {
    characterRenderer = GetComponentInChildren<IsometricCharacterRenderer>();

        gridPosition = tilemap.WorldToCell(transform.position);
        gridPosition.z = tilemap.origin.z;
        transform.position = tilemap.GetCellCenterWorld(gridPosition);

        UpdateAnimationDirection(Vector2.zero);
    }

    public bool IsMoving()
    {
        return isMoving;
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

    UpdateAnimationDirection(DirectionFromIndex());
    StartCoroutine(MoveToCell(targetCell));
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

private System.Collections.IEnumerator MoveToCell(Vector3Int targetCell)
{
    isMoving = true;

    Vector3 start = transform.position;
    Vector3 end = tilemap.GetCellCenterWorld(targetCell);

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

    // ✅ NOW switch to static facing direction
    UpdateAnimationDirection(Vector2.zero);
}
}