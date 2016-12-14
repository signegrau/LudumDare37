using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Editor : MonoBehaviour
{
    private LevelManager _levelManager;
    private Tile.State selectedState = Tile.State.Platform;

    private Tile.State[] tileStates = new Tile.State[100];

    private void Start()
    {
        LoadLevel();
    }

    private void OnEnable()
    {
        Tile.tilePressed += OnTilePressed;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        Tile.tilePressed -= OnTilePressed;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnTilePressed(int index, int mouseButton)
    {
        if (mouseButton == 0)
        {
            tileStates[index] = selectedState;
            var tile = _levelManager.Tiles[index];
            tile.GotoState(selectedState);
        }
        else
        {
            tileStates[index] = Tile.State.Wall;
            var tile = _levelManager.Tiles[index];
            tile.GotoState(Tile.State.Wall);
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
}
