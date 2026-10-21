using UnityEngine;

public class SkillTreeUI : MonoBehaviour
{
    [SerializeField] private GameObject _skillTreeMainPanel;

    private void OnEnable()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (_skillTreeMainPanel != null)
        {
            _skillTreeMainPanel.SetActive(true);
        }
    }

    public void CloseUI()
    {
        gameObject.SetActive(false);
    }
}