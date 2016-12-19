using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomLevelLocationLabel : MonoBehaviour
{
    public InputField inputField;

    private void Start()
    {
        inputField.text = LevelLoader.levelFolderPath;
        inputField.onValueChanged.AddListener(s => inputField.text = LevelLoader.levelFolderPath);
    }
}
