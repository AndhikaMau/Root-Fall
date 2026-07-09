using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MissionStartBanner : MonoBehaviour
{
    public static MissionStartBanner Instance { get; private set; }

    public CanvasGroup canvasGroup;
    public Text messageText;
    public string message = "MISI DIMULAI";
    public float fadeInDuration = 0.5f;
    public float showDuration = 1.2f;
    public float fadeOutDuration = 0.6f;

    [Header("Mission Objective")]
    public CanvasGroup objectiveGroup;
    public Text objectiveTitleText;
    public Text objectiveDetailText;
    public string objectiveTitle = "Misi Pertama";
    public string objectiveDetail = "Cari jalan menuju bawah tanah.";
    public float objectiveFadeDuration = 0.4f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        Instance = this;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        HideInstant();
        HideObjectiveInstant();
    }

    public void Play()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(PlayRoutine());
    }

    public IEnumerator PlayAndWait()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(PlayRoutine());
        yield return currentRoutine;
    }

    private IEnumerator PlayRoutine()
    {
        if (messageText != null)
            messageText.text = message;

        gameObject.SetActive(true);

        yield return Fade(0f, 1f, fadeInDuration);
        yield return new WaitForSecondsRealtime(showDuration);
        yield return Fade(1f, 0f, fadeOutDuration);

        HideInstant();

        if (objectiveGroup != null)
            yield return ShowObjectiveRoutine();

        currentRoutine = null;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (canvasGroup == null)
            yield break;

        if (duration <= 0f)
        {
            canvasGroup.alpha = to;
            yield break;
        }

        float elapsed = 0f;
        canvasGroup.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    private void HideInstant()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private IEnumerator ShowObjectiveRoutine()
    {
        if (objectiveTitleText != null)
            objectiveTitleText.text = objectiveTitle;

        if (objectiveDetailText != null)
            objectiveDetailText.text = objectiveDetail;

        objectiveGroup.gameObject.SetActive(true);
        objectiveGroup.interactable = false;
        objectiveGroup.blocksRaycasts = false;

        float elapsed = 0f;
        objectiveGroup.alpha = 0f;

        while (elapsed < objectiveFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            objectiveGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / objectiveFadeDuration);
            yield return null;
        }

        objectiveGroup.alpha = 1f;
    }

    private void HideObjectiveInstant()
    {
        if (objectiveGroup == null)
            return;

        objectiveGroup.alpha = 0f;
        objectiveGroup.interactable = false;
        objectiveGroup.blocksRaycasts = false;
        objectiveGroup.gameObject.SetActive(false);
    }
}
