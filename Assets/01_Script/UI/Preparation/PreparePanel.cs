using UnityEngine;
using UnityEngine.UI;

public class PreparePanel : MonoBehaviour
{
    [Header("References")]
    [Header("Skill Loadout Panel")]
    [SerializeField] private SkillLoadoutPanel skillLoadoutPanel;

    [Header("Item Loadout Panel")]
    [SerializeField] private ItemLoadoutPanel itemLoadoutPanel;

    [Header("Button")]
    [SerializeField] private Button btnBack;
    [SerializeField] private Button btnSkillTab;
    [SerializeField] private Button btnItemTab;


    private void OnEnable()
    {
        Initialize();
    }

    private void OnDisable()
    {
        if (btnBack != null) btnBack.onClick.RemoveListener(HandleBackButton);
        if (btnSkillTab != null) btnSkillTab.onClick.RemoveListener(ActivateSkillLoadout);
        if (btnItemTab != null) btnItemTab.onClick.RemoveListener(ActivateItemLoadout);
    }

    public void Initialize()
    {
        InitializeButton();

        ActivateSkillLoadout();
    }

    public void ActivateSelf()
    {
        if (gameObject.activeSelf)
        {
            Initialize();
            return;
        }

        gameObject.SetActive(true);
    }

    public void DeactivateSelf() => gameObject.SetActive(false);

    public void ActivateSkillLoadout()
    {
        if (itemLoadoutPanel != null)
            itemLoadoutPanel.gameObject.SetActive(false);

        if (skillLoadoutPanel != null)
            skillLoadoutPanel.gameObject.SetActive(true);
    }

    public void DeactivateSkillLoadout()
    {
        if (skillLoadoutPanel != null)
            skillLoadoutPanel.gameObject.SetActive(false);
    }

    public void ActivateItemLoadout()
    {
        if (skillLoadoutPanel != null)
            skillLoadoutPanel.gameObject.SetActive(false);

        if (itemLoadoutPanel != null)
            itemLoadoutPanel.gameObject.SetActive(true);
    }

    public void DeactivateItemLoadout()
    {
        if (itemLoadoutPanel != null)
            itemLoadoutPanel.gameObject.SetActive(false);
    }

    #region Helper

    private void InitializeButton()
    {
        if (btnBack != null)
        {
            btnBack.onClick.RemoveListener(HandleBackButton);
            btnBack.onClick.AddListener(HandleBackButton);
        }

        if (btnSkillTab != null)
        {
            btnSkillTab.onClick.RemoveListener(ActivateSkillLoadout);
            btnSkillTab.onClick.AddListener(ActivateSkillLoadout);
        }

        if (btnItemTab != null)
        {
            btnItemTab.onClick.RemoveListener(ActivateItemLoadout);
            btnItemTab.onClick.AddListener(ActivateItemLoadout);
        }

    }

    private void HandleBackButton()
    {
        DeactivateSelf();
        PlayerUI.Instance?.SelectPreparationFirstSelected();
    }

    #endregion
}
