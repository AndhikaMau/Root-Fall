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

    [Header("Area Spawn Random (dunia)")]
    public float minX = -10f;
    public float maxX = 10f;

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

    private readonly List<float> usedX = new List<float>();
    private readonly List<GameObject> spawned = new List<GameObject>();

    // Dipanggil saat menekan tombol Start
    public void SpawnNPCs()
    {
        ClearSpawned();
        usedX.Clear();

        // 1) Yang pasti muncul (mis. Tetua)
        if (alwaysSpawn != null)
        {
            foreach (GameObject prefab in alwaysSpawn)
            {
                if (prefab != null)
                    SpawnOne(prefab);
            }
        }

        // 2) NPC acak sebanyak spawnCount
        if (npcPrefabs != null && npcPrefabs.Length > 0)
        {
            for (int i = 0; i < spawnCount; i++)
            {
                GameObject prefab = npcPrefabs[Random.Range(0, npcPrefabs.Length)];
                if (prefab != null)
                    SpawnOne(prefab);
            }
        }
    }

    void SpawnOne(GameObject prefab)
    {
        Vector3 pos = GetRandomPosition(usedX);
        usedX.Add(pos.x);

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

    Vector3 GetRandomPosition(List<float> used)
    {
        float x = Random.Range(minX, maxX);

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

                x = Random.Range(minX, maxX);
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
    }
}
