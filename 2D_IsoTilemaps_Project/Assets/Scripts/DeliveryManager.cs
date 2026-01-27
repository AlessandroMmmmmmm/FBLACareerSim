using TMPro; // Add this using statement
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public int packagesRequired = 3;
    public int currentPackagesInTruck = 0;
    public float shiftTimer = 120f;
    public GameObject driveHUD;

    [Header("UI References")]
    public TextMeshProUGUI timerText;   // Drag 'Timer_Text' here
    public TextMeshProUGUI packageText; // Drag 'Package_Text' here
    private bool timerRunning = false; // New: Controls when timer updates

    void Start()
    {
        // Start the game paused until the truck is ready
        timerRunning = false;
    }

    void Update()
    {
        if (!timerRunning) return; // Only update if the timer is running

        shiftTimer -= Time.deltaTime;

        // Update the UI text fields
        timerText.text = $"Time Left: {Mathf.Round(shiftTimer)}s";
        packageText.text = $"Packages: {currentPackagesInTruck}/{packagesRequired}";

        if (shiftTimer <= 0) EndShift(false);
    }

    // New: Called by LoadingZone once scanning is complete
    public void StartShiftTimer()
    {
        timerRunning = true;

        // Initialize the Delivery Text immediately so it doesn't show old data
        if (packageText != null)
        {
            packageText.text = $"Deliveries: 0/{packagesRequired}";
        }
    }


    public void CheckDelivery()
    {
        if (currentPackagesInTruck >= packagesRequired) EndShift(true);
        else Debug.Log("Not enough packages!");
    }

    void EndShift(bool success)
    {
        timerRunning = false; // Stop the timer when the shift ends
        Debug.Log(success ? "Shift Complete! Career XP Gained." : "Shift Failed. Try again.");
        Time.timeScale = 0; // Pause game
    }
}
