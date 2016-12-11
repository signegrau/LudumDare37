using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject playerPrefab;
    private PlayerScript player;
    private Vector3 playerStartPosition;

    List<Tile.State[]> allLevelStates;
    private int currentStateIndex;

    public TextAsset statesFile;

    TileGenerator tileGenerator;
    Tile[] tiles;

	void OnEnable() {
		Pickup.OnPickup += AdvanceState;
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
                case '#': s = Tile.State.Platform; break;
                case '*': s = Tile.State.Pickup; break;
                case 's':
                case 'S':
                    s = Tile.State.Spring;
                    break;
                 case '@':
                    s = Tile.State.PlayerStart;
                    break;
                default: s = Tile.State.Wall; break;
            }
            currentState[tileIndex] = s;

            Debug.Log(currentState.Length);
            if (++tileIndex >= currentState.Length) {
                tileIndex = 0;
                allLevelStates.Add(currentState);
                currentState = new Tile.State[100];
            }
        }

        player = Instantiate(playerPrefab).GetComponent<PlayerScript>();
        player.transform.position = new Vector3(20, 20, 0);

        AdvanceState();
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.U)) {
            AdvanceState();
        }
    }
    
    public void AdvanceState()
    {
        var hasPlayerSpawn = false;

        Debug.Log(currentStateIndex);
        Debug.Log(allLevelStates.Count);
        if (currentStateIndex < allLevelStates.Count) {
            Debug.Log("kage!!");
            Tile.State[] newState = allLevelStates[currentStateIndex++];

            for(int i = 0; i < newState.Length; ++i) {
                Tile.State tileState = newState[i];
                Tile tile = tiles[i];

                if (tileState == Tile.State.PlayerStart)
                {
                    hasPlayerSpawn = true;
                    playerStartPosition = tile.transform.position + new Vector3(0, 0, -1);
                }

                tile.GotoState(tileState);
            }
        }
        else {
            // WE win!
        }

        if (hasPlayerSpawn)
        {
            player.transform.position = playerStartPosition;
        }
    }
}
