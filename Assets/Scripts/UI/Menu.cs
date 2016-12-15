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
    public Button quitButton;

    private void Start()
    {
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
