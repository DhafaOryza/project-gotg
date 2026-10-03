using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PreparationUI : MonoBehaviour
{
    [SerializeField] private GameObject RootUI;
    [SerializeField] private GameObject skillSelectorPanel;

    [Header("Header")]
    [SerializeField] private TextMeshProUGUI nightCounter;

    [Header("Button")]
    [SerializeField] private Button startButton;

    [Header("Skill Deck Button")]
    [SerializeField] private Button skillOpenButton;
    [SerializeField] private Button skillCloseButton;

    private NightController nightController;

    void Awake()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
        skillOpenButton.onClick.AddListener(OpenSkillDeck);
        skillCloseButton.onClick.AddListener(CloseSkillDeck);
    }

    void Start()
    {
        nightController = GameManager.Instance?.nightController;

        if (nightController == null)
        {
            Debug.LogWarning("[PreparationUI] NightController belum tersedia di GameManager.");
            return;
        }

        // nightController.OnNightEnded += HandleNightEnded;
        nightController.OnAllNightsCompleted += HandleAllNightsCompleted;

        // Saat game dimulai, tampilkan UI persiapan untuk night pertama
        RootUI.SetActive(true);
        HandleNightCounter();
    }

    void OnDestroy()
    {
        if (nightController == null) return;

        // nightController.OnNightEnded -= HandleNightEnded;
        nightController.OnAllNightsCompleted -= HandleAllNightsCompleted;
    }

    private void OnStartButtonClicked()
    {
        if (nightController == null) return;

        CloseSkillDeck();
        RootUI.SetActive(false);
        nightController.StartNight();
    }

    // Night selesai -> munculkan lagi UI persiapan untuk night berikutnya
    // private void HandleNightEnded(int nightIndex)
    // {
    //     RootUI.SetActive(true);
    //     HandleNightCounter();
    // }

    public void ShowPreparationUI()
    {
        RootUI.SetActive(true);
        HandleNightCounter();
    }

    // Night terakhir selesai -> tidak ada night lagi, jadi UI persiapan disembunyikan
    // (ganti dengan logika menang / ending sesuai game Anda)
    private void HandleAllNightsCompleted()
    {
        RootUI.SetActive(false);
    }

    private void HandleNightCounter()
    {
        nightCounter.text = $"Night {nightController.CurrentNightIndex + 1}";
    }

    public void OpenSkillDeck()
    {
        if (skillSelectorPanel.activeSelf) return;
        skillSelectorPanel.SetActive(true);
    }

    public void CloseSkillDeck()
    {
        if (!skillSelectorPanel.activeSelf) return;
        skillSelectorPanel.SetActive(false);
    }
}