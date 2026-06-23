using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
public float speed = 5f;
public float jumpForce = 10f;
public float dashSpeed = 15f;
public float dashDuration = 0.2f;

public Transform groundCheck;
public float groundCheckRadius = 0.2f;
public LayerMask groundLayer;

private Rigidbody2D rb;
private Animator anim;
private PlayerHealth health;

private bool isGrounded;
private bool isDashing = false;
private bool facingRight = true;

void Start()
{
    rb = GetComponent<Rigidbody2D>();
    anim = GetComponent<Animator>();
    health = GetComponent<PlayerHealth>();
}

void Update()
{   
    if (Time.timeScale == 0f)
    return;

    // Jika mati, hentikan semua kontrol
    if (health != null && health.IsDead)
    {
        anim.SetFloat("Speed", 0);
        return;
    }

    if (isDashing)
        return;

    isGrounded = Physics2D.OverlapCircle(
        groundCheck.position,
        groundCheckRadius,
        groundLayer);

    float move = 0;

    if (Input.GetKey(KeyCode.A))
        move = -1;

    if (Input.GetKey(KeyCode.D))
        move = 1;

    rb.linearVelocity =
        new Vector2(move * speed,
                    rb.linearVelocity.y);

    // Animator
    anim.SetFloat("Speed", Mathf.Abs(move));
    anim.SetBool("IsGrounded", isGrounded);

    // Flip karakter
    if (move > 0 && !facingRight)
        Flip();

    if (move < 0 && facingRight)
        Flip();

    // Lompat
    if (Input.GetKeyDown(KeyCode.W) &&
        isGrounded)
    {
        rb.linearVelocity =
            new Vector2(
                rb.linearVelocity.x,
                jumpForce);
    }

    // Dash
    if (Input.GetKeyDown(KeyCode.K))
    {
        StartCoroutine(Dash());
    }
}

private System.Collections.IEnumerator Dash()
{
    isDashing = true;

    float direction =
        facingRight ? 1f : -1f;

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

    Vector3 scale =
        transform.localScale;

    scale.x *= -1;

    transform.localScale =
        scale;
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
