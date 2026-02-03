using UnityEngine;

public class DirectHitboxSync : MonoBehaviour
{
    public Transform hitbox2D;      // Drag your 'Truck_Hitbox' here
    public Camera truckCamera;      // The camera looking at the 3D truck
    public Camera isometricCamera;  // Your main 2D game camera

    void Update()
    {
        // 1. Where is the 3D truck on the screen?
        Vector3 screenPoint = truckCamera.WorldToScreenPoint(transform.position);

        // 2. Where is that screen point in the 2D world?
        Vector3 worldPoint = isometricCamera.ScreenToWorldPoint(screenPoint);
        worldPoint.z = 0; // Lock to 2D plane

        // 3. Move the hitbox there
        hitbox2D.position = worldPoint;

        // 4. Sync Rotation (3D Y to 2D Z)
        hitbox2D.rotation = Quaternion.Euler(0, 0, -transform.eulerAngles.y);
    }
}
