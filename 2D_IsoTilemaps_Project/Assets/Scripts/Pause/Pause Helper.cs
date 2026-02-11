// using UnityEngine;

// public class PauseMenuButtons : MonoBehaviour
// {
//     private PauseManager pauseManager;
    
//     void Start()
//     {
//         pauseManager = FindFirstObjectByType<PauseManager>();
//     }
    
//     public void OnResumeClicked()
//     {
//         if (pauseManager != null) pauseManager.Resume();
//     }
    
//     public void OnRestartClicked()
//     {
//         if (pauseManager != null) pauseManager.RestartLevel();
//     }
    
//     public void OnMainMenuClicked()
//     {
//         if (pauseManager != null) pauseManager.LoadMainMenu();
//     }
    
//     public void OnQuitClicked()
//     {
//         if (pauseManager != null) pauseManager.QuitGame();
//     }
// }