using System.Collections.Generic;
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

    private readonly Dictionary<GameObject, bool> siblingActiveStates = new Dictionary<GameObject, bool>();

    private void OnEnable()
    {
        DeactivateSiblings();
        Initialize();
    }

    private void OnDisable()
    {
        if (btnBack != null) btnBack.onClick.RemoveListener(HandleBackButton);
        if (btnSkillTab != null) btnSkillTab.onClick.RemoveListener(ActivateSkillLoadout);
        if (btnItemTab != null) btnItemTab.onClick.RemoveListener(ActivateItemLoadout);

        RestoreSiblings();
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

        RefreshPanelNavigation(gameObject);
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

        RefreshPanelNavigation(gameObject);
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

    private static void RefreshPanelNavigation(GameObject panel)
    {
        if (panel == null) return;

        var navigationControllers = new HashSet<ParentSelectableNavigation>();

        foreach (var navigationController in panel.GetComponentsInChildren<ParentSelectableNavigation>(true))
            navigationControllers.Add(navigationController);

        // Navigation controller can live on the shared parent of both loadout panels.
        for (Transform current = panel.transform; current != null; current = current.parent)
        {
            var navigationController = current.GetComponent<ParentSelectableNavigation>();
            if (navigationController != null)
                navigationControllers.Add(navigationController);
        }

        foreach (var navigationController in navigationControllers)
        {
            if (navigationController != null && navigationController.isActiveAndEnabled)
                navigationController.RefreshNavigation();
        }
    }

    private void DeactivateSiblings()
    {
        siblingActiveStates.Clear();

        Transform parent = transform.parent;
        if (parent == null) return;

        foreach (Transform sibling in parent)
        {
            GameObject siblingObject = sibling.gameObject;
            if (siblingObject == gameObject) continue;

            siblingActiveStates[siblingObject] = siblingObject.activeSelf;
            siblingObject.SetActive(false);
        }
    }

    private void RestoreSiblings()
    {
        foreach (var siblingState in siblingActiveStates)
        {
            if (siblingState.Key != null)
                siblingState.Key.SetActive(siblingState.Value);
        }

        siblingActiveStates.Clear();
    }

    #endregion
}
