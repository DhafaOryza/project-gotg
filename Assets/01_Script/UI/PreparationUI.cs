using UnityEngine;
using UnityEngine.UI;

public class PreparationUI : MonoBehaviour
{
    [SerializeField] private GameObject RootUI;
    [SerializeField] private Button startButton;

    void Awake()
    {
        startButton.onClick.AddListener(() =>
        {
            var waveController = GameManager.Instance?.waveController;
            waveController.StartWave();
            ToggleUI();
        });
    }

    void Start()
    {
        ToggleUI();
    }

    private void ToggleUI()
    {
        var waveController = GameManager.Instance?.waveController;
        if (!waveController.IsWaveRunning)
            RootUI.SetActive(true);
        else
            RootUI.SetActive(false);
    }
}