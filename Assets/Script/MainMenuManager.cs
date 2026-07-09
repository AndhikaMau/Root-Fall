using UnityEngine;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject mainMenu;
    public GameObject gameUI;

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

    void Start()
    {
        if (gameUI == null)
            gameUI = FindSceneObject("UI");

        mainCamera.orthographicSize = startSize;

        playerMovement.canMove = false;
        playerMovement.SetMenuActive(true);

        cameraController.canFollow = false;

        mainMenu.SetActive(true);

        if (gameUI != null)
            gameUI.SetActive(false);
    }

    public void StartGame()
    {
        StartCoroutine(StartSequence());
    }

    IEnumerator StartSequence()
    {
    if (gameUI == null)
        gameUI = FindSceneObject("UI");

    mainMenu.SetActive(false);

    if (gameUI != null)
        gameUI.SetActive(true);

    // Munculkan NPC & Tetua di posisi random
    if (npcManager != null)
        npcManager.SpawnNPCs();

    // Kamera langsung mulai mengikuti player
    cameraController.canFollow = true;

    float time = 0f;

    while (time < zoomDuration)
    {
        mainCamera.orthographicSize =
            Mathf.Lerp(startSize, gameSize, time / zoomDuration);

        time += Time.deltaTime;

        yield return null;
    }

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
    playerMovement.canMove = true;
    playerMovement.SetMenuActive(false);
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
