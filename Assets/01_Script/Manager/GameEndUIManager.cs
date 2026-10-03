using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameEndUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject winPanel;

    [Header("References")]
    [SerializeField] private WaveController waveController;
    [SerializeField] private GranaryBase granary;

    [Header("Scene Config")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void OnEnable()
    {
        if (granary != null)
            granary.OnGranaryDestroyed.AddListener(ShowLosePanel);

        if (waveController != null)
            waveController.OnNightCompleted += HandleNightCompleted;
    }
    private void OnDisable()
    {
        if (granary != null)
            granary.OnGranaryDestroyed.RemoveListener(ShowLosePanel);

        if (waveController != null)
            waveController.OnNightCompleted -= HandleNightCompleted;
    }

    private void Start()
    {
        if (losePanel != null) losePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        // GranaryBase granaryBase = FindAnyObjectByType<GranaryBase>();
        // if (granary != null)
        // {
        //     granary.OnGranaryDestroyed.AddListener(ShowLosePanel);
        // }

        if (waveController == null) waveController = FindAnyObjectByType<WaveController>();
        if (granary == null) granary = FindAnyObjectByType<GranaryBase>();
    }

    private void HandleNightCompleted()
    {
        StartCoroutine(CheckRemainingEnemiesRoutine());
    }

    private IEnumerator CheckRemainingEnemiesRoutine()
    {
        while (FindObjectsByType<EnemyBaseEntity>(FindObjectsSortMode.None).Length > 0)
        {
            yield return new WaitForSeconds(0.5f);
        }
        ShowWinPanel();
    }

    public void ShowWinPanel() 
    {
        if (winPanel != null)
            winPanel.SetActive(true);
    }

    public void ShowLosePanel()
    {
        if (waveController != null)
            waveController.StopWave();

        if (losePanel != null)
            losePanel.SetActive(true);
    }

    public void BackToMainMenu() => SceneManager.LoadScene(mainMenuSceneName);
    public void ContinueToNextNight()
    {
        if (winPanel != null)
            winPanel.SetActive(false);

        var NightController = GameManager.Instance.nightController;
        if (NightController != null)
        {
            NightController.ProceedToNextNight();
        }
        else
        {
            Debug.LogError("[GameEndUIManager] NightController tidak ditemukan di GameManager!");
        }

    }
}