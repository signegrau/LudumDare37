﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Editor : MonoBehaviour
{
    public delegate void SelectedStateChangedHandler(Tile.State state);
    public static event SelectedStateChangedHandler selectedStateChanged;

    public delegate void PlayingStartHandler();
    public static event PlayingStartHandler playingStart;

    public delegate void PlayingStopHandler();
    public static event PlayingStopHandler playingStop;

    public GameManager gameManager;
    public Text stateLabel;
    private string stateLabelTemplate;

    private LevelManager _levelManager;
    private Tile.State selectedState = Tile.State.Platform;

    private int currentStateIndex;
    private Level level = new Level();

    private Tile.State[] currentTileStates
    {
        get { return level.GetState(currentStateIndex).tileStates; }
    }

    public List<Tile.State> statesToShow = new List<Tile.State>();
    public RectTransform tileStateButtonPanel;
    public GameObject tileStateButtonPrefab;

    private bool isPlaying;

    private void Start()
    {
        LoadLevel();

        foreach (var tileState in statesToShow)
        {
            var go = Instantiate(tileStateButtonPrefab, tileStateButtonPanel);
            go.GetComponent<TileStateButton>().SetState(tileState);
        }

        var state = new LevelState();
        level.AddState(state);

        ChangeState(Tile.State.Platform);

        stateLabelTemplate = stateLabel.text;
        stateLabel.text = string.Format(stateLabelTemplate, 1);
    }

    private void OnEnable()
    {
        Tile.tilePressed += OnTilePressed;
        SceneManager.sceneLoaded += OnSceneLoaded;
        TileStateButton.tileStateButtonPressed += ChangeState;
    }

    private void OnDisable()
    {
        Tile.tilePressed -= OnTilePressed;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        TileStateButton.tileStateButtonPressed -= ChangeState;
    }

    private void OnTilePressed(int index, int mouseButton)
    {
        if (mouseButton == 0)
        {
            currentTileStates[index] = selectedState;
            var tile = _levelManager.Tiles[index];
            tile.GotoState(selectedState, true);
        }
        else
        {
            currentTileStates[index] = Tile.State.Wall;
            var tile = _levelManager.Tiles[index];
            tile.GotoState(Tile.State.Wall, true);
        }

    }

    private void ChangeState(Tile.State state)
    {
        this.selectedState = state;

        if (selectedStateChanged != null)
        {
            selectedStateChanged(state);
        }
    }

    private void LoadLevel()
    {
        var scene = SceneManager.GetSceneByName("Level");

        if (scene.isLoaded) return;

        SceneManager.LoadScene("Level", LoadSceneMode.Additive);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.name == "Level")
        {
            _levelManager = FindObjectOfType<LevelManager>();
            StartCoroutine(_levelManager.Setup());
        }
    }

    public void PlayLevel()
    {
        if (isPlaying) return;

        isPlaying = true;

        level.StatesFindSpecialIndexes();

        gameManager.levelManager = _levelManager;
        gameManager.StartGame(level);

        if (playingStart != null)
        {
            playingStart();
        }
    }

    public void StopPlayingLevel()
    {
        isPlaying = false;
        gameManager.StopGame();
        _levelManager.ChangeState(currentTileStates, true);

        if (playingStop != null)
        {
            playingStop();
        }
    }

    public void TogglePlaying()
    {
        if (isPlaying)
        {
            StopPlayingLevel();
        }
        else
        {
            PlayLevel();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isPlaying)
            {
                StopPlayingLevel();
            }
            else
            {
                PlayLevel();
            }
        }

        for (var i = 0; i < Mathf.Min(statesToShow.Count, 10); i++)
        {
            if (Input.GetKeyDown((i + 1 % 10).ToString()))
            {
                ChangeState(statesToShow[i]);
            }
        }
    }
}
