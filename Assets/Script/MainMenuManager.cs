using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject mainMenu;
    public GameObject gameUI;
    public Button[] menuButtons;

    [Header("Camera")]
    public Camera mainCamera;
    public CameraController cameraController;

    public float startSize = 8f;
    public float gameSize = 5f;
    public float zoomDuration = 1.5f;

    [Header("Player")]
    public PlayerMovement playerMovement;

    [Header("NPC")]
    public NPCManager npcManager;

    [Header("Guideline Control")]
    public ControlGuide controlGuide;

    [Header("Legacy Main Menu Scene")]
    public Transform focusPoint;
    public float targetSize = 3f;
    public float moveDuration = 1.5f;
    public string nextSceneName = "TutorialStage";

    private PanelButtonNavigator menuNavigator;
    private bool isStarting;

    void Start()
    {
        if (gameUI == null)
            gameUI = FindSceneObject("UI");

        if (mainCamera != null && mainMenu != null)
            mainCamera.orthographicSize = startSize;

        if (playerMovement != null)
        {
            playerMovement.canMove = false;
            playerMovement.SetMenuActive(true);
        }

        if (cameraController != null)
            cameraController.canFollow = false;

        if (mainMenu != null)
            mainMenu.SetActive(true);

        SetupMainMenuKeyboard();

        if (gameUI != null)
            gameUI.SetActive(false);
    }

    public void StartGame()
    {
        if (isStarting)
            return;

        if (mainMenu != null && cameraController != null && playerMovement != null)
            StartCoroutine(StartSequence());
        else
            StartCoroutine(LegacyStartSequence());
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    IEnumerator StartSequence()
    {
        isStarting = true;

        if (gameUI == null)
            gameUI = FindSceneObject("UI");

        if (mainMenu != null)
            mainMenu.SetActive(false);

        if (gameUI != null)
            gameUI.SetActive(true);

        // Munculkan NPC & Tetua di posisi random
        if (npcManager != null)
            npcManager.SpawnNPCs();

        // Kamera langsung mulai mengikuti player
        if (cameraController != null)
            cameraController.canFollow = true;

        float time = 0f;

        while (time < zoomDuration)
        {
            if (mainCamera != null)
            {
                mainCamera.orthographicSize =
                    Mathf.Lerp(startSize, gameSize, time / zoomDuration);
            }

            time += Time.deltaTime;

            yield return null;
        }

        if (mainCamera != null)
            mainCamera.orthographicSize = gameSize;

        // Setelah kamera fokus ke Asep, tampilkan panduan kontrol.
        // Player belum bisa bergerak sampai panduan ditutup pemain.
        if (controlGuide != null)
        {
            controlGuide.Show();

            while (controlGuide.IsShowing)
                yield return null;
        }

        // Panduan sudah ditutup -> player bisa bergerak
        if (playerMovement != null)
        {
            playerMovement.canMove = true;
            playerMovement.SetMenuActive(false);
        }
    }

    private IEnumerator LegacyStartSequence()
    {
        isStarting = true;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainMenu != null)
            mainMenu.SetActive(false);

        if (mainCamera == null || focusPoint == null)
        {
            LoadNextSceneIfSet();
            yield break;
        }

        Vector3 startPosition = mainCamera.transform.position;
        Vector3 targetPosition = new Vector3(focusPoint.position.x, focusPoint.position.y, startPosition.z);
        float startZoom = mainCamera.orthographicSize;
        float time = 0f;

        while (time < moveDuration)
        {
            float progress = time / moveDuration;
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
            mainCamera.orthographicSize = Mathf.Lerp(startZoom, targetSize, progress);
            time += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = targetPosition;
        mainCamera.orthographicSize = targetSize;

        LoadNextSceneIfSet();
    }

    private void LoadNextSceneIfSet()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    private void SetupMainMenuKeyboard()
    {
        if (menuButtons == null || menuButtons.Length == 0)
            menuButtons = mainMenu != null
                ? mainMenu.GetComponentsInChildren<Button>(true)
                : FindSceneButtons();

        GameObject navigatorHost = mainMenu != null ? mainMenu : gameObject;

        menuNavigator = navigatorHost.GetComponent<PanelButtonNavigator>();
        if (menuNavigator == null)
            menuNavigator = navigatorHost.AddComponent<PanelButtonNavigator>();

        menuNavigator.buttons = menuButtons;
        menuNavigator.Setup();

        BindMenuButtons();
    }

    private Button[] FindSceneButtons()
    {
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();
        int count = 0;

        for (int i = 0; i < allButtons.Length; i++)
        {
            if (allButtons[i] != null && allButtons[i].gameObject.scene.IsValid())
                count++;
        }

        Button[] sceneButtons = new Button[count];
        int index = 0;

        for (int i = 0; i < allButtons.Length; i++)
        {
            if (allButtons[i] != null && allButtons[i].gameObject.scene.IsValid())
                sceneButtons[index++] = allButtons[i];
        }

        return sceneButtons;
    }

    private void BindMenuButtons()
    {
        if (menuButtons == null)
            return;

        for (int i = 0; i < menuButtons.Length; i++)
        {
            Button button = menuButtons[i];
            if (button == null)
                continue;

            string buttonName = button.gameObject.name.ToLowerInvariant();
            if (buttonName.Contains("exit") || buttonName.Contains("quit") || buttonName.Contains("keluar"))
                button.onClick.AddListener(QuitGame);
            else if (buttonName.Contains("start") || buttonName.Contains("play") || buttonName.Contains("main"))
                button.onClick.AddListener(StartGame);
        }
    }

    private GameObject FindSceneObject(string objectName)
    {
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in objects)
        {
            if (obj.name == objectName && obj.scene.IsValid())
                return obj;
        }

        return null;
    }
}
