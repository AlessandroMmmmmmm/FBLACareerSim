using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Detects when truck drives on grass or buildings and subtracts time from countdown timer
/// Attach this to grass tiles and buildings
/// </summary>
public class OffRoadPenalty : MonoBehaviour
{
    [Header("Penalty Settings")]
    public float penaltyPerSecond = 2f;
    public PenaltyType penaltyType = PenaltyType.Grass;

    [Header("Events")]
    public UnityEvent<float> onPenaltyApplied;

    [Header("Audio")]
    public AudioClip penaltySound; // Off-road warning sound

    private bool playerOnPenaltyZone = false;
    private float penaltyAccumulator = 0f;
    private DeliveryManager deliveryManager;
    private AudioSource audioSource;
    private bool soundPlaying = false;

    public enum PenaltyType
    {
        Grass,
        Building
    }

    private void Start()
    {
        deliveryManager = FindObjectOfType<DeliveryManager>();

        if (deliveryManager == null)
            Debug.LogWarning("OffRoadPenalty: Could not find DeliveryManager!");

        // Create audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnPenaltyZone = true;

            // Play penalty sound once when entering
            if (penaltySound != null && audioSource != null && !soundPlaying)
            {
                audioSource.PlayOneShot(penaltySound, 0.5f);
                soundPlaying = true;
            }

            string zoneName = penaltyType == PenaltyType.Grass ? "grass" : "building";
            Debug.LogWarning($"Driving on {zoneName}! Time penalty applied!");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && playerOnPenaltyZone)
        {
            float penalty = penaltyPerSecond * Time.deltaTime;
            penaltyAccumulator += penalty;

            if (deliveryManager != null)
            {
                deliveryManager.shiftTimer -= penalty;
                deliveryManager.totalPenalties += penalty;

                if (deliveryManager.shiftTimer < 0f)
                    deliveryManager.shiftTimer = 0f;

                if (deliveryManager.timerText != null)
                    deliveryManager.timerText.color = Color.red;
            }

            onPenaltyApplied?.Invoke(penalty);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnPenaltyZone = false;
            soundPlaying = false;

            if (deliveryManager != null && deliveryManager.timerText != null)
                deliveryManager.timerText.color = Color.white;

            if (penaltyAccumulator > 0.1f)
            {
                string zoneName = penaltyType == PenaltyType.Grass ? "grass" : "building";
                Debug.Log($"Left {zoneName}. Total time lost: -{penaltyAccumulator:F1} seconds");
                penaltyAccumulator = 0f;
            }
        }
    }
}
