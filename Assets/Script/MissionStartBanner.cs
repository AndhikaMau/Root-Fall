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
    public RectTransform objectiveRect;
    public Image objectiveBackground;
    public Vector2 objectiveLargePosition = new Vector2(0f, -68f);
    public Vector2 objectiveLargeSize = new Vector2(520f, 86f);
    public Vector2 objectiveSmallPosition = new Vector2(-18f, -16f);
    public Vector2 objectiveSmallSize = new Vector2(310f, 38f);
    public Color objectiveLargeBackgroundColor = new Color(0.05f, 0.06f, 0.055f, 0.72f);
    public Color objectiveSmallBackgroundColor = new Color(0.16f, 0.26f, 0.30f, 0.64f);
    public Color objectiveLargeDetailColor = new Color(1f, 1f, 1f, 0.92f);
    public Color objectiveSmallDetailColor = new Color(1f, 1f, 1f, 0.95f);
    public float objectiveLargeDuration = 1.6f;
    public float objectiveMoveDuration = 0.55f;
    public int titleLargeFontSize = 24;
    public int titleSmallFontSize = 15;
    public int detailLargeFontSize = 17;
    public int detailSmallFontSize = 10;

    private Coroutine currentRoutine;
    private RectTransform objectiveTitleRect;
    private RectTransform objectiveDetailRect;

    private void Awake()
    {
        Instance = this;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (objectiveRect == null && objectiveGroup != null)
            objectiveRect = objectiveGroup.GetComponent<RectTransform>();

        if (objectiveBackground == null && objectiveGroup != null)
            objectiveBackground = objectiveGroup.GetComponent<Image>();

        CacheObjectiveTextRects();
        HideInstant();
        HideObjectiveInstant();
    }

    public void Play()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(PlayRoutine(message, true));
    }

    public IEnumerator PlayAndWait()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(PlayRoutine(message, true));
        yield return currentRoutine;
    }

    public void PlayMessage(string customMessage, bool showObjectiveAfter = false)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(PlayRoutine(customMessage, showObjectiveAfter));
    }

    public IEnumerator PlayMessageAndWait(string customMessage, bool showObjectiveAfter = false)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(PlayRoutine(customMessage, showObjectiveAfter));
        yield return currentRoutine;
    }

    public void SetObjective(string title, string detail)
    {
        objectiveTitle = title;
        objectiveDetail = detail;

        if (objectiveTitleText != null)
            objectiveTitleText.text = objectiveTitle;

        if (objectiveDetailText != null)
            objectiveDetailText.text = objectiveDetail;
    }

    public void PlayObjectiveUpdate(string bannerMessage, string title, string detail)
    {
        SetObjective(title, detail);
        PlayMessage(bannerMessage, true);
    }

    public IEnumerator PlayObjectiveUpdateAndWait(string bannerMessage, string title, string detail)
    {
        SetObjective(title, detail);
        yield return PlayMessageAndWait(bannerMessage, true);
    }

    public void HideObjective()
    {
        HideObjectiveInstant();
    }

    private IEnumerator PlayRoutine(string displayMessage, bool showObjectiveAfter)
    {
        if (messageText != null)
            messageText.text = displayMessage;

        gameObject.SetActive(true);

        yield return Fade(0f, 1f, fadeInDuration);
        yield return new WaitForSecondsRealtime(showDuration);
        yield return Fade(1f, 0f, fadeOutDuration);

        HideInstant();

        if (showObjectiveAfter && objectiveGroup != null)
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
        {
            objectiveTitleText.gameObject.SetActive(true);
            objectiveTitleText.text = objectiveTitle;
        }

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

        if (objectiveRect != null)
        {
            yield return new WaitForSecondsRealtime(objectiveLargeDuration);
            yield return MoveObjectiveToCornerRoutine();
        }
    }

    private IEnumerator MoveObjectiveToCornerRoutine()
    {
        SetObjectiveAsLarge();
        Canvas.ForceUpdateCanvases();

        Vector3 startPosition = objectiveRect.position;
        Vector2 startSize = objectiveRect.sizeDelta;

        SetObjectiveAsSmall();
        Canvas.ForceUpdateCanvases();

        Vector3 targetPosition = objectiveRect.position;
        Vector2 targetSize = objectiveRect.sizeDelta;

        SetObjectiveAsLarge();
        ApplySmallObjectiveContentStyle();

        if (objectiveMoveDuration <= 0f)
        {
            SetObjectiveAsSmall();
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < objectiveMoveDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / objectiveMoveDuration);

            objectiveRect.position = Vector3.Lerp(startPosition, targetPosition, t);
            objectiveRect.sizeDelta = Vector2.Lerp(startSize, targetSize, t);
            SetObjectiveFontSize(t);

            yield return null;
        }

        SetObjectiveAsSmall();
    }

    private void SetObjectiveAsLarge()
    {
        if (objectiveRect == null)
            return;

        objectiveRect.anchorMin = new Vector2(0.5f, 1f);
        objectiveRect.anchorMax = new Vector2(0.5f, 1f);
        objectiveRect.pivot = new Vector2(0.5f, 1f);
        objectiveRect.anchoredPosition = objectiveLargePosition;
        objectiveRect.sizeDelta = objectiveLargeSize;

        if (objectiveBackground != null)
            objectiveBackground.color = objectiveLargeBackgroundColor;

        if (objectiveTitleText != null)
        {
            objectiveTitleText.gameObject.SetActive(true);
            objectiveTitleText.alignment = TextAnchor.MiddleCenter;
        }

        if (objectiveTitleRect != null)
        {
            objectiveTitleRect.anchorMin = new Vector2(0f, 1f);
            objectiveTitleRect.anchorMax = new Vector2(1f, 1f);
            objectiveTitleRect.pivot = new Vector2(0.5f, 1f);
            objectiveTitleRect.anchoredPosition = new Vector2(0f, -10f);
            objectiveTitleRect.sizeDelta = new Vector2(-28f, 28f);
        }

        if (objectiveDetailRect != null)
        {
            objectiveDetailRect.anchorMin = Vector2.zero;
            objectiveDetailRect.anchorMax = Vector2.one;
            objectiveDetailRect.pivot = new Vector2(0.5f, 0.5f);
            objectiveDetailRect.anchoredPosition = new Vector2(0f, -18f);
            objectiveDetailRect.sizeDelta = new Vector2(-34f, -42f);
        }

        if (objectiveDetailText != null)
        {
            objectiveDetailText.alignment = TextAnchor.MiddleCenter;
            objectiveDetailText.color = objectiveLargeDetailColor;
        }

        SetObjectiveFontSize(0f);
    }

    private void SetObjectiveAsSmall()
    {
        if (objectiveRect == null)
            return;

        objectiveRect.anchorMin = Vector2.one;
        objectiveRect.anchorMax = Vector2.one;
        objectiveRect.pivot = Vector2.one;
        objectiveRect.anchoredPosition = objectiveSmallPosition;
        objectiveRect.sizeDelta = objectiveSmallSize;

        ApplySmallObjectiveContentStyle();
        SetObjectiveFontSize(1f);
    }

    private void ApplySmallObjectiveContentStyle()
    {
        if (objectiveBackground != null)
            objectiveBackground.color = objectiveSmallBackgroundColor;

        if (objectiveTitleText != null)
            objectiveTitleText.gameObject.SetActive(false);

        if (objectiveDetailRect != null)
        {
            objectiveDetailRect.anchorMin = Vector2.zero;
            objectiveDetailRect.anchorMax = Vector2.one;
            objectiveDetailRect.pivot = new Vector2(0.5f, 0.5f);
            objectiveDetailRect.anchoredPosition = Vector2.zero;
            objectiveDetailRect.sizeDelta = new Vector2(-16f, -8f);
        }

        if (objectiveDetailText != null)
        {
            objectiveDetailText.alignment = TextAnchor.MiddleCenter;
            objectiveDetailText.color = objectiveSmallDetailColor;
        }
    }

    private void SetObjectiveFontSize(float t)
    {
        if (objectiveTitleText != null)
            objectiveTitleText.fontSize = Mathf.RoundToInt(Mathf.Lerp(titleLargeFontSize, titleSmallFontSize, t));

        if (objectiveDetailText != null)
            objectiveDetailText.fontSize = Mathf.RoundToInt(Mathf.Lerp(detailLargeFontSize, detailSmallFontSize, t));
    }

    private void HideObjectiveInstant()
    {
        if (objectiveGroup == null)
            return;

        if (objectiveRect == null)
            objectiveRect = objectiveGroup.GetComponent<RectTransform>();

        CacheObjectiveTextRects();
        SetObjectiveAsLarge();
        objectiveGroup.alpha = 0f;
        objectiveGroup.interactable = false;
        objectiveGroup.blocksRaycasts = false;
        objectiveGroup.gameObject.SetActive(false);
    }

    private void CacheObjectiveTextRects()
    {
        if (objectiveTitleText != null && objectiveTitleRect == null)
            objectiveTitleRect = objectiveTitleText.GetComponent<RectTransform>();

        if (objectiveDetailText != null && objectiveDetailRect == null)
            objectiveDetailRect = objectiveDetailText.GetComponent<RectTransform>();
    }
}
