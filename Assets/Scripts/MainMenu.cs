using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public RawImage instructions;

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void LoadGame()
    {
        SceneManager.LoadScene(1);
    }

    public void ToggleInstructions()
    {
        instructions.gameObject.SetActive(!instructions.gameObject.activeInHierarchy);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
