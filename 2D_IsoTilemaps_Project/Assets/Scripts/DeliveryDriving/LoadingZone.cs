using UnityEngine;

public class LoadingZone : MonoBehaviour
{
    public WarehouseManager warehouseManager;
    public DeliveryManager deliveryManager;
    [Header("Startup Objective")]
    public GameObject globalObjectiveText; // Drag 'GlobalObjectiveText' here
    private bool objectiveCompleted = false;
    public bool isTruckInZone = false;
    public GameObject sharedHUD;
    //
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            if (!objectiveCompleted)
            {
                if (globalObjectiveText != null)
                {
                    // Hard Fix: Physically delete the object from the game
                    Destroy(globalObjectiveText);
                    Debug.Log("Objective Destroyed Permanently.");
                }
                objectiveCompleted = true;
            }
            // 1. Show the Timer group immediately
            if (sharedHUD != null) sharedHUD.SetActive(true);

            // 2. Start the clock logic
            if (deliveryManager != null) deliveryManager.StartShiftTimer();

            // 3. Trigger warehouse instructions
            if (warehouseManager != null) warehouseManager.SetInLoadingZone(true);
        }
    }



    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isTruckInZone = false;
            if (warehouseManager != null)
                warehouseManager.SetInLoadingZone(false);
        }
    }
}
