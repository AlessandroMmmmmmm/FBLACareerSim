// 1/11/2026 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class PlayerTileSceneSwitcher : MonoBehaviour
{
    public Tilemap collisionTilemap; // Assign the Collision Tilemap in the Inspector
    public string targetTileName = "temple-sliced_02"; // Name of the tile to check collision with
    public string sceneToLoad = "Scene_Biome_Desert"; // Name of the scene to load upon collision

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision is with the tilemap
        if (collision.gameObject == collisionTilemap.gameObject)
        {
            // Get the contact point of the collision
            Vector3 contactPoint = collision.contacts[0].point;

            // Convert the contact point to tilemap cell position
            Vector3Int cellPosition = collisionTilemap.WorldToCell(contactPoint);

            // Get the tile at the cell position
            TileBase collidedTile = collisionTilemap.GetTile(cellPosition);

            // Check if the tile matches the target tile name
            if (collidedTile != null && collidedTile.name == targetTileName)
            {
                Debug.Log($"Player collided with the target tile: {targetTileName} at position {cellPosition}");

                // Load the new scene
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }
}