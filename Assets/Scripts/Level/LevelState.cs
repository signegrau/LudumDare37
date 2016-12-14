using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelState
{
    public Tile.State[] tileStates;
    private int playerStartIndex;
    private int pickupIndex;

    public bool HasStartPosition
    {
        get { return playerStartIndex > 0; }
    }

    public int PlayerStartIndex
    {
        get { return playerStartIndex; }
    }

    public int PickupPosition
    {
        get { return pickupIndex; }
    }

    public LevelState(int size = 100)
    {
        tileStates = new Tile.State[size];
        for (var i = 0; i < size; i++)
        {
            tileStates[i] = Tile.State.Wall;
        }
    }

    public LevelState(Tile.State[] tileStates)
    {
        this.tileStates = tileStates;
        FindSpecialIndexes();
    }

    public void FindSpecialIndexes()
    {
        for (var i = 0; i < tileStates.Length; i++)
        {
            if (tileStates[i] == Tile.State.Pickup)
            {
                pickupIndex = i;
            }
            else if (tileStates[i] == Tile.State.PlayerStart)
            {
                playerStartIndex = i;
            }
        }
    }

    public LevelState(Tile.State[] tileStates, int pickupIndex, int playerStartIndex = -1)
    {
        this.tileStates = tileStates;
        this.pickupIndex = pickupIndex;
        this.playerStartIndex = playerStartIndex;
    }
}
