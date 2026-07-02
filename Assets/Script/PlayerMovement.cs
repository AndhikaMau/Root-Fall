using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 10f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public bool canMove = false;
    private bool menuActive = false;

    private Rigidbody2D rb;
    private Animator anim;
    private PlayerHealth health;
    private PlayerAudio playerAudio;

    private bool isGrounded;
    private bool wasGrounded;
    private bool isDashing = false;
    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        health = GetComponent<PlayerHealth>();
        playerAudio = GetComponent<PlayerAudio>();
    }

    public void SetMenuActive(bool active)
    {
        menuActive = active;
    }

    void Update()
    {
        if (Time.timeScale == 0f)
            return;

        bool isMainMenuScene = SceneManager.GetActiveScene().name == "MainMenu";
        bool shouldPauseAnimation = !canMove || isMainMenuScene || menuActive;

        // Ground Check tetap berjalan
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer);

        if (health != null && health.IsDead)
        {
            if (anim != null)
            {
                anim.speed = 0f;
                anim.SetFloat("Speed", 0);
            }
            return;
        }

        // ===========================
        // MAIN MENU
        // ===========================
        if (shouldPauseAnimation)
        {
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            if (anim != null)
            {
                anim.speed = 0f;
                anim.SetFloat("Speed", 0);
                anim.SetBool("IsGrounded", true);

                // Ganti "Idle" jika nama state Idle berbeda
                anim.Play("idle", 0, 0f);
            }

            return;
        }

        if (anim != null)
            anim.speed = 1f;

        anim.SetBool("IsGrounded", isGrounded);

        if (isDashing)
            return;

        // Suara saat baru mendarat
        if (!wasGrounded && isGrounded)
        {
            if (playerAudio != null)
                playerAudio.PlayLand();
        }

        wasGrounded = isGrounded;

        float move = 0;

        if (Input.GetKey(KeyCode.A))
            move = -1;

        if (Input.GetKey(KeyCode.D))
            move = 1;

        rb.linearVelocity =
            new Vector2(
                move * speed,
                rb.linearVelocity.y);

        anim.SetFloat("Speed", Mathf.Abs(move));

        if (move > 0 && !facingRight)
            Flip();

        if (move < 0 && facingRight)
            Flip();

        // Jump
        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            rb.linearVelocity =
                new Vector2(
                    rb.linearVelocity.x,
                    jumpForce);
        }

        // Dash
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (playerAudio != null)
                playerAudio.PlayDash();

            StartCoroutine(Dash());
        }
    }

    private System.Collections.IEnumerator Dash()
    {
        isDashing = true;

        float direction = facingRight ? 1f : -1f;

        float currentSpeed = dashSpeed;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            rb.linearVelocity =
                new Vector2(
                    direction * currentSpeed,
                    rb.linearVelocity.y);

            currentSpeed = Mathf.Lerp(
                dashSpeed,
                speed,
                elapsed / dashDuration);

            elapsed += Time.deltaTime;

            yield return null;
        }

        isDashing = false;
    }

    void Flip()
    {
        facingRight = !facingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius);
    }
}