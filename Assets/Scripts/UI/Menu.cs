using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public delegate void PlayPressedHandler();
    public static PlayPressedHandler OnPlayPressed;

    public CanvasGroup titleMenu;
    public CanvasGroup customLevelMenu;
    public Button quitButton;

    private void Start()
    {
        titleMenu.SetVisibility(true);
        customLevelMenu.SetVisibility(false);

#if UNITY_STANDALONE
        quitButton.gameObject.SetActive(true);
#else
        quitButton.gamerObject.SetActive(false);
#endif
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    public void GotoCustomLevelMenu()
    {
        titleMenu.SetVisibility(false);
        customLevelMenu.SetVisibility(true);
    }

    public void GotoTitleMenu()
    {
        titleMenu.SetVisibility(true);
        customLevelMenu.SetVisibility(false);
    }

    public void GotoEditor()
    {
        SceneManager.LoadScene("Editor", LoadSceneMode.Single);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
