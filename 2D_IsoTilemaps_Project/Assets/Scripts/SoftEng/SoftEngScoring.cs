using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Scoring system for Software Engineer minigame
/// Rates performance based on time taken and number of attempts
/// </summary>
public class SoftwareEngScoring : MonoBehaviour
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
    public TextMeshProUGUI timeText; // "Time: 45s"
    public TextMeshProUGUI attemptsText; // "Attempts: 3"
    public TextMeshProUGUI commandsText; // "Commands: 12"
    public TextMeshProUGUI finalScoreText; // "Score: 4/5 Stars"
    public TextMeshProUGUI statusText; // "LEVELS COMPLETE"
    public TextMeshProUGUI ratingText; // "Excellent Work"

    [Header("Text Colors")]
    public Color statusSuccessColor = new Color(0.4f, 1f, 0.4f); // Light green
    public Color timeTextColor = Color.white;
    public Color attemptsTextColor = Color.white;
    public Color commandsTextColor = Color.white;
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
    [Tooltip("Time thresholds in seconds")]
    public float fiveStarTimeThreshold = 30f; // Complete in under 30s
    public float fourStarTimeThreshold = 60f; // Complete in under 60s
    public float threeStarTimeThreshold = 90f; // Complete in under 90s
    public float twoStarTimeThreshold = 120f; // Complete in under 120s

    [Tooltip("Attempt thresholds (number of times Run was pressed)")]
    public int fiveStarAttemptThreshold = 1; // 1 run (perfect first try)
    public int fourStarAttemptThreshold = 2; // 2 runs
    public int threeStarAttemptThreshold = 3; // 3 runs
    public int twoStarAttemptThreshold = 5; // 5 runs

    private int currentSceneIndex;
    private float totalTime = 0f;
    private int totalAttempts = 0;
    private int commandsUsed = 0;
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

    // Call this when player restarts a level (from bug collision or manual reset)
    public void IncrementAttempts()
    {
        totalAttempts++;
        Debug.Log($"Attempts: {totalAttempts}");
    }

    // Call this when all levels are complete
    public void ShowEndGameReport(int finalCommandCount)
    {
        if (scorePanel == null) return;

        isTracking = false; // Stop tracking time
        commandsUsed = finalCommandCount;
        scorePanel.SetActive(true);

        // Pause the game
        Time.timeScale = 0f;

        int stars = CalculateStars(totalTime, totalAttempts, commandsUsed);
        UpdateStarDisplay(stars);

        if (statusText != null)
        {
            statusText.text = "== LEVEL COMPLETE ==";
            statusText.color = statusSuccessColor;
        }

        if (timeText != null)
        {
            int timeSeconds = Mathf.RoundToInt(totalTime);
            int minutes = timeSeconds / 60;
            int seconds = timeSeconds % 60;
            timeText.text = $"Total Time\n<size=80><b>{minutes}:{seconds:D2}</b></size>";
            timeText.color = timeTextColor;
        }

        if (attemptsText != null)
        {
            attemptsText.text = $"Runs\n<size=80><b>{totalAttempts}</b></size>";
            attemptsText.color = attemptsTextColor;
        }

        if (commandsText != null)
        {
            commandsText.text = $"Commands Used\n<size=80><b>{commandsUsed}</b></size>";
            commandsText.color = commandsTextColor;
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

    private int CalculateStars(float timeTaken, int attempts, int commands)
    {
        // Calculate stars for each factor independently
        int timeStars = 5;
        int attemptStars = 5;

        // Time-based stars
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

        // Attempt-based stars (based on number of runs)
        if (attempts > twoStarAttemptThreshold)
        {
            attemptStars = 1;
        }
        else if (attempts > threeStarAttemptThreshold)
        {
            attemptStars = 2;
        }
        else if (attempts > fourStarAttemptThreshold)
        {
            attemptStars = 3;
        }
        else if (attempts > fiveStarAttemptThreshold)
        {
            attemptStars = 4;
        }
        else if (attempts == fiveStarAttemptThreshold)
        {
            attemptStars = 5; // Perfect - completed on first run
        }

        // Take the minimum of time and attempts only (commands is display-only)
        int finalStars = Mathf.Min(timeStars, attemptStars);

        Debug.Log($"Stars breakdown - Time: {timeStars}, Attempts: {attemptStars}, Commands: {commands} (display only), Final: {finalStars}");

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
        totalAttempts = 0;
        commandsUsed = 0;
        isTracking = true;
    }
}

// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.SceneManagement;

// /// <summary>
// /// Scoring system for Software Engineer minigame
// /// Rates performance based on time taken and number of attempts
// /// </summary>
// public class SoftwareEngScoring : MonoBehaviour
// {
//     [Header("UI References")]
//     public GameObject scorePanel; // Panel to show at end

//     [Header("Buttons")]
//     public Button exitButton; // Exit to overworld
//     public Button retryButton; // Retry minigame

//     [Header("Scene Settings")]
//     public int overworldSceneIndex = 1; // Scene index for overworld
//     public string overworldSceneName = ""; // Or use scene name instead

//     [Header("Star Images (Assign 5 Image GameObjects)")]
//     public Image[] starImages = new Image[5]; // Drag 5 UI Image objects here
//     public Sprite filledStarSprite; // Sprite for filled star
//     public Sprite emptyStarSprite; // Sprite for empty star

//     [Header("Text Elements")]
//     public TextMeshProUGUI timeText; // "Time: 45s"
//     public TextMeshProUGUI attemptsText; // "Attempts: 3"
//     public TextMeshProUGUI commandsText; // "Commands: 12"
//     public TextMeshProUGUI finalScoreText; // "Score: 4/5 Stars"
//     public TextMeshProUGUI statusText; // "LEVELS COMPLETE"
//     public TextMeshProUGUI ratingText; // "Excellent Work"

//     [Header("Text Colors")]
//     public Color statusSuccessColor = new Color(0.4f, 1f, 0.4f); // Light green
//     public Color timeTextColor = Color.white;
//     public Color attemptsTextColor = Color.white;
//     public Color commandsTextColor = Color.white;
//     public Color scoreTextColor = Color.white;
//     public Color ratingTextColor = new Color(1f, 0.84f, 0f); // Gold

//     [Header("Star Colors")]
//     public Color fiveStarColor = new Color(1f, 0.84f, 0f); // Gold
//     public Color fourStarColor = new Color(0.75f, 0.75f, 0.75f); // Silver
//     public Color threeStarColor = new Color(0.8f, 0.5f, 0.2f); // Bronze
//     public Color twoStarColor = new Color(1f, 0.5f, 0f); // Orange
//     public Color oneStarColor = Color.red;
//     public Color zeroStarColor = Color.gray;
//     public Color emptyStarColor = new Color(0.3f, 0.3f, 0.3f); // Dark gray

//     [Header("Scoring Thresholds")]
//     [Tooltip("Time thresholds in seconds")]
//     public float fiveStarTimeThreshold = 30f; // Complete in under 30s
//     public float fourStarTimeThreshold = 60f; // Complete in under 60s
//     public float threeStarTimeThreshold = 90f; // Complete in under 90s
//     public float twoStarTimeThreshold = 120f; // Complete in under 120s

//     [Tooltip("Attempt thresholds (number of restarts)")]
//     public int fiveStarAttemptThreshold = 0; // No restarts
//     public int fourStarAttemptThreshold = 1; // 1 restart
//     public int threeStarAttemptThreshold = 3; // 3 restarts
//     public int twoStarAttemptThreshold = 5; // 5 restarts

//     private int currentSceneIndex;
//     private float totalTime = 0f;
//     private int totalAttempts = 0;
//     private int commandsUsed = 0;
//     private bool isTracking = true;

//     void Start()
//     {
//         currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

//         if (scorePanel != null)
//         {
//             scorePanel.SetActive(false);
//         }

//         // Connect buttons
//         if (exitButton != null)
//         {
//             exitButton.onClick.AddListener(OnExitClicked);
//         }

//         if (retryButton != null)
//         {
//             retryButton.onClick.AddListener(OnRetryClicked);
//         }
//     }

//     void Update()
//     {
//         // Track time while playing
//         if (isTracking && Time.timeScale > 0)
//         {
//             totalTime += Time.deltaTime;
//         }
//     }

//     // Call this when player restarts a level (from bug collision or manual reset)
//     public void IncrementAttempts()
//     {
//         totalAttempts++;
//         Debug.Log($"Attempts: {totalAttempts}");
//     }

//     // Call this when all levels are complete
//     public void ShowEndGameReport(int finalCommandCount)
//     {
//         if (scorePanel == null) return;

//         isTracking = false; // Stop tracking time
//         commandsUsed = finalCommandCount;
//         scorePanel.SetActive(true);

//         // Pause the game
//         Time.timeScale = 0f;

//         int stars = CalculateStars(totalTime, totalAttempts, commandsUsed);
//         UpdateStarDisplay(stars);

//         if (statusText != null)
//         {
//             statusText.text = "== LEVEL COMPLETE ==";
//             statusText.color = statusSuccessColor;
//         }

//         if (timeText != null)
//         {
//             int timeSeconds = Mathf.RoundToInt(totalTime);
//             int minutes = timeSeconds / 60;
//             int seconds = timeSeconds % 60;
//             timeText.text = $"Total Time\n<size=80><b>{minutes}:{seconds:D2}</b></size>";
//             timeText.color = timeTextColor;
//         }

//         if (attemptsText != null)
//         {
//             attemptsText.text = $"Restarts\n<size=80><b>{totalAttempts}</b></size>";
//             attemptsText.color = attemptsTextColor;
//         }

//         if (commandsText != null)
//         {
//             commandsText.text = $"Commands Used\n<size=80><b>{commandsUsed}</b></size>";
//             commandsText.color = commandsTextColor;
//         }

//         if (finalScoreText != null)
//         {
//             finalScoreText.text = $"PERFORMANCE RATING\n<size=90><b>{stars}</b></size> <size=60>/ 5</size>";
//             finalScoreText.color = scoreTextColor;
//         }

//         if (ratingText != null)
//         {
//             ratingText.text = GetRatingText(stars);
//             ratingText.color = ratingTextColor;
//         }
//     }

//     private void UpdateStarDisplay(int stars)
//     {
//         if (starImages == null || starImages.Length != 5) return;

//         Color starColor = GetStarColor(stars);

//         for (int i = 0; i < 5; i++)
//         {
//             if (starImages[i] != null)
//             {
//                 if (i < stars)
//                 {
//                     starImages[i].sprite = filledStarSprite;
//                     starImages[i].color = starColor;
//                 }
//                 else
//                 {
//                     starImages[i].sprite = emptyStarSprite;
//                     starImages[i].color = emptyStarColor;
//                 }
//             }
//         }
//     }

//     private int CalculateStars(float timeTaken, int attempts, int commands)
//     {
//         // Calculate stars for each factor independently
//         int timeStars = 5;
//         int attemptStars = 5;

//         // Time-based stars
//         if (timeTaken > twoStarTimeThreshold)
//         {
//             timeStars = 1;
//         }
//         else if (timeTaken > threeStarTimeThreshold)
//         {
//             timeStars = 2;
//         }
//         else if (timeTaken > fourStarTimeThreshold)
//         {
//             timeStars = 3;
//         }
//         else if (timeTaken > fiveStarTimeThreshold)
//         {
//             timeStars = 4;
//         }

//         // Attempt-based stars
//         if (attempts > twoStarAttemptThreshold)
//         {
//             attemptStars = 1;
//         }
//         else if (attempts > threeStarAttemptThreshold)
//         {
//             attemptStars = 2;
//         }
//         else if (attempts > fourStarAttemptThreshold)
//         {
//             attemptStars = 3;
//         }
//         else if (attempts > fiveStarAttemptThreshold)
//         {
//             attemptStars = 4;
//         }

//         // Take the minimum of time and attempts only (commands is display-only)
//         int finalStars = Mathf.Min(timeStars, attemptStars);

//         Debug.Log($"Stars breakdown - Time: {timeStars}, Attempts: {attemptStars}, Commands: {commands} (display only), Final: {finalStars}");

//         return Mathf.Clamp(finalStars, 0, 5);
//     }

//     private string GetRatingText(int stars)
//     {
//         switch (stars)
//         {
//             case 5: return "OUTSTANDING!";
//             case 4: return "Excellent Work";
//             case 3: return "Good Job";
//             case 2: return "Needs Improvement";
//             case 1: return "Barely Passing";
//             default: return "Failed";
//         }
//     }

//     private Color GetStarColor(int stars)
//     {
//         switch (stars)
//         {
//             case 5: return fiveStarColor;
//             case 4: return fourStarColor;
//             case 3: return threeStarColor;
//             case 2: return twoStarColor;
//             case 1: return oneStarColor;
//             default: return zeroStarColor;
//         }
//     }

//     // Button click handlers
//     void OnExitClicked()
//     {
//         // Unpause the game
//         Time.timeScale = 1f;

//         // Load overworld
//         if (!string.IsNullOrEmpty(overworldSceneName))
//         {
//             SceneManager.LoadScene(overworldSceneName);
//         }
//         else
//         {
//             SceneManager.LoadScene(overworldSceneIndex);
//         }
//     }

//     void OnRetryClicked()
//     {
//         // Unpause the game
//         Time.timeScale = 1f;

//         // Reload current minigame scene
//         SceneManager.LoadScene(currentSceneIndex);
//     }

//     // Public method to reset tracking (if you want to restart without reloading scene)
//     public void ResetTracking()
//     {
//         totalTime = 0f;
//         totalAttempts = 0;
//         commandsUsed = 0;
//         isTracking = true;
//     }
// }