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
    [SerializeField] private ItemCardUI _cardPrefab;
    private const int MaxCards = 8;

    [Header("Currency")]
    [SerializeField] private TextMeshProUGUI _txtCurrency;
    
    [Header("Detail")]
    [SerializeField] private ItemDetailUI _detailUI;

    private CurrencyManager _currencyManager;
    private ItemManager _itemManager;
    private readonly List<ItemCardUI> _cards = new List<ItemCardUI>();
    private readonly List<GameObject> _emptySlots = new List<GameObject>();
    private ItemCardUI _selectedCard;

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
        _itemManager = gameManager != null ? gameManager.itemManager : null;

        if (_currencyManager != null)
            _currencyManager.OnChange += HandleCurrencyChanged;

        if (_itemManager != null)
            _itemManager.OnChosenItemsChanged += RefreshDetail;

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
        List<ItemDataSO> shopItems = sessionData != null && sessionData.items != null
            ? sessionData.items
            : new List<ItemDataSO>();

        int generatedCount = 0;

        if (_cardPrefab != null)
        {
            foreach (var item in shopItems)
            {
                if (item == null) continue;
                if (generatedCount >= MaxCards) break;

                ItemCardUI card = Instantiate(_cardPrefab, _contentContainer);
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

    private void HandleCardSelected(ItemCardUI selected)
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

        ItemDataSO item = _selectedCard.Data;
        bool canAfford = _currencyManager != null && _currencyManager.Money >= item.price;
        bool isAvailable = _itemManager != null && _itemManager.CanAddItem(item);

        _detailUI.Show(item, canAfford, isAvailable);
        
        Debug.Log($"[ShopUI] money={(_currencyManager != null ? _currencyManager.Money : -1)}, price={item.price}, canAfford={canAfford}, isAvailable={isAvailable}, hasManager={_itemManager != null}");
    }

    /// <summary>
    /// Pays for the selected item and puts it in a free slot. The money is refunded if adding fails.
    /// </summary>
    private void HandleBuyClicked()
    {
        if (_selectedCard == null || _selectedCard.Data == null) return;
        if (_currencyManager == null || _itemManager == null) return;

        ItemDataSO item = _selectedCard.Data;

        if (!_itemManager.CanAddItem(item))
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

        if (!_itemManager.TryAddItem(item))
            _currencyManager.AddMoney(price);

        RefreshDetail();
    }

    private void UnsubscribeManagers()
    {
        if (_currencyManager != null)
            _currencyManager.OnChange -= HandleCurrencyChanged;

        if (_itemManager != null)
            _itemManager.OnChosenItemsChanged -= RefreshDetail;
    }

    private void RefreshCurrency()
    {
        if (_txtCurrency == null || _currencyManager == null) return;

        _txtCurrency.text = $"{_currencyManager.Money}";
    }

    #endregion
}
