using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class GateMissionTrigger : MonoBehaviour
{
    [Header("Chatbox")]
    [TextArea(2, 4)]
    public string gateMessage = "Sepertinya ini gerbang menuju ke bawah deh, aku harus menemukan cara untuk membukanya.";
    public Transform chatTarget;
    public Vector3 chatOffset = new Vector3(0f, 1.6f, 0f);
    public float chatDuration = 3f;
    public float chatFadeDuration = 0.18f;
    public Vector2 chatBoxSize = new Vector2(380f, 110f);
    public Vector3 chatPopStartScale = new Vector3(0.008f, 0.008f, 0.008f);
    public Vector3 chatPopEndScale = new Vector3(0.01f, 0.01f, 0.01f);
    public GameObject chatBoxObject;
    public Text chatText;

    [Header("Mission")]
    public MissionStartBanner missionBanner;
    public string missionClearMessage = "MISI PERTAMA SELESAI";
    public string newMissionMessage = "MISI BARU";
    public string newObjectiveTitle = "Misi Kedua";
    public string newObjectiveDetail = "Cari item untuk membuka gerbang.";

    [Header("Player Detection")]
    public string playerLayerName = "Player";
    public bool requirePlayerMovement = true;
    public bool triggerOnlyOnce = true;

    private int playerLayer = -1;
    private bool hasTriggered;
    private Coroutine currentRoutine;
    private CanvasGroup chatCanvasGroup;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer(playerLayerName);

        Collider2D triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;

        if (chatTarget == null)
            chatTarget = transform;

        if (missionBanner == null)
            missionBanner = MissionStartBanner.Instance != null
                ? MissionStartBanner.Instance
                : FindAnyObjectByType<MissionStartBanner>();

        EnsureChatBox();
        HideChatBox();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerOnlyOnce && hasTriggered)
            return;

        if (!IsPlayer(other))
            return;

        hasTriggered = true;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(TriggerRoutine());
    }

    private IEnumerator TriggerRoutine()
    {
        if (missionBanner == null)
            missionBanner = MissionStartBanner.Instance != null
                ? MissionStartBanner.Instance
                : FindAnyObjectByType<MissionStartBanner>();

        if (missionBanner != null)
            missionBanner.HideObjective();

        yield return ShowChatBoxRoutine();
        yield return new WaitForSeconds(chatDuration);
        yield return HideChatBoxRoutine();

        if (missionBanner != null)
        {
            yield return missionBanner.PlayMessageAndWait(missionClearMessage);
            yield return missionBanner.PlayObjectiveUpdateAndWait(
                newMissionMessage,
                newObjectiveTitle,
                newObjectiveDetail);
        }

        currentRoutine = null;
    }

    private bool IsPlayer(Collider2D other)
    {
        if (other == null)
            return false;

        if (playerLayer >= 0 && other.gameObject.layer != playerLayer)
            return false;

        return !requirePlayerMovement || other.GetComponentInParent<PlayerMovement>() != null;
    }

    private void EnsureChatBox()
    {
        if (chatBoxObject != null && chatText != null)
            return;

        if (chatBoxObject == null)
        {
            chatBoxObject = new GameObject("GateChatBox", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup));
            chatBoxObject.transform.SetParent(chatTarget, false);
            chatBoxObject.transform.localPosition = chatOffset;
            chatCanvasGroup = chatBoxObject.GetComponent<CanvasGroup>();

            Canvas canvas = chatBoxObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;

            RectTransform canvasRect = chatBoxObject.GetComponent<RectTransform>();
            canvasRect.sizeDelta = chatBoxSize;
            canvasRect.localScale = chatPopEndScale;

            GameObject backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(Image));
            backgroundObject.transform.SetParent(chatBoxObject.transform, false);

            RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            Image background = backgroundObject.GetComponent<Image>();
            background.color = new Color(0.05f, 0.035f, 0.02f, 0.88f);

            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(backgroundObject.transform, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(18f, 10f);
            textRect.offsetMax = new Vector2(-18f, -10f);

            chatText = textObject.GetComponent<Text>();
            chatText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            chatText.fontSize = 18;
            chatText.alignment = TextAnchor.MiddleCenter;
            chatText.color = Color.white;
            chatText.horizontalOverflow = HorizontalWrapMode.Wrap;
            chatText.verticalOverflow = VerticalWrapMode.Overflow;
        }
        else if (chatText == null)
        {
            chatText = chatBoxObject.GetComponentInChildren<Text>(true);
        }

        if (chatCanvasGroup == null && chatBoxObject != null)
            chatCanvasGroup = chatBoxObject.GetComponent<CanvasGroup>();

        if (chatCanvasGroup == null && chatBoxObject != null)
            chatCanvasGroup = chatBoxObject.AddComponent<CanvasGroup>();

        ApplyChatBoxLayout();
    }

    private void ApplyChatBoxLayout()
    {
        if (chatBoxObject == null)
            return;

        Canvas canvas = chatBoxObject.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;
        }

        RectTransform canvasRect = chatBoxObject.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            canvasRect.sizeDelta = chatBoxSize;
            canvasRect.localScale = chatPopEndScale;
        }
    }

    private IEnumerator ShowChatBoxRoutine()
    {
        EnsureChatBox();

        if (chatBoxObject == null)
            yield break;

        chatBoxObject.transform.position = chatTarget.position + chatOffset;

        if (chatText != null)
            chatText.text = gateMessage;

        chatBoxObject.SetActive(true);

        if (chatFadeDuration <= 0f || chatCanvasGroup == null)
        {
            if (chatCanvasGroup != null)
                chatCanvasGroup.alpha = 1f;

            chatBoxObject.transform.localScale = chatPopEndScale;
            yield break;
        }

        float elapsed = 0f;
        chatCanvasGroup.alpha = 0f;
        chatBoxObject.transform.localScale = chatPopStartScale;

        while (elapsed < chatFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / chatFadeDuration);

            chatCanvasGroup.alpha = t;
            chatBoxObject.transform.localScale = Vector3.Lerp(chatPopStartScale, chatPopEndScale, t);

            yield return null;
        }

        chatCanvasGroup.alpha = 1f;
        chatBoxObject.transform.localScale = chatPopEndScale;
    }

    private IEnumerator HideChatBoxRoutine()
    {
        if (chatBoxObject == null)
            yield break;

        if (chatFadeDuration > 0f && chatCanvasGroup != null)
        {
            float elapsed = 0f;

            while (elapsed < chatFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / chatFadeDuration);

                chatCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                chatBoxObject.transform.localScale = Vector3.Lerp(chatPopEndScale, chatPopStartScale, t);

                yield return null;
            }
        }

        HideChatBox();
    }

    private void HideChatBox()
    {
        if (chatBoxObject == null)
            return;

        if (chatCanvasGroup != null)
            chatCanvasGroup.alpha = 0f;

        chatBoxObject.SetActive(false);
    }
}
