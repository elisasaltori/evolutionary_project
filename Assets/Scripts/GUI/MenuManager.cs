using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Contains functions for loading scenes for use in buttons
/// </summary>
public class MenuManager : MonoBehaviour
{

    public void LoadLevel(int levelNumber)
    {
        string levelName = "Lvl" + levelNumber;
        SceneManager.LoadScene(levelName);
    }

    public void LoadCredits()
    {
        SceneManager.LoadScene("CreditsMenu");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadSettings()
    {
        SceneManager.LoadScene("OptionsMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadLevelMenu()
    {
        SceneManager.LoadScene("LevelMenu");
    }


}
