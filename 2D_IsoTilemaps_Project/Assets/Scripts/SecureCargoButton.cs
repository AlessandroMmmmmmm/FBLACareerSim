using UnityEngine;

public class CargoSecurer : MonoBehaviour
{
    public Transform truckBed; // Drag your 'TruckBed' empty object here
    public GameObject secureButton; // Drag the UI Button here to hide it after use

    public void SecureCargo()
    {
        // Find all boxes that are children of the truck bed
        foreach (Transform box in truckBed)
        {
            Rigidbody rb = box.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // 1. Parent the box to the truck bed so it moves with the truck
                box.SetParent(truckBed);

                // 2. Set to Kinematic ONLY if you want them 100% frozen. 
                // For a 'Career Quest' challenge, keep it FALSE so they can still fall out!
                rb.isKinematic = false;

                // 3. Improve physics stability for high-speed driving in Unity 6
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
        }

        Debug.Log("Cargo Secured! Shift starting...");

        // Hide the button after securing so the player can't spam it while driving
        if (secureButton != null) secureButton.SetActive(false);
    }
}
