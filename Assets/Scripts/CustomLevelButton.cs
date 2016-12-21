using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomLevelButton : MonoBehaviour
{
    public delegate void PressedHandler(string fileName);

    public static event PressedHandler buttonPressed;

    public Text nameLabel;
    public Text authorLabel;

    private FilePicker filePicker;
    private string fileName;

    private List<CustomLevelButton> levelButtons = new List<CustomLevelButton>();

    public void Setup(string fileName, Level level)
    {
        nameLabel.text = string.IsNullOrEmpty(level.name) ? fileName : level.name;
        authorLabel.text = string.IsNullOrEmpty(level.author) ? "Unknown" : level.author;
        this.fileName = fileName;
    }

    public void Pressed()
    {
        if (buttonPressed != null)
        {
            buttonPressed(fileName);
        }
    }
}
