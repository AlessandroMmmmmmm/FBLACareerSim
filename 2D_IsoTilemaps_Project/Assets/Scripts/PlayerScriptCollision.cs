using UnityEngine;

using UnityEngine.Tilemaps;

public class PlayerCollision : MonoBehaviour
{
    public Tilemap collisionTilemap; // Assign the Collision Tilemap in the Inspector
    public string targetTileName = "desert-sliced_04"; // Name of the tile to check collision with

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
                // Add your custom logic here
            }
        }
    }
}
