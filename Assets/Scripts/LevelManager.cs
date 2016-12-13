using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public bool isEditor;

    public delegate void StateChangedHandler(int stateIndex);
    public static event StateChangedHandler OnStateChanged;

    public delegate void NoStatesLeftHandler();
    public static event NoStatesLeftHandler OnNoStatesLeft;

    Level level;
    private int currentStateIndex;

    public TextAsset statesFile;

    TileGenerator tileGenerator;
    Tile[] tiles;

    private bool isChanging;

    private int countDeath;

    private void Start ()
    {
        tileGenerator = GetComponent<TileGenerator>();
    }

    public IEnumerator Setup()
    {
        tiles = tileGenerator.GenerateTiles();
        currentStateIndex = 0;

        level = LevelLoader.LoadLevel(statesFile.text);

        yield return null;

        AdvanceState();
    }
    
    public void AdvanceState()
    {
        if (ChangeState(currentStateIndex, !isEditor))
        {
            if (OnStateChanged != null)
            {
                OnStateChanged(currentStateIndex);
            }

            ++currentStateIndex;
        }
        else if (OnNoStatesLeft != null)
        {
            OnNoStatesLeft();
        }
    }

    public void ChangeState(LevelState state)
    {
        for(var i = 0; i < state.tileStates.Length; ++i) {
            var tileState = state.tileStates[i];
            var tile = tiles[i];

            tile.GotoState(tileState);
        }
        SoundManager.single.PlayAdvanceSound();
    }

    public bool ChangeState(int stateIndex, bool movePlayer)
    {
        if (stateIndex < level.StatesCount)
        {
            ChangeState(level.GetState(stateIndex));
            currentStateIndex = stateIndex;
            return true;
        }

        return false;
    }

    public Vector3 PlayerStartPosition()
    {
        var index = level.GetPlayerStartIndex(currentStateIndex);
        return tiles[index].transform.position;
    }
}
