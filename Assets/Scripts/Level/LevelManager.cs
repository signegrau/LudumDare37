using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
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

    public int CurrentStateIndex
    {
        get { return currentStateIndex; }
    }

    

    TileGenerator tileGenerator;
    private Tile[] tiles;

    private bool isChanging;

    private int countDeath;
    private bool hasGenerated;

    public Tile[] Tiles
    {
        get { return tiles; }
    }

    private void Start ()
    {
        tileGenerator = GetComponent<TileGenerator>();
    }

    public IEnumerator Setup(Level level = null, int firstState = -1)
    {
        yield return null;

        if (!hasGenerated)
        {
            tiles = tileGenerator.GenerateTiles();
            hasGenerated = true;
        }

    currentStateIndex = 0;

        this.level = level;

        yield return null;

        if (firstState > -1)
        {
            currentStateIndex = firstState;
            ChangeState(firstState);
        }
    }
    
    public void AdvanceState()
    {
        ++currentStateIndex;
        if (!ChangeState(currentStateIndex))
        {
            currentStateIndex--;

            if (OnNoStatesLeft != null)
                OnNoStatesLeft();

            Debug.Log("No states left!");
        }
    }

    public void ChangeState(Tile.State[] tileStates, bool asEditor = false)
    {
        for (var i = 0; i < tileStates.Length; ++i)
        {
            var tileState = tileStates[i];
            var tile = tiles[i];

            tile.GotoState(tileState, asEditor);
        }
        SoundManager.single.PlayAdvanceSound();

        if (OnStateChanged != null)
        {
            OnStateChanged(currentStateIndex);
        }
    }

    public void ChangeState(LevelState state, bool asEditor = false)
    {
        ChangeState(state.tileStates, asEditor);
    }

    public bool ChangeState(int stateIndex, bool asEditor = false)
    {
        if (stateIndex < level.StatesCount)
        {
            ChangeState(level.GetState(stateIndex), asEditor);
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
