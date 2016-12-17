using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorStatePanel : MonoBehaviour
{
    public Button firstStateButton;
    public Button previousStateButton;
    public Button nextStateButton;
    public Button lastStateButton;

    private Image firstStateButtonImage;
    private Image previousStateButtonImage;
    private Image nextStateButtonImage;
    private Image lastStateButtonImage;

    public Sprite fastBackward;
    public Sprite fastForward;
    public Sprite leftArrow;
    public Sprite rightArrow;
    public Sprite plus;
    public Sprite duplicate;

    private void Awake()
    {
        firstStateButtonImage = firstStateButton.GetComponent<Image>();
        previousStateButtonImage = previousStateButton.GetComponent<Image>();
        nextStateButtonImage = nextStateButton.GetComponent<Image>();
        lastStateButtonImage = lastStateButton.GetComponent<Image>();
    }

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
        if (index + 1 < statesCount)
        {
            nextStateButtonImage.sprite = rightArrow;
            lastStateButtonImage.sprite = fastForward;
        }
        else
        {
            nextStateButtonImage.sprite = plus;
            lastStateButtonImage.sprite = duplicate;
        }

        previousStateButton.interactable = index != 0;
        firstStateButton.interactable = index != 0;
    }
}
