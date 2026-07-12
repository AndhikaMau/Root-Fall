using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PeriodicHazard2D : MonoBehaviour
{
    [Header("Timing")]
    public float startDelay = 0f;
    public float warningTime = 0.4f;
    public float activeTime = 1f;
    public float inactiveTime = 1.5f;

    [Header("Damage")]
    public int damage = 1;
    public string playerLayerName = "Player";
    public bool requirePlayerHealth = true;

    [Header("Visual")]
    public GameObject warningVisual;
    public GameObject activeVisual;

    [Header("Audio")]
    public AudioClip activateClip;
    public float volume = 1f;

    private Collider2D hazardCollider;
    private int playerLayer = -1;
    private bool isActive;

    private void Awake()
    {
        hazardCollider = GetComponent<Collider2D>();
        hazardCollider.isTrigger = true;
        hazardCollider.enabled = false;

        playerLayer = LayerMask.NameToLayer(playerLayerName);
        SetVisuals(false, false);
    }

    private void OnEnable()
    {
        StartCoroutine(HazardRoutine());
    }

    private IEnumerator HazardRoutine()
    {
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        while (enabled)
        {
            isActive = false;
            hazardCollider.enabled = false;
            SetVisuals(false, false);

            if (inactiveTime > 0f)
                yield return new WaitForSeconds(inactiveTime);

            SetVisuals(true, false);

            if (warningTime > 0f)
                yield return new WaitForSeconds(warningTime);

            isActive = true;
            hazardCollider.enabled = true;
            SetVisuals(false, true);
            PlayActivateSound();

            if (activeTime > 0f)
                yield return new WaitForSeconds(activeTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    private void TryDamagePlayer(Collider2D other)
    {
        if (!isActive || other == null)
            return;

        if (playerLayer >= 0 && other.gameObject.layer != playerLayer)
            return;

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
        {
            if (requirePlayerHealth)
                return;

            playerHealth = FindAnyObjectByType<PlayerHealth>();
        }

        if (playerHealth != null)
            playerHealth.TakeDamage(damage);
    }

    private void SetVisuals(bool showWarning, bool showActive)
    {
        if (warningVisual != null)
            warningVisual.SetActive(showWarning);

        if (activeVisual != null)
            activeVisual.SetActive(showActive);
    }

    private void PlayActivateSound()
    {
        if (activateClip == null)
            return;

        AudioSource.PlayClipAtPoint(activateClip, transform.position, volume);
    }
}
