using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel;

    private bool isPaused;
    private PanelButtonNavigator pauseNavigator;

    void Start()
    {
        SetupPauseKeyboardNavigation();
        BindPauseButtons();

        pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        SetupPauseKeyboardNavigation();

        Time.timeScale = 0f;

        isPaused = true;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);

        Time.timeScale = 1f;

        isPaused = false;
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void BindPauseButtons()
    {
        if (pausePanel == null)
            return;

        Button[] buttons = pausePanel.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            if (button == null)
                continue;

            string buttonName = button.gameObject.name.ToLowerInvariant();

            if (buttonName.Contains("main") || buttonName.Contains("resume"))
                button.onClick.AddListener(ResumeGame);
            else if (buttonName.Contains("exit") || buttonName.Contains("quit") || buttonName.Contains("keluar"))
                button.onClick.AddListener(QuitGame);
        }
    }

    private void SetupPauseKeyboardNavigation()
    {
        if (pausePanel == null)
            return;

        Button[] buttons = GetPauseButtonsInMenuOrder();

        pauseNavigator = pausePanel.GetComponent<PanelButtonNavigator>();
        if (pauseNavigator == null)
            pauseNavigator = pausePanel.AddComponent<PanelButtonNavigator>();

        pauseNavigator.buttons = buttons;
        pauseNavigator.sortButtonsTopToBottom = false;
        pauseNavigator.wrapSelection = false;
        pauseNavigator.useEventSystemSelection = false;
        pauseNavigator.Setup();
    }

    private Button[] GetPauseButtonsInMenuOrder()
    {
        Button[] allButtons = pausePanel.GetComponentsInChildren<Button>(true);
        Button resumeButton = null;
        Button exitButton = null;

        foreach (Button button in allButtons)
        {
            if (button == null)
                continue;

            string buttonName = button.gameObject.name.ToLowerInvariant();
            if (resumeButton == null && (buttonName.Contains("main") || buttonName.Contains("resume")))
                resumeButton = button;
            else if (exitButton == null && (buttonName.Contains("exit") || buttonName.Contains("quit") || buttonName.Contains("keluar")))
                exitButton = button;
        }

        if (resumeButton != null && exitButton != null)
            return new[] { resumeButton, exitButton };

        return allButtons;
    }
}
