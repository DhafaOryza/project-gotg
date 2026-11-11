using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Instance { get; set; }

    [Header("References")]
    [SerializeField] private HUD _playerHUD;
    [SerializeField] private PreparationUI preparationUI;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (_playerHUD == null) _playerHUD = FindFirstObjectByType<HUD>();
        if (preparationUI == null) preparationUI = FindFirstObjectByType<PreparationUI>();

        if (_playerHUD != null) _playerHUD.Initialize();
        
        DeactivateAllPanel();
    }

    public void ActivatePreparationUI() => preparationUI.ActivateSelf();
    public void DeactivatePreparationUI() => preparationUI.DeactivateSelf();

    #region Helper

    private void DeactivateAllPanel()
    {
        DeactivatePreparationUI();
    }

    #endregion
}
