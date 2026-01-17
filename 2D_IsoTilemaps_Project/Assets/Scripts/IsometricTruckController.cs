using UnityEngine;

public class IsometricTruckController : MonoBehaviour
{

    public float movementSpeed = 1f;
    IsometricTruckRenderer isoRenderer;
    public float rotationSpeed = 100f;
    Rigidbody2D rbody;

    private void Awake()
    {
        rbody = GetComponent<Rigidbody2D>();
        isoRenderer = GetComponentInChildren<IsometricTruckRenderer>();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        // 1. Get Vertical Input only for Forward/Backward movement
        float moveInput = Input.GetAxis("Vertical");

        // 2. Calculate movement relative to the truck's local forward
        // 'transform.forward' is the blue arrow direction of your 3D truck
        Vector3 worldMovement = transform.right * moveInput * movementSpeed;

        // 3. Update the Renderer with the movement vector 
        // This tells it to play "Run N" if moving forward, or "Run S" if backward
        isoRenderer.SetDirection(worldMovement);

        // 4. Move the Rigidbody2D using the calculated movement
        // Since this is a 2D Rigidbody, we convert the 3D Vector3 back to Vector2
        Vector2 currentPos = rbody.position;
        Vector2 movement2D = new Vector2(worldMovement.x, worldMovement.z);
        Vector2 newPos = currentPos + movement2D * Time.fixedDeltaTime;

        rbody.MovePosition(newPos);
    }

    void Update()
    {
        // Handle rotation on the Y-axis (Standard for 3D trucks in isometric views)
        float rotationInput = Input.GetAxis("Horizontal"); // Uses A/D or Left/Right

        // Rotate counter-clockwise when 'A' or 'Left' is pressed
        // Note: We use 'rotationInput' so both 'A' and 'D' work automatically
        transform.Rotate(0, rotationInput * rotationSpeed * Time.deltaTime, 0);
    }
}
