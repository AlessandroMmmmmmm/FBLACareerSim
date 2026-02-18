using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SoftwareEngScoring : MonoBehaviour
{
    [Header("UI References")]
    public GameObject scorePanel;

    [Header("Buttons")]
    public Button exitButton;
    public Button retryButton;

    [Header("Scene Settings")]
    public int overworldSceneIndex = 1;
    public string overworldSceneName = "";

    [Header("Star Images (Assign 5 Image GameObjects)")]
    public Image[] starImages = new Image[5];
    public Sprite filledStarSprite;
    public Sprite emptyStarSprite;

    [Header("Text Elements")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI attemptsText;
    public TextMeshProUGUI commandsText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI ratingText;

    [Header("Performance Color Gradients")]
    [Tooltip("Color for time - based on speed (green=fast, red=slow)")]
    public Color timeExcellent = new Color(0.4f, 1f, 0.4f);      // Green - very fast
    public Color timeGood = new Color(0.8f, 1f, 0.4f);           // Yellow-green
    public Color timeAverage = new Color(1f, 1f, 0.4f);          // Yellow
    public Color timePoor = new Color(1f, 0.6f, 0.2f);           // Orange
    public Color timeSlow = new Color(1f, 0.3f, 0.3f);           // Red

    [Tooltip("Color for attempts - based on efficiency (green=few, red=many)")]
    public Color attemptsExcellent = new Color(0.4f, 1f, 0.4f);  // Green - 1 run
    public Color attemptsGood = new Color(0.8f, 1f, 0.4f);       // Yellow-green - 2 runs
    public Color attemptsAverage = new Color(1f, 1f, 0.4f);      // Yellow - 3 runs
    public Color attemptsPoor = new Color(1f, 0.6f, 0.2f);       // Orange - 4-5 runs
    public Color attemptsMany = new Color(1f, 0.3f, 0.3f);       // Red - 6+ runs

    [Tooltip("Color for commands - based on efficiency (green=optimal, red=verbose)")]
    public Color commandsExcellent = new Color(0.4f, 1f, 0.4f);  // Green
    public Color commandsGood = new Color(0.8f, 1f, 0.4f);       // Yellow-green
    public Color commandsAverage = new Color(1f, 1f, 0.4f);      // Yellow
    public Color commandsVerbose = new Color(1f, 0.6f, 0.2f);    // Orange
    public Color commandsExcessive = new Color(1f, 0.3f, 0.3f);  // Red

    [Header("Star Colors")]
    public Color fiveStarColor = new Color(1f, 0.84f, 0f);
    public Color fourStarColor = new Color(0.75f, 0.75f, 0.75f);
    public Color threeStarColor = new Color(0.8f, 0.5f, 0.2f);
    public Color twoStarColor = new Color(1f, 0.5f, 0f);
    public Color oneStarColor = Color.red;
    public Color zeroStarColor = Color.gray;
    public Color emptyStarColor = new Color(0.3f, 0.3f, 0.3f);

    [Header("Scoring Thresholds")]
    [Tooltip("Time thresholds in seconds")]
    public float fiveStarTimeThreshold = 30f;
    public float fourStarTimeThreshold = 60f;
    public float threeStarTimeThreshold = 90f;
    public float twoStarTimeThreshold = 120f;

    [Tooltip("Attempt thresholds (number of times Run was pressed)")]
    public int fiveStarAttemptThreshold = 1;
    public int fourStarAttemptThreshold = 2;
    public int threeStarAttemptThreshold = 3;
    public int twoStarAttemptThreshold = 5;

    [Header("Command Efficiency Thresholds")]
    [Tooltip("Optimal command counts for coloring")]
    public int commandsOptimal = 10;      // Green if ≤ this
    public int commandsGoodRange = 15;    // Yellow-green if ≤ this
    public int commandsAvgRange = 20;     // Yellow if ≤ this
    public int commandsVerboseRange = 30; // Orange if ≤ this
    // Red if > verboseRange

    private int currentSceneIndex;
    private float totalTime = 0f;
    private int totalAttempts = 0;
    private int commandsUsed = 0;
    private bool isTracking = true;

    void Start()
    {
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (scorePanel != null)
            scorePanel.SetActive(false);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitClicked);

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);
    }

    void Update()
    {
        if (isTracking && Time.timeScale > 0)
            totalTime += Time.deltaTime;
    }

    public void IncrementAttempts()
    {
        totalAttempts++;
        Debug.Log($"Attempts: {totalAttempts}");
    }

    public void ShowEndGameReport(int finalCommandCount)
    {
        if (scorePanel == null) return;

        isTracking = false;
        commandsUsed = finalCommandCount;
        scorePanel.SetActive(true);
        Time.timeScale = 0f;

        // Play level complete sound
        ProgramManager pm = FindFirstObjectByType<ProgramManager>();
        if (pm != null)
            pm.PlayLevelComplete();

        int stars = CalculateStars(totalTime, totalAttempts, commandsUsed);
        UpdateStarDisplay(stars);

        if (statusText != null)
        {
            statusText.text = "== LEVEL COMPLETE ==";
            statusText.color = new Color(0.4f, 1f, 0.4f);
        }

        // TIME - Color based on speed
        if (timeText != null)
        {
            int timeSeconds = Mathf.RoundToInt(totalTime);
            int minutes = timeSeconds / 60;
            int seconds = timeSeconds % 60;
            timeText.text = $"Total Time\n<size=80><b>{minutes}:{seconds:D2}</b></size>";
            timeText.color = GetTimeColor(totalTime);
        }

        // ATTEMPTS - Color based on number of runs
        if (attemptsText != null)
        {
            attemptsText.text = $"Runs\n<size=80><b>{totalAttempts}</b></size>";
            attemptsText.color = GetAttemptsColor(totalAttempts);
        }

        // COMMANDS - Color based on efficiency
        if (commandsText != null)
        {
            commandsText.text = $"Commands Used\n<size=80><b>{commandsUsed}</b></size>";
            commandsText.color = GetCommandsColor(commandsUsed);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = $"PERFORMANCE RATING\n<size=90><b>{stars}</b></size> <size=60>/ 5</size>";
            finalScoreText.color = GetStarColor(stars);
        }

        if (ratingText != null)
        {
            ratingText.text = GetRatingText(stars);
            ratingText.color = GetStarColor(stars);
        }
    }

    private Color GetTimeColor(float time)
    {
        if (time <= fiveStarTimeThreshold) return timeExcellent;        // ≤30s
        if (time <= fourStarTimeThreshold) return timeGood;             // 31-60s
        if (time <= threeStarTimeThreshold) return timeAverage;         // 61-90s
        if (time <= twoStarTimeThreshold) return timePoor;              // 91-120s
        return timeSlow;                                                 // >120s
    }

    private Color GetAttemptsColor(int attempts)
    {
        if (attempts == 1) return attemptsExcellent;                    // Perfect first try
        if (attempts == 2) return attemptsGood;                         // 2 runs
        if (attempts == 3) return attemptsAverage;                      // 3 runs
        if (attempts <= 5) return attemptsPoor;                         // 4-5 runs
        return attemptsMany;                                             // 6+ runs
    }

    private Color GetCommandsColor(int commands)
    {
        if (commands <= commandsOptimal) return commandsExcellent;      // ≤10
        if (commands <= commandsGoodRange) return commandsGood;         // 11-15
        if (commands <= commandsAvgRange) return commandsAverage;       // 16-20
        if (commands <= commandsVerboseRange) return commandsVerbose;   // 21-30
        return commandsExcessive;                                        // 31+
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
        int timeStars = 5;
        int attemptStars = 5;

        // Time-based stars
        if (timeTaken > twoStarTimeThreshold) timeStars = 1;
        else if (timeTaken > threeStarTimeThreshold) timeStars = 2;
        else if (timeTaken > fourStarTimeThreshold) timeStars = 3;
        else if (timeTaken > fiveStarTimeThreshold) timeStars = 4;

        // Attempt-based stars
        if (attempts > twoStarAttemptThreshold) attemptStars = 1;
        else if (attempts > threeStarAttemptThreshold) attemptStars = 2;
        else if (attempts > fourStarAttemptThreshold) attemptStars = 3;
        else if (attempts > fiveStarAttemptThreshold) attemptStars = 4;
        else if (attempts == fiveStarAttemptThreshold) attemptStars = 5;

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

    void OnExitClicked()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(overworldSceneName))
            SceneManager.LoadScene(overworldSceneName);
        else
            SceneManager.LoadScene(overworldSceneIndex);
    }

    void OnRetryClicked()
    {
        Debug.Log("=== OnRetryClicked START ===");
        Time.timeScale = 1f;

        // Check if there's a second level to load
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        Debug.Log($"LevelManager found: {levelManager != null}");

        if (levelManager != null)
        {
            bool shouldLoad = levelManager.ShouldLoadSecondLevel();
            Debug.Log($"ShouldLoadSecondLevel returned: {shouldLoad}");

            if (shouldLoad)
            {
                Debug.Log("Closing scoring panel and loading second level...");

                // Close scoring panel and load second level
                if (scorePanel != null)
                    scorePanel.SetActive(false);

                levelManager.LoadSecondLevel();
            }
            else
            {
                Debug.Log("No second level - reloading scene");
                SceneManager.LoadScene(currentSceneIndex);
            }
        }
        else
        {
            Debug.LogError("LevelManager not found!");
            SceneManager.LoadScene(currentSceneIndex);
        }

        Debug.Log("=== OnRetryClicked END ===");
    }

    public void ResetTracking()
    {
        totalTime = 0f;
        totalAttempts = 0;
        commandsUsed = 0;
        isTracking = true;
    }
}
