using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomLevelButton : MonoBehaviour
{
    public delegate void PressedHandler(string fileName);

    public static event PressedHandler buttonPressed;

    public Text label;
    private FilePicker filePicker;
    private string fileName;

    private List<CustomLevelButton> levelButtons = new List<CustomLevelButton>();

    public void Setup(string fileName)
    {
        label.text = fileName;
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
