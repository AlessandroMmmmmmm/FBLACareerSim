using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI; // Assign your pause menu panel here
    public KeyCode pauseKey = KeyCode.Escape; // Default pause key
    
    private bool isPaused = false;
    
    void Update()
    {
        // Check for pause key press
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }
    
    public void Pause()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }
        Time.timeScale = 0f; // Freeze game time
        isPaused = true;
    }
    
    public void Resume()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        Time.timeScale = 1f; // Resume game time
        isPaused = false;
    }
    
    public void RestartLevel()
    {
        Time.timeScale = 1f; // Make sure time is running
        SceneManager.LoadScene(2);
    }
    
    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Make sure time is running
        SceneManager.LoadScene(0); // Assumes title screen is index 0
    }
    
    public void QuitGame()
    {
        Time.timeScale = 1f; // Make sure time is running
        Application.Quit();
        Debug.Log("Quit Game");
    }
}