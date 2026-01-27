using TMPro;
using UnityEngine;

public class WarehouseManager : MonoBehaviour
{
    [Header("UI State Elements")]
    public GameObject truckInstructionsText; // "Park at Ramp"
    public GameObject enterWarehouseButton;  // The button that appears at the ramp
    public GameObject minigamePanel;         // The 2D UI scanning minigame
    public GameObject masterWarehouseUI;
    public GameObject securePackagesButton;
    public GameObject driveHUD;
    public DeliveryManager deliveryManager;

    [Header("Minigame Logic")]
    public TextMeshProUGUI scanCountText;
    public int packagesScanned = 0;
    public int requiredPackages = 3;
    public WaveFunctionCollapse cityGenerator;

    void Start()
    {
        // 2026 Security: Start everything hidden
        truckInstructionsText.SetActive(false);
        enterWarehouseButton.SetActive(false);
        minigamePanel.SetActive(false);
    }

    // Step 1: Called by LoadingZone Trigger
    public void SetInLoadingZone(bool isInside)
    {
        // Toggle the master folder
        if (masterWarehouseUI != null)
            masterWarehouseUI.SetActive(isInside);

        // If entering, show only the first instruction
        if (isInside)
        {
            truckInstructionsText.SetActive(true);
            enterWarehouseButton.SetActive(false); // Hide button until they hit the Ramp_Trigger
            minigamePanel.SetActive(false);
        }
    }



    // Step 2: Called by Ramp_Trigger
    public void SetAtRamp(bool isAtRamp)
    {
        if (isAtRamp)
        {
            truckInstructionsText.SetActive(false); // Hide instructions
            enterWarehouseButton.SetActive(true);   // Show button
        }
        else
        {
            enterWarehouseButton.SetActive(false);
        }
    }

    // Step 3: Button Click Action
    // Inside WarehouseManager.cs

    public void OnEnterWarehouseClick()
    {
        // 1. Hide the button that got us here
        if (enterWarehouseButton != null)
            enterWarehouseButton.SetActive(false);

        // 2. Hide the instructions (if they were still visible)
        if (truckInstructionsText != null)
            truckInstructionsText.SetActive(false);

        // 3. Show the 2D UI Minigame
        if (minigamePanel != null)
            minigamePanel.SetActive(true);

        Debug.Log("Career Quest: User entered Warehouse. Launching Minigame.");
    }


    public void OnScanPackage()
    {
        packagesScanned++;

        // Update the minigame text
        if (scanCountText != null)
            scanCountText.text = $"Scanned: {packagesScanned}/{requiredPackages}";

        if (packagesScanned >= requiredPackages)
        {
            // ONLY show the secure button. 
            // DO NOT activate driveHUD or InitializeWaveFunction here!
            ShowSecureButton();
        }
    }





    // Step 2: Show the final button
    private void ShowSecureButton()
    {
        // Ensure the scanning interface stays up or hides, 
        // but the HUD must stay FALSE.
        if (securePackagesButton != null)
        {
            securePackagesButton.SetActive(true);
        }

        // Safety check:
        if (driveHUD != null) driveHUD.SetActive(false);
    }



    public void OnSecurePackagesClick()
    {
        // Hide warehouse UI
        masterWarehouseUI.SetActive(false);

        // Show ONLY the delivery info now
        if (driveHUD != null) driveHUD.SetActive(true);

        // Build the city
        cityGenerator.InitializeWaveFunction();
    }





    private void FinishCareerTask()
    {
        minigamePanel.SetActive(false);
        cityGenerator.InitializeWaveFunction();
        Debug.Log("Quest Progress: Logistics Task Complete. Starting City Drive.");
    }
}
