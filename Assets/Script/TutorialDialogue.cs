using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialDialogue : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueLine
    {
        public string speaker;
        [TextArea(2, 5)]
        public string text;
    }

    [Header("UI")]
    public GameObject panel;
    public Text speakerText;
    public Text bodyText;
    public Button nextButton;
    public Sprite dialogBoxSprite;
    public MissionStartBanner missionStartBanner;

    [Header("Input")]
    public KeyCode nextKey = KeyCode.Space;

    [Header("Dialog Asep & Tetua")]
    public DialogueLine[] lines =
    {
        new DialogueLine
        {
            speaker = "Tetua",
            text = "Asep... kau melihatnya sendiri, bukan? Cahaya Ember Roots terus meredup dari hari ke hari. Jika ini terus berlanjut, bukan hanya pohon-pohon yang mati, tetapi seluruh koloni ini juga akan kehilangan sumber kehidupannya."
        },
        new DialogueLine
        {
            speaker = "Asep",
            text = "Aku juga merasakannya. Bahkan tanaman di sekitar rumahku mulai layu. Apa sebenarnya yang menyebabkan semua ini terjadi?"
        },
        new DialogueLine
        {
            speaker = "Tetua",
            text = "Kami tidak mengetahui penyebab pastinya. Namun semua energi Ember Roots berasal dari akar utama yang berada jauh di bawah tanah. Jika ada sesuatu yang mengganggu sumbernya, maka seluruh jaringan akar akan ikut melemah."
        },
        new DialogueLine
        {
            speaker = "Asep",
            text = "Jadi sumber masalahnya mungkin berada di dalam akar itu?"
        },
        new DialogueLine
        {
            speaker = "Tetua",
            text = "Kemungkinan besar. Pergilah dan temukan jalan menuju bawah tanah. Cari tahu apa yang terjadi pada Ember Roots sebelum cahaya terakhirnya benar-benar padam."
        },
        new DialogueLine
        {
            speaker = "Asep",
            text = "Aku mengerti. Aku akan mencari jawabannya."
        }
    };

    public bool IsShowing { get; private set; }

    private bool advanceRequested;

    private void Awake()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(RequestAdvance);

        Hide();
    }

    private void Update()
    {
        if (!IsShowing)
            return;

        if (Input.GetKeyDown(nextKey))
            advanceRequested = true;
    }

    public IEnumerator ShowAndWait()
    {
        if (panel == null || lines == null || lines.Length == 0)
            yield break;

        IsShowing = true;

        if (panel != null)
            panel.SetActive(true);

        for (int i = 0; i < lines.Length; i++)
        {
            ShowLine(lines[i]);
            advanceRequested = false;

            yield return null;

            while (!advanceRequested)
                yield return null;
        }

        Hide();
    }

    public void RequestAdvance()
    {
        advanceRequested = true;
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);

        IsShowing = false;
        advanceRequested = false;
    }

    private void ShowLine(DialogueLine line)
    {
        if (speakerText != null)
            speakerText.text = line.speaker;

        if (bodyText != null)
            bodyText.text = line.text;
    }

}
