using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private GameObject _healthMainPanel;

    private void OnEnable()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (_healthMainPanel != null)
        {
            _healthMainPanel.SetActive(true);
        }
    }

    public void CloseUI()
    {
        gameObject.SetActive(false);
    }
}