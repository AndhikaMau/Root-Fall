using UnityEngine;
using System.Collections.Generic;

// Mengatur NPC & Tetua:
// - Tidak ada NPC saat Main Menu.
// - Saat Start ditekan, meng-INSTANTIATE sejumlah NPC di posisi random.
//   NPC diambil acak dari daftar prefab, jadi 3 aset bisa dipakai berulang.
public class NPCManager : MonoBehaviour
{
    [Header("Prefab NPC (aset dipilih acak)")]
    [Tooltip("Isi dengan prefab NPC_1, NPC_2, NPC_3, dll. " +
             "Tiap NPC yang di-spawn memilih salah satu secara acak.")]
    public GameObject[] npcPrefabs;

    [Tooltip("Berapa banyak NPC acak yang dimunculkan saat Start.")]
    public int spawnCount = 5;

    [Header("Selalu di-spawn (mis. Tetua)")]
    [Tooltip("Prefab yang PASTI muncul, satu kali per elemen. Boleh kosong.")]
    public GameObject[] alwaysSpawn;

    [Header("Area Spawn NPC Biasa - Kiri (dunia)")]
    public float minX = -10f;
    public float maxX = 10f;

    [Header("Area Spawn Tetua / Always Spawn - Kanan (dunia)")]
    public float alwaysSpawnMinX = 18f;
    public float alwaysSpawnMaxX = 58f;

    [Tooltip("Acak posisi Y juga? Jika tidak, semua muncul di spawnY.")]
    public bool randomizeY = false;
    public float spawnY = 0f;
    public float minY = 0f;
    public float maxY = 0f;

    [Tooltip("Jarak minimum antar NPC agar tidak menumpuk (0 = boleh dekat).")]
    public float minSpacing = 1.5f;
    public int maxPlacementTries = 20;

    [Tooltip("Parent untuk NPC hasil spawn (opsional, biar Hierarchy rapi).")]
    public Transform spawnParent;

    private readonly List<float> usedNpcX = new List<float>();
    private readonly List<float> usedAlwaysX = new List<float>();
    private readonly List<GameObject> spawned = new List<GameObject>();

    // Dipanggil saat menekan tombol Start
    public void SpawnNPCs()
    {
        ClearSpawned();
        usedNpcX.Clear();
        usedAlwaysX.Clear();

        // 1) Yang pasti muncul (mis. Tetua)
        if (alwaysSpawn != null)
        {
            foreach (GameObject prefab in alwaysSpawn)
            {
                if (prefab != null)
                    SpawnOne(prefab, alwaysSpawnMinX, alwaysSpawnMaxX, usedAlwaysX);
            }
        }

        // 2) NPC acak sebanyak spawnCount
        if (npcPrefabs != null && npcPrefabs.Length > 0)
        {
            for (int i = 0; i < spawnCount; i++)
            {
                GameObject prefab = npcPrefabs[Random.Range(0, npcPrefabs.Length)];
                if (prefab != null)
                    SpawnOne(prefab, minX, maxX, usedNpcX);
            }
        }
    }

    void SpawnOne(GameObject prefab, float areaMinX, float areaMaxX, List<float> used)
    {
        Vector3 pos = GetRandomPosition(areaMinX, areaMaxX, used);
        used.Add(pos.x);

        GameObject npc = Instantiate(prefab, pos, Quaternion.identity, spawnParent);
        npc.SetActive(true);
        spawned.Add(npc);
    }

    public void ClearSpawned()
    {
        foreach (GameObject npc in spawned)
        {
            if (npc != null)
                Destroy(npc);
        }
        spawned.Clear();
    }

    Vector3 GetRandomPosition(float areaMinX, float areaMaxX, List<float> used)
    {
        float min = Mathf.Min(areaMinX, areaMaxX);
        float max = Mathf.Max(areaMinX, areaMaxX);
        float x = Random.Range(min, max);

        // Coba cari X yang tidak terlalu dekat dengan NPC lain
        if (minSpacing > 0f)
        {
            for (int i = 0; i < maxPlacementTries; i++)
            {
                bool tooClose = false;

                foreach (float ux in used)
                {
                    if (Mathf.Abs(ux - x) < minSpacing)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                    break;

                x = Random.Range(min, max);
            }
        }

        float y = randomizeY ? Random.Range(minY, maxY) : spawnY;

        return new Vector3(x, y, 0f);
    }

    // Garis bantu area spawn di Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        float y = randomizeY ? (minY + maxY) * 0.5f : spawnY;
        Vector3 l = new Vector3(minX, y, 0f);
        Vector3 r = new Vector3(maxX, y, 0f);
        Gizmos.DrawLine(l, r);
        Gizmos.DrawWireSphere(l, 0.2f);
        Gizmos.DrawWireSphere(r, 0.2f);

        Gizmos.color = Color.cyan;
        Vector3 alwaysL = new Vector3(alwaysSpawnMinX, y + 0.25f, 0f);
        Vector3 alwaysR = new Vector3(alwaysSpawnMaxX, y + 0.25f, 0f);
        Gizmos.DrawLine(alwaysL, alwaysR);
        Gizmos.DrawWireSphere(alwaysL, 0.2f);
        Gizmos.DrawWireSphere(alwaysR, 0.2f);
    }
}
