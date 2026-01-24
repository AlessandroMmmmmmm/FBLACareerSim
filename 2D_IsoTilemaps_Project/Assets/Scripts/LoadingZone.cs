using UnityEngine;

public class LoadingZone : MonoBehaviour
{
    public bool isTruckInZone = false;

    [Header("UI Elements")]
    public GameObject loadingInstructionsUI; // Text: "Park here to load"
    public GameObject secureButton;          // The button that calls SecureCargo

    void Start()
    {
        // Ensure UI is hidden when the career level starts
        if (loadingInstructionsUI != null) loadingInstructionsUI.SetActive(false);
        if (secureButton != null) secureButton.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        // Requirement: Your truck MUST have the "Player" tag
        if (other.CompareTag("Player"))
        {
            isTruckInZone = true;

            if (loadingInstructionsUI != null) loadingInstructionsUI.SetActive(true);
            if (secureButton != null) secureButton.SetActive(true);

            Debug.Log("Entered Loading Zone: Career Task - Load and Secure Cargo");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isTruckInZone = false;

            if (loadingInstructionsUI != null) loadingInstructionsUI.SetActive(false);
            if (secureButton != null) secureButton.SetActive(false);
        }
    }
}
