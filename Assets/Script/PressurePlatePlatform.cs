using UnityEngine;

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

    private int playerLayer = -1;
    private int playerTouches;
    private bool hasBeenPressed;
    private Vector3 plateStartPosition;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer(playerLayerName);

        if (plateVisual == null)
            plateVisual = transform;

        plateStartPosition = plateVisual.localPosition;

        if (hidePlatformOnStart)
            SetPlatformActive(false);
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
    }
}
