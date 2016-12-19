using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    private class TimeInterval
    {
        public float startTime;
        public float endTime;

        public float ElapsedTime
        {
            get { return endTime - startTime; }
        }

        public TimeInterval(float startTime, float endTime)
        {
            this.startTime = startTime;
            this.endTime = endTime;
        }
    }

    public bool startGameAtLoad;
    public TextAsset statesFile;
//	  Does not work in build:
//    public UnityEngine.Object levelScene;
	public String levelSceneName;
    public LevelManager levelManager;

    public delegate void GameStartHandler(float time);
    public static event GameStartHandler OnGameStart;

    public delegate void GameEndHandler(float time, int deaths);
    public static event GameEndHandler OnGameEnd;

    public delegate void PauseHandler();
    public static event PauseHandler paused;

    public delegate void ResumeHandler(float timeStart, float timeAdd);
    public static event ResumeHandler resume;

    private float timeStart;
    private int countDeath;

    private List<TimeInterval> timeIntervals = new List<TimeInterval>();

    public GameObject playerPrefab;
    private Transform player;

    private bool gameStarting;
    private bool gameStarted;
    private bool isPaused;

    public static Level loadLevel;

    public float restartTimer;
    public Image restartIndicator;

    public static float StartTime
    {
        get { return instance.timeStart; }
    }

    public static float TimeElapsed
    {
        get { return instance.timeIntervals.Sum(t => t.ElapsedTime); }
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (startGameAtLoad)
        {
            if (loadLevel == null)
            {
                StartGame();
            }
            else
            {
                StartGame(loadLevel);
            }
        }
    }

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
        PlayerScript.OnDeath += OnPlayerDeath;
        EndScreen.playAgainPressed += RestartGame;
        EndScreen.mainMenuPressed += EndGame;
        LevelManager.OnStateChanged += OnStateChanged;
        LevelManager.OnNoStatesLeft += OnNoStatesLeft;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        Pickup.OnPickup -= AdvanceState;
        PlayerScript.OnDeath -= OnPlayerDeath;
        EndScreen.playAgainPressed -= RestartGame;
        EndScreen.mainMenuPressed -= EndGame;
        LevelManager.OnStateChanged -= OnStateChanged;
        LevelManager.OnNoStatesLeft -= OnNoStatesLeft;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnNoStatesLeft()
    {
        if (OnGameEnd != null)
        {
            OnGameEnd((Time.time - timeStart) + TimeElapsed, countDeath);
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

        timeStart = Time.time;
        countDeath = 0;

        gameStarted = true;
        isPaused = false;

        if (OnGameStart != null)
        {
            OnGameStart(timeStart);
        }
    }

    public int StopGame()
    {
        Destroy(player.gameObject);
        timeStart = 0;
        countDeath = 0;
        loadLevel = null;
        gameStarted = false;
        return levelManager.CurrentStateIndex;
    }

    private void RestartGame()
    {
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    private void EndGame()
    {
        loadLevel = null;
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    private void Update()
    {
        if (gameStarted)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
            }

            if (Input.GetKey(KeyCode.R))
            {
                if (restartTimer >= 0.5f)
                {
                    RestartGame();
                }

                restartTimer += Time.deltaTime;
                restartIndicator.fillAmount = restartTimer / 0.5f;
            }
            else
            {
                restartTimer = 0;
                restartIndicator.fillAmount = 0;
            }

        }
    }

    private void Pause()
    {
        isPaused = true;
        var timeInterval = new TimeInterval(timeStart, Time.time);
        timeIntervals.Add(timeInterval);

        if (paused != null)
        {
            paused();
        }
    }

    private void Resume()
    {
        isPaused = false;
        timeStart = Time.time;

        if (resume != null)
        {
            resume(timeStart, TimeElapsed);
        }
    }

    public static void InstanceResume()
    {
        instance.Resume();
    }

    public static void InstanceRestart()
    {
        instance.RestartGame();
    }

    public static void InstanceQuit()
    {
        instance.EndGame();
    }
}
