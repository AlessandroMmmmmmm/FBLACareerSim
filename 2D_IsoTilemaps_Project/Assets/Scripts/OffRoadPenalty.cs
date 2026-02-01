using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Detects when truck drives on grass or buildings and applies time penalty
/// Attach this to grass tiles and buildings
/// </summary>
public class OffRoadPenalty : MonoBehaviour
{
    [Header("Penalty Settings")]
    public float penaltyPerSecond = 2f; // Seconds added to timer per second off-road
    public PenaltyType penaltyType = PenaltyType.Grass;
    
    [Header("Events")]
    public UnityEvent<float> onPenaltyApplied;
    
    private bool playerOnPenaltyZone = false;
    private float penaltyAccumulator = 0f;
    private DeliveryTimer deliveryTimer;
    
    public enum PenaltyType
    {
        Grass,
        Building
    }
    
    private void Start()
    {
        // Find the delivery timer
        deliveryTimer = FindObjectOfType<DeliveryTimer>();
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
            
            // Apply to timer if exists
            if (deliveryTimer != null)
            {
                deliveryTimer.AddPenalty(penalty);
            }
            
            onPenaltyApplied?.Invoke(penalty);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnPenaltyZone = false;
            
            if (penaltyAccumulator > 0.1f)
            {
                string zoneName = penaltyType == PenaltyType.Grass ? "grass" : "building";
                Debug.Log($"Left {zoneName}. Total penalty: +{penaltyAccumulator:F1} seconds");
                penaltyAccumulator = 0f;
            }
        }
    }
}
