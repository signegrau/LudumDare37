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

    public delegate void StateChanged(int index, int statesCount);
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
    private bool isDirty;

    private int currentStateIndex
    {
        get { return _currentStateIndex; }
        set
        {
            _currentStateIndex = value;
            if (stateChanged != null)
            {
                stateChanged(value, level.StatesCount);
            }

            stateLabel.text = string.Format(stateLabelTemplate, value + 1);
        }
    }

    private Level level;

    private Tile.State[] currentTileStates
    {
        get { return level.GetState(currentStateIndex).tileStates; }
    }

    public List<Tile.State> statesToShow = new List<Tile.State>();
    public RectTransform tileStateButtonPanel;
    public GameObject tileStateButtonPrefab;

    private int currentPlayerStartIndex;

    private bool isPlaying;
    private bool startMissing;

    private List<int> indexesMissingPickups = new List<int>();
    private bool noStartPosition;

    public RectTransform missingStatesPanel;
    public Text missingStateLabel;
    public Text noSpawnLabel;

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

        
    }

    public void ButtonCreateNewLevel()
    {
        if (isDirty)
        {
            dialogOpen = true;
            UnsavedChangesDialog.result += UnsavedChangesDialogOnResultNewLevel;
            unsavedChangesDialog.Show();
        }
        else
        {
            CreateNewLevel();
        }
    }

    private void UnsavedChangesDialogOnResultNewLevel(bool overwrite)
    {
        if (overwrite)
        {
            CreateNewLevel();
        }

        UnsavedChangesDialog.result -= UnsavedChangesDialogOnResultNewLevel;
    }

    private void CreateNewLevel()
    {
        level = new Level();
        var state = LevelState.Empty;
        level.AddState(state);

        _levelManager.ChangeState(state, true);
        currentStateIndex = 0;

        currentFileName = "<new level>";
        fileNameLabel.text = "<new level>";
        SetDirty(false);
        CheckForErrors();
    }

    private void SetDirty(bool dirty)
    {
        isDirty = dirty;

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
        if (dialogOpen || level == null) return;
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

        CheckForErrors();
    }

    private void CheckForErrors()
    {
        indexesMissingPickups.Clear();
        level.StatesFindSpecialIndexes();
        for (var i = 0; i < level.StatesCount; i++)
        {
            var state = level.GetState(i);
            if (state.PickupPosition == -1)
            {
                indexesMissingPickups.Add(i);
            }
        }

        noStartPosition = !level.GetState(0).HasStartPosition;

        if (indexesMissingPickups.Count > 0)
        {
            missingStatesPanel.gameObject.SetActive(true);

            missingStateLabel.text = "";
            foreach(var index in indexesMissingPickups)
            {
                missingStateLabel.text += string.Format("{0}, ", index+1);
            }
        }
        else
        {
            missingStatesPanel.gameObject.SetActive(false);
        }

        noSpawnLabel.enabled = noStartPosition;
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
            StartCoroutine(OnLevelSceneLoaded());
        }
    }

    public IEnumerator OnLevelSceneLoaded()
    {
        _levelManager = FindObjectOfType<LevelManager>();
        yield return StartCoroutine(_levelManager.Setup());

        Camera.main.orthographicSize = 7;

        if (level == null)
        {
            CreateNewLevel();
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
        if (!level.HasState(currentStateIndex + 1))
        {
            var newState = new LevelState();
            level.AddState(newState);
        }

        GotoState(currentStateIndex + 1);
    }

    public void GotoPreviousState()
    {
        if (currentStateIndex < 1) return;

        GotoState(currentStateIndex - 1);
    }

    private void GotoState(int index)
    {
        int playerStartIndex = -1;
        if (currentStateIndex < level.StatesCount)
        {
            playerStartIndex = SanitizeCurrentState();
        }

        CheckForErrors();

        currentStateIndex = index;

        if (index > 0)
        {
            playerStartIndex = level.GetState(index - 1).PickupPosition;

            if (playerStartIndex >= 0)
            {
                currentPlayerStartIndex = playerStartIndex;
                currentTileStates[playerStartIndex] = Tile.State.PlayerStart;
            }
            
        }

        _levelManager.ChangeState(currentTileStates, true);
    }

    private int SanitizeCurrentState()
    {
        return SanitizeState(level.GetState(currentStateIndex), currentStateIndex == 0);
    }

    private int SanitizeState(LevelState state, bool isFirst)
    {
        var startIndex = 0;
        var tileStates = state.tileStates;

        for (var i = 0; i < tileStates.Length; i++)
        {
            if (tileStates[i] == Tile.State.PlayerStart)
            {
                startIndex = i;

                if (!isFirst)
                {
                    tileStates[i] = Tile.State.Wall;
                    
                }
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
                var startIndex = SanitizeCurrentState();

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
        if (isDirty)
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

        CheckForErrors();
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

    public void GotoFirstState()
    {
        GotoState(0);
    }

    public void GotoLastState()
    {
        if (currentStateIndex + 1 == level.StatesCount)
        {
            var newState = level.GetState(currentStateIndex).Duplicate();
            SanitizeState(newState, false);
            level.AddState(newState);
        }

        GotoState(level.StatesCount - 1);

    }

    public void GotoMainMenu()
    {
        if (isDirty)
        {
            dialogOpen = true;
            UnsavedChangesDialog.result += UnsavedChangesDialogOnResultExit;
            unsavedChangesDialog.Show();
        }
        else
        {
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }
    }

    private void UnsavedChangesDialogOnResultExit(bool overwrite)
    {
        if (overwrite)
        {
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }

        UnsavedChangesDialog.result -= UnsavedChangesDialogOnResultExit;
        dialogOpen = false;
    }

    public void DeleteCurrentState()
    {
        if (currentStateIndex <= 0) return;

        level.RemoveState(currentStateIndex);

        GotoState(currentStateIndex - 1);
        // Side effects making currentStateIndex = currentStateIndex - 1
        
    }
}
