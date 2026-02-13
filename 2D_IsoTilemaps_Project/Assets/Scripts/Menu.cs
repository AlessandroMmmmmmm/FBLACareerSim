    using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using  UnityEngine.SceneManagement;
public class Menu : MonoBehaviour
{

    public float buttonSoundDelay = 0.15f;

    public void onPlayButton()
    {
        StartCoroutine(LoadSceneWithDelay(2));

    }
    public void onInstructionButton()
    {
        StartCoroutine(LoadSceneWithDelay("Instructions"));

    }

    public void onBackButton()
    {
        StartCoroutine(LoadSceneWithDelay(0));
    }

    public void onOptionsButton()
    {
        StartCoroutine(LoadSceneWithDelay("Options"));
    }

    public void onQuitButton()
    {
        Application.Quit();
    }

    // Helper coroutine to wait before loading scene
    IEnumerator LoadSceneWithDelay(int sceneIndex)
    {
        yield return new WaitForSeconds(buttonSoundDelay);
        SceneManager.LoadScene(sceneIndex);
    }
    
    IEnumerator LoadSceneWithDelay(string sceneName)
    {
        yield return new WaitForSeconds(buttonSoundDelay);
        SceneManager.LoadScene(sceneName);
    }
    

}
