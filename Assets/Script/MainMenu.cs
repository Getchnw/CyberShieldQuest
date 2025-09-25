using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void NewGame ()
    {
        SceneManager.LoadScene("Home");
    }
    public void ContinuosGame()
    {
        SceneManager.LoadScene("Home");
    }
    public void QuitGame ()
    {
        Debug.Log("Quit GAme");
        Application.Quit();
    }
}
