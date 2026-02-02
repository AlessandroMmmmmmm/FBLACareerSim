using UnityEngine;
using TMPro;

/// <summary>
/// Calculates and displays end-of-shift performance rating
/// Based on packages delivered, time remaining, and penalties
/// </summary>
public class ShiftScoring : MonoBehaviour
{
    [Header("UI References")]
    public GameObject scorePanel; // Panel to show at end of shift
    public TextMeshProUGUI starsText; // Shows "★★★★★" or "★★★☆☆"
    public TextMeshProUGUI packagesText; // "Packages: 3/3"
    public TextMeshProUGUI timeText; // "Time Remaining: 45s"
    public TextMeshProUGUI penaltyText; // "Penalties: -12s"
    public TextMeshProUGUI finalScoreText; // "Score: 4/5 Stars"
    
    [Header("Scoring Thresholds")]
    public float fiveStarTimeThreshold = 60f; // Need 60+ seconds for 5 stars
    public float fourStarTimeThreshold = 40f; // Need 40+ seconds for 4 stars
    public float threeStarTimeThreshold = 20f; // Need 20+ seconds for 3 stars
    public float twoStarTimeThreshold = 10f; // Need 10+ seconds for 2 stars
    
    private DeliveryManager deliveryManager;
    private ArcadeTruck truck;
    private float totalPenalties = 0f;
    
    void Start()
    {
        deliveryManager = FindObjectOfType<DeliveryManager>();
        truck = FindObjectOfType<ArcadeTruck>();
        
        if (scorePanel != null)
        {
            scorePanel.SetActive(false);
        }
        
        // Subscribe to penalty events
        OffRoadPenalty[] penalties = FindObjectsOfType<OffRoadPenalty>();
        foreach (var penalty in penalties)
        {
            penalty.onPenaltyApplied.AddListener(AddPenalty);
        }
    }
    
    public void AddPenalty(float amount)
    {
        totalPenalties += amount;
    }
    
    public void ShowEndOfShiftReport(bool success)
    {
        if (scorePanel == null) return;
        
        // Show the panel
        scorePanel.SetActive(true);
        
        // Get stats
        int packagesDelivered = truck != null ? truck.packagesDelivered : 0;
        int packagesRequired = deliveryManager != null ? deliveryManager.packagesRequired : 3;
        float timeRemaining = deliveryManager != null ? deliveryManager.shiftTimer : 0f;
        
        // Calculate star rating
        int stars = CalculateStars(packagesDelivered, packagesRequired, timeRemaining, totalPenalties);
        
        // Update UI
        if (starsText != null)
        {
            starsText.text = GetStarDisplay(stars);
            starsText.color = GetStarColor(stars);
        }
        
        if (packagesText != null)
        {
            packagesText.text = $"Packages Delivered: {packagesDelivered}/{packagesRequired}";
        }
        
        if (timeText != null)
        {
            timeText.text = $"Time Remaining: {Mathf.Max(0, Mathf.Round(timeRemaining))}s";
        }
        
        if (penaltyText != null)
        {
            penaltyText.text = $"Off-Road Penalties: -{Mathf.Round(totalPenalties)}s";
        }
        
        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Rating: {stars}/5 Stars";
        }
    }
    
    private int CalculateStars(int delivered, int required, float timeLeft, float penalties)
    {
        // Must deliver all packages to get any stars
        if (delivered < required)
        {
            return 0; // Failed - no stars
        }
        
        // All packages delivered - calculate stars based on time and penalties
        float effectiveTime = timeLeft; // Already has penalties subtracted by OffRoadPenalty
        
        // 5 stars: All packages + 60+ seconds remaining
        if (effectiveTime >= fiveStarTimeThreshold)
        {
            return 5;
        }
        // 4 stars: All packages + 40+ seconds
        else if (effectiveTime >= fourStarTimeThreshold)
        {
            return 4;
        }
        // 3 stars: All packages + 20+ seconds
        else if (effectiveTime >= threeStarTimeThreshold)
        {
            return 3;
        }
        // 2 stars: All packages + 10+ seconds
        else if (effectiveTime >= twoStarTimeThreshold)
        {
            return 2;
        }
        // 1 star: All packages but barely any time left
        else if (effectiveTime > 0)
        {
            return 1;
        }
        // 0 stars: Failed (time ran out)
        else
        {
            return 0;
        }
    }
    
    private string GetStarDisplay(int stars)
    {
        string fullStar = "★";
        string emptyStar = "☆";
        
        string display = "";
        for (int i = 0; i < 5; i++)
        {
            if (i < stars)
            {
                display += fullStar;
            }
            else
            {
                display += emptyStar;
            }
        }
        
        return display;
    }
    
    private Color GetStarColor(int stars)
    {
        switch (stars)
        {
            case 5: return new Color(1f, 0.84f, 0f); // Gold
            case 4: return new Color(0.75f, 0.75f, 0.75f); // Silver
            case 3: return new Color(0.8f, 0.5f, 0.2f); // Bronze
            case 2: return new Color(1f, 0.5f, 0f); // Orange
            case 1: return Color.red;
            default: return Color.gray;
        }
    }
}
