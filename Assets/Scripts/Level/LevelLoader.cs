using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
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
        { 'x', Tile.State.Spike },
		{ '+', Tile.State.LaserCannon },
		{ '_', Tile.State.Wall }
    };

    private static Dictionary<Tile.State, char> stateToChar;

    public static Level ParseLevel(string levelText)
    {
        var tileIndex = 0;
        var pickupIndex = 0;
        var playerStartIndex = -1;

        var level = new Level();
        var state = new Tile.State[100];

        foreach (var c in levelText)
        {
            if (c < ' ') continue;

            Tile.State tileState;
            if (charToState.ContainsKey(char.ToLower(c)))
            {
                tileState = charToState[char.ToLower(c)];
            }
            else
            {
                tileState = Tile.State.Wall;
            }

            if (tileState == Tile.State.PlayerStart)
            {
                playerStartIndex = tileIndex;
            }

            state[tileIndex] = tileState;

            if (++tileIndex >= state.Length) {

                level.AddState(new LevelState(state, pickupIndex, playerStartIndex));

                tileIndex = 0;
                pickupIndex = 0;
                playerStartIndex = 0;

                state = new Tile.State[100];
            }
        }

        return level;
    }

    public static string EncodeLevel(Level level)
    {
        if (stateToChar == null)
        {
            stateToChar = new Dictionary<Tile.State, char>();
            foreach (var pair in charToState)
            {
                stateToChar.Add(pair.Value, pair.Key);
            }
        }

        string encodedLevel = "";

        for (var stateIndex = 0; stateIndex < level.StatesCount; stateIndex++)
        {
            var tiles = level.GetState(stateIndex).tileStates;
            foreach (var tileState in tiles)
            {
                encodedLevel += stateToChar[tileState];
            }
        }

        return encodedLevel;
    }

    /// <summary>
    /// Load level from persistent data
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static Level LoadLevelFromFile(string fileName)
    {
        var path = Application.persistentDataPath + "/levels/" + fileName + ".mutolocus";

        if (File.Exists(path))
        {
            var text = File.ReadAllText(path);
            return ParseLevel(text);
        }
        else
        {
            Debug.Log("file doesn't exsist");
        }

        return null;

    }

    public static void SaveLevelToFile(string fileName, Level level)
    {
        var path = Application.persistentDataPath + "/levels/";

        Directory.CreateDirectory(path);

        path = path + fileName + ".mutolocus";
        var text = EncodeLevel(level);

        Debug.Log("Saved level to " + path);

        File.WriteAllText(path, text);
    }

    public static List<string> GetLevels()
    {
        var path = Application.persistentDataPath + "/levels/";

        Debug.Log(path);

        return Directory.GetFiles(path)
            .Where(s => Path.GetExtension(s) == ".mutolocus")
            .Select(Path.GetFileNameWithoutExtension).ToList();
    }
}
