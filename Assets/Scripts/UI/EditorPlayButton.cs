using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorPlayButton : MonoBehaviour
{
    public Text label;
    public string playStartText = "Play Level";
    public string playStopText = "Stop playing";

    private void Start()
    {
        //label.text = playStartText;
    }

    private void OnEnable()
    {
        Editor.playingStart += EditorOnPlayingStart;
        Editor.playingStop += EditorOnPlayingStop;
    }

    private void EditorOnPlayingStop()
    {
        //label.text = playStartText;
    }

    private void OnDisable()
    {
        Editor.playingStart -= EditorOnPlayingStart;
        Editor.playingStop -= EditorOnPlayingStop;
    }

    private void EditorOnPlayingStart()
    {
        //label.text = playStopText;
    }
}
