using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Менеджер магазина бустеров за монеты
/// Управляет покупкой бустеров через внутриигровую валюту
/// </summary>
public class BoosterShopManager : MonoBehaviour
{
    [Header("UI элементы магазина бустеров")]
    [Tooltip("Панель с бустерами")]
    public Transform boostersPanel;
    
    [Tooltip("Кнопка получения бомбочки за рекламу (заменяет покупку за монеты)")]
    public Button getBombButton;
    
    [Tooltip("Кнопка покупки лапки")]
    public Button buyPawButton;
    
    [Tooltip("Кнопка покупки кота-обжорки")]
    public Button buyCatButton;

    [Header("Модальное окно покупки")]
    [Tooltip("Панель модального окна покупки")]
    public GameObject buyModalPanel;
    
    [Tooltip("Кнопка закрытия модального окна")]
    public Button closeModalButton;
    
    [Tooltip("Иконка товара в модальном окне")]
    public Image itemIcon;
    
    [Tooltip("Название товара в модальном окне")]
    public TextMeshProUGUI itemTitle;

    [Header("Элементы управления количеством")]
    [Tooltip("Кнопка уменьшения количества")]
    public Button decreaseQuantityButton;
    
    [Tooltip("Кнопка увеличения количества")]
    public Button increaseQuantityButton;
    
    [Tooltip("Текст с текущим количеством")]
    public TextMeshProUGUI quantityText;
    
    [Tooltip("Текст с общей ценой")]
    public TextMeshProUGUI totalPriceText;
    
    [Tooltip("Кнопка подтверждения покупки")]
    public Button confirmPurchaseButton;

    [Header("UI элементы отображения")]
    [Tooltip("Текст с количеством монет в магазине")]
    public TextMeshProUGUI coinsDisplayText;
    
    [Header("Настройки бустеров")]
    [Tooltip("Конфигурация экономики")]
    public EconomyConfig economyConfig;
    
    
    [Tooltip("Цена лапки (берется из EconomyConfig)")]
    public int pawPrice => economyConfig != null ? economyConfig.pawPrice : 75;
    
    [Tooltip("Цена кота-обжорки (берется из EconomyConfig)")]
    public int catPrice => economyConfig != null ? economyConfig.catPrice : 150;

    [Header("Спрайты бустеров")]
    [Tooltip("Спрайт бомбочки")]
    public Sprite bombSprite;
    
    [Tooltip("Спрайт лапки")]
    public Sprite pawSprite;
    
    [Tooltip("Спрайт кота-обжорки")]
    public Sprite catSprite;

    // Текущее состояние покупки
    private string currentBoosterType = "";
    private int currentQuantity = 0;
    private int currentUnitPrice = 0;

    // Ссылки на менеджеры
    private GameManager gameManager;
    private ShopBillingManager shopBillingManager;

    private void Start()
    {
        InitializeReferences();
        SetupButtonListeners();
        UpdateUI();
    }

    /// <summary>
    /// Инициализирует ссылки на менеджеры
    /// </summary>
    private void InitializeReferences()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        shopBillingManager = ShopBillingManager.Instance;

        if (gameManager == null)
        {
            Debug.LogError("BoosterShopManager: GameManager не найден!");
        }

        if (shopBillingManager == null)
        {
            Debug.LogWarning("BoosterShopManager: ShopBillingManager не найден!");
        }
        
        // Подписываемся на события GameManager для обновления монет
        if (gameManager != null)
        {
            gameManager.OnCoinsChanged += UpdateCoinsDisplay;
        }
        
        // Также подписываемся на события GameDataManager для надежности
        GameDataManager.OnCoinsChanged += UpdateCoinsDisplay;
    }

    /// <summary>
    /// Настраивает обработчики кнопок
    /// </summary>
    private void SetupButtonListeners()
    {
        // Кнопки покупки бустеров
        if (getBombButton != null)
            getBombButton.onClick.AddListener(GetBombForAd);
            
        if (buyPawButton != null)
            buyPawButton.onClick.AddListener(() => OpenBuyModal("paw"));
            
        if (buyCatButton != null)
            buyCatButton.onClick.AddListener(() => OpenBuyModal("cat"));

        // Кнопки модального окна
        if (closeModalButton != null)
            closeModalButton.onClick.AddListener(CloseBuyModal);
            
        if (decreaseQuantityButton != null)
            decreaseQuantityButton.onClick.AddListener(DecreaseQuantity);
            
        if (increaseQuantityButton != null)
            increaseQuantityButton.onClick.AddListener(IncreaseQuantity);
            
        if (confirmPurchaseButton != null)
            confirmPurchaseButton.onClick.AddListener(ConfirmPurchase);
    }

    /// <summary>
    /// Получает бомбочку за просмотр рекламы
    /// </summary>
    private void GetBombForAd()
    {
        if (RewardedAdManager.Instance != null)
        {
            if (RewardedAdManager.Instance.IsAdAvailable())
            {
                Debug.Log("BoosterShopManager: Показываем рекламу");
                RewardedAdManager.Instance.ShowRewardedAd();
            }
            else
            {
                Debug.LogWarning("BoosterShopManager: Реклама недоступна");
                // Можно показать уведомление пользователю
            }
        }
        else
        {
            Debug.LogError("BoosterShopManager: RewardedAdManager не найден!");
        }
    }

    /// <summary>
    /// Открывает модальное окно покупки
    /// </summary>
    private void OpenBuyModal(string boosterType)
    {
        currentBoosterType = boosterType;
        currentQuantity = 0; // По умолчанию количество = 0
        currentUnitPrice = GetBoosterPrice(boosterType);

        // Настраиваем UI модального окна
        SetupModalUI(boosterType);
        
        // Показываем модальное окно
        if (buyModalPanel != null)
            buyModalPanel.SetActive(true);

        // Обновляем UI
        UpdateQuantityUI();
    }

    /// <summary>
    /// Закрывает модальное окно покупки
    /// </summary>
    private void CloseBuyModal()
    {
        if (buyModalPanel != null)
            buyModalPanel.SetActive(false);
            
        // Сбрасываем состояние
        currentBoosterType = "";
        currentQuantity = 0;
        currentUnitPrice = 0;
    }

    /// <summary>
    /// Уменьшает количество покупаемых бустеров
    /// </summary>
    private void DecreaseQuantity()
    {
        if (currentQuantity > 0)
        {
            currentQuantity--;
            UpdateQuantityUI();
        }
    }

    /// <summary>
    /// Увеличивает количество покупаемых бустеров
    /// </summary>
    private void IncreaseQuantity()
    {
        // Можно ограничить максимальное количество, если нужно
        // Например, максимум 99 штук
        if (currentQuantity < 99)
        {
            currentQuantity++;
            UpdateQuantityUI();
        }
    }

    /// <summary>
    /// Обновляет UI элементов количества и цены
    /// </summary>
    private void UpdateQuantityUI()
    {
        // Обновляем текст количества
        if (quantityText != null)
        {
            quantityText.text = currentQuantity.ToString();
        }

        // Вычисляем общую цену
        int totalPrice = currentQuantity * currentUnitPrice;
        
        // Обновляем текст общей цены
        if (totalPriceText != null)
        {
            totalPriceText.text = totalPrice.ToString();
        }

        // Проверяем, достаточно ли монет у игрока
        bool canAfford = CanAffordPurchase(totalPrice);
        
        // Активируем/деактивируем кнопку покупки
        if (confirmPurchaseButton != null)
        {
            confirmPurchaseButton.interactable = canAfford && currentQuantity > 0;
        }

        // Обновляем кнопки увеличения/уменьшения
        if (decreaseQuantityButton != null)
        {
            decreaseQuantityButton.interactable = currentQuantity > 0;
        }

        if (increaseQuantityButton != null)
        {
            increaseQuantityButton.interactable = currentQuantity < 99; // Можно изменить лимит
        }
    }

    /// <summary>
    /// Подтверждает покупку бустеров
    /// </summary>
    private void ConfirmPurchase()
    {
        if (currentQuantity <= 0)
        {
            Debug.LogWarning("BoosterShopManager: Попытка купить 0 бустеров");
            return;
        }

        int totalPrice = currentQuantity * currentUnitPrice;
        
        if (!CanAffordPurchase(totalPrice))
        {
            Debug.LogWarning("BoosterShopManager: Недостаточно монет для покупки");
            return;
        }

        // Списываем монеты через GameDataManager
        bool success = GameDataManager.SpendCoins(totalPrice);
        if (!success)
        {
            Debug.LogWarning("BoosterShopManager: Не удалось списать монеты!");
            return;
        }

        // Выдаем бустеры
        GiveBoosters(currentBoosterType, currentQuantity);

        Debug.Log($"BoosterShopManager: Куплено {currentQuantity} бустеров типа {currentBoosterType} за {totalPrice} монет");

        // Закрываем модальное окно
        CloseBuyModal();
        
        // Обновляем UI
        UpdateUI();
    }

    /// <summary>
    /// Проверяет, может ли игрок позволить себе покупку
    /// </summary>
    private bool CanAffordPurchase(int totalPrice)
    {
        return GameDataManager.CanAffordCoins(totalPrice);
    }

    /// <summary>
    /// Выдает бустеры игроку
    /// </summary>
    private void GiveBoosters(string boosterType, int quantity)
    {
        // Используем GameDataManager для добавления бустеров
        GameDataManager.AddBoosters(boosterType, quantity);
        
        // Обновляем UI через ShopBillingManager
        if (shopBillingManager != null)
        {
            shopBillingManager.InitializeBoosterUI();
        }
        
        // Также обновляем BoosterManager напрямую
        BoosterManager boosterManager = FindFirstObjectByType<BoosterManager>();
        if (boosterManager != null)
        {
            boosterManager.UpdateBoosterUI();
            Debug.Log($"BoosterShopManager: Обновлен UI через BoosterManager для {boosterType}");
        }
        else
        {
            Debug.LogWarning("BoosterShopManager: BoosterManager не найден!");
        }
    }


    /// <summary>
    /// Настраивает UI модального окна
    /// </summary>
    private void SetupModalUI(string boosterType)
    {
        // Настраиваем иконку
        if (itemIcon != null)
        {
            Sprite sprite = GetBoosterSprite(boosterType);
            if (sprite != null)
                itemIcon.sprite = sprite;
        }

        // Настраиваем название
        if (itemTitle != null)
        {
            itemTitle.text = GetBoosterName(boosterType);
        }
    }

    /// <summary>
    /// Получает цену бустера
    /// </summary>
    private int GetBoosterPrice(string boosterType)
    {
        switch (boosterType)
        {
            case "bomb": 
                Debug.LogWarning("BoosterShopManager: Попытка получить цену бомбочки - теперь она получается за рекламу!");
                return 0; // Бомбочка больше не продается за монеты
            case "paw": return pawPrice;
            case "cat": return catPrice;
            default: return 0;
        }
    }

    /// <summary>
    /// Получает спрайт бустера
    /// </summary>
    private Sprite GetBoosterSprite(string boosterType)
    {
        switch (boosterType)
        {
            case "bomb": return bombSprite;
            case "paw": return pawSprite;
            case "cat": return catSprite;
            default: return null;
        }
    }

    /// <summary>
    /// Получает название бустера
    /// </summary>
    private string GetBoosterName(string boosterType)
    {
        switch (boosterType)
        {
            case "bomb": return "БОМБОЧКА";
            case "paw": return "ЛАПКА";
            case "cat": return "КОТ-ОБЖОРКА";
            default: return "Неизвестно";
        }
    }

    /// <summary>
    /// Получает описание бустера
    /// </summary>
    private string GetBoosterDescription(string boosterType)
    {
        switch (boosterType)
        {
            case "bomb": return "Уничтожает три любых блюда, которые оказываются в зоне взрыва.";
            case "paw": return "Уничтожает все блюда одного типа.";
            case "cat": return "Уничтожает все что можно съесть!";
            default: return "Описание недоступно";
        }
    }

    /// <summary>
    /// Обновляет весь UI
    /// </summary>
    private void UpdateUI()
    {
        // Здесь можно добавить обновление других элементов UI
        // Например, обновление отображения количества бустеров
        if (shopBillingManager != null)
        {
            shopBillingManager.InitializeBoosterUI();
        }
        
        // Обновляем отображение монет
        if (gameManager != null)
        {
            UpdateCoinsDisplay(gameManager.currentCoins);
        }
    }
    
    /// <summary>
    /// Обновляет отображение монет в магазине
    /// </summary>
    private void UpdateCoinsDisplay(int coins)
    {
        if (coinsDisplayText != null)
        {
            coinsDisplayText.text = coins.ToString();
            Debug.Log($"BoosterShopManager: Обновлено отображение монет в магазине: {coins}");
        }
        else
        {
            Debug.LogWarning("BoosterShopManager: coinsDisplayText не назначен!");
        }
    }
    
    /// <summary>
    /// Обновляет UI при открытии магазина
    /// </summary>
    public void RefreshShopUI()
    {
        Debug.Log($"BoosterShopManager: RefreshShopUI вызван");
        
        // Обновляем отображение монет
        if (gameManager != null)
        {
            int currentCoins = gameManager.currentCoins;
            Debug.Log($"BoosterShopManager: Текущие монеты из GameManager: {currentCoins}");
            UpdateCoinsDisplay(currentCoins);
        }
        else
        {
            Debug.LogWarning("BoosterShopManager: gameManager не назначен!");
        }
        
        // Обновляем UI бустеров
        if (shopBillingManager != null)
        {
            shopBillingManager.InitializeBoosterUI();
        }
    }

    private void OnDestroy()
    {
        // Отписываемся от событий GameManager
        if (gameManager != null)
        {
            gameManager.OnCoinsChanged -= UpdateCoinsDisplay;
        }
        
        // Отписываемся от событий GameDataManager
        GameDataManager.OnCoinsChanged -= UpdateCoinsDisplay;
        
        // Отписываемся от событий кнопок при уничтожении
        if (getBombButton != null)
            getBombButton.onClick.RemoveAllListeners();
        if (buyPawButton != null)
            buyPawButton.onClick.RemoveAllListeners();
        if (buyCatButton != null)
            buyCatButton.onClick.RemoveAllListeners();
        if (closeModalButton != null)
            closeModalButton.onClick.RemoveAllListeners();
        if (decreaseQuantityButton != null)
            decreaseQuantityButton.onClick.RemoveAllListeners();
        if (increaseQuantityButton != null)
            increaseQuantityButton.onClick.RemoveAllListeners();
        if (confirmPurchaseButton != null)
            confirmPurchaseButton.onClick.RemoveAllListeners();
    }
}
