using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public float invincibleTime = 1f;

    [Header("Checkpoint")]
    public string checkpointLayerName = "Checkpoint";
    public Vector2 checkpointRespawnOffset = new Vector2(0f, 0.35f);
    public bool respawnAtCheckpointOnDeath = true;
    public float respawnDelay = 1f;

    // UI Hati
    public HealthUI healthUI;

    // Panel Game Over
    public GameObject gameOverPanel;

    private int currentHealth;
    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private PlayerAudio playerAudio;

    private bool isInvincible;
    private Vector3 checkpointPosition;
    private bool hasCheckpoint;
    private int checkpointLayer = -1;

    public bool IsDead { get; private set; }

    void Start()
    {
        currentHealth = maxHealth;

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        playerAudio = GetComponent<PlayerAudio>();
        checkpointLayer = LayerMask.NameToLayer(checkpointLayerName);
        checkpointPosition = transform.position;
        hasCheckpoint = true;

        if (healthUI != null)
        {
            healthUI.UpdateHearts(currentHealth);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TrySetCheckpoint(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TrySetCheckpoint(collision.collider);
    }

    private void TrySetCheckpoint(Collider2D other)
    {
        if (other == null || checkpointLayer < 0)
            return;

        if (other.gameObject.layer != checkpointLayer)
            return;

        SetCheckpoint(other.transform.position + (Vector3)checkpointRespawnOffset);
    }

    public void SetCheckpoint(Vector3 position)
    {
        checkpointPosition = position;
        hasCheckpoint = true;
        Debug.Log("Checkpoint tersimpan: " + checkpointPosition);
    }

    public void TakeDamage(int damage)
    {
        if (IsDead || isInvincible)
            return;

        currentHealth -= damage;

        if (healthUI != null)
        {
            healthUI.UpdateHearts(currentHealth);
        }

        Debug.Log("Player HP: " + currentHealth);

        StartCoroutine(DamageFlash());

        if (currentHealth > 0)
        {
            if (playerAudio != null)
                playerAudio.PlayHurt();

            anim.SetTrigger("Hurt");
            StartCoroutine(Invincibility());
        }
        else
        {
            Die();
        }
    }

    IEnumerator DamageFlash()
    {
        for (int i = 0; i < 3; i++)
        {
            sr.color = Color.red;

            yield return new WaitForSeconds(0.08f);

            sr.color = Color.white;

            yield return new WaitForSeconds(0.08f);
        }
    }

    IEnumerator Invincibility()
    {
        isInvincible = true;

        yield return new WaitForSeconds(invincibleTime);

        isInvincible = false;
    }

    IEnumerator ShowGameOver()
    {
        // tunggu animasi mati selesai
        yield return new WaitForSecondsRealtime(1.0f);

        if (gameOverPanel != null)
        {
            PanelButtonNavigator navigator = gameOverPanel.GetComponent<PanelButtonNavigator>();
            if (navigator == null)
                navigator = gameOverPanel.AddComponent<PanelButtonNavigator>();

            navigator.Setup();
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    void Die()
    {
        if (IsDead)
            return;

        IsDead = true;

        if (playerAudio != null)
        {
            playerAudio.StopWalk();
            playerAudio.PlayDeath();
        }

        rb.linearVelocity =
            new Vector2(0f, rb.linearVelocity.y);

        anim.SetFloat("Speed", 0);

        if (healthUI != null)
        {
            healthUI.UpdateHearts(0);
        }

        // Paksa langsung masuk animasi mati
        anim.Play("playerdeath", 0, 0f);

        if (respawnAtCheckpointOnDeath && hasCheckpoint)
            StartCoroutine(RespawnAtCheckpointAfterDelay());
        else
            StartCoroutine(ShowGameOver());

        Debug.Log("Player Mati");
    }

    private IEnumerator RespawnAtCheckpointAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);

        transform.position = checkpointPosition;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        currentHealth = maxHealth;
        IsDead = false;
        isInvincible = false;

        if (healthUI != null)
            healthUI.UpdateHearts(currentHealth);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (sr != null)
            sr.color = Color.white;

        if (anim != null)
        {
            anim.speed = 1f;
            anim.ResetTrigger("Hurt");
            anim.SetFloat("Speed", 0f);
            anim.SetBool("IsGrounded", true);
            anim.Play("idle", 0, 0f);
        }

        StartCoroutine(Invincibility());
    }
}
