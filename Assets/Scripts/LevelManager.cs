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

	void OnEnable() {
		Pickup.OnPickup += AdvanceState;
	    Menu.OnPlayPressed += AdvanceState;
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
            switch(c) {
                case '#':
                    s = Tile.State.Platform;
                    break;
                case '*':
                    s = Tile.State.Pickup;
                    break;
                case 's':
                case 'S':
                    s = Tile.State.Spring;
                    break;
                case 'u':
                case 'U':
                    s = Tile.State.BoostUp;
                    break;
                case '@':
                    s = Tile.State.PlayerStart;
                    break;
                case '+':
                    s = Tile.State.Spike;
                    break;
                default:
                    s = Tile.State.Wall;
                    break;
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

        player.gameObject.SetActive(false);
        AdvanceState();
        yield return new WaitForSeconds(1f);
        player.transform.position =
            new Vector3(previousPickupPosition.x, previousPickupPosition.y, 0);
        player.gameObject.SetActive(true);

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
