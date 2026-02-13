    using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using  UnityEngine.SceneManagement;
public class Menu : MonoBehaviour
{
    public void onPlayButton()
    {
        SceneManager.LoadScene(2);

    }
    public void onInstructionButton()
    {
        SceneManager.LoadScene("Instructions");

    }

    public void onBackButton()
    {
        SceneManager.LoadScene(0);

    }

    public void onOptionsButton()
    {
        SceneManager.LoadScene("Options");
    }

    public void onQuitButton()
    {
        Application.Quit();

    }
    

}
