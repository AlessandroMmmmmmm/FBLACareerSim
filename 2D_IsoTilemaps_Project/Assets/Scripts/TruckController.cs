using UnityEngine;

public class ArcadeTruck : MonoBehaviour
{
    public float speed = 40f;
    public float turnSpeed = 120f;
    public GameObject packageVisual;
    public bool hasPackage = false;

    [Header("Delivery System")]
    public int packagesDelivered = 0;
    private bool inDeliveryZone = false;
    private string currentDeliveryZone = "";

    private Rigidbody rb;
    private DeliveryManager deliveryManager;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // COMPLETELY FREEZE X and Z rotation - truck stays upright always
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (packageVisual != null) packageVisual.SetActive(false);

        // Find delivery manager
        deliveryManager = FindObjectOfType<DeliveryManager>();
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

        // 5. DELIVERY INPUT - Press E to deliver when in zone
        if (Input.GetKeyDown(KeyCode.E) && inDeliveryZone && hasPackage)
        {
            DeliverPackage();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check for delivery zones
        if (other.CompareTag("Tile1Hitbox") || other.CompareTag("Tile2Hitbox") || other.CompareTag("Tile3Hitbox"))
        {
            inDeliveryZone = true;
            currentDeliveryZone = other.tag;
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
            Debug.Log("Left delivery zone");
        }
    }

    private void DeliverPackage()
    {
        // Unload package
        hasPackage = false;
        if (packageVisual != null)
        {
            packageVisual.SetActive(false);
        }

        // Increment delivery count
        packagesDelivered++;

        // Update delivery manager
        if (deliveryManager != null)
        {
            deliveryManager.currentPackagesInTruck--;
            Debug.Log($"Package delivered! Total: {packagesDelivered}/{deliveryManager.packagesRequired}");

            // Check if all deliveries complete
            if (packagesDelivered >= deliveryManager.packagesRequired)
            {
                deliveryManager.CheckDelivery();
            }
        }
        else
        {
            Debug.Log($"Package delivered! Total: {packagesDelivered}");
        }
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
