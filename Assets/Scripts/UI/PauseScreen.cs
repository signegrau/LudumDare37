using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseScreen : MonoBehaviour
{
    private CanvasGroup _canvasGroup;

    private void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.SetVisibility(false);
    }

    private void OnEnable()
    {
        GameManager.paused += GameManagerOnPaused;
        GameManager.resume += GameManagerOnResume;
    }

    private void OnDisable()
    {
        GameManager.paused -= GameManagerOnPaused;
        GameManager.resume -= GameManagerOnResume;
    }

    private void GameManagerOnResume(float timeStart, float timeAdd)
    {
        _canvasGroup.SetVisibility(false);
    }

    private void GameManagerOnPaused()
    {
        _canvasGroup.SetVisibility(true);
    }

    public void ResumeGame()
    {
        GameManager.InstanceResume();
    }

    public void RestartGame()
    {
        GameManager.InstanceRestart();
    }

    public void QuitGame()
    {
        GameManager.InstanceQuit();
    }
}
