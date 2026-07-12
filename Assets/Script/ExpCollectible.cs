using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class ExpCollectible : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("Isi unik untuk tiap EXP, contoh: Stage1_EXP_1 sampai Stage1_EXP_5.")]
    public string collectibleId;

    [Header("Player Detection")]
    public string playerLayerName = "Player";
    public bool requirePlayerMovement = true;

    [Header("Feedback")]
    public AudioClip pickupClip;
    public AudioSource pickupAudio;
    public float pickupVolume = 1f;
    public bool destroyAfterCollected = true;

    private int playerLayer = -1;
    private bool isCollected;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer(playerLayerName);

        Collider2D triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;

        if (string.IsNullOrWhiteSpace(collectibleId))
            collectibleId = BuildDefaultId();

        if (ExpProgress.HasCollected(collectibleId))
            gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected || !IsPlayer(other))
            return;

        Collect();
    }

    private void Collect()
    {
        if (!ExpProgress.TryCollect(collectibleId))
            return;

        isCollected = true;

        PlayPickupSound();

        if (destroyAfterCollected)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    private bool IsPlayer(Collider2D other)
    {
        if (other == null)
            return false;

        if (playerLayer >= 0 && other.gameObject.layer != playerLayer)
            return false;

        return !requirePlayerMovement || other.GetComponentInParent<PlayerMovement>() != null;
    }

    private string BuildDefaultId()
    {
        return SceneManager.GetActiveScene().name + "_" + gameObject.name;
    }

    private void PlayPickupSound()
    {
        AudioClip clip = pickupClip;
        float volume = pickupVolume;

        if (clip == null && pickupAudio != null)
        {
            clip = pickupAudio.clip;
            volume = pickupAudio.volume;
        }

        if (clip == null)
            return;

        GameObject soundObject = new GameObject("ExpPickupSound");
        AudioSource audioSource = soundObject.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;
        audioSource.Play();

        Destroy(soundObject, clip.length + 0.1f);
    }
}
