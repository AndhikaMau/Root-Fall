using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
public Transform attackPoint;
public float attackRange = 1f;
public int attackDamage = 1;
public LayerMask enemyLayer;

public float attackCooldown = 1f;

private float nextAttackTime;
private Animator anim;
private PlayerHealth health;
private PlayerAudio playerAudio;

void Start()
{
    anim = GetComponent<Animator>();
    health = GetComponent<PlayerHealth>();
    playerAudio = GetComponent<PlayerAudio>();
}

void Update()
{
    if (Time.timeScale == 0f)
    return;
    
    // Jika mati tidak bisa menyerang
    if (health != null && health.IsDead)
        return;

    if (Input.GetKeyDown(KeyCode.J) &&
        Time.time >= nextAttackTime)
    {
        anim.SetTrigger("Attack");

        if (playerAudio != null)
            playerAudio.PlayAttack();

        nextAttackTime =
            Time.time + attackCooldown;
    }
}

// Dipanggil oleh Animation Event
public void DealDamage()
{
    Collider2D[] hitEnemies =
        Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayer);

    foreach (Collider2D enemy in hitEnemies)
    {
        EnemyHealth enemyHealth =
            enemy.GetComponent<EnemyHealth>();

        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(
                attackDamage);
        }
    }
}

private void OnDrawGizmosSelected()
{
    if (attackPoint == null)
        return;

    Gizmos.color = Color.red;

    Gizmos.DrawWireSphere(
        attackPoint.position,
        attackRange);
}

}
