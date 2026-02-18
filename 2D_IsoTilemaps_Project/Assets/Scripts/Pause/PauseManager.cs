using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public KeyCode pauseKey = KeyCode.Escape;

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Resume()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        // Check if we're in the software engineer game and there's a second level to load
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null && levelManager.ShouldLoadSecondLevel())
        {
            Debug.Log("PauseManager: Loading second level instead of restarting");

            // Close any open UI
            if (pauseMenuUI != null)
                pauseMenuUI.SetActive(false);

            // Close scoring panel if it's open
            SoftwareEngScoring scoring = FindFirstObjectByType<SoftwareEngScoring>();
            if (scoring != null && scoring.scorePanel != null)
                scoring.scorePanel.SetActive(false);

            levelManager.LoadSecondLevel();
        }
        else
        {
            // Normal restart - reload the scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void LoadMainScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(2);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
