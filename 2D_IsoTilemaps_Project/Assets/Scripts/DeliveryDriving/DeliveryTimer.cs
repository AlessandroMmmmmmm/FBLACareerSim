using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tracks delivery time and applies penalties for off-road driving
/// Supports both Unity UI Text and TextMeshPro
/// </summary>
public class DeliveryTimer : MonoBehaviour
{
    [Header("UI (Use one or the other)")]
    public Text timerText; // Legacy UI Text
    public TextMeshProUGUI timerTextTMP; // TextMeshPro

    [Header("Timer Settings")]
    public bool isRunning = false;
    public float currentTime = 0f;
    public float penaltyTime = 0f;

    private void Update()
    {
        if (isRunning)
        {
            currentTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    public void StartTimer()
    {
        isRunning = true;
        currentTime = 0f;
        penaltyTime = 0f;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void AddPenalty(float seconds)
    {
        penaltyTime += seconds;
    }

    public float GetTotalTime()
    {
        return currentTime + penaltyTime;
    }

    public float GetFinalTime()
    {
        return currentTime;
    }

    private void UpdateTimerDisplay()
    {
        float displayTime = GetTotalTime();
        int minutes = Mathf.FloorToInt(displayTime / 60f);
        int seconds = Mathf.FloorToInt(displayTime % 60f);
        int milliseconds = Mathf.FloorToInt((displayTime * 100f) % 100f);

        string timeString = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
        Color displayColor = penaltyTime > 0.1f ? Color.red : Color.white;

        // Update whichever text component is assigned
        if (timerText != null)
        {
            timerText.text = timeString;
            timerText.color = displayColor;
        }

        if (timerTextTMP != null)
        {
            timerTextTMP.text = timeString;
            timerTextTMP.color = displayColor;
        }
    }

    public string GetTimeString()
    {
        float displayTime = GetTotalTime();
        int minutes = Mathf.FloorToInt(displayTime / 60f);
        int seconds = Mathf.FloorToInt(displayTime % 60f);
        int milliseconds = Mathf.FloorToInt((displayTime * 100f) % 100f);

        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }
}
