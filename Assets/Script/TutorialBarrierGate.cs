using UnityEngine;

public class TutorialBarrierGate : MonoBehaviour
{
    [Tooltip("Jika ON, player ditahan di sisi kiri barrier sampai dialog Elder selesai.")]
    public bool blockFromLeft = true;
    public float blockOffset = 0.8f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryBlockPlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryBlockPlayer(other);
    }

    private void TryBlockPlayer(Collider2D other)
    {
        if (ElderDialogueInteract.HasCompletedElderDialogue)
            return;

        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player == null)
            return;

        Transform playerTransform = player.transform;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        float barrierX = transform.position.x;
        float blockedX = blockFromLeft
            ? barrierX - blockOffset
            : barrierX + blockOffset;

        Vector3 position = playerTransform.position;
        bool shouldBlock = blockFromLeft
            ? position.x > blockedX
            : position.x < blockedX;

        if (!shouldBlock)
            return;

        position.x = blockedX;
        playerTransform.position = position;

        if (rb != null)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }
}
