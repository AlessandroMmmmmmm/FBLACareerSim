using UnityEngine;
using UnityEngine.Tilemaps;

// Teleporter
public class TeleporterObstacle : Obstacle
{
    public TeleporterObstacle linkedTeleporter;
    public Tilemap tilemap;
    private bool isTeleporting = false;
    
    public override void OnPlayerEnter(PlayerController player)
    {
        if (linkedTeleporter != null && !isTeleporting)
        {
            isTeleporting = true;
            linkedTeleporter.isTeleporting = true;
            
            Vector3Int targetPos = tilemap.WorldToCell(linkedTeleporter.transform.position);
            player.TeleportTo(targetPos);
            
            Invoke("ResetTeleport", 0.5f);
        }
    }
    
    void ResetTeleport()
    {
        isTeleporting = false;
        if (linkedTeleporter != null)
        {
            linkedTeleporter.isTeleporting = false;
        }
    }
    
    public override void OnPlayerExit(PlayerController player)
    {
        // Not used
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTeleporting)
        {
            OnPlayerEnter(other.GetComponent<PlayerController>());
        }
    }
}
