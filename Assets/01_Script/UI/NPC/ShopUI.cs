using UnityEngine;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private GameObject _shopMainPanel;

    private void OnEnable()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (_shopMainPanel != null)
        {
            _shopMainPanel.SetActive(true);
        }
    }

    public void CloseUI()
    {
        gameObject.SetActive(false);
    }
}