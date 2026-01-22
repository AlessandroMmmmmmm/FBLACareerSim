using UnityEngine;

public class ArcadeTruck : MonoBehaviour
{
    public float speed = 40f;
    public float turnSpeed = 120f;
    public GameObject packageVisual;
    public bool hasPackage = false;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Freeze X and Z rotation so the truck doesn't tip over
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (packageVisual != null) packageVisual.SetActive(false);
    }

    void FixedUpdate()
    {
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
        // This tells Unity: "Unless I am pressing A or D, stop spinning immediately."
        if (turnInput == 0)
        {
            rb.angularVelocity = Vector3.zero;
        }
        // ----------------------------------------

        // 4. BRAKING
        if (moveInput == 0)
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, Time.fixedDeltaTime * 3f);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
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
}
