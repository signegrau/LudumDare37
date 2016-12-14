using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level
{
    private List<LevelState> states = new List<LevelState>();

    public int StatesCount
    {
        get { return states.Count; }
    }

    public int GetPlayerStartIndex(int stateIndex)
    {
        if (states[stateIndex].HasStartPosition)
        {
            return states[stateIndex].PlayerStartIndex;
        }

        return states[stateIndex - 1].PickupPosition;
    }

    public void AddState(LevelState state)
    {
        states.Add(state);
    }

    public LevelState GetState(int index)
    {
        return states[index];
    }

    public void StatesFindSpecialIndexes()
    {
        foreach (var state in states)
        {
            state.FindSpecialIndexes();
        }
    }

    public bool HasState(int index)
    {
        return index < states.Count;
    }
}
