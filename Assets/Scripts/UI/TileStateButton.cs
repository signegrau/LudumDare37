using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileStateButton : MonoBehaviour
{
    public delegate void PressedHandler(Tile.State state);

    public static event PressedHandler tileStateButtonPressed;

    public Image icon;
    public Image background;
    private Button button;

    [Serializable]
    public struct StateSprite
    {
        public Tile.State state;
        public Sprite sprite;
    }

    public List<StateSprite> stateSprites = new List<StateSprite>();
    private Dictionary<Tile.State, Sprite> stateToSprite = new Dictionary<Tile.State, Sprite>();

    private Tile.State state;

    private void Awake()
    {
        foreach (var stateSprite in stateSprites)
        {
            stateToSprite.Add(stateSprite.state, stateSprite.sprite);
        }

        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        tileStateButtonPressed += OnTileStateButtonPressed;
        Editor.selectedStateChanged += EditorOnSelectedStateChanged;
        Editor.stateChanged += EditorOnStateChanged;
    }

    private void EditorOnStateChanged(int index, int statesCount)
    {
        if (state == Tile.State.PlayerStart)
        {
            button.interactable = index == 0;
            icon.color = index == 0 ? Color.white : Color.gray;
        }
    }

    private void EditorOnSelectedStateChanged(Tile.State state)
    {
        if (state == this.state)
        {
            background.enabled = true;
        }
        else
        {
            background.enabled = false;
        }
    }

    private void OnDisable()
    {
        tileStateButtonPressed -= OnTileStateButtonPressed;
        Editor.selectedStateChanged -= EditorOnSelectedStateChanged;
        Editor.stateChanged -= EditorOnStateChanged;
    }

    private void OnTileStateButtonPressed(Tile.State state)
    {
        if (state != this.state)
        {
            background.enabled = false;
        }
    }

    public void SetState(Tile.State newState)
    {
        state = newState;
        icon.sprite = stateToSprite[newState];
    }

    public void OnPressed()
    {
        background.enabled = true;

        if (tileStateButtonPressed != null)
        {
            tileStateButtonPressed(state);
        }
    }
}
