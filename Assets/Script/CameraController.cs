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

    [Header("Camera Bounds (opsional)")]
    public bool useBounds = false;
    public float minX, maxX, minY, maxY;

    [Header("Y Offset")]
    public float yOffset = 1f;

    private Vector3 velocity = Vector3.zero;
    private float lookAheadVelocity = 0f;
    private float currentLookAhead = 0f;
    private float lastTargetX;
    private Rigidbody2D targetRb;

    void Start()
    {
        if (target != null)
        {
            transform.position = new Vector3(target.position.x, target.position.y + yOffset, transform.position.z);
            lastTargetX = target.position.x;
            targetRb = target.GetComponent<Rigidbody2D>();
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // === LOOK AHEAD ===
        float moveDir = 0f;
        if (targetRb != null)
            moveDir = targetRb.linearVelocity.x;
        else
            moveDir = target.position.x - lastTargetX;

        float targetLookAhead = Mathf.Sign(moveDir) * lookAheadDistance;
        if (Mathf.Abs(moveDir) < 0.1f) targetLookAhead = 0f;

        currentLookAhead = Mathf.SmoothDamp(currentLookAhead, targetLookAhead, ref lookAheadVelocity, lookAheadSmoothTime);

        // === DEAD ZONE ===
        float targetX = target.position.x + currentLookAhead;
        float targetY = target.position.y + yOffset;

        float diffX = targetX - transform.position.x;
        float diffY = targetY - transform.position.y;

        if (Mathf.Abs(diffX) < deadZoneX) targetX = transform.position.x;
        if (Mathf.Abs(diffY) < deadZoneY) targetY = transform.position.y;

        // === SMOOTH FOLLOW ===
        Vector3 desiredPos = new Vector3(targetX, targetY, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity,
                             new Vector3(smoothTimeX, smoothTimeY, 0).magnitude > 0 ? smoothTimeX : 0.2f);

        // Smooth Y secara terpisah biar bisa beda nilai
        float newX = Mathf.SmoothDamp(transform.position.x, targetX, ref velocity.x, smoothTimeX);
        float newY = Mathf.SmoothDamp(transform.position.y, targetY, ref velocity.y, smoothTimeY);
        transform.position = new Vector3(newX, newY, transform.position.z);

        // === BOUNDS ===
        if (useBounds)
        {
            float camH = Camera.main.orthographicSize;
            float camW = camH * Camera.main.aspect;
            float clampedX = Mathf.Clamp(transform.position.x, minX + camW, maxX - camW);
            float clampedY = Mathf.Clamp(transform.position.y, minY + camH, maxY - camH);
            transform.position = new Vector3(clampedX, clampedY, transform.position.z);
        }

        lastTargetX = target.position.x;
    }
}