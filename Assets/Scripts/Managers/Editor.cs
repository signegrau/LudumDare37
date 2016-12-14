using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
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

    public delegate void StateChanged(int index);
    public static event StateChanged stateChanged;

    public GameManager gameManager;
    public Text stateLabel;
    private string stateLabelTemplate;

    public InputField fileNameInput;

    private LevelManager _levelManager;
    private Tile.State selectedState = Tile.State.Platform;

    private int _currentStateIndex;

    private int currentStateIndex
    {
        get { return _currentStateIndex; }
        set
        {
            _currentStateIndex = value;
            if (stateChanged != null)
            {
                stateChanged(value);
            }

            stateLabel.text = string.Format(stateLabelTemplate, value + 1);
        }
    }

    private Level level = new Level();

    private Tile.State[] currentTileStates
    {
        get { return level.GetState(currentStateIndex).tileStates; }
    }

    public List<Tile.State> statesToShow = new List<Tile.State>();
    public RectTransform tileStateButtonPanel;
    public GameObject tileStateButtonPrefab;

    private int currentPlayerStartIndex;

    private bool isPlaying;

    private void Start()
    {
        LoadLevelScene();

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
        if (currentStateIndex > 0 && currentTileStates[index] == Tile.State.PlayerStart) return;

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

    private void LoadLevelScene()
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

    public void StartPlayingLevel()
    {
        StartPlaying(0);
    }

    public void StartPlaying(int index)
    {
        if (isPlaying) return;

        isPlaying = true;

        level.StatesFindSpecialIndexes();

        gameManager.levelManager = _levelManager;
        gameManager.StartGame(level, index);

        if (playingStart != null)
        {
            playingStart();
        }
    }

    public void StopPlaying()
    {
        isPlaying = false;
        currentStateIndex = gameManager.StopGame() - 1;

        if (currentStateIndex > 0)
        {
            var playerStartIndex = level.GetState(currentStateIndex-1).PickupPosition;
            currentTileStates[playerStartIndex] = Tile.State.PlayerStart;
        }

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
            StopPlaying();
        }
        else
        {
            StartPlaying(currentStateIndex);
        }
    }

    public void TogglePlayingLevel()
    {
        if (isPlaying)
        {
            StopPlaying();
        }
        else
        {
            StartPlayingLevel();
        }
    }

    private void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isPlaying)
            {
                StopPlaying();
            }
            else
            {
                StartPlayingLevel();
            }
        }

        for (var i = 0; i < Mathf.Min(statesToShow.Count, 10); i++)
        {
            if (Input.GetKeyDown((i + 1 % 10).ToString()))
            {
                ChangeState(statesToShow[i]);
            }
        }*/
    }

    public void GotoNextState()
    {
        level.StatesFindSpecialIndexes();
        var playerStartIndex = level.GetState(currentStateIndex).PickupPosition;

        if (playerStartIndex == -1)
        {
            Debug.Log("State must have a pickup");
            return;
        }

        SanitizeLevel();

        if (!level.HasState(currentStateIndex + 1))
        {
            var newState = new LevelState();
            level.AddState(newState);
        }

        currentPlayerStartIndex = playerStartIndex;
        currentStateIndex += 1;

        currentTileStates[playerStartIndex] = Tile.State.PlayerStart;
        
        _levelManager.ChangeState(currentTileStates, true);

        
    }

    public void GotoPreviousState()
    {
        if (currentStateIndex < 1) return;

        level.StatesFindSpecialIndexes();

        int playerStartIndex = SanitizeLevel();

        currentStateIndex -= 1;

        if (currentStateIndex > 0)
        {
            playerStartIndex = level.GetState(currentStateIndex - 1).PickupPosition;

            currentPlayerStartIndex = playerStartIndex;
            currentTileStates[playerStartIndex] = Tile.State.PlayerStart;
        }

        _levelManager.ChangeState(currentTileStates, true);
    }

    private int SanitizeLevel()
    {
        var startIndex = 0;
        
        for (var i = 0; i < currentTileStates.Length; i++)
        {
            if (currentTileStates[i] == Tile.State.PlayerStart)
            {
                if (currentStateIndex > 0)
                {
                    currentTileStates[i] = Tile.State.Wall;
                }
                startIndex = i;
            }
        }

        return startIndex;
    }

    public void SaveLevel()
    {
        var fileName = fileNameInput.text;

        if (string.IsNullOrEmpty(fileName))
        {
            Debug.Log("No file name provided");
        }
        else
        {
            var startIndex = SanitizeLevel();

            LevelLoader.SaveLevelToFile(fileName, level);

            if (currentStateIndex > 0)
            {
                currentTileStates[startIndex] = Tile.State.PlayerStart;
            }
        }


    }

    public void LoadLevelFromFile()
    {
        var fileName = fileNameInput.text;

        if (string.IsNullOrEmpty(fileName))
        {
            Debug.Log("No file name provided");
        }
        else
        {
            var newLevel = LevelLoader.LoadLevelFromFile(fileName);

            if (newLevel == null) return;

            level = newLevel;
            currentStateIndex = 0;
            _levelManager.ChangeState(currentTileStates, true);
        }
    }
}
