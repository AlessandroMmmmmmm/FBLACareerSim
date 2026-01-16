using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class PlayerTileSceneSwitcher : MonoBehaviour
{
    public Tilemap collisionTilemap;
    public string targetTileName = "temple-sliced_02";
    public string sceneToLoad = "Scene_Biome_Desert";

    private Collider2D playerCollider;

    private void Start()
    {
        playerCollider = GetComponentInChildren<CircleCollider2D>();
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == collisionTilemap.gameObject)
        {
            CheckTilesUnderPlayer();
        }
    }

    // Also check while staying, in case the player walks into a tile 
    // without the collider technically "re-entering" the tilemap object
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject == collisionTilemap.gameObject)
        {
            CheckTilesUnderPlayer();
        }
    }

    private void CheckTilesUnderPlayer()
    {
        // Get the bounding box of the player's collider in world space
        Bounds bounds = playerCollider.bounds;

        // Convert the corners of the bounds to tilemap cell coordinates
        Vector3Int minCell = collisionTilemap.WorldToCell(bounds.min);
        Vector3Int maxCell = collisionTilemap.WorldToCell(bounds.max);

        // Loop through every tile cell covered by the player's bounds
        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                TileBase tile = collisionTilemap.GetTile(cellPos);

                if (tile != null && tile.name == targetTileName)
                {
                    Debug.Log($"Overlap detected with {targetTileName} at {cellPos}");
                    SceneManager.LoadScene(sceneToLoad);
                    return; // Exit once we find a match to avoid multiple scene loads
                }
            }
        }
    }
}
