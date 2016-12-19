using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorStatePanel : MonoBehaviour
{
    public Text stateCountText;

    public Button firstStateButton;
    public Button previousStateButton;
    public Button nextStateButton;
    public Button lastStateButton;
    public Button deleteStateButton;

    private void OnEnable()
    {
        Editor.stateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        Editor.stateChanged -= OnStateChanged;
    }

    private void OnStateChanged(int index, int statesCount)
    {
        bool hasNextState = index + 1 < statesCount;
        bool isFirstState = index == 0;
        bool hasMultipleStates = statesCount > 1;

        nextStateButton.interactable = hasNextState;
        lastStateButton.interactable = hasNextState;

        previousStateButton.interactable = !isFirstState;
        firstStateButton.interactable = !isFirstState;

        deleteStateButton.interactable = hasMultipleStates;

        stateCountText.text = string.Format("State {0}/{1}", index + 1, statesCount);
    }
}
