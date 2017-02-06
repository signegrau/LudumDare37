using UnityEngine;
using UnityEngine.EventSystems;

public class PauseScreen : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    public GameObject firstSelected;

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
        EventSystem.current.SetSelectedGameObject(firstSelected);
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
