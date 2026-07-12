using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform2D : MonoBehaviour
{
    [Header("Movement")]
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;
    public float waitTime = 0.25f;
    public bool startMoving = true;

    [Header("Passenger")]
    public string playerLayerName = "Player";
    public bool requirePlayerMovement = true;
    public float topNormalThreshold = 0.5f;

    private Rigidbody2D rb;
    private Transform targetPoint;
    private Transform carriedPlayer;
    private float waitTimer;
    private int playerLayer = -1;
    private Vector2 previousPosition;
    private Vector2 platformDelta;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        playerLayer = LayerMask.NameToLayer(playerLayerName);
        targetPoint = pointB != null ? pointB : pointA;
        previousPosition = rb.position;
    }

    private void FixedUpdate()
    {
        Vector2 currentPosition = rb.position;
        Vector2 nextPosition = currentPosition;

        if (startMoving && targetPoint != null)
            nextPosition = GetNextPosition(currentPosition);

        platformDelta = nextPosition - currentPosition;
        rb.MovePosition(nextPosition);

        if (carriedPlayer != null)
            carriedPlayer.position += (Vector3)platformDelta;

        previousPosition = nextPosition;
    }

    private Vector2 GetNextPosition(Vector2 currentPosition)
    {
        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            return currentPosition;
        }

        Vector2 targetPosition = targetPoint.position;
        Vector2 nextPosition = Vector2.MoveTowards(
            currentPosition,
            targetPosition,
            speed * Time.fixedDeltaTime);

        if (Vector2.Distance(nextPosition, targetPosition) <= 0.01f)
        {
            nextPosition = targetPosition;
            SwitchTargetPoint();
            waitTimer = waitTime;
        }

        return nextPosition;
    }

    private void SwitchTargetPoint()
    {
        if (pointA == null || pointB == null)
            return;

        targetPoint = targetPoint == pointA ? pointB : pointA;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryCarryPlayer(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryCarryPlayer(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (carriedPlayer != null && collision.transform == carriedPlayer)
            carriedPlayer = null;
    }

    private void TryCarryPlayer(Collision2D collision)
    {
        if (!IsPlayer(collision.collider))
            return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);
            if (contact.normal.y <= -topNormalThreshold)
            {
                carriedPlayer = collision.transform;
                return;
            }
        }
    }

    private bool IsPlayer(Collider2D other)
    {
        if (other == null)
            return false;

        if (playerLayer >= 0 && other.gameObject.layer != playerLayer)
            return false;

        return !requirePlayerMovement || other.GetComponentInParent<PlayerMovement>() != null;
    }

    private void OnDrawGizmosSelected()
    {
        if (pointA == null || pointB == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pointA.position, pointB.position);
        Gizmos.DrawWireSphere(pointA.position, 0.18f);
        Gizmos.DrawWireSphere(pointB.position, 0.18f);
    }
}
