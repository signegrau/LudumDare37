using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    public Text timer;
    public Text deathCounter;

    private string timerTemplate;
    private string deathCounterTemplate;

    private float startTime;
    private bool gameStarted;

    private float miliseconds, seconds, minutes, time;

    private CanvasGroup _canvasGroup;

    private void OnEnable()
    {
        LevelManager.OnGameStart += OnGameStart;
    }

    private void OnDisable()
    {
        LevelManager.OnGameStart -= OnGameStart;
    }

    private void Start()
    {
        timerTemplate = timer.text;
        deathCounterTemplate = deathCounter.text;

        _canvasGroup = GetComponent<CanvasGroup>();

        _canvasGroup.alpha = 0;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
    }

    private void Update()
    {
        if (!gameStarted) return;

        time = Time.time - startTime;

        minutes = time / 60;
        seconds = Mathf.Floor(time) % 60;
        miliseconds = time * 1000 % 1000;

        timer.text = string.Format(timerTemplate, minutes, seconds, miliseconds);
    }

    private void OnGameStart(float time)
    {
        startTime = time;
        gameStarted = true;

        _canvasGroup.alpha = 1;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.interactable = true;
    }
}
