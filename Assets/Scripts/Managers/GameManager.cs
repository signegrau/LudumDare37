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
            if (gameStarting)
            {
                StartGameContinued(gameStartLevel, gameStartIndex);
            }
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

            startTime = Time.time;
            countDeath = 0;

            gameStarting = false;

            if (OnGameStart != null)
            {
                OnGameStart(startTime);
            }
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
        StartGame(null);
    }

    private Level gameStartLevel;
    private int gameStartIndex;
    public void StartGame(Level level = null, int index = 0)
    {
        gameStarting = true;
        if (levelManager == null)
        {
            gameStartIndex = index;
            gameStartLevel = level;
            LoadLevel();
        }
        else
        {
            StartGameContinued(level, index);
        }
    }

    private void StartGameContinued(Level level = null, int index = 0)
    {
        if (player == null)
        {
            player = Instantiate(playerPrefab).transform;
            player.gameObject.SetActive(false);
            player.transform.position = new Vector3(20, -20, 0);
        }

        if (level == null)
        {
            level = LevelLoader.ParseLevel(statesFile.text);
        }

        StartCoroutine(levelManager.Setup(level, index));
    }

    public int StopGame()
    {
        Destroy(player.gameObject);
        startTime = 0;
        countDeath = 0;
        return levelManager.CurrentStateIndex;
    }

    private void RestartGame()
    {
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }
}
