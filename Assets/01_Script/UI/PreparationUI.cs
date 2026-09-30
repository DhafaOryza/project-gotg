using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreparationUI : MonoBehaviour
{
    [SerializeField] private GameObject RootUI;
    [SerializeField] private Button startButton;
    [SerializeField] private GameObject skillSelectorPanel;
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

    /// <summary>
    /// Mengatur visibilitas Skill Selector Panel dan Tombol Start
    /// </summary>
    public void SetSkillSelectorActive(bool isOpen)
    {
        if (skillSelectorPanel != null)
            skillSelectorPanel.SetActive(isOpen);
        if (startButton != null)
            startButton.gameObject.SetActive(!isOpen);
    }
}