using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public bool isEditor = false;

    public delegate void GameStartHandler(float time);

    public static event GameStartHandler OnGameStart;

    public delegate void GameEndHandler(float time, int deaths);

    public static event GameEndHandler OnGameEnd;

    private float startTime;

    public GameObject playerPrefab;
    private Transform player;
    private Vector3 playerStartPosition;

    private Vector3 pickupPosition;
    private Vector3 previousPickupPosition;

    List<Tile.State[]> allLevelStates;
    private int currentStateIndex;

    public TextAsset statesFile;

    TileGenerator tileGenerator;
    Tile[] tiles;

    private bool isChanging;

    private int countDeath;



	void OnEnable() {
		Pickup.OnPickup += AdvanceState;
	    Menu.OnPlayPressed += AdvanceState;
	    PlayerScript.OnDeath += OnPlayerDeath;
        EndScreen.OnPlayAgainPressed += RestartGame; 
	}

    void OnDisable()
    {
        Pickup.OnPickup -= AdvanceState;
        Menu.OnPlayPressed -= AdvanceState;
        PlayerScript.OnDeath -= OnPlayerDeath;
        EndScreen.OnPlayAgainPressed -= RestartGame;
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(0);
    }

    private void OnPlayerDeath()
    {
        countDeath++;
    }

    // Use this for initialization
    private IEnumerator Start () {
        tileGenerator = GetComponent<TileGenerator>();
        tiles = tileGenerator.GenerateTiles();

        yield return null;

        currentStateIndex = 0;

        allLevelStates = LevelLoader.LoadLevel(statesFile.text);

        player = Instantiate(playerPrefab).transform;
        player.gameObject.SetActive(false);
        player.transform.position = new Vector3(20, -20, 0);

        AdvanceState();
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.U) && !isChanging)
        {
            StartCoroutine(PlayerGotoNextState());
        }
    }

    private IEnumerator PlayerGotoNextState()
    {
        isChanging = true;

        player.transform.position = new Vector3(pickupPosition.x, pickupPosition.y, 0);
        yield return new WaitForSeconds(0.1f);

        isChanging = false;
    }
    
    public void AdvanceState()
    {
        if (ChangeState(currentStateIndex, !isEditor))
        {
            ++currentStateIndex;
        }
        else if (OnGameEnd != null)
        {
            OnGameEnd(Time.time - startTime, countDeath);
        }
    }

    public void ChangeState(Tile.State[] state, bool movePlayer)
    {
        bool hasPlayerSpawn = false;

        for(var i = 0; i < state.Length; ++i) {
            var tileState = state[i];
            var tile = tiles[i];

            if (tileState == Tile.State.PlayerStart)
            {
                hasPlayerSpawn = true;
                playerStartPosition = tile.transform.position + new Vector3(0, 0, 0);
            }
            else if (tileState == Tile.State.Pickup)
            {
                previousPickupPosition = pickupPosition;
                pickupPosition = tile.transform.position;
            }


            tile.GotoState(tileState);
        }
        SoundManager.single.PlayAdvanceSound();

        if (movePlayer && hasPlayerSpawn)
        {
            startTime = Time.time;
            if (OnGameStart != null)
            {
                OnGameStart(startTime);
            }

            player.transform.position = playerStartPosition;
            player.gameObject.SetActive(true);
        }
    }

    public bool ChangeState(int stateIndex, bool movePlayer)
    {
        if (stateIndex < allLevelStates.Count)
        {
            ChangeState(allLevelStates[stateIndex], movePlayer);
            return true;
        }

        return false;
    }
}
