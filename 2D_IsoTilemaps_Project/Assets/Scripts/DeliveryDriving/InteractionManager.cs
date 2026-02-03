using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public LoadingZone zone;
    public Transform truckBedLocation; // An empty GameObject placed inside your truck's cargo area
    public LayerMask packageLayer; // Set your box prefabs to a layer called "Packages"

    void Update()
    {
        // Only allow loading if the truck is parked in the warehouse zone
        if (zone.isTruckInZone && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 10f, packageLayer))
            {
                LoadPackage(hit.collider.gameObject);
            }
        }
    }

    void LoadPackage(GameObject box)
    {
        Package packageScript = box.GetComponent<Package>();

        if (packageScript != null && !packageScript.isScanned)
        {
            packageScript.ScanPackage(); // From our previous script

            // Move box to truck bed and make it a "child" so it moves with the truck
            box.transform.position = truckBedLocation.position + new Vector3(0, 0.5f, 0);
            box.transform.SetParent(truckBedLocation);

            // Option: Disable physics temporarily so it doesn't fly out during loading
            // box.GetComponent<Rigidbody>().isKinematic = true; 
        }
    }
}
