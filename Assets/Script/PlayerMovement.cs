using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
public float speed = 5f;
public float jumpForce = 10f;
public float dashSpeed = 15f;
public float dashDuration = 0.2f;

private Rigidbody2D rb;
private Animator anim;

private bool isDashing = false;
private bool facingRight = true;

void Start()
{
    rb = GetComponent<Rigidbody2D>();
    anim = GetComponent<Animator>();
}

void Update()
{
    if (isDashing) return;

    float move = 0;

    if (Input.GetKey(KeyCode.A))
        move = -1;

    if (Input.GetKey(KeyCode.D))
        move = 1;

    rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);

    // Animator
    anim.SetFloat("Speed", Mathf.Abs(move));
    anim.SetBool("IsGrounded", Mathf.Abs(rb.linearVelocity.y) < 0.1f);

    // Flip karakter
    if (move > 0 && !facingRight)
        Flip();

    if (move < 0 && facingRight)
        Flip();

    // Lompat (W)
    if (Input.GetKeyDown(KeyCode.W))
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    // Dash (K)
    if (Input.GetKeyDown(KeyCode.K))
    {
        StartCoroutine(Dash());
    }
}

private System.Collections.IEnumerator Dash()
{
    isDashing = true;

    float direction = facingRight ? 1f : -1f;

    rb.linearVelocity = new Vector2(direction * dashSpeed, 0);

    yield return new WaitForSeconds(dashDuration);

    isDashing = false;
}

void Flip()
{
    facingRight = !facingRight;

    Vector3 scale = transform.localScale;
    scale.x *= -1;
    transform.localScale = scale;
}

}
