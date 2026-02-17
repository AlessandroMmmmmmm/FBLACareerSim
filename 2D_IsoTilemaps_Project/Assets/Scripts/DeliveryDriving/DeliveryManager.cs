using TMPro;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public int packagesRequired = 3;
    public int currentPackagesInTruck = 0;
    public int packagesDelivered = 0;
    public float shiftTimer = 120f;
    public float totalPenalties = 0f;
    public GameObject driveHUD;

    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI packageText;

    [Header("Music")]
    public DeliveryMusicController musicController;

    private bool timerRunning = false;

    void Start()
    {
        timerRunning = false;
        packagesDelivered = 0;

        // Start music immediately when scene loads - never restarts
        if (musicController != null)
            musicController.StartShiftMusic();
    }

    void Update()
    {
        if (!timerRunning) return;

        shiftTimer -= Time.deltaTime;

        timerText.text = $"Time Left: {Mathf.Round(shiftTimer)}s";
        packageText.text = $"Deliveries: {packagesDelivered}/{packagesRequired}";

        if (shiftTimer <= 0)
            EndShift(false);
    }

    // Called by WarehouseManager once secure packages is clicked
    public void StartShiftTimer()
    {
        timerRunning = true;
        packagesDelivered = 0;
        totalPenalties = 0f;

        if (packageText != null)
            packageText.text = $"Deliveries: 0/{packagesRequired}";
        // No music call here - already playing from Start()
    }

    public void RegisterDelivery()
    {
        packagesDelivered++;
        Debug.Log($"Delivery registered: {packagesDelivered}/{packagesRequired}");

        if (packageText != null)
            packageText.text = $"Deliveries: {packagesDelivered}/{packagesRequired}";
    }

    public void CheckDelivery()
    {
        if (packagesDelivered >= packagesRequired)
            EndShift(true);
        else
            Debug.Log($"Not enough deliveries! {packagesDelivered}/{packagesRequired}");
    }

    void EndShift(bool success)
    {
        timerRunning = false;

        ShiftScoring scoring = FindObjectOfType<ShiftScoring>();
        if (scoring != null)
            scoring.ShowEndOfShiftReport(success);

        if (musicController != null)
        {
            if (success) musicController.PlaySuccessMusic();
            else musicController.PlayFailureMusic();
        }

        Debug.Log(success ? "Shift Complete! Career XP Gained." : "Shift Failed. Try again.");
        Time.timeScale = 0;
    }

    void OnDestroy()
    {
        if (musicController != null)
            musicController.StopMusic();
    }
}
