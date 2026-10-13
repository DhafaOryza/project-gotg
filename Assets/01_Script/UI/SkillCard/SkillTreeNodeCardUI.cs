using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillTreeUI : MonoBehaviour
{
   [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI priceOrStatusText;
    [SerializeField] private GameObject lockPanel;
    [SerializeField] private Button actionButton; 

    private SkillDataSO _skillData;
    private SkillTreeManagerUI _managerUI;

    public void Setup(SkillDataSO data, SkillTreeManagerUI parentManager)
    {
        _skillData = data;
        _managerUI = parentManager;

        if (iconImage != null)
            iconImage.sprite = data != null ? data.icon : null;

        UpdateCardState();

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnCardClicked);
        }
    }

    /// <summary>
    /// Memperbarui tampilan visual berdasarkan status kepemilikan dan syarat tier.
    /// </summary>
    private void UpdateCardState()
    {
        if (_skillData == null || GameSessionData.Instance == null) return;

        bool isOwned = GameSessionData.Instance.isSkillUnlocked(_skillData.skillId);
        bool isTierUnlocked = CheckTierRequirement(_skillData.tier);

        if (isOwned)
        {
            if (priceOrStatusText != null) priceOrStatusText.text = "OWNED";
            if (lockPanel != null) lockPanel.SetActive(false);
            if (actionButton != null) actionButton.interactable = false;
        }
        else if (!isTierUnlocked)
        {
            if (priceOrStatusText != null) priceOrStatusText.text = "LOCKED";
            if (lockPanel != null) lockPanel.SetActive(true);
            if (actionButton != null) actionButton.interactable = false;
        }
        else
        {
            int price = GetPriceForTier(_skillData.tier);
            if (priceOrStatusText != null) priceOrStatusText.text = $"{price} Coins";
            if (lockPanel != null) lockPanel.SetActive(false);
            if (actionButton != null) actionButton.interactable = true;
        }
    }

    private void OnCardClicked()
    {
        if (_skillData == null || GameSessionData.Instance == null) return;

        int price = GetPriceForTier(_skillData.tier);
        int currentCoins = GameSessionData.Instance.GetPlayerCoins();

        if (currentCoins >= price)
        {
            GameSessionData.Instance.SpendCoins(price);
            GameSessionData.Instance.UnlockSkill(_skillData.skillId);
            // Debug.Log($"[SkillTree] Berhasil membeli skill: {skillData.displayName}");
            // if (_managerUI != null)
            //     _managerUI.RefreshSkillTree();
        }
        else
            Debug.LogWarning("[SkillTree] Koin tidak mencukupi untuk membeli skill ini.");
    }
    private int GetPriceForTier(int tier) => 100 + (tier - 1) * 25;

    /// <summary>
    /// Validasi syarat jumlah unlock di tier sebelumnya (Segitiga Kiri).
    /// </summary>
    private bool CheckTierRequirement(int tier)
    {
        if (tier == 1) return true;
        int unlockedPreviousTierCount = GameSessionData.Instance.GetUnlockedCountForTier(tier - 1);

        int requiredCount = 0;
        switch (tier)
        {
            case 2: requiredCount = 5; break;
            case 3: requiredCount = 8; break;
            case 4: requiredCount = 10; break;
        }

        return unlockedPreviousTierCount >= requiredCount;
    }
}
