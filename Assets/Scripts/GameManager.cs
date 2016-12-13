using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
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

    private void OnEnable() {
        Pickup.OnPickup += AdvanceState;
        Menu.OnPlayPressed += StartGame;
        PlayerScript.OnDeath += OnPlayerDeath;
        EndScreen.OnPlayAgainPressed += RestartGame;
        LevelManager.OnStateChanged += OnStateChanged;
        LevelManager.OnNoStatesLeft += OnNoStatesLeft;
    }

    private void OnDisable()
    {
        Pickup.OnPickup -= AdvanceState;
        Menu.OnPlayPressed -= AdvanceState;
        PlayerScript.OnDeath -= OnPlayerDeath;
        EndScreen.OnPlayAgainPressed -= RestartGame;
        LevelManager.OnStateChanged -= OnStateChanged;
        LevelManager.OnNoStatesLeft -= OnNoStatesLeft;
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
        gameStarting = true;
        player = Instantiate(playerPrefab).transform;
        player.gameObject.SetActive(false);
        player.transform.position = new Vector3(20, -20, 0);

        StartCoroutine(levelManager.Setup());
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(0);
    }
}
