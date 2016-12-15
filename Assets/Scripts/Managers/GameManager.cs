using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public TextAsset statesFile;
//	  Does not work in build:
//    public UnityEngine.Object levelScene;
	public String levelSceneName;
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
		var scene = SceneManager.GetSceneByName(levelSceneName);

        if (scene.isLoaded) return;

		SceneManager.LoadScene(levelSceneName, LoadSceneMode.Additive);
        //SceneManager.LoadScene(levelScene.name, LoadSceneMode.Additive);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
		if (scene.name == levelSceneName)
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
    public void StartGame(Level level = null, int index = 0)
    {
        StartCoroutine(StartGameAsync(level, index));
    }

    public IEnumerator StartGameAsync(Level level = null, int index = 0)
    {
        if (level == null)
        {
            level = LevelLoader.ParseLevel(statesFile.text);
        }

        gameStarting = true;
        if (levelManager == null)
        {
            LoadLevel();
        }

        while (levelManager == null)
        {
            yield return null;
        }

        if (player == null)
        {
            player = Instantiate(playerPrefab).transform;
            player.gameObject.SetActive(false);
            player.transform.position = new Vector3(20, -20, 0);
        }

        yield return StartCoroutine(levelManager.Setup(level, index));

        gameStarting = false;
        player.transform.position = (Vector2)levelManager.PlayerStartPosition();
        player.gameObject.SetActive(true);

        startTime = Time.time;
        countDeath = 0;

        if (OnGameStart != null)
        {
            OnGameStart(startTime);
        }
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
