using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Editor : MonoBehaviour
{
    public GameManager gameManager;
    private LevelManager _levelManager;
    private Tile.State selectedState = Tile.State.Platform;

    private Tile.State[] tileStates = new Tile.State[100];

    public List<Tile.State> statesToShow = new List<Tile.State>();
    public RectTransform tileStateButtonPanel;
    public GameObject tileStateButtonPrefab;

    private bool isPlaying;

    private void Start()
    {
        LoadLevel();

        foreach (var state in statesToShow)
        {
            var go = Instantiate(tileStateButtonPrefab, tileStateButtonPanel);
            go.GetComponent<TileStateButton>().SetState(state);
        }
    }

    private void OnEnable()
    {
        Tile.tilePressed += OnTilePressed;
        SceneManager.sceneLoaded += OnSceneLoaded;
        TileStateButton.tileStateButtonPressed += OnTileStateButtonPressed;
    }

    private void OnTileStateButtonPressed(Tile.State state)
    {
        selectedState = state;
    }

    private void OnDisable()
    {
        Tile.tilePressed -= OnTilePressed;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        TileStateButton.tileStateButtonPressed -= OnTileStateButtonPressed;
    }

    private void OnTilePressed(int index, int mouseButton)
    {
        if (mouseButton == 0)
        {
            tileStates[index] = selectedState;
            var tile = _levelManager.Tiles[index];
            tile.GotoState(selectedState, true);
        }
        else
        {
            tileStates[index] = Tile.State.Wall;
            var tile = _levelManager.Tiles[index];
            tile.GotoState(Tile.State.Wall, true);
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
        var level = new Level();
        var state = new LevelState(tileStates);

        level.AddState(state);
        gameManager.levelManager = _levelManager;
        gameManager.StartGame(level);
    }
}
