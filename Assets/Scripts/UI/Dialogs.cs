using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialogs : MonoBehaviour
{
    private CanvasGroup _canvasGroup;

    public FilePicker filePicker;

    private void OnEnable()
    {
        FilePicker.fileChoosen += FilePickerOnFileChoosen;
    }

    private void OnDisable()
    {
        FilePicker.fileChoosen -= FilePickerOnFileChoosen;
    }

    private void FilePickerOnFileChoosen(string fileName)
    {
        _canvasGroup.SetVisibility(false);
    }

    private void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (filePicker == null)
        {
            GetComponentInChildren<FilePicker>();
        }

        _canvasGroup.SetVisibility(false);
    }

    public void OpenFilePicker(bool asLoad)
    {
        _canvasGroup.SetVisibility(true);
        filePicker.Show(asLoad);
    }

}
