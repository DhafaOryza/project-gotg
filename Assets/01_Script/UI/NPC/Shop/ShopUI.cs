using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private GameObject _shopMainPanel;

    [Header("UI Controls")]
    [SerializeField] private Button _exitButton;

    [Header("Shop List")]
    [SerializeField] private Transform _contentContainer;
    [SerializeField] private ConsumableCardUI _cardPrefab;
    private const int MaxCards = 8;

    [Header("Currency")]
    [SerializeField] private TextMeshProUGUI _txtCurrency;
    
    [Header("Detail")]
    [SerializeField] private ConsumableDetailUI _detailUI;

    private CurrencyManager _currencyManager;
    private ConsumableAndItemManager _consumableManager;
    private readonly List<ConsumableCardUI> _cards = new List<ConsumableCardUI>();
    private readonly List<GameObject> _emptySlots = new List<GameObject>();
    private ConsumableCardUI _selectedCard;

    private void Awake()
    {
        if (_exitButton != null)
            _exitButton.onClick.AddListener(CloseUI);

        if (_detailUI != null)
            _detailUI.OnBuyClicked += HandleBuyClicked;
    }

    private void OnDestroy()
    {
        if (_exitButton != null)
            _exitButton.onClick.RemoveListener(CloseUI);

        if (_detailUI != null)
            _detailUI.OnBuyClicked -= HandleBuyClicked;
    }

    private void OnEnable()
    {
        Initialize();
    }

    private void OnDisable()
    {
        UnsubscribeManagers();
        ClearCards();
    }

    public void Initialize()
    {
        // Managers are fetched here, before subscribing, because they can still be null in Awake.
        UnsubscribeManagers();

        GameManager gameManager = GameManager.Instance;
        _currencyManager = gameManager != null ? gameManager.currencyManager : null;
        _consumableManager = gameManager != null ? gameManager.consumableAndItemManager : null;

        if (_currencyManager != null)
            _currencyManager.OnChange += HandleCurrencyChanged;

        if (_consumableManager != null)
            _consumableManager.OnChosenConsumablesChanged += RefreshDetail;

        if (_shopMainPanel != null)
            _shopMainPanel.SetActive(true);

        RefreshCurrency();
        BuildCards();
    }

    private void HandleCurrencyChanged()
    {
        RefreshCurrency();
        RefreshDetail();
    }

    public void CloseUI()
    {
        if (_shopMainPanel != null)
        {
            _shopMainPanel.SetActive(false);
        }

        if (PlayerUI.Instance != null)
        {
            PlayerUI.Instance.CloseAllRolePanels();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    #region Cards

    private void BuildCards()
    {
        // Clear first, otherwise the cards are duplicated every time the shop opens.
        ClearCards();

        if (_contentContainer == null) return;

        GameSessionData sessionData = GameSessionData.GetOrCreate();
        List<ConsumableDataSO> shopItems = sessionData != null && sessionData.consumables != null
            ? sessionData.consumables
            : new List<ConsumableDataSO>();

        int generatedCount = 0;

        if (_cardPrefab != null)
        {
            foreach (var item in shopItems)
            {
                if (item == null) continue;
                if (generatedCount >= MaxCards) break;

                ConsumableCardUI card = Instantiate(_cardPrefab, _contentContainer);
                card.Initialize(item);
                card.OnSelected += HandleCardSelected;
                _cards.Add(card);
                generatedCount++;
            }
        }

        while (generatedCount < MaxCards)
        {
            GameObject emptySlot = new GameObject("None", typeof(RectTransform));
            emptySlot.transform.SetParent(_contentContainer, false);
            _emptySlots.Add(emptySlot);
            generatedCount++;
        }

        if (_cards.Count > 0)
            HandleCardSelected(_cards[0]);
        else
            RefreshDetail();
    }

    private void ClearCards()
    {
        foreach (var card in _cards)
        {
            if (card == null) continue;

            card.OnSelected -= HandleCardSelected;
        }

        // Clear every existing entry in the container, including objects not tracked by this instance.
        if (_contentContainer != null)
        {
            for (int i = _contentContainer.childCount - 1; i >= 0; i--)
                Destroy(_contentContainer.GetChild(i).gameObject);
        }

        _cards.Clear();
        _emptySlots.Clear();
        _selectedCard = null;

        if (_detailUI != null)
            _detailUI.Clear();
    }

    private void HandleCardSelected(ConsumableCardUI selected)
    {
        _selectedCard = selected;

        RefreshDetail();
    }

    #endregion

    #region Detail & Buy

    private void RefreshDetail()
    {
        if (_detailUI == null) return;

        if (_selectedCard == null || _selectedCard.Data == null)
        {
            _detailUI.Clear();
            return;
        }

        ConsumableDataSO item = _selectedCard.Data;
        bool canAfford = _currencyManager != null && _currencyManager.Money >= item.price;
        bool isAvailable = _consumableManager != null && _consumableManager.CanAddConsumable(item);

        _detailUI.Show(item, canAfford, isAvailable);
        
        Debug.Log($"[ShopUI] money={(_currencyManager != null ? _currencyManager.Money : -1)}, price={item.price}, canAfford={canAfford}, isAvailable={isAvailable}, hasManager={_consumableManager != null}");
    }

    /// <summary>
    /// Pays for the selected consumable and puts it in a free slot. The money is refunded if adding fails.
    /// </summary>
    private void HandleBuyClicked()
    {
        if (_selectedCard == null || _selectedCard.Data == null) return;
        if (_currencyManager == null || _consumableManager == null) return;

        ConsumableDataSO item = _selectedCard.Data;

        if (!_consumableManager.CanAddConsumable(item))
        {
            RefreshDetail();
            return;
        }

        int price = item.price;
        if (!_currencyManager.SpendMoney(price))
        {
            RefreshDetail();
            return;
        }

        if (!_consumableManager.TryAddConsumable(item))
            _currencyManager.AddMoney(price);

        RefreshDetail();
    }

    private void UnsubscribeManagers()
    {
        if (_currencyManager != null)
            _currencyManager.OnChange -= HandleCurrencyChanged;

        if (_consumableManager != null)
            _consumableManager.OnChosenConsumablesChanged -= RefreshDetail;
    }

    private void RefreshCurrency()
    {
        if (_txtCurrency == null || _currencyManager == null) return;

        _txtCurrency.text = $"{_currencyManager.Money}";
    }

    #endregion
}
