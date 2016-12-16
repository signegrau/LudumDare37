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
    public Text fileNameLabel;

    public InputField fileNameInput;
    

    public FilePicker filePicker;
    public UnsavedChangesDialog unsavedChangesDialog;

    private LevelManager _levelManager;
    private Tile.State selectedState = Tile.State.Platform;

    private int _currentStateIndex;

    private bool dialogOpen;

    private string currentFileName;
    private bool hasBeenSaved;
    private bool _dirty;

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

        ChangeState(Tile.State.Platform);

        stateLabelTemplate = stateLabel.text;
        stateLabel.text = string.Format(stateLabelTemplate, 1);

        CreateNewLevel();
    }

    private void CreateNewLevel()
    {
        level = new Level();
        var state = new LevelState();
        level.AddState(state);

        currentFileName = "<new level>";
        fileNameLabel.text = "<new level>";
        SetDirty(false);
    }

    private void SetDirty(bool dirty)
    {
        _dirty = dirty;

        if (dirty)
        {
            fileNameLabel.text = currentFileName + "*";
        }
        else
        {
            fileNameLabel.text = currentFileName;
        }
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
        if (dialogOpen) return;
        if (currentStateIndex > 0 && currentTileStates[index] == Tile.State.PlayerStart) return;

        if (mouseButton == 0)
        {
            if (currentTileStates[index] == selectedState) return;

            currentTileStates[index] = selectedState;
            var tile = _levelManager.Tiles[index];
            tile.GotoState(selectedState, true);
            SetDirty(true);
        }
        else
        {
            if (currentTileStates[index] == Tile.State.Wall) return;

            currentTileStates[index] = Tile.State.Wall;
            var tile = _levelManager.Tiles[index];
            tile.GotoState(Tile.State.Wall, true);
            SetDirty(true);
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

            Camera.main.orthographicSize = 7;
        }
    }

    public void StartPlayingLevel()
    {
        StartPlaying(0);
    }

    public void StartPlayingFromCurrentState()
    {
        StartPlaying(currentStateIndex);
    }

    public void StartPlaying(int index)
    {
        if (isPlaying) return;

        level.StatesFindSpecialIndexes();

        if (!level.GetState(index).HasStartPosition)
        {
            Debug.Log("A start position is required");
            return;
        }

        isPlaying = true;
        gameManager.levelManager = _levelManager;
        gameManager.StartGame(level, index);

        if (playingStart != null)
        {
            playingStart();
        }
    }

    public void StopPlaying()
    {
        if (!isPlaying) return;

        isPlaying = false;
        currentStateIndex = gameManager.StopGame();
        Debug.Log(currentStateIndex);

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
        if (string.IsNullOrEmpty(currentFileName) || !hasBeenSaved)
        {
            filePicker.Show(false);
            FilePicker.fileChoosen += OnSaveFileChoosen;
            dialogOpen = true;
        }
        else
        {
            SetDirty(false);
            LevelLoader.SaveLevelToFile(currentFileName, level);
        }
    }

    public void OnSaveFileChoosen(string fileName)
    {
        if (fileName != null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.Log("No file name provided");
            }
            else
            {
                SetDirty(false);
                hasBeenSaved = true;
                fileNameLabel.text = fileName;

                currentFileName = fileName;
                var startIndex = SanitizeLevel();

                LevelLoader.SaveLevelToFile(fileName, level);

                if (currentStateIndex > 0)
                {
                    currentTileStates[startIndex] = Tile.State.PlayerStart;
                }
            }
        }

        dialogOpen = false;
        FilePicker.fileChoosen -= OnSaveFileChoosen;
    }

    public void LoadLevelFromFile()
    {
        if (_dirty)
        {
            dialogOpen = true;
            UnsavedChangesDialog.result += UnsavedChangesDialogOnResultLoad;
            unsavedChangesDialog.Show();
        }
        else
        {
            ShowLoadLevelPicker();
        }
    }

    private void UnsavedChangesDialogOnResultLoad(bool overwrite)
    {
        dialogOpen = false;
        if (overwrite)
        {
            ShowLoadLevelPicker();
        }
        UnsavedChangesDialog.result -= UnsavedChangesDialogOnResultLoad;
    }

    private void ShowLoadLevelPicker()
    {
        filePicker.Show(true);
        FilePicker.fileChoosen += OnLevelFileChoosen;
        dialogOpen = true;
    }

    public void OnLevelFileChoosen(string fileName)
    {
        if (fileName != null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.Log("No file name provided");
            }
            else
            {
                SetDirty(false);
                hasBeenSaved = true;
                fileNameLabel.text = fileName;
                currentFileName = fileName;

                var newLevel = LevelLoader.LoadLevelFromFile(fileName);

                if (newLevel == null) return;

                level = newLevel;
                currentStateIndex = 0;
                _levelManager.ChangeState(currentTileStates, true);
            }
        }

        dialogOpen = false;
        FilePicker.fileChoosen -= OnLevelFileChoosen;
    }

    public void Update()
    {
        if (dialogOpen) return;

        if (Input.GetKeyDown(KeyCode.Return))
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
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isPlaying)
            {
                StartPlaying(currentStateIndex);
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
