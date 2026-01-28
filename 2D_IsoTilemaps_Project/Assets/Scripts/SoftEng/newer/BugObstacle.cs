using UnityEngine;
using UnityEngine.Tilemaps;

// Moving bug that resets player position
public class BugObstacle : Obstacle
{
    private Vector2Int[] patrolPath;
    private int currentPathIndex = 0;
    private Tilemap tilemap;
    private float moveSpeed = 1f;
    private float moveTimer = 0f;
    private bool isMoving = false;
    
    public void SetPatrolPath(Vector2Int[] path, Tilemap map, float speed)
    {
        patrolPath = path;
        tilemap = map;
        moveSpeed = speed;
        
        if (path.Length > 0)
        {
            Vector3Int startPos = new Vector3Int(path[0].x, path[0].y, 0);
            transform.position = tilemap.GetCellCenterWorld(startPos);
        }
    }
    
    void Update()
    {
        if (patrolPath == null || patrolPath.Length <= 1) return;
        
        moveTimer += Time.deltaTime;
        
        if (moveTimer >= moveSpeed && !isMoving)
        {
            moveTimer = 0f;
            MoveToNextPoint();
        }
    }
    
    void MoveToNextPoint()
    {
        currentPathIndex = (currentPathIndex + 1) % patrolPath.Length;
        Vector3Int targetPos = new Vector3Int(patrolPath[currentPathIndex].x, patrolPath[currentPathIndex].y, 0);
        
        StartCoroutine(SmoothMove(tilemap.GetCellCenterWorld(targetPos)));
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
        FindFirstObjectByType<LevelManager>().RestartLevel();
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
