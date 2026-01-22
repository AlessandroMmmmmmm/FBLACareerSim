using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;       // Drag your Truck here
    public Vector3 offset;         // The distance between truck and camera
    public float smoothSpeed = 5f; // How smoothly the camera follows

    void Start()
    {
        // Automatically calculate the offset based on where you placed the camera in the scene
        if (target != null)
        {
            offset = transform.position - target.position;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Calculate the desired position
        Vector3 desiredPosition = target.position + offset;

        // 2. Smoothly move the camera to that position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 3. Update position
        transform.position = desiredPosition;
    }
}
