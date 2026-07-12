using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EvolutionAltar : MonoBehaviour
{
    [Header("Requirement")]
    public int requiredExp = ExpProgress.RequiredExpForEvolve;
    public bool evolveOnlyOnce = true;

    [Header("Player Detection")]
    public string playerLayerName = "Player";
    public bool requirePlayerMovement = true;

    [Header("Evolution Result")]
    public Animator playerAnimator;
    public string evolveTriggerName = "Evolve";
    public GameObject objectToActivate;
    public GameObject objectToDeactivate;

    [Header("Messages")]
    public MissionStartBanner missionBanner;
    public string notEnoughExpMessage = "Butuh 5 EXP untuk berevolusi.";
    public string alreadyEvolvedMessage = "Kamu sudah berevolusi.";
    public string evolveMessage = "EVOLVE BERHASIL";

    private int playerLayer = -1;
    private bool isProcessing;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer(playerLayerName);

        Collider2D triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;

        if (missionBanner == null)
            missionBanner = MissionStartBanner.Instance != null
                ? MissionStartBanner.Instance
                : FindAnyObjectByType<MissionStartBanner>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isProcessing || !IsPlayer(other))
            return;

        TryEvolve(other.GetComponentInParent<PlayerMovement>());
    }

    private void TryEvolve(PlayerMovement player)
    {
        if (evolveOnlyOnce && ExpProgress.HasEvolved)
        {
            ShowMessage(alreadyEvolvedMessage);
            return;
        }

        if (ExpProgress.ExpCount < requiredExp)
        {
            ShowMessage(notEnoughExpMessage + " (" + ExpProgress.ExpCount + "/" + requiredExp + ")");
            return;
        }

        isProcessing = true;
        ExpProgress.MarkEvolved();

        if (playerAnimator == null && player != null)
            playerAnimator = player.GetComponent<Animator>();

        if (playerAnimator != null && !string.IsNullOrWhiteSpace(evolveTriggerName))
            playerAnimator.SetTrigger(evolveTriggerName);

        if (objectToActivate != null)
            objectToActivate.SetActive(true);

        if (objectToDeactivate != null)
            objectToDeactivate.SetActive(false);

        ShowMessage(evolveMessage);
    }

    private bool IsPlayer(Collider2D other)
    {
        if (other == null)
            return false;

        if (playerLayer >= 0 && other.gameObject.layer != playerLayer)
            return false;

        return !requirePlayerMovement || other.GetComponentInParent<PlayerMovement>() != null;
    }

    private void ShowMessage(string message)
    {
        if (missionBanner == null)
            missionBanner = MissionStartBanner.Instance != null
                ? MissionStartBanner.Instance
                : FindAnyObjectByType<MissionStartBanner>();

        if (missionBanner != null)
            missionBanner.PlayMessage(message);
        else
            Debug.Log(message);
    }
}
