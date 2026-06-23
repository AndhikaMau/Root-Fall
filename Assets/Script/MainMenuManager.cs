using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    public Camera mainCamera;
    public Transform focusPoint;

    public float targetSize = 3f;
    public float moveDuration = 1.5f;

    public string nextSceneName = "TutorialStage";

    private bool isStarting = false;

    public void StartGame()
    {
        if (!isStarting)
            StartCoroutine(StartGameSequence());
    }

    IEnumerator StartGameSequence()
    {
        isStarting = true;

        Vector3 startPos = mainCamera.transform.position;
        Vector3 targetPos = new Vector3(
            focusPoint.position.x,
            focusPoint.position.y,
            mainCamera.transform.position.z
        );

        float startSize = mainCamera.orthographicSize;
        float timer = 0f;

        while (timer < moveDuration)
        {
            timer += Time.deltaTime;
            float t = timer / moveDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);

            yield return null;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}