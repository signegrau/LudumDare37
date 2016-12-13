using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class LevelLoader
{
    private static readonly Dictionary<char, Tile.State> charToState = new Dictionary<char, Tile.State>
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

    public static List<Tile.State[]> LoadLevel(string levelText)
    {
        int tileIndex = 0;
        Tile.State tileState;

        var level = new List<Tile.State[]>();
        var state = new Tile.State[100];

        foreach (char c in levelText) {
            if (c < ' ') continue;

            if (charToState.ContainsKey(char.ToLower(c)))
            {
                tileState = charToState[char.ToLower(c)];
            }
            else
            {
                tileState = Tile.State.Wall;
            }

            state[tileIndex] = tileState;

            if (++tileIndex >= state.Length) {
                tileIndex = 0;
                level.Add(state);
                state = new Tile.State[100];
            }
        }

        return level;
    }

    /// <summary>
    /// Load level from persistent data
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static List<Tile.State[]> LoadLevelFromFile(string fileName)
    {
        string path = Application.persistentDataPath + "\\" + fileName;

        string text = System.IO.File.ReadAllText(path);

        return LoadLevel(text);
    }
}
