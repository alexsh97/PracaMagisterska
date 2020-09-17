using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Klasa do zarządzania menu głównym
/// </summary>
public class MainMenu : MonoBehaviour
{
    /// <summary>
    /// Włączenie gry, ładując pierwszą scene
    /// </summary>
    public void PlayGame()
    {
        SceneManager.LoadScene("Level1");
    }

    /// <summary>
    /// Opuszczanie gry - wyłączenie aplikacji
    /// </summary>
    public void QuiteGame()
    {
        Application.Quit();
    }
}
