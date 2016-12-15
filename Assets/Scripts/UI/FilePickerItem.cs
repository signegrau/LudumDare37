using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FilePickerItem : MonoBehaviour
{
    public delegate void PressedHandler(string fileName);

    public static event PressedHandler itemPressed;

    public Text label;
    public Image background;
    public Color selectedColor;
    private FilePicker filePicker;
    private string fileName;

    private void OnEnable()
    {
        FilePicker.fileSelected += FilePickerOnFileSelected;
    }

    private void FilePickerOnFileSelected(string fileName)
    {
        SetSelected(fileName == this.fileName);
    }

    private void OnDisable()
    {
        FilePicker.fileSelected -= FilePickerOnFileSelected;
    }

    public void Setup(FilePicker filePicker, string fileName)
    {
        this.filePicker = filePicker;

        label.text = fileName;
        this.fileName = fileName;
    }

    public void SetSelected(bool selected)
    {
        background.color = selected ? selectedColor : Color.white;
    }

    public void Select()
    {
        SetSelected(true);
        if (itemPressed != null)
        {
            itemPressed(fileName);
        }
    }
}
