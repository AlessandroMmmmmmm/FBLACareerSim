using TMPro;
using UnityEngine;

public class ArcadeTruck : MonoBehaviour
{
    public float speed = 40f;
    public float turnSpeed = 120f;
    public GameObject packageVisual;
    public bool hasPackage = false;

    [Header("Delivery System")]
    public int packagesDelivered = 0;
    public bool[] zoneDelivered = new bool[3]; // Tracks WHICH specific zones were delivered
    public GameObject box1; // Assign Box1 GameObject
    public GameObject box2; // Assign Box2 GameObject
    public GameObject box3; // Assign Box3 GameObject
    public TextMeshProUGUI deliveryInstructionText; // Shows "Press E to deliver"

    [Header("Speed Limit")]
    public float speedLimit = 20f; // Max allowed speed (units/sec)

    private bool inDeliveryZone = false;
    private string currentDeliveryZone = "";

    private Rigidbody rb;
    private DeliveryManager deliveryManager;
    private MinimapController minimap;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // COMPLETELY FREEZE X and Z rotation - truck stays upright always
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (packageVisual != null) packageVisual.SetActive(false);

        // Hide all boxes at start
        if (box1 != null) box1.SetActive(false);
        if (box2 != null) box2.SetActive(false);
        if (box3 != null) box3.SetActive(false);

        // Hide instruction text at start
        if (deliveryInstructionText != null)
        {
            deliveryInstructionText.gameObject.SetActive(false);
        }

        // Reset zone tracking
        zoneDelivered = new bool[3];

        // Find delivery manager and minimap
        deliveryManager = FindObjectOfType<DeliveryManager>();
        minimap = FindObjectOfType<MinimapController>();
    }

    void Update()
    {
        // DELIVERY INPUT - Press E to deliver when in zone
        // Put in Update() for better input responsiveness
        if (Input.GetKeyDown(KeyCode.E) && inDeliveryZone && hasPackage)
        {
            DeliverPackage();
        }
    }

    void FixedUpdate()
    {
        // Force truck to stay level (extra safety)
        Vector3 currentRotation = transform.eulerAngles;
        transform.eulerAngles = new Vector3(0, currentRotation.y, 0);

        // 1. INPUT
        float moveInput = 0;
        if (Input.GetKey(KeyCode.W)) moveInput = 1;
        else if (Input.GetKey(KeyCode.S)) moveInput = -1;

        float turnInput = 0;
        if (Input.GetKey(KeyCode.D)) turnInput = 1;
        else if (Input.GetKey(KeyCode.A)) turnInput = -1;

        // 2. MOVEMENT
        Vector3 movement = transform.forward * moveInput * speed;
        rb.AddForce(movement, ForceMode.Acceleration);

        // 3. STEERING (Manual control)
        float rotation = turnInput * turnSpeed * Time.fixedDeltaTime;
        transform.Rotate(0, rotation, 0);

        // --- THE FIX: STOP AUTOMATIC SPINNING ---
        if (turnInput == 0)
        {
            rb.angularVelocity = Vector3.zero;
        }

        // 4. BRAKING
        if (moveInput == 0)
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, Time.fixedDeltaTime * 3f);
        }

        // 5. SPEED CAP
        if (rb.linearVelocity.magnitude > speedLimit)
            rb.linearVelocity = rb.linearVelocity.normalized * speedLimit;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check for delivery zones
        if (other.CompareTag("Tile1Hitbox") || other.CompareTag("Tile2Hitbox") || other.CompareTag("Tile3Hitbox"))
        {
            inDeliveryZone = true;
            currentDeliveryZone = other.tag;

            // Show instruction
            if (deliveryInstructionText != null)
            {
                deliveryInstructionText.gameObject.SetActive(true);
                deliveryInstructionText.text = "Press E to Deliver Package";
            }

            Debug.Log($"Entered {currentDeliveryZone}. Press E to deliver package!");
        }

        // Old pickup zone code (if still needed)
        if (other.CompareTag("PickupZone") && !hasPackage)
        {
            hasPackage = true;
            if (packageVisual) packageVisual.SetActive(true);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("DropOffZone") && hasPackage)
        {
            hasPackage = false;
            if (packageVisual) packageVisual.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Exit delivery zone
        if (other.CompareTag("Tile1Hitbox") || other.CompareTag("Tile2Hitbox") || other.CompareTag("Tile3Hitbox"))
        {
            inDeliveryZone = false;
            currentDeliveryZone = "";

            // Hide instruction
            if (deliveryInstructionText != null)
            {
                deliveryInstructionText.gameObject.SetActive(false);
            }

            Debug.Log("Left delivery zone");
        }
    }

    private void DeliverPackage()
    {
        // Determine which zone index (0, 1, 2) was delivered
        int zoneIndex = -1;
        if (currentDeliveryZone == "Tile1Hitbox") zoneIndex = 0;
        else if (currentDeliveryZone == "Tile2Hitbox") zoneIndex = 1;
        else if (currentDeliveryZone == "Tile3Hitbox") zoneIndex = 2;

        if (zoneIndex == -1) return;

        // Don't deliver to same zone twice
        if (zoneDelivered[zoneIndex])
        {
            Debug.Log($"Zone {zoneIndex + 1} already delivered!");
            return;
        }

        // Mark this specific zone as delivered
        zoneDelivered[zoneIndex] = true;
        packagesDelivered++;

        // Show the appropriate box at the door
        if (zoneIndex == 0 && box1 != null)
        {
            box1.SetActive(true);
            Debug.Log("Package delivered to Tile 1!");
        }
        else if (zoneIndex == 1 && box2 != null)
        {
            box2.SetActive(true);
            Debug.Log("Package delivered to Tile 2!");
        }
        else if (zoneIndex == 2 && box3 != null)
        {
            box3.SetActive(true);
            Debug.Log("Package delivered to Tile 3!");
        }

        // Tell minimap which SPECIFIC marker to turn green
        if (minimap != null)
            minimap.OnDeliveryMade(zoneIndex);

        // Update delivery manager
        if (deliveryManager != null)
        {
            Debug.Log($"Package delivered! Total: {packagesDelivered}/{deliveryManager.packagesRequired}");

            // Update delivery manager's package count
            deliveryManager.RegisterDelivery();

            // Check if this is the LAST delivery
            if (packagesDelivered >= deliveryManager.packagesRequired)
            {
                // Hide package visual only after last delivery
                hasPackage = false;
                if (packageVisual != null)
                {
                    packageVisual.SetActive(false);
                }

                deliveryManager.CheckDelivery();
            }
        }
        else
        {
            // No delivery manager - just hide after each delivery
            hasPackage = false;
            if (packageVisual != null)
            {
                packageVisual.SetActive(false);
            }

            Debug.Log($"Package delivered! Total: {packagesDelivered}");
        }

        // Hide instruction after delivery
        if (deliveryInstructionText != null)
        {
            deliveryInstructionText.gameObject.SetActive(false);
        }

        // Exit the zone
        inDeliveryZone = false;
    }

    /// <summary>
    /// Call this method when SecureCargo button is clicked
    /// </summary>
    public void LoadPackage()
    {
        hasPackage = true;
        if (packageVisual != null)
        {
            packageVisual.SetActive(true);
        }
    }

    /// <summary>
    /// Call this method when package is delivered
    /// </summary>
    public void UnloadPackage()
    {
        hasPackage = false;
        if (packageVisual != null)
        {
            packageVisual.SetActive(false);
        }
    }
}
