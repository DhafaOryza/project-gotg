using UnityEngine;

public class GranaryUI : MonoBehaviour
{
    [SerializeField] private GameObject _granaryMainPanel;

    private void OnEnable()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (_granaryMainPanel != null)
        {
            _granaryMainPanel.SetActive(true);
        }
    }

    public void CloseUI()
    {
        gameObject.SetActive(false);
    }
}