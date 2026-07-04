using UnityEngine;

// Menampilkan panel "Player Control" setelah kamera fokus ke Asep.
// Pemain menutupnya dengan menekan tombol / klik, lalu baru bisa bergerak.
public class ControlGuide : MonoBehaviour
{
    [Header("Panel UI")]
    public GameObject panel;   // panel berisi daftar kontrol

    [Header("Cara Menutup")]
    public bool dismissOnAnyKey = true;
    public KeyCode dismissKey = KeyCode.Space;   // dipakai jika dismissOnAnyKey = false

    // Dibaca MainMenuManager untuk menunggu sampai panel ditutup.
    public bool IsShowing { get; private set; }

    private bool armed;   // mulai mendengar input 1 frame setelah tampil

    void Awake()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    public void Show()
    {
        if (panel != null)
            panel.SetActive(true);

        IsShowing = true;
        armed = false;   // abaikan input di frame yang sama saat muncul
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);

        IsShowing = false;
    }

    void Update()
    {
        if (!IsShowing)
            return;

        // Lewati satu frame agar klik/tekan pemicu tidak langsung menutup panel
        if (!armed)
        {
            armed = true;
            return;
        }

        bool pressed = dismissOnAnyKey
            ? (Input.anyKeyDown || Input.GetMouseButtonDown(0))
            : Input.GetKeyDown(dismissKey);

        if (pressed)
            Hide();
    }
}
