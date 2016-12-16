using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverwritingDialog : MonoBehaviour
{
    public delegate void ResultHandler(bool overwrite);

    public static event ResultHandler result;

    public Text textLabel;
    private string textLabelTemplate;
    private CanvasGroup _canvasGroup;

    private void Start()
    {
        textLabelTemplate = textLabel.text;
        _canvasGroup = GetComponent<CanvasGroup>();

        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        _canvasGroup.SetVisibility(false);
    }

    public void Show(string fileName)
    {
        textLabel.text = string.Format(textLabelTemplate, fileName);
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
