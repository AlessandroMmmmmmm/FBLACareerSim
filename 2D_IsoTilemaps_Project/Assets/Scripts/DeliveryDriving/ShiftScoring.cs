using TMPro;
using UnityEngine;

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

        // Color based on performance
        Color starColor = GetStarColor(stars);
        string starColorHex = ColorUtility.ToHtmlStringRGB(starColor);

        // Update UI with professional formatting
        if (starsText != null)
        {
            starsText.text = $"<color=#{starColorHex}>{GetStarDisplay(stars)}</color>";
            starsText.fontSize = 100;
        }

        if (packagesText != null)
        {
            Color pkgColor = packagesDelivered >= packagesRequired ? Color.green : Color.red;
            string pkgColorHex = ColorUtility.ToHtmlStringRGB(pkgColor);

            packagesText.text = $"<size=40>Packages Delivered</size>\n" +
                               $"<size=80><color=#{pkgColorHex}><b>{packagesDelivered}</b></color> <size=50>/ {packagesRequired}</size></size>";
        }

        if (timeText != null)
        {
            int timeLeft = Mathf.Max(0, Mathf.RoundToInt(timeRemaining));
            string timeColor = timeLeft > 30 ? "66FF66" : (timeLeft > 10 ? "FFFF66" : "FF6666");

            timeText.text = $"<size=40>Time Remaining</size>\n" +
                           $"<size=80><color=#{timeColor}><b>{timeLeft}</b></color> <size=50>seconds</size></size>";
        }

        if (penaltyText != null)
        {
            int penaltySeconds = Mathf.RoundToInt(totalPenalties);
            if (penaltySeconds > 0)
            {
                penaltyText.text = $"<size=40>Off-Road Penalties</size>\n" +
                                  $"<size=80><color=#FF6666><b>-{penaltySeconds}</b></color> <size=50>seconds</size></size>";
            }
            else
            {
                penaltyText.text = $"<size=40>Off-Road Penalties</size>\n" +
                                  $"<size=60><color=#66FF66><b>PERFECT DRIVING!</b></color></size>";
            }
        }

        if (finalScoreText != null)
        {
            string statusText = success ?
                "<size=50><color=#66FF66>== SHIFT COMPLETE ==</color></size>" :
                "<size=50><color=#FF6666>== SHIFT FAILED ==</color></size>";

            string ratingText = GetRatingText(stars);

            finalScoreText.text = $"{statusText}\n\n" +
                                 $"<size=45>PERFORMANCE RATING</size>\n" +
                                 $"<size=90><color=#{starColorHex}><b>{stars}</b></color> <size=60>/ 5</size></size>\n" +
                                 $"<size=40><i>{ratingText}</i></size>";
        }
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
        // Use asterisks instead of unicode stars for better compatibility
        string fullStar = "*";
        string emptyStar = "-";

        string display = "";
        for (int i = 0; i < 5; i++)
        {
            if (i < stars)
            {
                display += fullStar + " ";
            }
            else
            {
                display += emptyStar + " ";
            }
        }

        return display.Trim();
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
