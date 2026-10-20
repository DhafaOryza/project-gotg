using UnityEngine;

public class ShopUI : MonoBehaviour
{
    // Di masa depan, Anda bisa menambahkan variabel khusus untuk Granary di sini
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