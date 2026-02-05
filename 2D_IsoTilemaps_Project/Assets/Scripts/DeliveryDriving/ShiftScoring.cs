using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Calculates and displays end-of-shift performance rating with real star images
/// All colors are fully customizable in the Inspector!
/// </summary>
public class ShiftScoring : MonoBehaviour
{
    [Header("UI References")]
    public GameObject scorePanel; // Panel to show at end of shift

    [Header("Star Images (Assign 5 Image GameObjects)")]
    public Image[] starImages = new Image[5]; // Drag 5 UI Image objects here
    public Sprite filledStarSprite; // Sprite for filled star
    public Sprite emptyStarSprite; // Sprite for empty star

    [Header("Text Elements")]
    public TextMeshProUGUI packagesText; // "Packages: 3/3"
    public TextMeshProUGUI timeText; // "Time Remaining: 45s"
    public TextMeshProUGUI penaltyText; // "Penalties: -12s"
    public TextMeshProUGUI finalScoreText; // "Score: 4/5 Stars"
    public TextMeshProUGUI statusText; // "SHIFT COMPLETE" / "SHIFT FAILED"
    public TextMeshProUGUI ratingText; // "Excellent Work"

    [Header("Text Colors - Customize Here!")]
    public Color statusSuccessColor = new Color(0.4f, 1f, 0.4f); // Light green
    public Color statusFailColor = new Color(1f, 0.4f, 0.4f); // Light red
    public Color packagesTextColor = Color.white;
    public Color timeGoodColor = new Color(0.4f, 1f, 0.4f); // Green
    public Color timeWarningColor = new Color(1f, 1f, 0.4f); // Yellow
    public Color timeCriticalColor = new Color(1f, 0.4f, 0.4f); // Red
    public Color penaltyTextColor = new Color(1f, 0.4f, 0.4f); // Red
    public Color noPenaltyColor = new Color(0.4f, 1f, 0.4f); // Green
    public Color scoreTextColor = Color.white;
    public Color ratingTextColor = new Color(1f, 0.84f, 0f); // Gold

    [Header("Star Colors - Customize Here!")]
    public Color fiveStarColor = new Color(1f, 0.84f, 0f); // Gold
    public Color fourStarColor = new Color(0.75f, 0.75f, 0.75f); // Silver
    public Color threeStarColor = new Color(0.8f, 0.5f, 0.2f); // Bronze
    public Color twoStarColor = new Color(1f, 0.5f, 0f); // Orange
    public Color oneStarColor = Color.red;
    public Color zeroStarColor = Color.gray;
    public Color emptyStarColor = new Color(0.3f, 0.3f, 0.3f); // Dark gray for empty stars

    [Header("Scoring Thresholds")]
    public float fiveStarTimeThreshold = 60f;
    public float fourStarTimeThreshold = 40f;
    public float threeStarTimeThreshold = 20f;
    public float twoStarTimeThreshold = 10f;

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

        scorePanel.SetActive(true);

        int packagesDelivered = truck != null ? truck.packagesDelivered : 0;
        int packagesRequired = deliveryManager != null ? deliveryManager.packagesRequired : 3;
        float timeRemaining = deliveryManager != null ? deliveryManager.shiftTimer : 0f;

        int stars = CalculateStars(packagesDelivered, packagesRequired, timeRemaining, totalPenalties);

        UpdateStarDisplay(stars);

        if (statusText != null)
        {
            statusText.text = success ? "== SHIFT COMPLETE ==" : "== SHIFT FAILED ==";
            statusText.color = success ? statusSuccessColor : statusFailColor;
        }

        if (packagesText != null)
        {
            packagesText.text = $"Packages Delivered\n<size=80><b>{packagesDelivered}</b></size> <size=50>/ {packagesRequired}</size>";
            packagesText.color = packagesTextColor;
        }

        if (timeText != null)
        {
            int timeLeft = Mathf.Max(0, Mathf.RoundToInt(timeRemaining));
            Color timeColor = timeLeft > 30 ? timeGoodColor : (timeLeft > 10 ? timeWarningColor : timeCriticalColor);

            timeText.text = $"Time Remaining\n<size=80><b>{timeLeft}</b></size> <size=50>seconds</size>";
            timeText.color = timeColor;
        }

        if (penaltyText != null)
        {
            int penaltySeconds = Mathf.RoundToInt(totalPenalties);
            if (penaltySeconds > 0)
            {
                penaltyText.text = $"Off-Road Penalties\n<size=80><b>-{penaltySeconds}</b></size> <size=50>seconds</size>";
                penaltyText.color = penaltyTextColor;
            }
            else
            {
                penaltyText.text = $"Off-Road Penalties\n<size=60><b>PERFECT DRIVING!</b></size>";
                penaltyText.color = noPenaltyColor;
            }
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = $"PERFORMANCE RATING\n<size=90><b>{stars}</b></size> <size=60>/ 5</size>";
            finalScoreText.color = scoreTextColor;
        }

        if (ratingText != null)
        {
            ratingText.text = GetRatingText(stars);
            ratingText.color = ratingTextColor;
        }
    }

    private void UpdateStarDisplay(int stars)
    {
        if (starImages == null || starImages.Length != 5) return;

        Color starColor = GetStarColor(stars);

        for (int i = 0; i < 5; i++)
        {
            if (starImages[i] != null)
            {
                if (i < stars)
                {
                    starImages[i].sprite = filledStarSprite;
                    starImages[i].color = starColor;
                }
                else
                {
                    starImages[i].sprite = emptyStarSprite;
                    starImages[i].color = emptyStarColor;
                }
            }
        }
    }

    private int CalculateStars(int delivered, int required, float timeLeft, float penalties)
    {
        if (delivered < required) return 0;

        float effectiveTime = timeLeft;

        if (effectiveTime >= fiveStarTimeThreshold) return 5;
        else if (effectiveTime >= fourStarTimeThreshold) return 4;
        else if (effectiveTime >= threeStarTimeThreshold) return 3;
        else if (effectiveTime >= twoStarTimeThreshold) return 2;
        else if (effectiveTime > 0) return 1;
        else return 0;
    }

    private string GetRatingText(int stars)
    {
        switch (stars)
        {
            case 5: return "OUTSTANDING!";
            case 4: return "Excellent Work";
            case 3: return "Good Job";
            case 2: return "Needs Improvement";
            case 1: return "Barely Passing";
            default: return "Failed";
        }
    }

    private Color GetStarColor(int stars)
    {
        switch (stars)
        {
            case 5: return fiveStarColor;
            case 4: return fourStarColor;
            case 3: return threeStarColor;
            case 2: return twoStarColor;
            case 1: return oneStarColor;
            default: return zeroStarColor;
        }
    }
}
