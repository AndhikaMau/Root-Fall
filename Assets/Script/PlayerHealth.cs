using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public float invincibleTime = 1f;

    [Header("Death")]
    public string deathAnimationName = "playerdeath";
    public string idleAnimationName = "idle";
    public float deathAnimationDuration = 1f;

    [Header("Checkpoint")]
    public string checkpointLayerName = "Checkpoint";
    public Vector2 checkpointRespawnOffset = new Vector2(0f, 0.35f);

    // UI Hati
    public HealthUI healthUI;

    // Panel Game Over
    public GameObject gameOverPanel;
    public Button gameOverMainButton;
    public Button gameOverExitButton;

    private int currentHealth;
    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private PlayerAudio playerAudio;

    private bool isInvincible;
    private Vector3 checkpointPosition;
    private bool hasCheckpoint;
    private int checkpointLayer = -1;
    private Coroutine gameOverRoutine;
    private PanelButtonNavigator gameOverNavigator;
    private bool gameOverButtonsBound;

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
            BindGameOverButtons();
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
        currentHealth = Mathf.Max(0, currentHealth);

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

    IEnumerator ShowGameOverAfterDeath()
    {
        // tunggu animasi mati selesai
        yield return new WaitForSeconds(deathAnimationDuration);

        if (gameOverPanel != null)
        {
            BindGameOverButtons();
            SetupGameOverKeyboardNavigation();
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;
        gameOverRoutine = null;
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

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (anim != null)
            anim.SetFloat("Speed", 0);

        if (healthUI != null)
        {
            healthUI.UpdateHearts(0);
        }

        if (anim != null)
        {
            anim.speed = 1f;
            anim.ResetTrigger("Hurt");

            if (!string.IsNullOrWhiteSpace(deathAnimationName))
                anim.Play(deathAnimationName, 0, 0f);
        }

        if (gameOverRoutine != null)
            StopCoroutine(gameOverRoutine);

        gameOverRoutine = StartCoroutine(ShowGameOverAfterDeath());

        Debug.Log("Player Mati");
    }

    public void RespawnFromGameOver()
    {
        if (!hasCheckpoint)
            checkpointPosition = transform.position;

        Time.timeScale = 1f;

        if (gameOverRoutine != null)
        {
            StopCoroutine(gameOverRoutine);
            gameOverRoutine = null;
        }

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

            if (!string.IsNullOrWhiteSpace(idleAnimationName))
                anim.Play(idleAnimationName, 0, 0f);
        }

        StartCoroutine(Invincibility());
    }

    public void QuitFromGameOver()
    {
        Time.timeScale = 1f;
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void BindGameOverButtons()
    {
        if (gameOverPanel == null || gameOverButtonsBound)
            return;

        Button[] buttons = gameOverPanel.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            if (button == null)
                continue;

            string buttonName = button.gameObject.name.ToLowerInvariant();

            if (gameOverMainButton == null && (buttonName.Contains("main") || buttonName.Contains("restart") || buttonName.Contains("respawn")))
                gameOverMainButton = button;
            else if (gameOverExitButton == null && (buttonName.Contains("exit") || buttonName.Contains("quit") || buttonName.Contains("keluar")))
                gameOverExitButton = button;
        }

        if (gameOverMainButton != null)
            gameOverMainButton.onClick.AddListener(RespawnFromGameOver);

        if (gameOverExitButton != null)
            gameOverExitButton.onClick.AddListener(QuitFromGameOver);

        gameOverButtonsBound = true;
    }

    private void SetupGameOverKeyboardNavigation()
    {
        if (gameOverPanel == null)
            return;

        if (gameOverNavigator == null)
            gameOverNavigator = gameOverPanel.GetComponent<PanelButtonNavigator>();

        if (gameOverNavigator == null)
            gameOverNavigator = gameOverPanel.AddComponent<PanelButtonNavigator>();

        if (gameOverMainButton != null && gameOverExitButton != null)
            gameOverNavigator.buttons = new[] { gameOverMainButton, gameOverExitButton };

        gameOverNavigator.sortButtonsTopToBottom = false;
        gameOverNavigator.wrapSelection = false;
        gameOverNavigator.useEventSystemSelection = false;
        gameOverNavigator.Setup();
    }
}
