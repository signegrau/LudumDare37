using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public delegate void GameStartHandler(float time);

    public static event GameStartHandler OnGameStart;

    private float startTime;

    public GameObject playerPrefab;
    private PlayerScript player;
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

    private readonly Dictionary<char, Tile.State> charToState = new Dictionary<char, Tile.State>
    {
        { '#', Tile.State.Platform },
        { '*', Tile.State.Pickup },
        { 's', Tile.State.Spring },
        { 'u', Tile.State.BoostUp },
        { 'l', Tile.State.BoostLeft },
        { 'r', Tile.State.BoostRight },
        { '@', Tile.State.PlayerStart },
        { '+', Tile.State.Spike }
    };

	void OnEnable() {
		Pickup.OnPickup += AdvanceState;
	    Menu.OnPlayPressed += AdvanceState;
	    PlayerScript.OnDeath += OnPlayerDeath;
	}

    private void OnPlayerDeath()
    {
        countDeath++;
    }

    // Use this for initialization
    IEnumerator Start () {
        tileGenerator = GetComponent<TileGenerator>();
        tiles = tileGenerator.GenerateTiles();

        yield return null;

        allLevelStates = new List<Tile.State[]>();

        // load states from file
        // TextAsset statesFile = (TextAsset)Resources.Load("all_states");
        currentStateIndex = 0;
        string allStateCharacters = statesFile.text;

        var currentState = new Tile.State[100];

        int tileIndex = 0;
        foreach (char c in allStateCharacters) {
            if (c < ' ') continue;
            // Debug.Log(c);

            Tile.State s;

            if (charToState.ContainsKey(char.ToLower(c)))
            {
                s = charToState[char.ToLower(c)];
            }
            else
            {
                s = Tile.State.Wall;
            }

            currentState[tileIndex] = s;

            if (++tileIndex >= currentState.Length) {
                tileIndex = 0;
                allLevelStates.Add(currentState);
                currentState = new Tile.State[100];
            }
        }

        player = Instantiate(playerPrefab).GetComponent<PlayerScript>();
        player.gameObject.SetActive(false);
        player.transform.position = new Vector3(20, -20, 0);

        AdvanceState();
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.U) && !isChanging)
        {
            StartCoroutine(GotoNextState());
        }
    }

    private IEnumerator GotoNextState()
    {
        isChanging = true;

        player.transform.position = new Vector3(pickupPosition.x, pickupPosition.y, 0);
        yield return new WaitForSeconds(0.1f);

        isChanging = false;
    }
    
    public void AdvanceState()
    {
        var hasPlayerSpawn = false;

        if (currentStateIndex < allLevelStates.Count) {
            Tile.State[] newState = allLevelStates[currentStateIndex++];

            for(int i = 0; i < newState.Length; ++i) {
                Tile.State tileState = newState[i];
                Tile tile = tiles[i];

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
        }
        else {
            // WE win!
        }

        if (hasPlayerSpawn)
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
}
