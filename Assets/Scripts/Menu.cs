using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public delegate void PlayPressedHandler();

    public static PlayPressedHandler OnPlayPressed;

    private CanvasGroup _canvasGroup;

    private void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void PlayGame()
    {
        if (OnPlayPressed != null)
        {
            OnPlayPressed();
        }

        _canvasGroup.alpha = 0;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
    }
}
