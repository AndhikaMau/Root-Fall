using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Collider2D))]
public class PressurePlatePlatform : MonoBehaviour
{
    [Header("Target")]
    public GameObject platformObject;
    public bool keepPlatformActiveAfterPressed = true;
    public bool hidePlatformOnStart = true;

    [Header("Player Detection")]
    public string playerLayerName = "Player";
    public bool requirePlayerMovement = true;

    [Header("Visual Feedback")]
    public Transform plateVisual;
    public Vector3 pressedOffset = new Vector3(0f, -0.08f, 0f);
    public SpriteRenderer plateRenderer;
    public Sprite normalSprite;
    public Sprite pressedSprite;
    public GameObject pressedVisualObject;

    [Header("Chatbox")]
    public bool showChatOnPress = true;
    public bool showChatOnlyOnce = true;
    [TextArea(2, 4)]
    public string chatMessage = "Sepertinya ada sesuatu yang muncul, coba kembali ke gua awal";
    public Transform chatTarget;
    public Vector3 chatOffset = new Vector3(0f, 1.6f, 0f);
    public float chatDuration = 3f;
    public float chatFadeDuration = 0.18f;
    public Vector2 chatBoxSize = new Vector2(420f, 110f);
    public Vector3 chatPopStartScale = new Vector3(0.008f, 0.008f, 0.008f);
    public Vector3 chatPopEndScale = new Vector3(0.01f, 0.01f, 0.01f);
    public GameObject chatBoxObject;
    public Text chatText;

    private int playerLayer = -1;
    private int playerTouches;
    private bool hasBeenPressed;
    private bool hasShownChat;
    private Vector3 plateStartPosition;
    private Coroutine chatRoutine;
    private CanvasGroup chatCanvasGroup;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer(playerLayerName);

        Collider2D triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;

        if (plateVisual == null)
            plateVisual = transform;

        ResolvePlateRenderer();
        ResolveSprites();

        if (chatTarget == null)
            chatTarget = transform;

        plateStartPosition = plateVisual.localPosition;

        if (hidePlatformOnStart)
            SetPlatformActive(false);

        SetPlateVisualPressed(false);
        EnsureChatBox();
        HideChatBox();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other))
            return;

        playerTouches++;
        PressPlate();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other))
            return;

        playerTouches = Mathf.Max(0, playerTouches - 1);

        if (playerTouches == 0 && !keepPlatformActiveAfterPressed)
            ReleasePlate();
    }

    private void PressPlate()
    {
        hasBeenPressed = true;
        SetPlatformActive(true);
        SetPlateVisualPressed(true);
        ShowPlateChat();
    }

    private void ReleasePlate()
    {
        if (!hasBeenPressed)
            return;

        SetPlatformActive(false);
        SetPlateVisualPressed(false);
    }

    private bool IsPlayer(Collider2D other)
    {
        if (other == null)
            return false;

        if (playerLayer >= 0 && other.gameObject.layer != playerLayer)
            return false;

        return !requirePlayerMovement || other.GetComponentInParent<PlayerMovement>() != null;
    }

    private void SetPlatformActive(bool active)
    {
        if (platformObject != null && platformObject.activeSelf != active)
            platformObject.SetActive(active);
    }

    private void SetPlateVisualPressed(bool pressed)
    {
        if (plateVisual == null)
            return;

        plateVisual.localPosition = pressed
            ? plateStartPosition + pressedOffset
            : plateStartPosition;

        if (pressedVisualObject != null && pressedVisualObject != gameObject)
        {
            pressedVisualObject.SetActive(pressed);

            if (plateRenderer != null)
                plateRenderer.enabled = !pressed;

            return;
        }

        if (plateRenderer == null)
            return;

        plateRenderer.enabled = true;

        if (pressed && pressedSprite != null)
        {
            plateRenderer.sprite = pressedSprite;
            return;
        }

        if (!pressed && normalSprite != null)
            plateRenderer.sprite = normalSprite;
    }

    private void ResolvePlateRenderer()
    {
        if (plateRenderer != null)
            return;

        plateRenderer = GetComponent<SpriteRenderer>();

        if (plateRenderer == null && plateVisual != null)
            plateRenderer = plateVisual.GetComponent<SpriteRenderer>();

        if (plateRenderer == null && plateVisual != null)
            plateRenderer = plateVisual.GetComponentInChildren<SpriteRenderer>(true);
    }

    private void ResolveSprites()
    {
        if (normalSprite == null && plateRenderer != null)
            normalSprite = plateRenderer.sprite;

#if UNITY_EDITOR
        if (pressedSprite == null)
        {
            pressedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/BG, DLL/pressureplate_pressed.png");

            if (pressedSprite == null)
            {
                Object[] sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/BG, DLL/pressureplate_pressed.png");
                foreach (Object asset in sprites)
                {
                    if (asset is Sprite sprite)
                    {
                        pressedSprite = sprite;
                        break;
                    }
                }
            }
        }
#endif
    }

    private void ShowPlateChat()
    {
        if (!showChatOnPress || (showChatOnlyOnce && hasShownChat))
            return;

        hasShownChat = true;

        if (chatRoutine != null)
            StopCoroutine(chatRoutine);

        chatRoutine = StartCoroutine(ChatRoutine());
    }

    private IEnumerator ChatRoutine()
    {
        yield return ShowChatBoxRoutine();
        yield return new WaitForSeconds(chatDuration);
        yield return HideChatBoxRoutine();
        chatRoutine = null;
    }

    private void EnsureChatBox()
    {
        if (chatBoxObject != null && chatText != null)
            return;

        if (chatBoxObject == null)
        {
            chatBoxObject = new GameObject("PressurePlateChatBox", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup));
            chatBoxObject.transform.SetParent(chatTarget, false);
            chatBoxObject.transform.localPosition = chatOffset;
            chatCanvasGroup = chatBoxObject.GetComponent<CanvasGroup>();

            Canvas canvas = chatBoxObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 120;

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
    }

    private IEnumerator ShowChatBoxRoutine()
    {
        EnsureChatBox();

        if (chatBoxObject == null)
            yield break;

        chatBoxObject.transform.position = chatTarget.position + chatOffset;

        if (chatText != null)
            chatText.text = chatMessage;

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
