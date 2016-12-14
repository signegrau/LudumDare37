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

    public GameObject playButton;

    private CanvasGroup _canvasGroup;

    private void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        EventSystem.current.SetSelectedGameObject(playButton, null);
    }

    public void PlayGame()
    {
        if (OnPlayPressed != null)
        {
            OnPlayPressed();
        }

        _canvasGroup.alpha = 0;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
    }

    public void GotoEditor()
    {
        SceneManager.LoadScene("Editor", LoadSceneMode.Single);
    }
}
