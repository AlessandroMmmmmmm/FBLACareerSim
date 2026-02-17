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
    public bool[] zoneDelivered = new bool[3];
    public GameObject box1;
    public GameObject box2;
    public GameObject box3;
    public TextMeshProUGUI deliveryInstructionText;

    [Header("Speed Limit")]
    public float speedLimit = 20f;

    [Header("Audio")]
    public AudioClip deliverySound; // Package delivered confirmation sound

    private bool inDeliveryZone = false;
    private string currentDeliveryZone = "";
    private Rigidbody rb;
    private DeliveryManager deliveryManager;
    private MinimapController minimap;
    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (packageVisual != null) packageVisual.SetActive(false);

        if (box1 != null) box1.SetActive(false);
        if (box2 != null) box2.SetActive(false);
        if (box3 != null) box3.SetActive(false);

        if (deliveryInstructionText != null)
            deliveryInstructionText.gameObject.SetActive(false);

        zoneDelivered = new bool[3];

        deliveryManager = FindObjectOfType<DeliveryManager>();
        minimap = FindObjectOfType<MinimapController>();

        // Create audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && inDeliveryZone && hasPackage)
            DeliverPackage();
    }

    void FixedUpdate()
    {
        Vector3 currentRotation = transform.eulerAngles;
        transform.eulerAngles = new Vector3(0, currentRotation.y, 0);

        float moveInput = 0;
        if (Input.GetKey(KeyCode.W)) moveInput = 1;
        else if (Input.GetKey(KeyCode.S)) moveInput = -1;

        float turnInput = 0;
        if (Input.GetKey(KeyCode.D)) turnInput = 1;
        else if (Input.GetKey(KeyCode.A)) turnInput = -1;

        Vector3 movement = transform.forward * moveInput * speed;
        rb.AddForce(movement, ForceMode.Acceleration);

        float rotation = turnInput * turnSpeed * Time.fixedDeltaTime;
        transform.Rotate(0, rotation, 0);

        if (turnInput == 0)
            rb.angularVelocity = Vector3.zero;

        if (moveInput == 0)
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, Time.fixedDeltaTime * 3f);

        if (rb.linearVelocity.magnitude > speedLimit)
            rb.linearVelocity = rb.linearVelocity.normalized * speedLimit;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tile1Hitbox") || other.CompareTag("Tile2Hitbox") || other.CompareTag("Tile3Hitbox"))
        {
            inDeliveryZone = true;
            currentDeliveryZone = other.tag;

            if (deliveryInstructionText != null)
            {
                deliveryInstructionText.gameObject.SetActive(true);
                deliveryInstructionText.text = "Press E to Deliver Package";
            }

            Debug.Log($"Entered {currentDeliveryZone}. Press E to deliver package!");
        }

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
        if (other.CompareTag("Tile1Hitbox") || other.CompareTag("Tile2Hitbox") || other.CompareTag("Tile3Hitbox"))
        {
            inDeliveryZone = false;
            currentDeliveryZone = "";

            if (deliveryInstructionText != null)
                deliveryInstructionText.gameObject.SetActive(false);

            Debug.Log("Left delivery zone");
        }
    }

    private void DeliverPackage()
    {
        int zoneIndex = -1;
        if (currentDeliveryZone == "Tile1Hitbox") zoneIndex = 0;
        else if (currentDeliveryZone == "Tile2Hitbox") zoneIndex = 1;
        else if (currentDeliveryZone == "Tile3Hitbox") zoneIndex = 2;

        if (zoneIndex == -1) return;

        if (zoneDelivered[zoneIndex])
        {
            Debug.Log($"Zone {zoneIndex + 1} already delivered!");
            return;
        }

        // Play delivery sound
        if (deliverySound != null && audioSource != null)
            audioSource.PlayOneShot(deliverySound, 0.75f);

        zoneDelivered[zoneIndex] = true;
        packagesDelivered++;

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

        if (minimap != null)
            minimap.OnDeliveryMade(zoneIndex);

        if (deliveryManager != null)
        {
            Debug.Log($"Package delivered! Total: {packagesDelivered}/{deliveryManager.packagesRequired}");

            deliveryManager.RegisterDelivery();

            if (packagesDelivered >= deliveryManager.packagesRequired)
            {
                hasPackage = false;
                if (packageVisual != null)
                    packageVisual.SetActive(false);

                deliveryManager.CheckDelivery();
            }
        }
        else
        {
            hasPackage = false;
            if (packageVisual != null)
                packageVisual.SetActive(false);

            Debug.Log($"Package delivered! Total: {packagesDelivered}");
        }

        if (deliveryInstructionText != null)
            deliveryInstructionText.gameObject.SetActive(false);

        inDeliveryZone = false;
    }

    public void LoadPackage()
    {
        hasPackage = true;
        if (packageVisual != null)
            packageVisual.SetActive(true);
    }

    public void UnloadPackage()
    {
        hasPackage = false;
        if (packageVisual != null)
            packageVisual.SetActive(false);
    }
}
