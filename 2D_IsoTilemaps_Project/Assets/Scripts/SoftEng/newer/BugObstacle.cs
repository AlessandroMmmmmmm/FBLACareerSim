using UnityEngine;
using UnityEngine.Tilemaps;

// Moving bug that resets player position
public class BugObstacle : Obstacle
{
    private Vector2Int[] patrolPath;
    private int currentPathIndex = 0;
    private Tilemap tilemap;
    private float moveSpeed = 1f;
    private bool isMoving = false;
    private ProgramManager programManager;

    [Header("Isometric Alignment")]
    [Tooltip("Y offset to align with tiles (should match player's yOffset)")]
    public float yOffset = -0.25f;

    void Start()
    {
        programManager = FindFirstObjectByType<ProgramManager>();
    }

    public void SetPatrolPath(Vector2Int[] path, Tilemap map, float speed)
    {
        patrolPath = path;
        tilemap = map;
        moveSpeed = speed;

        if (path.Length > 0)
        {
            Vector3Int startPos = new Vector3Int(path[0].x, path[0].y, 0);
            Vector3 worldPos = tilemap.GetCellCenterWorld(startPos);
            worldPos.y += yOffset;
            transform.position = worldPos;
        }
    }

    // Call this method from ProgramManager when player moves
    public void StepForward()
    {
        if (patrolPath == null || patrolPath.Length <= 1 || isMoving) return;

        MoveToNextPoint();
    }

    void MoveToNextPoint()
    {
        currentPathIndex = (currentPathIndex + 1) % patrolPath.Length;
        Vector3Int targetPos = new Vector3Int(patrolPath[currentPathIndex].x, patrolPath[currentPathIndex].y, 0);

        Vector3 worldPos = tilemap.GetCellCenterWorld(targetPos);
        worldPos.y += yOffset;
        StartCoroutine(SmoothMove(worldPos));
    }

    System.Collections.IEnumerator SmoothMove(Vector3 targetPos)
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * 2f;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;
    }

    public override void OnPlayerEnter(PlayerController player)
    {
        Debug.Log("Player hit a bug! Restarting level...");
        // Play collision failure sound
        if (programManager != null)
        {
            programManager.PlayCollisionFailure();
        }
        // Use Invoke to delay restart slightly, preventing collection modification errors
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.Invoke("RestartLevel", 0.1f);
        }
    }

    public override void OnPlayerExit(PlayerController player)
    {
        // Not used for bugs
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerEnter(other.GetComponent<PlayerController>());
        }
    }
}