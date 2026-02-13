using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Detects when truck drives on grass or buildings and subtracts time from countdown timer
/// Attach this to grass tiles and buildings
/// </summary>
public class OffRoadPenalty : MonoBehaviour
{
    [Header("Penalty Settings")]
    public float penaltyPerSecond = 2f; // Seconds subtracted from timer per second off-road
    public PenaltyType penaltyType = PenaltyType.Grass;

    [Header("Events")]
    public UnityEvent<float> onPenaltyApplied;

    private bool playerOnPenaltyZone = false;
    private float penaltyAccumulator = 0f;
    private DeliveryManager deliveryManager;

    public enum PenaltyType
    {
        Grass,
        Building
    }

    private void Start()
    {
        // Find the DeliveryManager to directly modify shiftTimer
        deliveryManager = FindObjectOfType<DeliveryManager>();

        if (deliveryManager == null)
        {
            Debug.LogWarning("OffRoadPenalty: Could not find DeliveryManager!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnPenaltyZone = true;

            string zoneName = penaltyType == PenaltyType.Grass ? "grass" : "building";
            Debug.LogWarning($"Driving on {zoneName}! Time penalty applied!");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && playerOnPenaltyZone)
        {
            // Accumulate penalty while on penalty zone
            float penalty = penaltyPerSecond * Time.deltaTime;
            penaltyAccumulator += penalty;

            // Subtract time directly from DeliveryManager
            if (deliveryManager != null)
            {
                deliveryManager.shiftTimer -= penalty;
                deliveryManager.totalPenalties += penalty; // Track total for scoring

                // Clamp to 0 minimum
                if (deliveryManager.shiftTimer < 0f)
                {
                    deliveryManager.shiftTimer = 0f;
                }

                // Make timer text red during penalty
                if (deliveryManager.timerText != null)
                {
                    deliveryManager.timerText.color = Color.red;
                }
            }

            onPenaltyApplied?.Invoke(penalty);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnPenaltyZone = false;

            // Reset timer color to white when back on road
            if (deliveryManager != null && deliveryManager.timerText != null)
            {
                deliveryManager.timerText.color = Color.white;
            }

            if (penaltyAccumulator > 0.1f)
            {
                string zoneName = penaltyType == PenaltyType.Grass ? "grass" : "building";
                Debug.Log($"Left {zoneName}. Total time lost: -{penaltyAccumulator:F1} seconds");
                penaltyAccumulator = 0f;
            }
        }
    }
}
