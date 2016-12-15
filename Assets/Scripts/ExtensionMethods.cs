using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    public static void SetVisibility(this CanvasGroup canvasGroup, bool show)
    {
        canvasGroup.interactable = show;
        canvasGroup.blocksRaycasts = show;
        canvasGroup.alpha = show ? 1 : 0;
    }
}
