using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public TextAsset statesFile;
    public UnityEngine.Object levelScene;
    public LevelManager levelManager;

    public delegate void GameStartHandler(float time);
    public static event GameStartHandler OnGameStart;

    public delegate void GameEndHandler(float time, int deaths);
    public static event GameEndHandler OnGameEnd;

    private float startTime;
    private int countDeath;

    public GameObject playerPrefab;
    private Transform player;

    private bool gameStarting;

    private void Start()
    {
        LoadLevel();
    }

    private void LoadLevel()
    {
        var scene = SceneManager.GetSceneByName(levelScene.name);

        if (scene.isLoaded) return;

        SceneManager.LoadScene(levelScene.name, LoadSceneMode.Additive);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.name == levelScene.name)
        {
            levelManager = FindObjectOfType<LevelManager>();
        }
    }

    private void OnEnable() {
        Pickup.OnPickup += AdvanceState;
        Menu.OnPlayPressed += StartGame;
        PlayerScript.OnDeath += OnPlayerDeath;
        EndScreen.OnPlayAgainPressed += RestartGame;
        LevelManager.OnStateChanged += OnStateChanged;
        LevelManager.OnNoStatesLeft += OnNoStatesLeft;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        Pickup.OnPickup -= AdvanceState;
        Menu.OnPlayPressed -= StartGame;
        PlayerScript.OnDeath -= OnPlayerDeath;
        EndScreen.OnPlayAgainPressed -= RestartGame;
        LevelManager.OnStateChanged -= OnStateChanged;
        LevelManager.OnNoStatesLeft -= OnNoStatesLeft;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnNoStatesLeft()
    {
        if (OnGameEnd != null)
        {
            OnGameEnd(Time.time - startTime, countDeath);
        }
    }

    private void OnStateChanged(int index)
    {
        if (gameStarting)
        {
            player.transform.position = (Vector2)levelManager.PlayerStartPosition();
            player.gameObject.SetActive(true);
            gameStarting = false;
        }
    }

    private void OnPlayerDeath()
    {
        countDeath++;
    }

    private void AdvanceState()
    {
        levelManager.AdvanceState();
    }

    public void StartGame()
    {
        LoadLevel();
        gameStarting = true;
        player = Instantiate(playerPrefab).transform;
        player.gameObject.SetActive(false);
        player.transform.position = new Vector3(20, -20, 0);

        StartCoroutine(levelManager.Setup(statesFile.text, true));
    }

    private void RestartGame()
    {
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }
}
