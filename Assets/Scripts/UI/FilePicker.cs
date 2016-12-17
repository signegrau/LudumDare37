using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FilePicker : MonoBehaviour
{
    public delegate void FileSelectedHandler(string fileName);
    public static event FileSelectedHandler fileSelected;

    public delegate void FileChoosenHandler(string fileName);
    public static event FileChoosenHandler fileChoosen;

    public Text title;
    public Button button;
    public InputField inputField;
    public RectTransform scrollViewContent;
    public GameObject listItemPrefab;
    public OverwritingDialog overwritingDialog;
    private List<FilePickerItem> items = new List<FilePickerItem>();

    private Text buttonText;
    private CanvasGroup _canvasGroup;
    private string fileName;

    private bool asLoad;


    private void OnEnable()
    {
        FilePickerItem.itemPressed += OnItemPressed;
    }

    private void OnDisable()
    {
        FilePickerItem.itemPressed -= OnItemPressed;
    }

    private void OnItemPressed(string fileName)
    {
        inputField.text = fileName;
        this.fileName = fileName;

        if (fileSelected != null)
            fileSelected(fileName);
    }

    private void Start()
    {
        buttonText = button.transform.GetComponentInChildren<Text>();
        _canvasGroup = GetComponent<CanvasGroup>();

        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        _canvasGroup.SetVisibility(false);
    }

    public void Show(bool asLoad)
    {
        this.asLoad = asLoad;
        _canvasGroup.SetVisibility(true);

        inputField.interactable = !asLoad;
        buttonText.text = asLoad ? "Load" : "Save";
        title.text = asLoad ? "Load Level" : "Save Level";

        if (items.Count > 0)
        {
            foreach (var item in items)
            {
                Destroy(item.gameObject);
            }
            items.Clear();
        }

        var files = LevelLoader.GetLevels();

        foreach (var file in files)
        {
            Debug.Log(file);
            var itemGameObject = Instantiate(listItemPrefab, scrollViewContent);
            var item = itemGameObject.GetComponent<FilePickerItem>();
            item.Setup(this, file);
            items.Add(item);
        }
    }

    public void Hide()
    {
        _canvasGroup.SetVisibility(false);
    }

    public void SelectFile(string fileName)
    {

    }

    public void ButtonPressed()
    {
        fileName = inputField.text;

        if (string.IsNullOrEmpty(fileName))
        {
            fileName = null;
        }
        else
        {
            if (LevelLoader.LevelFileExists(fileName) && !asLoad)
            {
                OverwritingDialog.result += OverwritingDialogOnResult;
                overwritingDialog.Show(fileName);
                return;
            }
        }

        if (fileChoosen != null)
        {
            fileChoosen(fileName);
        }

        Hide();
    }

    private void OverwritingDialogOnResult(bool overwrite)
    {
        if (overwrite)
        {
            if (fileChoosen != null)
            {
                fileChoosen(fileName);
            }

            Hide();
        }

        OverwritingDialog.result -= OverwritingDialogOnResult;
    }

    public void ClosePressed()
    {
        if (fileChoosen != null)
        {
            fileChoosen(null);
        }

        Hide();
    }
}
