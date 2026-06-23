using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public float invincibleTime = 1f;

    private int currentHealth;
    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private bool isInvincible;

    public bool IsDead { get; private set; }

    void Start()
    {
        currentHealth = maxHealth;

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(int damage)
    {
        if (IsDead || isInvincible)
            return;

        currentHealth -= damage;

        Debug.Log("Player HP: " + currentHealth);

        StartCoroutine(DamageFlash());

        if (currentHealth > 0)
        {
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

    void Die()
    {
        if (IsDead)
            return;

        IsDead = true;

        rb.linearVelocity =
            new Vector2(0f, rb.linearVelocity.y);

        anim.SetFloat("Speed", 0);

        // Paksa langsung masuk animasi mati
        anim.Play("playerdeath", 0, 0f);

        Debug.Log("Player Mati");
    }
}