using TMPro;
using UnityEngine;

public class WarehouseManager : MonoBehaviour
{
    [Header("UI State Elements")]
    public GameObject truckInstructionsText;
    public GameObject enterWarehouseButton;
    public GameObject minigamePanel;
    public GameObject masterWarehouseUI;
    public GameObject securePackagesButton;
    public GameObject driveHUD;
    public DeliveryManager deliveryManager;

    [Header("Scan Instruction")]
    public GameObject scanInstructionBox;
    public GameObject minimapContainer;

    [Header("Minigame Logic")]
    public TextMeshProUGUI scanCountText;
    public int packagesScanned = 0;
    public int requiredPackages = 3;

    [Header("Audio")]
    public AudioClip securePackagesSound; // "Packages secured" confirmation sound

    private AudioSource audioSource;

    void Start()
    {
        truckInstructionsText.SetActive(false);
        enterWarehouseButton.SetActive(false);
        minigamePanel.SetActive(false);

        if (scanInstructionBox != null)
            scanInstructionBox.SetActive(false);

        if (minimapContainer != null)
            minimapContainer.SetActive(false);

        // Create audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

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

    public void OnEnterWarehouseClick()
    {
        if (enterWarehouseButton != null)
            enterWarehouseButton.SetActive(false);

        if (truckInstructionsText != null)
            truckInstructionsText.SetActive(false);

        if (minigamePanel != null)
            minigamePanel.SetActive(true);

        if (scanInstructionBox != null)
            scanInstructionBox.SetActive(true);

        packagesScanned = 0;
        if (scanCountText != null)
            scanCountText.text = $"Scanned: 0/{requiredPackages}";

        Debug.Log("Career Quest: User entered Warehouse. Launching Minigame.");
    }

    public void OnScanPackage()
    {
        packagesScanned++;

        if (scanCountText != null)
            scanCountText.text = $"Scanned: {packagesScanned}/{requiredPackages}";

        if (packagesScanned >= requiredPackages)
        {
            if (scanInstructionBox != null)
                scanInstructionBox.SetActive(false);

            ShowSecureButton();
        }
    }

    private void ShowSecureButton()
    {
        if (securePackagesButton != null)
            securePackagesButton.SetActive(true);

        if (driveHUD != null)
            driveHUD.SetActive(false);
    }

    public void OnSecurePackagesClick()
    {
        // Play secure packages sound
        if (securePackagesSound != null && audioSource != null)
            audioSource.PlayOneShot(securePackagesSound, 0.8f);

        masterWarehouseUI.SetActive(false);

        if (driveHUD != null)
            driveHUD.SetActive(true);

        if (minimapContainer != null)
            minimapContainer.SetActive(true);

        RoadGenerator roadGen = FindObjectOfType<RoadGenerator>();
        if (roadGen != null)
            roadGen.GenerateRoadNetwork();

        if (deliveryManager != null)
            deliveryManager.StartShiftTimer();
    }

    private void FinishCareerTask()
    {
        minigamePanel.SetActive(false);
        Debug.Log("Quest Progress: Logistics Task Complete. Starting City Drive.");
    }
}
