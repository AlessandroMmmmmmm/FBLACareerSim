using TMPro;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public int packagesRequired = 3;
    public int currentPackagesInTruck = 0;
    public int packagesDelivered = 0; // Track deliveries
    public float shiftTimer = 120f;
    public float totalPenalties = 0f; // Accumulated off-road penalty seconds
    public GameObject driveHUD;

    [Header("UI References")]
    public TextMeshProUGUI timerText;   // Drag 'Timer_Text' here
    public TextMeshProUGUI packageText; // Drag 'Package_Text' here

    private bool timerRunning = false; // Controls when timer updates

    void Start()
    {
        // Start the game paused until the truck is ready
        timerRunning = false;
        packagesDelivered = 0;
    }

    void Update()
    {
        if (!timerRunning) return; // Only update if the timer is running

        shiftTimer -= Time.deltaTime;

        // Update the UI text fields
        timerText.text = $"Time Left: {Mathf.Round(shiftTimer)}s";
        packageText.text = $"Deliveries: {packagesDelivered}/{packagesRequired}";

        if (shiftTimer <= 0) EndShift(false);
    }

    // Called by WarehouseManager once secure packages is clicked
    public void StartShiftTimer()
    {
        timerRunning = true;
        packagesDelivered = 0;
        totalPenalties = 0f;

        // Initialize the Delivery Text immediately so it doesn't show old data
        if (packageText != null)
            packageText.text = $"Deliveries: 0/{packagesRequired}";
    }

    // Called by ArcadeTruck when a package is delivered
    public void RegisterDelivery()
    {
        packagesDelivered++;
        Debug.Log($"Delivery registered: {packagesDelivered}/{packagesRequired}");

        // Update UI immediately
        if (packageText != null)
        {
            packageText.text = $"Deliveries: {packagesDelivered}/{packagesRequired}";
        }
    }

    public void CheckDelivery()
    {
        if (packagesDelivered >= packagesRequired)
        {
            EndShift(true);
        }
        else
        {
            Debug.Log($"Not enough deliveries! {packagesDelivered}/{packagesRequired}");
        }
    }

    void EndShift(bool success)
    {
        timerRunning = false; // Stop the timer when the shift ends

        // Show scoring
        ShiftScoring scoring = FindObjectOfType<ShiftScoring>();
        if (scoring != null)
        {
            scoring.ShowEndOfShiftReport(success);
        }

        Debug.Log(success ? "Shift Complete! Career XP Gained." : "Shift Failed. Try again.");
        Time.timeScale = 0; // Pause game
    }
}
