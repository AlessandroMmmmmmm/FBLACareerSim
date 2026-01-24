using TMPro;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public int packagesRequired = 3;
    public int currentPackagesInTruck = 0;
    public float shiftTimer = 120f;
    public TextMeshProUGUI statusUI;

    void Update()
    {
        shiftTimer -= Time.deltaTime;
        statusUI.text = $"Packages: {currentPackagesInTruck}/{packagesRequired} | Time: {Mathf.Round(shiftTimer)}s";

        if (shiftTimer <= 0) EndShift(false);
    }

    public void CheckDelivery()
    {
        if (currentPackagesInTruck >= packagesRequired) EndShift(true);
        else Debug.Log("Not enough packages!");
    }

    void EndShift(bool success)
    {
        Debug.Log(success ? "Shift Complete! Career XP Gained." : "Shift Failed. Try again.");
        Time.timeScale = 0; // Pause game
    }
}
