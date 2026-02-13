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

    [Header("Scan Instruction")]
    public GameObject scanInstructionBox;    // NEW: Panel/Text telling user to hold click

    [Header("Minigame Logic")]
    public TextMeshProUGUI scanCountText;
    public int packagesScanned = 0;
    public int requiredPackages = 3;
    public WaveFunctionCollapse cityGenerator;

    void Start()
    {
        // Start everything hidden
        truckInstructionsText.SetActive(false);
        enterWarehouseButton.SetActive(false);
        minigamePanel.SetActive(false);

        // Hide scan instruction at start
        if (scanInstructionBox != null)
            scanInstructionBox.SetActive(false);
    }

    // Step 1: Called by LoadingZone Trigger
    public void SetInLoadingZone(bool isInside)
    {
        if (masterWarehouseUI != null)
            masterWarehouseUI.SetActive(isInside);

        if (isInside)
        {
            truckInstructionsText.SetActive(true);
            enterWarehouseButton.SetActive(false);
            minigamePanel.SetActive(false);
        }
    }

    // Step 2: Called by Ramp_Trigger
    public void SetAtRamp(bool isAtRamp)
    {
        if (isAtRamp)
        {
            truckInstructionsText.SetActive(false);
            enterWarehouseButton.SetActive(true);
        }
        else
        {
            enterWarehouseButton.SetActive(false);
        }
    }

    // Step 3: Button Click - opens the minigame
    public void OnEnterWarehouseClick()
    {
        if (enterWarehouseButton != null)
            enterWarehouseButton.SetActive(false);

        if (truckInstructionsText != null)
            truckInstructionsText.SetActive(false);

        if (minigamePanel != null)
            minigamePanel.SetActive(true);

        // Show the scan instruction when minigame opens
        if (scanInstructionBox != null)
            scanInstructionBox.SetActive(true);

        // Reset scan count each time warehouse is entered
        packagesScanned = 0;
        if (scanCountText != null)
            scanCountText.text = $"Scanned: 0/{requiredPackages}";

        Debug.Log("Career Quest: User entered Warehouse. Launching Minigame.");
    }

    // Step 4: Called each time a package is scanned
    public void OnScanPackage()
    {
        packagesScanned++;

        if (scanCountText != null)
            scanCountText.text = $"Scanned: {packagesScanned}/{requiredPackages}";

        if (packagesScanned >= requiredPackages)
        {
            // All packages scanned - hide instruction, show secure button
            if (scanInstructionBox != null)
                scanInstructionBox.SetActive(false);

            ShowSecureButton();
        }
    }

    // Step 5: Show the final secure button
    private void ShowSecureButton()
    {
        if (securePackagesButton != null)
            securePackagesButton.SetActive(true);

        if (driveHUD != null)
            driveHUD.SetActive(false);
    }

    // Step 6: Secure packages - start the shift
    public void OnSecurePackagesClick()
    {
        // Hide warehouse UI
        masterWarehouseUI.SetActive(false);

        // Show the delivery HUD
        if (driveHUD != null)
            driveHUD.SetActive(true);

        // Start shift timer + generate new roads
        if (deliveryManager != null)
            deliveryManager.StartShiftTimer();

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
