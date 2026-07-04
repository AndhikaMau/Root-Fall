using UnityEngine;

// AI sederhana: NPC / Tetua berjalan bolak-balik kiri-kanan.
// Aset otomatis menghadap arah gerak (kanan saat ke kanan, kiri saat ke kiri).
public class NPCWander : MonoBehaviour
{
    public enum FlipMethod
    {
        SpriteFlipX,  // pakai SpriteRenderer.flipX (1 sprite tanpa child)
        LocalScale    // balik skala X (sama seperti Player, cocok jika ada child)
    }

    [Header("Gerak")]
    public float speed = 2f;
    public bool startMovingRight = true;
    [Tooltip("Jika ON, arah awal jalan diacak (abaikan Start Moving Right).")]
    public bool randomizeStartDirection = true;

    [Tooltip("Jika OFF: NPC jalan terus sampai menabrak barrier Bnpc. " +
             "Jika ON: NPC juga dibatasi jarak Patrol Distance dari titik spawn.")]
    public bool usePatrolDistanceLimit = false;
    public float patrolDistance = 3f;   // jarak setengah lintasan (hanya dipakai jika limit ON)

    [Header("Jeda saat berbalik")]
    public float turnPauseDuration = 0.5f;

    [Header("Berbalik saat menyentuh collider")]
    // Set ke layer "Bnpc" -> NPC berhenti & berbalik saat menyentuhnya.
    public LayerMask turnLayers;
    public bool debugLog = true;   // matikan setelah selesai debug

    [Header("Arah Hadap Aset")]
    public FlipMethod flipMethod = FlipMethod.SpriteFlipX;
    // Centang jika sprite default (tanpa flip) sudah menghadap KANAN.
    public bool spriteFacesRight = true;

    [Header("Animasi (opsional)")]
    public Animator anim;                // biarkan kosong jika NPC tidak beranimasi
    public string speedParam = "Speed";  // parameter float di Animator (boleh dikosongkan)
    [Tooltip("Tanpa clip idle: bekukan animasi jalan saat NPC berhenti di barrier.")]
    public bool freezeAnimWhenStopped = true;

    private Rigidbody2D rb;
    private float leftBound;
    private float rightBound;
    private int direction;               // -1 = kiri, +1 = kanan
    private float pauseTimer;
    private bool hasSpeedParam;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (anim == null)
            anim = GetComponent<Animator>();

        hasSpeedParam = AnimatorHasParam(anim, speedParam);
    }

    static bool AnimatorHasParam(Animator a, string paramName)
    {
        if (a == null || string.IsNullOrEmpty(paramName))
            return false;

        foreach (AnimatorControllerParameter p in a.parameters)
        {
            if (p.type == AnimatorControllerParameterType.Float && p.name == paramName)
                return true;
        }
        return false;
    }

    // Dipanggil tiap kali NPC diaktifkan (mis. saat di-spawn di posisi random).
    // Lintasan dihitung ulang dari posisi saat ini.
    void OnEnable()
    {
        InitPatrol();
    }

    void InitPatrol()
    {
        float startX = transform.position.x;
        leftBound = startX - patrolDistance;
        rightBound = startX + patrolDistance;

        if (randomizeStartDirection)
            direction = Random.value < 0.5f ? -1 : 1;
        else
            direction = startMovingRight ? 1 : -1;

        pauseTimer = 0f;
        ApplyFacing();
    }

    void Update()
    {
        if (Time.timeScale == 0f)
            return;

        // Sedang jeda (berhenti sejenak) di ujung / setelah menyentuh Bnpc
        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;

            // Bekukan animasi jalan agar terlihat diam (tanpa perlu clip idle)
            if (anim != null && freezeAnimWhenStopped && !hasSpeedParam)
                anim.speed = 0f;

            Move(0f);
            return;
        }

        // Pastikan animasi kembali berjalan setelah jeda
        if (anim != null && freezeAnimWhenStopped && !hasSpeedParam)
            anim.speed = 1f;

        // Batas jarak hanya berlaku jika diaktifkan.
        // Secara default NPC berbalik hanya saat menabrak barrier Bnpc.
        if (usePatrolDistanceLimit)
        {
            float x = transform.position.x;

            if (direction > 0 && x >= rightBound)
                Turn(-1);
            else if (direction < 0 && x <= leftBound)
                Turn(1);
        }

        Move(direction * speed);
    }

    void Move(float velocityX)
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(velocityX, rb.linearVelocity.y);
        }
        else
        {
            transform.Translate(Vector2.right * velocityX * Time.deltaTime, Space.World);
        }

        if (anim != null && hasSpeedParam)
            anim.SetFloat(speedParam, Mathf.Abs(velocityX));
    }

    void Turn(int newDirection)
    {
        direction = newDirection;
        pauseTimer = turnPauseDuration;
        ApplyFacing();
    }

    // ============================================================
    // Berhenti & berbalik saat menyentuh collider layer "Bnpc"
    // (mendukung collider solid maupun trigger)
    // ============================================================
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (debugLog)
            Debug.Log(name + " -> OnCollisionEnter2D dgn " + collision.collider.name);

        TryTurnFrom(collision.collider);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (debugLog)
            Debug.Log(name + " -> OnTriggerEnter2D dgn " + other.name);

        TryTurnFrom(other);
    }

    // Stay menangkap kasus NPC lahir (spawn random) sudah overlap barrier,
    // di mana OnTriggerEnter2D tidak pernah menyala.
    void OnTriggerStay2D(Collider2D other)
    {
        TryTurnFrom(other);
    }

    void TryTurnFrom(Collider2D other)
    {
        int otherLayer = other.gameObject.layer;
        bool match = ((1 << otherLayer) & turnLayers.value) != 0;

        if (!match)
            return;

        // Titik barrier terdekat dari posisi NPC
        float barrierX = other.bounds.center.x;

        // Hanya berbalik jika barrier berada DI ARAH gerak kita.
        // Ini mencegah flip berulang saat masih overlap (setelah berbalik
        // kita bergerak menjauh, jadi Stay berikutnya tidak memicu lagi).
        bool barrierAhead =
            (direction > 0 && barrierX >= transform.position.x) ||
            (direction < 0 && barrierX <= transform.position.x);

        if (!barrierAhead)
            return;

        if (debugLog)
            Debug.Log(name + " BERBALIK karena '" + other.name +
                      "' (layer=" + LayerMask.LayerToName(otherLayer) + ")");

        // Berbalik ke arah berlawanan (dan jeda sejenak = berhenti)
        Turn(-direction);
    }

    // Menyesuaikan arah hadap aset dengan arah gerak
    void ApplyFacing()
    {
        bool movingRight = direction > 0;
        bool faceRight = movingRight == spriteFacesRight;

        if (flipMethod == FlipMethod.SpriteFlipX)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.flipX = !faceRight;
        }
        else // LocalScale
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (faceRight ? 1f : -1f);
            transform.localScale = scale;
        }
    }

    // Garis bantu lintasan patroli di Scene view
    private void OnDrawGizmosSelected()
    {
        if (!usePatrolDistanceLimit)
            return;

        Gizmos.color = Color.cyan;
        Vector3 pos = transform.position;
        Vector3 l = new Vector3(pos.x - patrolDistance, pos.y, pos.z);
        Vector3 r = new Vector3(pos.x + patrolDistance, pos.y, pos.z);
        Gizmos.DrawLine(l, r);
        Gizmos.DrawWireSphere(l, 0.15f);
        Gizmos.DrawWireSphere(r, 0.15f);
    }
}
