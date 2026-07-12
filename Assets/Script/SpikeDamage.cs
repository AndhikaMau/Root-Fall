using UnityEngine;

public class SpikeDamage : MonoBehaviour
{
    public int damage = 1;
    public string playerLayerName = "Player";
    public bool requirePlayerHealth = true;

    private int playerLayer = -1;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer(playerLayerName);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision.collider);
    }

    private void TryDamagePlayer(Collider2D other)
    {
        if (other == null)
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
}
