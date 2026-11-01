using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpDetail : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Button _levelUpButton;
	[SerializeField] private TextMeshProUGUI _levelUpCostText;

	[Header("Current Level Preview")]
	[SerializeField] private TextMeshProUGUI _currentLevelText;
	[SerializeField] private TextMeshProUGUI _currentHealthText;
	[SerializeField] private SliderBar _currentHealthBar;
	[SerializeField] private TextMeshProUGUI _currentCooldownText;

	[Header("Next Level Preview")]
	[SerializeField] private TextMeshProUGUI _nextLevelText;
	[SerializeField] private TextMeshProUGUI _nextHealthText;
	[SerializeField] private SliderBar _nextHealthBar;
	[SerializeField] private TextMeshProUGUI _nextCooldownText;

	private PlayerBaseEntity player;
	private CurrencyManager currencyManager;

	private void Awake()
	{
		if (_levelUpButton != null)
			_levelUpButton.onClick.AddListener(HandleLevelUpButton);
	}

	private void OnEnable()
	{
		currencyManager = GameManager.Instance?.currencyManager;
		if (currencyManager != null)
			currencyManager.OnChange += Refresh;

		Refresh();
	}

	private void OnDisable()
	{
		if (currencyManager != null)
			currencyManager.OnChange -= Refresh;
	}

	private void OnDestroy()
	{
		if (_levelUpButton != null)
			_levelUpButton.onClick.RemoveListener(HandleLevelUpButton);
	}

	public void Initialize()
	{
		player = GameManager.Instance?.levelSpawnerManager?.GetCurrentPlayer();
		currencyManager = GameManager.Instance?.currencyManager;
		Refresh();
	}

	private void HandleLevelUpButton()
	{
		if (player == null || currencyManager == null || !player.CanLevelUpCharacter)
		{
			Refresh();
			return;
		}

		int price = player.GetCharacterLevelUpPrice();
		if (!currencyManager.SpendMoney(price))
		{
			Refresh();
			return;
		}

		if (!player.LevelUpCharacter())
			currencyManager.AddMoney(price);

		Refresh();
	}

	private void Refresh()
	{
		if (player == null)
			player = GameManager.Instance?.levelSpawnerManager?.GetCurrentPlayer();

		bool hasPlayer = player != null;
		bool canLevelUp = hasPlayer && currencyManager != null && player.CanLevelUpCharacter;
		int currentLevel = hasPlayer ? player.CharacterLevel : 1;
		int nextLevel = hasPlayer ? Mathf.Min(currentLevel + 1, PlayerDataSO.CharacterMaxLevel) : 1;
		int currentMaxHealth = hasPlayer ? player.GetCharacterMaxHealthAtLevel(currentLevel) : 0;
		int nextMaxHealth = hasPlayer ? player.GetCharacterMaxHealthAtLevel(nextLevel) : 0;
		int currentHealth = hasPlayer ? Mathf.Min(player.CurrentHealth, currentMaxHealth) : 0;
		int nextHealth = hasPlayer
			? Mathf.Min(currentHealth + Mathf.Max(0, nextMaxHealth - currentMaxHealth), nextMaxHealth)
			: 0;

		if (_currentLevelText != null) _currentLevelText.text = $"{currentLevel}";
		if (_nextLevelText != null)
			_nextLevelText.text = player != null && player.CanLevelUpCharacter ? $"{nextLevel}" : "MAX";

		if (_currentHealthText != null) _currentHealthText.text = $"{currentHealth}/{currentMaxHealth}";
		if (_nextHealthText != null) _nextHealthText.text = $"{nextHealth}/{nextMaxHealth}";
		_currentHealthBar?.SetHP(currentHealth, currentMaxHealth);
		_nextHealthBar?.SetHP(nextHealth, nextMaxHealth);

		if (_currentCooldownText != null)
		{
			float multiplier = hasPlayer ? player.GetCharacterCooldownMultiplierAtLevel(currentLevel) : 1f;
			_currentCooldownText.text = $"Cooldown x{multiplier:0.##}";
		}

		if (_nextCooldownText != null)
		{
			float multiplier = hasPlayer ? player.GetCharacterCooldownMultiplierAtLevel(nextLevel) : 1f;
			_nextCooldownText.text = player != null && player.CanLevelUpCharacter
				? $"Cooldown x{multiplier:0.##}"
				: "MAX";
		}

		if (_levelUpCostText != null)
			_levelUpCostText.text = hasPlayer && player.CanLevelUpCharacter
				? player.GetCharacterLevelUpPrice().ToString()
				: "MAX";

		if (_levelUpButton != null)
			_levelUpButton.interactable = canLevelUp && currencyManager.Money >= player.GetCharacterLevelUpPrice();
	}
}