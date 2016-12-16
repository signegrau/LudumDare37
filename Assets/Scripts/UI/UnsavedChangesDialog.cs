﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnsavedChangesDialog : MonoBehaviour
{
    public delegate void ResultHandler(bool overwrite);

    public static event ResultHandler result;

    private CanvasGroup _canvasGroup;

    private void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        _canvasGroup.SetVisibility(false);
    }

    public void Show()
    {
        _canvasGroup.SetVisibility(true);
    }


    public void Cancel()
    {
        if (result != null)
        {
            result(false);
        }

        _canvasGroup.SetVisibility(false);
    }

    public void Confirm()
    {
        if (result != null)
        {
            result(true);
        }

        _canvasGroup.SetVisibility(false);
    }
}
