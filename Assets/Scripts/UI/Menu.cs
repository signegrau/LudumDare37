using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public delegate void PlayPressedHandler();
    public static PlayPressedHandler OnPlayPressed;

    public CanvasGroup titleMenu;
    public CanvasGroup customLevelMenu;

    public Button customLevelButton;
    public Button editorButton;

    public Text customEditorWebExplainer;

    public Button quitButton;

    public RectTransform levelButtonContainer;
    public GameObject levelButtonPrefab;
    public GameObject noLevelText;

    private List<CustomLevelButton> levelButtons = new List<CustomLevelButton>();

    private string levelToLoad;

    private void Start()
    {
        titleMenu.SetVisibility(true);
        customLevelMenu.SetVisibility(false);

#if UNITY_STANDALONE
        quitButton.gameObject.SetActive(true);
        customLevelButton.interactable = false;
        editorButton.interactable = false;
        customEditorWebExplainer.gameObject.SetActive(false);
#else
        quitButton.gameObject.SetActive(false);
        customLevelButton.interactable = false;
        editorButton.interactable = false;
        customEditorWebExplainer.gameObject.SetActive(true);
#endif
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    public void GotoCustomLevelMenu()
    {
        titleMenu.SetVisibility(false);
        customLevelMenu.SetVisibility(true);

        if (levelButtons.Count > 0)
        {
            foreach (var button in levelButtons)
            {
                Destroy(button.gameObject);
            }
            levelButtons.Clear();
        }

        var files = LevelLoader.GetLevels();

        foreach (var file in files)
        {
            var itemGameObject = Instantiate(levelButtonPrefab, levelButtonContainer);
            itemGameObject.transform.localScale = Vector3.one;
            var item = itemGameObject.GetComponent<CustomLevelButton>();
            var level = LevelLoader.LoadLevelFromFile(file);
            item.Setup(file, level);
            levelButtons.Add(item);
        }
    }

    public void GotoTitleMenu()
    {
        titleMenu.SetVisibility(true);
        customLevelMenu.SetVisibility(false);
    }

    public void GotoEditor()
    {
        SceneManager.LoadScene("Editor", LoadSceneMode.Single);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void OnEnable()
    {
        CustomLevelButton.buttonPressed += OnCustomLevelButtonPressed;
        ImportDialog.closed += GotoCustomLevelMenu;
    }

    private void OnDisable()
    {
        CustomLevelButton.buttonPressed -= OnCustomLevelButtonPressed;
        ImportDialog.closed -= GotoCustomLevelMenu;
    }

    private void OnCustomLevelButtonPressed(string fileName)
    {
        Debug.Log("Start level: " + fileName);
        var level = LevelLoader.LoadLevelFromFile(fileName);

        // One of the least elegent solution in this project imo
        GameManager.loadLevel = level;
        // End

        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    public void OpenSubreddit()
    {
        Application.OpenURL("https://reddit.com/r/MutoLocus");
    }

    public void OpenItchIo()
    {
        Application.OpenURL("https://mechagk.itch.io/muto-locus");
    }
}
