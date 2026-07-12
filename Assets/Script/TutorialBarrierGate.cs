using UnityEngine;

public class TutorialBarrierGate : MonoBehaviour
{
    [Tooltip("Jika ON, player ditahan di sisi kiri barrier sampai dialog Elder selesai.")]
    public bool blockFromLeft = true;
    public float blockOffset = 0.8f;

    [Header("Stage Banner")]
    public bool showStageBannerAfterPassed = true;
    public string stageBannerMessage = "STAGE 1";
    public float passedOffset = 0.25f;
    public MissionStartBanner stageBanner;

    private bool hasShownStageBanner;

    private void Awake()
    {
        if (stageBanner == null)
            stageBanner = MissionStartBanner.Instance != null
                ? MissionStartBanner.Instance
                : FindAnyObjectByType<MissionStartBanner>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryBlockPlayer(other);
        TryShowStageBanner(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryBlockPlayer(other);
        TryShowStageBanner(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        TryShowStageBanner(other);
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

    private void TryShowStageBanner(Collider2D other)
    {
        if (!showStageBannerAfterPassed || hasShownStageBanner)
            return;

        if (!ElderDialogueInteract.HasCompletedElderDialogue)
            return;

        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player == null)
            return;

        float barrierX = transform.position.x;
        float passedX = blockFromLeft
            ? barrierX + passedOffset
            : barrierX - passedOffset;

        float playerX = player.transform.position.x;
        bool hasPassed = blockFromLeft
            ? playerX >= passedX
            : playerX <= passedX;

        if (!hasPassed)
            return;

        hasShownStageBanner = true;

        if (stageBanner == null)
            stageBanner = MissionStartBanner.Instance != null
                ? MissionStartBanner.Instance
                : FindAnyObjectByType<MissionStartBanner>();

        if (stageBanner != null)
            stageBanner.PlayMessage(stageBannerMessage);
    }
}
