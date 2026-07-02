using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Smooth Follow")]
    public float smoothTimeX = 0.25f;
    public float smoothTimeY = 0.2f;

    [Header("Look Ahead")]
    public float lookAheadDistance = 2f;
    public float lookAheadSmoothTime = 0.4f;

    [Header("Dead Zone")]
    public float deadZoneX = 0.5f;
    public float deadZoneY = 0.3f;

    [Header("Camera Bounds")]
    public bool useBounds = true;
    public BoxCollider2D cameraBounds;

    [Header("Y Offset")]
    public float yOffset = 1f;

    [Header("Main Menu")]
    public bool canFollow = false;

    private float lookAheadVelocity;
    private float currentLookAhead;
    private float lastTargetX;

    private float velocityX;
    private float velocityY;

    private Rigidbody2D targetRb;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        if (target != null)
        {
            targetRb = target.GetComponent<Rigidbody2D>();
            lastTargetX = target.position.x;
        }
    }

    void LateUpdate()
    {
        // Saat Main Menu kamera tidak mengikuti player
        if (!canFollow)
            return;

        if (target == null)
            return;

        float moveDir = 0f;

        if (targetRb != null)
            moveDir = targetRb.linearVelocity.x;
        else
            moveDir = target.position.x - lastTargetX;

        float targetLookAhead = Mathf.Sign(moveDir) * lookAheadDistance;

        if (Mathf.Abs(moveDir) < 0.1f)
            targetLookAhead = 0f;

        currentLookAhead = Mathf.SmoothDamp(
            currentLookAhead,
            targetLookAhead,
            ref lookAheadVelocity,
            lookAheadSmoothTime);

        float targetX = target.position.x + currentLookAhead;
        float targetY = target.position.y + yOffset;

        float diffX = targetX - transform.position.x;
        float diffY = targetY - transform.position.y;

        if (Mathf.Abs(diffX) < deadZoneX)
            targetX = transform.position.x;

        if (Mathf.Abs(diffY) < deadZoneY)
            targetY = transform.position.y;

        float newX = Mathf.SmoothDamp(
            transform.position.x,
            targetX,
            ref velocityX,
            smoothTimeX);

        float newY = Mathf.SmoothDamp(
            transform.position.y,
            targetY,
            ref velocityY,
            smoothTimeY);

        Vector3 newPosition = new Vector3(
            newX,
            newY,
            transform.position.z);

        if (useBounds && cameraBounds != null)
        {
            Bounds bounds = cameraBounds.bounds;

            float camHeight = cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;

            float minX = bounds.min.x + camWidth;
            float maxX = bounds.max.x - camWidth;
            float minY = bounds.min.y + camHeight;
            float maxY = bounds.max.y - camHeight;

            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
        }

        transform.position = newPosition;

        lastTargetX = target.position.x;
    }
}