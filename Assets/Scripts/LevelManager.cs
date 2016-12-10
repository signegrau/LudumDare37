using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

public class LevelManager : MonoBehaviour {

    Queue<Tile.State[]> allLevelStates;

    public TextAsset statesFile;

    TileGenerator tileGenerator;
    Tile[] tiles;

    private Dictionary<char, Tile.State> charToTileState = new Dictionary<char, Tile.State>() {
        // {' ', Tile.State.Empty},
        // {' ', Tile.State.Empty},
        // {' ', Tile.State.Empty},
        // {' ', Tile.State.Empty},
        // {' ', Tile.State.Empty},
        // {' ', Tile.State.Empty},
    };

    // Use this for initialization
    void Start () {
        tileGenerator = GetComponent<TileGenerator>();
        tiles = tileGenerator.GenerateTiles();

        allLevelStates = new Queue<Tile.State[]>();

        // load states from file
        TextAsset statesFile = (TextAsset)Resources.Load("all_states");
        string allStateCharacters = statesFile.text;

        var currentState = new Tile.State[100];

        int tileIndex = 0;
        foreach (char c in allStateCharacters) {
            if (c < ' ') continue;

            currentState[tileIndex] = charToTileState[c];

            if (++tileIndex > currentState.Length) {
                tileIndex = 0;
                allLevelStates.Enqueue(currentState);
                currentState = new Tile.State[100];
            }
        }

    }
    
    public void AdvanceState() {
        Tile.State[] newState = allLevelStates.Dequeue();

        for(int i = 0; i < newState.Length; ++i) {
            Tile.State tileState = newState[i];
            Tile tile = tiles[i];

            tile.GotoState(tileState);
        }
    }
}
