using UnityEngine;

public class IsometricTruckRenderer : MonoBehaviour
{
    // 0 = Forward (North relative to truck), 1 = Backward (South relative to truck)
    public static readonly string[] staticDirections = { "Static N", "Static S" };
    public static readonly string[] runDirections = { "Run N", "Run S" };

    Animator animator;
    int lastDirection;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Pass the movement vector from your input (e.g., transform.forward * verticalInput)
    public void SetDirection(Vector3 worldMovement)
    {
        string[] directionArray;

        // 1. Check if we are moving
        if (worldMovement.magnitude < .01f)
        {
            directionArray = staticDirections;
        }
        else
        {
            directionArray = runDirections;

            // 2. Determine if movement is Forward or Backward relative to the truck
            // Dot Product returns 1 if moving exactly Forward, -1 if exactly Backward
            float dot = Vector3.Dot(transform.forward, worldMovement.normalized);

            // If dot is positive, we are moving generally forward. If negative, backward.
            lastDirection = (dot >= 0) ? 0 : 1;
        }

        // 3. Play animation
        animator.Play(directionArray[lastDirection]);
    }
}