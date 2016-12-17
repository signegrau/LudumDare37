using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialogs : MonoBehaviour
{
    public FilePicker filePicker;

    private void Start()
    {
        if (filePicker == null)
        {
            GetComponentInChildren<FilePicker>();
        }
    }

    public void OpenFilePicker(bool asLoad)
    {
        filePicker.Show(asLoad);
    }

}
