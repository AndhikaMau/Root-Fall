using System.Collections;
using UnityEngine;

public class ElderDialogueInteract : MonoBehaviour
{
    public static bool HasCompletedElderDialogue { get; private set; }

    public KeyCode interactKey = KeyCode.F;
    public GameObject promptCanvas;
    public TutorialDialogue dialogue;
    public MissionStartBanner missionStartBanner;

    private PlayerMovement playerMovement;
    private NPCWander elderWander;
    private Rigidbody2D elderRb;
    private Animator elderAnim;
    private bool isTalking;
    private int interactionAreaTouches;

    private void Awake()
    {
        HasCompletedElderDialogue = false;

        UpdatePrompt(false);

        if (dialogue == null)
            dialogue = FindAnyObjectByType<TutorialDialogue>();

        if (missionStartBanner == null)
            missionStartBanner = dialogue != null && dialogue.missionStartBanner != null
                ? dialogue.missionStartBanner
                : MissionStartBanner.Instance != null
                ? MissionStartBanner.Instance
                : FindAnyObjectByType<MissionStartBanner>();

        elderWander = GetComponent<NPCWander>();
        elderRb = GetComponent<Rigidbody2D>();
        elderAnim = GetComponent<Animator>();
    }

    private void OnDisable()
    {
        interactionAreaTouches = 0;
        UpdatePrompt(false);
    }

    private void Update()
    {
        UpdatePrompt(CanInteract);

        if (isTalking || Time.timeScale == 0f)
            return;

        if (!Input.GetKeyDown(interactKey))
            return;

        PlayerMovement player = GetPlayer();
        if (player == null)
            return;

        if (interactionAreaTouches > 0)
            StartCoroutine(Talk(player));
    }

    private bool CanInteract
    {
        get { return interactionAreaTouches > 0 && !isTalking && Time.timeScale != 0f; }
    }

    private PlayerMovement GetPlayer()
    {
        if (playerMovement == null)
            playerMovement = FindAnyObjectByType<PlayerMovement>();

        return playerMovement;
    }

    private IEnumerator Talk(PlayerMovement player)
    {
        isTalking = true;
        UpdatePrompt(false);

        bool previousCanMove = player.canMove;
        bool wasWanderEnabled = elderWander != null && elderWander.enabled;
        float previousAnimSpeed = elderAnim != null ? elderAnim.speed : 1f;
        Animator playerAnim = player.GetComponent<Animator>();
        float previousPlayerAnimSpeed = playerAnim != null ? playerAnim.speed : 1f;

        player.canMove = false;
        player.SetMenuActive(true);
        PlayPlayerIdle(player, playerAnim);
        PauseElder();

        if (dialogue != null)
            yield return dialogue.ShowAndWait();

        HasCompletedElderDialogue = true;

        if (missionStartBanner == null)
            missionStartBanner = dialogue != null && dialogue.missionStartBanner != null
                ? dialogue.missionStartBanner
                : MissionStartBanner.Instance != null
                ? MissionStartBanner.Instance
                : FindAnyObjectByType<MissionStartBanner>();

        if (missionStartBanner != null)
            yield return missionStartBanner.PlayAndWait();

        ResumeElder(wasWanderEnabled, previousAnimSpeed);
        ResumePlayerAnimator(playerAnim, previousPlayerAnimSpeed);

        player.canMove = previousCanMove;
        player.SetMenuActive(false);

        isTalking = false;
    }

    private void PlayPlayerIdle(PlayerMovement player, Animator playerAnim)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        PlayerAudio playerAudio = player.GetComponent<PlayerAudio>();
        if (playerAudio != null)
            playerAudio.StopWalk();

        if (playerAnim == null)
            return;

        playerAnim.speed = 1f;
        playerAnim.SetFloat("Speed", 0f);
        playerAnim.SetBool("IsGrounded", true);
        playerAnim.Play("idle", 0, 0f);
    }

    private void PauseElder()
    {
        if (elderWander != null)
            elderWander.enabled = false;

        if (elderRb != null)
            elderRb.linearVelocity = new Vector2(0f, elderRb.linearVelocity.y);

        if (elderAnim != null)
        {
            elderAnim.SetFloat("Speed", 0f);
            elderAnim.speed = 1f;
            elderAnim.Play("ElderIdle", 0, 0f);
        }
    }

    private void ResumeElder(bool wasWanderEnabled, float previousAnimSpeed)
    {
        if (elderAnim != null)
            elderAnim.speed = previousAnimSpeed;

        if (elderWander != null)
            elderWander.enabled = wasWanderEnabled;
    }

    private void ResumePlayerAnimator(Animator playerAnim, float previousPlayerAnimSpeed)
    {
        if (playerAnim != null)
            playerAnim.speed = previousPlayerAnimSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayerInteractionArea(other))
            return;

        interactionAreaTouches++;
        playerMovement = other.GetComponentInParent<PlayerMovement>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayerInteractionArea(other))
            return;

        interactionAreaTouches = Mathf.Max(0, interactionAreaTouches - 1);
    }

    private bool IsPlayerInteractionArea(Collider2D other)
    {
        if (!other.isTrigger)
            return false;

        return other.GetComponentInParent<PlayerMovement>() != null;
    }

    private void UpdatePrompt(bool show)
    {
        if (promptCanvas == null)
            return;

        if (promptCanvas.activeSelf != show)
            promptCanvas.SetActive(show);
    }
}
