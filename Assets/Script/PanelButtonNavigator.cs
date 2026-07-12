using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelButtonNavigator : MonoBehaviour
{
    public Button[] buttons;
    public Color normalTextColor = Color.white;
    public Color selectedTextColor = new Color(1f, 0.82f, 0.32f, 1f);
    public Color normalGraphicColor = Color.white;
    public Color selectedGraphicColor = new Color(1f, 0.92f, 0.58f, 1f);
    public Vector3 selectedScaleMultiplier = new Vector3(1.08f, 1.08f, 1f);

    private int selectedIndex;
    private Text[] buttonTexts;
    private Graphic[] buttonGraphics;
    private Vector3[] baseScales;
    private Button[] cachedButtons;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        Setup();
    }

    private void OnEnable()
    {
        Setup();
        selectedIndex = GetFirstValidButtonIndex();
        RefreshSelection();
    }

    private void Update()
    {
        if (buttons == null || buttons.Length == 0)
            return;

        SyncSelectionFromEventSystem();

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            MoveSelection(-1);
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) ||
                 Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            MoveSelection(1);
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            ActivateSelection();
    }

    public void Setup()
    {
        if (buttons == null || buttons.Length == 0)
            buttons = GetComponentsInChildren<Button>(true);

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        CacheButtonVisuals();
        DisableMouseRaycasts();
    }

    private void DisableMouseRaycasts()
    {
        Graphic[] graphics = GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
            graphics[i].raycastTarget = false;
    }

    private void CacheButtonVisuals()
    {
        if (buttons == null)
            return;

        buttonTexts = new Text[buttons.Length];
        buttonGraphics = new Graphic[buttons.Length];

        bool rebuildScaleCache = cachedButtons == null || cachedButtons.Length != buttons.Length || baseScales == null || baseScales.Length != buttons.Length;

        if (!rebuildScaleCache)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (cachedButtons[i] != buttons[i])
                {
                    rebuildScaleCache = true;
                    break;
                }
            }
        }

        if (rebuildScaleCache)
        {
            cachedButtons = new Button[buttons.Length];
            baseScales = new Vector3[buttons.Length];

            for (int i = 0; i < buttons.Length; i++)
            {
                cachedButtons[i] = buttons[i];
                baseScales[i] = buttons[i] != null ? buttons[i].transform.localScale : Vector3.one;
            }
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null)
                continue;

            buttons[i].transform.localScale = baseScales[i];
            buttonTexts[i] = buttons[i].GetComponentInChildren<Text>(true);
            buttonGraphics[i] = buttons[i].targetGraphic != null
                ? buttons[i].targetGraphic
                : buttons[i].GetComponent<Graphic>();
        }
    }

    private int GetFirstValidButtonIndex()
    {
        if (buttons == null)
            return 0;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (IsButtonSelectable(buttons[i]))
                return i;
        }

        return 0;
    }

    private void MoveSelection(int direction)
    {
        if (buttons == null || buttons.Length == 0)
            return;

        int nextIndex = selectedIndex;

        for (int i = 0; i < buttons.Length; i++)
        {
            nextIndex = (nextIndex + direction + buttons.Length) % buttons.Length;
            if (IsButtonSelectable(buttons[nextIndex]))
            {
                selectedIndex = nextIndex;
                RefreshSelection();
                return;
            }
        }
    }

    private void ActivateSelection()
    {
        if (buttons == null || selectedIndex < 0 || selectedIndex >= buttons.Length)
            return;

        Button selectedButton = buttons[selectedIndex];
        if (IsButtonSelectable(selectedButton))
            selectedButton.onClick.Invoke();
    }

    private void RefreshSelection()
    {
        if (buttons == null || baseScales == null)
            return;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null)
                continue;

            bool selected = i == selectedIndex;
            buttons[i].transform.localScale = selected
                ? Vector3.Scale(baseScales[i], selectedScaleMultiplier)
                : baseScales[i];

            if (buttonTexts != null && i < buttonTexts.Length && buttonTexts[i] != null)
                buttonTexts[i].color = selected ? selectedTextColor : normalTextColor;

            if (buttonGraphics != null && i < buttonGraphics.Length && buttonGraphics[i] != null)
                buttonGraphics[i].color = selected ? selectedGraphicColor : normalGraphicColor;
        }

        SelectCurrentButtonInEventSystem();
    }

    private void SyncSelectionFromEventSystem()
    {
        if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null || buttons == null)
            return;

        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null || buttons[i].gameObject != selectedObject)
                continue;

            if (selectedIndex != i)
            {
                selectedIndex = i;
                RefreshSelection();
            }

            return;
        }
    }

    private void SelectCurrentButtonInEventSystem()
    {
        if (EventSystem.current == null || buttons == null || selectedIndex < 0 || selectedIndex >= buttons.Length)
            return;

        Button selectedButton = buttons[selectedIndex];
        if (!IsButtonSelectable(selectedButton))
            return;

        if (EventSystem.current.currentSelectedGameObject != selectedButton.gameObject)
            EventSystem.current.SetSelectedGameObject(selectedButton.gameObject);
    }

    private bool IsButtonSelectable(Button button)
    {
        return button != null && button.interactable && button.gameObject.activeInHierarchy;
    }
}
