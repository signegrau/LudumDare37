using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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

    public class LevelStruct
    {
        public string name { get; set; }
        public string author { get; set; }
        public string levelData { get; set; }
    }

    private static Dictionary<Tile.State, char> stateToChar;

    private static string levelSubFolder = "/levels/";
    private static string levelFileExtension = ".mutolocus";

    public static string levelFolderPath
    {
        get { return Application.persistentDataPath + levelSubFolder;  }
    }

    public static Level ParseLevel(string levelText)
    {
        Level level;

        if (levelText.StartsWith("name"))
        {
            var input = new StringReader(levelText);
            var deserializer = new Deserializer(namingConvention: new HyphenatedNamingConvention());

            var levelStruct = deserializer.Deserialize<LevelStruct>(input);

            level = ParseLevelData(levelStruct.levelData);
            level.name = levelStruct.name;
            level.author = levelStruct.author;
        }
        else
        {
            level = ParseLevelData(levelText);
        }

        return level;
    }

    public static Level ParseLevelData(string levelText)
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

            encodedLevel += "\n";
        }

        var levelStruct = new LevelStruct{
            name = level.name,
            author = level.author,
            levelData = encodedLevel
        };

        var serializer = new Serializer(SerializationOptions.None, new HyphenatedNamingConvention());
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);
        serializer.Serialize(stringWriter, levelStruct);

        return stringBuilder.ToString();
    }

    public static string GetFullPath(string fileName)
    {
        var path = levelFolderPath + fileName;

        if (Path.GetExtension(path) == levelFileExtension)
        {
            return path;
        }

        return path + levelFileExtension;
    }

    /// <summary>
    /// Load level from persistent data
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static Level LoadLevelFromFile(string fileName)
    {
        #if UNITY_WEBGL
        if (PlayerPrefs.HasKey("lvl_" + fileName)) {
            var text = PlayerPrefs.GetString("lvl_" + fileName);
            return ParseLevel(text);
        }
        #else
        var path = GetFullPath(fileName);

        if (File.Exists(path))
        {
            var text = File.ReadAllText(path);
            return ParseLevel(text);
        }
        #endif

        Debug.Log("file doesn't exist");
        return null;
    }

    public static void SaveLevelToFile(string fileName, Level level)
    {
        var text = EncodeLevel(level);

        #if UNITY_WEBGL
        PlayerPrefs.SetString("lvl_" + fileName, text);

        if (PlayerPrefs.HasKey("levels")) {
            var levelsList = PlayerPrefs.GetString("levels");
            if (!levelsList.Contains(fileName)) {
                levelsList += fileName + ',';
            }

            PlayerPrefs.SetString("levels", levelsList);
        }
        else {
            PlayerPrefs.SetString("levels", fileName + ',');
        }
        
        PlayerPrefs.Save();
        #else
        var path = levelFolderPath;
        Directory.CreateDirectory(path);

        path = GetFullPath(fileName);
        

        Debug.Log("Saved level to " + path + "\n" + text);

        File.WriteAllText(path, text);
        #endif
    }

    public static void ImportLevel(string levelText)
    {
        var level = ParseLevel(levelText);

        var fileName = string.Format("{0}_{1}", level.name, "{0:000}");
        var testPath = GetFullPath(fileName);

        for (var i = 0; i < 999; i++)
        {
            if (!File.Exists(string.Format(testPath, i)))
            {
                fileName = string.Format(fileName, i);
                break;
            }
        }

        SaveLevelToFile(fileName, level);
    }

    public static List<string> GetLevels()
    {
        #if UNITY_WEBGL
        var levels = new List<String>();

        if (PlayerPrefs.HasKey("levels")) {
            var levelsString = PlayerPrefs.GetString("levels");
            levels = levelsString.Split(',').ToList();
            levels.RemoveAt(levels.Count - 1);
        }

        return levels;

        #else
        var path = levelFolderPath;

        if (!Directory.Exists(path))
        {
            Debug.Log("No level folder");
			return new List<String>();
        }

        return Directory.GetFiles(path)
            .Where(s => Path.GetExtension(s) == levelFileExtension)
            .Select(Path.GetFileNameWithoutExtension).ToList();
        #endif
    }

    public static bool LevelFileExists(string fileName)
    {
        #if UNITY_WEBGL
        return PlayerPrefs.HasKey("lvl_" + fileName);
        #else
        var path = GetFullPath(fileName);

        return File.Exists(path);
        #endif
    }
}
