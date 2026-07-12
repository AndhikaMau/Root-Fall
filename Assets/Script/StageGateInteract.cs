using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class StageGateInteract : MonoBehaviour
{
    [Header("Scene")]
    public string targetSceneName = "Stage 2";
    public KeyCode interactKey = KeyCode.F;

    [Header("Requirement")]
    public bool requireHeldItem = true;
    public string requiredItemName = "Key";
    public string playerLayerName = "Player";
    public bool requirePlayerMovement = true;

    [Header("Prompt")]
    public string promptText = "Tekan F";
    public Vector3 promptOffset = new Vector3(0f, 1.3f, 0f);
    public Vector2 promptSize = new Vector2(180f, 54f);
    public Vector3 promptScale = new Vector3(0.01f, 0.01f, 0.01f);
    public GameObject promptObject;
    public Text promptLabel;

    private int playerLayer = -1;
    private PlayerCarry playerCarry;
    private bool playerInRange;
    private bool isLoading;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer(playerLayerName);

        Collider2D triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;

        EnsurePrompt();
        HidePrompt();
    }

    private void Update()
    {
        if (Time.timeScale == 0f || isLoading)
            return;

        bool canInteract = playerInRange && HasRequiredItem();
        SetPromptVisible(canInteract);

        if (canInteract && Input.GetKeyDown(interactKey))
            LoadTargetScene();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other))
            return;

        playerCarry = other.GetComponentInParent<PlayerCarry>();
        playerInRange = true;
        SetPromptVisible(HasRequiredItem());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other))
            return;

        playerInRange = false;
        playerCarry = null;
        HidePrompt();
    }

    private bool IsPlayer(Collider2D other)
    {
        if (other == null)
            return false;

        if (playerLayer >= 0 && other.gameObject.layer != playerLayer)
            return false;

        return !requirePlayerMovement || other.GetComponentInParent<PlayerMovement>() != null;
    }

    private bool HasRequiredItem()
    {
        if (!requireHeldItem)
            return true;

        return playerCarry != null && playerCarry.IsHoldingItem(requiredItemName);
    }

    private void LoadTargetScene()
    {
        if (string.IsNullOrWhiteSpace(targetSceneName))
            return;

        isLoading = true;
        HidePrompt();

        PlayerPrefs.Save();
        SceneManager.LoadScene(targetSceneName);
    }

    private void EnsurePrompt()
    {
        if (promptObject != null && promptLabel != null)
            return;

        if (promptObject == null)
        {
            promptObject = new GameObject("GatePrompt", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup));
            promptObject.transform.SetParent(transform, false);
            promptObject.transform.localPosition = promptOffset;

            Canvas canvas = promptObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 130;

            RectTransform canvasRect = promptObject.GetComponent<RectTransform>();
            canvasRect.sizeDelta = promptSize;
            canvasRect.localScale = promptScale;

            GameObject backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(Image));
            backgroundObject.transform.SetParent(promptObject.transform, false);

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
            textRect.offsetMin = new Vector2(12f, 6f);
            textRect.offsetMax = new Vector2(-12f, -6f);

            promptLabel = textObject.GetComponent<Text>();
            promptLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            promptLabel.fontSize = 20;
            promptLabel.alignment = TextAnchor.MiddleCenter;
            promptLabel.color = Color.white;
        }
        else if (promptLabel == null)
        {
            promptLabel = promptObject.GetComponentInChildren<Text>(true);
        }

        if (promptLabel != null)
            promptLabel.text = promptText;
    }

    private void SetPromptVisible(bool visible)
    {
        EnsurePrompt();

        if (promptObject == null)
            return;

        promptObject.transform.position = transform.position + promptOffset;
        promptObject.SetActive(visible);

        if (promptLabel != null)
            promptLabel.text = promptText;
    }

    private void HidePrompt()
    {
        if (promptObject != null)
            promptObject.SetActive(false);
    }
}
