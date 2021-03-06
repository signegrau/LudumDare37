﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShareDialog : MonoBehaviour
{
    public delegate void CloseHandler();
    public static event CloseHandler closed;

    private CanvasGroup _canvasGroup;
    public InputField inputField;

    private string levelText = "";

    private void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        _canvasGroup.SetVisibility(false);

        inputField.onValueChanged.AddListener(s => inputField.text = levelText);
    }

    public void Show(string levelText)
    {
        this.levelText = levelText;

        inputField.text = levelText;
        _canvasGroup.SetVisibility(true);
    }

    public void Close()
    {
        if (closed != null)
        {
            closed();
        }

        _canvasGroup.SetVisibility(false);
    }
}
