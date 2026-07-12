using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float acceleration = 45f;
    public float deceleration = 55f;
    public float jumpForce = 10f;
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.12f;
    public float jumpCutMultiplier = 0.5f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.35f;
    public float wallCheckDistance = 0.05f;

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public bool canMove = false;
    private bool menuActive = false;

    private Rigidbody2D rb;
    private Collider2D bodyCollider;
    private Animator anim;
    private PlayerHealth health;
    private PlayerAudio playerAudio;

    private bool isGrounded;
    private bool wasGrounded;
    private bool isDashing = false;
    private bool facingRight = true;
    private bool wasMovementLocked;
    private float moveInput;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private float dashCooldownCounter;
    private readonly RaycastHit2D[] wallHits = new RaycastHit2D[4];

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        health = GetComponent<PlayerHealth>();
        playerAudio = GetComponent<PlayerAudio>();

        ApplyFrictionlessMaterial();
    }

    public void SetMenuActive(bool active)
    {
        menuActive = active;
    }

    void Update()
    {
        if (Time.timeScale == 0f)
        {
            if (playerAudio != null)
                playerAudio.StopWalk();

            return;
        }

        bool isMainMenuScene = SceneManager.GetActiveScene().name == "MainMenu";
        bool shouldLockMovement = !canMove || isMainMenuScene || menuActive;

        UpdateGroundedState();

        if (health != null && health.IsDead)
        {
            if (playerAudio != null)
                playerAudio.StopWalk();

            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            if (anim != null)
            {
                anim.speed = 1f;
                anim.SetFloat("Speed", 0);
            }
            return;
        }

        // ===========================
        // MAIN MENU
        // ===========================
        if (shouldLockMovement)
        {
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            if (anim != null)
            {
                anim.speed = 1f;
                anim.SetFloat("Speed", 0);
                anim.SetBool("IsGrounded", true);

                if (!wasMovementLocked)
                    anim.Play("idle", 0, 0f);
            }

            if (playerAudio != null)
                playerAudio.StopWalk();

            wasMovementLocked = true;
            wasGrounded = isGrounded;
            return;
        }

        wasMovementLocked = false;

        if (anim != null)
            anim.speed = 1f;

        if (anim != null)
            anim.SetBool("IsGrounded", isGrounded);

        if (isDashing)
        {
            if (playerAudio != null)
                playerAudio.StopWalk();

            return;
        }

        // Suara saat baru mendarat
        if (!wasGrounded && isGrounded)
        {
            if (playerAudio != null)
                playerAudio.PlayLand();
        }

        wasGrounded = isGrounded;

        ReadMoveInput();
        UpdateJumpTimers();
        ApplyHorizontalMovement();

        if (anim != null)
            anim.SetFloat("Speed", Mathf.Abs(moveInput));

        if (playerAudio != null)
        {
            if (moveInput != 0 && isGrounded && Mathf.Abs(rb.linearVelocity.x) > 0.05f)
                playerAudio.PlayWalk();
            else
                playerAudio.StopWalk();
        }

        if (moveInput > 0 && !facingRight)
            Flip();

        if (moveInput < 0 && facingRight)
            Flip();

        TryJump();
        CutJumpWhenReleased();

        // Dash
        dashCooldownCounter -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.K) && dashCooldownCounter <= 0f)
        {
            if (playerAudio != null)
            {
                playerAudio.StopWalk();
                playerAudio.PlayDash();
            }

            StartCoroutine(Dash());
        }
    }

    private System.Collections.IEnumerator Dash()
    {
        isDashing = true;
        dashCooldownCounter = dashCooldown;

        float direction = facingRight ? 1f : -1f;

        float currentSpeed = dashSpeed;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            if (IsBlockedHorizontally(direction))
                break;

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
        rb.linearVelocity = new Vector2(Mathf.Clamp(rb.linearVelocity.x, -speed, speed), rb.linearVelocity.y);
    }

    private void UpdateGroundedState()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer);
    }

    private void ReadMoveInput()
    {
        moveInput = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            moveInput = -1f;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            moveInput = 1f;
    }

    private void ApplyHorizontalMovement()
    {
        float targetVelocityX = moveInput * speed;

        if (moveInput != 0f && IsBlockedHorizontally(moveInput))
            targetVelocityX = 0f;

        float rate = Mathf.Abs(targetVelocityX) > 0f ? acceleration : deceleration;
        float velocityX = Mathf.MoveTowards(rb.linearVelocity.x, targetVelocityX, rate * Time.deltaTime);

        rb.linearVelocity = new Vector2(velocityX, rb.linearVelocity.y);
    }

    private void UpdateJumpTimers()
    {
        if (isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;
    }

    private void TryJump()
    {
        if (jumpBufferCounter <= 0f || coyoteCounter <= 0f)
            return;

        if (playerAudio != null)
            playerAudio.StopWalk();

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpBufferCounter = 0f;
        coyoteCounter = 0f;

        if (playerAudio != null)
            playerAudio.PlayJump();
    }

    private void CutJumpWhenReleased()
    {
        bool releasedJump = Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.Space);

        if (releasedJump && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
    }

    private bool IsBlockedHorizontally(float direction)
    {
        if (bodyCollider == null || Mathf.Approximately(direction, 0f))
            return false;

        int hitCount = bodyCollider.Cast(new Vector2(Mathf.Sign(direction), 0f), wallHits, wallCheckDistance);

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D hit = wallHits[i];

            if (hit.collider == null || hit.collider.isTrigger)
                continue;

            if (hit.normal.x * Mathf.Sign(direction) < -0.5f)
                return true;
        }

        return false;
    }

    private void ApplyFrictionlessMaterial()
    {
        if (bodyCollider == null || bodyCollider.sharedMaterial != null)
            return;

        PhysicsMaterial2D material = new PhysicsMaterial2D("Player_Runtime_NoFriction")
        {
            friction = 0f,
            bounciness = 0f
        };

        bodyCollider.sharedMaterial = material;
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
