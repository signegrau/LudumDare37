using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndScreen : MonoBehaviour
{
    public delegate void PlayAgainPressedHandler();
    public static event PlayAgainPressedHandler OnPlayAgainPressed;

    public Text timer;
    public Text deathCounter;

    private string timerTemplate;
    private string deathCounterTemplate;

    private float miliseconds, seconds, minutes;

    private CanvasGroup _canvasGroup;

    private void OnEnable()
    {
        GameManager.OnGameEnd += OnGameEnd;
    }

    private void OnGameEnd(float time, int deaths)
    {
        minutes = time / 60;
        seconds = Mathf.Floor(time) % 60;
        miliseconds = time * 1000 % 1000;

        timer.text = string.Format(timerTemplate, minutes, seconds, miliseconds);

        deathCounter.text = string.Format(deathCounterTemplate, deaths);

        _canvasGroup.alpha = 1;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.interactable = true;
    }

    private void OnDisable()
    {
        GameManager.OnGameEnd -= OnGameEnd;
    }

    private void Start()
    {
        timerTemplate = timer.text;
        deathCounterTemplate = deathCounter.text;

        _canvasGroup = GetComponent<CanvasGroup>();

        _canvasGroup.alpha = 0;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;

        deathCounter.text = string.Format(deathCounterTemplate, 0);
    }

    public void PlayAgain()
    {
        if (OnPlayAgainPressed != null)
        {
            OnPlayAgainPressed();
        }

        _canvasGroup.alpha = 0;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
    }
}
