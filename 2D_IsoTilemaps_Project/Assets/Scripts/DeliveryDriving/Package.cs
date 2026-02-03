using UnityEngine;

public class Package : MonoBehaviour
{
    public bool isScanned = false;
    private DeliveryManager manager;

    void Start() { manager = FindObjectOfType<DeliveryManager>(); }

    // Simulates the 'Scanner' task
    public void ScanPackage()
    {
        if (!isScanned)
        {
            isScanned = true;
            manager.currentPackagesInTruck++;
            GetComponent<Renderer>().material.color = Color.green; // Visual feedback
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // If package hits the road (floor) after being loaded
        if (collision.gameObject.CompareTag("Road") && isScanned)
        {
            isScanned = false;
            manager.currentPackagesInTruck--;
            GetComponent<Renderer>().material.color = Color.red;
        }
    }
}
