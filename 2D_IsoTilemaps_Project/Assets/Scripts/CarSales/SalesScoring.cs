using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Scoring system for Car Salesman minigame
/// Rates performance based on money earned vs max possible and successful sales count
/// </summary>

/// <summary>
/// Scoring system for Car Salesman minigame
/// Rates performance based on money earned vs max possible and successful sales count
/// </summary>
public class CarSalesScoring : MonoBehaviour
{
    [Header("UI References")]
    public GameObject scorePanel; // Panel to show at end

    [Header("Buttons")]
    public Button exitButton; // Exit to overworld
    public Button retryButton; // Retry minigame

    [Header("Scene Settings")]
    public int overworldSceneIndex = 1; // Scene index for overworld
    public string overworldSceneName = ""; // Or use scene name instead

    [Header("Star Images (Assign 5 Image GameObjects)")]
    public Image[] starImages = new Image[5]; // Drag 5 UI Image objects here
    public Sprite filledStarSprite; // Sprite for filled star
    public Sprite emptyStarSprite; // Sprite for empty star

    [Header("Text Elements")]
    public TextMeshProUGUI moneyText; // "Money Earned: $15,000 / $20,000"
    public TextMeshProUGUI salesText; // "Successful Sales: 2/3"
    public TextMeshProUGUI timeText; // "Time: 2:45"
    public TextMeshProUGUI finalScoreText; // "Score: 4/5 Stars"
    public TextMeshProUGUI statusText; // "SHIFT COMPLETE"
    public TextMeshProUGUI ratingText; // "Excellent Work"

    [Header("Text Colors")]
    public Color statusSuccessColor = new Color(0.4f, 1f, 0.4f); // Light green

    [Header("Money Text Colors")]
    public Color moneyExcellentColor = new Color(0.4f, 1f, 0.4f); // Green (90%+)
    public Color moneyGoodColor = new Color(0.7f, 1f, 0.4f); // Yellow-green (75-90%)
    public Color moneyAverageColor = new Color(1f, 1f, 0.4f); // Yellow (50-75%)
    public Color moneyPoorColor = new Color(1f, 0.6f, 0.2f); // Orange (30-50%)
    public Color moneyFailColor = new Color(1f, 0.4f, 0.4f); // Red (<30%)

    [Header("Sales Text Colors")]
    public Color salesPerfectColor = new Color(0.4f, 1f, 0.4f); // Green (all sales)
    public Color salesGoodColor = new Color(1f, 1f, 0.4f); // Yellow (2/3)
    public Color salesPoorColor = new Color(1f, 0.6f, 0.2f); // Orange (1/3)
    public Color salesFailColor = new Color(1f, 0.4f, 0.4f); // Red (0/3)

    [Header("Time Text Colors")]
    public Color timeFastColor = new Color(0.4f, 1f, 0.4f); // Green (<2 min)
    public Color timeGoodColor = new Color(0.7f, 1f, 0.4f); // Yellow-green (2-3 min)
    public Color timeAverageColor = new Color(1f, 1f, 0.4f); // Yellow (3-4 min)
    public Color timeSlowColor = new Color(1f, 0.6f, 0.2f); // Orange (4-5 min)
    public Color timeVerySlowColor = new Color(1f, 0.4f, 0.4f); // Red (>5 min)
    public Color scoreTextColor = Color.white;
    public Color ratingTextColor = new Color(1f, 0.84f, 0f); // Gold

    [Header("Star Colors")]
    public Color fiveStarColor = new Color(1f, 0.84f, 0f); // Gold
    public Color fourStarColor = new Color(0.75f, 0.75f, 0.75f); // Silver
    public Color threeStarColor = new Color(0.8f, 0.5f, 0.2f); // Bronze
    public Color twoStarColor = new Color(1f, 0.5f, 0f); // Orange
    public Color oneStarColor = Color.red;
    public Color zeroStarColor = Color.gray;
    public Color emptyStarColor = new Color(0.3f, 0.3f, 0.3f); // Dark gray

    [Header("Scoring Thresholds")]
    [Tooltip("Money earned percentage thresholds (0-1 range)")]
    public float fiveStarMoneyThreshold = 0.90f; // 90%+ of max possible
    public float fourStarMoneyThreshold = 0.75f; // 75%+ of max possible
    public float threeStarMoneyThreshold = 0.50f; // 50%+ of max possible
    public float twoStarMoneyThreshold = 0.30f; // 30%+ of max possible

    [Tooltip("Time thresholds in seconds")]
    public float fiveStarTimeThreshold = 120f; // Under 2 minutes
    public float fourStarTimeThreshold = 180f; // Under 3 minutes
    public float threeStarTimeThreshold = 240f; // Under 4 minutes
    public float twoStarTimeThreshold = 300f; // Under 5 minutes

    private int currentSceneIndex;
    private float totalTime = 0f;
    private bool isTracking = true;

    void Start()
    {
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (scorePanel != null)
        {
            scorePanel.SetActive(false);
        }

        // Connect buttons
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitClicked);
        }

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryClicked);
        }
    }

    void Update()
    {
        // Track time while playing
        if (isTracking && Time.timeScale > 0)
        {
            totalTime += Time.deltaTime;
        }
    }

    /// <summary>
    /// Call this when all rounds are complete
    /// </summary>
    /// <param name="moneyEarned">Total profit earned</param>
    /// <param name="maxPossibleMoney">Maximum possible profit</param>
    /// <param name="successfulSales">Number of successful sales</param>
    /// <param name="totalRounds">Total number of sales rounds</param>
    public void ShowEndGameReport(int moneyEarned, int maxPossibleMoney, int successfulSales, int totalRounds)
    {
        if (scorePanel == null) return;

        isTracking = false; // Stop tracking time
        scorePanel.SetActive(true);

        // Force panel to front
        scorePanel.transform.SetAsLastSibling();

        // Make sure Canvas is set correctly
        Canvas canvas = scorePanel.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = 100; // High number to render on top

            // For Screen Space - Camera mode, ensure camera is assigned
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
            {
                canvas.worldCamera = Camera.main;
                Debug.Log("CarSalesScoring: Assigned Main Camera to Canvas");
            }
        }

        // Pause the game
        Time.timeScale = 0f;

        // Calculate money ratio
        float moneyRatio = maxPossibleMoney > 0 ? (float)moneyEarned / maxPossibleMoney : 0f;

        int stars = CalculateStars(moneyRatio, totalTime, successfulSales, totalRounds);
        UpdateStarDisplay(stars);

        if (statusText != null)
        {
            statusText.text = "== SHIFT COMPLETE ==";
            statusText.color = statusSuccessColor;
        }

        if (moneyText != null)
        {
            int percentage = Mathf.RoundToInt(moneyRatio * 100f);
            moneyText.text = $"Money Earned\n<size=60><b>${moneyEarned:N0}</b> / ${maxPossibleMoney:N0}</size>\n<size=50>({percentage}%)</size>";
            moneyText.color = GetMoneyColor(moneyRatio);
        }

        if (salesText != null)
        {
            salesText.text = $"Successful Sales\n<size=80><b>{successfulSales}</b></size> <size=50>/ {totalRounds}</size>";
            salesText.color = GetSalesColor(successfulSales, totalRounds);
        }

        if (timeText != null)
        {
            int timeSeconds = Mathf.RoundToInt(totalTime);
            int minutes = timeSeconds / 60;
            int seconds = timeSeconds % 60;
            timeText.text = $"Total Time\n<size=80><b>{minutes}:{seconds:D2}</b></size>";
            timeText.color = GetTimeColor(totalTime);
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

    private int CalculateStars(float moneyRatio, float timeTaken, int salesSucceeded, int totalRounds)
    {
        // Calculate stars for each factor independently
        int moneyStars = 5;
        int timeStars = 5;

        // Money ratio-based stars (primary factor)
        if (moneyRatio < twoStarMoneyThreshold)
        {
            moneyStars = 1;
        }
        else if (moneyRatio < threeStarMoneyThreshold)
        {
            moneyStars = 2;
        }
        else if (moneyRatio < fourStarMoneyThreshold)
        {
            moneyStars = 3;
        }
        else if (moneyRatio < fiveStarMoneyThreshold)
        {
            moneyStars = 4;
        }

        // Time-based stars (secondary factor)
        if (timeTaken > twoStarTimeThreshold)
        {
            timeStars = 1;
        }
        else if (timeTaken > threeStarTimeThreshold)
        {
            timeStars = 2;
        }
        else if (timeTaken > fourStarTimeThreshold)
        {
            timeStars = 3;
        }
        else if (timeTaken > fiveStarTimeThreshold)
        {
            timeStars = 4;
        }

        // Penalty: If no sales were made, cap at 1 star
        if (salesSucceeded == 0)
        {
            moneyStars = Mathf.Min(moneyStars, 1);
        }

        // Penalty: If only 1 sale out of 3, cap at 3 stars
        if (totalRounds == 3 && salesSucceeded == 1)
        {
            moneyStars = Mathf.Min(moneyStars, 3);
        }

        // Take the minimum of money and time factors
        int finalStars = Mathf.Min(moneyStars, timeStars);

        Debug.Log($"Stars breakdown - Money ratio: {moneyRatio:P0} ({moneyStars}⭐), Time: {timeTaken:F0}s ({timeStars}⭐), Sales: {salesSucceeded}/{totalRounds}, Final: {finalStars}⭐");

        return Mathf.Clamp(finalStars, 0, 5);
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

    private Color GetMoneyColor(float ratio)
    {
        if (ratio >= fiveStarMoneyThreshold) return moneyExcellentColor; // 90%+
        if (ratio >= fourStarMoneyThreshold) return moneyGoodColor;      // 75-90%
        if (ratio >= threeStarMoneyThreshold) return moneyAverageColor;  // 50-75%
        if (ratio >= twoStarMoneyThreshold) return moneyPoorColor;       // 30-50%
        return moneyFailColor;                                            // <30%
    }

    private Color GetSalesColor(int sales, int total)
    {
        if (sales == total) return salesPerfectColor;     // All sales
        if (total == 3 && sales == 2) return salesGoodColor; // 2/3
        if (sales > 0) return salesPoorColor;             // Some sales
        return salesFailColor;                            // No sales
    }

    private Color GetTimeColor(float seconds)
    {
        if (seconds <= fiveStarTimeThreshold) return timeFastColor;       // <2 min
        if (seconds <= fourStarTimeThreshold) return timeGoodColor;       // 2-3 min
        if (seconds <= threeStarTimeThreshold) return timeAverageColor;   // 3-4 min
        if (seconds <= twoStarTimeThreshold) return timeSlowColor;        // 4-5 min
        return timeVerySlowColor;                                         // >5 min
    }

    // Button click handlers
    void OnExitClicked()
    {
        // Unpause the game
        Time.timeScale = 1f;

        // Load overworld
        if (!string.IsNullOrEmpty(overworldSceneName))
        {
            SceneManager.LoadScene(overworldSceneName);
        }
        else
        {
            SceneManager.LoadScene(overworldSceneIndex);
        }
    }

    void OnRetryClicked()
    {
        // Unpause the game
        Time.timeScale = 1f;

        // Reload current minigame scene
        SceneManager.LoadScene(currentSceneIndex);
    }

    // Public method to reset tracking (if you want to restart without reloading scene)
    public void ResetTracking()
    {
        totalTime = 0f;
        isTracking = true;
    }
}