using System;
using UnityEngine;

public class GranaryUI : MonoBehaviour
{
    [SerializeField] private GameObject _granaryMainPanel;

    private void OnEnable()
    {
        Initialize();
    }

    /// <summary>
    /// Initializes the granary UI components and data.
    /// </summary>
    public void Initialize()
    {
        if (_granaryMainPanel != null)
        {
            _granaryMainPanel.SetActive(true);
        }
        
        Debug.Log("[GranaryUI] Initialized successfully.");
    }

    public void CloseUI()
    {
        gameObject.SetActive(false);
    }
}