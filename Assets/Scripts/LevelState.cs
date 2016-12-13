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

    public LevelState(Tile.State[] tileStates, int pickupIndex, int playerStartIndex = -1)
    {
        this.tileStates = tileStates;
        this.pickupIndex = pickupIndex;
        this.playerStartIndex = playerStartIndex;
    }
}
